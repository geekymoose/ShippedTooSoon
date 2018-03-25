#define DISPLAY_GROUP_NAME_AND_VALUE_NAME

namespace AK.Wwise.Editor
{
	[UnityEditor.CustomPropertyDrawer(typeof(Switch))]
	public class SwitchDrawer : BaseTypeDrawer
	{
		public override string UpdateIds(System.Guid[] in_guid)
		{
			var list = AkWwiseProjectInfo.GetData().SwitchWwu;

			for (var i = 0; i < list.Count; i++)
			{
				var group = list[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[1]));

				if (group != null)
				{
					var index = group.ValueGuids.FindIndex(x => new System.Guid(x.bytes).Equals(in_guid[0]));

					if (index < 0)
						break;

					m_IDProperty[0].intValue = group.valueIDs[index];
					m_IDProperty[1].intValue = group.ID;

#if DISPLAY_GROUP_NAME_AND_VALUE_NAME
					return group.Name + "/" + group.values[index];
#else
				return group.values[index];
#endif // DISPLAY_GROUP_NAME_AND_VALUE_NAME
				}
			}

			m_IDProperty[0].intValue = m_IDProperty[1].intValue = 0;
			return string.Empty;
		}

		public override void SetupSerializedProperties(UnityEditor.SerializedProperty property)
		{
			m_objectType = AkWwiseProjectData.WwiseObjectType.SWITCH;
			m_typeName = "Switch";

			m_IDProperty = new UnityEditor.SerializedProperty[2];
			m_IDProperty[0] = property.FindPropertyRelative("ID");
			m_IDProperty[1] = property.FindPropertyRelative("groupID");

			m_guidProperty = new UnityEditor.SerializedProperty[2];
			m_guidProperty[0] = property.FindPropertyRelative("valueGuid.Array");
			m_guidProperty[1] = property.FindPropertyRelative("groupGuid.Array");
		}
	}
}