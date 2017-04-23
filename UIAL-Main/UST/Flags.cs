using zuoanqh.libzut;
using zuoanqh.libzut.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// This class is immutable. 
  /// It tries handles all the nuance with the flag system but will sometimes fail since the system is not perfect.
  /// We should handle "known" flags perfectly, which contains all the flags from Vanilla, Moresampler, and Utaugrowl.
  /// You have the liberty to remove flags and replace them with your own, but please be kind.
  /// When you create an object with a string, ToString() preserves that input.
  /// </summary>
  public class Flags
  {
    //public const string KEY_ALL_KNOWN_FLAGS = "Flags.ALL_KNOWN_FLAGS", KEY_NO_PARAMETER_FLAGS = "Flags.NO_PARAMETER_FLAGS";

    /// <summary>
    /// The collection of all known flags.
    /// </summary>
    public static IReadOnlyCollection<string> ALL_KNOWN_FLAGS { get { return (IReadOnlyCollection<string>)allFlags; } }
    private static HashSet<string> allFlags;

    /// <summary>
    /// The subset of flags that does not use a numeral value.
    /// </summary>
    public static IReadOnlyCollection<string> NO_PARAMETER_FLAGS { get { return (IReadOnlyCollection<string>)noParamFlags; } }
    private static HashSet<string> noParamFlags;

    /// <summary>
    /// KNOWN_FLAGS with NO_PARAMETER_FLAGS removed. funny name, i know. now stop cringing lol.
    /// </summary>
    public static IReadOnlyCollection<string> YES_PARAMETER_FLAGS { get { return (IReadOnlyCollection<string>)yesParamFlags; } }
    private static HashSet<string> yesParamFlags;

    private static string[] vanilla = new string[] { "g", "t", "B", "Y", "H", "h", "F", "L", "b", "C", "c", "D", "E", "P", "W", "G" };
    private static string[] moresampler = new string[] { "e", "A", "Mt", "Mb", "Md", "Mo", "ME", "Mm", "Ms", "Me", ":e", "MC", "MG", "MD" }; //note ":e" is now obsolete.
    private static string[] utaugrowl = new string[] { "w", "<", ">", "_", "%" };
    static Flags()
    {
      //this will make a setting file the first time you run it, but wont change anything if there already is values.
      //which means you can ignore utaugrowl or moresampler's flags if you want.

      //SimpleSettings.UseDefaultValue(KEY_ALL_KNOWN_FLAGS, zusp.List(vanilla.Union(moresampler).Union(utaugrowl)));
      //SimpleSettings.UseDefaultValue(KEY_NO_PARAMETER_FLAGS, zusp.List(new string[] { "G", "W", "N", "Me" }));
      //
      //ALL_KNOWN_FLAGS = zusp.Split(SimpleSettings.GetString(KEY_ALL_KNOWN_FLAGS), ", ").ToList();
      //NO_PARAMETER_FLAGS = zusp.Split(SimpleSettings.GetString(KEY_NO_PARAMETER_FLAGS), ", ").ToList();
      allFlags = new HashSet<string>(vanilla.Union(moresampler).Union(utaugrowl));
      noParamFlags = new HashSet<string>(new string[] { "G", "W", "N", "Me" });
      yesParamFlags = new HashSet<string>(allFlags.Except(noParamFlags));
    }

    /// <summary>
    /// Add a bunch of new flags! We do check if they break the existing system though.
    /// We strongly recommend you do not add no-parameter-flags due to the increasing ambiguity it brings to the system, especially short ones, and use like 1 to mean flag is on or whatever instead.
    /// </summary>
    /// <param name="IgnoreIfExistsAlready"></param>
    /// <param name="NewFlags"></param>
    public static void AddNewNoParamFlags(bool IgnoreIfExistsAlready, params string[] NewFlags)
    {
      foreach (string s in NewFlags)
      {
        foreach (string v in noParamFlags)
        {
          if (!IgnoreIfExistsAlready)
            if (v.Equals(s))
              throw new ArgumentException("Flag already exists: " + s);

          foreach (string v2 in noParamFlags)//so yeah it is n^3, but how many flags are you gonna have? how often you want to add them?
            if ((s + v2).Equals(v) || (v2 + s).Equals(v))
              throw new ArgumentException("Flag " + s + " cannot be added because there exists flag [" + v2 + "] that makes another existing flag [" + v + "] ambiguous when combined");//oh the pain of string concatenation
        }

        foreach (string v in yesParamFlags)
        {
          if (v.Equals(s))
            throw new ArgumentException("Flag is already registered as a yes-parameter-flag: " + s);

          foreach (string v2 in yesParamFlags)
            if ((s + v2).Equals(v))//v2+s is not a problem even if it exists since v will always be what you mean and what we parse.
              throw new ArgumentException("Flag " + s + " cannot be added because there exists flag [" + v2 + "] that makes another existing flag [" + v + "] ambiguous when combined");//oh the pain of string concatenation
        }
      }//if everything's okay...

      allFlags.UnionWith(NewFlags);
      noParamFlags.UnionWith(NewFlags);
    }

    /// <summary>
    /// Add a bunch of new flags! We do check if they break the existing system though.
    /// </summary>
    /// <param name="IgnoreIfExistsAlready"></param>
    /// <param name="NewFlags"></param>
    public static void AddNewYesParamFlags(bool IgnoreIfExistsAlready, params string[] NewFlags)
    {
      foreach (string s in NewFlags)
      {
        if (noParamFlags.Contains(s))
          throw new ArgumentException("Flag is already registered as a no-parameter-flag: " + s);

        foreach (string v in yesParamFlags)
        {
          if (IgnoreIfExistsAlready && v.Equals(s))
            throw new ArgumentException("Flag already exists: " + s);

          foreach (string v2 in noParamFlags)
            if ((v2 + s).Equals(v))//which is still simpler than the no-param-flag checking procedure
              throw new ArgumentException("Flag " + s + " cannot be added because there exists flag [" + v2 + "] that makes another existing flag [" + v + "] ambiguous when combined");//oh the pain of string concatenation
        }
      }//if everything's okay...

      allFlags.UnionWith(NewFlags);
      yesParamFlags.UnionWith(NewFlags);
    }

    /// <summary>
    /// Note NaN was used as value if a flag does not have parameter.
    /// </summary>
    public IReadOnlyList<Pair<string, double>> flags;

    /// <summary>
    /// 
    /// </summary>
    public readonly string FlagText;

    public bool HasFlag(string Flag)
    {
      foreach (var v in flags)
        if (v.First.Equals(Flag))
          return true;
      //if not found return false
      return false;

      //Old implementation
      //return FlagText.Contains(Flag);
    }

    /// <summary>
    /// Return the value of the first occurrence of given flag. This is the value effective for UTAU.
    /// This will throw error if we can't find the flag. Use HasFlag first.
    /// Please note due to there's no formal grammar to flags, This can get messed up.
    /// To get all the flag values, please STOP THINKING IT :p (because the whole system is crazy enough already)
    /// </summary>
    /// <param name="Flag"></param>
    /// <returns></returns>
    public double GetFlagsFirstValue(string Flag)
    {
      foreach (var p in flags)
        if (p.First.Equals(Flag)) return p.Second;

      throw new ArgumentException("Flag not found.");
      //old implementation, might have some use so not deleted.
      //return Convert.ToDouble(zusp.Drop(
      //    Regex.Match(FlagText, Flag + @"[\+\-\d]+").Value,
      //    Flag.Length));
    }

    /// <summary>
    /// Creates a new Flags object with given values. If there's multiple values for the same flag, only first will be changed.
    /// Yes, this creates another object.
    /// Please note due to there's no formal grammar on flags, This can get messed up.
    /// </summary>
    /// <param name="Flag"></param>
    /// <param name="Value"></param>
    public Flags WithFlag(string Flag, double Value)
    {
      if (HasFlag(Flag))
      {
        var l = new List<Pair<string, double>>();
        bool firstReplaced = false;
        foreach (var f in flags)
        {
          if (f.First.Equals(Flag) && !firstReplaced)//replace only the first
            l.Add(new Pair<string, double>(Flag, Value));
          else
            l.Add(f);
        }
        return new Flags(l);
      }
      else
      {//just append it
        var l = new List<Pair<string, double>>(this.flags) { new Pair<string, double>(Flag, Value) };
        return new Flags(l);
      }
      //if (HasFlag(Flag))
      //  return new Flags(FlagText.Replace(Regex.Match(FlagText, Flag + @"[\d]+").Value, Flag + value));
      //else
      //  return new Flags(FlagText + Flag + value);
    }
    /// <summary>
    /// Adds a no-parameter flag to this one.
    /// </summary>
    /// <param name="Flag"></param>
    /// <returns></returns>
    public Flags WithFlag(string Flag)
    {
      if (HasFlag(Flag))
        return this;
      else
        return WithFlag(Flag, double.NaN);
    }

    /// <summary>
    /// Creates a new Flags object with repeated flags removed.
    /// </summary>
    /// <returns></returns> 
    public Flags WithoutRepeatedFlags()
    {
      var ans = new List<Pair<string, double>>();
      HashSet<string> addedFlags = new HashSet<string>();
      //sure, not the most efficient way but works right?
      foreach (var f in flags)
      {
        if (!addedFlags.Contains(f.First))
        {
          addedFlags.Add(f.First);
          ans.Add(f);
        }
      }

      return new Flags(ans);
    }

    /// <summary>
    /// Creates a new Flags object without flag(s) of (exactly) given name.
    /// </summary>
    /// <param name="Flag"></param>
    /// <returns></returns>
    public Flags WithoutFlag(string Flag)
    {
      return new Flags(flags.Where((s) => !s.First.Equals(Flag)).ToList());
    }

    private List<string> SegmentNoParamFlags(string input)
    {
      List<string> ans = new List<string>();
      string s = input;
      while (true)
      {
        //find all flags this could be
        string bestMatch = "";
        foreach (var c in NO_PARAMETER_FLAGS)
          if (s.StartsWith(c) && bestMatch.Length < c.Length)
            bestMatch = c;//take the longest of them

        if (!bestMatch.Equals(""))
        {//we found a known flag this starts with. cut it out & carry on.
          ans.Add(bestMatch);
          s = zusp.Drop(s, bestMatch.Length);
        }
        else
        {//something unknown. treat the entire thing as a single flag.
          ans.Add(s);
          break;
        }
      }
      return ans;
    }

    /// <summary>
    /// You have no idea how ugly the code looks like in this method :p
    /// </summary>
    /// <param name="Flag"></param>
    public Flags(string Flag)
    {
      this.FlagText = Flag;
      var flaglist = new List<Pair<string, double>>();

      string fs = Flag.Trim();
      while (true)//wait for it.....
      {
        if (fs.Equals("")) break;
        var match = Regex.Match(fs, @"[A-Za-z]+");//extract a letter part that can have multiple flags in it.
        if (match.Success)
        {
          fs = zusp.Drop(fs, match.Length);//remove the matched part.
          var matched = match.Value;
          var matchnumbers = Regex.Match(fs, zusp.RegEx.FLOATING_POINT_NUMBER + "|" + zusp.RegEx.INTEGER);//try to extract a floating point number. if fails, try to extract an integer
          if (matchnumbers.Success)
          {
            double flagValue = Convert.ToDouble(matchnumbers.Value);
            fs = zusp.Drop(fs, matchnumbers.Length);

            //the letter part must be ending with a parameter flag, let's see what's our best match.
            string bestMatch = "";
            foreach (var c in YES_PARAMETER_FLAGS)
              if (matched.EndsWith(c) && bestMatch.Length < c.Length)
                bestMatch = c;//take the longest of them

            if (!bestMatch.Equals(""))
            {//it is one of our known flags with parameter
              matched = zusp.DropLast(matched, bestMatch.Length);//leave that out

              if (matched.Length > 0)
              {//segment all no-parameter flags before it
                flaglist.AddRange(SegmentNoParamFlags(matched)
                .Select((s) => new Pair<string, double>(s, double.NaN)));
              }

              flaglist.Add(new Pair<string, double>(bestMatch, flagValue));
            }
            else
            {//hmm.... it does not end with any. Let's try to chip away no-parameter flags in the beginning.
              var v = SegmentNoParamFlags(matched);//this will give us a bunch of no-param flags if any, then the last will be a flag with parameter.
              for (int i = 0; i < v.Count - 1; i++)//add first ones
                flaglist.Add(new Pair<string, double>(v[i], double.NaN));
              //add that last.
              flaglist.Add(new Pair<string, double>(v.Last(), flagValue));
            }
          }
          else
          {//the remainder is all letters. They can be many no-parameter-flag or one.
            flaglist.AddRange(SegmentNoParamFlags(matched)
              .Select((s) => new Pair<string, double>(s, double.NaN)));
            break;
          }
        }
        else//malformed flag or I made a terrible mistake
        {//everyone PANIC!!!!!!!
          throw new ArgumentException("Flag not in correct format or not processed correctly.");
        }
      }
      this.flags = flaglist;//whew....
    }

    /// <summary>
    /// Creates an object with no flags.
    /// </summary>
    public Flags() : this("") { }

    /// <summary>
    /// This can expose internal data representation and wreck things, hence should not be used unless you absolutely know what you're doing.
    /// </summary>
    /// <param name="list"></param>
    private Flags(List<Pair<string, double>> list)
    {//used for efficiency.
      this.flags = list;
      //make up the flag string.
      StringBuilder sb = new StringBuilder();
      foreach (var v in list)
        sb.Append(v.First)
          .Append((double.IsNaN(v.Second)) ? ("") : ("" + Math.Round(v.Second, 2)));
      FlagText = sb.ToString();
    }

    /// <summary>
    /// Copy constructor.
    /// </summary>
    /// <param name="Another"></param>
    public Flags(Flags Another)
    {
      this.flags = new List<Pair<string, double>>(Another.flags);
      this.FlagText = Another.FlagText;
    }

    public override string ToString()
    {//because it's immutable
      return FlagText;
    }
  }
}
