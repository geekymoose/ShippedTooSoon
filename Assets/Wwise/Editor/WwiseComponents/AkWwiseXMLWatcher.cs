#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

public class AkWwiseXMLWatcher
{
	private static AkWwiseXMLWatcher Instance;
	private readonly string SoundBankFolder;
	private readonly System.IO.FileSystemWatcher XmlWatcher;


	private AkWwiseXMLWatcher()
	{
		XmlWatcher = new System.IO.FileSystemWatcher();
		SoundBankFolder = AkBasePathGetter.GetSoundbankBasePath();

		try
		{
			XmlWatcher.Path = SoundBankFolder;
			XmlWatcher.NotifyFilter = System.IO.NotifyFilters.LastWrite;

			// Event handlers that are watching for specific event
			XmlWatcher.Created += RaisePopulateFlag;
			XmlWatcher.Changed += RaisePopulateFlag;

			XmlWatcher.Filter = "*.xml";
			XmlWatcher.IncludeSubdirectories = true;
		}
		catch (System.Exception)
		{
			// Deliberately left empty
		}
	}

	public static AkWwiseXMLWatcher GetInstance()
	{
		if (Instance == null)
			Instance = new AkWwiseXMLWatcher();

		return Instance;
	}

	public void StartXMLWatcher()
	{
		XmlWatcher.EnableRaisingEvents = true;
	}

	public void StopXMLWatcher()
	{
		XmlWatcher.EnableRaisingEvents = false;
	}


	private void RaisePopulateFlag(object sender, System.IO.FileSystemEventArgs e)
	{
		// Signal the main thread it's time to populate (cannot run populate somewhere else than on main thread)
		AkAmbientInspector.populateSoundBank = true;
	}
}
#endif