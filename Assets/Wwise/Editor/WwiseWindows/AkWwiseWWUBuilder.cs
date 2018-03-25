#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

#pragma warning disable 0168
[UnityEditor.InitializeOnLoad]
public class AkWwiseWWUBuilder
{
	private const string s_progTitle = "Populating Wwise Picker";
	private const int s_SecondsBetweenChecks = 3;

	private static string s_wwiseProjectPath = System.IO.Path.GetDirectoryName(
		AkUtilities.GetFullPath(UnityEngine.Application.dataPath, WwiseSettings.LoadSettings().WwiseProjectPath));

	private static readonly string[] FoldersOfInterest =
	{
		"Events", "States", "Switches", "SoundBanks", "Master-Mixer Hierarchy", "Game Parameters", "Triggers",
		"Virtual Acoustics"
	};

	private static System.DateTime s_lastFileCheck = System.DateTime.Now.AddSeconds(-s_SecondsBetweenChecks);
	private static readonly FileInfo_CompareByPath s_FileInfo_CompareByPath = new FileInfo_CompareByPath();
	private static readonly tmpData_CompareByName s_comparetmpDataByName = new tmpData_CompareByName();

	private readonly System.Collections.Generic.HashSet<string> m_WwuToProcess =
		new System.Collections.Generic.HashSet<string>();

	private int m_currentWwuCnt;
	private int m_totWwuCnt = 1;

	public static void Tick()
	{
		if (AkWwiseProjectInfo.GetData() != null)
		{
			if (System.DateTime.Now.Subtract(s_lastFileCheck).Seconds > s_SecondsBetweenChecks &&
			    !UnityEditor.EditorApplication.isCompiling && !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode &&
			    AkWwiseProjectInfo.GetData().autoPopulateEnabled)
			{
				AkWwisePicker.treeView.SaveExpansionStatus();
				if (Populate())
				{
					AkWwisePicker.PopulateTreeview();
					//Make sure that the Wwise picker and the inspector are updated
					AkUtilities.RepaintInspector();
				}

				s_lastFileCheck = System.DateTime.Now;
			}
		}
	}

	public static bool AutoPopulate()
	{
		if (!System.IO.File.Exists(AkUtilities.GetFullPath(UnityEngine.Application.dataPath,
			WwiseSetupWizard.Settings.WwiseProjectPath)))
		{
			AkWwisePicker.WwiseProjectFound = false;
			return false;
		}

		AkWwisePicker.WwiseProjectFound = true;

		if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || string.IsNullOrEmpty(s_wwiseProjectPath) ||
		    UnityEditor.EditorApplication.isCompiling)
			return false;

		AkPluginActivator.Update();

		var builder = new AkWwiseWWUBuilder();
		if (!builder.GatherModifiedFiles())
			return false;

