#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
public class AkSoundEngineController
{
	#region Public Data Members

	public static readonly string s_DefaultBasePath = System.IO.Path.Combine("Audio", "GeneratedSoundBanks");
	public static string s_Language = "English(US)";
	public static int s_DefaultPoolSize = 4096;
	public static int s_LowerPoolSize = 2048;
	public static int s_StreamingPoolSize = 1024;
	public static int s_PreparePoolSize = 0;
	public static float s_MemoryCutoffThreshold = 0.95f;
	public static int s_MonitorPoolSize = 128;
	public static int s_MonitorQueuePoolSize = 64;
	public static int s_CallbackManagerBufferSize = 4;
	public static bool s_EngineLogging = true;
	public static int s_SpatialAudioPoolSize = 4096;

	public string basePath = s_DefaultBasePath;
	public string language = s_Language;
	public bool engineLogging = s_EngineLogging;

	#endregion

	private static AkSoundEngineController ms_Instance;

	public static AkSoundEngineController Instance
	{
		get
		{
			if (ms_Instance == null)
				ms_Instance = new AkSoundEngineController();

			return ms_Instance;
		}
	}

	~AkSoundEngineController()
	{
		if (ms_Instance == this)
		{
#if UNITY_EDITOR
#if UNITY_2017_2_OR_NEWER
			UnityEditor.EditorApplication.pauseStateChanged -= OnPauseStateChanged;
#else
			UnityEditor.EditorApplication.playmodeStateChanged -= OnEditorPlaymodeStateChanged;
#endif
			UnityEditor.EditorApplication.update -= LateUpdate;
#endif
			ms_Instance = null;
		}

		// Do nothing. AkTerminator handles sound engine termination.
	}

	public static string GetDecodedBankFolder()
	{
		return "DecodedBanks";
	}

