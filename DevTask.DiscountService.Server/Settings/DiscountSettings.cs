namespace DevTask.DiscountService.Server.Settings;

public class DiscountSettings
{
    public required ConnectionStrings ConnectionStrings { get; set; }
    /// <summary>
    /// Maximum number of codes which can be generated in a single request
    /// </summary>
    public uint MaxGenerateCount { get; set; }
    
    /// <summary>
    /// Maximum number of parallel tasks used for code generation
    /// </summary>
    public int MaxParallelTasks { get; set; }
}