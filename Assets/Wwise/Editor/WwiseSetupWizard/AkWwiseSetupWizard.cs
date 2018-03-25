#if UNITY_EDITOR

[UnityEditor.InitializeOnLoad]
public class WwiseSetupWizard
{
	public static WwiseSettings Settings;

	static WwiseSetupWizard()
	{
		try
		{
			Settings = WwiseSettings.LoadSettings();
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Failed to load the settings, exception caught: " + e);
		}
	}

	public static void RunModify()
	{
		try
		{
			UnityEngine.Debug.Log("WwiseUnity: Running modify setup...");

			AkSceneUtils.CreateNewScene();

			ModifySetup();

			UnityEngine.Debug.Log("WwiseUnity: Refreshing asset database.");
			UnityEditor.AssetDatabase.Refresh();

			UnityEngine.Debug.Log("WwiseUnity: End of modify setup, exiting Unity.");
			UnityEditor.EditorApplication.Exit(0);
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Exception caught: " + e);
			UnityEditor.EditorApplication.Exit(1);
		}
	}

	public static void RunSetup()
	{
		try
		{
			UnityEngine.Debug.Log("WwiseUnity: Running install setup...");

			AkSceneUtils.CreateNewScene();

			Setup();

			UnityEngine.Debug.Log("WwiseUnity: Refreshing asset database.");
			UnityEditor.AssetDatabase.Refresh();

			UnityEngine.Debug.Log("WwiseUnity: End of setup, exiting Unity.");
			UnityEditor.EditorApplication.Exit(0);
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Exception caught: " + e);
			UnityEditor.EditorApplication.Exit(1);
		}
	}

	public static void RunDemoSceneSetup()
	{
		try
		{
			UnityEngine.Debug.Log("WwiseUnity: Running demo scene setup...");

			AkSceneUtils.CreateNewScene();

			Setup();

			AkSceneUtils.OpenExistingScene("Assets/WwiseDemoScene/WwiseDemoScene.unity");

			UnityEngine.Debug.Log("WwiseUnity: Refreshing asset database.");
			UnityEditor.AssetDatabase.Refresh();

			UnityEngine.Debug.Log("WwiseUnity: End of demo scene setup, exiting Unity.");
			UnityEditor.EditorApplication.Exit(0);
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Exception caught: " + e);
			UnityEditor.EditorApplication.Exit(1);
		}
	}

	private static void UpdateProgressBar(float progress)
	{
		UnityEditor.EditorUtility.DisplayProgressBar("Wwise Integration", "Migration in progress - Please wait...", progress);
	}

