#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.

namespace AK.Wwise
{
	[System.Serializable]
	///@brief This type represents the base for all Wwise Types that require a GUID.
	public class BaseType
	{
		public int ID = (int) AkSoundEngine.AK_INVALID_UNIQUE_ID;

#if UNITY_EDITOR
		public byte[] valueGuid = new byte[16];
#endif

		protected uint GetID()
		{
			return (uint) ID;
		}

		public virtual bool IsValid()
		{
			return ID != AkSoundEngine.AK_INVALID_UNIQUE_ID;
		}

		public bool Validate()
		{
			if (IsValid())
				return true;

			UnityEngine.Debug.LogWarning("Wwise ID has not been resolved. Consider picking a new " + GetType().Name + ".");
			return false;
		}

		protected void Verify(AKRESULT result)
		{
#if UNITY_EDITOR
			if (result != AKRESULT.AK_Success && AkSoundEngine.IsInitialized())
				UnityEngine.Debug.LogWarning("Unsuccessful call made on " + GetType().Name + ".");
#endif
		}
	}

	[System.Serializable]
	///@brief This type represents the base for all Wwise Types that also require a group GUID, such as State and Switch.
	public class BaseGroupType : BaseType
	{
#if UNITY_EDITOR
		public byte[] groupGuid = new byte[16];
#endif

		public int groupID = 0;

		protected uint GetGroupID()
		{
			return (uint) groupID;
		}

		public override bool IsValid()
		{
			return base.IsValid() && groupID != 0;
		}
	}

	[System.Serializable]
	///@brief This type represents the values of the flags used when posting an Event with a callback.
	public class CallbackFlags
	{
		public uint value = 0;
	}

	[System.Serializable]
	///@brief This type can be used to post Events to the sound engine.
	public class Event : BaseType
	{
		private void VerifyPlayingID(uint playingId)
		{
#if UNITY_EDITOR
			if (playingId == AkSoundEngine.AK_INVALID_PLAYING_ID && AkSoundEngine.IsInitialized())
			{
				UnityEngine.Debug.LogError("Could not post event ID \"" + GetID() +
				                           "\". Did you make sure to load the appropriate SoundBank?");
			}
#endif
		}

		/// <summary>
		///     Posts this Event on a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <returns>Returns the playing ID.</returns>
		public uint Post(UnityEngine.GameObject gameObject)
		{
			if (!IsValid())
				return AkSoundEngine.AK_INVALID_PLAYING_ID;

			var playingId = AkSoundEngine.PostEvent(GetID(), gameObject);
			VerifyPlayingID(playingId);
			return playingId;
		}

		/// <summary>
		///     Posts this Event on a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="flags"></param>
		/// <param name="callback"></param>
		/// <param name="cookie">Optional cookie received by the callback</param>
		/// <returns>Returns the playing ID.</returns>
		public uint Post(UnityEngine.GameObject gameObject, CallbackFlags flags, AkCallbackManager.EventCallback callback,
			object cookie = null)
		{
			if (!IsValid())
				return AkSoundEngine.AK_INVALID_PLAYING_ID;

			var playingId = AkSoundEngine.PostEvent(GetID(), gameObject, flags.value, callback, cookie);
			VerifyPlayingID(playingId);
			return playingId;
		}

		/// <summary>
		///     Posts this Event on a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="flags"></param>
		/// <param name="callback"></param>
		/// <param name="cookie">Optional cookie received by the callback</param>
		/// <returns>Returns the playing ID.</returns>
		public uint Post(UnityEngine.GameObject gameObject, uint flags, AkCallbackManager.EventCallback callback,
			object cookie = null)
		{
			if (!IsValid())
				return AkSoundEngine.AK_INVALID_PLAYING_ID;

			var playingId = AkSoundEngine.PostEvent(GetID(), gameObject, flags, callback, cookie);
			VerifyPlayingID(playingId);
			return playingId;
		}

		public void Stop(UnityEngine.GameObject gameObject, int transitionDuration = 0,
			AkCurveInterpolation curveInterpolation = AkCurveInterpolation.AkCurveInterpolation_Linear)
		{
			ExecuteAction(gameObject, AkActionOnEventType.AkActionOnEventType_Stop, transitionDuration, curveInterpolation);
		}

