using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotVampireDiceRoller
{
  /// <summary>
  /// A little clas to manage some stats of a character
  /// </summary>
  public class VampireCharacter
  {
    /// <summary>
    /// Konstruktor
    /// </summary>
    /// <param name="_name"></param>
    /// <param name="_hunger"></param>
    public VampireCharacter(
      string _name,
      int _hunger)
    {
      this.Name = _name;
      this.Hunger = _hunger;
    }

    /// <summary>
    /// The characters name
    /// </summary>
    public string Name
    {
      get;
      set;
    }

    /// <summary>
    /// The characters current Hunger
    /// </summary>
    public int Hunger
    {
      get;
      set;
    }

    /// <summary>
    /// The Characters current superficial Willpower
    /// </summary>
    public int WillpowerSup
    {
      get;
      set;
    }

    /// <summary>
    /// The Characters current aggrevated Willpower
    /// </summary>
    public int WillpowerAgg
    {
      get;
      set;
    }

    /// <summary>
    /// Makes a Rousecheck for the Character
    /// Increases the Hunger if not successfull
    /// Returns a Text that can be send to the user
    /// </summary>
    /// <returns>A nice message</returns>
    public string Rouse()
    {
      if (VampireDiceRollerController.Rouse())
      {
        // Check successful - Everythings fine
        return "You have your beast under control";
      }
      else
      {
        // Hunger increases to a maximum of 5!
        this.Hunger++;
        // Return a message depending on the new Hunger
        switch (Hunger)
        {
          case 0:
            return $"{this.NameWithGenetivS} hunger is at 0, your beast is satisfied.";
          case 1:
            return $"{this.NameWithGenetivS} hunger is at 1, the beast awakens.";
          case 2:
            return $"{this.NameWithGenetivS} hunger is at 2, you can hear your beast clearly in the back of your head.";
          case 3:
            return $"{this.NameWithGenetivS} hunger is at 3, you can feel the beast clawing it's way up.";
          case 4:
            return $"{this.NameWithGenetivS} hunger is at 4, the beast demands payment in blood.";
          case 5:
            return $"{this.NameWithGenetivS} hunger is at 5, it's nearly impossible to ignore the beast.";
          case 6:
            // Reduce the hunger back to 5
            this.Hunger = 5;
            return $"**The beast takes control, {this.Name} enters a frenzy!**";
          default:
            return "";
        }
      }
    }

    private string NameWithGenetivS
    {
      get
      {
        return this.Name + "'s";
      }
    }
  }
}
