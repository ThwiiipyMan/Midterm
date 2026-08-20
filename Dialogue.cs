namespace CommuteGame.Engine;

/// <summary>
/// Small pools of flavor lines, picked at random, so the same day never
/// reads exactly the same way twice even when the underlying obstacles
/// repeat.
/// </summary>
public static class Dialogue
{
    private static readonly string[] MorningLines =
    {
        "\"Anak, gising na! You're going to be late again!\" your mom shouts from the kitchen.",
        "Your alarm blares for the third time. No more snoozing today.",
        "Your phone buzzes: \"Wea u at?? Quiz starts 7:30 sharp\" - a classmate texts.",
        "The smell of fried rice pulls you out of bed, but the clock says you don't have time to eat.",
        "\"Ingat ka diyan ha,\" your dad calls out as you grab your bag.",
        "You check the weather app. Rain, of course. Today of all days.",
        "Your little sibling is hogging the bathroom. Again.",
    };

    private static readonly string[] SuccessLines =
    {
        "Nailed it without breaking stride.",
        "Smooth. You barely even notice the delay.",
        "Quick hands, quicker feet - you're through.",
        "A classmate gives you a thumbs up as you power past.",
        "Not even a hiccup. On to the next one.",
        "You surprise yourself with how fast that was.",
    };

    private static readonly string[] FailureLines =
    {
        "Your fingers fumble and the moment slips away.",
        "\"Bilisan mo!\" someone shouts behind you - too late.",
        "You mistype and have to shake it off.",
        "That one cost you more than you'd like to admit.",
        "You freeze for half a second, and that's all it takes.",
        "Your hands are shakier than you thought.",
    };

    private static readonly string[] OnTimeLines =
    {
        "You slide into your seat just as the bell rings. Made it.",
        "The guard barely looks up as you walk in - right on time.",
        "You catch your breath at your desk. Another day, on time.",
        "Your seatmate leans over: \"Ang bilis mo naman today!\"",
    };

    private static readonly string[] LateLines =
    {
        "You slip in during the second period, hoping the teacher doesn't notice.",
        "\"Late again?\" the guard says, marking your name in the logbook.",
        "You mouth \"sorry po\" to your teacher as you find your seat.",
        "Another tardy slip. Your bag feels heavier walking in.",
    };

    public static string RandomMorningLine(Random rng) => MorningLines[rng.Next(MorningLines.Length)];
    public static string RandomSuccessLine(Random rng) => SuccessLines[rng.Next(SuccessLines.Length)];
    public static string RandomFailureLine(Random rng) => FailureLines[rng.Next(FailureLines.Length)];
    public static string RandomOnTimeLine(Random rng) => OnTimeLines[rng.Next(OnTimeLines.Length)];
    public static string RandomLateLine(Random rng) => LateLines[rng.Next(LateLines.Length)];
}
