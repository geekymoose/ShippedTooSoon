#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2017 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

#if UNITY_2017_1_OR_NEWER

#if UNITY_EDITOR
public class MinMaxEventDuration
{
	private UnityEngine.Vector2 MinMaxDuration = UnityEngine.Vector2.zero;

	public float MinDuration
	{
		get { return MinMaxDuration.x; }
		set { MinMaxDuration.Set(value, MinMaxDuration.y); }
	}

	public float MaxDuration
	{
		get { return MinMaxDuration.y; }
		set { MinMaxDuration.Set(MinMaxDuration.x, value); }
	}

	public static MinMaxEventDuration GetMinMaxDuration(AK.Wwise.Event akEvent)
	{
		var result = new MinMaxEventDuration();
		var FullSoundbankPath = AkBasePathGetter.GetPlatformBasePath();
		var filename = System.IO.Path.Combine(FullSoundbankPath, "SoundbanksInfo.xml");
		var MaxDuration = 1000000.0f;
		if (System.IO.File.Exists(filename))
		{
			var doc = new System.Xml.XmlDocument();
			doc.Load(filename);

			var soundBanks = doc.GetElementsByTagName("SoundBanks");
			for (var i = 0; i < soundBanks.Count; i++)
			{
				var soundBank = soundBanks[i].SelectNodes("SoundBank");
				for (var j = 0; j < soundBank.Count; j++)
				{
					var includedEvents = soundBank[j].SelectNodes("IncludedEvents");
					for (var ie = 0; ie < includedEvents.Count; ie++)
					{
						var events = includedEvents[i].SelectNodes("Event");
						for (var e = 0; e < events.Count; e++)
						{
							if (events[e].Attributes["Id"] != null && uint.Parse(events[e].Attributes["Id"].InnerText) == (uint) akEvent.ID)
							{
								if (events[e].Attributes["DurationType"] != null &&
								    events[e].Attributes["DurationType"].InnerText == "Infinite")
								{
									// Set both min and max to MaxDuration for infinite events
									result.MinDuration = MaxDuration;
									result.MaxDuration = MaxDuration;
								}

								if (events[e].Attributes["DurationMin"] != null)
									result.MinDuration = float.Parse(events[e].Attributes["DurationMin"].InnerText);
								if (events[e].Attributes["DurationMax"] != null)
									result.MaxDuration = float.Parse(events[e].Attributes["DurationMax"].InnerText);
								break;
							}
						}
					}
				}
			}
		}

		return result;
	}
}
#endif //UNITY_EDITOR

public class WwiseEventTracker
{
	public float currentDuration = -1.0f;
	public float currentDurationProportion = 1.0f;
	public bool eventIsPlaying;
	public bool fadeoutTriggered;
	public uint playingID;
	public float previousEventStartTime;

	public void CallbackHandler(object in_cookie, AkCallbackType in_type, object in_info)
	{
		if (in_type == AkCallbackType.AK_EndOfEvent)
		{
			eventIsPlaying = false;
			fadeoutTriggered = false;
		}
		else if (in_type == AkCallbackType.AK_Duration)
		{
			var estimatedDuration = ((AkDurationCallbackInfo) in_info).fEstimatedDuration;
			currentDuration = estimatedDuration * currentDurationProportion / 1000.0f;
		}
	}
}

/// @brief A playable asset containing a Wwise event that can be placed within a \ref AkEventTrack in a timeline.
/// @details Use this class to play Wwise events from a timeline and synchronise them to the animation. Events will be emitted from the GameObject that is bound to the AkEventTrack. Use the overrideTrackEmitterObject option to choose a different GameObject from which to emit the Wwise event. 
/// \sa
/// - \ref AkEventTrack
/// - \ref AkEventPlayableBehavior
[System.Serializable]
public class AkEventPlayable : UnityEngine.Playables.PlayableAsset, UnityEngine.Timeline.ITimelineClipAsset
{
	private readonly WwiseEventTracker eventTracker = new WwiseEventTracker();
	public AK.Wwise.Event akEvent;
	private float blendInDuration;
	private float blendOutDuration;

	private float easeInDuration;
	private float easeOutDuration;
	public UnityEngine.ExposedReference<UnityEngine.GameObject> emitterObjectRef;

