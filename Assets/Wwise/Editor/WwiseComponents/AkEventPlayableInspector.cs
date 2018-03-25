#if UNITY_EDITOR

#if UNITY_2017_1_OR_NEWER

//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2017 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////
[UnityEditor.CustomEditor(typeof(AkEventPlayable))]
public class AkEventPlayableInspector : UnityEditor.Editor
{
	private UnityEditor.SerializedProperty akEvent;
	private UnityEditor.SerializedProperty emitterObjectRef;
	private AkEventPlayable m_AkEventPlayable;
	private UnityEditor.SerializedProperty[] m_guidProperty;
	private UnityEditor.SerializedProperty[] m_IDProperty;

	private UnityEngine.Rect m_pickerPos;
	private UnityEditor.SerializedProperty overrideTrackEmitterObject;
	private UnityEditor.SerializedProperty retriggerEvent;

	public void OnEnable()
	{
		m_AkEventPlayable = target as AkEventPlayable;
		akEvent = serializedObject.FindProperty("akEvent");
		overrideTrackEmitterObject = serializedObject.FindProperty("overrideTrackEmitterObject");
		emitterObjectRef = serializedObject.FindProperty("emitterObjectRef");
		retriggerEvent = serializedObject.FindProperty("retriggerEvent");

		m_IDProperty = new UnityEditor.SerializedProperty[1];
		m_IDProperty[0] = akEvent.FindPropertyRelative("ID");
		m_guidProperty = new UnityEditor.SerializedProperty[1];
		m_guidProperty[0] = akEvent.FindPropertyRelative("valueGuid.Array");

		if (m_IDProperty[0].intValue == AkSoundEngine.AK_INVALID_UNIQUE_ID)
			UnityEditor.EditorApplication.delayCall += DelayCreateCall;
	}

	public override void OnInspectorGUI()
	{
		if (m_AkEventPlayable != null && m_AkEventPlayable.OwningClip != null)
			m_AkEventPlayable.OwningClip.displayName = name;
		serializedObject.Update();

		UnityEngine.GUILayout.Space(2);

		UnityEngine.GUILayout.BeginVertical("Box");
		{
			UnityEditor.EditorGUILayout.PropertyField(overrideTrackEmitterObject,
				new UnityEngine.GUIContent("Override Track Object: "));
			if (overrideTrackEmitterObject.boolValue)
				UnityEditor.EditorGUILayout.PropertyField(emitterObjectRef, new UnityEngine.GUIContent("Emitter Object Ref: "));
			UnityEditor.EditorGUILayout.PropertyField(retriggerEvent, new UnityEngine.GUIContent("Retrigger Event: "));
			UnityEditor.EditorGUILayout.PropertyField(akEvent, new UnityEngine.GUIContent("Event: "));
		}

		if (m_AkEventPlayable != null && m_AkEventPlayable.OwningClip != null)
		{
			var componentName = GetEventName(new System.Guid(m_AkEventPlayable.akEvent.valueGuid));
			m_AkEventPlayable.OwningClip.displayName = componentName;
		}

		UnityEngine.GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();

		var currentEvent = UnityEngine.Event.current;
		if (currentEvent.type == UnityEngine.EventType.Repaint)
			m_pickerPos = AkUtilities.GetLastRectAbsolute(false);
	}

	public string GetEventName(System.Guid in_guid)
	{
		var list = AkWwiseProjectInfo.GetData().EventWwu;

		for (var i = 0; i < list.Count; i++)
		{
			var element = list[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid));
			if (element != null)
				return element.Name;
		}

		return string.Empty;
	}

	protected void DelayCreateCall()
	{
		AkWwiseComponentPicker.Create(AkWwiseProjectData.WwiseObjectType.EVENT, m_guidProperty, m_IDProperty,
			akEvent.serializedObject, m_pickerPos);
	}
}

#endif //UNITY_2017_1_OR_NEWER

#endif //UNITY_EDITOR