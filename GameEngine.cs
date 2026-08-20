using CommuteGame.Models;

namespace CommuteGame.Engine;

/// <summary>
/// Drives the whole game: main menu, player setup, a full school year of
/// commutes (each with a transport choice and randomized obstacles), and
/// the year-end win/lose screens with multiple possible endings.
/// </summary>
public class GameEngine
{
    private const int QuartersInYear = 4;
    private const int DaysPerQuarter = 2; // 8 sampled school days across the year

    // A day only counts as "on time" if you finish with at least so much
    // Focus left - just surviving isn't enough, you have to still be
    // together enough to walk in composed. Exactly how much depends on
    // which transport mode you picked that morning (see TransportProfile).

    // The year is only "passed" if you were on time at least this often.
    private const double AttendancePassThreshold = 0.75;

    private static readonly string[] QuarterNames =
    {
        "First Grading", "Second Grading", "Third Grading", "Fourth Grading"
    };

    private readonly Random _rng = new();

    // ---------- Main Menu requirement ----------
    public void ShowMainMenu()
    {
        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("      CATCH THE 7:15 - a commute game");
            Console.WriteLine("========================================");
            Console.WriteLine("1. Start Game");
            Console.WriteLine("2. Exit Game");
            Console.Write("Choose an option: ");

            string? choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    StartGame();
                    break;
                case "2":
                    running = false;
                    Console.WriteLine("Goodbye! See you at the next commute.");
                    break;
                default:
                    Console.WriteLine("Invalid option. Press Enter to try again.");
                    Console.ReadLine();
                    break;
            }
        }
    }

    // ---------- Start Game / Player Name Input requirement ----------
    private void StartGame()
    {
        Console.Clear();
        Console.WriteLine("A new school year begins. Every morning is a fresh");
        Console.WriteLine("commute, and every commute has its own obstacles.\n");

        Console.Write("Enter your name, commuter: ");
        string? nameInput = Console.ReadLine();
        string name = string.IsNullOrWhiteSpace(nameInput) ? "Commuter" : nameInput.Trim();

        var player = new Player(name);
        RunSchoolYear(player);
    }

    private void RunSchoolYear(Player player)
    {
        var quarterOnTimeCounts = new int[QuartersInYear];

        for (int quarterIndex = 0; quarterIndex < QuarterNames.Length; quarterIndex++)
        {
            string quarterName = QuarterNames[quarterIndex];
            for (int day = 1; day <= DaysPerQuarter; day++)
            {
                bool onTime = RunSchoolDay(player, quarterName, day);
                if (onTime) quarterOnTimeCounts[quarterIndex]++;
            }
        }

        ShowYearEndScreen(player, quarterOnTimeCounts);
    }

    // ---------- Navigation between locations requirement ----------
    private bool RunSchoolDay(Player player, string quarterName, int dayNumber)
    {
        Console.Clear();
        Console.WriteLine($"{quarterName} - Day {dayNumber}");
        Console.WriteLine();
        Console.WriteLine(Dialogue.RandomMorningLine(_rng));
        Console.WriteLine("\nPress Enter to head out...");
        Console.ReadLine();

        TransportMode mode = ChooseTransport(quarterName, dayNumber);
        TransportProfile profile = TransportProfile.For(mode);

        // Transport Consequences requirement: the mode you picked sets
        // your starting Focus for the day and the bar you need to clear
        // to count as on time - every option is a real tradeoff, not
        // just a different flavor of the same commute.
        player.StartNewDay(profile.StartingFocusModifier);

        if (profile.StartingFocusModifier != 0)
        {
            Console.WriteLine();
            Console.WriteLine($"  Riding the {profile.DisplayName.ToLower()} today: {profile.Disadvantage}");
            Console.WriteLine($"  (-{-profile.StartingFocusModifier} starting Focus)");
            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }

        List<Location> route = TransportRoutes.GetDailyRoute(mode, _rng);

        foreach (Location location in route)
        {
            Console.Clear();
            Console.WriteLine($"{quarterName} - Day {dayNumber}  |  Riding: {mode}");
            Console.WriteLine($"Location: {location.Name}");
            Console.WriteLine($"Obstacle: {location.ObstacleDescription}");
            player.PrintStats();

            ChallengeResult result = TypingChallenge.Run(location.Prompt, location.TimeLimitSeconds);

            if (result.Success)
            {
                player.RecordSuccess(location.PointsOnSuccess);
                Console.WriteLine($"  {Dialogue.RandomSuccessLine(_rng)}");
            }
            else
            {
                // The riskier/more stressful the ride, the more a
                // mistake actually costs you.
                int effectivePenalty = (int)Math.Round(location.FocusPenaltyOnFail * profile.FocusPenaltyMultiplier);
                player.RecordFailure(effectivePenalty);
                Console.WriteLine($"  {Dialogue.RandomFailureLine(_rng)} (-{effectivePenalty} Focus)");
            }

            // Running fully out of Focus cuts the day short - you still
            // "arrive", just too late and too frazzled to matter.
            if (player.IsOutOfFocus)
            {
                Console.WriteLine("\n  You're too frazzled to keep going.");
                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
                break;
            }

            Console.WriteLine("\nPress Enter to continue...");
            Console.ReadLine();
        }

        // Losing Condition (per day): surviving isn't enough on its own -
        // you need enough Focus left to clear the bar for however you
        // chose to commute (see TransportProfile.OnTimeThreshold).
        bool onTime = player.Focus >= profile.OnTimeThreshold;
        player.RecordDayResult(onTime);

        Console.Clear();
        PrintAttendanceBanner(onTime, player.Focus, profile.OnTimeThreshold);
        Console.WriteLine(onTime
            ? $"  {quarterName}, Day {dayNumber}: {Dialogue.RandomOnTimeLine(_rng)}"
            : $"  {quarterName}, Day {dayNumber}: {Dialogue.RandomLateLine(_rng)}");
        Console.WriteLine("\nPress Enter to continue to the next day...");
        Console.ReadLine();

        return onTime;
    }

    // A loud, unmistakable readout of whether the day counted as on
    // time, and by how much - no need to do the Focus-vs-threshold math
    // yourself to know where you stood.
    private static void PrintAttendanceBanner(bool onTime, int finalFocus, int requiredFocus)
    {
        ConsoleColor previousColor = Console.ForegroundColor;
        Console.ForegroundColor = onTime ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine("========================================");
        Console.WriteLine(onTime ? "   ON TIME" : "   LATE");
        Console.WriteLine("========================================");
        Console.ForegroundColor = previousColor;
        Console.WriteLine($"  Arrived with {finalFocus} Focus (needed {requiredFocus}+ for this commute).");
        Console.WriteLine();
    }

    private static TransportMode ChooseTransport(string quarterName, int dayNumber)
    {
        Console.Clear();
        Console.WriteLine($"{quarterName} - Day {dayNumber}");
        Console.WriteLine("How are you getting to school today? Every option is a tradeoff:\n");

        PrintTransportOption(1, "Walk", TransportProfile.Walk);
        PrintTransportOption(2, "Ride the jeep", TransportProfile.Jeep);
        PrintTransportOption(3, "Catch the bus", TransportProfile.Bus);
        PrintTransportOption(4, "Drive the motorcycle", TransportProfile.Motorcycle);

        while (true)
        {
            Console.Write("\nChoose an option: ");
            switch (Console.ReadLine())
            {
                case "1": return TransportMode.Walk;
                case "2": return TransportMode.Jeep;
                case "3": return TransportMode.Bus;
                case "4": return TransportMode.Motorcycle;
                default:
                    Console.WriteLine("Invalid option, try again.");
                    break;
            }
        }
    }

    private static void PrintTransportOption(int number, string label, TransportProfile profile)
    {
        Console.WriteLine($"{number}. {label}  (on time if you arrive with {profile.OnTimeThreshold}+ Focus)");
        Console.WriteLine($"     + {profile.Advantage}");
        Console.WriteLine($"     - {profile.Disadvantage}");
    }

    // ---------- Winning / Losing Condition requirement, with multiple endings ----------
    // The year as a whole is won or lost based on attendance, but which
    // specific ending you get also depends on the *shape* of the year -
    // whether you got better, got worse, bounced around, or stayed the
    // same from First Grading through Fourth Grading.
    private enum YearTrend { Improving, Declining, Volatile, Consistent }

    private static YearTrend DetermineTrend(int[] quarterOnTimeCounts)
    {
        int firstHalf = quarterOnTimeCounts[0] + quarterOnTimeCounts[1];
        int secondHalf = quarterOnTimeCounts[2] + quarterOnTimeCounts[3];
        int diff = secondHalf - firstHalf;

        // A big enough shift between the first two gradings and the last
        // two outweighs any single-quarter wobble.
        if (diff >= 2) return YearTrend.Improving;
        if (diff <= -2) return YearTrend.Declining;

        // No consistent direction, but if some quarter was great and
        // another was rough, that's a bumpy year rather than a steady one.
        int spread = quarterOnTimeCounts.Max() - quarterOnTimeCounts.Min();
        if (spread >= 2) return YearTrend.Volatile;

        return YearTrend.Consistent;
    }

    private void ShowYearEndScreen(Player player, int[] quarterOnTimeCounts)
    {
        int totalDays = QuartersInYear * DaysPerQuarter;
        double attendanceRate = totalDays == 0 ? 0 : (double)player.DaysOnTime / totalDays;
        bool passedTheYear = attendanceRate >= AttendancePassThreshold;
        YearTrend trend = DetermineTrend(quarterOnTimeCounts);

        (string title, string message) = passedTheYear
            ? GetPassingEnding(player, trend)
            : GetFailingEnding(player, trend, attendanceRate);

        Console.Clear();
        Console.WriteLine("========================================");
        Console.WriteLine("           END OF SCHOOL YEAR");
        Console.WriteLine("========================================");
        for (int i = 0; i < QuarterNames.Length; i++)
        {
            Console.WriteLine($" {QuarterNames[i],-16} {quarterOnTimeCounts[i]}/{DaysPerQuarter} on time");
        }
        Console.WriteLine("----------------------------------------");
        Console.WriteLine($"  {title}");
        Console.WriteLine(message);
        player.PrintStats();
        Console.WriteLine($" Attendance rate:  {attendanceRate:P0}");
        Console.WriteLine("\nPress Enter to return to the main menu...");
        Console.ReadLine();
    }

    private static (string Title, string Message) GetPassingEnding(Player player, YearTrend trend)
    {
        if (player.DaysLate == 0 && player.MistakesMade <= 1)
        {
            return ("PERFECT ATTENDANCE", "Not a single late mark all year. You're the commuter everyone else wishes they were.");
        }

        if (trend == YearTrend.Improving)
        {
            return ("COMEBACK KID", "First Grading was rough, but something clicked - the back half of the year was a completely different story.");
        }

        if (trend == YearTrend.Declining)
        {
            return ("FADED CHAMPION", "You built such a strong lead early in the year that even a slower back half couldn't cost you the pass.");
        }

        if (trend == YearTrend.Volatile)
        {
            return ("UP AND DOWN", "Some gradings were smooth, others were a scramble - but somehow it balanced out to a passing year.");
        }

        if (player.DaysLate <= 1)
        {
            return ("SOLID COMMUTER", "A late day here and there, but nothing that hurt your record. Well played.");
        }

        return ("JUST PASSED", "It was close some mornings, but you scraped through the year on time often enough.");
    }

    private static (string Title, string Message) GetFailingEnding(Player player, YearTrend trend, double attendanceRate)
    {
        if (trend == YearTrend.Improving)
        {
            return ("TOO LITTLE, TOO LATE", "You finally found your rhythm in the second half of the year - it just wasn't enough to make up for how rough the start was.");
        }

        if (trend == YearTrend.Volatile)
        {
            return ("ROLLERCOASTER YEAR", "A great grading followed by a terrible one, over and over - too inconsistent to ever get ahead of it.");
        }

        if (attendanceRate <= 0.25)
        {
            return ("CHRONICALLY LATE", "More mornings than not, you walked in after the bell. The year wore you down from the very start.");
        }

        return ("BURNED OUT", "You had good days, but too many rough mornings caught up with you by year's end.");
    }
}