	[UnityEngine.SerializeField] private float eventDurationMax = -1.0f;

	[UnityEngine.SerializeField] private float eventDurationMin = -1.0f;

	public bool overrideTrackEmitterObject = false;

	private UnityEngine.Timeline.TimelineClip owningClip;

#if UNITY_EDITOR
	//Used to track when the event has been changed in OnValidate so that the duration can be updated at the correct time.
	private int previousEventID;
#endif

	public bool retriggerEvent = false;

	public UnityEngine.Timeline.TimelineClip OwningClip
	{
		get { return owningClip; }
		set { owningClip = value; }
	}

	public override double duration
	{
		get
		{
			if (akEvent == null)
				return base.duration;

			return eventDurationMax;
		}
	}

	public UnityEngine.Timeline.ClipCaps clipCaps
	{
		get
		{
			if (!retriggerEvent)
				return UnityEngine.Timeline.ClipCaps.All;
			return UnityEngine.Timeline.ClipCaps.Looping & UnityEngine.Timeline.ClipCaps.Extrapolation &
			       UnityEngine.Timeline.ClipCaps.ClipIn & UnityEngine.Timeline.ClipCaps.SpeedMultiplier;
		}
	}

	public void setEaseInDuration(float d)
	{
		easeInDuration = d;
	}

	public void setEaseOutDuration(float d)
	{
		easeOutDuration = d;
	}

	public void setBlendInDuration(float d)
	{
		blendInDuration = d;
	}

	public void setBlendOutDuration(float d)
	{
		blendOutDuration = d;
	}

	public override UnityEngine.Playables.Playable CreatePlayable(UnityEngine.Playables.PlayableGraph graph,
		UnityEngine.GameObject owner)
	{
		var playable = UnityEngine.Playables.ScriptPlayable<AkEventPlayableBehavior>.Create(graph);
		var b = playable.GetBehaviour();
		initializeBehaviour(graph, b, owner);
		b.akEventMinDuration = eventDurationMin;
		b.akEventMaxDuration = eventDurationMax;
		return playable;
	}

	public void initializeBehaviour(UnityEngine.Playables.PlayableGraph graph, AkEventPlayableBehavior b,
		UnityEngine.GameObject owner)
	{
		b.akEvent = akEvent;
		b.eventTracker = eventTracker;
		b.easeInDuration = easeInDuration;
		b.easeOutDuration = easeOutDuration;
		b.blendInDuration = blendInDuration;
		b.blendOutDuration = blendOutDuration;
		b.eventShouldRetrigger = retriggerEvent;
		b.overrideTrackEmittorObject = overrideTrackEmitterObject;

		if (overrideTrackEmitterObject)
			b.eventObject = emitterObjectRef.Resolve(graph.GetResolver());
		else
			b.eventObject = owner;
	}

#if UNITY_EDITOR
	private void updateWwiseEventDurations()
	{
		if (akEvent != null)
		{
			var MinMaxDuration = MinMaxEventDuration.GetMinMaxDuration(akEvent);
			eventDurationMin = MinMaxDuration.MinDuration;
			eventDurationMax = MinMaxDuration.MaxDuration;
		}
	}

	public void OnValidate()
	{
		if (previousEventID != akEvent.ID)
		{
			previousEventID = akEvent.ID;
			updateWwiseEventDurations();
			if (owningClip != null) owningClip.duration = eventDurationMax;
		}
	}
#endif
}

/// @brief Defines the behavior of a \ref AkEventPlayable within a \ref AkEventTrack.
/// \sa
/// - \ref AkEventTrack
/// - \ref AkEventPlayable
public class AkEventPlayableBehavior : UnityEngine.Playables.PlayableBehaviour
{
	public enum AkPlayableAction
	{
		None = 0,
		Playback = 1,
		Retrigger = 2,
		Stop = 4,
		DelayedStop = 8,
		Seek = 16,
		FadeIn = 32,
		FadeOut = 64
	}

	public static int scrubPlaybackLengthMs = 100;

	public AK.Wwise.Event akEvent;
	public float akEventMaxDuration = -1.0f;

