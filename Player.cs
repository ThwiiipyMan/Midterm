namespace CommuteGame.Models;

/// <summary>
/// Represents the commuter (player). Focus is a per-day health bar that
/// resets every morning; everything else accumulates across the whole
/// school year and is what the final ending is judged on.
/// </summary>
public class Player
{
    public string Name { get; }

    // Health Management System requirement.
    // Focus represents how composed the commuter is *today*. It resets
    // each morning, so one rough day doesn't end the whole game.
    public int Focus { get; private set; }
    public const int MaxFocus = 100;

    // Player Statistics requirement - tracked across the full year.
    public int DaysOnTime { get; private set; }
    public int DaysLate { get; private set; }
    public int LocationsPassed { get; private set; }
    public int MistakesMade { get; private set; }
    public int Score { get; private set; }

    public Player(string name)
    {
        Name = name;
        Focus = MaxFocus;
    }

    public bool IsOutOfFocus => Focus <= 0;

    public void StartNewDay(int startingFocusModifier = 0)
    {
        Focus = Math.Clamp(MaxFocus + startingFocusModifier, 0, MaxFocus);
    }

    public void RecordSuccess(int pointsEarned)
    {
        LocationsPassed++;
        Score += pointsEarned;
    }

    public void RecordFailure(int focusPenalty)
    {
        MistakesMade++;
        LoseFocus(focusPenalty);
    }

    public void LoseFocus(int amount)
    {
        Focus -= amount;
        if (Focus < 0) Focus = 0;
    }

    // On-time bonus and late penalty, so Score reflects the whole year's
    // performance, not just how many typing challenges you won.
    private const int OnTimeBonus = 20;
    private const int LatePenalty = 15;

    public void RecordDayResult(bool onTime)
    {
        if (onTime)
        {
            DaysOnTime++;
            Score += OnTimeBonus;
        }
        else
        {
            DaysLate++;
            Score -= LatePenalty;
            if (Score < 0) Score = 0;
        }
    }

    public void PrintStats()
    {
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($" Commuter:         {Name}");
        Console.WriteLine($" Focus today:      {Focus}/{MaxFocus}");
        Console.WriteLine($" Days on time:     {DaysOnTime}");
        Console.WriteLine($" Days late:        {DaysLate}");
        Console.WriteLine($" Mistakes (year):  {MistakesMade}");
        Console.WriteLine($" Score:            {Score}");
        Console.WriteLine("----------------------------------------");
    }
}