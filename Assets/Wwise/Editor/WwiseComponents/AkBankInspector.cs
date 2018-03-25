#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEditor.CanEditMultipleObjects]
[UnityEditor.CustomEditor(typeof(AkBank))]
public class AkBankInspector : AkBaseInspector
{
	private UnityEditor.SerializedProperty bankName;
	private UnityEditor.SerializedProperty decode;
	private UnityEditor.SerializedProperty loadAsync;

	private readonly AkUnityEventHandlerInspector m_LoadBankEventHandlerInspector = new AkUnityEventHandlerInspector();
	private readonly AkUnityEventHandlerInspector m_UnloadBankEventHandlerInspector = new AkUnityEventHandlerInspector();
	private UnityEditor.SerializedProperty saveDecoded;

	private void OnEnable()
	{
		m_LoadBankEventHandlerInspector.Init(serializedObject, "triggerList", "Load On: ", false);
		m_UnloadBankEventHandlerInspector.Init(serializedObject, "unloadTriggerList", "Unload On: ", false);

		bankName = serializedObject.FindProperty("bankName");
		loadAsync = serializedObject.FindProperty("loadAsynchronous");
		decode = serializedObject.FindProperty("decodeBank");
		saveDecoded = serializedObject.FindProperty("saveDecodedBank");

		m_guidProperty = new[] { serializedObject.FindProperty("valueGuid.Array") };

		//Needed by the base class to know which type of component its working with
		m_typeName = "Bank";
		m_objectType = AkWwiseProjectData.WwiseObjectType.SOUNDBANK;
	}

	public override void OnChildInspectorGUI()
	{
		serializedObject.Update();

		m_LoadBankEventHandlerInspector.OnGUI();
		m_UnloadBankEventHandlerInspector.OnGUI();

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		UnityEngine.GUILayout.BeginVertical("Box");
		{
			var oldDecodeValue = decode.boolValue;
			var oldSaveDecodedValue = saveDecoded.boolValue;
			UnityEditor.EditorGUILayout.PropertyField(loadAsync, new UnityEngine.GUIContent("Asynchronous:"));
			UnityEditor.EditorGUILayout.PropertyField(decode, new UnityEngine.GUIContent("Decode compressed data:"));

			if (decode.boolValue)
			{
				if (decode.boolValue != oldDecodeValue && AkWwiseProjectInfo.GetData().preparePoolSize == 0)
					UnityEditor.EditorUtility.DisplayDialog("Warning",
						"You will need to define a prepare pool size in the AkInitializer component options.", "Ok");
				UnityEditor.EditorGUILayout.PropertyField(saveDecoded, new UnityEngine.GUIContent("Save decoded bank:"));
				if (oldSaveDecodedValue && !saveDecoded.boolValue)
				{
					var decodedBankPath =
						System.IO.Path.Combine(AkSoundEngineController.GetDecodedBankFullPath(), bankName.stringValue + ".bnk");
					try
					{
						System.IO.File.Delete(decodedBankPath);
					}
					catch (System.Exception e)
					{
						UnityEngine.Debug.Log("WwiseUnity: Could not delete existing decoded SoundBank. Please delete it manually. " + e);
					}
				}
			}
		}
		UnityEngine.GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
	}

	public override string UpdateIds(System.Guid[] in_guid)
	{
		for (var i = 0; i < AkWwiseProjectInfo.GetData().BankWwu.Count; i++)
		{
			var bank = AkWwiseProjectInfo.GetData().BankWwu[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[0]));

			if (bank != null)
			{
				serializedObject.Update();
				bankName.stringValue = bank.Name;
				serializedObject.ApplyModifiedProperties();

				return bank.Name;
			}
		}

		return string.Empty;
	}
}
#endif