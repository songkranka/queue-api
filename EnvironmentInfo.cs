
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

public struct EnvironmentInfo
{
    public EnvironmentInfo()
    {
        GCMemoryInfo gcInfo = GC.GetGCMemoryInfo();
        TotalAvailableMemoryBytes = gcInfo.TotalAvailableMemoryBytes;

        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        string[] memoryLimitPaths = new string[]
        {
            "/sys/fs/cgroup/memory.max",
            "/sys/fs/cgroup/memory.high",
            "/sys/fs/cgroup/memory.low",
            "/sys/fs/cgroup/memory/memory.limit_in_bytes",
        };

        string[] currentMemoryPaths = new string[]
        {
            "/sys/fs/cgroup/memory.current",
            "/sys/fs/cgroup/memory/memory.usage_in_bytes",
        };

        MemoryLimit = GetBestValue(memoryLimitPaths);
        MemoryUsage = GetBestValue(currentMemoryPaths);
        appsettingjson = GetConfigSettingValue("/app/appsetting.Production.json");
    }

    public string RuntimeVersion => RuntimeInformation.FrameworkDescription;
    public string OSVersion => RuntimeInformation.OSDescription;
    public string OSArchitecture => RuntimeInformation.OSArchitecture.ToString();
    public string User => Environment.UserName;
    public int ProcessorCount => Environment.ProcessorCount;
    public long TotalAvailableMemoryBytes { get; }
    public long MemoryLimit { get; }
    public long MemoryUsage { get; }

    public string appsettingjson { get; }
    public string HostName => Dns.GetHostName();

    public string BaseApi = Environment.GetEnvironmentVariable("BASE_API");

    public string EnvAppSetting = Environment.GetEnvironmentVariable("APP_SETTING");
    
    private static long GetBestValue(string[] paths)
    {
        foreach (string path in paths)
        {
            if (Path.Exists(path) &&
                long.TryParse(File.ReadAllText(path), out long result))
            {
                return result;
            }
        }

        return 0;
    }
    private static string GetConfigSettingValue(string paths)
    {

        if (Path.Exists(paths))
        {
            return File.ReadAllText(paths);
        }
        return "null";
    }
}

