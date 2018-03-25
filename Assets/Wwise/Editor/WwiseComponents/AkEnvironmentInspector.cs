#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEditor.CanEditMultipleObjects]
[UnityEditor.CustomEditor(typeof(AkEnvironment))]
public class AkEnvironmentInspector : AkBaseInspector
{
	private AkEnvironment m_AkEnvironment;

	private UnityEditor.SerializedProperty m_auxBusId;
	private UnityEditor.SerializedProperty m_excludeOthers;
	private UnityEditor.SerializedProperty m_isDefault;
	private UnityEditor.SerializedProperty m_priority;

	private void OnEnable()
	{
		m_AkEnvironment = target as AkEnvironment;

		m_auxBusId = serializedObject.FindProperty("m_auxBusID");
		m_priority = serializedObject.FindProperty("priority");
		m_isDefault = serializedObject.FindProperty("isDefault");
		m_excludeOthers = serializedObject.FindProperty("excludeOthers");

		m_guidProperty = new[] { serializedObject.FindProperty("valueGuid.Array") };

		//Needed by the base class to know which type of component its working with
		m_typeName = "AuxBus";
		m_objectType = AkWwiseProjectData.WwiseObjectType.AUXBUS;

		//We move and replace the game object to trigger the OnTriggerStay function
		ShakeEnvironment();
	}

	public override void OnChildInspectorGUI()
	{
		serializedObject.Update();

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		UnityEditor.EditorGUILayout.BeginVertical("Box");
		{
			m_priority.intValue = UnityEditor.EditorGUILayout.IntField("Priority: ", m_priority.intValue);

			UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

			m_isDefault.boolValue = UnityEditor.EditorGUILayout.Toggle("Default: ", m_isDefault.boolValue);
			if (m_isDefault.boolValue)
				m_excludeOthers.boolValue = false;

			UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

			m_excludeOthers.boolValue = UnityEditor.EditorGUILayout.Toggle("Exclude Others: ", m_excludeOthers.boolValue);
			if (m_excludeOthers.boolValue)
				m_isDefault.boolValue = false;
		}
		UnityEngine.GUILayout.EndVertical();

		AkGameObjectInspector.RigidbodyCheck(m_AkEnvironment.gameObject);

		serializedObject.ApplyModifiedProperties();
	}

	public override string UpdateIds(System.Guid[] in_guid)
	{
		for (var i = 0; i < AkWwiseProjectInfo.GetData().AuxBusWwu.Count; i++)
		{
			var akInfo = AkWwiseProjectInfo.GetData().AuxBusWwu[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[0]));

			if (akInfo != null)
			{
				serializedObject.Update();
				m_auxBusId.intValue = akInfo.ID;
				serializedObject.ApplyModifiedProperties();

				return akInfo.Name;
			}
		}

		return string.Empty;
	}

	public void ShakeEnvironment()
	{
		var temp = m_AkEnvironment.transform.position;
		temp.x *= 1.0000001f;
		m_AkEnvironment.transform.position = temp;

		UnityEditor.EditorApplication.update += ReplaceEnvironment;
	}

	private void ReplaceEnvironment()
	{
		UnityEditor.EditorApplication.update -= ReplaceEnvironment;
		if (m_AkEnvironment && m_AkEnvironment.transform)
		{
			var temp = m_AkEnvironment.transform.position;
			temp.x /= 1.0000001f;
			m_AkEnvironment.transform.position = temp;
		}
	}
}
#endif