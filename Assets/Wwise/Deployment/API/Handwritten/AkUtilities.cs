#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
#if UNITY_EDITOR
[System.Serializable]
public class WwiseSettings
{
	public const string WwiseSettingsFilename = "WwiseSettings.xml";

	private static WwiseSettings s_Instance;
	public bool CopySoundBanksAsPreBuildStep = true;
	public bool CreatedPicker = false;
	public bool CreateWwiseGlobal = true;
	public bool CreateWwiseListener = true;
	public bool GenerateSoundBanksAsPreBuildStep = false;
	public bool ShowMissingRigidBodyWarning = true;
	public string SoundbankPath;
	public string WwiseInstallationPathMac;
	public string WwiseInstallationPathWindows;
	public string WwiseProjectPath;

	// Save the WwiseSettings structure to a serialized XML file
	public static void SaveSettings(WwiseSettings Settings)
	{
		try
		{
			var xmlDoc = new System.Xml.XmlDocument();
			var xmlSerializer = new System.Xml.Serialization.XmlSerializer(Settings.GetType());
			using (var xmlStream = new System.IO.MemoryStream())
			{
				var streamWriter = new System.IO.StreamWriter(xmlStream, System.Text.Encoding.UTF8);
				xmlSerializer.Serialize(streamWriter, Settings);
				xmlStream.Position = 0;
				xmlDoc.Load(xmlStream);
				xmlDoc.Save(System.IO.Path.Combine(UnityEngine.Application.dataPath, WwiseSettingsFilename));
			}
		}
		catch (System.Exception)
		{
		}
	}

	// Load the WwiseSettings structure from a serialized XML file
	public static WwiseSettings LoadSettings(bool ForceLoad = false)
	{
		if (s_Instance != null && !ForceLoad)
			return s_Instance;

		var Settings = new WwiseSettings();
		try
		{
			if (System.IO.File.Exists(System.IO.Path.Combine(UnityEngine.Application.dataPath, WwiseSettingsFilename)))
			{
				var xmlSerializer = new System.Xml.Serialization.XmlSerializer(Settings.GetType());
				var xmlFileStream = new System.IO.FileStream(UnityEngine.Application.dataPath + "/" + WwiseSettingsFilename,
					System.IO.FileMode.Open, System.IO.FileAccess.Read);
				Settings = (WwiseSettings) xmlSerializer.Deserialize(xmlFileStream);
				xmlFileStream.Close();
			}
			else
			{
				var projectDir = System.IO.Path.GetDirectoryName(UnityEngine.Application.dataPath);
				var foundWwiseProjects = System.IO.Directory.GetFiles(projectDir, "*.wproj", System.IO.SearchOption.AllDirectories);

				if (foundWwiseProjects.Length == 0)
					Settings.WwiseProjectPath = "";
				else
				{
					Settings.WwiseProjectPath =
						AkUtilities.MakeRelativePath(UnityEngine.Application.dataPath + "/fake_depth", foundWwiseProjects[0]);
				}

				Settings.SoundbankPath = AkSoundEngineController.s_DefaultBasePath;
			}

			s_Instance = Settings;
		}
		catch (System.Exception)
		{
		}

		return Settings;
	}
}

public partial class AkUtilities
{
	public static bool IsMigrating = false;

	/// Unity platform enum to Wwise soundbank reference platform name mapping.
	private static readonly System.Collections.Generic.IDictionary<UnityEditor.BuildTarget, string[]> platformMapping =
		new System.Collections.Generic.Dictionary<UnityEditor.BuildTarget, string[]>
		{
#if UNITY_2017_3_OR_NEWER
			{ UnityEditor.BuildTarget.StandaloneOSX, new[] { "Mac" } },
#else
			{ UnityEditor.BuildTarget.StandaloneOSXUniversal, new[] { "Mac" } },
			{ UnityEditor.BuildTarget.StandaloneOSXIntel, new[] { "Mac" } },
			{ UnityEditor.BuildTarget.StandaloneOSXIntel64, new[] { "Mac" } },
#endif
			{ UnityEditor.BuildTarget.StandaloneWindows, new[] { "Windows" } },
			{ UnityEditor.BuildTarget.iOS, new[] { "iOS" } },
			{ UnityEditor.BuildTarget.Android, new[] { "Android" } },
			{ UnityEditor.BuildTarget.StandaloneLinux, new[] { "Linux" } },
			{ UnityEditor.BuildTarget.StandaloneWindows64, new[] { "Windows" } },
			{ UnityEditor.BuildTarget.WSAPlayer, new[] { "Windows" } },
			{ UnityEditor.BuildTarget.StandaloneLinux64, new[] { "Linux" } },
			{ UnityEditor.BuildTarget.StandaloneLinuxUniversal, new[] { "Linux" } },
			{ UnityEditor.BuildTarget.PSP2, new[] { "VitaHW", "VitaSW" } },
			{ UnityEditor.BuildTarget.PS4, new[] { "PS4" } },
			{ UnityEditor.BuildTarget.XboxOne, new[] { "XboxOne" } }
		};


