using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBotVampireDiceRoller
{
  /// <summary>
  /// MainClass
  /// </summary>
  public class Program
  {
    private DiscordSocketClient discordClient;

    private Dictionary<SocketUser, VampireDiceRoll> dicLastRoll4User = new Dictionary<SocketUser, VampireDiceRoll>();

    Dictionary<string, string> chatter;

    /// <summary>
    /// MainMethod
    /// </summary>
    /// <param name="args"></param>
    public static void Main(string[] args)
    => new Program().MainAsync().GetAwaiter().GetResult();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task MainAsync()
    {
      this.readChatter();
      
      this.discordClient = new DiscordSocketClient();
      this.discordClient.Log += Log;
      this.discordClient.MessageReceived += MessageReceived;

      // Token
      string strToken = DiscordBotVampireDiceRoller.Properties.Resources.TOKEN;

      await this.discordClient.LoginAsync(TokenType.Bot, strToken);
      await this.discordClient.StartAsync();

      // Block this task until the program is closed.
      await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
      Console.WriteLine(msg.ToString());
      return Task.CompletedTask;
    }

    private async Task MessageReceived(SocketMessage message)
    {
      if (message.Content.StartsWith(this.discordClient.CurrentUser.Mention))
      {
        // The Bot is mentioned in the message - Parse it
        string strMessage = message.Content.Replace(this.discordClient.CurrentUser.Mention, "").Trim();
        // The Bot knows roll, reroll and help
        if (strMessage.StartsWith("roll", StringComparison.OrdinalIgnoreCase))
        {
          // Split the string with Blanks
          string[] strSplit = strMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
          // We're expecting 3 values now
          if (strSplit.Length == 3)
          {
            // We ignore the first one (it should be roll) and cast the rest to integer
            int intDiceTotal, intDiceRed;
            if (Int32.TryParse(strSplit[1], out intDiceTotal) == false)
            {
              await message.Channel.SendMessageAsync($"{message.Author.Mention} Unable to cast '{strSplit[1]}' to a number");
            }
            if (Int32.TryParse(strSplit[2], out intDiceRed) == false)
            {
              await message.Channel.SendMessageAsync($"{message.Author.Mention} Unable to cast '{strSplit[2]}' to a number");
            }

            VampireDiceRoll diceRoll = new VampireDiceRoll(intDiceTotal, intDiceRed);
            this.dicLastRoll4User[message.Author] = diceRoll;

            await message.Channel.SendMessageAsync($"{message.Author.Mention} {diceRoll.Results}");
          }
          else
          {
            // Haven't found what we we're looking for
            await message.Channel.SendMessageAsync($"{message.Author.Mention} Somethings not right with your roll command");
          }
        }
        else if (strMessage.StartsWith("reroll", StringComparison.OrdinalIgnoreCase))
        {
          VampireDiceRoll diceRoll;
          if (this.dicLastRoll4User.TryGetValue(message.Author, out diceRoll))
          {
            // Check if the user wants to reroll something specific
            string[] strSplit = strMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string strRerollOptions = string.Empty;
            if (strSplit.Length > 1)
            {
              // In the second entry can be specific dice to reroll
              strRerollOptions = strSplit[1];
            }

            await message.Channel.SendMessageAsync($"{message.Author.Mention} {diceRoll.Reroll(strRerollOptions)}");
          }
          else
          {
            // No roll found for the user...
            await message.Channel.SendMessageAsync($"{message.Author.Mention} I found no roll for you to reroll");
          }
        }
        else if (strMessage.StartsWith("help", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync($"{message.Author.Mention} You can tell me to roll the dice by telling me \"roll {{number of total dice}} {{number of red dice}}\" (\"roll 7 2\")" +
            $"{Environment.NewLine}or \"reroll\" to reroll up to three normal dice from your last roll. Without specification a reroll will reroll failures. " +
            $"You can specify what you want to reroll by adding 'c' for a critical, 's' for an success or 'f' for a failure (reroll cff).");
        }
        else
        {
          await message.Channel.SendMessageAsync($"{message.Author.Mention} I don't understand. Type \"help\" when you need some help from me");
        }
      }

      if (message.Author.IsBot == false)
      {
        // Response to see, that the Bot is still responding
        foreach (KeyValuePair<string,string> pair in chatter)
        {
          if (message.Content.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
          {
            await message.Channel.SendMessageAsync(pair.Value);
            break;
          }
        }
      }
    }

    private void readChatter()
    {
      // Read chatter file
      string[] chatterLines = System.IO.File.ReadAllLines(@"chatter.txt");

      // Write chatter file into dictionary
      this.chatter = new Dictionary<string, string>();

      foreach (string line in chatterLines)
      {
        string[] strLineSplit = line.Split(':');
        if (strLineSplit.Length > 1)
        {
          StringBuilder builder = new StringBuilder();
          builder.Append(strLineSplit[1]);
          for (int index = 2; index < strLineSplit.Length; index++)
          {
            builder.Append(':');
            builder.Append(strLineSplit[index]);
          }

          chatter.Add(strLineSplit[0], builder.ToString());
        }
        
      }
    }
  }
}
