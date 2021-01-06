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

    private Dictionary<string, VampireDiceRoll> dicLastRoll4User = new Dictionary<string, VampireDiceRoll>();
    private Dictionary<string, VampireCharacter> dicChar4User = new Dictionary<string, VampireCharacter>();

    private Dictionary<string, string> dicChatter;

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
      this.ReadChatter();
      
      this.discordClient = new DiscordSocketClient();
      this.discordClient.Log += Log;
      this.discordClient.MessageReceived += MessageReceived;

      // Token
      string strToken = Properties.Resources.TOKEN;

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
      if (message.MentionedUsers.FirstOrDefault(user => GetUniqueDiscriminator(user) == GetUniqueDiscriminator(this.discordClient.CurrentUser)) != null)
      {
        // Remove all mentions from the content
        Regex regi = new Regex(@"<[@|#][!]?[\w]*>");
        string strMessage = regi.Replace(message.Content, "").Trim();
        // The Bot knows roll, reroll and help
        if (strMessage.StartsWith("roll", StringComparison.OrdinalIgnoreCase))
        {
          #region Roll
          // Split the string with Blanks
          string[] strSplit = strMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
          // We're expecting 3 values or 2 and an initialized character
          VampireCharacter chara = null;
          int intDiceTotal, intDiceRed;
          if ((strSplit.Length == 2 && this.dicChar4User.TryGetValue(GetUniqueDiscriminator(message.Author), out chara)) || strSplit.Length == 3)
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
            this.dicLastRoll4User[GetUniqueDiscriminator(message.Author)] = diceRoll;

            await message.Channel.SendMessageAsync($"{message.Author.Mention} {diceRoll.Results}");
          }
          else
          {
            // Haven't found what we we're looking for
            await message.Channel.SendMessageAsync($"{message.Author.Mention} Somethings not right with your roll command");
          }
          #endregion
        }
        else if (strMessage.StartsWith("reroll", StringComparison.OrdinalIgnoreCase))
        {
          #region Reroll
          VampireDiceRoll diceRoll;
          if (this.dicLastRoll4User.TryGetValue(GetUniqueDiscriminator(message.Author), out diceRoll))
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
          #endregion
        }
        else if (strMessage.StartsWith("initchar", StringComparison.OrdinalIgnoreCase))
        {
          #region InitChar
          if (this.dicChar4User.ContainsKey(GetUniqueDiscriminator(message.Author)))
          {
            await message.Channel.SendMessageAsync($"{message.Author.Mention} Your character is already initialized.");
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
              this.dicChar4User.Add(GetUniqueDiscriminator(message.Author), new VampireCharacter(strName, intHugner));
              await message.Channel.SendMessageAsync($"{message.Author.Mention} Hello {strName}. Nice to meet you.");
            }
          }
          #endregion
        }
        else if (strMessage.StartsWith("kill", StringComparison.OrdinalIgnoreCase))
        {
          #region kill
          VampireCharacter character;
          if (this.dicChar4User.TryGetValue(GetUniqueDiscriminator(message.Author), out character))
          {
            this.dicChar4User.Remove(GetUniqueDiscriminator(message.Author));
            await message.Channel.SendMessageAsync($"{message.Author.Mention} {character.Name} has met his final death.");
          }
          else
          {
            await message.Channel.SendMessageAsync($"{message.Author.Mention} I can't kill something I cannot see.");
          }
          #endregion
        }
        else if (strMessage.StartsWith("rouse", StringComparison.OrdinalIgnoreCase))
        {
          #region Rouse
          // Check if the check can bererolled
          bool bolReroll = strMessage.StartsWith("rouse+", StringComparison.OrdinalIgnoreCase);
          if (this.dicChar4User.ContainsKey(GetUniqueDiscriminator(message.Author)) == false)
          {
            if (VampireDiceRollerController.Rouse(bolReroll))
            {
              await message.Channel.SendMessageAsync($"{message.Author.Mention} You are fine.");
            }
            else
            {
              await message.Channel.SendMessageAsync($"{message.Author.Mention} Your hunger increases.");
            }
          }
          else
          {
            VampireCharacter vampChar = this.dicChar4User[GetUniqueDiscriminator(message.Author)];
            await message.Channel.SendMessageAsync($"{message.Author.Mention} {vampChar.Rouse(bolReroll)}");
          }
          #endregion
        }
        else if (strMessage.StartsWith("hunger", StringComparison.OrdinalIgnoreCase))
        {
          #region hunger
          // Hunger makes only sense with an initialized character
          VampireCharacter character;
          if (this.dicChar4User.TryGetValue(GetUniqueDiscriminator(message.Author), out character))
          {
            // Hunger needs to be followed by something
            string[] strSplit = strMessage.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (strSplit.Length == 1)
            {
              await message.Channel.SendMessageAsync($"{message.Author.Mention} {character.GetMessageForHunger()}");
            }
            else
            {
              // We only care about the second string in the array - check if it starts with +/-
              bool bolModusModify = false;
              if (strSplit[1].StartsWith('+') || strSplit[1].StartsWith('-'))
              {
                bolModusModify = true;
              }

              // Try to parse the string to a number
              int intHunger;
              if (Int32.TryParse(strSplit[1], out intHunger))
              {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} {character.SetHunger(intHunger, bolModusModify)}");
              }
              else
              {
                await message.Channel.SendMessageAsync($"{message.Author.Mention} I cannot get a number from '{strSplit[1]}'.");
              }
            }
          }
          else
          {
            await message.Channel.SendMessageAsync($"{message.Author.Mention} I have no character for you. Only you can get hungrier.");
          }
          #endregion
        }
        else if (strMessage.StartsWith("about", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync($"Vampire Dice Roller Version '{this.GetType().Assembly.GetName().Version}' under GPL-3.0 License.{Environment.NewLine}" +
            $"Source code can be found under: https://github.com/Prohl/DiscordBotVampireDiceRoller{Environment.NewLine}");
        }
        else if (strMessage.StartsWith("help", StringComparison.OrdinalIgnoreCase))
        {
          await message.Channel.SendMessageAsync($"{message.Author.Mention} I understand the following commands:{Environment.NewLine}" +
            $"- roll: let me roll some dice for you. You can either tell me to roll a number of dice and hungerdice with (roll X Y) or if you have an initialized character simply (roll X) and I will get the hungerdice from your characters hunger{Environment.NewLine}" +
            $"- reroll: I will reroll up to three normal dice from your last roll. Without specification a reroll will reroll failures. " +
            $"You can specify what you want to reroll by adding 'c' for a critical, 's' for an success or 'f' for a failure (reroll cff).{Environment.NewLine}" +
            $"- initchar: followed by a name and your current hunger (initchar foo 2). Initializes your character.{Environment.NewLine}" +
            $"- kill: removes your current character and you can initialize a new one.{Environment.NewLine}" +
            $"- rouse: makes a rousecheck. When your character is initialized I tell you your new hunger, if it increases.{Environment.NewLine}" +
            $"- hunger: used to modify, set or get your hunger. To set it simply type hunger X and I will set your hunger to X. When you write +/- before X I will modify your hunger by X. Just hunger will reveal your current hunger.");
        }
        else
        {
          await message.Channel.SendMessageAsync($"{message.Author.Mention} I don't understand. Type \"help\" when you need some help from me.");
        }
      }

      if (message.Author.IsBot == false)
      {
        // Response to see, that the Bot is still responding
        foreach (KeyValuePair<string,string> pair in dicChatter)
        {
          if (message.Content.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
          {
            await message.Channel.SendMessageAsync(pair.Value);
            break;
          }
        }
      }
    }

    private void ReadChatter()
    {
      // Read chatter file
      if (System.IO.File.Exists(@"chatter.txt"))
      {
        string[] chatterLines = System.IO.File.ReadAllLines(@"chatter.txt");

        // Write chatter file into dictionary
        this.dicChatter = new Dictionary<string, string>();

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

            dicChatter.Add(strLineSplit[0], builder.ToString());
          }

        }
      }
      
    }

    /// <summary>
    /// Return username#discriminator of the user
    /// </summary>
    /// <param name="_user"></param>
    /// <returns></returns>
    private string GetUniqueDiscriminator(SocketUser _user)
    {
      return $"{_user.Username}#{_user.Discriminator}";
    }
  }
}