	public static string GetDecodedBankFullPath()
	{
#if (UNITY_ANDROID || UNITY_IOS || UNITY_SWITCH) && !UNITY_EDITOR
// This is for platforms that only have a specific file location for persistent data.
		return System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, GetDecodedBankFolder());
#else
		return System.IO.Path.Combine(AkBasePathGetter.GetPlatformBasePath(), GetDecodedBankFolder());
#endif
	}

	public void LateUpdate()
	{
#if UNITY_EDITOR
		if (!IsSoundEngineLoaded)
			return;
#endif

		//Execute callbacks that occurred in last frame (not the current update)
		AkCallbackManager.PostCallbacks();
		AkBankManager.DoUnloadBanks();
		AkAudioListener.DefaultListeners.Refresh();
		AkSoundEngine.RenderAudio();
	}

	public void Init(AkInitializer akInitializer)
	{
#if UNITY_EDITOR
		if (!WasInitializedInPlayMode(akInitializer))
			return;

		var arguments = System.Environment.GetCommandLineArgs();
		if (System.Array.IndexOf(arguments, "-nographics") >= 0 &&
		    System.Array.IndexOf(arguments, "-wwiseEnableWithNoGraphics") < 0)
			return;

		var isInitialized = false;
		try
		{
			isInitialized = AkSoundEngine.IsInitialized();
			IsSoundEngineLoaded = true;
		}
		catch (System.DllNotFoundException)
		{
			IsSoundEngineLoaded = false;
			UnityEngine.Debug.LogWarning("WwiseUnity: AkSoundEngine is not loaded.");
			return;
		}
#else
		var isInitialized = AkSoundEngine.IsInitialized();
#endif

		engineLogging = akInitializer.engineLogging;

		AkLogger.Instance.Init();

		AKRESULT result;
		uint BankID;
		if (isInitialized)
		{
#if UNITY_EDITOR
			if (UnityEngine.Application.isPlaying || UnityEditor.BuildPipeline.isBuildingPlayer)
			{
				AkSoundEngine.ClearBanks();
				AkBankManager.Reset();

				result = AkSoundEngine.LoadBank("Init.bnk", AkSoundEngine.AK_DEFAULT_POOL_ID, out BankID);
				if (result != AKRESULT.AK_Success)
					UnityEngine.Debug.LogError("WwiseUnity: Failed load Init.bnk with result: " + result);
			}

			result = AkCallbackManager.Init(akInitializer.callbackManagerBufferSize * 1024);
			if (result != AKRESULT.AK_Success)
			{
				UnityEngine.Debug.LogError("WwiseUnity: Failed to initialize Callback Manager. Terminate sound engine.");
				AkSoundEngine.Term();
				return;
			}

			UnityEditor.EditorApplication.update += LateUpdate;
#endif
			return;
		}

#if UNITY_EDITOR
		if (UnityEditor.BuildPipeline.isBuildingPlayer)
			return;
#endif

		UnityEngine.Debug.Log("WwiseUnity: Initialize sound engine ...");
		basePath = akInitializer.basePath;
		language = akInitializer.language;

		//Use default properties for most SoundEngine subsystem.  
		//The game programmer should modify these when needed.  See the Wwise SDK documentation for the initialization.
		//These settings may very well change for each target platform.
		var memSettings = new AkMemSettings();
		memSettings.uMaxNumPools = 20;

		var deviceSettings = new AkDeviceSettings();
		AkSoundEngine.GetDefaultDeviceSettings(deviceSettings);

		var streamingSettings = new AkStreamMgrSettings();
		streamingSettings.uMemorySize = (uint) akInitializer.streamingPoolSize * 1024;

		var initSettings = new AkInitSettings();
		AkSoundEngine.GetDefaultInitSettings(initSettings);
		initSettings.uDefaultPoolSize = (uint) akInitializer.defaultPoolSize * 1024;
		initSettings.uMonitorPoolSize = (uint) akInitializer.monitorPoolSize * 1024;
		initSettings.uMonitorQueuePoolSize = (uint) akInitializer.monitorQueuePoolSize * 1024;
#if (!UNITY_ANDROID && !UNITY_WSA) || UNITY_EDITOR // Exclude WSA. It only needs the name of the DLL, and no path.
		initSettings.szPluginDLLPath = System.IO.Path.Combine(UnityEngine.Application.dataPath,
			"Plugins" + System.IO.Path.DirectorySeparatorChar);
#endif

		var platformSettings = new AkPlatformInitSettings();
		AkSoundEngine.GetDefaultPlatformInitSettings(platformSettings);
		platformSettings.uLEngineDefaultPoolSize = (uint) akInitializer.lowerPoolSize * 1024;
		platformSettings.fLEngineDefaultPoolRatioThreshold = akInitializer.memoryCutoffThreshold;

		var musicSettings = new AkMusicSettings();
		AkSoundEngine.GetDefaultMusicSettings(musicSettings);

		var spatialAudioSettings = new AkSpatialAudioInitSettings();
		spatialAudioSettings.uPoolSize = (uint) akInitializer.spatialAudioPoolSize * 1024;
		spatialAudioSettings.uMaxSoundPropagationDepth = akInitializer.maxSoundPropagationDepth;
		spatialAudioSettings.uDiffractionFlags = (uint) akInitializer.diffractionFlags;

#if UNITY_EDITOR
		AkSoundEngine.SetGameName(UnityEngine.Application.productName + " (Editor)");
#else
		AkSoundEngine.SetGameName(UnityEngine.Application.productName);
#endif

		result = AkSoundEngine.Init(memSettings, streamingSettings, deviceSettings, initSettings, platformSettings,
			musicSettings, spatialAudioSettings, (uint) akInitializer.preparePoolSize * 1024);

		if (result != AKRESULT.AK_Success)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Failed to initialize the sound engine. Abort.");
			AkSoundEngine.Term();
			return; //AkSoundEngine.Init should have logged more details.
		}

		var basePathToSet = AkBasePathGetter.GetSoundbankBasePath();
		if (string.IsNullOrEmpty(basePathToSet))
		{
			UnityEngine.Debug.LogError("WwiseUnity: Couldn't find soundbanks base path. Terminate sound engine.");
			AkSoundEngine.Term();
			return;
		}

		result = AkSoundEngine.SetBasePath(basePathToSet);
		if (result != AKRESULT.AK_Success)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Failed to set soundbanks base path. Terminate sound engine.");
			AkSoundEngine.Term();
			return;
		}

