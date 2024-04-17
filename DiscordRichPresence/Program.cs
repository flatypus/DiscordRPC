using Discord;
using System.Text.Json;

class Config
{
    public required long clientID { get; set; }
    public required string state { get; set; }
    public string? details { get; set; }
    public long? startTimestamp { get; set; }
    public string? largeImage { get; set; }
    public string? largeText { get; set; }
    public string? smallImage { get; set; }
    public string? smallText { get; set; }
}

class ConfigReader
{
    public Config config;
    public Config readConfig()
    {
        string text = File.ReadAllText(@"./config.json");
        var configResult = JsonSerializer.Deserialize<Config>(text);
        if (configResult == null)
        {
            throw new Exception("Failed to parse config.json");
        }
        config = configResult;
        return configResult;
    }

    public ConfigReader()
    {
        readConfig();
    }
}


class DiscordActivityUpdater
{
    private ConfigReader configReader;
    private static ActivityManager activityManager;

    public DiscordActivityUpdater()
    {
        configReader = new ConfigReader();
        Console.WriteLine("Starting Discord Activity Updater");
        var discord = new Discord.Discord(configReader.config.clientID, (ulong)CreateFlags.NoRequireDiscord);
        activityManager = discord.GetActivityManager();

        SetActivity();
        while (true)
        {
            SetActivity();
            discord.RunCallbacks();
            Task.Delay(1000).Wait();
        }
    }

    void SetActivity()
    {
        var config = configReader.readConfig();
        var startTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
        var activity = new Activity
        {
            State = config.state,
            Details = config.details ?? "",
            Timestamps = { Start = config.startTimestamp ?? startTimestamp },
            Assets =
            {
                LargeImage = config.largeImage ?? "",
                LargeText = config.largeText ?? "",
                SmallImage = config.smallImage ?? "",
                SmallText = config.smallText ?? "",
            },
            Instance = true,
        };

        activityManager.UpdateActivity(activity, (result) =>
        {
            if (result == Result.Ok)
            {
                Console.WriteLine("Success!");
            }
            else
            {
                Console.WriteLine("Failed");
            }
        });
    }
}

class Program
{
    public static void Main()
    {
        new DiscordActivityUpdater();
    }
}