#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEditor.CanEditMultipleObjects]
[UnityEditor.CustomEditor(typeof(AkEvent))]
public class AkEventInspector : AkBaseInspector
{
	private UnityEditor.SerializedProperty actionOnEventType;
	private UnityEditor.SerializedProperty callbackData;
	private UnityEditor.SerializedProperty curveInterpolation;

	private UnityEngine.GameObject emitterObject;
	private UnityEditor.SerializedProperty enableActionOnEvent;

	private UnityEditor.SerializedProperty eventID;

	private readonly AkUnityEventHandlerInspector m_UnityEventHandlerInspector = new AkUnityEventHandlerInspector();
	private UnityEditor.SerializedProperty transitionDuration;

	public void OnEnable()
	{
		m_UnityEventHandlerInspector.Init(serializedObject);

		eventID = serializedObject.FindProperty("eventID");
		enableActionOnEvent = serializedObject.FindProperty("enableActionOnEvent");
		actionOnEventType = serializedObject.FindProperty("actionOnEventType");
		curveInterpolation = serializedObject.FindProperty("curveInterpolation");
		transitionDuration = serializedObject.FindProperty("transitionDuration");

		callbackData = serializedObject.FindProperty("m_callbackData");

		m_guidProperty = new[] { serializedObject.FindProperty("valueGuid.Array") };

		//Needed by the base class to know which type of component its working with
		m_typeName = "Event";
		m_objectType = AkWwiseProjectData.WwiseObjectType.EVENT;
	}

	public override void OnChildInspectorGUI()
	{
		serializedObject.Update();

		m_UnityEventHandlerInspector.OnGUI();

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		UnityEngine.GUILayout.BeginVertical("Box");
		{
			UnityEditor.EditorGUILayout.PropertyField(enableActionOnEvent, new UnityEngine.GUIContent("Action On Event: "));

			if (enableActionOnEvent.boolValue)
			{
				UnityEditor.EditorGUILayout.PropertyField(actionOnEventType, new UnityEngine.GUIContent("Action On EventType: "));
				UnityEditor.EditorGUILayout.PropertyField(curveInterpolation, new UnityEngine.GUIContent("Curve Interpolation: "));
				UnityEditor.EditorGUILayout.Slider(transitionDuration, 0.0f, 60.0f,
					new UnityEngine.GUIContent("Fade Time (secs): "));
			}
		}
		UnityEngine.GUILayout.EndVertical();

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		UnityEngine.GUILayout.BeginVertical("Box");
		{
			UnityEditor.EditorGUI.BeginChangeCheck();
			UnityEditor.EditorGUILayout.PropertyField(callbackData);
			if (UnityEditor.EditorGUI.EndChangeCheck())
				serializedObject.ApplyModifiedProperties();
		}
		UnityEngine.GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();

		UnityEngine.GUILayout.BeginVertical("Box");
		{
			var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.button);
			float inspectorWidth = UnityEngine.Screen.width - UnityEngine.GUI.skin.box.margin.left -
			                       UnityEngine.GUI.skin.box.margin.right;

			if (targets.Length == 1)
			{
				var akEvent = (AkEvent) target;
				var eventPlaying = AkEditorEventPlayer.Instance.IsEventPlaying(akEvent);
				if (eventPlaying)
				{
					if (UnityEngine.GUILayout.Button("Stop", style, UnityEngine.GUILayout.MaxWidth(inspectorWidth)))
					{
						UnityEngine.GUIUtility.hotControl = 0;
						AkEditorEventPlayer.Instance.StopEvent(akEvent);
					}
				}
				else
				{
					if (UnityEngine.GUILayout.Button("Play", style, UnityEngine.GUILayout.MaxWidth(inspectorWidth)))
					{
						UnityEngine.GUIUtility.hotControl = 0;
						AkEditorEventPlayer.Instance.PlayEvent(akEvent);
					}
				}
			}
			else
			{
				var playingEventsSelected = false;
				var stoppedEventsSelected = false;
				for (var i = 0; i < targets.Length; ++i)
				{
					var akEventTarget = targets[i] as AkEvent;
					if (akEventTarget != null)
					{
						if (AkEditorEventPlayer.Instance.IsEventPlaying(akEventTarget))
							playingEventsSelected = true;
						else
							stoppedEventsSelected = true;
						if (playingEventsSelected && stoppedEventsSelected)
							break;
					}
				}

				if (stoppedEventsSelected &&
				    UnityEngine.GUILayout.Button("Play Multiple", style, UnityEngine.GUILayout.MaxWidth(inspectorWidth)))
				{
					for (var i = 0; i < targets.Length; ++i)
					{
						var akEventTarget = targets[i] as AkEvent;
						if (akEventTarget != null)
							AkEditorEventPlayer.Instance.PlayEvent(akEventTarget);
					}
				}

				if (playingEventsSelected &&
				    UnityEngine.GUILayout.Button("Stop Multiple", style, UnityEngine.GUILayout.MaxWidth(inspectorWidth)))
				{
					for (var i = 0; i < targets.Length; ++i)
					{
						var akEventTarget = targets[i] as AkEvent;
						if (akEventTarget != null)
							AkEditorEventPlayer.Instance.StopEvent(akEventTarget);
					}
				}
			}

			if (UnityEngine.GUILayout.Button("Stop All", style, UnityEngine.GUILayout.MaxWidth(inspectorWidth)))
			{
				UnityEngine.GUIUtility.hotControl = 0;
				AkEditorEventPlayer.Instance.StopAll();
			}
		}