#if !UNITY_SWITCH
		// Calling Application.persistentDataPath crashes Switch
		var decodedBankFullPath = GetDecodedBankFullPath();
		// AkSoundEngine.SetDecodedBankPath creates the folders for writing to (if they don't exist)
		AkSoundEngine.SetDecodedBankPath(decodedBankFullPath);
#endif

		AkSoundEngine.SetCurrentLanguage(language);

#if !UNITY_SWITCH
		// Calling Application.persistentDataPath crashes Switch
		// AkSoundEngine.AddBasePath is currently only implemented for iOS and Android; No-op for all other platforms.
		AkSoundEngine.AddBasePath(UnityEngine.Application.persistentDataPath + System.IO.Path.DirectorySeparatorChar);
		// Adding decoded bank path last to ensure that it is the first one used when writing decoded banks.
		AkSoundEngine.AddBasePath(decodedBankFullPath);
#endif

		result = AkCallbackManager.Init(akInitializer.callbackManagerBufferSize * 1024);
		if (result != AKRESULT.AK_Success)
		{
			UnityEngine.Debug.LogError("WwiseUnity: Failed to initialize Callback Manager. Terminate sound engine.");
			AkSoundEngine.Term();
			return;
		}

		AkBankManager.Reset();

		UnityEngine.Debug.Log("WwiseUnity: Sound engine initialized.");

		//Load the init bank right away.  Errors will be logged automatically.
		result = AkSoundEngine.LoadBank("Init.bnk", AkSoundEngine.AK_DEFAULT_POOL_ID, out BankID);
		if (result != AKRESULT.AK_Success)
			UnityEngine.Debug.LogError("WwiseUnity: Failed load Init.bnk with result: " + result);

#if UNITY_EDITOR
#if UNITY_2017_2_OR_NEWER
		UnityEditor.EditorApplication.pauseStateChanged += OnPauseStateChanged;
#else
		UnityEditor.EditorApplication.playmodeStateChanged += OnEditorPlaymodeStateChanged;
#endif

		OnEnableEditorListener(akInitializer.gameObject);
		UnityEditor.EditorApplication.update += LateUpdate;
#endif
	}

	public void OnDisable()
	{
#if UNITY_EDITOR
		if (!IsSoundEngineLoaded)
			return;

		OnDisableEditorListener();
#endif
	}

	public void Terminate()
	{
#if UNITY_EDITOR
		ClearInitializeState();

		if (!IsSoundEngineLoaded)
			return;
#endif

		if (!AkSoundEngine.IsInitialized())
			return;

		// Stop everything, and make sure the callback buffer is empty. We try emptying as much as possible, and wait 10 ms before retrying.
		// Callbacks can take a long time to be posted after the call to RenderAudio().
		AkSoundEngine.StopAll();
		AkSoundEngine.ClearBanks();
		AkSoundEngine.RenderAudio();
		var retry = 5;
		do
		{
			var numCB = 0;
			do
			{
				numCB = AkCallbackManager.PostCallbacks();

				// This is a WSA-friendly sleep
				using (System.Threading.EventWaitHandle tmpEvent = new System.Threading.ManualResetEvent(false))
				{
					tmpEvent.WaitOne(System.TimeSpan.FromMilliseconds(1));
				}
			}
			while (numCB > 0);

			// This is a WSA-friendly sleep
			using (System.Threading.EventWaitHandle tmpEvent = new System.Threading.ManualResetEvent(false))
			{
				tmpEvent.WaitOne(System.TimeSpan.FromMilliseconds(10));
			}

			retry--;
		}
		while (retry > 0);

		AkSoundEngine.Term();

		// Make sure we have no callbacks left after Term. Some might be posted during termination.
		AkCallbackManager.PostCallbacks();

		AkCallbackManager.Term();
		AkBankManager.Reset();
	}

