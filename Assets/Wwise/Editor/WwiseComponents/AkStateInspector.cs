#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEditor.CanEditMultipleObjects]
[UnityEditor.CustomEditor(typeof(AkState))]
public class AkStateInspector : AkBaseInspector
{
	private readonly AkUnityEventHandlerInspector m_UnityEventHandlerInspector = new AkUnityEventHandlerInspector();
	private UnityEditor.SerializedProperty m_groupID;
	private UnityEditor.SerializedProperty m_valueID;

	private void OnEnable()
	{
		m_UnityEventHandlerInspector.Init(serializedObject);

		m_groupID = serializedObject.FindProperty("groupID");
		m_valueID = serializedObject.FindProperty("valueID");

		m_guidProperty = new UnityEditor.SerializedProperty[2];
		m_guidProperty[0] = serializedObject.FindProperty("valueGuid.Array");
		m_guidProperty[1] = serializedObject.FindProperty("groupGuid.Array");

		//Needed by the base class to know which type of component its working with
		m_typeName = "State";
		m_objectType = AkWwiseProjectData.WwiseObjectType.STATE;
	}

	public override void OnChildInspectorGUI()
	{
		serializedObject.Update();

		m_UnityEventHandlerInspector.OnGUI();

		serializedObject.ApplyModifiedProperties();
	}

	public override string UpdateIds(System.Guid[] in_guid)
	{
		var stateName = string.Empty;
		for (var i = 0; i < AkWwiseProjectInfo.GetData().StateWwu.Count; i++)
		{
			var stateGroup = AkWwiseProjectInfo.GetData().StateWwu[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[1]));

			if (stateGroup != null)
			{
				serializedObject.Update();

				stateName = stateGroup.Name + "/";
				m_groupID.intValue = stateGroup.ID;

				var index = stateGroup.ValueGuids.FindIndex(x => new System.Guid(x.bytes).Equals(in_guid[0]));
				m_valueID.intValue = stateGroup.valueIDs[index];

				serializedObject.ApplyModifiedProperties();

				return stateName + stateGroup.values[index];
			}
		}

		return string.Empty;
	}
}
#endif