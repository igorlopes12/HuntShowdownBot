using Discord;
using Discord.WebSocket;
using HuntShowdownBot.Types;
using System.Xml;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient _client;
    private ulong _guildId = 1198618069052948561;

    public async Task MainAsync()
    {
        var token = Environment.GetEnvironmentVariable("DiscordToken", EnvironmentVariableTarget.User);

        var options = new DiscordSocketConfig()
        {
            UseInteractionSnowflakeDate = false
        };

        _client = new DiscordSocketClient(options);
        _client.Log += Log;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        _client.Ready += Ready;
        _client.SlashCommandExecuted += SlashCommandExecuted;

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }

    private async Task Ready()
    {
        var guild = _client.GetGuild(_guildId);

        var guildCommand = new SlashCommandBuilder()
        .WithName("mmr")
        .WithDescription("Command to retrieve your MMR.")
        .AddOption("file", ApplicationCommandOptionType.Attachment, "The file to parse.", true);

        await guild.CreateApplicationCommandAsync(guildCommand.Build());
    }

    private async Task SlashCommandExecuted(SocketSlashCommand command)
    {
        var option = command.Data.Options.First(o => o.Type == ApplicationCommandOptionType.Attachment).Value as Attachment; // I'm not checking for nulls here, you should!

        string url = option.Url;
        var xml = await GetFileAttachment(url);

        HuntMatch huntMatch;
        try
        {
            huntMatch = XmlParse(xml);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"O filho da puta \"{command.User.Username}\" mandou um arquivo nada a vê");

            var resposta = new EmbedBuilder()
                .WithTitle("Invalid File")
                .WithDescription("FILHO DA PUTA, MANDA O BAGULHO DIREITO")
                .WithColor(255, 0, 0);

            await command.RespondAsync(embed: resposta.Build(), ephemeral: true);
            return;
        }

        var embed = new EmbedBuilder()
            .WithTitle("Team MMR")
            .WithFooter("This show the MMR of each player in the team.")
            .WithColor(255, 0, 0);

        foreach (var team in huntMatch.Teams)
        {
            embed.AddField($"Team {team.Id + 1}", $"`{team.MMR}`", true);
            embed.AddField($"Players", $"`{string.Join("\n", team.Players.Select(player => $"{player.Name}"))}`", true);
            embed.AddField($"MMR", $"`{string.Join("\n", team.Players.Select(player => $"{player.MMR}"))}`", true);
            embed.AddField("** **", "** **", false);
        }

        await command.RespondAsync(embed: embed.Build(), ephemeral: true);

        async Task<string> GetFileAttachment(string url)
        {
            var httpClient = new HttpClient();
            var responseStream = await httpClient.GetStreamAsync(url);
            using var reader = new StreamReader(responseStream);
            return reader.ReadToEnd();
        }
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private HuntMatch XmlParse(string xml)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(xml);

        XmlNode mainNode = doc.SelectSingleNode("Attributes")!;

        int numberOfTeams = int.Parse(mainNode.SelectSingleNode("//Attr[@name=\"MissionBagNumTeams\"]")!.Attributes!["value"]!.Value);

        var teams = new List<HuntTeam>();
        for (int i = 0; i < numberOfTeams; i++)
        {
            int numberOfPlayers = int.Parse(mainNode.SelectSingleNode($"//Attr[@name=\"MissionBagTeam_{i}_numplayers\"]")!.Attributes!["value"]!.Value);
            int teamMMR = int.Parse(mainNode.SelectSingleNode($"//Attr[@name=\"MissionBagTeam_{i}_mmr\"]")!.Attributes!["value"]!.Value);

            var players = new List<HuntPlayer>();
            for (int j = 0; j < numberOfPlayers; j++)
            {
                var playerId = long.Parse(mainNode.SelectSingleNode($"//Attr[@name=\"MissionBagPlayer_{i}_{j}_profileid\"]")!.Attributes!["value"]!.Value);
                var playerName = mainNode.SelectSingleNode($"//Attr[@name=\"MissionBagPlayer_{i}_{j}_blood_line_name\"]")!.Attributes!["value"]!.Value;
                var playerMMR = int.Parse(mainNode.SelectSingleNode($"//Attr[@name=\"MissionBagPlayer_{i}_{j}_mmr\"]")!.Attributes!["value"]!.Value);

                players.Add(new HuntPlayer(playerId, playerName, playerMMR));
            }

            teams.Add(new HuntTeam(i, players, teamMMR));
        }

        return new HuntMatch(teams);
    }
}