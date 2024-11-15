namespace backend.metrics;

using System.Diagnostics;

public static class SystemResourceMonitor
{
    public static float GetCpuUsage()
    {
        #if WINDOWS
            using var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            return cpuCounter.NextValue();
        #else
            throw new PlatformNotSupportedException("CPU usage monitoring is only supported on Windows.");
        #endif
    }

    public static float GetAvailableMemory()
    {
        #if WINDOWS
            using var ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            return ramCounter.NextValue();
        #else
            throw new PlatformNotSupportedException("CPU usage monitoring is only supported on Windows.");
        #endif
    }
}