		builder.UpdateFiles();
		return true;
	}

	public static bool Populate()
	{
		try
		{
			if (WwiseSetupWizard.Settings.WwiseProjectPath == null)
				WwiseSettings.LoadSettings();

			if (string.IsNullOrEmpty(WwiseSetupWizard.Settings.WwiseProjectPath))
			{
				UnityEngine.Debug.LogError("WwiseUnity: Wwise project needed to populate from Work Units. Aborting.");
				return false;
			}

			s_wwiseProjectPath = System.IO.Path.GetDirectoryName(AkUtilities.GetFullPath(UnityEngine.Application.dataPath,
				WwiseSetupWizard.Settings.WwiseProjectPath));

			if (!System.IO.File.Exists(AkUtilities.GetFullPath(UnityEngine.Application.dataPath,
				WwiseSetupWizard.Settings.WwiseProjectPath)))
			{
				AkWwisePicker.WwiseProjectFound = false;
				return false;
			}

			AkWwisePicker.WwiseProjectFound = true;

			if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || string.IsNullOrEmpty(s_wwiseProjectPath) ||
			    UnityEditor.EditorApplication.isCompiling)
				return false;

			AkPluginActivator.Update();

			var builder = new AkWwiseWWUBuilder();
			if (!builder.GatherModifiedFiles())
				return false;

			builder.UpdateFiles();
			return true;
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError(e.ToString());
			UnityEditor.EditorUtility.ClearProgressBar();
		}

		return true; //There was an error, assume that we need to refresh.
	}

	private int RecurseWorkUnit(AssetType in_type, System.IO.FileInfo in_workUnit, string in_currentPathInProj,
		string in_currentPhysicalPath, System.Collections.Generic.LinkedList<AkWwiseProjectData.PathElement> in_pathAndIcons,
		string in_parentPhysicalPath = "")
	{
		m_WwuToProcess.Remove(in_workUnit.FullName);
		System.Xml.XmlReader reader = null;
		var wwuIndex = -1;
		try
		{
			//Progress bar stuff
			var msg = "Parsing Work Unit " + in_workUnit.Name;
			UnityEditor.EditorUtility.DisplayProgressBar(s_progTitle, msg, m_currentWwuCnt / (float) m_totWwuCnt);
			m_currentWwuCnt++;

			in_currentPathInProj =
				System.IO.Path.Combine(in_currentPathInProj, System.IO.Path.GetFileNameWithoutExtension(in_workUnit.Name));
			in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(
				System.IO.Path.GetFileNameWithoutExtension(in_workUnit.Name), AkWwiseProjectData.WwiseObjectType.WORKUNIT));
			var WwuPhysicalPath = System.IO.Path.Combine(in_currentPhysicalPath, in_workUnit.Name);

			AkWwiseProjectData.WorkUnit wwu = null;

			ReplaceWwuEntry(WwuPhysicalPath, in_type, out wwu, out wwuIndex);

			wwu.ParentPhysicalPath = in_parentPhysicalPath;
			wwu.PhysicalPath = WwuPhysicalPath;
			wwu.Guid = "";
			wwu.SetLastTime(System.IO.File.GetLastWriteTime(in_workUnit.FullName));

			reader = System.Xml.XmlReader.Create(in_workUnit.FullName);

			reader.MoveToContent();
			reader.Read();
			while (!reader.EOF && reader.ReadState == System.Xml.ReadState.Interactive)
			{
				if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name.Equals("WorkUnit"))
				{
					if (wwu.Guid.Equals(""))
						wwu.Guid = reader.GetAttribute("ID");

					var persistMode = reader.GetAttribute("PersistMode");
					if (persistMode == "Reference")
					{
						// ReadFrom advances the reader
						var matchedElement = System.Xml.Linq.XNode.ReadFrom(reader) as System.Xml.Linq.XElement;
						var newWorkUnitPath =
							System.IO.Path.Combine(in_workUnit.Directory.FullName, matchedElement.Attribute("Name").Value + ".wwu");
						var newWorkUnit = new System.IO.FileInfo(newWorkUnitPath);

						// Parse the referenced Work Unit
						if (m_WwuToProcess.Contains(newWorkUnit.FullName))
						{
							RecurseWorkUnit(in_type, newWorkUnit, in_currentPathInProj, in_currentPhysicalPath, in_pathAndIcons,
								WwuPhysicalPath);
						}
					}
					else
					{
						// If the persist mode is "Standalone" or "Nested", it means the current XML tag
						// is the one corresponding to the current file. We can ignore it and advance the reader
						reader.Read();
					}
				}
				else if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name.Equals("AuxBus"))
				{
					in_currentPathInProj = System.IO.Path.Combine(in_currentPathInProj, reader.GetAttribute("Name"));
					in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(reader.GetAttribute("Name"),
						AkWwiseProjectData.WwiseObjectType.AUXBUS));
					var isEmpty = reader.IsEmptyElement;
					AddElementToList(in_currentPathInProj, reader, in_type, in_pathAndIcons, wwuIndex);

					if (isEmpty)
					{
						in_currentPathInProj =
							in_currentPathInProj.Remove(in_currentPathInProj.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
						in_pathAndIcons.RemoveLast();
					}
				}
				// Busses and folders act the same for the Hierarchy: simply add them to the path
				else if (reader.NodeType == System.Xml.XmlNodeType.Element &&
				         (reader.Name.Equals("Folder") || reader.Name.Equals("Bus")))
				{
					//check if node has children
					if (!reader.IsEmptyElement)
					{
						// Add the folder/bus to the path
						in_currentPathInProj = System.IO.Path.Combine(in_currentPathInProj, reader.GetAttribute("Name"));
						if (reader.Name.Equals("Folder"))
						{
							in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(reader.GetAttribute("Name"),
								AkWwiseProjectData.WwiseObjectType.FOLDER));
						}
						else if (reader.Name.Equals("Bus"))
						{
							in_pathAndIcons.AddLast(new AkWwiseProjectData.PathElement(reader.GetAttribute("Name"),
								AkWwiseProjectData.WwiseObjectType.BUS));
						}
					}

					// Advance the reader
					reader.Read();
				}
				else if (reader.NodeType == System.Xml.XmlNodeType.EndElement &&
				         (reader.Name.Equals("Folder") || reader.Name.Equals("Bus") || reader.Name.Equals("AuxBus")))
				{
					// Remove the folder/bus from the path
					in_currentPathInProj =
						in_currentPathInProj.Remove(in_currentPathInProj.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
					in_pathAndIcons.RemoveLast();

					// Advance the reader
					reader.Read();
				}
				else if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name.Equals(in_type.XmlElementName))
				{
					// Add the element to the list
					AddElementToList(in_currentPathInProj, reader, in_type, in_pathAndIcons, wwuIndex);
				}
				else
					reader.Read();
			}

			// Sort the newly populated Wwu alphabetically
			SortWwu(in_type.RootDirectoryName, wwuIndex);
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError(e.ToString());
			wwuIndex = -1;
		}

		if (reader != null)
			reader.Close();

		in_pathAndIcons.RemoveLast();
		return wwuIndex;
	}

	public static void StartWWUWatcher()
	{
		Tick();
		UnityEditor.EditorApplication.update += Tick;
	}

	public static void StopWWUWatcher()
	{
		UnityEditor.EditorApplication.update -= Tick;
	}

	private bool GatherModifiedFiles()
	{
		var bChanged = false;
		var iBasePathLen = s_wwiseProjectPath.Length + 1;
		foreach (var dir in FoldersOfInterest)
		{
			var deleted = new System.Collections.Generic.List<int>();
			var knownFiles = AkWwiseProjectInfo.GetData().GetWwuListByString(dir);
			var cKnownBefore = knownFiles.Count;

			try
			{
				//Get all Wwus in this folder.
				var di = new System.IO.DirectoryInfo(System.IO.Path.Combine(s_wwiseProjectPath, dir));
				var files = di.GetFiles("*.wwu", System.IO.SearchOption.AllDirectories);
				System.Array.Sort(files, s_FileInfo_CompareByPath);

				//Walk both arrays
				var iKnown = 0;
				var iFound = 0;

				while (iFound < files.Length && iKnown < knownFiles.Count)
				{
					var workunit = knownFiles[iKnown] as AkWwiseProjectData.WorkUnit;
					var foundRelPath = files[iFound].FullName.Substring(iBasePathLen);
					switch (workunit.PhysicalPath.CompareTo(foundRelPath))
					{
						case 0:
							//File was there and is still there.  Check the FileTimes.
							try
							{
								var lastParsed = workunit.GetLastTime();
								if (files[iFound].LastWriteTime > lastParsed)
								{
									//File has been changed!   
									//If this file had a parent, parse recursively the parent itself
									m_WwuToProcess.Add(files[iFound].FullName);
									bChanged = true;
								}
							}
							catch (System.Exception)
							{
								//Access denied probably (file does exists since it was picked up by GetFiles).
								//Just ignore this file.
							}

							iFound++;
							iKnown++;
							break;

						case 1:
							m_WwuToProcess.Add(files[iFound].FullName);
							iFound++;
							break;

						case -1:
							//A file was deleted.  Can't process it now, it would change the array indices.                                
							deleted.Add(iKnown);
							iKnown++;
							break;
					}
				}

				//The remainder from the files found on disk are all new files.
				for (; iFound < files.Length; iFound++)
					m_WwuToProcess.Add(files[iFound].FullName);

				//All the remainder is deleted.  From the end, of course.
				if (iKnown < knownFiles.Count)
					knownFiles.RemoveRange(iKnown, knownFiles.Count - iKnown);

				//Delete those tagged.
				for (var i = deleted.Count - 1; i >= 0; i--)
					knownFiles.RemoveAt(deleted[i]);

				bChanged |= cKnownBefore != knownFiles.Count;
			}
			catch (System.Exception)
			{
				return false;
			}
		}

		return bChanged || m_WwuToProcess.Count > 0;
	}

	private void UpdateFiles()
	{
		m_totWwuCnt = m_WwuToProcess.Count;

		var iBasePathLen = s_wwiseProjectPath.Length + 1;
		var iUnprocessed = 0;
		while (m_WwuToProcess.Count - iUnprocessed > 0)
		{
			System.Collections.IEnumerator e = m_WwuToProcess.GetEnumerator();
			for (var i = 0; i < iUnprocessed + 1; i++)
				e.MoveNext();

			var fullPath = e.Current as string;
			var relPath = fullPath.Substring(iBasePathLen);
			var typeStr = relPath.Remove(relPath.IndexOf(System.IO.Path.DirectorySeparatorChar));
			if (!createWorkUnit(relPath, typeStr, fullPath))
				iUnprocessed++;
		}

		//Add the unprocessed directly.  This can happen if we don't find the parent WorkUnit.  
		//Normally, it should never happen, this is just a safe guard.
		while (m_WwuToProcess.Count > 0)
		{
			System.Collections.IEnumerator e = m_WwuToProcess.GetEnumerator();
			e.MoveNext();
			var fullPath = e.Current as string;
			var relPath = fullPath.Substring(iBasePathLen);
			var typeStr = relPath.Remove(relPath.IndexOf(System.IO.Path.DirectorySeparatorChar));
			UpdateWorkUnit(string.Empty, fullPath, typeStr, relPath);
		}

		UnityEditor.EditorUtility.ClearProgressBar();
	}

	private static void SortValues(AkWwiseProjectData.GroupValue groupToSort)
	{
		if (groupToSort.values.Count > 0)
		{
			var listToSort = new tmpData[groupToSort.values.Count];
			for (var i = 0; i < groupToSort.values.Count; i++)
			{
				listToSort[i] = new tmpData();
				listToSort[i].valueName = groupToSort.values[i];
				listToSort[i].pathElem = groupToSort.ValueIcons[i];
				listToSort[i].ID = groupToSort.valueIDs[i];
				listToSort[i].Guid = groupToSort.ValueGuids[i];
			}

			System.Array.Sort(listToSort, s_comparetmpDataByName);

			for (var i = 0; i < groupToSort.values.Count; i++)
			{
				groupToSort.values[i] = listToSort[i].valueName;
				groupToSort.ValueIcons[i] = listToSort[i].pathElem;
				groupToSort.valueIDs[i] = listToSort[i].ID;
				groupToSort.ValueGuids[i] = listToSort[i].Guid;
			}
		}
	}

	private static void SortWwu(string in_type, int in_wwuIndex)
	{
		if (string.Equals(in_type, "Events", System.StringComparison.OrdinalIgnoreCase))
		{
			System.Collections.ArrayList.Adapter(AkWwiseProjectInfo.GetData().EventWwu[in_wwuIndex].List)
				.Sort(AkWwiseProjectData.s_compareAkInformationByName);
		}
		else if (string.Equals(in_type, "States", System.StringComparison.OrdinalIgnoreCase))
		{
			var StateList = AkWwiseProjectInfo.GetData().StateWwu[in_wwuIndex].List;
			System.Collections.ArrayList.Adapter(StateList).Sort(AkWwiseProjectData.s_compareAkInformationByName);
			foreach (var StateGroup in StateList)
				SortValues(StateGroup);
		}
		else if (string.Equals(in_type, "Switches", System.StringComparison.OrdinalIgnoreCase))
		{
			var SwitchList = AkWwiseProjectInfo.GetData().SwitchWwu[in_wwuIndex].List;
			System.Collections.ArrayList.Adapter(SwitchList).Sort(AkWwiseProjectData.s_compareAkInformationByName);
			foreach (var SwitchGroup in SwitchList)
				SortValues(SwitchGroup);
		}
		else if (string.Equals(in_type, "Master-Mixer Hierarchy", System.StringComparison.OrdinalIgnoreCase))
		{
			System.Collections.ArrayList.Adapter(AkWwiseProjectInfo.GetData().AuxBusWwu[in_wwuIndex].List)
				.Sort(AkWwiseProjectData.s_compareAkInformationByName);
		}
		else if (string.Equals(in_type, "SoundBanks", System.StringComparison.OrdinalIgnoreCase))
		{
			System.Collections.ArrayList.Adapter(AkWwiseProjectInfo.GetData().BankWwu[in_wwuIndex].List)
				.Sort(AkWwiseProjectData.s_compareAkInformationByName);
		}
		else if (string.Equals(in_type, "Game Parameters", System.StringComparison.OrdinalIgnoreCase))
		{
			System.Collections.ArrayList.Adapter(AkWwiseProjectInfo.GetData().RtpcWwu[in_wwuIndex].List)
				.Sort(AkWwiseProjectData.s_compareAkInformationByName);
		}
		else if (string.Equals(in_type, "Triggers", System.StringComparison.OrdinalIgnoreCase))
		{
			System.Collections.ArrayList.Adapter(AkWwiseProjectInfo.GetData().TriggerWwu[in_wwuIndex].List)
				.Sort(AkWwiseProjectData.s_compareAkInformationByName);
		}
		else if (string.Equals(in_type, "Virtual Acoustics", System.StringComparison.OrdinalIgnoreCase))
		{
			System.Collections.ArrayList.Adapter(AkWwiseProjectInfo.GetData().AcousticTextureWwu[in_wwuIndex].List)
				.Sort(AkWwiseProjectData.s_compareAkInformationByName);
		}
	}

	private static void ReplaceWwuEntry(string in_currentPhysicalPath, AssetType in_type,
		out AkWwiseProjectData.WorkUnit out_wwu, out int out_wwuIndex)
	{
		var list = AkWwiseProjectInfo.GetData().GetWwuListByString(in_type.RootDirectoryName);
		out_wwuIndex = list.BinarySearch(new AkWwiseProjectData.WorkUnit(in_currentPhysicalPath),
			AkWwiseProjectData.s_compareByPhysicalPath);
		out_wwu = AkWwiseProjectInfo.GetData().NewChildWorkUnit(in_type.RootDirectoryName);

		if (out_wwuIndex < 0)
		{
			out_wwuIndex = ~out_wwuIndex;
			list.Insert(out_wwuIndex, out_wwu);
		}
		else
			list[out_wwuIndex] = out_wwu;
	}

	private static void AddElementToList(string in_currentPathInProj, System.Xml.XmlReader in_reader, AssetType in_type,
		System.Collections.Generic.LinkedList<AkWwiseProjectData.PathElement> in_pathAndIcons, int in_wwuIndex)
	{
		if (in_type.RootDirectoryName == "Events" || in_type.RootDirectoryName == "Master-Mixer Hierarchy" ||
		    in_type.RootDirectoryName == "SoundBanks" || in_type.RootDirectoryName == "Game Parameters" ||
		    in_type.RootDirectoryName == "Triggers" || in_type.RootDirectoryName == "Virtual Acoustics")
		{
			var valueToAdd = new AkWwiseProjectData.Event();

			valueToAdd.Name = in_reader.GetAttribute("Name");
			valueToAdd.Guid = new System.Guid(in_reader.GetAttribute("ID")).ToByteArray();
			valueToAdd.ID = (int) AkUtilities.ShortIDGenerator.Compute(valueToAdd.Name);
			valueToAdd.PathAndIcons = new System.Collections.Generic.List<AkWwiseProjectData.PathElement>(in_pathAndIcons);

			switch (in_type.RootDirectoryName)
			{
				case "Events":
					valueToAdd.Path = System.IO.Path.Combine(in_currentPathInProj, valueToAdd.Name);
					valueToAdd.PathAndIcons.Add(
						new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.EVENT));
					AkWwiseProjectInfo.GetData().EventWwu[in_wwuIndex].List.Add(valueToAdd);
					break;

				case "SoundBanks":
					valueToAdd.Path = System.IO.Path.Combine(in_currentPathInProj, valueToAdd.Name);
					valueToAdd.PathAndIcons.Add(
						new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.SOUNDBANK));
					AkWwiseProjectInfo.GetData().BankWwu[in_wwuIndex].List.Add(valueToAdd);
					break;

				case "Master-Mixer Hierarchy":
					valueToAdd.Path = in_currentPathInProj;
					AkWwiseProjectInfo.GetData().AuxBusWwu[in_wwuIndex].List.Add(valueToAdd);
					break;

				case "Game Parameters":
					valueToAdd.Path = System.IO.Path.Combine(in_currentPathInProj, valueToAdd.Name);
					valueToAdd.PathAndIcons.Add(
						new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.GAMEPARAMETER));
					AkWwiseProjectInfo.GetData().RtpcWwu[in_wwuIndex].List.Add(valueToAdd);
					break;

				case "Triggers":
					valueToAdd.Path = System.IO.Path.Combine(in_currentPathInProj, valueToAdd.Name);
					valueToAdd.PathAndIcons.Add(
						new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.TRIGGER));
					AkWwiseProjectInfo.GetData().TriggerWwu[in_wwuIndex].List.Add(valueToAdd);
					break;

				case "Virtual Acoustics":
					valueToAdd.Path = System.IO.Path.Combine(in_currentPathInProj, valueToAdd.Name);
					valueToAdd.PathAndIcons.Add(new AkWwiseProjectData.PathElement(valueToAdd.Name,
						AkWwiseProjectData.WwiseObjectType.ACOUSTICTEXTURE));
					AkWwiseProjectInfo.GetData().AcousticTextureWwu[in_wwuIndex].List.Add(valueToAdd);
					break;
			}

			in_reader.Read();
		}
		else if (in_type.RootDirectoryName == "States" || in_type.RootDirectoryName == "Switches")
		{
			var XmlElement = System.Xml.Linq.XNode.ReadFrom(in_reader) as System.Xml.Linq.XElement;

			var valueToAdd = new AkWwiseProjectData.GroupValue();
			AkWwiseProjectData.WwiseObjectType SubElemIcon;
			valueToAdd.Name = XmlElement.Attribute("Name").Value;
			valueToAdd.Guid = new System.Guid(XmlElement.Attribute("ID").Value).ToByteArray();
			valueToAdd.ID = (int) AkUtilities.ShortIDGenerator.Compute(valueToAdd.Name);
			valueToAdd.Path = System.IO.Path.Combine(in_currentPathInProj, valueToAdd.Name);
			valueToAdd.PathAndIcons = new System.Collections.Generic.List<AkWwiseProjectData.PathElement>(in_pathAndIcons);

			if (in_type.RootDirectoryName == "States")
			{
				SubElemIcon = AkWwiseProjectData.WwiseObjectType.STATE;
				valueToAdd.PathAndIcons.Add(
					new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.STATEGROUP));
			}
			else
			{
				SubElemIcon = AkWwiseProjectData.WwiseObjectType.SWITCH;
				valueToAdd.PathAndIcons.Add(
					new AkWwiseProjectData.PathElement(valueToAdd.Name, AkWwiseProjectData.WwiseObjectType.SWITCHGROUP));
			}

			var ChildrenList = System.Xml.Linq.XName.Get("ChildrenList");
			var ChildElem = System.Xml.Linq.XName.Get(in_type.ChildElementName);

			var ChildrenElement = XmlElement.Element(ChildrenList);
			if (ChildrenElement != null)
			{
				foreach (var element in ChildrenElement.Elements(ChildElem))
				{
					if (element.Name == in_type.ChildElementName)
					{
						var elementName = element.Attribute("Name").Value;
						valueToAdd.values.Add(elementName);
						valueToAdd.ValueGuids.Add(
							new AkWwiseProjectData.ByteArrayWrapper(new System.Guid(element.Attribute("ID").Value).ToByteArray()));
						valueToAdd.valueIDs.Add((int) AkUtilities.ShortIDGenerator.Compute(elementName));
						valueToAdd.ValueIcons.Add(new AkWwiseProjectData.PathElement(elementName, SubElemIcon));
					}
				}
			}

			if (in_type.RootDirectoryName == "States")
				AkWwiseProjectInfo.GetData().StateWwu[in_wwuIndex].List.Add(valueToAdd);
			else
				AkWwiseProjectInfo.GetData().SwitchWwu[in_wwuIndex].List.Add(valueToAdd);
		}
		else
			UnityEngine.Debug.LogError("WwiseUnity: Unknown asset type in WWU parser");
	}

	private bool createWorkUnit(string in_relativePath, string in_wwuType, string in_fullPath)
	{
		var ParentID = string.Empty;
		try
		{
			var reader = System.Xml.XmlReader.Create(in_fullPath);
			reader.MoveToContent();

			//We check if the current work unit has a parent and save its guid if its the case
			while (!reader.EOF && reader.ReadState == System.Xml.ReadState.Interactive)
			{
				if (reader.NodeType == System.Xml.XmlNodeType.Element && reader.Name.Equals("WorkUnit"))
				{
					if (reader.GetAttribute("PersistMode").Equals("Nested"))
						ParentID = reader.GetAttribute("OwnerID");
					break;
				}

				reader.Read();
			}

			reader.Close();
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.Log("WwiseUnity: A changed Work unit wasn't found. It must have been deleted " + in_fullPath);
			return false;
		}

		if (!string.IsNullOrEmpty(ParentID))
		{
			var parentPhysicalPath = string.Empty;

			var list = AkWwiseProjectInfo.GetData().GetWwuListByString(in_wwuType);

			//search for the parent and save its physical path
			for (var i = 0; i < list.Count; i++)
			{
				if ((list[i] as AkWwiseProjectData.WorkUnit).Guid.Equals(ParentID))
				{
					parentPhysicalPath = (list[i] as AkWwiseProjectData.WorkUnit).PhysicalPath;
					UpdateWorkUnit(parentPhysicalPath, in_fullPath, in_wwuType, in_relativePath);
					return true;
				}
			}

			//Not found.  Wait for it to load
			return false;
		}

		//Root Wwu
		UpdateWorkUnit(string.Empty, in_fullPath, in_wwuType, in_relativePath);
		return true;
	}

	private void UpdateWorkUnit(string in_parentRelativePath, string in_wwuFullPath, string in_wwuType,
		string in_relativePath)
	{
		var wwuRelPath = in_parentRelativePath;

		var PathAndIcons = new System.Collections.Generic.LinkedList<AkWwiseProjectData.PathElement>();

		//We need to build the work unit's hierarchy to display it in the right place in the picker
		var currentPathInProj = string.Empty;
		while (!wwuRelPath.Equals(string.Empty))
		{
			//Add work unit name to the hierarchy
			var wwuName = System.IO.Path.GetFileNameWithoutExtension(wwuRelPath);
			currentPathInProj = System.IO.Path.Combine(wwuName, currentPathInProj);
			//Add work unit icon to the hierarchy
			PathAndIcons.AddFirst(new AkWwiseProjectData.PathElement(wwuName, AkWwiseProjectData.WwiseObjectType.WORKUNIT));

			//Get the physical path of the parent work unit if any
			var list = AkWwiseProjectInfo.GetData().GetWwuListByString(in_wwuType);
			var index = list.BinarySearch(new AkWwiseProjectData.WorkUnit(wwuRelPath),
				AkWwiseProjectData.s_compareByPhysicalPath);
			wwuRelPath = (list[index] as AkWwiseProjectData.WorkUnit).ParentPhysicalPath;
		}

		//Add physical folders to the hierarchy if the work unit isn't in the root folder
		var physicalPath = in_relativePath.Split(System.IO.Path.DirectorySeparatorChar);
		for (var i = physicalPath.Length - 2; i > 0; i--)
		{
			PathAndIcons.AddFirst(
				new AkWwiseProjectData.PathElement(physicalPath[i], AkWwiseProjectData.WwiseObjectType.PHYSICALFOLDER));
			currentPathInProj = System.IO.Path.Combine(physicalPath[i], currentPathInProj);
		}

		//Parse the work unit file
		RecurseWorkUnit(GetAssetTypeByRootDir(in_wwuType), new System.IO.FileInfo(in_wwuFullPath), currentPathInProj,
			in_relativePath.Remove(in_relativePath.LastIndexOf(System.IO.Path.DirectorySeparatorChar)), PathAndIcons,
			in_parentRelativePath);
	}

	private static AssetType GetAssetTypeByRootDir(string in_rootDir)
	{
		if (string.Equals(in_rootDir, "Events", System.StringComparison.OrdinalIgnoreCase))
			return new AssetType("Events", "Event", "");
		if (string.Equals(in_rootDir, "States", System.StringComparison.OrdinalIgnoreCase))
			return new AssetType("States", "StateGroup", "State");
		if (string.Equals(in_rootDir, "Switches", System.StringComparison.OrdinalIgnoreCase))
			return new AssetType("Switches", "SwitchGroup", "Switch");
		if (string.Equals(in_rootDir, "Master-Mixer Hierarchy", System.StringComparison.OrdinalIgnoreCase))
			return new AssetType("Master-Mixer Hierarchy", "AuxBus", "");
		if (string.Equals(in_rootDir, "SoundBanks", System.StringComparison.OrdinalIgnoreCase))
			return new AssetType("SoundBanks", "SoundBank", "");
		if (string.Equals(in_rootDir, "Game Parameters", System.StringComparison.OrdinalIgnoreCase))
			return new AssetType("Game Parameters", "GameParameter", "");
		if (string.Equals(in_rootDir, "Triggers", System.StringComparison.OrdinalIgnoreCase))
			return new AssetType("Triggers", "Trigger", "");
		if (string.Equals(in_rootDir, "Virtual Acoustics", System.StringComparison.OrdinalIgnoreCase))
			return new AssetType("Virtual Acoustics", "AcousticTexture", "");

		return null;
	}

	public class AssetType
	{
		public string ChildElementName;
		public string RootDirectoryName;
		public string XmlElementName;

		public AssetType(string RootFolder, string XmlElemName, string ChildName)
		{
			RootDirectoryName = RootFolder;
			XmlElementName = XmlElemName;
			ChildElementName = ChildName;
		}
	}

	private class FileInfo_CompareByPath : System.Collections.Generic.IComparer<System.IO.FileInfo>
	{
		int System.Collections.Generic.IComparer<System.IO.FileInfo>.Compare(System.IO.FileInfo wwuA, System.IO.FileInfo wwuB)
		{
			return wwuA.FullName.CompareTo(wwuB.FullName);
		}
	}

	private class tmpData
	{
		public AkWwiseProjectData.ByteArrayWrapper Guid;
		public int ID;
		public AkWwiseProjectData.PathElement pathElem;
		public string valueName;
	}

	private class tmpData_CompareByName : System.Collections.Generic.IComparer<tmpData>
	{
		int System.Collections.Generic.IComparer<tmpData>.Compare(tmpData AkInfA, tmpData AkInfB)
		{
			return AkInfA.valueName.CompareTo(AkInfB.valueName);
		}
	}
}
#endif