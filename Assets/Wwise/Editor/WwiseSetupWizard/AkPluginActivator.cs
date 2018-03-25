#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
public class AkPluginActivator
{
	public const string ALL_PLATFORMS = "All";
	public const string CONFIG_DEBUG = "Debug";
	public const string CONFIG_PROFILE = "Profile";
	public const string CONFIG_RELEASE = "Release";

	private const string EditorConfiguration = CONFIG_PROFILE;

	private const string MENU_PATH = "Assets/Wwise/Activate Plugins/";
	private const UnityEditor.BuildTarget INVALID_BUILD_TARGET = (UnityEditor.BuildTarget) (-1);

	private const string WwisePluginFolder = "Wwise/Deployment/Plugins";

	// The following check is required until "BuildTarget.Switch" is available on all versions of Unity that we support.
	private static readonly UnityEditor.BuildTarget SwitchBuildTarget = GetPlatformBuildTarget("Switch");

	private static readonly System.Collections.Generic.Dictionary<string, System.DateTime> s_LastParsed =
		new System.Collections.Generic.Dictionary<string, System.DateTime>();

	private static readonly System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>
		s_PerPlatformPlugins =
			new System.Collections.Generic.Dictionary<string, System.Collections.Generic.HashSet<string>>();

	private static readonly System.Collections.Generic.HashSet<PluginID> builtInPluginIDs =
		new System.Collections.Generic.HashSet<PluginID>
		{
			PluginID.SineGenerator,
			PluginID.WwiseSilence,
			PluginID.ToneGenerator,
			PluginID.WwiseParametricEQ,
			PluginID.Delay,
			PluginID.WwiseCompressor,
			PluginID.WwiseExpander,
			PluginID.WwisePeakLimiter,
			PluginID.MatrixReverb,
			PluginID.WwiseRoomVerb,
			PluginID.WwiseMeter,
			PluginID.Gain,
			PluginID.VitaReverb,
			PluginID.VitaCompressor,
			PluginID.VitaDelay,
			PluginID.VitaDistortion,
			PluginID.VitaEQ
		};

	static AkPluginActivator()
	{
		ActivatePluginsForEditor();
	}

	// Use reflection because projects that were created in Unity 4 won't have the CurrentPluginConfig field
	public static string GetCurrentConfig()
	{
		var CurrentPluginConfigField = typeof(AkWwiseProjectData).GetField("CurrentPluginConfig");
		var CurrentConfig = string.Empty;
		var data = AkWwiseProjectInfo.GetData();

		if (CurrentPluginConfigField != null && data != null)
			CurrentConfig = (string) CurrentPluginConfigField.GetValue(data);

		if (string.IsNullOrEmpty(CurrentConfig))
			CurrentConfig = CONFIG_PROFILE;

		return CurrentConfig;
	}

	private static void SetCurrentConfig(string config)
	{
		var CurrentPluginConfigField = typeof(AkWwiseProjectData).GetField("CurrentPluginConfig");
		var data = AkWwiseProjectInfo.GetData();
		if (CurrentPluginConfigField != null)
			CurrentPluginConfigField.SetValue(data, config);

		UnityEditor.EditorUtility.SetDirty(AkWwiseProjectInfo.GetData());
	}

	private static void ActivateConfig(string config)
	{
		SetCurrentConfig(config);
		CheckMenuItems(config);
	}

	[UnityEditor.MenuItem(MENU_PATH + CONFIG_DEBUG)]
	public static void ActivateDebug()
	{
		ActivateConfig(CONFIG_DEBUG);
	}

	[UnityEditor.MenuItem(MENU_PATH + CONFIG_PROFILE)]
	public static void ActivateProfile()
	{
		ActivateConfig(CONFIG_PROFILE);
	}

	[UnityEditor.MenuItem(MENU_PATH + CONFIG_RELEASE)]
	public static void ActivateRelease()
	{
		ActivateConfig(CONFIG_RELEASE);
	}

	private static UnityEditor.BuildTarget GetPlatformBuildTarget(string platform)
	{
		var targets = System.Enum.GetNames(typeof(UnityEditor.BuildTarget));
		var values = System.Enum.GetValues(typeof(UnityEditor.BuildTarget));

		for (var ii = 0; ii < targets.Length; ++ii)
		{
			if (platform.Equals(targets[ii]))
				return (UnityEditor.BuildTarget) values.GetValue(ii);
		}

		return INVALID_BUILD_TARGET;
	}

