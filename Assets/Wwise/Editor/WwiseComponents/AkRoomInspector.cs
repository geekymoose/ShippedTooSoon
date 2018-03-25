#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEditor.CustomEditor(typeof(AkRoom))]
public class AkRoomInspector : UnityEditor.Editor
{
	private AkRoom m_AkRoom;
	private UnityEditor.SerializedProperty priority;

	private UnityEditor.SerializedProperty reverbAuxBus;
	private UnityEditor.SerializedProperty reverbLevel;
	private UnityEditor.SerializedProperty wallOcclusion;

	private void OnEnable()
	{
		m_AkRoom = target as AkRoom;

        reverbAuxBus = serializedObject.FindProperty("reverbAuxBus");
        reverbLevel = serializedObject.FindProperty("reverbLevel");
        wallOcclusion = serializedObject.FindProperty("wallOcclusion");
        priority = serializedObject.FindProperty("priority");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

		using (new UnityEditor.EditorGUILayout.VerticalScope("box"))
		{
			UnityEditor.EditorGUILayout.PropertyField(reverbAuxBus);
			UnityEditor.EditorGUILayout.PropertyField(reverbLevel);
			UnityEditor.EditorGUILayout.PropertyField(wallOcclusion);
			UnityEditor.EditorGUILayout.PropertyField(priority);
		}

        AkGameObjectInspector.RigidbodyCheck(m_AkRoom.gameObject);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif