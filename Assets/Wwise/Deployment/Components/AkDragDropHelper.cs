#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
#if UNITY_EDITOR
public class AkDragDropData
{
	public System.Guid guid;
	public int ID;
	public string name;
	public string typeName;
}

public class AkDragDropGroupData : AkDragDropData
{
	public System.Guid groupGuid;
	public int groupID;
}


/// <summary>
///     @brief This class is used to perform DragAndDrop operations from the AkWwisePicker to any GameObject.
///     We found out that DragAndDrop operations in Unity do not transfer components, but only scripts. This
///     prevented us to set the name and ID of our components before performing the drag and drop. To fix this,
///     the DragAndDrop operation always transfers a AkDragDropHelper component that gets instantiated on the
///     target GameObject. On its first Update() call, it will parse the DragAndDrop structure, which contains
///     all necessary information to instantiate the correct component, with the correct information
/// </summary>
[UnityEngine.ExecuteInEditMode]
public class AkDragDropHelper : UnityEngine.MonoBehaviour
{
	public static string DragDropIdentifier = "AKWwiseDDInfo";

	private void Awake()
	{
		var DDData = UnityEditor.DragAndDrop.GetGenericData(DragDropIdentifier) as AkDragDropData;
		var DDGroupData = DDData as AkDragDropGroupData;

		if (DDGroupData != null)
		{
			switch (DDData.typeName)
			{
				case "State":
					CreateState(DDGroupData);
					break;
				case "Switch":
					CreateSwitch(DDGroupData);
					break;
			}
		}
		else if (DDData != null)
		{
			switch (DDData.typeName)
			{
				case "AuxBus":
					CreateAuxBus(DDData);
					break;
				case "Event":
					CreateAmbient(DDData);
					break;
				case "Bank":
					CreateBank(DDData);
					break;
			}
		}

		UnityEngine.GUIUtility.hotControl = 0;
	}

	private void Start()
	{
		// Don't forget to destroy the AkDragDropHelper when we're done!
		DestroyImmediate(this);
	}

	private bool HasSameEnvironment(System.Guid auxBusGuid)
	{
		var akEnvironments = gameObject.GetComponents<AkEnvironment>();
		for (var i = 0; i < akEnvironments.Length; i++)
		{
			if (new System.Guid(akEnvironments[i].valueGuid).Equals(auxBusGuid))
				return true;
		}

		return false;
	}

	private void CreateAuxBus(AkDragDropData DDData)
	{
		if (HasSameEnvironment(DDData.guid))
			return;

		var akEnvironment = UnityEditor.Undo.AddComponent<AkEnvironment>(gameObject);
		if (akEnvironment != null)
			SetTypeValue(ref akEnvironment.valueGuid, ref akEnvironment.m_auxBusID, DDData);
	}

	private void CreateAmbient(AkDragDropData DDData)
	{
		var ambient = UnityEditor.Undo.AddComponent<AkAmbient>(gameObject);
		if (ambient != null)
			SetTypeValue(ref ambient.valueGuid, ref ambient.eventID, DDData);
	}

	private void CreateBank(AkDragDropData DDData)
	{
		var bank = UnityEditor.Undo.AddComponent<AkBank>(gameObject);
		if (bank != null)
		{
			var valueID = 0;
			SetTypeValue(ref bank.valueGuid, ref valueID, DDData);
			bank.bankName = DDData.name;
		}
	}

	private void CreateState(AkDragDropGroupData DDGroupData)
	{
		var akState = UnityEditor.Undo.AddComponent<AkState>(gameObject);
		if (akState != null)
		{
			SetTypeValue(ref akState.valueGuid, ref akState.valueID, DDGroupData);
			SetGroupTypeValue(ref akState.groupGuid, ref akState.groupID, DDGroupData);
		}
	}

	private void CreateSwitch(AkDragDropGroupData DDGroupData)
	{
		var akSwitch = UnityEditor.Undo.AddComponent<AkSwitch>(gameObject);
		if (akSwitch != null)
		{
			SetTypeValue(ref akSwitch.valueGuid, ref akSwitch.valueID, DDGroupData);
			SetGroupTypeValue(ref akSwitch.groupGuid, ref akSwitch.groupID, DDGroupData);
		}
	}

	private void SetTypeValue(ref byte[] valueGuid, ref int ID, AkDragDropData DDData)
	{
		DDData.guid.ToByteArray().CopyTo(valueGuid, 0);
		ID = DDData.ID;
	}

	private void SetGroupTypeValue(ref byte[] groupGuid, ref int groupID, AkDragDropGroupData DDGroupData)
	{
		DDGroupData.groupGuid.ToByteArray().CopyTo(groupGuid, 0);
		groupID = DDGroupData.groupID;
	}
}
#endif // UNITY_EDITOR
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.