	// returns the name of the folder that contains plugins for a specific target
	private static string GetPluginDeploymentPlatformName(UnityEditor.BuildTarget target)
	{
		switch (target)
		{
			case UnityEditor.BuildTarget.Android:
				return "Android";

			case UnityEditor.BuildTarget.iOS:
				return "iOS";

			case UnityEditor.BuildTarget.tvOS:
				return "tvOS";

			case UnityEditor.BuildTarget.StandaloneLinux:
			case UnityEditor.BuildTarget.StandaloneLinux64:
			case UnityEditor.BuildTarget.StandaloneLinuxUniversal:
				return "Linux";

#if UNITY_2017_3_OR_NEWER
			case UnityEditor.BuildTarget.StandaloneOSX:
#else
			case UnityEditor.BuildTarget.StandaloneOSXIntel:
			case UnityEditor.BuildTarget.StandaloneOSXIntel64:
			case UnityEditor.BuildTarget.StandaloneOSXUniversal:
#endif
				return "Mac";

			case UnityEditor.BuildTarget.PS4:
				return "PS4";

			case UnityEditor.BuildTarget.PSP2:
				return "Vita";

			case UnityEditor.BuildTarget.StandaloneWindows:
			case UnityEditor.BuildTarget.StandaloneWindows64:
				return "Windows";

			case UnityEditor.BuildTarget.WSAPlayer:
				return "WSA";

			case UnityEditor.BuildTarget.XboxOne:
				return "XboxOne";

#if UNITY_5_6_OR_NEWER
			case UnityEditor.BuildTarget.Switch:
				return "Switch";
#endif
		}

		return target.ToString();
	}

	public static void ActivatePluginsForDeployment(UnityEditor.BuildTarget target, bool Activate)
	{
		var ChangedSomeAssets = false;

		var staticPluginRegistration =
			target == UnityEditor.BuildTarget.iOS || target == UnityEditor.BuildTarget.tvOS || target == SwitchBuildTarget
				? new StaticPluginRegistration(target)
				: null;

		//PluginImporter[] importers = PluginImporter.GetImporters(target);
		var importers = UnityEditor.PluginImporter.GetAllImporters();

		foreach (var pluginImporter in importers)
		{
			if (!pluginImporter.assetPath.Contains(WwisePluginFolder))
				continue;

			var splitPath = pluginImporter.assetPath.Split('/');

			// Path is Assets/Wwise/Deployment/Plugins/Platform. We need the platform string
			var pluginPlatform = splitPath[4];
			if (pluginPlatform != GetPluginDeploymentPlatformName(target))
				continue;

			var pluginArch = string.Empty;
			var pluginConfig = string.Empty;

			switch (pluginPlatform)
			{
				case "iOS":
				case "tvOS":
				case "PS4":
				case "XboxOne":
					pluginConfig = splitPath[5];
					break;

				case "Android":
					pluginArch = splitPath[5];
					pluginConfig = splitPath[6];

					if (pluginArch == "armeabi-v7a")
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.Android, "CPU", "ARMv7");
					else if (pluginArch == "x86")
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.Android, "CPU", "x86");
					else
						UnityEngine.Debug.Log("WwiseUnity: Architecture not found: " + pluginArch);
					break;

				case "Linux":
					pluginArch = splitPath[5];
					pluginConfig = splitPath[6];

					if (pluginArch == "x86")
					{
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux, "CPU", "x86");
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux64, "CPU", "None");
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinuxUniversal, "CPU", "x86");
					}
					else if (pluginArch == "x86_64")
					{
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux, "CPU", "None");
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux64, "CPU", "x86_64");
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinuxUniversal, "CPU", "x86_64");
					}
					else
					{
						UnityEngine.Debug.Log("WwiseUnity: Architecture not found: " + pluginArch);
						continue;
					}

#if UNITY_2017_3_OR_NEWER
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSX, "CPU", "None");
#else
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSXIntel, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSXIntel64, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSXUniversal, "CPU", "None");
#endif
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows64, "CPU", "None");
					break;

				case "Mac":
					pluginConfig = splitPath[5];
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux64, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinuxUniversal, "CPU", "None");

