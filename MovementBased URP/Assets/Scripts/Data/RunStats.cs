/// <summary>
/// Static class to pass run statistics between scenes.
/// Data persists until the game is closed.
/// </summary>
public static class RunStats
{
    public static int LevelsCompleted { get; set; }
    public static int DifficultyType { get; set; }
    public static int MoneyEarned { get; set; }
    public static int MoneyTotal { get; set; }
    public static float MoneyMultiplier { get; set; }
    public static float TotalPlayTime { get; set; }
    
    public static void Clear()
    {
        LevelsCompleted = 0;
        DifficultyType = 0;
        MoneyEarned = 0;
        MoneyTotal = 0;
        MoneyMultiplier = 1f;
        TotalPlayTime = 0f;
    }
}
