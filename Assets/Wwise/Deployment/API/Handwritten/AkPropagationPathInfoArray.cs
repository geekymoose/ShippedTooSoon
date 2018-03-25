#if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.
//////////////////////////////////////////////////////////////////////
//
// Copyright (c) 2012 Audiokinetic Inc. / All Rights Reserved
//
//////////////////////////////////////////////////////////////////////

public class AkPropagationPathInfoArray : System.IDisposable
{
	private readonly int SIZE_OF_STRUCTURE = AkSoundEnginePINVOKE.CSharp_AkPropagationPathInfoProxy_GetSizeOf();
	private System.IntPtr m_Buffer;
	private int m_Count;

	public AkPropagationPathInfoArray(int count)
	{
		m_Count = count;
		m_Buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(count * SIZE_OF_STRUCTURE);
	}

	public void Dispose()
	{
		if (m_Buffer != System.IntPtr.Zero)
		{
			System.Runtime.InteropServices.Marshal.FreeHGlobal(m_Buffer);
			m_Buffer = System.IntPtr.Zero;
			m_Count = 0;
		}
	}

	~AkPropagationPathInfoArray()
	{
		Dispose();
	}

	public void Reset()
	{
		m_Count = 0;
	}

	public AkPropagationPathInfoProxy GetPropagationPathInfo(int index)
	{
		if (index >= m_Count)
			return null;

		return new AkPropagationPathInfoProxy(GetObjectPtr(index), false);
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
		return (System.IntPtr) (m_Buffer.ToInt64() + SIZE_OF_STRUCTURE * index);
	}
}
#endif // #if ! (UNITY_DASHBOARD_WIDGET || UNITY_WEBPLAYER || UNITY_WII || UNITY_WIIU || UNITY_NACL || UNITY_FLASH || UNITY_BLACKBERRY) // Disable under unsupported platforms.