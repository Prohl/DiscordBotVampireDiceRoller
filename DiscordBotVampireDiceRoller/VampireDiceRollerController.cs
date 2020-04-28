using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotVampireDiceRoller
{
  /// <summary>
  /// Class for several things
  /// </summary>
  public static class VampireDiceRollerController
  {
    /// <summary>
    /// The value you need to achive for a success on a roll
    /// </summary>
    public static int MIN_VALUE_SUCCESS = 6;

    /// <summary>
    /// The Randoomizer that's being used
    /// </summary>
    private static Random Randy = new Random((int)DateTime.Now.Ticks);

    /// <summary>
    /// Rolls a d10 and returns the result
    /// </summary>
    /// <returns></returns>
    public static int RollD10()
    {
      return Randy.Next(1, 11);
    }

    /// <summary>
    /// Makes a "rouse" check and returns if it was successfull
    /// </summary>
    /// <returns></returns>
    public static bool Rouse()
    {
      return RollD10() >= MIN_VALUE_SUCCESS;
    }
  }
}
