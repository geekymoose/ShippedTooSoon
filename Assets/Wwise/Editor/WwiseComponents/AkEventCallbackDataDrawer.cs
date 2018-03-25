#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2017 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

[UnityEditor.CustomPropertyDrawer(typeof(AkEventCallbackData))]
internal class AkEventCallbackDataDrawer : UnityEditor.PropertyDrawer
{
	private readonly float callbackDeltaHeight = UnityEditor.EditorGUIUtility.singleLineHeight;
	private readonly float callbackSpacerHeight = 5;
	private readonly float callbackSpacerWidth = 4;

	private readonly float deltaHeight = UnityEditor.EditorGUIUtility.singleLineHeight +
	                                     UnityEditor.EditorGUIUtility.standardVerticalSpacing;

	private readonly float spacerHeight = UnityEditor.EditorGUIUtility.standardVerticalSpacing;

	public override float GetPropertyHeight(UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
	{
		var height = deltaHeight;

		var callbackData = (AkEventCallbackData) property.objectReferenceValue;
		if (callbackData != null)
		{
			height += (callbackDeltaHeight + callbackSpacerHeight) * callbackData.callbackGameObj.Count;
			height += deltaHeight * 2 + spacerHeight * 3;
		}

		return height;
	}

	public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property,
		UnityEngine.GUIContent label)
	{
		// Using BeginProperty / EndProperty on the parent property means that
		// prefab override logic works on the entire property.
		UnityEditor.EditorGUI.BeginProperty(position, label, property);

		var initialRect = position;

		// Draw label
		position = UnityEditor.EditorGUI.PrefixLabel(position,
			UnityEngine.GUIUtility.GetControlID(UnityEngine.FocusType.Passive), new UnityEngine.GUIContent("Use Callback:"));
		position.height = deltaHeight;

		var useCallback = property.objectReferenceValue != null;

		UnityEditor.EditorGUI.BeginChangeCheck();

		useCallback = UnityEngine.GUI.Toggle(position, useCallback, "");

		if (UnityEditor.EditorGUI.EndChangeCheck())
		{
			if (useCallback && property.objectReferenceValue == null)
			{
				var callbackData = UnityEngine.ScriptableObject.CreateInstance<AkEventCallbackData>();
				callbackData.callbackFunc.Add(string.Empty);
				callbackData.callbackFlags.Add(0);
				callbackData.callbackGameObj.Add(null);
				property.objectReferenceValue = callbackData;
			}
			else if (!useCallback && property.objectReferenceValue != null)
			{
				UnityEditor.Undo.RecordObject(property.objectReferenceValue, "Use Callback Change");

				property.objectReferenceValue = null;
				UnityEngine.GUIUtility.keyboardControl = 0;
				UnityEngine.GUIUtility.hotControl = 0;
			}
		}

		if (useCallback)
		{
			position.y += deltaHeight + spacerHeight;
			float removeButtonWidth = 20;
			var callbackFieldsWidth = initialRect.width - removeButtonWidth;
			position.width = callbackFieldsWidth / 3 - callbackSpacerWidth;

			position.x = initialRect.x + 0 * callbackFieldsWidth / 3;
			UnityEngine.GUI.Label(position, "Game Object");

			position.x = initialRect.x + 1 * callbackFieldsWidth / 3;
			UnityEngine.GUI.Label(position, "Callback Function");

			position.x = initialRect.x + 2 * callbackFieldsWidth / 3;
			UnityEngine.GUI.Label(position, "Callback Flags");

			var callbackData = (AkEventCallbackData) property.objectReferenceValue;
			callbackData.uFlags = 0;

			for (var i = 0; i < callbackData.callbackFunc.Count; i++)
			{
				UnityEditor.EditorGUI.BeginChangeCheck();

				position.y += callbackDeltaHeight + callbackSpacerHeight;
				position.x = initialRect.x + 0 * callbackFieldsWidth / 3;
				position.width = callbackFieldsWidth / 3 - callbackSpacerWidth;

				var gameObj = (UnityEngine.GameObject) UnityEditor.EditorGUI.ObjectField(position, callbackData.callbackGameObj[i],
					typeof(UnityEngine.GameObject), true);

				position.x = initialRect.x + 1 * callbackFieldsWidth / 3;
				var func = UnityEditor.EditorGUI.TextField(position, callbackData.callbackFunc[i]);

				position.x = initialRect.x + 2 * callbackFieldsWidth / 3;

				//Since some callback flags are unsupported, some bits are not used.
				//But when using EditorGUILayout.MaskField, clicking the third flag will set the third bit to one even if the third flag in the AkCallbackType enum is unsupported.
				//This is a problem because clicking the third supported flag would internally select the third flag in the AkCallbackType enum which is unsupported.
				//To solve this problem we use a mask for display and another one for the actual callback
				var displayMask = AK.Wwise.Editor.CallbackFlagsDrawer.GetDisplayMask(callbackData.callbackFlags[i]);
				displayMask = UnityEditor.EditorGUI.MaskField(position, displayMask,
					AK.Wwise.Editor.CallbackFlagsDrawer.SupportedCallbackFlags);
				var flags = AK.Wwise.Editor.CallbackFlagsDrawer.GetWwiseCallbackMask(displayMask);

				if (UnityEditor.EditorGUI.EndChangeCheck())
				{
					UnityEditor.Undo.RecordObject(callbackData, "Modified Callback");

					callbackData.callbackGameObj[i] = gameObj;
					callbackData.callbackFunc[i] = func;
					callbackData.callbackFlags[i] = flags;
				}

				position.x = initialRect.x + callbackFieldsWidth;
				position.width = removeButtonWidth;

				if (UnityEngine.GUI.Button(position, "X"))
				{
					UnityEditor.Undo.RecordObject(callbackData, "Remove Callback");

					UnityEngine.GUIUtility.keyboardControl = 0;
					UnityEngine.GUIUtility.hotControl = 0;

					if (callbackData.callbackFunc.Count == 1)
					{
						callbackData.callbackFunc[0] = string.Empty;
						callbackData.callbackFlags[0] = 0;
						callbackData.callbackGameObj[0] = null;
					}
					else
					{
						callbackData.callbackFunc.RemoveAt(i);
						callbackData.callbackFlags.RemoveAt(i);
						callbackData.callbackGameObj.RemoveAt(i);

						i--;
						continue;
					}
				}

				callbackData.uFlags |= callbackData.callbackFlags[i];
			}

			position.x = initialRect.x;
			position.width = initialRect.width;
			position.y += deltaHeight + spacerHeight;

			if (UnityEngine.GUI.Button(position, "Add"))
			{
				UnityEditor.Undo.RecordObject(callbackData, "Add Callback");

				callbackData.callbackFunc.Add(string.Empty);
				callbackData.callbackFlags.Add(0);
				callbackData.callbackGameObj.Add(null);
			}
		}

		UnityEditor.EditorGUI.EndProperty();
	}
}
#endif