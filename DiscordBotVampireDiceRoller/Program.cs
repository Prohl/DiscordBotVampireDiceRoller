using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    private Dictionary<SocketUser, VampireCharacter> dicChar4User = new Dictionary<SocketUser, VampireCharacter>();

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
      if (message.MentionedUsers.FirstOrDefault(user => user.Id == this.discordClient.CurrentUser.Id) != null)
      {
        // Remove all mentions from the content
        Regex regi = new Regex(@"<[@|#][!]?[\w]*>");
        string strMessage = regi.Replace(message.Content, "").Trim();
        // The Bot knows roll, reroll and help
        if (strMessage.StartsWith("roll", StringComparison.OrdinalIgnoreCase))
        {
          // Split the string with Blanks
          string[] strSplit = strMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
          // We're expecting 3 values or 2 and an initialized character
          VampireCharacter chara = null;
          int intDiceTotal, intDiceRed;
          if ((strSplit.Length == 2 && this.dicChar4User.TryGetValue(message.Author, out chara)) || strSplit.Length == 3)
          {
            // We ignore the first one (it should be roll) and cast the rest to integer
            if (Int32.TryParse(strSplit[1], out intDiceTotal) == false)
            {
              await message.Channel.SendMessageAsync($"{message.Author.Mention} Unable to cast '{strSplit[1]}' to a number");
            }
            if (strSplit.Length == 3)
            {
              if (Int32.TryParse(strSplit[2], out intDiceRed) == false)
              {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} Unable to cast '{strSplit[2]}' to a number");
              }
            }
            else
            {
              intDiceRed = chara.Hunger;
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
            await message.Channel.SendMessageAsync($"{message.Author.Mention} I found no roll for you to reroll.");
          }
        }
        else if (strMessage.StartsWith("initchar", StringComparison.OrdinalIgnoreCase))
        {
          if (this.dicChar4User.ContainsKey(message.Author))
          {
            await message.Channel.SendMessageAsync($"{message.Author.Mention} Your character ist already initialized.");
          }
          else
          {
            // Read the stats from the input
            string[] strSplit = strMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int intHunger;
            // In the moment we expect the command, the name and the hunger - that makes 3 
            if (strSplit.Length < 3)
            {
              await message.Channel.SendMessageAsync($"{message.Author.Mention} Not enough parameters to create your character. Please use 'initchar name hunger'");
            }
            else if (int.TryParse(strSplit[2], out intHunger) == false)
            {
              await message.Channel.SendMessageAsync($"{message.Author.Mention} I'm very sorry, but I cannot read a number from '{strSplit[2]}'.");
            }
            else
            {
              // If the hunger ist out of the allowed range tell it to the user und set hunger to 1
              if (intHunger < 0 || intHunger > 5)
              {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} Hunger must be a number between 0 und 5 but you gave me {intHunger}. Your hunger has been set to 1.");
                intHunger = 1;
              }
              string strName = strSplit[1];
              int intHugner = intHunger;
              this.dicChar4User.Add(message.Author, new VampireCharacter(strName, intHugner));
              await message.Channel.SendMessageAsync($"{message.Author.Mention} Hello {strName}. Nice to meet you.");
            }
          }
        }
        else if (strMessage.StartsWith("rouse", StringComparison.OrdinalIgnoreCase))
        {
          if (this.dicChar4User.ContainsKey(message.Author) == false)
          {
            await message.Channel.SendMessageAsync($"{message.Author.Mention} Your character ist not initialized. I can't rouse your blood.");
          }
          else
          {
            VampireCharacter vampChar = this.dicChar4User[message.Author];
            await message.Channel.SendMessageAsync($"{message.Author.Mention} {vampChar.Rouse()}");
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
      if (System.IO.File.Exists(@"chatter.txt"))
      {
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
}