	public float akEventMinDuration = -1.0f;
	public float blendInDuration;
	public float blendOutDuration;

	public float easeInDuration;
	public float easeOutDuration;
	public UnityEngine.GameObject eventObject;

	public bool eventShouldRetrigger;

	public WwiseEventTracker eventTracker;

	public float lastEffectiveWeight = 1.0f;

	public bool overrideTrackEmittorObject;

	public uint requiredActions;

	public override void PrepareFrame(UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info)
	{
		if (eventTracker != null)
		{
			// We disable scrubbing in edit mode, due to an issue with how FrameData.EvaluationType is handled in edit mode.
			// This is a known issue and Unity are aware of it: https://fogbugz.unity3d.com/default.asp?953109_kitf7pso0vmjm0m0
			var scrubbing = info.evaluationType == UnityEngine.Playables.FrameData.EvaluationType.Evaluate &&
			                UnityEngine.Application.isPlaying;
			if (scrubbing && ShouldPlay(playable))
			{
				if (!eventTracker.eventIsPlaying)
				{
					requiredActions |= (uint) AkPlayableAction.Playback;
					requiredActions |= (uint) AkPlayableAction.DelayedStop;
					checkForFadeIn((float) UnityEngine.Playables.PlayableExtensions.GetTime(playable));
					checkForFadeOut(playable);
				}

				requiredActions |= (uint) AkPlayableAction.Seek;
			}
			else // The clip is playing but the event hasn't been triggered. We need to start the event and jump to the correct time.
			{
				if (!eventTracker.eventIsPlaying && (requiredActions & (uint) AkPlayableAction.Playback) == 0)
				{
					requiredActions |= (uint) AkPlayableAction.Retrigger;
					checkForFadeIn((float) UnityEngine.Playables.PlayableExtensions.GetTime(playable));
				}

				checkForFadeOut(playable);
			}
		}
	}

	public override void OnBehaviourPlay(UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info)
	{
		if (akEvent != null)
		{
			if (ShouldPlay(playable))
			{
				requiredActions |= (uint) AkPlayableAction.Playback;
				// If we've explicitly set the playhead, only play a small snippet.
				// We disable scrubbing in edit mode, due to an issue with how FrameData.EvaluationType is handled in edit mode.
				// This is a known issue and Unity are aware of it: https://fogbugz.unity3d.com/default.asp?953109_kitf7pso0vmjm0m0
				if (info.evaluationType == UnityEngine.Playables.FrameData.EvaluationType.Evaluate &&
				    UnityEngine.Application.isPlaying)
				{
					requiredActions |= (uint) AkPlayableAction.DelayedStop;
					checkForFadeIn((float) UnityEngine.Playables.PlayableExtensions.GetTime(playable));
					checkForFadeOut(playable);
				}
				else
				{
					var proportionalTime = getProportionalTime(playable);
					var alph = 0.05f;
					// we need to jump to the correct position in the case where the event is played from some non-start position.
					if (proportionalTime > alph)
						requiredActions |= (uint) AkPlayableAction.Seek;
					checkForFadeIn((float) UnityEngine.Playables.PlayableExtensions.GetTime(playable));
					checkForFadeOut(playable);
				}
			}
		}
	}

	public override void OnBehaviourPause(UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info)
	{
		if (eventObject != null)
			stopEvent();
	}

	public override void ProcessFrame(UnityEngine.Playables.Playable playable, UnityEngine.Playables.FrameData info,
		object playerData)
	{
		if (!overrideTrackEmittorObject)
		{
			var obj = playerData as UnityEngine.GameObject;
			if (obj != null)
				eventObject = obj;
		}

		if (eventObject != null)
		{
			var clipTime = (float) UnityEngine.Playables.PlayableExtensions.GetTime(playable);
			if (actionIsRequired(AkPlayableAction.Playback))
				playEvent();
			if (eventShouldRetrigger && actionIsRequired(AkPlayableAction.Retrigger))
				retriggerEvent(playable);
			if (actionIsRequired(AkPlayableAction.Stop))
				akEvent.Stop(eventObject);
			if (actionIsRequired(AkPlayableAction.DelayedStop))
				stopEvent(scrubPlaybackLengthMs);
			if (actionIsRequired(AkPlayableAction.Seek))
				seekToTime(playable);
			if (actionIsRequired(AkPlayableAction.FadeIn))
				triggerFadeIn(clipTime);
			if (actionIsRequired(AkPlayableAction.FadeOut))
			{
				var timeLeft = (float) (UnityEngine.Playables.PlayableExtensions.GetDuration(playable) -
				                        UnityEngine.Playables.PlayableExtensions.GetTime(playable));
				triggerFadeOut(timeLeft);
			}
		}

