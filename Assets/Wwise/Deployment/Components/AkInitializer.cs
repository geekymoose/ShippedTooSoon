#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEngine.AddComponentMenu("Wwise/AkInitializer")]
[UnityEngine.RequireComponent(typeof(AkTerminator))]
[UnityEngine.DisallowMultipleComponent]
/// This script deals with initialization, and frame updates of the Wwise audio engine.  
/// It is marked as \c DontDestroyOnLoad so it stays active for the life of the game, 
/// not only one scene. You can, and probably should, modify this script to change the 
/// initialization parameters for the sound engine. A few are already exposed in the property inspector.
/// It must be present on one Game Object at the beginning of the game to initialize the audio properly.
/// It must be executed BEFORE any other MonoBehaviors that use AkSoundEngine.
/// \sa
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=workingwithsdks__initialization.html" target="_blank">Initialize the Different Modules of the Sound Engine</a> (Note: This is described in the Wwise SDK documentation.)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=namespace_a_k_1_1_sound_engine_a27257629833b9481dcfdf5e793d9d037.html#a27257629833b9481dcfdf5e793d9d037" target="_blank">AK::SoundEngine::Init()</a> (Note: This is described in the Wwise SDK documentation.)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=namespace_a_k_1_1_sound_engine_a9176602bbe972da4acc1f8ebdb37f2bf.html#a9176602bbe972da4acc1f8ebdb37f2bf" target="_blank">AK::SoundEngine::Term()</a> (Note: This is described in the Wwise SDK documentation.)
/// - AkCallbackManager
[UnityEngine.ExecuteInEditMode]
public class AkInitializer : UnityEngine.MonoBehaviour
{
	#region Public Data Members

	///Path for the soundbanks. This must contain one sub folder per platform, with the same as in the Wwise project.
	public string basePath = AkSoundEngineController.s_DefaultBasePath;

	/// Language sub-folder.
	public string language = AkSoundEngineController.s_Language;

	///Default Pool size.  This contains the meta data for your audio project.  Default size is 4 MB, but you should adjust for your needs.
	public int defaultPoolSize = AkSoundEngineController.s_DefaultPoolSize;

	///Lower Pool size.  This contains the audio processing buffers and DSP data.  Default size is 2 MB, but you should adjust for your needs.
	public int lowerPoolSize = AkSoundEngineController.s_LowerPoolSize;

	///Streaming Pool size.  This contains the streaming buffers.  Default size is 1 MB, but you should adjust for your needs.
	public int streamingPoolSize = AkSoundEngineController.s_StreamingPoolSize;

	///Prepare Pool size.  This contains the banks loaded using PrepareBank (Banks decoded on load use this).  Default size is 0 MB, but you should adjust for your needs.
	public int preparePoolSize = AkSoundEngineController.s_PreparePoolSize;

	///This setting will trigger the killing of sounds when the memory is reaching 95% of capacity.  Lowest priority sounds are killed.
	public float memoryCutoffThreshold = AkSoundEngineController.s_MemoryCutoffThreshold;

	///Monitor Pool size.  Size of the monitoring pool, in bytes. This parameter is not used in Release build.
	public int monitorPoolSize = AkSoundEngineController.s_MonitorPoolSize;

	///Monitor Queue Pool size.  Size of the monitoring queue pool, in bytes. This parameter is not used in Release build.
	public int monitorQueuePoolSize = AkSoundEngineController.s_MonitorQueuePoolSize;

	///CallbackManager buffer size.  The size of the buffer used per-frame to transfer callback data. Default size is 4 KB, but you should increase this, if required.
	public int callbackManagerBufferSize = AkSoundEngineController.s_CallbackManagerBufferSize;

	///Spatial Audio Lower Pool size.  Default size is 4 MB, but you should adjust for your needs.
	public int spatialAudioPoolSize = AkSoundEngineController.s_SpatialAudioPoolSize;

	[UnityEngine.Range(0, AkSoundEngine.AK_MAX_SOUND_PROPAGATION_DEPTH)]
	/// Spatial Audio Max Sound Propagation Depth. Maximum number of rooms that sound can propagate through; must be less than or equal to AK_MAX_SOUND_PROPAGATION_DEPTH.
	public uint maxSoundPropagationDepth = AkSoundEngine.AK_MAX_SOUND_PROPAGATION_DEPTH;

	[UnityEngine.Tooltip("Default Diffraction Flags combine all the diffraction flags")]
	/// Enable or disable specific diffraction features. See AkDiffractionFlags.
	public AkDiffractionFlags diffractionFlags = AkDiffractionFlags.DefaultDiffractionFlags;

	///Enable Wwise engine logging. Option to turn on/off the logging of the Wwise engine.
	public bool engineLogging = AkSoundEngineController.s_EngineLogging;

	#endregion

	public static string GetBasePath()
	{
#if UNITY_EDITOR
		return WwiseSettings.LoadSettings().SoundbankPath;
#else
		return AkSoundEngineController.Instance.basePath;
#endif
	}

	public static string GetCurrentLanguage()
	{
		return AkSoundEngineController.Instance.language;
	}

#if !UNITY_EDITOR
	private void Awake()
	{
		DontDestroyOnLoad(this);
	}
#endif

	private void OnEnable()
	{
		AkSoundEngineController.Instance.Init(this);
	}

	private void OnDisable()
	{
		AkSoundEngineController.Instance.OnDisable();
	}

	//Use LateUpdate instead of Update() to ensure all gameobjects positions, listener positions, environements, RTPC, etc are set before finishing the audio frame.
	private void LateUpdate()
	{
		AkSoundEngineController.Instance.LateUpdate();
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.