	private static readonly System.Collections.Generic.Dictionary<string, string> s_ProjectBankPaths =
		new System.Collections.Generic.Dictionary<string, string>();

	private static System.DateTime s_LastBankPathUpdate = System.DateTime.MinValue;

	private static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>
		s_BaseToCustomPF = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>();

	public static bool IsSoundbankGenerationAvailable()
	{
		return GetWwiseCLI() != null;
	}

	/// Executes a command-line. Blocks the calling thread until the new process has completed. Returns the logged stdout in one big string.
	public static string ExecuteCommandLine(string command, string arguments)
	{
		var process = new System.Diagnostics.Process();
		process.StartInfo.FileName = command;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.RedirectStandardOutput = true;
		process.StartInfo.CreateNoWindow = true;
		process.StartInfo.Arguments = arguments;
		process.Start();

		// Synchronously read the standard output of the spawned process. 
		var reader = process.StandardOutput;
		var output = reader.ReadToEnd();

		// Waiting for the process to exit directly in the UI thread. Similar cases are working that way too.

		// TODO: Is it better to provide a timeout avoid any issues of forever blocking the UI thread? If so, what is
		// a relevant timeout value for soundbank generation?
		process.WaitForExit();
		process.Close();

		return output;
	}

	private static string GetWwiseCLI()
	{
		string result = null;

		var settings = WwiseSettings.LoadSettings();

#if UNITY_EDITOR_WIN
		if (!string.IsNullOrEmpty(settings.WwiseInstallationPathWindows))
		{
			result = System.IO.Path.Combine(settings.WwiseInstallationPathWindows, @"Authoring\x64\Release\bin\WwiseCLI.exe");

			if (!System.IO.File.Exists(result))
				result = System.IO.Path.Combine(settings.WwiseInstallationPathWindows, @"Authoring\Win32\Release\bin\WwiseCLI.exe");
		}
#elif UNITY_EDITOR_OSX
		if (!string.IsNullOrEmpty(settings.WwiseInstallationPathMac))
			result = System.IO.Path.Combine(settings.WwiseInstallationPathMac, "Contents/Tools/WwiseCLI.sh");
#endif

		if (result != null && System.IO.File.Exists(result))
			return result;

		return null;
	}

	// Generate all the SoundBanks for all the supported platforms in the Wwise project. This effectively calls Wwise for the project
	// that is configured in the UnityWwise integration.
	public static void GenerateSoundbanks(System.Collections.Generic.List<string> platforms = null)
	{
		var Settings = WwiseSettings.LoadSettings();
		var wwiseProjectFullPath = GetFullPath(UnityEngine.Application.dataPath, Settings.WwiseProjectPath);

		if (IsSoundbankOverrideEnabled(wwiseProjectFullPath))
		{
			UnityEngine.Debug.LogWarning(
				"The SoundBank generation process ignores the SoundBank Settings' Overrides currently enabled in the User settings. The project's SoundBank settings will be used.");
		}

		var wwiseCli = GetWwiseCLI();

		if (wwiseCli == null)
		{
			UnityEngine.Debug.LogError("Couldn't locate WwiseCLI, unable to generate SoundBanks.");
			return;
		}

#if UNITY_EDITOR_WIN
		var command = wwiseCli;
		var arguments = "";
#elif UNITY_EDITOR_OSX
		var command = "/bin/sh";
		var arguments = "\"" + wwiseCli + "\"";
#else
		var command = "";
		var arguments = "";
#endif

		arguments += " \"" + wwiseProjectFullPath + "\"";

		if (platforms != null)
		{
			foreach (var platform in platforms)
			{
				if (!string.IsNullOrEmpty(platform))
					arguments += " -Platform " + platform;
			}
		}

		arguments += " -GenerateSoundBanks";

		var output = ExecuteCommandLine(command, arguments);

		var success = output.Contains("Process completed successfully.");
		var warning = output.Contains("Process completed with warning");

		var message = "WwiseUnity: SoundBanks generation " +
		              (success ? "successful" : (warning ? "has warning(s)" : "error")) + ":\n" + output;

		if (success)
			UnityEngine.Debug.Log(message);
		else if (warning)
			UnityEngine.Debug.LogWarning(message);
		else
			UnityEngine.Debug.LogError(message);

		UnityEditor.AssetDatabase.Refresh();
	}