	public static void RunMigrate()
	{
		try
		{
			UnityEngine.Debug.Log("WwiseUnity: Running migration setup...");

			UnityEngine.Debug.Log("WwiseUnity: Reading parameters...");

			var arguments = System.Environment.GetCommandLineArgs();
			string migrateStartString = null;
			var indexMigrateStart = System.Array.IndexOf(arguments, "-wwiseInstallMigrateStart");

			if (indexMigrateStart != -1)
				migrateStartString = arguments[indexMigrateStart + 1];
			else
			{
				UnityEngine.Debug.LogError("WwiseUnity: ERROR: Missing parameter wwiseInstallMigrateStart.");
				UnityEditor.EditorApplication.Exit(1);
			}

			string migrateStopString = null;
			var indexMigrateStop = System.Array.IndexOf(arguments, "-wwiseInstallMigrateStop");

			if (indexMigrateStop != -1)
				migrateStopString = arguments[indexMigrateStop + 1];
			else
			{
				UnityEngine.Debug.LogError("WwiseUnity: ERROR: Missing parameter wwiseInstallMigrateStart.");
				UnityEditor.EditorApplication.Exit(1);
			}

			int migrateStart;
			int migrateStop;

			if (!int.TryParse(migrateStartString, out migrateStart))
			{
				UnityEngine.Debug.LogError("WwiseUnity: ERROR: wwiseInstallMigrateStart is not a number.");
				return;
			}

			if (!int.TryParse(migrateStopString, out migrateStop))
			{
				UnityEngine.Debug.LogError("WwiseUnity: ERROR: wwiseInstallMigrateStop is not a number.");
				return;
			}

			PerformMigration(migrateStart, migrateStop);

			UnityEngine.Debug.Log("WwiseUnity: Refreshing asset database.");
			UnityEditor.AssetDatabase.Refresh();

			UnityEngine.Debug.Log("WwiseUnity: End of setup, exiting Unity.");
			UnityEditor.EditorApplication.Exit(0);
		}
		catch (System.Exception e)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Exception caught: " + e);
			UnityEditor.EditorApplication.Exit(1);
		}
	}

	private static void MigrateCurrentScene(System.IO.FileInfo[] files, int migrateStart, int migrateStop)
	{
		var objectTypeMap = new System.Collections.Generic.Dictionary<System.Type, UnityEngine.Object[]>();

		foreach (var file in files)
		{
			var className = System.IO.Path.GetFileNameWithoutExtension(file.Name);

			// Since monobehaviour scripts need to have the same name as the class it contains, we can use it to get the type of the object.
			var objectType = System.Type.GetType(className + ", Assembly-CSharp");

			if (objectType != null && objectType.IsSubclassOf(typeof(UnityEngine.Object)))
			{
				// Get all objects in the scene with the specified type.
				var objects = UnityEngine.Object.FindObjectsOfType(objectType);

				if (objects != null && objects.Length > 0)
					objectTypeMap[objectType] = objects;
			}
		}

		for (var ii = migrateStart; ii <= migrateStop; ++ii)
		{
			var migrationMethodName = "Migrate" + ii;
			var preMigrationMethodName = "PreMigration" + ii;
			var postMigrationMethodName = "PostMigration" + ii;

			foreach (var objectTypePair in objectTypeMap)
			{
				var objectType = objectTypePair.Key;
				var objects = objectTypePair.Value;
				var className = objectType.Name;

				var preMigrationMethodInfo = objectType.GetMethod(preMigrationMethodName,
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				if (preMigrationMethodInfo != null)
				{
					UnityEngine.Debug.Log("WwiseUnity: PreMigration step <" + ii + "> for class <" + className + ">");
					preMigrationMethodInfo.Invoke(null, null);
				}

				var migrationMethodInfo = objectType.GetMethod(migrationMethodName,
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
				if (migrationMethodInfo != null)
				{
					UnityEngine.Debug.Log("WwiseUnity: Migration step <" + ii + "> for class <" + className + ">");

					// Call the migration method of each object.
					foreach (var currentObject in objects)
						migrationMethodInfo.Invoke(currentObject, null);
				}

				var postMigrationMethodInfo = objectType.GetMethod(postMigrationMethodName,
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
				if (postMigrationMethodInfo != null)
				{
					UnityEngine.Debug.Log("WwiseUnity: PostMigration step <" + ii + "> for class <" + className + ">");
					postMigrationMethodInfo.Invoke(null, null);
				}
			}
		}
	}

	public static void PerformMigration(int migrateStart, int migrateStop)
	{
		UpdateProgressBar(0f);

		UnityEngine.Debug.Log("WwiseUnity: Migrating incrementally to versions " + migrateStart + " up to " + migrateStop);

		AkPluginActivator.DeactivateAllPlugins();
		AkPluginActivator.Update();
		AkPluginActivator.ActivatePluginsForEditor();

		// Get the name of the currently opened scene.
		var currentScene = AkSceneUtils.GetCurrentScene().Replace('/', '\\');

		var files =
			new System.IO.DirectoryInfo(UnityEngine.Application.dataPath + "/Wwise/Deployment/Components").GetFiles("*.cs",
				System.IO.SearchOption.AllDirectories);
		var sceneInfo =
			new System.IO.DirectoryInfo(UnityEngine.Application.dataPath).GetFiles("*.unity",
				System.IO.SearchOption.AllDirectories);
		var scenes = new string[sceneInfo.Length];

		AkSceneUtils.CreateNewScene();
		AkUtilities.IsMigrating = true;

		for (var i = 0; i < scenes.Length; i++)
		{
			UpdateProgressBar((float) i / scenes.Length);

			var scene = "Assets" + sceneInfo[i].FullName.Substring(UnityEngine.Application.dataPath.Length);
			UnityEngine.Debug.Log("WwiseUnity: Migrating scene " + scene);

			AkSceneUtils.OpenExistingScene(scene);
			MigrateCurrentScene(files, migrateStart - 1, migrateStop - 1);
			AkSceneUtils.SaveCurrentScene(null);
		}

		UpdateProgressBar(1.0f);

		AkSceneUtils.CreateNewScene();

		AkUtilities.IsMigrating = false;

		// Reopen the scene that was opened before the migration process started.
		AkSceneUtils.OpenExistingScene(currentScene);

		UnityEngine.Debug.Log("WwiseUnity: Removing lock for launcher.");

		// TODO: Moving one folder up is not nice at all. How to find the current project path?
		try
		{
			System.IO.File.Delete(UnityEngine.Application.dataPath + "/../.WwiseLauncherLockFile");
		}
		catch (System.Exception)
		{
			// Ignore if not present.
		}

		UnityEditor.EditorUtility.ClearProgressBar();
	}

	public static void ModifySetup()
	{
		var currentConfig = AkPluginActivator.GetCurrentConfig();

		if (string.IsNullOrEmpty(currentConfig))
			currentConfig = AkPluginActivator.CONFIG_PROFILE;

		AkPluginActivator.DeactivateAllPlugins();
		AkPluginActivator.Update();
		AkPluginActivator.ActivatePluginsForEditor();
	}

	// Perform all necessary steps to use the Wwise Unity integration.
	private static void Setup()
	{
		AkPluginActivator.DeactivateAllPlugins();

		// 0. Make sure the soundbank directory exists
		var sbPath = AkUtilities.GetFullPath(UnityEngine.Application.streamingAssetsPath, Settings.SoundbankPath);
		if (!System.IO.Directory.Exists(sbPath))
			System.IO.Directory.CreateDirectory(sbPath);

		// 1. Disable built-in audio
		if (!DisableBuiltInAudio())
		{
			UnityEngine.Debug.LogWarning(
				"WwiseUnity: Could not disable built-in audio. Please disable built-in audio by going to Project->Project Settings->Audio, and check \"Disable Audio\".");
		}

		// 2. Create a "WwiseGlobal" game object and set the AkSoundEngineInitializer and terminator scripts
		// 3. Set the SoundBank path property on AkSoundEngineInitializer
		CreateWwiseGlobalObject();

		// 5. Add AkAudioListener component to camera
		SetListener();

		// 6. Enable "Run In Background" in PlayerSettings (PlayerSettings.runInbackground property)
		UnityEditor.PlayerSettings.runInBackground = true;

		AkPluginActivator.Update();
		AkPluginActivator.ActivatePluginsForEditor();

		// 9. Activate WwiseIDs file generation, and point Wwise to the Assets/Wwise folder
		// 10. Change the SoundBanks options so it adds Max Radius information in the Wwise project
		if (!SetSoundbankSettings())
			UnityEngine.Debug.LogWarning("WwiseUnity: Could not modify Wwise Project to generate the header file!");

		// 11. Activate XboxOne network sockets.
		AkXboxOneUtils.EnableXboxOneNetworkSockets();
	}

	// Create a Wwise Global object containing the initializer and terminator scripts. Set the soundbank path of the initializer script.
	// This game object will live for the whole project; there is no need to instanciate one per scene.
	private static void CreateWwiseGlobalObject()
	{
		// Look for a game object which has the initializer component
		var AkInitializers = UnityEngine.Object.FindObjectsOfType<AkInitializer>();
		UnityEngine.GameObject WwiseGlobalGameObject = null;
		if (AkInitializers.Length > 0)
			UnityEngine.Object.DestroyImmediate(AkInitializers[0].gameObject);

		WwiseGlobalGameObject = new UnityEngine.GameObject("WwiseGlobal");

		// attach initializer component
		var AkInit = WwiseGlobalGameObject.AddComponent<AkInitializer>();

		// Set the soundbank path property on the initializer
		AkInit.basePath = Settings.SoundbankPath;

		// Set focus on WwiseGlobal
		UnityEditor.Selection.activeGameObject = WwiseGlobalGameObject;
	}

	private static bool DisableBuiltInAudio()
	{
		UnityEditor.SerializedObject audioSettingsAsset = null;
		UnityEditor.SerializedProperty disableAudioProperty = null;

		var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/AudioManager.asset");
		if (assets.Length > 0)
			audioSettingsAsset = new UnityEditor.SerializedObject(assets[0]);

		if (audioSettingsAsset != null)
			disableAudioProperty = audioSettingsAsset.FindProperty("m_DisableAudio");

		if (disableAudioProperty == null)
			return false;

		disableAudioProperty.boolValue = true;
		audioSettingsAsset.ApplyModifiedProperties();
		return true;
	}

	// Disable the built-in audio listener, and add the AkGameObj to the camera
	private static void SetListener()
	{
		var settings = WwiseSettings.LoadSettings();

		// Remove the audio listener script
		if (settings.CreateWwiseListener && UnityEngine.Camera.main != null)
		{
			var listener = UnityEngine.Camera.main.gameObject.GetComponent<UnityEngine.AudioListener>();
			if (listener != null)
				UnityEngine.Object.DestroyImmediate(listener);

			// Add the AkGameObj script
			{
				UnityEngine.Camera.main.gameObject.AddComponent<AkAudioListener>();

				var akGameObj = UnityEngine.Camera.main.gameObject.GetComponent<AkGameObj>();
				akGameObj.isEnvironmentAware = false;
			}
		}
	}

	// Modify the .wproj file to set needed soundbank settings
	private static bool SetSoundbankSettings()
	{
		if (string.IsNullOrEmpty(Settings.WwiseProjectPath))
			return true;

		var r = new System.Text.RegularExpressions.Regex("_WwiseIntegrationTemp.*?([/\\\\])");
		var SoundbankPath = AkUtilities.GetFullPath(r.Replace(UnityEngine.Application.streamingAssetsPath, "$1"),
			Settings.SoundbankPath);
		var WprojPath = AkUtilities.GetFullPath(UnityEngine.Application.dataPath, Settings.WwiseProjectPath);
#if UNITY_EDITOR_OSX
		SoundbankPath = "Z:" + SoundbankPath;
#endif

		if (AkUtilities.EnableBoolSoundbankSettingInWproj("SoundBankGenerateHeaderFile", WprojPath))
		{
			if (AkUtilities.SetSoundbankHeaderFilePath(WprojPath, SoundbankPath))
				return AkUtilities.EnableBoolSoundbankSettingInWproj("SoundBankGenerateMaxAttenuationInfo", WprojPath);
		}

		return false;
	}
}

#endif // UNITY_EDITOR