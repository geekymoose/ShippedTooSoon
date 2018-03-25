#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

public class AkWwiseTreeView : AK.Wwise.TreeView.TreeViewControl
{
	public AK.Wwise.TreeView.TreeViewItem LastDoubleClickedItem;

	private UnityEngine.GUIStyle m_filterBoxStyle;
	private UnityEngine.GUIStyle m_filterBoxCancelButtonStyle;
	private string m_filterString = string.Empty;
	private static UnityEditor.MonoScript DragDropHelperMonoScript;

#if UNITY_2017_2_OR_NEWER
	private void SaveExpansionStatusBeforePlay(UnityEditor.PlayModeStateChange playMode)
	{
		if (playMode == UnityEditor.PlayModeStateChange.ExitingEditMode)
			SaveExpansionStatus();
	}
#else
	private void SaveExpansionStatusBeforePlay()
	{
		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !UnityEditor.EditorApplication.isPlaying)
			SaveExpansionStatus();
	}
#endif

	public AkWwiseTreeView()
	{
#if UNITY_2017_2_OR_NEWER
		UnityEditor.EditorApplication.playModeStateChanged += SaveExpansionStatusBeforePlay;
#else
		UnityEditor.EditorApplication.playmodeStateChanged += SaveExpansionStatusBeforePlay;
#endif
	}

	public class AkTreeInfo
	{
		public byte[] Guid = new byte[16];
		public int ID;
		public AkWwiseProjectData.WwiseObjectType ObjectType;

		public AkTreeInfo(int id, AkWwiseProjectData.WwiseObjectType objType)
		{
			ID = id;
			ObjectType = objType;
		}

		public AkTreeInfo(int id, byte[] guid, AkWwiseProjectData.WwiseObjectType objType)
		{
			ID = id;
			ObjectType = objType;
			Guid = guid;
		}
	}

	private AK.Wwise.TreeView.TreeViewItem AddPathToTreeItem(AK.Wwise.TreeView.TreeViewItem item,
		AkWwiseProjectData.AkInformation AkInfo)
	{
		var parentItem = item;

		var path = "/" + RootItem.Header + "/" + item.Header;

		for (var i = 0; i < AkInfo.PathAndIcons.Count; i++)
		{
			var PathElem = AkInfo.PathAndIcons[i];
			var childItem = parentItem.FindItemByName(PathElem.ElementName);

			path = path + "/" + PathElem.ElementName;

			if (childItem == null)
			{
				if (i != AkInfo.PathAndIcons.Count - 1)
					childItem = parentItem.AddItem(PathElem.ElementName,
						new AkTreeInfo(0, System.Guid.Empty.ToByteArray(), PathElem.ObjectType), GetExpansionStatus(path));
				else
				{
					var isDraggable = !(PathElem.ObjectType == AkWwiseProjectData.WwiseObjectType.STATEGROUP ||
					                    PathElem.ObjectType == AkWwiseProjectData.WwiseObjectType.SWITCHGROUP);
					childItem = parentItem.AddItem(PathElem.ElementName, isDraggable, GetExpansionStatus(path),
						new AkTreeInfo(AkInfo.ID, AkInfo.Guid, PathElem.ObjectType));
				}
			}

			AddHandlerEvents(childItem);
			parentItem = childItem;
		}

		return parentItem;
	}

	public void SetRootItem(string Header, AkWwiseProjectData.WwiseObjectType ObjType)
	{
		RootItem.Items.Clear();
		RootItem.Header = Header;
		RootItem.DataContext = new AkTreeInfo(0, ObjType);
		AddHandlerEvents(RootItem);

		RootItem.IsExpanded = GetExpansionStatus("/" + RootItem.Header);
	}

	public void PopulateItem(AK.Wwise.TreeView.TreeViewItem attachTo, string itemName,
		System.Collections.Generic.List<AkWwiseProjectData.AkInfoWorkUnit> workUnits)
	{
		var attachPoint = attachTo.AddItem(itemName, false, GetExpansionStatus("/" + RootItem.Header + "/" + itemName),
			new AkTreeInfo(0, AkWwiseProjectData.WwiseObjectType.PHYSICALFOLDER));

		foreach (var wwu in workUnits)
		{
			foreach (var akInfo in wwu.List)
			{
				AddHandlerEvents(AddPathToTreeItem(attachPoint, akInfo));
			}
		}

		AddHandlerEvents(attachPoint);
	}

	public void PopulateItem(AK.Wwise.TreeView.TreeViewItem attachTo, string itemName,
		System.Collections.Generic.List<AkWwiseProjectData.EventWorkUnit> Events)
	{
		var akInfoWwu = new System.Collections.Generic.List<AkWwiseProjectData.AkInfoWorkUnit>(Events.Count);
		for (var i = 0; i < Events.Count; i++)
		{
			akInfoWwu.Add(new AkWwiseProjectData.AkInfoWorkUnit());
			akInfoWwu[i].PhysicalPath = Events[i].PhysicalPath;
			akInfoWwu[i].ParentPhysicalPath = Events[i].ParentPhysicalPath;
			akInfoWwu[i].Guid = Events[i].Guid;
			akInfoWwu[i].List = Events[i].List.ConvertAll(x => (AkWwiseProjectData.AkInformation) x);
		}

		PopulateItem(attachTo, itemName, akInfoWwu);
	}


	public void PopulateItem(AK.Wwise.TreeView.TreeViewItem attachTo, string itemName,
		System.Collections.Generic.List<AkWwiseProjectData.GroupValWorkUnit> GroupWorkUnits)
	{
		var attachPoint = attachTo.AddItem(itemName, false, GetExpansionStatus("/" + RootItem.Header + "/" + itemName),
			new AkTreeInfo(0, AkWwiseProjectData.WwiseObjectType.PHYSICALFOLDER));

		foreach (var wwu in GroupWorkUnits)
		{
			foreach (var group in wwu.List)
			{
				var groupItem = AddPathToTreeItem(attachPoint, group);
				AddHandlerEvents(groupItem);

				for (var i = 0; i < group.values.Count; i++)
				{
					var item = groupItem.AddItem(group.values[i], true, false,
						new AkTreeInfo(group.valueIDs[i], group.ValueGuids[i].bytes, group.ValueIcons[i].ObjectType));
					AddHandlerEvents(item);
				}
			}
		}

		AddHandlerEvents(attachPoint);
	}

	/// <summary>
	///     Handler functions for TreeViewControl
	/// </summary>
	private void AddHandlerEvents(AK.Wwise.TreeView.TreeViewItem item)
	{
		// Uncomment this when we support right click
		item.Click = HandleClick;
		item.Dragged = PrepareDragDrop;
		item.CustomIconBuilder = CustomIconHandler;
	}

	private void HandleClick(object sender, System.EventArgs args)
	{
		if (UnityEngine.Event.current.button == 0)
		{
			if ((args as AK.Wwise.TreeView.TreeViewItem.ClickEventArgs).m_clickCount == 2)
			{
				LastDoubleClickedItem = (AK.Wwise.TreeView.TreeViewItem) sender;

				if (LastDoubleClickedItem.HasChildItems())
					LastDoubleClickedItem.IsExpanded = !LastDoubleClickedItem.IsExpanded;
			}
		}
	}

	private void PrepareDragDrop(object sender, System.EventArgs args)
	{
		var item = (AK.Wwise.TreeView.TreeViewItem) sender;
		try
		{
			if (item == null || !item.IsDraggable)
				return;

			var objectReferences = new UnityEngine.Object[1];
			var treeInfo = (AkTreeInfo) item.DataContext;

			AkDragDropData DDData = null;

			var objType = GetObjectType(treeInfo.ObjectType);
			if (objType == "State" || objType == "Switch")
			{
				var DDGroupData = new AkDragDropGroupData();
				var ParentTreeInfo = (AkTreeInfo) item.Parent.DataContext;
				DDGroupData.groupGuid = new System.Guid(ParentTreeInfo.Guid);
				DDGroupData.groupID = ParentTreeInfo.ID;
				DDData = DDGroupData;
			}
			else
				DDData = new AkDragDropData();

			DDData.name = item.Header;
			DDData.guid = new System.Guid(treeInfo.Guid);
			DDData.ID = treeInfo.ID;
			DDData.typeName = objType;

			objectReferences[0] = DragDropHelperMonoScript;
			UnityEngine.GUIUtility.hotControl = 0;
			UnityEditor.DragAndDrop.objectReferences = objectReferences;
			UnityEditor.DragAndDrop.SetGenericData(AkDragDropHelper.DragDropIdentifier, DDData);
			UnityEditor.DragAndDrop.StartDrag("Dragging an AkObject");
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.Log(e.ToString());
		}
	}

	private static string GetObjectType(AkWwiseProjectData.WwiseObjectType item)
	{
		switch (item)
		{
			case AkWwiseProjectData.WwiseObjectType.AUXBUS:
				return "AuxBus";
			case AkWwiseProjectData.WwiseObjectType.EVENT:
				return "Event";
			case AkWwiseProjectData.WwiseObjectType.SOUNDBANK:
				return "Bank";
			case AkWwiseProjectData.WwiseObjectType.STATE:
				return "State";
			case AkWwiseProjectData.WwiseObjectType.SWITCH:
				return "Switch";
			case AkWwiseProjectData.WwiseObjectType.GAMEPARAMETER:
				return "GameParameter";
			case AkWwiseProjectData.WwiseObjectType.ACOUSTICTEXTURE:
				return "AcousticTexture";
			default:
				return "undefined";
		}
	}

	private void ShowButtonTextureInternal(UnityEngine.Texture2D texture)
	{
		if (null == texture || m_forceButtonText)
			UnityEngine.GUILayout.Button("", UnityEngine.GUILayout.MaxWidth(16));
		else
			ShowButtonTexture(texture);
	}

	public void CustomIconHandler(object sender, System.EventArgs args)
	{
		var item = (AK.Wwise.TreeView.TreeViewItem) sender;
		var treeInfo = (AkTreeInfo) item.DataContext;
		switch (treeInfo.ObjectType)
		{
			case AkWwiseProjectData.WwiseObjectType.AUXBUS:
				ShowButtonTextureInternal(m_textureWwiseAuxBusIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.BUS:
				ShowButtonTextureInternal(m_textureWwiseBusIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.EVENT:
			case AkWwiseProjectData.WwiseObjectType.GAMEPARAMETER:
			case AkWwiseProjectData.WwiseObjectType.ACOUSTICTEXTURE:
				ShowButtonTextureInternal(m_textureWwiseEventIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.FOLDER:
				ShowButtonTextureInternal(m_textureWwiseFolderIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.PHYSICALFOLDER:
				ShowButtonTextureInternal(m_textureWwisePhysicalFolderIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.PROJECT:
				ShowButtonTextureInternal(m_textureWwiseProjectIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.SOUNDBANK:
				ShowButtonTextureInternal(m_textureWwiseSoundbankIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.STATE:
				ShowButtonTextureInternal(m_textureWwiseStateIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.STATEGROUP:
				ShowButtonTextureInternal(m_textureWwiseStateGroupIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.SWITCH:
				ShowButtonTextureInternal(m_textureWwiseSwitchIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.SWITCHGROUP:
				ShowButtonTextureInternal(m_textureWwiseSwitchGroupIcon);
				break;
			case AkWwiseProjectData.WwiseObjectType.WORKUNIT:
				ShowButtonTextureInternal(m_textureWwiseWorkUnitIcon);
				break;
			default:
				break;
		}
	}

	/// <summary>
	///     Wwise logos
	/// </summary>
	private UnityEngine.Texture2D m_textureWwiseAuxBusIcon;
	private UnityEngine.Texture2D m_textureWwiseBusIcon;
	private UnityEngine.Texture2D m_textureWwiseEventIcon;
	private UnityEngine.Texture2D m_textureWwiseFolderIcon;
	private UnityEngine.Texture2D m_textureWwisePhysicalFolderIcon;
	private UnityEngine.Texture2D m_textureWwiseProjectIcon;
	private UnityEngine.Texture2D m_textureWwiseSoundbankIcon;
	private UnityEngine.Texture2D m_textureWwiseStateIcon;
	private UnityEngine.Texture2D m_textureWwiseStateGroupIcon;
	private UnityEngine.Texture2D m_textureWwiseSwitchIcon;
	private UnityEngine.Texture2D m_textureWwiseSwitchGroupIcon;
	private UnityEngine.Texture2D m_textureWwiseWorkUnitIcon;

	/// <summary>
	///     TreeViewControl overrides for our custom logos
	/// </summary>
	public override void AssignDefaults()
	{
		base.AssignDefaults();
		var tempWwisePath = "Assets/Wwise/Editor/WwiseWindows/TreeViewControl/";
		m_textureWwiseAuxBusIcon = GetTexture(tempWwisePath + "auxbus_nor.png");
		m_textureWwiseBusIcon = GetTexture(tempWwisePath + "bus_nor.png");
		m_textureWwiseEventIcon = GetTexture(tempWwisePath + "event_nor.png");
		m_textureWwiseFolderIcon = GetTexture(tempWwisePath + "folder_nor.png");
		m_textureWwisePhysicalFolderIcon = GetTexture(tempWwisePath + "physical_folder_nor.png");
		m_textureWwiseProjectIcon = GetTexture(tempWwisePath + "wproj.png");
		m_textureWwiseSoundbankIcon = GetTexture(tempWwisePath + "soundbank_nor.png");
		m_textureWwiseStateIcon = GetTexture(tempWwisePath + "state_nor.png");
		m_textureWwiseStateGroupIcon = GetTexture(tempWwisePath + "stategroup_nor.png");
		m_textureWwiseSwitchIcon = GetTexture(tempWwisePath + "switch_nor.png");
		m_textureWwiseSwitchGroupIcon = GetTexture(tempWwisePath + "switchgroup_nor.png");
		m_textureWwiseWorkUnitIcon = GetTexture(tempWwisePath + "workunit_nor.png");

		if (m_filterBoxStyle == null)
		{
			var InspectorSkin =
				UnityEngine.Object.Instantiate(UnityEditor.EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector));
			InspectorSkin.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
			m_filterBoxStyle = InspectorSkin.FindStyle("SearchTextField");
			m_filterBoxCancelButtonStyle = InspectorSkin.FindStyle("SearchCancelButton");
		}

		if (DragDropHelperMonoScript == null)
		{
			var scripts = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEditor.MonoScript>();
			for (var i = 0; i < scripts.Length; i++)
			{
				if (scripts[i].GetClass() == typeof(AkDragDropHelper))
				{
					DragDropHelperMonoScript = scripts[i];
					break;
				}
			}
		}
	}

	public override void DisplayTreeView(DisplayTypes displayType)
	{
		if (AkWwisePicker.WwiseProjectFound)
		{
			var filterString = m_filterString;

			if (m_filterBoxStyle == null)
			{
				m_filterBoxStyle = UnityEngine.Object
					.Instantiate(UnityEditor.EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector))
					.FindStyle("SearchTextField");
				m_filterBoxCancelButtonStyle = UnityEngine.Object
					.Instantiate(UnityEditor.EditorGUIUtility.GetBuiltinSkin(UnityEditor.EditorSkin.Inspector))
					.FindStyle("SearchCancelButton");
			}

			UnityEngine.GUILayout.BeginHorizontal("Box");
			{
				m_filterString = UnityEngine.GUILayout.TextField(m_filterString, m_filterBoxStyle);
				if (UnityEngine.GUILayout.Button("", m_filterBoxCancelButtonStyle))
					m_filterString = "";
			}
			UnityEngine.GUILayout.EndHorizontal();

			if (!m_filterString.Equals(filterString))
			{
				if (filterString.Equals(string.Empty))
					SaveExpansionStatus();

				FilterTreeview(RootItem);

				if (m_filterString.Equals(string.Empty))
				{
					var path = "";
					RestoreExpansionStatus(RootItem, ref path);
				}
			}

			base.DisplayTreeView(displayType);
		}
		else
		{
			UnityEngine.GUILayout.Label("Wwise Project not found at path:");
			UnityEngine.GUILayout.Label(AkUtilities.GetFullPath(UnityEngine.Application.dataPath,
				WwiseSetupWizard.Settings.WwiseProjectPath));
			UnityEngine.GUILayout.Label("Wwise Picker will not be usable.");
		}
	}

	private bool FilterTreeview(AK.Wwise.TreeView.TreeViewItem in_item)
	{
		in_item.IsHidden = in_item.Header.IndexOf(m_filterString, System.StringComparison.OrdinalIgnoreCase) < 0;
		in_item.IsExpanded = true;

		for (var i = 0; i < in_item.Items.Count; i++)
		{
			if (!FilterTreeview(in_item.Items[i]))
				in_item.IsHidden = false;
		}

		return in_item.IsHidden;
	}

	private void RestoreExpansionStatus(AK.Wwise.TreeView.TreeViewItem in_item, ref string in_path)
	{
		in_path = in_path + "/" + in_item.Header;

		in_item.IsExpanded = GetExpansionStatus(in_path);

		for (var i = 0; i < in_item.Items.Count; i++)
			RestoreExpansionStatus(in_item.Items[i], ref in_path);

		in_path = in_path.Remove(in_path.LastIndexOf('/'));
	}

	public void SaveExpansionStatus()
	{
		if (AkWwisePicker.WwiseProjectFound)
		{
			if (RootItem.Header == "Root item")
			{
				// We were unpopulated, no need to save. But we still need to display the correct data, though.
				AkWwisePicker.PopulateTreeview();
				return;
			}

			if (AkWwiseProjectInfo.GetData() != null)
			{
				var PreviousExpandedItems = AkWwiseProjectInfo.GetData().ExpandedItems;
				AkWwiseProjectInfo.GetData().ExpandedItems.Clear();

				var path = string.Empty;

				if (RootItem.HasChildItems() && RootItem.IsExpanded)
					SaveExpansionStatus(RootItem, path);

				AkWwiseProjectInfo.GetData().ExpandedItems.Sort();

				if (System.Linq.Enumerable.Count(System.Linq.Enumerable.Except(AkWwiseProjectInfo.GetData().ExpandedItems, PreviousExpandedItems)) > 0)
				UnityEditor.EditorUtility.SetDirty(AkWwiseProjectInfo.GetData());
			}
		}
	}

	private void SaveExpansionStatus(AK.Wwise.TreeView.TreeViewItem in_item, string in_path)
	{
		in_path = in_path + "/" + in_item.Header;

		AkWwiseProjectInfo.GetData().ExpandedItems.Add(in_path);

		for (var i = 0; i < in_item.Items.Count; i++)
		{
			if (in_item.Items[i].HasChildItems() && in_item.Items[i].IsExpanded)
				SaveExpansionStatus(in_item.Items[i], in_path);
		}

		in_path = in_path.Remove(in_path.LastIndexOf('/'));
	}

	public bool GetExpansionStatus(string in_path)
	{
		return AkWwiseProjectInfo.GetData().ExpandedItems.BinarySearch(in_path) >= 0;
	}

	public void SetScrollViewPosition(UnityEngine.Vector2 in_pos)
	{
		m_scrollView = in_pos;
	}

	public AK.Wwise.TreeView.TreeViewItem GetItemByPath(string in_path)
	{
		var headers = in_path.Split('/');

		if (!RootItem.Header.Equals(headers[0]))
			return null;

		var item = RootItem;

		for (var i = 1; i < headers.Length; i++)
		{
			item = item.Items.Find(x => x.Header.Equals(headers[i]));

			if (item == null)
				return null;
		}

		return item;
	}

	public AK.Wwise.TreeView.TreeViewItem GetItemByGuid(System.Guid in_guid)
	{
		return GetItemByGuid(RootItem, in_guid);
	}

	public AK.Wwise.TreeView.TreeViewItem GetItemByGuid(AK.Wwise.TreeView.TreeViewItem in_item, System.Guid in_guid)
	{
		var itemGuid = new System.Guid((in_item.DataContext as AkTreeInfo).Guid);

		if (itemGuid.Equals(in_guid))
			return in_item;

		for (var i = 0; i < in_item.Items.Count; i++)
		{
			var item = GetItemByGuid(in_item.Items[i], in_guid);

			if (item != null)
				return item;
		}

		return null;
	}

	public AK.Wwise.TreeView.TreeViewItem GetSelectedItem()
	{
		return GetSelectedItem(RootItem);
	}

	public AK.Wwise.TreeView.TreeViewItem GetSelectedItem(AK.Wwise.TreeView.TreeViewItem in_item)
	{
		if (in_item.IsSelected)
			return in_item;

		for (var i = 0; i < in_item.Items.Count; i++)
		{
			var item = GetSelectedItem(in_item.Items[i]);

			if (item != null)
				return item;
		}

		return null;
	}
}
#endif