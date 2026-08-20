namespace CommuteGame.Models;

/// <summary>
/// Represents one stage of the commute (Navigation between locations
/// requirement). Each location presents a typing challenge themed
/// around a real commuting obstacle.
/// </summary>
public class Location
{
    public string Name { get; }
    public string ObstacleDescription { get; }
    public string Prompt { get; }
    public double TimeLimitSeconds { get; }
    public int FocusPenaltyOnFail { get; }
    public int PointsOnSuccess { get; }

    public Location(
        string name,
        string obstacleDescription,
        string prompt,
        double timeLimitSeconds,
        int focusPenaltyOnFail,
        int pointsOnSuccess)
    {
        Name = name;
        ObstacleDescription = obstacleDescription;
        Prompt = prompt;
        TimeLimitSeconds = timeLimitSeconds;
        FocusPenaltyOnFail = focusPenaltyOnFail;
        PointsOnSuccess = pointsOnSuccess;
    }
}
