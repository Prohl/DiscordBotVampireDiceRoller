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
      private set;
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
    /// <param name="_reroll">Can the check rerolled if the first one ist not successful?</param>
    /// <returns>A nice message</returns>
    public string Rouse(bool _reroll)
    {
      if (VampireDiceRollerController.Rouse(_reroll))
      {
        // Check successful - Everythings fine
        return "You have your beast under control";
      }
      else
      {
        // Hunger increases to a maximum of 5!
        this.Hunger++;
        // Return a message depending on the new Hunger
        return this.GetMessageForHunger();
      }
    }

    /// <summary>
    /// Sets the Hunger for the character
    /// </summary>
    /// <param name="_hunger">The difference or the new hunger value</param>
    /// <param name="_modify">if the value should be added to the hunger (true) or set as new hunger (false)</param>
    /// <returns></returns>
    public string SetHunger(int _hunger, bool _modify)
    {
      if (_modify)
      {
        this.Hunger += _hunger;
      }
      else
      {
        this.Hunger = _hunger;
      }

      // Normalize the hunger to a value between 0 and 6
      if (this.Hunger < 0)
      {
        this.Hunger = 0;
      }
      else if (this.Hunger > 6)
      {
        this.Hunger = 6;
      }

      return this.GetMessageForHunger();
    }

    /// <summary>
    /// Returns a nice text with the characters name and his hunger
    /// </summary>
    /// <returns></returns>
    public string GetMessageForHunger()
    {
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

    private string NameWithGenetivS
    {
      get
      {
        return this.Name + "'s";
      }
    }
  }
}
