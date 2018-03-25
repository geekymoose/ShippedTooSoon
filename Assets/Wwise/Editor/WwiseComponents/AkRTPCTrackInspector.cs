#if UNITY_2017_1_OR_NEWER

[UnityEditor.CustomEditor(typeof(AkRTPCTrack))]
public class AkRTPCTrackInspector : UnityEditor.Editor
{
	private UnityEditor.SerializedProperty Parameter;

	public void OnEnable()
	{
		Parameter = serializedObject.FindProperty("Parameter");
	}

	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		UnityEngine.GUILayout.Space(2);

		UnityEngine.GUILayout.BeginVertical("Box");
		{
			UnityEditor.EditorGUILayout.PropertyField(Parameter, new UnityEngine.GUIContent("Parameter: "));
		}
		UnityEngine.GUILayout.EndVertical();

		serializedObject.ApplyModifiedProperties();
	}
}

#endif //UNITY_2017_1_OR_NEWER