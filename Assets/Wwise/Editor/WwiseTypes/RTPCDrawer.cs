namespace AK.Wwise.Editor
{
	[UnityEditor.CustomPropertyDrawer(typeof(RTPC))]
	public class RTPCDrawer : BaseTypeDrawer
	{
		public override string UpdateIds(System.Guid[] in_guid)
		{
			var list = AkWwiseProjectInfo.GetData().RtpcWwu;

			for (var i = 0; i < list.Count; i++)
			{
				var element = list[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[0]));

				if (element != null)
				{
					m_IDProperty[0].intValue = element.ID;
					return element.Name;
				}
			}

			m_IDProperty[0].intValue = 0;
			return string.Empty;
		}

		public override void SetupSerializedProperties(UnityEditor.SerializedProperty property)
		{
			m_objectType = AkWwiseProjectData.WwiseObjectType.GAMEPARAMETER;
			m_typeName = "GameParameter";

			m_IDProperty = new UnityEditor.SerializedProperty[1];
			m_IDProperty[0] = property.FindPropertyRelative("ID");

			m_guidProperty = new UnityEditor.SerializedProperty[1];
			m_guidProperty[0] = property.FindPropertyRelative("valueGuid.Array");
		}
	}
}