		/// <summary>
		///     Executes various actions on this event associated with a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="actionOnEventType"></param>
		/// <param name="transitionDuration"></param>
		/// <param name="curveInterpolation"></param>
		public void ExecuteAction(UnityEngine.GameObject gameObject, AkActionOnEventType actionOnEventType,
			int transitionDuration, AkCurveInterpolation curveInterpolation)
		{
			if (IsValid())
			{
				var result = AkSoundEngine.ExecuteActionOnEvent(GetID(), actionOnEventType, gameObject, transitionDuration,
					curveInterpolation);
				Verify(result);
			}
		}

		/// <summary>
		///     Posts MIDI Events on this Event associated with a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="array">The array of AkMIDIPost that are posted.</param>
		public void PostMIDI(UnityEngine.GameObject gameObject, AkMIDIPostArray array)
		{
			if (IsValid())
				array.PostOnEvent(GetID(), gameObject);
		}

		/// <summary>
		///     Posts MIDI Events on this Event associated with a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		/// <param name="array">The array of AkMIDIPost that are posted.</param>
		/// <param name="count">The number of elements from the array that will be posted.</param>
		public void PostMIDI(UnityEngine.GameObject gameObject, AkMIDIPostArray array, int count)
		{
			if (IsValid())
				array.PostOnEvent(GetID(), gameObject, count);
		}

		/// <summary>
		///     Stops MIDI Events on this Event associated with a GameObject.
		/// </summary>
		/// <param name="gameObject">The GameObject</param>
		public void StopMIDI(UnityEngine.GameObject gameObject)
		{
			if (IsValid())
				AkSoundEngine.StopMIDIOnEvent(GetID(), gameObject);
		}

		/// <summary>
		///     Stops all MIDI Events on this Event.
		/// </summary>
		public void StopMIDI()
		{
			if (IsValid())
				AkSoundEngine.StopMIDIOnEvent(GetID());
		}
	}

	[System.Serializable]
	///@brief This type can be used to set game parameter values to the sound engine.
	public class RTPC : BaseType
	{
		public void SetValue(UnityEngine.GameObject gameObject, float value)
		{
			if (IsValid())
			{
				var result = AkSoundEngine.SetRTPCValue(GetID(), value, gameObject);
				Verify(result);
			}
		}

		public void SetGlobalValue(float value)
		{
			if (IsValid())
			{
				var result = AkSoundEngine.SetRTPCValue(GetID(), value);
				Verify(result);
			}
		}
	}

	[System.Serializable]
	///@brief This type can be used to post triggers to the sound engine.
	public class Trigger : BaseType
	{
		public void Post(UnityEngine.GameObject gameObject)
		{
			if (IsValid())
			{
				var result = AkSoundEngine.PostTrigger(GetID(), gameObject);
				Verify(result);
			}
		}
	}

	[System.Serializable]
	///@brief This type can be used to set Wwise States.
	public class State : BaseGroupType
	{
		public void SetValue()
		{
			if (IsValid())
			{
				var result = AkSoundEngine.SetState(GetGroupID(), GetID());
				Verify(result);
			}
		}
	}

	[System.Serializable]
	///@brief This type can be used to set Switch values on gameobjects.
	public class Switch : BaseGroupType
	{
		public void SetValue(UnityEngine.GameObject gameObject)
		{
			if (IsValid())
			{
				var result = AkSoundEngine.SetSwitch(GetGroupID(), GetID(), gameObject);
				Verify(result);
			}
		}
	}

	[System.Serializable]
	///@brief This type represents an auxiliary send in the Master-Mixer Hierarchy.
	public class AuxBus : BaseType
	{
	}

	[System.Serializable]
	///@brief This type can be used to load/unload SoundBanks.
	public class Bank : BaseType
	{
		public string name;

		public void Load(bool decodeBank = false, bool saveDecodedBank = false)
		{
			if (IsValid())
				AkBankManager.LoadBank(name, decodeBank, saveDecodedBank);
		}

		public void LoadAsync(AkCallbackManager.BankCallback callback = null)
		{
			if (IsValid())
				AkBankManager.LoadBankAsync(name, callback);
		}

		public void Unload()
		{
			if (IsValid())
				AkBankManager.UnloadBank(name);
		}

		public override bool IsValid()
		{
			return name.Length != 0 || base.IsValid();
		}
	}

	[System.Serializable]
	///@brief This type represents an Acoustic Texture.
	public class AcousticTexture : BaseType
	{
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.