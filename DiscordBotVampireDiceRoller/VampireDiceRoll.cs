using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBotVampireDiceRoller
{
  /// <summary>
  /// Class for a VampireDiceRoll
  /// </summary>
  public class VampireDiceRoll
  {
    private List<int> lstResultsNormal, lstResultsRed;
    private bool bolRerolled = false;

    /// <summary>
    /// Konstruktor - Takes the number of total dice and red dice
    /// </summary>
    /// <param name="_diceTotal"></param>
    /// <param name="_diceRed"></param>
    public VampireDiceRoll(int _diceTotal, int _diceRed)
    {
      // Initialize the resultslists
      this.lstResultsNormal = new List<int>(_diceTotal - _diceRed);
      this.lstResultsRed = new List<int>(_diceRed);

      // Do the rolling
      for (int intCount = 0; intCount < _diceTotal - _diceRed; intCount++)
      {
        this.lstResultsNormal.Add(VampireDiceRollerController.RollD10());
      }

      for (int intCount = 0; intCount < _diceRed; intCount++)
      {
        this.lstResultsRed.Add(VampireDiceRollerController.RollD10());
      }
    }

    /// <summary>
    /// Rerolling
    /// </summary>
    /// <param name="_rerollOptions"></param>
    /// <returns></returns>
    public string Reroll(string _rerollOptions)
    {
      if (this.bolRerolled == false)
      {
        // We can only reroll up to 3 dices
        int intRerolls = 0;

        if (string.IsNullOrEmpty(_rerollOptions))
        {
          // Iterate over the normal dices
          for (int intIndex = 0; intIndex < this.lstResultsNormal.Count; intIndex++)
          {
            // Check if not a success
            if (intRerolls < 3 && this.lstResultsNormal[intIndex] < VampireDiceRollerController.MIN_VALUE_SUCCESS)
            {
              this.lstResultsNormal[intIndex] = VampireDiceRollerController.RollD10();
              intRerolls++;
            }
          }
        }
        else
        {
          
          List<int> lstIndexRerolledDice = new List<int>();

          // First Check if there are reroll options
          foreach (char optionAktuell in _rerollOptions)
          {
            // Iterate over the normal dices
            for (int intIndex = 0; intIndex < this.lstResultsNormal.Count; intIndex++)
            {
              Func<int, bool> funcCompare;
              if (char.ToUpperInvariant(optionAktuell) == char.ToUpperInvariant('c'))
              {
                funcCompare = (intValue => intValue == 10);
              }
              else if (char.ToUpperInvariant(optionAktuell) == char.ToUpperInvariant('s'))
              {
                funcCompare = (intValue => intValue >= VampireDiceRollerController.MIN_VALUE_SUCCESS);
              }
              else if (char.ToUpperInvariant(optionAktuell) == char.ToUpperInvariant('f'))
              {
                funcCompare = (intValue => intValue < VampireDiceRollerController.MIN_VALUE_SUCCESS);
              }
              else
              {
                continue;
              }
              // Check if the option is met
              if (intRerolls < 3 && lstIndexRerolledDice.Contains(intIndex) == false && funcCompare(this.lstResultsNormal[intIndex]))
              {
                this.lstResultsNormal[intIndex] = VampireDiceRollerController.RollD10();
                lstIndexRerolledDice.Add(intIndex);
                intRerolls++;
                break;
              }
            }
          }
        }
        

        this.bolRerolled = true;

        return Results;
      }
      else
      {
        // Already rerolled
        return "Already rerolled";
      }
    }

    /// <summary>
    /// Gets a formated string with the resulst from the roll
    /// </summary>
    public string Results
    {
      get
      {
        // Run over the results and count everything
        int intNormalFailures = 0;
        int intSuccesses = 0;
        int intPossibleCrits = 0;
        int intRedCrits = 0;
        int intRedZeros = 0;

        foreach (int result in this.lstResultsNormal)
        {
          if (result > 5)
          {
            intSuccesses++;
          }
          else
          {
            intNormalFailures++;
          }
          if (result == 10)
          {
            intPossibleCrits++;
          }
        }
        foreach (int result in this.lstResultsRed)
        {
          if (result == 1)
          {
            intRedZeros++;
          }
          if (result >= VampireDiceRollerController.MIN_VALUE_SUCCESS)
          {
            intSuccesses++;
          }
          if (result == 10)
          {
            intPossibleCrits++;
            intRedCrits++;
          }
        }

        StringBuilder strBuilderReturn = new StringBuilder();
        strBuilderReturn.Append("Successes: ");
        strBuilderReturn.Append(intSuccesses + (intPossibleCrits / 2 * 2));
        if (intPossibleCrits > 1)
        {
          if (intRedCrits > 0)
          {
            strBuilderReturn.Append(" | **Messy critical**");
          }
          else
          {
            strBuilderReturn.Append(" | **Critical**");
          }
        }

        if (intRedZeros > 0)
        {
          strBuilderReturn.Append(" | **Possible beastial failure**");
        }

        strBuilderReturn.Append(" | Failures to reroll: ");
        strBuilderReturn.Append(intNormalFailures);

        strBuilderReturn.Append(" | Detailed results: ");
        foreach (int result in this.lstResultsNormal)
        {
          if (result < VampireDiceRollerController.MIN_VALUE_SUCCESS)
          {
            strBuilderReturn.Append("F");
          }
          else if (result < 10)
          {
            strBuilderReturn.Append("S");
          }
          else
          {
            strBuilderReturn.Append("C");
          }
        }
        strBuilderReturn.Append(" ");
        foreach (int result in this.lstResultsRed)
        {
          if (result < 2)
          {
            strBuilderReturn.Append("B");
          }
          else if (result < VampireDiceRollerController.MIN_VALUE_SUCCESS)
          {
            strBuilderReturn.Append("F");
          }
          else if (result < 10)
          {
            strBuilderReturn.Append("S");
          }
          else
          {
            strBuilderReturn.Append("C");
          }
        }

        return strBuilderReturn.ToString();
      }
    }
  }
}
