namespace CommuteGame.Models;

/// <summary>
/// The tradeoff sheet for each way of getting to school (Transport
/// Consequences requirement). No mode is strictly best: faster modes
/// give you a lower Focus bar to clear at the end of the commute, but
/// they take something from you up front or punish mistakes harder.
/// Slower modes are gentler minute-to-minute but demand you still be
/// sharp by the time you arrive.
/// </summary>
public class TransportProfile
{
    public TransportMode Mode { get; }
    public string DisplayName { get; }
    public string Advantage { get; }
    public string Disadvantage { get; }

    // Applied once, at the start of the day, before any obstacles.
    // Negative = you start already a little rattled (crowding, waiting,
    // risk). Walk and Motorcycle don't touch this at all.
    public int StartingFocusModifier { get; }

    // How much Focus you need left when you reach school to count as
    // "on time" (Losing Condition requirement). Faster modes get a
    // lower bar because there's more slack in the morning; slower
    // modes need you to still be composed when you walk in.
    public int OnTimeThreshold { get; }

    // Multiplies FocusPenaltyOnFail whenever a challenge is flubbed on
    // this mode - the riskier or more stressful the ride, the more a
    // mistake actually costs you.
    public double FocusPenaltyMultiplier { get; }

    private TransportProfile(
        TransportMode mode,
        string displayName,
        string advantage,
        string disadvantage,
        int startingFocusModifier,
        int onTimeThreshold,
        double focusPenaltyMultiplier)
    {
        Mode = mode;
        DisplayName = displayName;
        Advantage = advantage;
        Disadvantage = disadvantage;
        StartingFocusModifier = startingFocusModifier;
        OnTimeThreshold = onTimeThreshold;
        FocusPenaltyMultiplier = focusPenaltyMultiplier;
    }

    public static readonly TransportProfile Walk = new(
        TransportMode.Walk,
        "Walk",
        "Free, full starting Focus, and no traffic or vehicle risk - mistakes still sting, just a little less.",
        "It's the slowest option, so you need to stay sharp the whole way (highest bar to be on time).",
        startingFocusModifier: 0,
        onTimeThreshold: 40,
        focusPenaltyMultiplier: 0.95);

    public static readonly TransportProfile Jeep = new(
        TransportMode.Jeep,
        "Jeep",
        "Cheap and reasonably quick - the bar to be on time is average.",
        "Waiting for one with space and squeezing aboard costs you Focus before you even sit down.",
        startingFocusModifier: -8,
        onTimeThreshold: 35,
        focusPenaltyMultiplier: 1.0);

    public static readonly TransportProfile Bus = new(
        TransportMode.Bus,
        "Bus",
        "Faster than the jeep on a good day, which lowers the bar for making it on time.",
        "Lines and checkpoints eat into your morning, and an easy-to-miss stop makes mistakes rattle you more.",
        startingFocusModifier: -5,
        onTimeThreshold: 30,
        focusPenaltyMultiplier: 1.15);

    public static readonly TransportProfile Motorcycle = new(
        TransportMode.Motorcycle,
        "Motorcycle",
        "By far the fastest way to beat the bell - the lowest bar of all to count as on time.",
        "Weaving through traffic is stressful and unforgiving, so every mistake costs extra Focus.",
        startingFocusModifier: 0,
        onTimeThreshold: 20,
        focusPenaltyMultiplier: 1.3);

    public static TransportProfile For(TransportMode mode) => mode switch
    {
        TransportMode.Walk => Walk,
        TransportMode.Jeep => Jeep,
        TransportMode.Bus => Bus,
        TransportMode.Motorcycle => Motorcycle,
        _ => Walk
    };
}