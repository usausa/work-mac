using System.Runtime.InteropServices;

// uptime
var time = new NativeMethods.Timeval();
var size = Marshal.SizeOf(typeof(NativeMethods.Timeval));
var ret = NativeMethods.sysctlbyname("kern.boottime", ref time, ref size, IntPtr.Zero, 0);

Console.WriteLine(ret.ToString());
var boot = DateTimeOffset.FromUnixTimeMilliseconds((time.tv_sec * 1000) + (time.tv_usec / 1000));
var uptime = DateTimeOffset.Now - boot; 

Console.WriteLine(uptime.TotalMilliseconds);

// statics
var hostPort = NativeMethods.mach_host_self();
if (hostPort == IntPtr.Zero)
{
    Console.WriteLine("Failed to get host port.");
    return;
}

var hostInfoCount = NativeMethods.HOST_VM_INFO_COUNT;
var hostInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeMethods.HostLoadInfo)));

try
{
    var result = NativeMethods.host_statistics(hostPort, NativeMethods.HOST_VM_INFO, hostInfoPtr, ref hostInfoCount);
    if (result == 0)
    {
        var hostLoadInfo = Marshal.PtrToStructure<NativeMethods.HostLoadInfo>(hostInfoPtr);

        Console.WriteLine("Host Load Information:");
        Console.WriteLine($"1-min Average Load: {hostLoadInfo.avenrun1 / (double)(1 << 16)}");
        Console.WriteLine($"5-min Average Load: {hostLoadInfo.avenrun5 / (double)(1 << 16)}");
        Console.WriteLine($"15-min Average Load: {hostLoadInfo.avenrun15 / (double)(1 << 16)}");
        Console.WriteLine($"Mach Factor: {hostLoadInfo.machFactor / (double)(1 << 16)}");
    }
    else
    {
        Console.WriteLine($"Error: Failed to retrieve host statistics (Error code: {result})");
    }
}
finally
{
    Marshal.FreeHGlobal(hostInfoPtr);
}

// ReSharper disable IdentifierTypo
public static class NativeMethods
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Timeval
    {
        public long tv_sec;
        public long tv_usec;
    }

    [DllImport("libc")]
    public static extern int sysctlbyname([MarshalAs(UnmanagedType.LPStr)] string name, ref Timeval oldp, ref int oldlen, IntPtr newp, int newlen);
    
    public const int HOST_VM_INFO = 2;
    public const int HOST_VM_INFO_COUNT = 0xf;

    [StructLayout(LayoutKind.Sequential)]
    public struct HostLoadInfo
    {
        public int avenrun1;
        public int avenrun5;
        public int avenrun15;
        public int machFactor;
    }

    [DllImport("libSystem")]
    public static extern IntPtr mach_host_self();

    [DllImport("libSystem")]
    public static extern int host_statistics(
        IntPtr host,
        int flavor,
        IntPtr hostInfoOut,
        ref int hostInfoOutCount
    );
}
