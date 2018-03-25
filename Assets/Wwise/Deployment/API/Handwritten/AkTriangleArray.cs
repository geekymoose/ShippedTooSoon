#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

public class AkTriangleArray : System.IDisposable
{
	private readonly int SIZE_OF_AKTRIANGLE = AkSoundEnginePINVOKE.CSharp_AkTriangleProxy_GetSizeOf();
	private System.IntPtr m_Buffer;
	private int m_Count;

	public AkTriangleArray(int count)
	{
		m_Count = count;
		m_Buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(count * SIZE_OF_AKTRIANGLE);

		if (m_Buffer != System.IntPtr.Zero)
		{
			for (var i = 0; i < count; ++i)
				AkSoundEnginePINVOKE.CSharp_AkTriangleProxy_Clear(GetObjectPtr(i));
		}
	}

	public void Dispose()
	{
		if (m_Buffer != System.IntPtr.Zero)
		{
			for (var i = 0; i < m_Count; ++i)
				AkSoundEnginePINVOKE.CSharp_AkTriangleProxy_DeleteName(GetObjectPtr(i));

			System.Runtime.InteropServices.Marshal.FreeHGlobal(m_Buffer);
			m_Buffer = System.IntPtr.Zero;
			m_Count = 0;
		}
	}

	~AkTriangleArray()
	{
		Dispose();
	}

	public void Reset()
	{
		m_Count = 0;
	}

	public AkTriangle GetTriangle(int index)
	{
		if (index >= m_Count)
			return null;

		return new AkTriangle(GetObjectPtr(index), false);
	}

	public System.IntPtr GetBuffer()
	{
		return m_Buffer;
	}

	public int Count()
	{
		return m_Count;
	}

	private System.IntPtr GetObjectPtr(int index)
	{
		return (System.IntPtr) (m_Buffer.ToInt64() + SIZE_OF_AKTRIANGLE * index);
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.