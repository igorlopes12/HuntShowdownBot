using Discord;
using Discord.WebSocket;
using System.Xml;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient _client;
    private ulong _guildId = 1198618069052948561;

    private string dir = "D:\\SteamLibrary\\steamapps\\common\\Hunt Showdown\\user\\profiles\\default\\attributes.xml";
 


    public async Task MainAsync()
    {
        var token = Environment.GetEnvironmentVariable("TokenHuntBot",EnvironmentVariableTarget.User);      

        
        _client = new DiscordSocketClient();
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

        var guildCommand = new SlashCommandBuilder();
        guildCommand.WithName("mmr");
        guildCommand.WithDescription("Command to retrieve your MMR.");


        await guild.CreateApplicationCommandAsync(guildCommand.Build());
    }

    private async Task SlashCommandExecuted(SocketSlashCommand command)
    {
        await command.RespondAsync(XmlParse());
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    private string XmlParse()
    {
        string output = "";

        XmlDocument doc = new XmlDocument();
        doc.Load(dir);

        XmlNode mainNode = doc.SelectSingleNode("Attributes");
        XmlNodeList players = mainNode.SelectNodes("//Attr[starts-with(@name, \"MissionBagPlayer\") and contains(@name, \"blood_line_name\")]");
        XmlNodeList mmrs = mainNode.SelectNodes("//Attr[starts-with(@name, \"MissionBagPlayer\") and contains(@name, \"mmr\")]");


        foreach (XmlNode node in players)
        {
            string teamPlayerIds = node.Attributes["name"].Value.Substring(17, 3);
            XmlNode mmr = mmrs.Cast<XmlNode>().Where(m => m.Attributes["name"].Value.Substring(17,3) == teamPlayerIds).FirstOrDefault();
            string teamId = teamPlayerIds.Substring(0, 1);
            
           

            output += $"Team:{teamId} {node.Attributes["value"].Value} {mmr.Attributes["value"].Value} {Environment.NewLine}";
        }
        return output;
    }
    

}