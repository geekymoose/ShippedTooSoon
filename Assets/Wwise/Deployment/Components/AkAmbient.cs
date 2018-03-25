#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

public enum MultiPositionTypeLabel
{
	Simple_Mode,
	Large_Mode,
	MultiPosition_Mode
}

public class AkMultiPosEvent
{
	public bool eventIsPlaying;
	public System.Collections.Generic.List<AkAmbient> list = new System.Collections.Generic.List<AkAmbient>();

	public void FinishedPlaying(object in_cookie, AkCallbackType in_type, object in_info)
	{
		eventIsPlaying = false;
	}
}

[UnityEngine.AddComponentMenu("Wwise/AkAmbient")]
/// @brief Use this component to attach a Wwise Event to any object in a scene.
/// The sound can be started at various moments, dependent on the selected Unity trigger. This component is more useful for ambient sounds (sounds related to scene-bound objects) but could also be used for other purposes.
/// Since AkAmbient has AkEvent as its base class, it features the play/stop, play multiple, stop multiple and stop all buttons for previewing the associated Wwise event.
/// \sa
/// - \ref unity_use_AkEvent_AkAmbient
/// - \ref AkGameObj
/// - \ref AkEvent
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=soundengine__events.html" target="_blank">Integration Details - Events</a> (Note: This is described in the Wwise SDK documentation.)
public class AkAmbient : AkEvent
{
	public static System.Collections.Generic.Dictionary<int, AkMultiPosEvent> multiPosEventTree =
		new System.Collections.Generic.Dictionary<int, AkMultiPosEvent>();

	public System.Collections.Generic.List<UnityEngine.Vector3> multiPositionArray =
		new System.Collections.Generic.List<UnityEngine.Vector3>();

	public AkMultiPositionType MultiPositionType = AkMultiPositionType.MultiPositionType_MultiSources;
	public MultiPositionTypeLabel multiPositionTypeLabel = MultiPositionTypeLabel.Simple_Mode;
	public AkAmbient ParentAkAmbience { get; set; }

	private void OnEnable()
	{
		if (multiPositionTypeLabel == MultiPositionTypeLabel.Simple_Mode)
		{
			var gameObj = gameObject.GetComponents<AkGameObj>();
			for (var i = 0; i < gameObj.Length; i++)
				gameObj[i].enabled = true;
		}
		else if (multiPositionTypeLabel == MultiPositionTypeLabel.Large_Mode)
		{
			var gameObj = gameObject.GetComponents<AkGameObj>();
			for (var i = 0; i < gameObj.Length; i++)
				gameObj[i].enabled = false;

			var positionArray = BuildAkPositionArray();
			AkSoundEngine.SetMultiplePositions(gameObject, positionArray, (ushort) positionArray.Count, MultiPositionType);
		}
		else if (multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode)
		{
			var gameObj = gameObject.GetComponents<AkGameObj>();
			for (var i = 0; i < gameObj.Length; i++)
				gameObj[i].enabled = false;

			AkMultiPosEvent eventPosList;

			if (multiPosEventTree.TryGetValue(eventID, out eventPosList))
			{
				if (!eventPosList.list.Contains(this))
					eventPosList.list.Add(this);
			}
			else
			{
				eventPosList = new AkMultiPosEvent();
				eventPosList.list.Add(this);
				multiPosEventTree.Add(eventID, eventPosList);
			}

			var positionArray = BuildMultiDirectionArray(eventPosList);

			//Set multiple positions
			AkSoundEngine.SetMultiplePositions(eventPosList.list[0].gameObject, positionArray, (ushort) positionArray.Count,
				MultiPositionType);
		}
	}

	private void OnDisable()
	{
		if (multiPositionTypeLabel == MultiPositionTypeLabel.MultiPosition_Mode)
		{
			var eventPosList = multiPosEventTree[eventID];

			if (eventPosList.list.Count == 1)
				multiPosEventTree.Remove(eventID);
			else
			{
				eventPosList.list.Remove(this);

				var positionArray = BuildMultiDirectionArray(eventPosList);
				AkSoundEngine.SetMultiplePositions(eventPosList.list[0].gameObject, positionArray, (ushort) positionArray.Count,
					MultiPositionType);
			}
		}
	}

	public override void HandleEvent(UnityEngine.GameObject in_gameObject)
	{
		if (multiPositionTypeLabel != MultiPositionTypeLabel.MultiPosition_Mode)
			base.HandleEvent(in_gameObject);
		else
		{
			var multiPositionSoundEmitter = multiPosEventTree[eventID];
			if (multiPositionSoundEmitter.eventIsPlaying)
				return;

			multiPositionSoundEmitter.eventIsPlaying = true;

			soundEmitterObject = multiPositionSoundEmitter.list[0].gameObject;

			if (enableActionOnEvent)
			{
				AkSoundEngine.ExecuteActionOnEvent((uint) eventID, actionOnEventType, multiPositionSoundEmitter.list[0].gameObject,
					(int) transitionDuration * 1000, curveInterpolation);
			}
			else
			{
				playingId = AkSoundEngine.PostEvent((uint) eventID, multiPositionSoundEmitter.list[0].gameObject,
					(uint) AkCallbackType.AK_EndOfEvent, multiPositionSoundEmitter.FinishedPlaying, null, 0, null,
					AkSoundEngine.AK_INVALID_PLAYING_ID);
			}
		}
	}

	public void OnDrawGizmosSelected()
	{
		UnityEngine.Gizmos.DrawIcon(transform.position, "WwiseAudioSpeaker.png", false);
	}

	public AkPositionArray BuildMultiDirectionArray(AkMultiPosEvent eventPosList)
	{
		var positionArray = new AkPositionArray((uint) eventPosList.list.Count);
		for (var i = 0; i < eventPosList.list.Count; i++)
		{
			positionArray.Add(eventPosList.list[i].transform.position, eventPosList.list[i].transform.forward,
				eventPosList.list[i].transform.up);
		}

		return positionArray;
	}

	private AkPositionArray BuildAkPositionArray()
	{
		var positionArray = new AkPositionArray((uint) multiPositionArray.Count);
		for (var i = 0; i < multiPositionArray.Count; i++)
			positionArray.Add(transform.position + multiPositionArray[i], transform.forward, transform.up);

		return positionArray;
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.