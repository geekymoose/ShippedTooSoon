#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEditor.CustomEditor(typeof(AkEnvironmentPortal))]
public class AkEnvironmentPortalInspector : UnityEditor.Editor
{
	private AkEnvironmentPortal m_envPortal;
	private readonly int[] m_selectedIndex = new int[2];

	[UnityEditor.MenuItem("GameObject/Wwise/Environment Portal", false, 1)]
	public static void CreatePortal()
	{
		var portal = new UnityEngine.GameObject("EnvironmentPortal");

		UnityEditor.Undo.AddComponent<AkEnvironmentPortal>(portal);
		portal.GetComponent<UnityEngine.Collider>().isTrigger = true;

		UnityEditor.Selection.objects = new UnityEngine.Object[] { portal };
	}

	private void OnEnable()
	{
		m_envPortal = target as AkEnvironmentPortal;
		FindOverlappingEnvironments();
		for (var i = 0; i < 2; i++)
		{
			var index = m_envPortal.envList[i].list.IndexOf(m_envPortal.environments[i]);
			m_selectedIndex[i] = index == -1 ? 0 : index;
		}
	}

	public override void OnInspectorGUI()
	{
		UnityEngine.GUILayout.BeginVertical("Box");
		{
			for (var i = 0; i < 2; i++)
			{
				var labels = new string[m_envPortal.envList[i].list.Count];

				for (var j = 0; j < labels.Length; j++)
				{
					if (m_envPortal.envList[i].list[j] != null)
						labels[j] = j + 1 + ". " + GetEnvironmentName(m_envPortal.envList[i].list[j]) + " (" +
						            m_envPortal.envList[i].list[j].name + ")";
					else
						m_envPortal.envList[i].list.RemoveAt(j);
				}

				m_selectedIndex[i] = UnityEditor.EditorGUILayout.Popup("Environment #" + (i + 1), m_selectedIndex[i], labels);

				m_envPortal.environments[i] = m_selectedIndex[i] < 0 || m_selectedIndex[i] >= m_envPortal.envList[i].list.Count
					? null
					: m_envPortal.envList[i].list[m_selectedIndex[i]];
			}
		}
		UnityEngine.GUILayout.EndVertical();

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		UnityEngine.GUILayout.BeginVertical("Box");
		{
			string[] axisLabels = { "X", "Y", "Z" };

			var index = 0;
			for (var i = 0; i < 3; i++)
			{
				if (m_envPortal.axis[i] == 1)
				{
					index = i;
					break;
				}
			}

			index = UnityEditor.EditorGUILayout.Popup("Axis", index, axisLabels);

			if (m_envPortal.axis[index] != 1)
			{
				m_envPortal.axis.Set(0, 0, 0);
				m_envPortal.envList = new[] { new AkEnvironmentPortal.EnvListWrapper(), new AkEnvironmentPortal.EnvListWrapper() };
				m_envPortal.axis[index] = 1;

				//We move and replace the game object to trigger the OnTriggerStay function
				FindOverlappingEnvironments();
			}
		}
		UnityEngine.GUILayout.EndVertical();

		AkGameObjectInspector.RigidbodyCheck(m_envPortal.gameObject);
	}

	private string GetEnvironmentName(AkEnvironment in_env)
	{
		for (var i = 0; i < AkWwiseProjectInfo.GetData().AuxBusWwu.Count; i++)
		{
			for (var j = 0; j < AkWwiseProjectInfo.GetData().AuxBusWwu[i].List.Count; j++)
				if (in_env.GetAuxBusID() == (uint) AkWwiseProjectInfo.GetData().AuxBusWwu[i].List[j].ID)
					return AkWwiseProjectInfo.GetData().AuxBusWwu[i].List[j].Name;
		}

		return string.Empty;
	}

	public void FindOverlappingEnvironments()
	{
		var myCollider = m_envPortal.gameObject.GetComponent<UnityEngine.Collider>();
		if (myCollider == null)
			return;

		var environments = FindObjectsOfType<AkEnvironment>();
		foreach (var environment in environments)
		{
			var otherCollider = environment.gameObject.GetComponent<UnityEngine.Collider>();
			if (otherCollider == null)
				continue;

			if (myCollider.bounds.Intersects(otherCollider.bounds))
			{
				//if index == 0 => the environment is on the negative side of the portal(opposite to the direction of the chosen axis)
				//if index == 1 => the environment is on the positive side of the portal(same direction as the chosen axis) 
				var index = UnityEngine.Vector3.Dot(m_envPortal.transform.rotation * m_envPortal.axis,
					            environment.transform.position - m_envPortal.transform.position) >= 0
					? 1
					: 0;
				if (!m_envPortal.envList[index].list.Contains(environment))
				{
					m_envPortal.envList[index].list.Add(environment);
					m_envPortal.envList[++index % 2].list.Remove(environment);
				}
			}
			else
			{
				for (var i = 0; i < 2; i++)
					m_envPortal.envList[i].list.Remove(environment);
			}
		}
	}
}
#endif