#if UNITY_2017_3_OR_NEWER
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSX, "CPU", "AnyCPU");
#else
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSXIntel, "CPU", "AnyCPU");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSXIntel64, "CPU", "AnyCPU");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSXUniversal, "CPU", "AnyCPU");
#endif
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows64, "CPU", "None");
					break;

				case "WSA":
					pluginArch = splitPath[5];
					pluginConfig = splitPath[6];

					if (pluginArch == "WSA_UWP_Win32")
					{
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "SDK", "AnySDK");
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "CPU", "X86");
					}
					else if (pluginArch == "WSA_UWP_x64")
					{
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "SDK", "AnySDK");
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "CPU", "X64");
					}
					else if (pluginArch == "WSA_UWP_ARM")
					{
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "SDK", "AnySDK");
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.WSAPlayer, "CPU", "ARM");
					}
					break;

				case "Windows":
					pluginArch = splitPath[5];
					pluginConfig = splitPath[6];

					if (pluginArch == "x86")
					{
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows, "CPU", "AnyCPU");
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows64, "CPU", "None");
					}
					else if (pluginArch == "x86_64")
					{
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows, "CPU", "None");
						pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneWindows64, "CPU", "AnyCPU");
					}
					else
					{
						UnityEngine.Debug.Log("WwiseUnity: Architecture not found: " + pluginArch);
						continue;
					}

					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinux64, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneLinuxUniversal, "CPU", "None");
#if UNITY_2017_3_OR_NEWER
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSX, "CPU", "None");
#else
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSXIntel, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSXIntel64, "CPU", "None");
					pluginImporter.SetPlatformData(UnityEditor.BuildTarget.StandaloneOSXUniversal, "CPU", "None");