		UnityEngine.GUILayout.EndVertical();
	}

	public override string UpdateIds(System.Guid[] in_guid)
	{
		for (var i = 0; i < AkWwiseProjectInfo.GetData().EventWwu.Count; i++)
		{
			var e = AkWwiseProjectInfo.GetData().EventWwu[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[0]));

			if (e != null)
			{
				eventID.intValue = e.ID;
				serializedObject.ApplyModifiedProperties();

				return e.Name;
			}
		}

		return string.Empty;
	}

	public class AkEditorEventPlayer
	{
		private static AkEditorEventPlayer ms_Instance;

		private readonly System.Collections.Generic.List<AkEvent> akEvents = new System.Collections.Generic.List<AkEvent>();

		public static AkEditorEventPlayer Instance
		{
			get
			{
				if (ms_Instance == null)
					ms_Instance = new AkEditorEventPlayer();
				return ms_Instance;
			}
		}

		private void CallbackHandler(object in_cookie, AkCallbackType in_type, object in_info)
		{
			if (in_type == AkCallbackType.AK_EndOfEvent)
				RemoveAkEvent(in_cookie as AkEvent);
		}

		public void PlayEvent(AkEvent akEvent)
		{
			if (IsEventPlaying(akEvent))
				return;

			var playingID = AkSoundEngine.PostEvent((uint) akEvent.eventID, akEvent.gameObject,
				(uint) AkCallbackType.AK_EndOfEvent, CallbackHandler, akEvent);
			if (playingID != AkSoundEngine.AK_INVALID_PLAYING_ID)
				AddAkEvent(akEvent);
		}

		public void StopEvent(AkEvent akEvent)
		{
			if (!IsEventPlaying(akEvent))
				return;

			var result = AkSoundEngine.ExecuteActionOnEvent((uint) akEvent.eventID, AkActionOnEventType.AkActionOnEventType_Stop,
				akEvent.gameObject, 0);
			if (result == AKRESULT.AK_Success)
				RemoveAkEvent(akEvent);
			else
				UnityEngine.Debug.LogWarning("WwiseUnity: AkEditorEventPlayer: Failed to stop event: " + akEvent.name + "(id: " +
				                             akEvent.eventID + ")!");
		}

		private void AddAkEvent(AkEvent akEvent)
		{
			akEvents.Add(akEvent);

			// In the case where objects are being placed in edit mode and then previewed, their positions won't yet be updated so we ensure they're updated here.
			AkSoundEngine.SetObjectPosition(akEvent.gameObject, akEvent.transform);
		}

		private void RemoveAkEvent(AkEvent akEvent)
		{
			if (akEvent != null)
				akEvents.Remove(akEvent);
		}

		public bool IsEventPlaying(AkEvent akEvent)
		{
			return akEvents.Contains(akEvent);
		}

		public void StopAll()
		{
			AkSoundEngine.StopAll();
		}
	}
}
#endif