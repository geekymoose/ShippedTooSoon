#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

/// <summary>
///     Event callback information.
///     Event callback functions can receive this structure as a parameter
/// </summary>
public class AkEventCallbackMsg
{
	///For more information about the event callback, see the classes derived from AkCallbackInfo.
	public AkCallbackInfo info;

	/// GameObject from whom the callback function was called
	public UnityEngine.GameObject sender;

	///AkSoundEngine.PostEvent callback flags. See the AkCallbackType enumeration for a list of all callbacks
	public AkCallbackType type;
}

[UnityEngine.AddComponentMenu("Wwise/AkEvent")]
[UnityEngine.RequireComponent(typeof(AkGameObj))]
/// @brief Helper class that knows a Wwise Event and when to trigger it in Unity. As of 2017.2.0, the AkEvent inspector has buttons for play/stop, play multiple, stop multiple, and stop all.
/// Play/Stop will play or stop the event such that it can be previewed both in edit mode and play mode. When multiple objects are selected, Play Multiple and Stop Multiple will play or stop the associated AkEvent for each object.
/// \sa
/// - \ref sect_edit_mode
/// - \ref unity_use_AkEvent_AkAmbient
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=soundengine__events.html" target="_blank">Integration Details - Events</a> (Note: This is described in the Wwise SDK documentation.)
public class AkEvent : AkUnityEventHandler
{
	/// Replacement action.  See AK::SoundEngine::ExecuteEventOnAction()
	public AkActionOnEventType actionOnEventType = AkActionOnEventType.AkActionOnEventType_Stop;

	/// Fade curve to use with the new Action.  See AK::SoundEngine::ExecuteEventOnAction()
	public AkCurveInterpolation curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear;

	/// Enables additional options to reuse existing events.  Use it to transform a Play event into a Stop event without having to define one in the Wwise Project.
	public bool enableActionOnEvent = false;

	/// ID of the Event as found in the WwiseID.cs file
	public int eventID = 0;

	//
	public AkEventCallbackData m_callbackData = null;
	public uint playingId = AkSoundEngine.AK_INVALID_PLAYING_ID;

	/// Game object onto which the Event will be posted.  By default, when empty, it is posted on the same object on which the component was added.
	public UnityEngine.GameObject soundEmitterObject;

	/// Duration of the fade.  See AK::SoundEngine::ExecuteEventOnAction()
	public float transitionDuration = 0.0f;
#if UNITY_EDITOR
	public byte[] valueGuid = new byte[16];
#endif

	private void Callback(object in_cookie, AkCallbackType in_type, AkCallbackInfo in_info)
	{
		for (var i = 0; i < m_callbackData.callbackFunc.Count; i++)
		{
			if (((int) in_type & m_callbackData.callbackFlags[i]) != 0 && m_callbackData.callbackGameObj[i] != null)
			{
				var callbackInfo = new AkEventCallbackMsg();
				callbackInfo.type = in_type;
				callbackInfo.sender = gameObject;
				callbackInfo.info = in_info;

				m_callbackData.callbackGameObj[i].SendMessage(m_callbackData.callbackFunc[i], callbackInfo);
			}
		}
	}

	public override void HandleEvent(UnityEngine.GameObject in_gameObject)
	{
		var gameObj = useOtherObject && in_gameObject != null ? in_gameObject : gameObject;

		soundEmitterObject = gameObj;

		if (enableActionOnEvent)
		{
			AkSoundEngine.ExecuteActionOnEvent((uint) eventID, actionOnEventType, gameObj, (int) transitionDuration * 1000,
				curveInterpolation);
			return;
		}

		if (m_callbackData != null)
		{
			playingId = AkSoundEngine.PostEvent((uint) eventID, gameObj, (uint) m_callbackData.uFlags, Callback, null, 0, null,
				AkSoundEngine.AK_INVALID_PLAYING_ID);
		}
		else
			playingId = AkSoundEngine.PostEvent((uint) eventID, gameObj);

		if (playingId == AkSoundEngine.AK_INVALID_PLAYING_ID && AkSoundEngine.IsInitialized())
		{
			UnityEngine.Debug.LogError("Could not post event ID \"" + (uint) eventID +
			                           "\". Did you make sure to load the appropriate SoundBank?");
		}
	}

	public void Stop(int _transitionDuration,
		AkCurveInterpolation _curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear)
	{
		AkSoundEngine.ExecuteActionOnEvent((uint) eventID, AkActionOnEventType.AkActionOnEventType_Stop, soundEmitterObject,
			_transitionDuration, _curveInterpolation);
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.