		requiredActions = (uint) AkPlayableAction.None;
	}

	private bool actionIsRequired(AkPlayableAction actionType)
	{
		return (requiredActions & (uint) actionType) != 0;
	}

	/** Check the playable time against the Wwise event duration to see if playback should occur.
     */
	private bool ShouldPlay(UnityEngine.Playables.Playable playable)
	{
		if (eventTracker != null)
		{
			// If max and min duration values from metadata are equal, we can assume a deterministic event.
			if (akEventMaxDuration == akEventMinDuration && akEventMinDuration != -1.0f)
			{
				return (float) UnityEngine.Playables.PlayableExtensions.GetTime(playable) < akEventMaxDuration ||
				       eventShouldRetrigger;
			}

			var currentTime = (float) UnityEngine.Playables.PlayableExtensions.GetTime(playable) -
			                  eventTracker.previousEventStartTime;
			var currentDuration = eventTracker.currentDuration;
			var maxDuration = currentDuration == -1.0f
				? (float) UnityEngine.Playables.PlayableExtensions.GetDuration(playable)
				: currentDuration;
			return currentTime < maxDuration || eventShouldRetrigger;
		}

		return false;
	}

	private bool fadeInRequired(float currentClipTime)
	{
		// Check whether we are currently within a fade in or blend in segment.
		var remainingBlendInDuration = blendInDuration - currentClipTime;
		var remainingEaseInDuration = easeInDuration - currentClipTime;
		return remainingBlendInDuration > 0.0f || remainingEaseInDuration > 0.0f;
	}

	private void checkForFadeIn(float currentClipTime)
	{
		if (fadeInRequired(currentClipTime))
			requiredActions |= (uint) AkPlayableAction.FadeIn;
	}

	private void checkForFadeInImmediate(float currentClipTime)
	{
		if (fadeInRequired(currentClipTime))
			triggerFadeIn(currentClipTime);
	}

	private bool fadeOutRequired(UnityEngine.Playables.Playable playable)
	{
		// Check whether we are currently within a fade out or blend out segment.
		var timeLeft = (float) (UnityEngine.Playables.PlayableExtensions.GetDuration(playable) -
		                        UnityEngine.Playables.PlayableExtensions.GetTime(playable));
		var remainingBlendOutDuration = blendOutDuration - timeLeft;
		var remainingEaseOutDuration = easeOutDuration - timeLeft;
		return remainingBlendOutDuration >= 0.0f || remainingEaseOutDuration >= 0.0f;
	}

	private void checkForFadeOutImmediate(UnityEngine.Playables.Playable playable)
	{
		if (eventTracker != null && !eventTracker.fadeoutTriggered)
		{
			if (fadeOutRequired(playable))
			{
				var timeLeft = (float) (UnityEngine.Playables.PlayableExtensions.GetDuration(playable) -
				                        UnityEngine.Playables.PlayableExtensions.GetTime(playable));
				triggerFadeOut(timeLeft);
			}
		}
	}

	private void checkForFadeOut(UnityEngine.Playables.Playable playable)
	{
		if (eventTracker != null && !eventTracker.fadeoutTriggered && fadeOutRequired(playable))
			requiredActions |= (uint) AkPlayableAction.FadeOut;
	}

	protected void triggerFadeIn(float currentClipTime)
	{
		if (eventObject != null && akEvent != null)
		{
			var fadeDuration = UnityEngine.Mathf.Max(easeInDuration - currentClipTime, blendInDuration - currentClipTime);
			if (fadeDuration > 0.0f)
			{
				akEvent.ExecuteAction(eventObject, AkActionOnEventType.AkActionOnEventType_Pause, 0,
					AkCurveInterpolation.AkCurveInterpolation_Linear);
				akEvent.ExecuteAction(eventObject, AkActionOnEventType.AkActionOnEventType_Resume, (int) (fadeDuration * 1000.0f),
					AkCurveInterpolation.AkCurveInterpolation_Linear);
			}
		}
	}

