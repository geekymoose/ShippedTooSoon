#if UNITY_EDITOR
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2014 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

public abstract class AkBaseInspector : UnityEditor.Editor
{
	private bool m_buttonWasPressed;
	private UnityEngine.Rect m_dropAreaRelativePos; //button position relative to the inspector

	protected UnityEditor.SerializedProperty[]
		m_guidProperty; //all components have 1 guid except switches and states which have 2. Index zero is value guid and index 1 is group guid

	protected bool m_isInDropArea = false; //is the mouse on top of the drop area(the button)	
	protected AkWwiseProjectData.WwiseObjectType m_objectType;
	private UnityEngine.Rect m_pickerPos;
	protected string m_typeName;

	public abstract void OnChildInspectorGUI();
	public abstract string UpdateIds(System.Guid[] in_guid); //set object properties and return its name

	private AkDragDropData GetAkDragDropData()
	{
		var DDData = UnityEditor.DragAndDrop.GetGenericData(AkDragDropHelper.DragDropIdentifier) as AkDragDropData;
		return DDData != null && DDData.typeName.Equals(m_typeName) ? DDData : null;
	}

	private void HandleDragAndDrop(UnityEngine.Event currentEvent, UnityEngine.Rect dropArea)
	{
		if (currentEvent.type == UnityEngine.EventType.DragExited)
		{
			// clear dragged data
			UnityEditor.DragAndDrop.PrepareStartDrag();
		}
		else if (currentEvent.type == UnityEngine.EventType.DragUpdated ||
		         currentEvent.type == UnityEngine.EventType.DragPerform)
		{
			if (dropArea.Contains(currentEvent.mousePosition))
			{
				var DDData = GetAkDragDropData();

				if (currentEvent.type == UnityEngine.EventType.DragUpdated)
					UnityEditor.DragAndDrop.visualMode = DDData != null
						? UnityEditor.DragAndDropVisualMode.Link
						: UnityEditor.DragAndDropVisualMode.Rejected;
				else
				{
					UnityEditor.DragAndDrop.AcceptDrag();

					if (DDData != null)
					{
						AkUtilities.SetByteArrayProperty(m_guidProperty[0], DDData.guid.ToByteArray());

						var DDGroupData = DDData as AkDragDropGroupData;
						if (DDGroupData != null && m_guidProperty.Length > 1)
							AkUtilities.SetByteArrayProperty(m_guidProperty[1], DDGroupData.groupGuid.ToByteArray());

						//needed for the undo operation to work
						UnityEngine.GUIUtility.hotControl = 0;
					}
				}

				currentEvent.Use();
			}
		}
	}


	public override void OnInspectorGUI()
	{
		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		OnChildInspectorGUI();

		serializedObject.ApplyModifiedProperties();

		var currentEvent = UnityEngine.Event.current;
		HandleDragAndDrop(currentEvent, m_dropAreaRelativePos);

		/************************************************Update Properties**************************************************/
		var componentGuid = new System.Guid[m_guidProperty.Length];
		for (var i = 0; i < componentGuid.Length; i++)
		{
			var guidBytes = AkUtilities.GetByteArrayProperty(m_guidProperty[i]);
			componentGuid[i] = guidBytes == null ? System.Guid.Empty : new System.Guid(guidBytes);
		}

		var componentName = UpdateIds(componentGuid);
		/*******************************************************************************************************************/

		/********************************************Draw GUI***************************************************************/

		UnityEngine.GUILayout.Space(UnityEditor.EditorGUIUtility.standardVerticalSpacing);

		UnityEngine.GUILayout.BeginHorizontal("box");
		{
			float inspectorWidth = UnityEngine.Screen.width - UnityEngine.GUI.skin.box.margin.left -
			                       UnityEngine.GUI.skin.box.margin.right - 19;
			UnityEngine.GUILayout.Label(m_typeName + " Name: ", UnityEngine.GUILayout.Width(inspectorWidth * 0.4f));

			var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.button);
			style.alignment = UnityEngine.TextAnchor.MiddleLeft;
			if (componentName.Equals(string.Empty))
			{
				componentName = "No " + m_typeName + " is currently selected";
				style.normal.textColor = UnityEngine.Color.red;
			}

			if (UnityEngine.GUILayout.Button(componentName, style,
				UnityEngine.GUILayout.MaxWidth(inspectorWidth * 0.6f - UnityEngine.GUI.skin.box.margin.right)))
			{
				m_buttonWasPressed = true;

				// We don't want to set object as dirty only because we clicked the button.
				// It will be set as dirty if the wwise object has been changed by the tree view.
				UnityEngine.GUI.changed = false;
			}

			//GUILayoutUtility.GetLastRect and AkUtilities.GetLastRectAbsolute must be called in repaint mode 
			if (currentEvent.type == UnityEngine.EventType.Repaint)
			{
				m_dropAreaRelativePos = UnityEngine.GUILayoutUtility.GetLastRect();

				if (m_buttonWasPressed)
				{
					m_pickerPos = AkUtilities.GetLastRectAbsolute();
					UnityEditor.EditorApplication.delayCall += DelayCreateCall;
					m_buttonWasPressed = false;
				}
			}
		}
		UnityEngine.GUILayout.EndHorizontal();

		/***********************************************************************************************************************/

		if (UnityEngine.GUI.changed)
			UnityEditor.EditorUtility.SetDirty(serializedObject.targetObject);
	}

	private void DelayCreateCall()
	{
		AkWwiseComponentPicker.Create(m_objectType, m_guidProperty, null, serializedObject, m_pickerPos);
	}
}

#endif