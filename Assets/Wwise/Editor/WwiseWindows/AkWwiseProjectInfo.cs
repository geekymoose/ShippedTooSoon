#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

public static class AkWwiseProjectInfo
{
	private const string WwiseEditorProjectDataDirectory = "Wwise/Editor/ProjectData";

	private const string AssetsWwiseProjectDataPath =
		"Assets/" + WwiseEditorProjectDataDirectory + "/AkWwiseProjectData.asset";

	public static AkWwiseProjectData m_Data;

	public static AkWwiseProjectData GetData()
	{
		if (m_Data == null && System.IO.Directory.Exists(System.IO.Path.Combine(UnityEngine.Application.dataPath, "Wwise")))
		{
			try
			{
				m_Data = UnityEditor.AssetDatabase.LoadAssetAtPath<AkWwiseProjectData>(AssetsWwiseProjectDataPath);

				if (m_Data == null)
				{
					if (!System.IO.Directory.Exists(System.IO.Path.Combine(UnityEngine.Application.dataPath,
						WwiseEditorProjectDataDirectory)))
					{
						System.IO.Directory.CreateDirectory(System.IO.Path.Combine(UnityEngine.Application.dataPath,
							WwiseEditorProjectDataDirectory));
					}

					m_Data = UnityEngine.ScriptableObject.CreateInstance<AkWwiseProjectData>();
					var assetPathAndName = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(AssetsWwiseProjectDataPath);
					UnityEditor.AssetDatabase.CreateAsset(m_Data, assetPathAndName);
				}
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.Log("WwiseUnity: Unable to load Wwise Data: " + e);
			}
		}

		return m_Data;
	}


	public static bool Populate()
	{
		var bDirty = false;
		if (AkWwisePicker.WwiseProjectFound)
		{
			bDirty = AkWwiseWWUBuilder.Populate();
			bDirty |= AkWwiseXMLBuilder.Populate();
			if (bDirty)
				UnityEditor.EditorUtility.SetDirty(GetData());
		}

		return bDirty;
	}
}
#endif