#endif
					break;

				case "Switch":
					pluginArch = splitPath[5];
					pluginConfig = splitPath[6];

					if (SwitchBuildTarget == INVALID_BUILD_TARGET)
						continue;

					if (pluginArch != "NX32" && pluginArch != "NX64")
					{
						UnityEngine.Debug.Log("WwiseUnity: Architecture not found: " + pluginArch);
						continue;
					}
					break;

				default:
					UnityEngine.Debug.Log("WwiseUnity: Unknown platform: " + pluginPlatform);
					continue;
			}

			var AssetChanged = false;
			if (pluginImporter.GetCompatibleWithAnyPlatform())
			{
				pluginImporter.SetCompatibleWithAnyPlatform(false);
				AssetChanged = true;
			}

			var bActivate = true;
			if (pluginConfig == "DSP")
			{
				if (!IsPluginUsed(pluginPlatform, System.IO.Path.GetFileNameWithoutExtension(pluginImporter.assetPath)))
					bActivate = false;
				else if (staticPluginRegistration != null)
					staticPluginRegistration.TryAddLibrary(pluginImporter.assetPath);
			}
			else if (pluginConfig != GetCurrentConfig())
				bActivate = false;

			if (!bActivate && target == UnityEditor.BuildTarget.WSAPlayer)
				AssetChanged = true;
			else
				AssetChanged |= pluginImporter.GetCompatibleWithPlatform(target) == !bActivate || !Activate;

			pluginImporter.SetCompatibleWithPlatform(target, bActivate && Activate);

			if (AssetChanged)
			{
				ChangedSomeAssets = true;
				UnityEditor.AssetDatabase.ImportAsset(pluginImporter.assetPath);
			}
		}

		if (ChangedSomeAssets && staticPluginRegistration != null)
			staticPluginRegistration.TryWriteToFile();
	}

	public static void ActivatePluginsForEditor()
	{
		var importers = UnityEditor.PluginImporter.GetAllImporters();
		var ChangedSomeAssets = false;

		foreach (var pluginImporter in importers)
		{
			if (!pluginImporter.assetPath.Contains(WwisePluginFolder))
				continue;

			var splitPath = pluginImporter.assetPath.Split('/');

			// Path is Assets/Wwise/Deployment/Plugins/Platform. We need the platform string
			var pluginPlatform = splitPath[4];
			var pluginConfig = string.Empty;
			var editorCPU = string.Empty;
			var editorOS = string.Empty;

			switch (pluginPlatform)
			{
				case "Mac":
					pluginConfig = splitPath[5];
					editorCPU = "AnyCPU";
					editorOS = "OSX";
					break;

				case "Windows":
					editorCPU = splitPath[5];
					pluginConfig = splitPath[6];
					editorOS = "Windows";
					break;
			}

			var AssetChanged = false;
			if (pluginImporter.GetCompatibleWithAnyPlatform())
			{
				pluginImporter.SetCompatibleWithAnyPlatform(false);
				AssetChanged = true;
			}

			var bActivate = false;
			if (!string.IsNullOrEmpty(editorOS))
			{
				if (pluginConfig == "DSP")
				{
					if (!s_PerPlatformPlugins.ContainsKey(pluginPlatform))
						continue;

					bActivate = IsPluginUsed(pluginPlatform, System.IO.Path.GetFileNameWithoutExtension(pluginImporter.assetPath));
				}
				else
					bActivate = pluginConfig == EditorConfiguration;

				if (bActivate)
				{
					pluginImporter.SetEditorData("CPU", editorCPU);
					pluginImporter.SetEditorData("OS", editorOS);
				}
			}

			AssetChanged |= pluginImporter.GetCompatibleWithEditor() != bActivate;
			pluginImporter.SetCompatibleWithEditor(bActivate);

			if (AssetChanged)
			{
				ChangedSomeAssets = true;
				UnityEditor.AssetDatabase.ImportAsset(pluginImporter.assetPath);
			}
		}

		if (ChangedSomeAssets)
			UnityEngine.Debug.Log("WwiseUnity: Plugins successfully activated for " + EditorConfiguration + " in Editor.");
	}

	private static void CheckMenuItems(string config)
	{
		UnityEditor.Menu.SetChecked(MENU_PATH + CONFIG_DEBUG, config == CONFIG_DEBUG);
		UnityEditor.Menu.SetChecked(MENU_PATH + CONFIG_PROFILE, config == CONFIG_PROFILE);
		UnityEditor.Menu.SetChecked(MENU_PATH + CONFIG_RELEASE, config == CONFIG_RELEASE);
	}

	public static void DeactivateAllPlugins()
	{
		var importers = UnityEditor.PluginImporter.GetAllImporters();

		foreach (var pluginImporter in importers)
		{
			if (!pluginImporter.assetPath.Contains(WwisePluginFolder))
				continue;

			pluginImporter.SetCompatibleWithAnyPlatform(false);
			UnityEditor.AssetDatabase.ImportAsset(pluginImporter.assetPath);
		}
	}

	public static bool IsPluginUsed(string in_UnityPlatform, string in_PluginName)
	{
		// For WSA, we use the plugin info for Windows, since they share banks. Same for tvOS vs iOS.
		var pluginDSPPlatform = in_UnityPlatform;
		switch (pluginDSPPlatform)
		{
			case "WSA":
				pluginDSPPlatform = "Windows";
				break;
			case "tvOS":
				pluginDSPPlatform = "iOS";
				break;
		}

		if (!s_PerPlatformPlugins.ContainsKey(pluginDSPPlatform))
			return false; //XML not parsed, don't touch anything.

		if (in_PluginName.Contains("AkSoundEngine"))
			return true;

		var pluginName = in_PluginName;
		if (in_PluginName.StartsWith("lib"))
			pluginName = in_PluginName.Substring(3);

		System.Collections.Generic.HashSet<string> plugins;
		if (s_PerPlatformPlugins.TryGetValue(pluginDSPPlatform, out plugins))
		{
			if (in_UnityPlatform != "iOS" && in_UnityPlatform != "tvOS" && in_UnityPlatform != "Switch")
				return plugins.Contains(pluginName);

			//iOS, tvOS, and Switch deal with the static libs directly, unlike all other platforms.
			//Luckily the DLL name is always a subset of the lib name.
			foreach (var pl in plugins)
			{
				if (!string.IsNullOrEmpty(pl) && pluginName.Contains(pl))
					return true;

				if (string.Compare(pl, "iZotope", false) == 0 && pluginName.StartsWith("iZ"))
					return true;
			}

			//Exceptions

			if (in_UnityPlatform == "iOS" && pluginName.Contains("AkiOSPlugins"))
				return true;

			if (in_UnityPlatform == "tvOS" && pluginName.Contains("AktvOSPlugins"))
				return true;

			if (in_UnityPlatform == "Switch" && pluginName.Contains("AkSwitchPlugins"))
				return true;

			if (plugins.Contains("AkSoundSeedAir") &&
			    (pluginName.Contains("SoundSeedWind") || pluginName.Contains("SoundSeedWoosh")))
				return true;
		}

		return false;
	}

	public static void Update(bool forceUpdate = false)
	{
		//Gather all GeneratedSoundBanks folder from the project
		var allPaths = AkUtilities.GetAllBankPaths();
		var bNeedRefresh = false;
		var projectPath = System.IO.Path.GetDirectoryName(AkUtilities.GetFullPath(UnityEngine.Application.dataPath,
			WwiseSettings.LoadSettings().WwiseProjectPath));

		var pfMap = AkUtilities.GetPlatformMapping();

		//Go through all BasePlatforms 
		foreach (var pairPF in pfMap)
		{
			//Go through all custom platforms related to that base platform and check if any of the bank files were updated.
			var bParse = forceUpdate;
			var fullPaths = new System.Collections.Generic.List<string>();
			foreach (var customPF in pairPF.Value)
			{
				string bankPath;
				if (!allPaths.TryGetValue(customPF, out bankPath))
					continue;

				var pluginFile = "";
				try
				{
					pluginFile = System.IO.Path.Combine(projectPath, System.IO.Path.Combine(bankPath, "PluginInfo.xml"));
					pluginFile = pluginFile.Replace('/', System.IO.Path.DirectorySeparatorChar);
					if (!System.IO.File.Exists(pluginFile))
					{
						//Try in StreamingAssets too.
						pluginFile = System.IO.Path.Combine(System.IO.Path.Combine(AkBasePathGetter.GetFullSoundBankPath(), customPF),
							"PluginInfo.xml");
						if (!System.IO.File.Exists(pluginFile))
							continue;
					}

					fullPaths.Add(pluginFile);

					var t = System.IO.File.GetLastWriteTime(pluginFile);
					var lastTime = System.DateTime.MinValue;
					s_LastParsed.TryGetValue(customPF, out lastTime);
					if (lastTime < t)
					{
						bParse = true;
						s_LastParsed[customPF] = t;
					}
				}
				catch (System.Exception ex)
				{
					UnityEngine.Debug.LogError("Wwise: " + pluginFile + " could not be parsed. " + ex.Message);
				}
			}

			if (bParse)
			{
				var platform = pairPF.Key;

				var newDlls = ParsePluginsXML(platform, fullPaths);
				System.Collections.Generic.HashSet<string> oldDlls = null;

				//Remap base Wwise platforms to Unity platform folders names
				if (platform.Contains("Vita"))
					platform = "Vita";
				//else other platforms already have the right name

				s_PerPlatformPlugins.TryGetValue(platform, out oldDlls);
				s_PerPlatformPlugins[platform] = newDlls;

				//Check if there was any change.
				if (!bNeedRefresh && oldDlls != null)
				{
					if (oldDlls.Count == newDlls.Count)
						oldDlls.IntersectWith(newDlls);

					bNeedRefresh |= oldDlls.Count != newDlls.Count;
				}
				else
					bNeedRefresh |= newDlls.Count > 0;
			}
		}

		if (bNeedRefresh)
			ActivatePluginsForEditor();

		var currentConfig = GetCurrentConfig();
		CheckMenuItems(currentConfig);
	}

	private static System.Collections.Generic.HashSet<string> ParsePluginsXML(string platform,
		System.Collections.Generic.List<string> in_pluginFiles)
	{
		var newDlls = new System.Collections.Generic.HashSet<string>();

		foreach (var pluginFile in in_pluginFiles)
		{
			if (!System.IO.File.Exists(pluginFile))
				continue;

			try
			{
				var doc = new System.Xml.XmlDocument();
				doc.Load(pluginFile);
				var Navigator = doc.CreateNavigator();
				var pluginInfoNode = Navigator.SelectSingleNode("//PluginInfo");
				var boolMotion = pluginInfoNode.GetAttribute("Motion", "");

				var it = Navigator.Select("//Plugin");

				if (boolMotion == "true")
					newDlls.Add("AkMotion");

				foreach (System.Xml.XPath.XPathNavigator node in it)
				{
					var pid = uint.Parse(node.GetAttribute("ID", ""));
					if (pid == 0)
						continue;

					var dll = string.Empty;

					if (platform == "Switch")
					{
						if ((PluginID) pid == PluginID.WwiseMeter)
							dll = "AkMeter";
					}
					else if (builtInPluginIDs.Contains((PluginID) pid))
						continue;

					if (string.IsNullOrEmpty(dll))
						dll = node.GetAttribute("DLL", "");

					newDlls.Add(dll);
				}
			}
			catch (System.Exception ex)
			{
				UnityEngine.Debug.LogError("Wwise: " + pluginFile + " could not be parsed. " + ex.Message);
			}
		}

		return newDlls;
	}

	private class StaticPluginRegistration
	{
		private readonly System.Collections.Generic.HashSet<string> FactoriesHeaderFilenames =
			new System.Collections.Generic.HashSet<string>();

		private readonly UnityEditor.BuildTarget Target;
		private bool Active;

		public StaticPluginRegistration(UnityEditor.BuildTarget target)
		{
			Target = target;
		}

		public void TryAddLibrary(string AssetPath)
		{
			Active = true;

			if (AssetPath.Contains(".a"))
			{
				//Extract the lib name, generate the registration code.
				var begin = AssetPath.LastIndexOf('/') + 4;
				var end = AssetPath.LastIndexOf('.') - begin;
				var LibName = AssetPath.Substring(begin, end); //Remove the lib prefix and the .a extension                    

				if (!LibName.Contains("AkSoundEngine"))
					FactoriesHeaderFilenames.Add(LibName + "Factory.h");
			}
			else if (AssetPath.Contains("Factory.h"))
				FactoriesHeaderFilenames.Add(System.IO.Path.GetFileName(AssetPath));
		}

		public void TryWriteToFile()
		{
			if (!Active)
				return;

			string RelativePath;
			string CppText;

			if (Target == UnityEditor.BuildTarget.iOS)
			{
				RelativePath = "/iOS/DSP/AkiOSPlugins.cpp";
				CppText = "#define AK_IOS";
			}
			else if (Target == UnityEditor.BuildTarget.tvOS)
			{
				RelativePath = "/tvOS/DSP/AktvOSPlugins.cpp";
				CppText = "#define AK_IOS";
			}
			else if (Target == SwitchBuildTarget)
			{
				RelativePath = "/Switch/NX64/DSP/AkSwitchPlugins.cpp";
				CppText = "#define AK_NX";
			}
			else
				return;

			CppText += @"
namespace AK { class PluginRegistration; };
#define AK_STATIC_LINK_PLUGIN(_pluginName_) \
extern AK::PluginRegistration _pluginName_##Registration; \
void *_pluginName_##_fp = (void*)&_pluginName_##Registration;

";

			foreach (var filename in FactoriesHeaderFilenames)
				CppText += "#include \"" + filename + "\"\n";

			try
			{
				var FullPath = System.IO.Path.Combine(UnityEngine.Application.dataPath, WwisePluginFolder + RelativePath);
				System.IO.File.WriteAllText(FullPath, CppText);
				FactoriesHeaderFilenames.Clear();
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogError("Wwise: Could not write <" + RelativePath + ">. Exception: " + e.Message);
			}
		}
	}

	private enum PluginID
	{
		SineGenerator = 0x00640002, //Sine
		WwiseSilence = 0x00650002, //Wwise Silence
		ToneGenerator = 0x00660002, //Tone Generator
		WwiseParametricEQ = 0x00690003, //Wwise Parametric EQ
		Delay = 0x006A0003, //Delay
		WwiseCompressor = 0x006C0003, //Wwise Compressor
		WwiseExpander = 0x006D0003, //Wwise Expander
		WwisePeakLimiter = 0x006E0003, //Wwise Peak Limiter
		MatrixReverb = 0x00730003, //Matrix Reverb
		WwiseRoomVerb = 0x00760003, //Wwise RoomVerb
		WwiseMeter = 0x00810003, //Wwise Meter
		Gain = 0x008B0003, //Gain
		VitaReverb = 0x008C0003, //Vita Reverb
		VitaCompressor = 0x008D0003, //Vita Compressor
		VitaDelay = 0x008E0003, //Vita Delay
		VitaDistortion = 0x008F0003, //Vita Distortion
		VitaEQ = 0x00900003 //Vita EQ
	}
}
#endif