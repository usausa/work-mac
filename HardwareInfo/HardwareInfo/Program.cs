using System.Runtime.InteropServices;

var time = new Timeval();
var size = Marshal.SizeOf(typeof(Timeval));
var ret = NativeMethods.sysctlbyname("kern.boottime", ref time, ref size, IntPtr.Zero, 0);

Console.WriteLine(ret.ToString());
var boot = DateTimeOffset.FromUnixTimeMilliseconds((time.tv_sec * 1000) + (time.tv_usec / 1000));
var uptime = DateTimeOffset.Now - boot;

Console.WriteLine(uptime.TotalMilliseconds);

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Timeval
{
    public long tv_sec;
    public long tv_usec;
}

public static unsafe class NativeMethods
{
    [DllImport("libc")]
    public static extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string name, ref Timeval oldp, ref int oldlen, IntPtr newp, int newlen);
}