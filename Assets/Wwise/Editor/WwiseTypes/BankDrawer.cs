namespace AK.Wwise.Editor
{
	[UnityEditor.CustomPropertyDrawer(typeof(Bank))]
	public class BankDrawer : BaseTypeDrawer
	{
		private UnityEditor.SerializedProperty bankNameProperty;

		public override string UpdateIds(System.Guid[] in_guid)
		{
			var list = AkWwiseProjectInfo.GetData().BankWwu;

			for (var i = 0; i < list.Count; i++)
			{
				var element = list[i].List.Find(x => new System.Guid(x.Guid).Equals(in_guid[0]));

				if (element != null)
				{
					m_IDProperty[0].intValue = element.ID;
					bankNameProperty.stringValue = element.Name;
					return bankNameProperty.stringValue;
				}
			}

			m_IDProperty[0].intValue = 0;
			bankNameProperty.stringValue = string.Empty;
			return bankNameProperty.stringValue;
		}

		public override void SetupSerializedProperties(UnityEditor.SerializedProperty property)
		{
			m_objectType = AkWwiseProjectData.WwiseObjectType.SOUNDBANK;
			m_typeName = "Bank";

			m_IDProperty = new UnityEditor.SerializedProperty[1];
			m_IDProperty[0] = property.FindPropertyRelative("ID");

			m_guidProperty = new UnityEditor.SerializedProperty[1];
			m_guidProperty[0] = property.FindPropertyRelative("valueGuid.Array");

			bankNameProperty = property.FindPropertyRelative("name");
		}
	}
}