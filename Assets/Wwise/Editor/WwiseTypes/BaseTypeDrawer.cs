namespace AK.Wwise.Editor
{
	public abstract class BaseTypeDrawer : UnityEditor.PropertyDrawer
	{
		private bool m_buttonWasPressed;

		protected UnityEditor.SerializedProperty[]
			m_guidProperty; //all components have 1 guid except switches and states which have 2. Index zero is value guid and index 1 is group guid

		protected UnityEditor.SerializedProperty[]
			m_IDProperty; //all components have 1 ID except switches and states which have 2. Index zero is ID and index 1 is groupID

		protected AkWwiseProjectData.WwiseObjectType m_objectType;

		private UnityEngine.Rect m_pickerPos;
		private UnityEngine.Rect m_pressedPosition;
		private UnityEditor.SerializedObject m_serializedObject;
		protected string m_typeName;

		public abstract string UpdateIds(System.Guid[] in_guid);
		public abstract void SetupSerializedProperties(UnityEditor.SerializedProperty property);


		private AkDragDropData GetAkDragDropData()
		{
			var DDData = UnityEditor.DragAndDrop.GetGenericData(AkDragDropHelper.DragDropIdentifier) as AkDragDropData;
			return DDData != null && DDData.typeName.Equals(m_typeName) ? DDData : null;
		}

		private void HandleDragAndDrop(UnityEngine.Event currentEvent, UnityEngine.Rect dropArea)
		{
			if (currentEvent.type == UnityEngine.EventType.DragExited)
				UnityEditor.DragAndDrop.PrepareStartDrag();
			else if (currentEvent.type == UnityEngine.EventType.DragUpdated ||
			         currentEvent.type == UnityEngine.EventType.DragPerform)
			{
				if (dropArea.Contains(currentEvent.mousePosition))
				{
					var DDData = GetAkDragDropData();

					if (currentEvent.type == UnityEngine.EventType.DragUpdated)
					{
						UnityEditor.DragAndDrop.visualMode = DDData != null
							? UnityEditor.DragAndDropVisualMode.Link
							: UnityEditor.DragAndDropVisualMode.Rejected;
					}
					else
					{
						UnityEditor.DragAndDrop.AcceptDrag();

						if (DDData != null)
						{
							AkUtilities.SetByteArrayProperty(m_guidProperty[0], DDData.guid.ToByteArray());
							m_IDProperty[0].intValue = DDData.ID;

							var DDGroupData = DDData as AkDragDropGroupData;
							if (DDGroupData != null)
							{
								if (m_guidProperty.Length > 1)
									AkUtilities.SetByteArrayProperty(m_guidProperty[1], DDGroupData.groupGuid.ToByteArray());
								if (m_IDProperty.Length > 1)
									m_IDProperty[1].intValue = DDGroupData.groupID;
							}

							//needed for the undo operation to work
							UnityEngine.GUIUtility.hotControl = 0;
						}
					}

					currentEvent.Use();
				}
			}
		}

		protected virtual void SetEmptyComponentName(ref string componentName, ref UnityEngine.GUIStyle style)
		{
			componentName = "No " + m_typeName + " is currently selected";
			style.normal.textColor = UnityEngine.Color.red;
		}

		public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property,
			UnityEngine.GUIContent label)
		{
			// Using BeginProperty / EndProperty on the parent property means that
			// prefab override logic works on the entire property.
			UnityEditor.EditorGUI.BeginProperty(position, label, property);

			SetupSerializedProperties(property);

			// Draw label
			position = UnityEditor.EditorGUI.PrefixLabel(position,
				UnityEngine.GUIUtility.GetControlID(UnityEngine.FocusType.Passive), label);

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
			var style = new UnityEngine.GUIStyle(UnityEngine.GUI.skin.button);
			style.alignment = UnityEngine.TextAnchor.MiddleLeft;
			style.fontStyle = UnityEngine.FontStyle.Normal;

			if (string.IsNullOrEmpty(componentName))
				SetEmptyComponentName(ref componentName, ref style);

			if (UnityEngine.GUI.Button(position, componentName, style))
			{
				m_pressedPosition = position;
				m_buttonWasPressed = true;

				// We don't want to set object as dirty only because we clicked the button.
				// It will be set as dirty if the wwise object has been changed by the tree view.
				UnityEngine.GUI.changed = false;
			}

			var currentEvent = UnityEngine.Event.current;

			if (currentEvent.type == UnityEngine.EventType.Repaint && m_buttonWasPressed && m_pressedPosition.Equals(position))
			{
				m_serializedObject = property.serializedObject;
				m_pickerPos = AkUtilities.GetLastRectAbsolute(false);

				UnityEditor.EditorApplication.delayCall += DelayCreateCall;
				m_buttonWasPressed = false;
			}

			HandleDragAndDrop(currentEvent, position);

			UnityEditor.EditorGUI.EndProperty();
		}

		private void DelayCreateCall()
		{
			AkWwiseComponentPicker.Create(m_objectType, m_guidProperty, m_IDProperty, m_serializedObject, m_pickerPos);
		}
	}
}