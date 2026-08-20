using CommuteGame.Models;

namespace CommuteGame.Engine;

/// <summary>
/// Holds a larger pool of possible obstacles for each transport mode and
/// draws a handful of them at random every day. Because the draw is
/// random and the pools are bigger than a single day's route, no two
/// commutes play out identically - even back-to-back days on the same
/// transport feel different.
/// </summary>
public static class TransportRoutes
{
    private const int StagesPerDay = 3;

    public static List<Location> GetDailyRoute(TransportMode mode, Random rng)
    {
        List<Location> pool = mode switch
        {
            TransportMode.Walk => WalkPool(),
            TransportMode.Jeep => JeepPool(),
            TransportMode.Bus => BusPool(),
            TransportMode.Motorcycle => MotorcyclePool(),
            _ => WalkPool()
        };

        // Shuffle the pool and take the first few - a different subset
        // (and a different order) almost every time.
        return pool.OrderBy(_ => rng.Next()).Take(StagesPerDay).ToList();
    }

    /// <summary>
    /// Time limit scales with how much there is to type, so a longer
    /// prompt isn't unfairly punished with a short clock. Difficulty
    /// still matters - it just shrinks or stretches that fair baseline
    /// instead of being set from scratch per obstacle.
    /// difficulty: Easy = more breathing room, Hard/VeryHard = tighter.
    /// </summary>
    private static double TimeFor(string prompt, Difficulty difficulty)
    {
        double baseTime = 1.3 + prompt.Length * 0.16; // reaction time + typing time
        double factor = difficulty switch
        {
            Difficulty.Easy => 1.2,
            Difficulty.Medium => 1.0,
            Difficulty.Hard => 0.85,
            Difficulty.VeryHard => 0.72,
            _ => 1.0
        };
        return Math.Round(baseTime * factor, 1);
    }

    private enum Difficulty { Easy, Medium, Hard, VeryHard }

    // (focusPenalty, points) per difficulty tier, kept consistent across
    // every transport pool so "Hard" always costs and pays the same.
    private static (int Penalty, int Points) Stakes(Difficulty difficulty) => difficulty switch
    {
        Difficulty.Easy => (15, 10),
        Difficulty.Medium => (20, 15),
        Difficulty.Hard => (25, 15),
        Difficulty.VeryHard => (30, 20),
        _ => (20, 15)
    };

    private static Location Make(string name, string obstacle, string prompt, Difficulty difficulty)
    {
        (int penalty, int points) = Stakes(difficulty);
        return new Location(name, obstacle, prompt, TimeFor(prompt, difficulty), penalty, points);
    }

    private static List<Location> WalkPool() => new()
    {
        Make("Sidewalk", "The morning sun is already brutal.", "wipe sweat and keep walking", Difficulty.Medium),
        Make("Muddy Shortcut", "Last night's rain left the shortcut a mess.", "step around the mud", Difficulty.Medium),
        Make("Stray Dogs", "A pack of dogs is barking near the fence.", "walk past them calmly", Difficulty.Hard),
        Make("Umbrella Check", "Clouds are rolling in fast.", "open the umbrella", Difficulty.Easy),
        Make("Sari-Sari Store", "You forgot to buy load and snacks.", "grab snacks quickly", Difficulty.Medium),
        Make("Crossing the Highway", "No stoplight, just timing and nerve.", "cross when it is clear", Difficulty.Hard),
        Make("Loose Shoelace", "Your shoelace snapped mid-stride.", "retie the shoelace fast", Difficulty.Medium),
        Make("Flooded Alley", "Yesterday's rain left the alley knee-deep.", "wade through the flood", Difficulty.Hard),
        Make("Barking Rooster", "The neighbor's rooster won't stop crowing at you.", "shoo the rooster away", Difficulty.Easy),
        Make("Tricycle Offer", "A tricycle driver honks, offering a ride you can't afford.", "wave him off politely", Difficulty.Medium),
    };

    private static List<Location> JeepPool() => new()
    {
        Make("Waiting Shed", "Three jeeps pass by, all of them full.", "wave down the next jeep", Difficulty.Medium),
        Make("Boarding", "Everyone is squeezing in at once.", "squeeze inside quickly", Difficulty.Hard),
        Make("Paying Fare", "Pass your fare down the line correctly.", "pass bayad po", Difficulty.Easy),
        Make("Getting Change", "The driver is asking for exact change.", "count exact change", Difficulty.Medium),
        Make("Traffic Jam", "The jeep is stuck at a busy intersection.", "wait out the jam", Difficulty.Medium),
        Make("Calling the Stop", "Your stop is coming up fast.", "para po sa tabi", Difficulty.Hard),
        Make("Squeezed Seat", "You're wedged between two sacks of rice.", "shift into the seat", Difficulty.Medium),
        Make("Conductor's Whistle", "The conductor blows the whistle - go time.", "hop off the jeep", Difficulty.Medium),
        Make("Overloaded Jeep", "The jeep is packed way past its limit.", "hold on to the bar", Difficulty.Hard),
        Make("Rainy Ride", "Rain splatters in through the open sides.", "pull down the tarp", Difficulty.Easy),
    };

    private static List<Location> BusPool() => new()
    {
        Make("Terminal Line", "The queue for the bus is long today.", "line up at the terminal", Difficulty.Medium),
        Make("Ticket Counter", "The clerk needs your destination fast.", "buy a ticket now", Difficulty.Medium),
        Make("Standing Room Only", "Every seat is taken, hang on tight.", "grab the handrail", Difficulty.Hard),
        Make("Missed Announcement", "The driver just called your stop.", "press the stop button", Difficulty.VeryHard),
        Make("Aircon Malfunction", "It is stuffy and everyone is annoyed.", "fan yourself and wait", Difficulty.Easy),
        Make("Road Checkpoint", "A checkpoint is slowing everything down.", "show your school id", Difficulty.Medium),
        Make("Luggage Rack", "Your bag keeps sliding off the rack.", "wedge the bag in tight", Difficulty.Medium),
        Make("Bus Rocking", "The bus sways hard around a sharp curve.", "brace against the seat", Difficulty.Hard),
        Make("Loud Phone Call", "A passenger is on speakerphone right behind you.", "put in your earphones", Difficulty.Easy),
        Make("Overshot Stop", "You almost missed your stop entirely.", "rush to the front door", Difficulty.VeryHard),
    };

    private static List<Location> MotorcyclePool() => new()
    {
        Make("Gas Station", "The tank is almost empty.", "fill up the tank", Difficulty.Medium),
        Make("Helmet Strap", "You forgot to strap your helmet.", "buckle the helmet strap", Difficulty.Medium),
        Make("Weaving Traffic", "Cars are packed tighter than usual.", "weave through carefully", Difficulty.VeryHard),
        Make("Red Light", "The light will not turn green fast enough.", "wait for the green light", Difficulty.Easy),
        Make("Pothole Alley", "This road is famous for its potholes.", "dodge the potholes", Difficulty.Hard),
        Make("Rain Check", "It just started drizzling out of nowhere.", "put on the rain coat", Difficulty.Medium),
        Make("Kickstand Down", "You almost forgot to lock the kickstand.", "flip the kickstand up", Difficulty.Medium),
        Make("Traffic Enforcer", "An enforcer flags you down for a quick check.", "show your license fast", Difficulty.Hard),
        Make("Overtaking Truck", "A slow truck is blocking the lane ahead.", "overtake when it is clear", Difficulty.Hard),
        Make("Wet Visor", "Your visor fogs up in the morning mist.", "wipe the visor clean", Difficulty.Easy),
    };
}