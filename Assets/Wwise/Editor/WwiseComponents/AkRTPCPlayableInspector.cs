#if UNITY_EDITOR

#if UNITY_2017_1_OR_NEWER

//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2017 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////
[UnityEditor.CustomEditor(typeof(AkRTPCPlayable))]
public class AkRTPCPlayableInspector : UnityEditor.Editor
{
	private UnityEditor.SerializedProperty Behaviour;
	private UnityEditor.SerializedProperty overrideTrackObject;
	private AkRTPCPlayable playable;
	private UnityEditor.SerializedProperty RTPCObject;

	private UnityEditor.SerializedProperty setRTPCGlobally;

	public void OnEnable()
	{
		playable = target as AkRTPCPlayable;

		setRTPCGlobally = serializedObject.FindProperty("setRTPCGlobally");
		overrideTrackObject = serializedObject.FindProperty("overrideTrackObject");
		RTPCObject = serializedObject.FindProperty("RTPCObject");
		Behaviour = serializedObject.FindProperty("template");

		if (playable != null && playable.OwningClip != null)
		{
			var componentName = GetRTPCName(new System.Guid(playable.Parameter.valueGuid));
			playable.OwningClip.displayName = componentName;
		}
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		UnityEngine.GUILayout.Space(2);

		UnityEngine.GUILayout.BeginVertical("Box");
		{
			if (setRTPCGlobally != null)
			{
				UnityEditor.EditorGUILayout.PropertyField(setRTPCGlobally, new UnityEngine.GUIContent("Set RTPC Globally: "));
				if (!setRTPCGlobally.boolValue)
				{
					if (overrideTrackObject != null)
					{
						UnityEditor.EditorGUILayout.PropertyField(overrideTrackObject,
							new UnityEngine.GUIContent("Override Track Object: "));
						if (overrideTrackObject.boolValue)
						{
							if (RTPCObject != null)
								UnityEditor.EditorGUILayout.PropertyField(RTPCObject, new UnityEngine.GUIContent("RTPC Object: "));
						}
					}
				}
			}
		}

		UnityEngine.GUILayout.EndVertical();
		if (Behaviour != null)
			UnityEditor.EditorGUILayout.PropertyField(Behaviour, new UnityEngine.GUIContent("Animated Value: "), true);
		if (playable != null && playable.OwningClip != null)
		{
			var componentName = GetRTPCName(new System.Guid(playable.Parameter.valueGuid));
			playable.OwningClip.displayName = componentName;
		}

		serializedObject.ApplyModifiedProperties();
	}

	public string GetRTPCName(System.Guid in_guid)
	{
		var list = AkWwiseProjectInfo.GetData().RtpcWwu;

		for (var i = 0; i < list.Count; i++)
		{
			var element = list[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid));
			if (element != null)
				return element.Name;
		}

		return string.Empty;
	}
}

#endif //UNITY_2017_1_OR_NEWER

#endif // #if UNITY_EDITOR