	/// Reads the user settings (not the project settings) to check if there is an override currently defined for the soundbank generation folders.
	public static bool IsSoundbankOverrideEnabled(string wwiseProjectPath)
	{
		var userConfigFile = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(wwiseProjectPath),
			System.IO.Path.GetFileNameWithoutExtension(wwiseProjectPath) + "." + System.Environment.UserName + ".wsettings");

		if (!System.IO.File.Exists(userConfigFile))
			return false;

		var userConfigDoc = new System.Xml.XmlDocument();
		userConfigDoc.Load(userConfigFile);
		var userConfigNavigator = userConfigDoc.CreateNavigator();

		var userConfigNode = userConfigNavigator.SelectSingleNode(
			System.Xml.XPath.XPathExpression.Compile("//Property[@Name='SoundBankPathUserOverride' and @Value = 'True']"));

		return userConfigNode != null;
	}

	// For a list of platform targets, gather all the required folder names for the generated soundbanks. The goal is to know the list of required
	// folder names for a list of platforms. The size of the returned array is not guaranteed to be the safe size at the targets array since
	// a single platform is no guaranteed to output a single soundbank folder.
	public static bool GetWwiseSoundBankDestinationFoldersByUnityPlatform(UnityEditor.BuildTarget target,
		string WwiseProjectPath, out string[] paths, out string[] platformNames)
	{
		paths = null;
		platformNames = null;

		try
		{
			if (WwiseProjectPath.Length == 0)
				return false;

			var doc = new System.Xml.XmlDocument();
			doc.Load(WwiseProjectPath);
			var configNavigator = doc.CreateNavigator();

			var pathsList = new System.Collections.Generic.List<string>();
			var platformNamesList = new System.Collections.Generic.List<string>();

			if (platformMapping.ContainsKey(target))
			{
				var referencePlatforms = platformMapping[target];

				// For each valid reference platform name for the provided Unity platform enum, list all the valid platform names
				// defined in the Wwise project XML, and for each of those accumulate the sound bank folders. In the end,
				// resultList will contain a list of soundbank folders that are generated for the provided unity platform enum.

				foreach (var reference in referencePlatforms)
				{
					var expression =
						System.Xml.XPath.XPathExpression.Compile(string.Format("//Platforms/Platform[@ReferencePlatform='{0}']",
							reference));
					var nodes = configNavigator.Select(expression);

					while (nodes.MoveNext())
					{
						var platform = nodes.Current.GetAttribute("Name", "");

						// If the sound bank path information was located either in the user configuration or the project configuration, acquire the paths
						// for the provided platform.
						string sbPath = null;
						if (s_ProjectBankPaths.TryGetValue(platform, out sbPath))
						{
#if UNITY_EDITOR_OSX
							sbPath = sbPath.Replace('\\', System.IO.Path.DirectorySeparatorChar);
#endif
							pathsList.Add(sbPath);
							platformNamesList.Add(platform);
						}
					}
				}
			}

			if (pathsList.Count != 0)
			{
				paths = pathsList.ToArray();
				platformNames = platformNamesList.ToArray();
				return true;
			}
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogException(e);
		}

		return false;
	}

	public static System.Collections.Generic.IDictionary<string, System.Collections.Generic.HashSet<string>>
		GetPlatformMapping()
	{
		return s_BaseToCustomPF;
	}

	public static System.Collections.Generic.IDictionary<string, string> GetAllBankPaths()
	{
		var Settings = WwiseSettings.LoadSettings();
		var WwiseProjectFullPath = GetFullPath(UnityEngine.Application.dataPath, Settings.WwiseProjectPath);
		UpdateSoundbanksDestinationFolders(WwiseProjectFullPath);
		return s_ProjectBankPaths;
	}

	private static void UpdateSoundbanksDestinationFolders(string WwiseProjectPath)
	{
		try
		{
			if (WwiseProjectPath.Length == 0)
				return;

			if (!System.IO.File.Exists(WwiseProjectPath))
				return;

			var t = System.IO.File.GetLastWriteTime(WwiseProjectPath);
			if (t <= s_LastBankPathUpdate)
				return;

			s_LastBankPathUpdate = t;
			s_ProjectBankPaths.Clear();

			var doc = new System.Xml.XmlDocument();
			doc.Load(WwiseProjectPath);
			var Navigator = doc.CreateNavigator();

			// Gather the mapping of Custom platform to Base platform
			var itpf = Navigator.Select("//Platform");
			s_BaseToCustomPF.Clear();
			foreach (System.Xml.XPath.XPathNavigator node in itpf)
			{
				System.Collections.Generic.HashSet<string> customList = null;
				var basePF = node.GetAttribute("ReferencePlatform", "");
				if (!s_BaseToCustomPF.TryGetValue(basePF, out customList))
				{
					customList = new System.Collections.Generic.HashSet<string>();
					s_BaseToCustomPF[basePF] = customList;
				}

				customList.Add(node.GetAttribute("Name", ""));
			}

			// Navigate the wproj file (XML format) to where generated soundbank paths are stored
			var it = Navigator.Select("//Property[@Name='SoundBankPaths']/ValueList/Value");
			foreach (System.Xml.XPath.XPathNavigator node in it)
			{
				var path = node.Value;
				AkBasePathGetter.FixSlashes(ref path);
				var pf = node.GetAttribute("Platform", "");
				s_ProjectBankPaths[pf] = path;
			}
		}
		catch (System.Exception ex)
		{
			// Error happened, return empty string
			UnityEngine.Debug.LogError("Wwise: Error while reading project " + WwiseProjectPath + ".  Exception: " + ex.Message);
		}
	}

	// Parses the .wproj to find out where soundbanks are generated for the given path.
	public static string GetWwiseSoundBankDestinationFolder(string Platform, string WwiseProjectPath)
	{
		try
		{
			UpdateSoundbanksDestinationFolders(WwiseProjectPath);
			return s_ProjectBankPaths[Platform];
		}
		catch (System.Exception)
		{
		}

		// Error happened, return empty string
		return "";
	}

	// Set soundbank-related bool settings in the wproj file.
	public static bool EnableBoolSoundbankSettingInWproj(string SettingName, string WwiseProjectPath)
	{
		try
		{
			if (WwiseProjectPath.Length == 0)
				return true;

			var doc = new System.Xml.XmlDocument();
			doc.PreserveWhitespace = true;
			doc.Load(WwiseProjectPath);
			var Navigator = doc.CreateNavigator();

			// Navigate the wproj file (XML format) to where our setting should be
			var pathInXml = string.Format("/WwiseDocument/ProjectInfo/Project/PropertyList/Property[@Name='{0}']", SettingName);
			var expression = System.Xml.XPath.XPathExpression.Compile(pathInXml);
			var node = Navigator.SelectSingleNode(expression);
			if (node == null)
			{
				// Setting isn't in the wproj, add it
				// Navigate to the SoundBankHeaderFilePath property (it is always there)
				expression =
					System.Xml.XPath.XPathExpression.Compile(
						"/WwiseDocument/ProjectInfo/Project/PropertyList/Property[@Name='SoundBankHeaderFilePath']");
				node = Navigator.SelectSingleNode(expression);
				if (node == null)
				{
					// SoundBankHeaderFilePath not in wproj, invalid wproj file
					UnityEngine.Debug.LogError(
						"WwiseUnity: Could not find SoundBankHeaderFilePath property in Wwise project file. File is invalid.");
					return false;
				}

				// Add the setting right above SoundBankHeaderFilePath
				var propertyToInsert = string.Format("<Property Name=\"{0}\" Type=\"bool\" Value=\"True\"/>", SettingName);
				node.InsertBefore(propertyToInsert);
			}
			else if (node.GetAttribute("Value", "") == "False")
			{
				// Value is present, we simply have to modify it.
				if (!node.MoveToAttribute("Value", ""))
					return false;

				// Modify the value to true
				node.SetValue("True");
			}
			else
			{
				// Parameter already set, nothing to do!
				return true;
			}

			doc.Save(WwiseProjectPath);
		}
		catch (System.Exception)
		{
			return false;
		}

		return true;
	}

	public static bool SetSoundbankHeaderFilePath(string WwiseProjectPath, string SoundbankPath)
	{
		try
		{
			if (WwiseProjectPath.Length == 0)
				return true;

			var doc = new System.Xml.XmlDocument();
			doc.PreserveWhitespace = true;
			doc.Load(WwiseProjectPath);
			var Navigator = doc.CreateNavigator();

			// Navigate to where the header file path is saved. The node has to be there, or else the wproj is invalid.
			var expression =
				System.Xml.XPath.XPathExpression.Compile(
					"/WwiseDocument/ProjectInfo/Project/PropertyList/Property[@Name='SoundBankHeaderFilePath']");
			var node = Navigator.SelectSingleNode(expression);
			if (node == null)
			{
				UnityEngine.Debug.LogError(
					"Could not find SoundBankHeaderFilePath property in Wwise project file. File is invalid.");
				return false;
			}

			// Change the "Value" attribute
			if (!node.MoveToAttribute("Value", ""))
				return false;

			node.SetValue(SoundbankPath);
			doc.Save(WwiseProjectPath);
			return true;
		}
		catch (System.Exception)
		{
			// Error happened, return empty string
			return false;
		}
	}

	// Make two paths relative to each other
	public static string MakeRelativePath(string fromPath, string toPath)
	{
		try
		{
			if (string.IsNullOrEmpty(fromPath))
				return toPath;

			if (string.IsNullOrEmpty(toPath))
				return "";

			var fromUri = new System.Uri(fromPath);
			var toUri = new System.Uri(toPath);

			if (fromUri.Scheme != toUri.Scheme)
				return toPath;

			var relativeUri = fromUri.MakeRelativeUri(toUri);
			var relativePath = System.Uri.UnescapeDataString(relativeUri.ToString());

			return relativePath;
		}
		catch
		{
			return toPath;
		}
	}

	// Reconcile a base path and a relative path to give a full path without any ".."
	public static string GetFullPath(string BasePath, string RelativePath)
	{
		string tmpString;
		if (string.IsNullOrEmpty(BasePath))
			return "";

		var wrongSeparatorChar = System.IO.Path.DirectorySeparatorChar == '/' ? '\\' : '/';

		if (string.IsNullOrEmpty(RelativePath))
			return BasePath.Replace(wrongSeparatorChar, System.IO.Path.DirectorySeparatorChar);

		if (System.IO.Path.GetPathRoot(RelativePath) != "")
			return RelativePath.Replace(wrongSeparatorChar, System.IO.Path.DirectorySeparatorChar);

		tmpString = System.IO.Path.Combine(BasePath, RelativePath);
		tmpString = System.IO.Path.GetFullPath(new System.Uri(tmpString).LocalPath);

		return tmpString.Replace(wrongSeparatorChar, System.IO.Path.DirectorySeparatorChar);
	}


	public static bool DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
	{
		// Get the subdirectories for the specified directory.
		var dir = new System.IO.DirectoryInfo(sourceDirName);

		if (!dir.Exists)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Source directory doesn't exist");
			return false;
		}

		var dirs = dir.GetDirectories();

		// If the destination directory doesn't exist, create it. 
		if (!System.IO.Directory.Exists(destDirName))
			System.IO.Directory.CreateDirectory(destDirName);

		// Get the files in the directory and copy them to the new location.
		var files = dir.GetFiles();
		foreach (var file in files)
		{
			var temppath = System.IO.Path.Combine(destDirName, file.Name);
			file.CopyTo(temppath, true);
		}

		// If copying subdirectories, copy them and their contents to new location. 
		if (copySubDirs)
		{
			foreach (var subdir in dirs)
			{
				var temppath = System.IO.Path.Combine(destDirName, subdir.Name);
				DirectoryCopy(subdir.FullName, temppath, copySubDirs);
			}
		}

		return true;
	}

	public static byte[] GetByteArrayProperty(UnityEditor.SerializedProperty property)
	{
		if (!property.isArray || property.arraySize == 0)
			return null;

		var byteArray = new byte[property.arraySize];

		for (var i = 0; i < byteArray.Length; i++)
			byteArray[i] = (byte) property.GetArrayElementAtIndex(i).intValue;

		return byteArray;
	}


	public static void SetByteArrayProperty(UnityEditor.SerializedProperty property, byte[] byteArray)
	{
		if (!property.isArray)
			return;

		var iterator = property.Copy();

		iterator.arraySize = byteArray.Length;

		while (iterator.name != "data")
			iterator.Next(true);

		for (var i = 0; i < byteArray.Length; i++)
		{
			iterator.intValue = byteArray[i];
			iterator.Next(true);
		}
	}


	///This function returns the absolute position and the width and height of the last drawn GuiLayout(or EditorGuiLayout) element in the inspector window.
	///This function must be called in the OnInspectorGUI function
	/// 
	///The inspector must be in repaint mode in order to get the correct position 
	///Example => if(Event.current.type == EventType.Repaint) Rect pos = AkUtilities.GetLastRectAbsolute();
	public static UnityEngine.Rect GetLastRectAbsolute(bool applyScroll = true)
	{
		var inspectorType = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor))
			.GetType("UnityEditor.InspectorWindow");

		var currentInspectorFieldInfo = inspectorType.GetField("s_CurrentInspectorWindow",
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
		var positionPropInfo = inspectorType.GetProperty("position",
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		var InspectorPosition = (UnityEngine.Rect) positionPropInfo.GetValue(currentInspectorFieldInfo.GetValue(null), null);

		if (!applyScroll)
			return new UnityEngine.Rect(InspectorPosition.x, InspectorPosition.y, InspectorPosition.width, 0);

		var scrollPosInfo = inspectorType.GetField("m_ScrollPosition",
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
		var scrollPos = (UnityEngine.Vector2) scrollPosInfo.GetValue(currentInspectorFieldInfo.GetValue(null));

		var relativePos = UnityEngine.GUILayoutUtility.GetLastRect();

		return new UnityEngine.Rect(InspectorPosition.x + relativePos.x - scrollPos.x,
			InspectorPosition.y + relativePos.y - scrollPos.y, relativePos.width, relativePos.height);
	}

	public static void RepaintInspector()
	{
		var inspectorType = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor))
			.GetType("UnityEditor.InspectorWindow");
		var getAllInspectorInfo = inspectorType.GetMethod("GetAllInspectorWindows",
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
		if (getAllInspectorInfo == null)
			return;

		var inspector = (UnityEditor.EditorWindow[]) getAllInspectorInfo.Invoke(null, null);
		if (inspector == null)
			return;

		for (var i = 0; i < inspector.Length; i++)
			inspector[i].Repaint();
	}
}
#endif // UNITY_EDITOR

