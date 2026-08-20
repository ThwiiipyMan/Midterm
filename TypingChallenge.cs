namespace CommuteGame.Engine;

/// <summary>
/// The result of a single timed typing challenge.
/// </summary>
public class ChallengeResult
{
    public bool Success { get; init; }
    public bool TimedOut { get; init; }
    public double SecondsTaken { get; init; }
}

/// <summary>
/// Runs the "type the prompt before the timer runs out" mechanic.
/// This is the core gameplay loop the whole game is built around.
/// </summary>
public static class TypingChallenge
{
    public static ChallengeResult Run(string prompt, double timeLimitSeconds)
    {
        Console.WriteLine();
        Console.WriteLine($"  Type exactly:  \"{prompt}\"");

        // Reserve a fixed line for the countdown so the ticker can keep
        // overwriting it without disturbing the line you're typing on.
        int timerRow = Console.CursorTop;
        Console.WriteLine("  Time left: --.-s");
        Console.Write("  > ");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var cts = new CancellationTokenSource();

        // Reading console input is a blocking call, so it has to run on
        // its own task in order to race it against a countdown timer.
        Task<string?> inputTask = Task.Run(Console.ReadLine);
        Task delayTask = Task.Delay(TimeSpan.FromSeconds(timeLimitSeconds));
        Task tickerTask = RunTicker(timerRow, timeLimitSeconds, stopwatch, cts.Token);

        Task finishedFirst = Task.WhenAny(inputTask, delayTask).GetAwaiter().GetResult();
        stopwatch.Stop();

        // Stop the ticker now that a winner is decided; it's purely
        // cosmetic and shouldn't keep drawing after the round ends.
        cts.Cancel();
        try { tickerTask.GetAwaiter().GetResult(); } catch (TaskCanceledException) { }

        double secondsTaken = stopwatch.Elapsed.TotalSeconds;

        if (finishedFirst == delayTask)
        {
            Console.WriteLine("\n  Too slow! Time ran out.");
            return new ChallengeResult
            {
                Success = false,
                TimedOut = true,
                SecondsTaken = timeLimitSeconds
            };
        }

        string? typed = inputTask.Result;
        bool matches = string.Equals(typed?.Trim(), prompt.Trim(), StringComparison.Ordinal);

        if (!matches)
        {
            Console.WriteLine("  That wasn't quite right.");
        }

        return new ChallengeResult
        {
            Success = matches,
            TimedOut = false,
            SecondsTaken = secondsTaken
        };
    }

    // Redraws the "Time left" line every 200ms until cancelled, without
    // moving the cursor away from wherever the player is currently typing.
    private static async Task RunTicker(int timerRow, double timeLimitSeconds, System.Diagnostics.Stopwatch stopwatch, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                double remaining = timeLimitSeconds - stopwatch.Elapsed.TotalSeconds;
                if (remaining < 0) remaining = 0;

                int savedRow = Console.CursorTop;
                int savedCol = Console.CursorLeft;

                Console.SetCursorPosition(0, timerRow);
                Console.Write($"  Time left: {remaining,4:F1}s   ");
                Console.SetCursorPosition(savedCol, savedRow);

                await Task.Delay(200, token);
            }
        }
        catch (TaskCanceledException)
        {
            // Expected when the round finishes before the next tick.
        }
    }
}