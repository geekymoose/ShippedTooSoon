#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////


// The inspector for AkInitializer is overriden to trap changes to initialization parameters and persist them across scenes.
[UnityEditor.CustomEditor(typeof(AkInitializer))]
public class AkInitializerInspector : UnityEditor.Editor
{
	private AkInitializer m_AkInit;

	//This data is a copy of the AkInitializer parameters.  
	//We need it to reapply the same values to copies of the object in different scenes
	private UnityEditor.SerializedProperty m_basePath;
	private UnityEditor.SerializedProperty m_callbackManagerBufferSize;
	private UnityEditor.SerializedProperty m_defaultPoolSize;
	private UnityEditor.SerializedProperty m_diffractionFlags;
	private UnityEditor.SerializedProperty m_engineLogging;
	private UnityEditor.SerializedProperty m_language;
	private UnityEditor.SerializedProperty m_lowerPoolSize;
	private UnityEditor.SerializedProperty m_maxSoundPropagationDepth;
	private UnityEditor.SerializedProperty m_memoryCutoffThreshold;
	private UnityEditor.SerializedProperty m_monitorPoolSize;
	private UnityEditor.SerializedProperty m_monitorQueuePoolSize;
	private UnityEditor.SerializedProperty m_preparePoolSize;
	private UnityEditor.SerializedProperty m_spatialAudioPoolSize;
	private UnityEditor.SerializedProperty m_streamingPoolSize;

	private void OnEnable()
	{
		m_AkInit = target as AkInitializer;

		m_basePath = serializedObject.FindProperty("basePath");
		m_language = serializedObject.FindProperty("language");
		m_defaultPoolSize = serializedObject.FindProperty("defaultPoolSize");
		m_lowerPoolSize = serializedObject.FindProperty("lowerPoolSize");
		m_streamingPoolSize = serializedObject.FindProperty("streamingPoolSize");
		m_preparePoolSize = serializedObject.FindProperty("preparePoolSize");
		m_memoryCutoffThreshold = serializedObject.FindProperty("memoryCutoffThreshold");
		m_monitorPoolSize = serializedObject.FindProperty("monitorPoolSize");
		m_monitorQueuePoolSize = serializedObject.FindProperty("monitorQueuePoolSize");
		m_callbackManagerBufferSize = serializedObject.FindProperty("callbackManagerBufferSize");
		m_engineLogging = serializedObject.FindProperty("engineLogging");
		m_spatialAudioPoolSize = serializedObject.FindProperty("spatialAudioPoolSize");
		m_maxSoundPropagationDepth = serializedObject.FindProperty("maxSoundPropagationDepth");
		m_diffractionFlags = serializedObject.FindProperty("diffractionFlags");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		UnityEngine.GUILayout.BeginVertical();
		UnityEditor.EditorGUILayout.PropertyField(m_basePath, new UnityEngine.GUIContent("Base Path"));
		UnityEditor.EditorGUILayout.PropertyField(m_language, new UnityEngine.GUIContent("Language"));
		UnityEditor.EditorGUILayout.PropertyField(m_defaultPoolSize, new UnityEngine.GUIContent("Default Pool Size (KB)"));
		UnityEditor.EditorGUILayout.PropertyField(m_lowerPoolSize, new UnityEngine.GUIContent("Lower Pool Size (KB)"));
		UnityEditor.EditorGUILayout.PropertyField(m_streamingPoolSize,
			new UnityEngine.GUIContent("Streaming Pool Size (KB)"));
		UnityEditor.EditorGUILayout.PropertyField(m_preparePoolSize, new UnityEngine.GUIContent("Prepare Pool Size (KB)"));
		UnityEditor.EditorGUILayout.PropertyField(m_memoryCutoffThreshold,
			new UnityEngine.GUIContent("Memory Cutoff Threshold"));
		UnityEditor.EditorGUILayout.PropertyField(m_monitorPoolSize, new UnityEngine.GUIContent("Monitor Pool Size (KB)"));
		UnityEditor.EditorGUILayout.PropertyField(m_monitorQueuePoolSize,
			new UnityEngine.GUIContent("Monitor Queue Pool Size (KB)"));
		UnityEditor.EditorGUILayout.PropertyField(m_callbackManagerBufferSize,
			new UnityEngine.GUIContent("CallbackManager Buffer Size (KB)"));
		UnityEditor.EditorGUILayout.PropertyField(m_engineLogging, new UnityEngine.GUIContent("Enable Wwise engine logging"));
		UnityEngine.GUILayout.EndVertical();

		UnityEngine.GUILayout.Space(10);

		UnityEditor.EditorGUILayout.LabelField("Spatial Audio Settings", UnityEditor.EditorStyles.boldLabel);
		UnityEditor.EditorGUILayout.PropertyField(m_spatialAudioPoolSize,
			new UnityEngine.GUIContent("Spatial Audio Pool Size (KB)"));
		UnityEditor.EditorGUILayout.PropertyField(m_maxSoundPropagationDepth,
			new UnityEngine.GUIContent("Max Sound Propagation Depth"));

		var displayMask = GetDisplayMask(m_diffractionFlags.intValue);
		displayMask =
			UnityEditor.EditorGUILayout.MaskField(new UnityEngine.GUIContent("Diffraction Flags"), displayMask, SupportedFlags);
		m_diffractionFlags.intValue = GetWwiseMask(displayMask);

		serializedObject.ApplyModifiedProperties();
		if (UnityEngine.GUI.changed)
			AkWwiseProjectInfo.GetData().SaveInitSettings(m_AkInit);
	}

	#region Diffraction Flags

	private static string[] m_supportedFlags;
	private static int[] m_supportedValues;

	private static void SetupSupportedValuesAndFlags()
	{
		var types = (int[]) System.Enum.GetValues(typeof(AkDiffractionFlags));
		int[] unsupportedValues = { (int) AkDiffractionFlags.DefaultDiffractionFlags };

		m_supportedFlags = new string[types.Length - unsupportedValues.Length];
		m_supportedValues = new int[types.Length - unsupportedValues.Length];

		var index = 0;
		for (var i = 0; i < types.Length; i++)
		{
			if (!Contains(unsupportedValues, types[i]))
			{
				m_supportedFlags[index] = System.Enum.GetName(typeof(AkDiffractionFlags), types[i]).Substring(17);
				m_supportedValues[index] = types[i];
				index++;
			}
		}
	}

	private static bool Contains(int[] in_array, int in_value)
	{
		for (var i = 0; i < in_array.Length; i++)
		{
			if (in_array[i] == in_value)
				return true;
		}

		return false;
	}

	public static string[] SupportedFlags
	{
		get
		{
			if (m_supportedFlags == null)
				SetupSupportedValuesAndFlags();

			return m_supportedFlags;
		}
	}

	public static int GetDisplayMask(int in_wwiseMask)
	{
		if (m_supportedValues == null)
			SetupSupportedValuesAndFlags();

		var displayMask = 0;
		for (var i = 0; i < m_supportedValues.Length; i++)
		{
			if ((m_supportedValues[i] & in_wwiseMask) != 0)
				displayMask |= 1 << i;
		}

		return displayMask;
	}

	public static int GetWwiseMask(int in_displayMask)
	{
		if (m_supportedValues == null)
			SetupSupportedValuesAndFlags();

		var wwiseMask = 0;
		for (var i = 0; i < m_supportedValues.Length; i++)
		{
			if ((in_displayMask & (1 << i)) != 0)
				wwiseMask |= m_supportedValues[i];
		}

		return wwiseMask;
	}

	#endregion Diffraction Flags
}
#endif