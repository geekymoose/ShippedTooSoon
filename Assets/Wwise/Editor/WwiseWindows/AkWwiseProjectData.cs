public class AkWwiseProjectData : UnityEngine.ScriptableObject
{
	public enum WwiseObjectType
	{
		// Insert Wwise icons description here
		NONE,
		AUXBUS,
		BUS,
		EVENT,
		FOLDER,
		PHYSICALFOLDER,
		PROJECT,
		SOUNDBANK,
		STATE,
		STATEGROUP,
		SWITCH,
		SWITCHGROUP,
		WORKUNIT,
		GAMEPARAMETER,
		TRIGGER,
		ACOUSTICTEXTURE
	}

	//An IComparer that enables us to sort work units by their physical path 
	public static WorkUnit_CompareByPhysicalPath s_compareByPhysicalPath = new WorkUnit_CompareByPhysicalPath();

	//An IComparer that enables us to sort AkInformations by their physical name
	public static AkInformation_CompareByName s_compareAkInformationByName = new AkInformation_CompareByName();

	public System.Collections.Generic.List<AkInfoWorkUnit> AcousticTextureWwu =
		new System.Collections.Generic.List<AkInfoWorkUnit>();

	public bool autoPopulateEnabled = true;

	public System.Collections.Generic.List<AkInfoWorkUnit> AuxBusWwu =
		new System.Collections.Generic.List<AkInfoWorkUnit>();

	public System.Collections.Generic.List<AkInfoWorkUnit> BankWwu = new System.Collections.Generic.List<AkInfoWorkUnit>();

	//This data is a copy of the AkInitializer parameters.  
	//We need it to reapply the same values to copies of the object in different scenes
	//It sits in this object so it is serialized in the same "asset" file
	public string basePath = AkSoundEngineController.s_DefaultBasePath;
	public int callbackManagerBufferSize = AkSoundEngineController.s_CallbackManagerBufferSize;

	public string CurrentPluginConfig;
	public int defaultPoolSize = AkSoundEngineController.s_DefaultPoolSize;

	//Can't use a list of WorkUnit and cast it when needed because unity will serialize it as 
	//Workunit and all the child class's fields will be deleted
	public System.Collections.Generic.List<EventWorkUnit> EventWwu = new System.Collections.Generic.List<EventWorkUnit>();

	//Contains the path of all items that are expanded in the Wwise picker
	public System.Collections.Generic.List<string> ExpandedItems = new System.Collections.Generic.List<string>();
	public string language = AkSoundEngineController.s_Language;
	public int lowerPoolSize = AkSoundEngineController.s_LowerPoolSize;
	[UnityEngine.SerializeField] private int m_lastPopulateTimePart2;

	//DateTime Objects are not serializable, so we have to use its binary format (64 bit long).
	//But apparently long isn't serializable neither, so we split it into two int
	[UnityEngine.SerializeField] private int m_lastPopulateTimePsrt1;
	public float memoryCutoffThreshold = AkSoundEngineController.s_MemoryCutoffThreshold;
	public int preparePoolSize = AkSoundEngineController.s_PreparePoolSize;
	public System.Collections.Generic.List<AkInfoWorkUnit> RtpcWwu = new System.Collections.Generic.List<AkInfoWorkUnit>();

	public System.Collections.Generic.List<GroupValWorkUnit> StateWwu =
		new System.Collections.Generic.List<GroupValWorkUnit>();

	public int streamingPoolSize = AkSoundEngineController.s_StreamingPoolSize;

	public System.Collections.Generic.List<GroupValWorkUnit> SwitchWwu =
		new System.Collections.Generic.List<GroupValWorkUnit>();

	public System.Collections.Generic.List<AkInfoWorkUnit> TriggerWwu =
		new System.Collections.Generic.List<AkInfoWorkUnit>();

	public System.Collections.ArrayList GetWwuListByString(string in_wwuType)
	{
		if (string.Equals(in_wwuType, "Events", System.StringComparison.OrdinalIgnoreCase))
			return System.Collections.ArrayList.Adapter(EventWwu);
		if (string.Equals(in_wwuType, "States", System.StringComparison.OrdinalIgnoreCase))
			return System.Collections.ArrayList.Adapter(StateWwu);
		if (string.Equals(in_wwuType, "Switches", System.StringComparison.OrdinalIgnoreCase))
			return System.Collections.ArrayList.Adapter(SwitchWwu);
		if (string.Equals(in_wwuType, "Master-Mixer Hierarchy", System.StringComparison.OrdinalIgnoreCase))
			return System.Collections.ArrayList.Adapter(AuxBusWwu);
		if (string.Equals(in_wwuType, "SoundBanks", System.StringComparison.OrdinalIgnoreCase))
			return System.Collections.ArrayList.Adapter(BankWwu);
		if (string.Equals(in_wwuType, "Game Parameters", System.StringComparison.OrdinalIgnoreCase))
			return System.Collections.ArrayList.Adapter(RtpcWwu);
		if (string.Equals(in_wwuType, "Triggers", System.StringComparison.OrdinalIgnoreCase))
			return System.Collections.ArrayList.Adapter(TriggerWwu);
		if (string.Equals(in_wwuType, "Virtual Acoustics", System.StringComparison.OrdinalIgnoreCase))
			return System.Collections.ArrayList.Adapter(AcousticTextureWwu);

		return null;
	}

	public WorkUnit NewChildWorkUnit(string in_wwuType)
	{
		if (string.Equals(in_wwuType, "Events", System.StringComparison.OrdinalIgnoreCase))
			return new EventWorkUnit();
		if (string.Equals(in_wwuType, "States", System.StringComparison.OrdinalIgnoreCase) ||
		    string.Equals(in_wwuType, "Switches", System.StringComparison.OrdinalIgnoreCase))
			return new GroupValWorkUnit();
		if (string.Equals(in_wwuType, "Master-Mixer Hierarchy", System.StringComparison.OrdinalIgnoreCase) ||
		    string.Equals(in_wwuType, "SoundBanks", System.StringComparison.OrdinalIgnoreCase) ||
		    string.Equals(in_wwuType, "Game Parameters", System.StringComparison.OrdinalIgnoreCase) ||
		    string.Equals(in_wwuType, "Virtual Acoustics", System.StringComparison.OrdinalIgnoreCase) ||
		    string.Equals(in_wwuType, "Triggers", System.StringComparison.OrdinalIgnoreCase))
			return new AkInfoWorkUnit();

		return null;
	}

	public bool IsSupportedWwuType(string in_wwuType)
	{
		if (string.Equals(in_wwuType, "Events", System.StringComparison.OrdinalIgnoreCase))
			return true;
		if (string.Equals(in_wwuType, "States", System.StringComparison.OrdinalIgnoreCase))
			return true;
		if (string.Equals(in_wwuType, "Switches", System.StringComparison.OrdinalIgnoreCase))
			return true;
		if (string.Equals(in_wwuType, "Master-Mixer Hierarchy", System.StringComparison.OrdinalIgnoreCase))
			return true;
		if (string.Equals(in_wwuType, "SoundBanks", System.StringComparison.OrdinalIgnoreCase))
			return true;
		if (string.Equals(in_wwuType, "Game Parameters", System.StringComparison.OrdinalIgnoreCase))
			return true;
		if (string.Equals(in_wwuType, "Triggers", System.StringComparison.OrdinalIgnoreCase))
			return true;
		if (string.Equals(in_wwuType, "Virtual Acoustics", System.StringComparison.OrdinalIgnoreCase))
			return true;

		return false;
	}

	public byte[] GetEventGuidById(int in_ID)
	{
		for (var i = 0; i < AkWwiseProjectInfo.GetData().EventWwu.Count; i++)
		{
			var e = AkWwiseProjectInfo.GetData().EventWwu[i].List.Find(x => x.ID == in_ID);
			if (e != null)
				return e.Guid;
		}

		return null;
	}

	public byte[] GetBankGuidByName(string in_name)
	{
		for (var i = 0; i < AkWwiseProjectInfo.GetData().BankWwu.Count; i++)
		{
			var bank = AkWwiseProjectInfo.GetData().BankWwu[i].List.Find(x => x.Name.Equals(in_name));
			if (bank != null)
				return bank.Guid;
		}

		return null;
	}

	public byte[] GetEnvironmentGuidByName(string in_name)
	{
		for (var i = 0; i < AkWwiseProjectInfo.GetData().AuxBusWwu.Count; i++)
		{
			var auxBus = AkWwiseProjectInfo.GetData().AuxBusWwu[i].List.Find(x => x.Name.Equals(in_name));
			if (auxBus != null)
				return auxBus.Guid;
		}

		return null;
	}

	public byte[][] GetStateGuidByName(string in_groupName, string in_valueName)
	{
		for (var i = 0; i < AkWwiseProjectInfo.GetData().StateWwu.Count; i++)
		{
			var stateGroup = AkWwiseProjectInfo.GetData().StateWwu[i].List.Find(x => x.Name.Equals(in_groupName));
			if (stateGroup == null)
				continue;

			var index = stateGroup.values.FindIndex(x => x == in_valueName);
			return new[] { stateGroup.Guid, stateGroup.ValueGuids[index].bytes };
		}

		return null;
	}

	public byte[][] GetSwitchGuidByName(string in_groupName, string in_valueName)
	{
		for (var i = 0; i < AkWwiseProjectInfo.GetData().SwitchWwu.Count; i++)
		{
			var switchGroup = AkWwiseProjectInfo.GetData().SwitchWwu[i].List.Find(x => x.Name.Equals(in_groupName));
			if (switchGroup == null)
				continue;

			var index = switchGroup.values.FindIndex(x => x == in_valueName);
			return new[] { switchGroup.Guid, switchGroup.ValueGuids[index].bytes };
		}

		return null;
	}

	public void SetLastPopulateTime(System.DateTime in_time)
	{
		var timeBin = in_time.ToBinary();

		m_lastPopulateTimePsrt1 = (int) timeBin;
		m_lastPopulateTimePart2 = (int) (timeBin >> 32);
	}

	public System.DateTime GetLastPopulateTime()
	{
		var timeBin = (long) m_lastPopulateTimePart2;
		timeBin <<= 32;
		timeBin |= (uint) m_lastPopulateTimePsrt1;

		return System.DateTime.FromBinary(timeBin);
	}

	public void SaveInitSettings(AkInitializer in_AkInit)
	{
		if (!AkWwisePicker.WwiseProjectFound)
			return;
		if (!CompareInitSettings(in_AkInit))
		{
			UnityEditor.Undo.RecordObject(this, "Save Init Settings");

			basePath = in_AkInit.basePath;
			language = in_AkInit.language;
			defaultPoolSize = in_AkInit.defaultPoolSize;
			lowerPoolSize = in_AkInit.lowerPoolSize;
			streamingPoolSize = in_AkInit.streamingPoolSize;
			preparePoolSize = in_AkInit.preparePoolSize;
			memoryCutoffThreshold = in_AkInit.memoryCutoffThreshold;
			callbackManagerBufferSize = in_AkInit.callbackManagerBufferSize;
		}
	}

	public void CopyInitSettings(AkInitializer in_AkInit)
	{
		if (!CompareInitSettings(in_AkInit))
		{
			UnityEditor.Undo.RecordObject(in_AkInit, "Copy Init Settings");

			in_AkInit.basePath = basePath;
			in_AkInit.language = language;
			in_AkInit.defaultPoolSize = defaultPoolSize;
			in_AkInit.lowerPoolSize = lowerPoolSize;
			in_AkInit.streamingPoolSize = streamingPoolSize;
			in_AkInit.preparePoolSize = preparePoolSize;
			in_AkInit.memoryCutoffThreshold = memoryCutoffThreshold;
			in_AkInit.callbackManagerBufferSize = callbackManagerBufferSize;
		}
	}

	private bool CompareInitSettings(AkInitializer in_AkInit)
	{
		return basePath == in_AkInit.basePath && language == in_AkInit.language &&
		       defaultPoolSize == in_AkInit.defaultPoolSize && lowerPoolSize == in_AkInit.lowerPoolSize &&
		       streamingPoolSize == in_AkInit.streamingPoolSize && preparePoolSize == in_AkInit.preparePoolSize &&
		       memoryCutoffThreshold == in_AkInit.memoryCutoffThreshold &&
		       callbackManagerBufferSize == in_AkInit.callbackManagerBufferSize;
	}

	public float GetEventMaxAttenuation(int in_eventID)
	{
		for (var i = 0; i < EventWwu.Count; i++)
		{
			for (var j = 0; j < EventWwu[i].List.Count; j++)
			{
				if (EventWwu[i].List[j].ID.Equals(in_eventID))
					return EventWwu[i].List[j].maxAttenuation;
			}
		}

		return 0.0f;
	}

	public void Reset()
	{
		EventWwu = new System.Collections.Generic.List<EventWorkUnit>();
		StateWwu = new System.Collections.Generic.List<GroupValWorkUnit>();
		SwitchWwu = new System.Collections.Generic.List<GroupValWorkUnit>();
		BankWwu = new System.Collections.Generic.List<AkInfoWorkUnit>();
		AuxBusWwu = new System.Collections.Generic.List<AkInfoWorkUnit>();
		RtpcWwu = new System.Collections.Generic.List<AkInfoWorkUnit>();
		TriggerWwu = new System.Collections.Generic.List<AkInfoWorkUnit>();
		AcousticTextureWwu = new System.Collections.Generic.List<AkInfoWorkUnit>();
	}

	[System.Serializable]
	public class ByteArrayWrapper
	{
		public byte[] bytes;

		public ByteArrayWrapper(byte[] byteArray)
		{
			bytes = byteArray;
		}
	}

	[System.Serializable]
	public class AkInformation
	{
		public byte[] Guid = null;
		public int ID;
		public string Name;
		public string Path;
		public System.Collections.Generic.List<PathElement> PathAndIcons = new System.Collections.Generic.List<PathElement>();
	}

	[System.Serializable]
	public class GroupValue : AkInformation
	{
		//Unity can't serialize a list of arrays. So we create a serializable wrapper class for our array 
		public System.Collections.Generic.List<ByteArrayWrapper> ValueGuids =
			new System.Collections.Generic.List<ByteArrayWrapper>();

		public System.Collections.Generic.List<PathElement> ValueIcons = new System.Collections.Generic.List<PathElement>();
		public System.Collections.Generic.List<int> valueIDs = new System.Collections.Generic.List<int>();
		public System.Collections.Generic.List<string> values = new System.Collections.Generic.List<string>();
	}

	[System.Serializable]
	public class Event : AkInformation
	{
		public float maxAttenuation;
		public float maxDuration;
		public float minDuration;
	}

	[System.Serializable]
	public class WorkUnit : System.IComparable
	{
		public string Guid;

		[UnityEngine.SerializeField] private int m_lastTimePart2;

		//DateTime Objects are not serializable, so we have to use its binary format (64 bit long).
		//But apparently long isn't serializable neither, so we split it into two int
		[UnityEngine.SerializeField] private int m_lastTimePsrt1;

		public string ParentPhysicalPath;

		public string PhysicalPath;

		public WorkUnit()
		{
		}

		public WorkUnit(string in_physicalPath)
		{
			PhysicalPath = in_physicalPath;
		}

		public int CompareTo(object other)
		{
			var otherWwu = other as WorkUnit;

			return PhysicalPath.CompareTo(otherWwu.PhysicalPath);
		}

		public void SetLastTime(System.DateTime in_time)
		{
			var timeBin = in_time.ToBinary();

			m_lastTimePsrt1 = (int) timeBin;
			m_lastTimePart2 = (int) (timeBin >> 32);
		}

		public System.DateTime GetLastTime()
		{
			var timeBin = (long) m_lastTimePart2;
			timeBin <<= 32;
			timeBin |= (uint) m_lastTimePsrt1;

			return System.DateTime.FromBinary(timeBin);
		}
	}

	public class WorkUnit_CompareByPhysicalPath : System.Collections.IComparer
	{
		int System.Collections.IComparer.Compare(object a, object b)
		{
			var wwuA = a as WorkUnit;
			var wwuB = b as WorkUnit;

			return wwuA.PhysicalPath.CompareTo(wwuB.PhysicalPath);
		}
	}

	public class AkInformation_CompareByName : System.Collections.IComparer
	{
		int System.Collections.IComparer.Compare(object a, object b)
		{
			var AkInfA = a as AkInformation;
			var AkInfB = b as AkInformation;

			return AkInfA.Name.CompareTo(AkInfB.Name);
		}
	}

	[System.Serializable]
	public class EventWorkUnit : WorkUnit
	{
		public System.Collections.Generic.List<Event> List = new System.Collections.Generic.List<Event>();
	}


	[System.Serializable]
	public class AkInfoWorkUnit : WorkUnit
	{
		public System.Collections.Generic.List<AkInformation> List = new System.Collections.Generic.List<AkInformation>();
	}

	[System.Serializable]
	public class GroupValWorkUnit : WorkUnit
	{
		public System.Collections.Generic.List<GroupValue> List = new System.Collections.Generic.List<GroupValue>();
	}

	[System.Serializable]
	public class PathElement
	{
		public string ElementName;
		public WwiseObjectType ObjectType;

		public PathElement(string Name, WwiseObjectType objType)
		{
			ElementName = Name;
			ObjectType = objType;
		}
	}
}