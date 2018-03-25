#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
[UnityEngine.AddComponentMenu("Wwise/AkRoomPortal")]
[UnityEngine.RequireComponent(typeof(UnityEngine.BoxCollider))]
[UnityEngine.DisallowMultipleComponent]
/// @brief An AkRoomPortal can connect two AkRoom components together.
/// @details 
public class AkRoomPortal : AkUnityEventHandler
{
	/// AkRoomPortals can only connect a maximum of 2 rooms.
	public const int MAX_ROOMS_PER_PORTAL = 2;

	private readonly AkVector extent = new AkVector();

	private readonly AkTransform portalTransform = new AkTransform();

	private ulong backRoomID = AkRoom.INVALID_ROOM_ID;

	public System.Collections.Generic.List<int> closePortalTriggerList = new System.Collections.Generic.List<int>();
	private ulong frontRoomID = AkRoom.INVALID_ROOM_ID;

	/// The front and back rooms connected by the portal.
	/// The first room is on the negative side of the portal(opposite to the direction of the local Z axis)
	/// The second room is on the positive side of the portal.
	public AkRoom[] rooms = new AkRoom[MAX_ROOMS_PER_PORTAL];

	/// Access the portal's ID
	public ulong GetID()
	{
		return (ulong) GetInstanceID();
	}

	protected override void Awake()
	{
		var collider = GetComponent<UnityEngine.BoxCollider>();
		collider.isTrigger = true;

		portalTransform.Set(collider.bounds.center.x, collider.bounds.center.y, collider.bounds.center.z, transform.forward.x,
			transform.forward.y, transform.forward.z, transform.up.x, transform.up.y, transform.up.z);

		extent.X = collider.size.x * transform.localScale.x / 2;
		extent.Y = collider.size.y * transform.localScale.y / 2;
		extent.Z = collider.size.z * transform.localScale.z / 2;

		frontRoomID = rooms[1] == null ? AkRoom.INVALID_ROOM_ID : rooms[1].GetID();
		backRoomID = rooms[0] == null ? AkRoom.INVALID_ROOM_ID : rooms[0].GetID();

		RegisterTriggers(closePortalTriggerList, ClosePortal);

		base.Awake();

		//Call the ClosePortal function if registered to the Awake Trigger
		if (closePortalTriggerList.Contains(AWAKE_TRIGGER_ID))
			ClosePortal(null);
	}

	protected override void Start()
	{
		base.Start();

		//Call the ClosePortal function if registered to the Start Trigger
		if (closePortalTriggerList.Contains(START_TRIGGER_ID))
			ClosePortal(null);
	}

	/// Opens the portal on trigger event
	public override void HandleEvent(UnityEngine.GameObject in_gameObject)
	{
		Open();
	}

	/// Closes the portal on trigger event
	public void ClosePortal(UnityEngine.GameObject in_gameObject)
	{
		Close();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		UnregisterTriggers(closePortalTriggerList, ClosePortal);

		if (closePortalTriggerList.Contains(DESTROY_TRIGGER_ID))
			ClosePortal(null);
	}

	public void Open()
	{
		ActivatePortal(true);
	}

	public void Close()
	{
		ActivatePortal(false);
	}

	private void ActivatePortal(bool active)
	{
		if (!enabled)
			return;

		if (frontRoomID != backRoomID)
			AkSoundEngine.SetRoomPortal(GetID(), portalTransform, extent, active, frontRoomID, backRoomID);
		else
			UnityEngine.Debug.LogError(name + " is not placed/oriented correctly");
	}

	public void FindOverlappingRooms(AkRoom.PriorityList[] roomList)
	{
		var portalCollider = gameObject.GetComponent<UnityEngine.BoxCollider>();
		if (portalCollider == null)
			return;

		// compute halfExtents and divide the local z extent by 2
		var halfExtents = new UnityEngine.Vector3(portalCollider.size.x * transform.localScale.x / 2,
			portalCollider.size.y * transform.localScale.y / 2, portalCollider.size.z * transform.localScale.z / 4);

		// move the center backward
		FillRoomList(UnityEngine.Vector3.forward * -0.25f, halfExtents, roomList[0]);

		// move the center forward
		FillRoomList(UnityEngine.Vector3.forward * 0.25f, halfExtents, roomList[1]);
	}