#if UNITY_EDITOR
	public bool IsSoundEngineLoaded { get; set; }

	// Enable/Disable the audio when pressing play/pause in the editor.
#if UNITY_2017_2_OR_NEWER
	private static void OnPauseStateChanged(UnityEditor.PauseState pauseState)
	{
		ActivateAudio(pauseState != UnityEditor.PauseState.Paused);
	}
#else
	private static void OnEditorPlaymodeStateChanged()
	{
		ActivateAudio(!UnityEditor.EditorApplication.isPaused);
	}
#endif

#elif !UNITY_IOS
//Keep out of UNITY_EDITOR because the sound needs to keep playing when switching windows (remote debugging in Wwise, for example).
//On iOS, application interruptions are handled in the sound engine already.
	void OnApplicationPause(bool pauseStatus) 
	{
		ActivateAudio(!pauseStatus);
	}

	void OnApplicationFocus(bool focus)
	{
		ActivateAudio(focus);
	}
#endif

#if UNITY_EDITOR || !UNITY_IOS
	private static void ActivateAudio(bool activate)
	{
		if (AkSoundEngine.IsInitialized())
		{
			if (activate)
				AkSoundEngine.WakeupFromSuspend();
			else
				AkSoundEngine.Suspend();

			AkSoundEngine.RenderAudio();
		}
	}
#endif

#if UNITY_EDITOR
	#region Editor Listener

	private UnityEngine.GameObject editorListenerGameObject;

	private void OnEnableEditorListener(UnityEngine.GameObject gameObject)
	{
		if (!UnityEngine.Application.isPlaying && AkSoundEngine.IsInitialized() && editorListenerGameObject == null)
		{
			editorListenerGameObject = gameObject;

			// Clearing the isDirty flag of the AkAudioListener list.
			AkAudioListener.DefaultListeners.Refresh();

			AkSoundEngine.RegisterGameObj(editorListenerGameObject, editorListenerGameObject.name);

			var id = AkSoundEngine.GetAkGameObjectID(editorListenerGameObject);
			AkSoundEnginePINVOKE.CSharp_AddDefaultListener(id);

			UnityEditor.EditorApplication.update += UpdateEditorListenerPosition;
		}
	}

	private void OnDisableEditorListener()
	{
		if (!UnityEngine.Application.isPlaying && AkSoundEngine.IsInitialized() && editorListenerGameObject != null)
		{
			UnityEditor.EditorApplication.update -= UpdateEditorListenerPosition;

			var id = AkSoundEngine.GetAkGameObjectID(editorListenerGameObject);
			AkSoundEnginePINVOKE.CSharp_RemoveDefaultListener(id);

			AkSoundEngine.UnregisterGameObj(editorListenerGameObject);
			editorListenerGameObject = null;
		}
	}

	private void UpdateEditorListenerPosition()
	{
		if (!UnityEngine.Application.isPlaying && AkSoundEngine.IsInitialized() &&
		    UnityEditor.SceneView.lastActiveSceneView != null && UnityEditor.SceneView.lastActiveSceneView.camera != null)
		{
			AkSoundEngine.SetObjectPosition(editorListenerGameObject,
				UnityEditor.SceneView.lastActiveSceneView.camera.transform);
		}
	}

	#endregion

	#region Initialize only once
	private readonly System.Collections.Generic.List<AkInitializer> AkInitializers = new System.Collections.Generic.List<AkInitializer>();

	private bool WasInitializedInPlayMode(AkInitializer akInitializer)
	{
		if (UnityEngine.Application.isPlaying)
		{
			if (AkInitializers.Contains(akInitializer))
				return false;

			AkInitializers.Add(akInitializer);
			return AkInitializers.Count == 1;
		}

		return true;
	}

	private void ClearInitializeState()
	{
		AkInitializers.Clear();
	}
	#endregion
#endif // UNITY_EDITOR

}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.