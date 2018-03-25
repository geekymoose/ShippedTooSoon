#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
[UnityEngine.AddComponentMenu("Wwise/AkRoom")]
[UnityEngine.RequireComponent(typeof(UnityEngine.Collider))]
[UnityEngine.DisallowMultipleComponent]
/// @brief An AkRoom is an enclosed environment that can only communicate to the outside/other rooms with AkRoomPortals
/// @details 
public class AkRoom : UnityEngine.MonoBehaviour
{
	public static ulong INVALID_ROOM_ID = unchecked((ulong) -1.0f);

	private static int RoomCount;

	[UnityEngine.Tooltip("Higher number has a higher priority")]
	/// In cases where a game object is in an area with two rooms, the higher priority room will be chosen for AK::SpatialAudio::SetGameObjectInRoom()
	/// The higher the priority number, the higher the priority of a room.
	public int priority = 0;

	/// The reverb auxiliary bus.
	public AK.Wwise.AuxBus reverbAuxBus;

	[UnityEngine.Range(0, 1)]
	/// The reverb control value for the send to the reverb aux bus.
	public float reverbLevel = 1;

	[UnityEngine.Range(0, 1)]
	/// Occlusion level modeling transmission through walls.
	public float wallOcclusion = 1;

	public static bool IsSpatialAudioEnabled
	{
		get { return AkSpatialAudioListener.TheSpatialAudioListener != null && RoomCount > 0; }
	}

	/// Access the room's ID
	public ulong GetID()
	{
		return (ulong) GetInstanceID();
	}

	private void OnEnable()
	{
		var roomParams = new AkRoomParams();

		roomParams.Up.X = transform.up.x;
		roomParams.Up.Y = transform.up.y;
		roomParams.Up.Z = transform.up.z;

		roomParams.Front.X = transform.forward.x;
		roomParams.Front.Y = transform.forward.y;
		roomParams.Front.Z = transform.forward.z;

		roomParams.ReverbAuxBus = (uint) reverbAuxBus.ID;
		roomParams.ReverbLevel = reverbLevel;
		roomParams.WallOcclusion = wallOcclusion;

		RoomCount++;
		AkSoundEngine.SetRoom(GetID(), roomParams, name);
	}

	private void OnDisable()
	{
		RoomCount--;
		AkSoundEngine.RemoveRoom(GetID());
	}

	private void OnTriggerEnter(UnityEngine.Collider in_other)
	{
		var spatialAudioObjects = in_other.GetComponentsInChildren<AkSpatialAudioBase>();
		for (var i = 0; i < spatialAudioObjects.Length; i++)
		{
			if (spatialAudioObjects[i].enabled)
				spatialAudioObjects[i].EnteredRoom(this);
		}
	}

	private void OnTriggerExit(UnityEngine.Collider in_other)
	{
		var spatialAudioObjects = in_other.GetComponentsInChildren<AkSpatialAudioBase>();
		for (var i = 0; i < spatialAudioObjects.Length; i++)
		{
			if (spatialAudioObjects[i].enabled)
				spatialAudioObjects[i].ExitedRoom(this);
		}
	}

	public class PriorityList
	{
		private static readonly CompareByPriority s_compareByPriority = new CompareByPriority();

		/// Contains all active rooms sorted by priority.
		public System.Collections.Generic.List<AkRoom> rooms = new System.Collections.Generic.List<AkRoom>();

		public ulong GetHighestPriorityRoomID()
		{
			var room = GetHighestPriorityRoom();
			return room == null ? INVALID_ROOM_ID : room.GetID();
		}

		public AkRoom GetHighestPriorityRoom()
		{
			if (rooms.Count == 0)
				return null;

			return rooms[0];
		}

		public void Add(AkRoom room)
		{
			var index = BinarySearch(room);
			if (index < 0)
				rooms.Insert(~index, room);
		}

		public void Remove(AkRoom room)
		{
			rooms.Remove(room);
		}

		public bool Contains(AkRoom room)
		{
			return BinarySearch(room) >= 0;
		}

		public int BinarySearch(AkRoom room)
		{
			return rooms.BinarySearch(room, s_compareByPriority);
		}

		private class CompareByPriority : System.Collections.Generic.IComparer<AkRoom>
		{
			public virtual int Compare(AkRoom a, AkRoom b)
			{
				var result = a.priority.CompareTo(b.priority);

				if (result == 0 && a != b)
					return 1;

				return -result; // inverted to have highest priority first
			}
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.