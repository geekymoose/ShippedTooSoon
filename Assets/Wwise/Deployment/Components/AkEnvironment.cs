#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEngine.AddComponentMenu("Wwise/AkEnvironment")]
[UnityEngine.RequireComponent(typeof(UnityEngine.Collider))]
/// @brief Use this component to define a reverb zone.  This needs to be added to a collider object to work properly. \ref unity_use_AkEvironment_AkEvironmentPortal
/// @details This component can be attached to any collider.  You can specify a roll-off to fade-in/out of the reverb.  
/// The reverb parameters will be defined in the Wwise project, by the sound designer.  All AkGameObj that are 
/// "environment"-aware will receive a send value when entering the attached collider.
/// \sa
/// - \ref unity_use_AkEvironment_AkEvironmentPortal
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=integrating__elements__environments.html" target="_blank">Integrating Environments and Game-defined Auxiliary Sends</a> (Note: This is described in the Wwise SDK documentation.)
/// - <a href="https://www.audiokinetic.com/library/edge/?source=SDK&id=namespace_a_k_1_1_sound_engine_a18f56e8e0e881c4efb9080545efbb233.html#a18f56e8e0e881c4efb9080545efbb233" target="_blank">AK::SoundEngine::SetGameObjectAuxSendValues</a> (Note: This is described in the Wwise SDK documentation.)
[UnityEngine.ExecuteInEditMode]
public class AkEnvironment : UnityEngine.MonoBehaviour
{
	public const int MAX_NB_ENVIRONMENTS = 4;

	public static AkEnvironment_CompareByPriority s_compareByPriority = new AkEnvironment_CompareByPriority();

	public static AkEnvironment_CompareBySelectionAlgorithm s_compareBySelectionAlgorithm =
		new AkEnvironment_CompareBySelectionAlgorithm();

	//if excludeOthers, then only the environment with the excludeOthers flag set to true and with the highest priority will be active
	public bool excludeOthers = false;

	//if isDefault, then this environment will be bumped out if any other is present 
	public bool isDefault = false;

	public int m_auxBusID;

	//Cache of the colliders for this environment, to avoid calls to GetComponent.
	private UnityEngine.Collider m_Collider;

	//smaller number has a higher priority
	public int priority = 0;

#if UNITY_EDITOR
	public byte[] valueGuid = new byte[16];
#endif

	public uint GetAuxBusID()
	{
		return (uint) m_auxBusID;
	}

	public void SetAuxBusID(int in_auxBusID)
	{
		m_auxBusID = in_auxBusID;
	}

	public float GetAuxSendValueForPosition(UnityEngine.Vector3 in_position)
	{
		return 1;
	}

	public UnityEngine.Collider GetCollider()
	{
		return m_Collider;
	}

	public void Awake()
	{
		m_Collider = GetComponent<UnityEngine.Collider>();
	}

	/// Sorts AkEnvironment's based on their priorities.
	public class AkEnvironment_CompareByPriority : System.Collections.Generic.IComparer<AkEnvironment>
	{
		public virtual int Compare(AkEnvironment a, AkEnvironment b)
		{
			var result = a.priority.CompareTo(b.priority);
			return result == 0 && a != b ? 1 : result;
		}
	}

	/// The selection algorithm is as follow: 
	/// -# Environments have priorities.
	/// -# Environments have a "Default" flag. This flag effectively says that this environment will be bumped out if any other is present.
	/// -# Environments have an "Exclude Other" flag. This flag will tell that this env is not overlappable with others. So, only one (the highest priority) should be selected.
	public class AkEnvironment_CompareBySelectionAlgorithm : AkEnvironment_CompareByPriority
	{
		public override int Compare(AkEnvironment a, AkEnvironment b)
		{
			if (a.isDefault)
				return b.isDefault ? base.Compare(a, b) : 1;

			if (b.isDefault)
				return -1;

			if (a.excludeOthers)
				return b.excludeOthers ? base.Compare(a, b) : -1;

			return b.excludeOthers ? 1 : base.Compare(a, b);
		}
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.