	private void FillRoomList(UnityEngine.Vector3 center, UnityEngine.Vector3 halfExtents, AkRoom.PriorityList list)
	{
		list.rooms.Clear();

		center = transform.TransformPoint(center);

        var colliders = UnityEngine.Physics.OverlapBox(center, halfExtents, transform.rotation, -1, UnityEngine.QueryTriggerInteraction.Collide);

		foreach (var collider in colliders)
		{
			var room = collider.gameObject.GetComponent<AkRoom>();
			if (room != null && !list.Contains(room))
				list.Add(room);
		}
	}

	public void SetFrontRoom(AkRoom room)
	{
		rooms[1] = room;
		frontRoomID = rooms[1] == null ? AkRoom.INVALID_ROOM_ID : rooms[1].GetID();
	}

	public void SetBackRoom(AkRoom room)
	{
		rooms[0] = room;
		backRoomID = rooms[0] == null ? AkRoom.INVALID_ROOM_ID : rooms[0].GetID();
	}

	public void UpdateOverlappingRooms()
	{
		var roomList = new[] { new AkRoom.PriorityList(), new AkRoom.PriorityList() };

		FindOverlappingRooms(roomList);
		for (var i = 0; i < 2; i++)
		{
			if (!roomList[i].Contains(rooms[i]))
				rooms[i] = roomList[i].GetHighestPriorityRoom();
		}

		frontRoomID = rooms[1] == null ? AkRoom.INVALID_ROOM_ID : rooms[1].GetID();
		backRoomID = rooms[0] == null ? AkRoom.INVALID_ROOM_ID : rooms[0].GetID();
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (!enabled)
			return;

		UnityEngine.Gizmos.matrix = transform.localToWorldMatrix;

		var centreOffset = UnityEngine.Vector3.zero;
		var sizeMultiplier = UnityEngine.Vector3.one;
		var collider = GetComponent<UnityEngine.BoxCollider>();
		if (collider)
		{
			centreOffset = collider.center;
			sizeMultiplier = collider.size;
		}

		// color faces
		var faceCenterPos = new UnityEngine.Vector3[4];
		faceCenterPos[0] = UnityEngine.Vector3.Scale(new UnityEngine.Vector3(0.5f, 0.0f, 0.0f), sizeMultiplier);
		faceCenterPos[1] = UnityEngine.Vector3.Scale(new UnityEngine.Vector3(0.0f, 0.5f, 0.0f), sizeMultiplier);
		faceCenterPos[2] = UnityEngine.Vector3.Scale(new UnityEngine.Vector3(-0.5f, 0.0f, 0.0f), sizeMultiplier);
		faceCenterPos[3] = UnityEngine.Vector3.Scale(new UnityEngine.Vector3(0.0f, -0.5f, 0.0f), sizeMultiplier);

		var faceSize = new UnityEngine.Vector3[4];
		faceSize[0] = new UnityEngine.Vector3(0, 1, 1);
		faceSize[1] = new UnityEngine.Vector3(1, 0, 1);
		faceSize[2] = faceSize[0];
		faceSize[3] = faceSize[1];

		UnityEngine.Gizmos.color = new UnityEngine.Color32(255, 204, 0, 100);
		for (var i = 0; i < 4; i++)
			UnityEngine.Gizmos.DrawCube(faceCenterPos[i] + centreOffset, UnityEngine.Vector3.Scale(faceSize[i], sizeMultiplier));

		// draw line in the center of the portal
		var CornerCenterPos = faceCenterPos;
		CornerCenterPos[0].y += 0.5f * sizeMultiplier.y;
		CornerCenterPos[1].x -= 0.5f * sizeMultiplier.x;
		CornerCenterPos[2].y -= 0.5f * sizeMultiplier.y;
		CornerCenterPos[3].x += 0.5f * sizeMultiplier.x;

		UnityEngine.Gizmos.color = UnityEngine.Color.red;
		for (var i = 0; i < 4; i++)
			UnityEngine.Gizmos.DrawLine(CornerCenterPos[i] + centreOffset, CornerCenterPos[(i + 1) % 4] + centreOffset);
	}
#endif
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.