	protected void triggerFadeOut(float fadeDuration)
	{
		if (eventObject != null && akEvent != null)
		{
			if (eventTracker != null)
				eventTracker.fadeoutTriggered = true;
			akEvent.ExecuteAction(eventObject, AkActionOnEventType.AkActionOnEventType_Stop, (int) (fadeDuration * 1000.0f),
				AkCurveInterpolation.AkCurveInterpolation_Linear);
		}
	}

	protected void stopEvent(int transition = 0)
	{
		if (eventObject != null && akEvent != null && eventTracker.eventIsPlaying)
		{
			akEvent.Stop(eventObject, transition);
			if (eventTracker != null)
				eventTracker.eventIsPlaying = false;
		}
	}

	protected void playEvent()
	{
		if (eventObject != null && akEvent != null && eventTracker != null)
		{
			eventTracker.playingID = akEvent.Post(eventObject,
				(uint) AkCallbackType.AK_EndOfEvent | (uint) AkCallbackType.AK_Duration, eventTracker.CallbackHandler, null);
			if (eventTracker.playingID != AkSoundEngine.AK_INVALID_PLAYING_ID)
			{
				eventTracker.eventIsPlaying = true;
				eventTracker.currentDurationProportion = 1.0f;
				eventTracker.previousEventStartTime = 0.0f;
			}
		}
	}

	protected void retriggerEvent(UnityEngine.Playables.Playable playable)
	{
		if (eventObject != null && akEvent != null && eventTracker != null)
		{
			eventTracker.playingID = akEvent.Post(eventObject,
				(uint) AkCallbackType.AK_EndOfEvent | (uint) AkCallbackType.AK_Duration, eventTracker.CallbackHandler, null);
			if (eventTracker.playingID != AkSoundEngine.AK_INVALID_PLAYING_ID)
			{
				eventTracker.eventIsPlaying = true;
				var proportionOfDurationLeft = seekToTime(playable);
				eventTracker.currentDurationProportion = proportionOfDurationLeft;
				eventTracker.previousEventStartTime = (float) UnityEngine.Playables.PlayableExtensions.GetTime(playable);
			}
		}
	}

	protected float getProportionalTime(UnityEngine.Playables.Playable playable)
	{
		if (eventTracker != null)
		{
			// If max and min duration values from metadata are equal, we can assume a deterministic event.
			if (akEventMaxDuration == akEventMinDuration && akEventMinDuration != -1.0f)
			{
				// If the timeline clip has length greater than the event duration, we want to loop.
				return (float) UnityEngine.Playables.PlayableExtensions.GetTime(playable) % akEventMaxDuration / akEventMaxDuration;
			}

			var currentTime = (float) UnityEngine.Playables.PlayableExtensions.GetTime(playable) -
			                  eventTracker.previousEventStartTime;
			var currentDuration = eventTracker.currentDuration;
			var maxDuration = currentDuration == -1.0f
				? (float) UnityEngine.Playables.PlayableExtensions.GetDuration(playable)
				: currentDuration;
			// If the timeline clip has length greater than the event duration, we want to loop.
			return currentTime % maxDuration / maxDuration;
		}

		return 0.0f;
	}

	// Seek to the current time, taking looping into account.
	// Return the proportion of the current event estimated duration that is left, after the seek.
	protected float seekToTime(UnityEngine.Playables.Playable playable)
	{
		if (eventObject != null && akEvent != null)
		{
			var proportionalTime = getProportionalTime(playable);
			if (proportionalTime < 1.0f) // Avoids Wwise "seeking beyond end of event: audio will stop" error.
			{
				AkSoundEngine.SeekOnEvent((uint) akEvent.ID, eventObject, proportionalTime);
				return 1.0f - proportionalTime;
			}
		}

		return 1.0f;
	}
}
#endif //UNITY_2017_1_OR_NEWER
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.