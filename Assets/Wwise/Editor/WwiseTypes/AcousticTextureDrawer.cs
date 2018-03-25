namespace AK.Wwise.Editor
{
	[UnityEditor.CustomPropertyDrawer(typeof(AcousticTexture))]
	public class AcousticTextureDrawer : BaseTypeDrawer
	{
		protected override void SetEmptyComponentName(ref string componentName, ref UnityEngine.GUIStyle style)
		{
			componentName = "None";
		}

		public override string UpdateIds(System.Guid[] in_guid)
		{
			var list = AkWwiseProjectInfo.GetData().AcousticTextureWwu;

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
			m_objectType = AkWwiseProjectData.WwiseObjectType.ACOUSTICTEXTURE;
			m_typeName = "AcousticTexture";

			m_IDProperty = new UnityEditor.SerializedProperty[1];
			m_IDProperty[0] = property.FindPropertyRelative("ID");

			m_guidProperty = new UnityEditor.SerializedProperty[1];
			m_guidProperty[0] = property.FindPropertyRelative("valueGuid.Array");
		}
	}
}