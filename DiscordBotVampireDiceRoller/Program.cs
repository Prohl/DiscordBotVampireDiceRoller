using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
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
        if (message.Content.Contains("test", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("Nein!");
        }
        else if (message.Content.Contains("doch", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("Oh!");
        }
        else if (message.Content.Contains("frank", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/drac-dracula-vampire-horror-shadow-gif-4888147");
        }
        else if (message.Content.Contains("federhauser", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/vampire-dracula-mr-burns-fangs-gif-5730463");
        }
        else if (message.Content.Contains("nacht", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/nesforatu-dracula-goodnight-gif-10044942");
        }
        else if (message.Content.Contains("morgen", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/wake-up-dracula-hangover-leslie-nielsen-gif-14211396");
        }
        else if (message.Content.Contains("cool", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/bela-lugosi-dracula-vampire-halloween-horror-gif-15453044");
        }
        else if (message.Content.Contains("zahl", StringComparison.OrdinalIgnoreCase) || message.Content.Contains("zählen", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/number-of-the-day-sesame-street-gif-14837388");
        }
        else if (message.Content.Contains("ronja", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/cat-bite-gif-7748718");
        }
        else if (message.Content.Contains("biss", StringComparison.OrdinalIgnoreCase) || message.Content.Contains("beißen", StringComparison.OrdinalIgnoreCase) || message.Content.Contains("beissen", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/kermit-vampire-gif-3563348");
        }
        else if (message.Content.Contains("nobody", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/alan-walker-hacker-gif-7953330");
        }
        else if (message.Content.Contains("dracula", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync("https://tenor.com/view/hellsing-vampire-anime-gif-9676269");
        }
      }
    }
  }
}
