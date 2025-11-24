namespace EduGraphScheduler.Application;

public class SyncSettings
{
    public int BatchSize { get; set; } = 100;
    public int MaxDegreeOfParallelism { get; set; } = 3;
    public int SyncIntervalInMinutes { get; set; } = 60;
}