public partial class AkUtilities
{
	/// <summary>
	///     This is based on FNVHash as used by the DataManager
	///     to assign short IDs to objects. Be sure to keep them both in sync
	///     when making changes!
	/// </summary>
	public class ShortIDGenerator
	{
		private const uint s_prime32 = 16777619;
		private const uint s_offsetBasis32 = 2166136261;

		private static byte s_hashSize;
		private static uint s_mask;

		static ShortIDGenerator()
		{
			HashSize = 32;
		}

		public static byte HashSize
		{
			get { return s_hashSize; }

			set
			{
				s_hashSize = value;
				s_mask = (uint) ((1 << s_hashSize) - 1);
			}
		}

		public static uint Compute(string in_name)
		{
			var buffer = System.Text.Encoding.UTF8.GetBytes(in_name.ToLower());

			// Start with the basis value
			var hval = s_offsetBasis32;

			for (var i = 0; i < buffer.Length; i++)
			{
				// multiply by the 32 bit FNV magic prime mod 2^32
				hval *= s_prime32;

				// xor the bottom with the current octet
				hval ^= buffer[i];
			}

			if (s_hashSize == 32)
				return hval;

			// XOR-Fold to the required number of bits
			return (hval >> s_hashSize) ^ (hval & s_mask);
		}
	}
}

#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.