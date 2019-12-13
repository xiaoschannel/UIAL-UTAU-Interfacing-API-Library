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
    /// Since each resampler defines its own flags, there's no one-size-fit-all solution to handling flags.
    /// The algorithm here should handle flags from Vanilla, Moresampler, and Utaugrowl perfectly.
    /// Other flags may not be parsed correctly without adding their definitions.
    /// 
    /// This class is immutable. 
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
    /// KNOWN_FLAGS with NO_PARAMETER_FLAGS removed. The name could've been better.
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
      //ALL_KNOWN_FLAGS = zusp.Split(SimpleSettings.GetString(KEY_ALL_KNOWN_FLAGS), ", ").ToList();
      //NO_PARAMETER_FLAGS = zusp.Split(SimpleSettings.GetString(KEY_NO_PARAMETER_FLAGS), ", ").ToList();

      allFlags = new HashSet<string>(vanilla.Union(moresampler).Union(utaugrowl));
      noParamFlags = new HashSet<string>(new string[] { "G", "W", "N", "Me" });
      yesParamFlags = new HashSet<string>(allFlags.Except(noParamFlags));
    }

    /// <summary>
    /// Add(define) a bunch of new flags!
    /// The program checks if they break the existing system.
    /// 
    /// Tip:
    /// No-parameter-flags are bad because they increase the ambiguity during parsing.
    /// You can convert them to yes-parameter-flags by adding 0 or 1 at the end to indicate on/off.
    /// </summary>
    /// <param name="IgnoreIfExistsAlready"></param>
    /// <param name="NewFlags"></param>
    public static void AddNewNoParamFlags(bool IgnoreIfExistsAlready, params string[] NewFlags)
    {
      //TODO: format really long lines here
      foreach (string s in NewFlags)
      {
        foreach (string v in noParamFlags)
        {
          if (!IgnoreIfExistsAlready)
            if (v.Equals(s))
              throw new ArgumentException("Flag already exists: " + s);
          //n^3 time complexity is acceptable because the amount of flags is small, and this code is not called very often.
          foreach (string v2 in noParamFlags)
            if ((s + v2).Equals(v) || (v2 + s).Equals(v))
              throw new ArgumentException("Flag " + s + " cannot be added because there exists flag [" + v2 + "] that makes another existing flag [" + v + "] ambiguous when combined");
        }

        foreach (string v in yesParamFlags)
        {
          if (v.Equals(s))
            throw new ArgumentException("Flag is already registered as a yes-parameter-flag: " + s);

          foreach (string v2 in yesParamFlags)
            if ((s + v2).Equals(v))//v2+s is not a problem even if it exists since v will always be what you mean and what we parse.
              throw new ArgumentException("Flag " + s + " cannot be added because there exists flag [" + v2 + "] that makes another existing flag [" + v + "] ambiguous when combined");
        }
      }//if everything's okay...

      allFlags.UnionWith(NewFlags);
      noParamFlags.UnionWith(NewFlags);
    }

    /// <summary>
    /// Add(define) a bunch of new flags!
    /// The program checks if they break the existing system.
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
            if ((v2 + s).Equals(v))
              throw new ArgumentException("Flag " + s + " cannot be added because there exists flag [" + v2 + "] that makes another existing flag [" + v + "] ambiguous when combined");//oh the pain of string concatenation
        }
      }//if everything's okay...

      allFlags.UnionWith(NewFlags);
      yesParamFlags.UnionWith(NewFlags);
    }

    /// <summary>
    /// Note NaN is used as the value if a flag does not have parameter.
    /// </summary>
    public IReadOnlyList<Pair<string, double>> flags;

    /// <summary>
    /// Raw text of all the flags
    /// </summary>
    public readonly string FlagText;

    public bool HasFlag(string Flag)
    {
      foreach (var v in flags)
        if (v.First.Equals(Flag))
          return true;

      //flag not found
      return false;
    }

    /// <summary>
    /// Return the value of the first occurrence of the given flag. This is the value effective for many resamplers.
    /// This will throw error if we can't find the flag. Use HasFlag first.
    /// </summary>
    /// <param name="Flag"></param>
    /// <returns></returns>
    public double GetFlagsFirstValue(string Flag)
    {
      foreach (var p in flags)
        if (p.First.Equals(Flag)) return p.Second;

      throw new ArgumentException("Flag not found.");
    }

    /// <summary>
    /// Creates a new Flags object containing a flag with given value.
    /// If the specified flag exists, its value is changed to the given.
    /// If there are multiple values for the same flag, only the first occourance is changed.
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
    /// Creates a new Flags object without repeated flags.
    /// For any flag, occourances beyond the first is removed.
    /// </summary>
    /// <returns></returns> 
    public Flags WithoutRepeatedFlags()
    {
      var ans = new List<Pair<string, double>>();
      HashSet<string> addedFlags = new HashSet<string>();
      //There might be a more efficient way without dynamic allocation
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
    /// Creates a new Flags object without the specified flag.
    /// </summary>
    /// <param name="Flag"></param>
    /// <returns></returns>
    public Flags WithoutFlag(string Flag)
    {
      return new Flags(flags.Where((s) => !s.First.Equals(Flag)).ToList());
    }

    /// <summary>
    /// Helper function. Segment a string by chipping away all the known no_parameter_flags at front.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
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
    /// Creates an object from the given flag string.
    /// </summary>
    /// <param name="Flag"></param>
    public Flags(string Flag)
    {
      this.FlagText = Flag;
      var flaglist = new List<Pair<string, double>>();

      string fs = Flag.Trim();
      while (true)
      {
        if (fs.Equals("")) break;
        var match = Regex.Match(fs, @"[A-Za-z]+");//extract a letter part that can have multiple flags in it.
        if (match.Success)
        {
          fs = zusp.Drop(fs, match.Length);//remove the matched part.
          var matched = match.Value;
          //try to extract a floating point number. if fails, try to extract an integer
          var matchnumbers = Regex.Match(fs, zusp.RegEx.FLOATING_POINT_NUMBER + "|" + zusp.RegEx.INTEGER);
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
            {
              var v = SegmentNoParamFlags(matched);//the last will be a flag with parameter.
              for (int i = 0; i < v.Count - 1; i++)//add first ones as no-param-flag, if any
                flaglist.Add(new Pair<string, double>(v[i], double.NaN));
              //add last along with params parsed
              flaglist.Add(new Pair<string, double>(v.Last(), flagValue));
            }
          }
          else
          {//the remainder is all letters. add all as no-param-flags
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
    /// Creates an empty object.
    /// </summary>
    public Flags() : this("") { }

    /// <summary>
    /// Shallow copy constructor.
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
    /// Deep Copy constructor.
    /// </summary>
    /// <param name="Another"></param>
    public Flags(Flags Another)
    {
      this.flags = new List<Pair<string, double>>(Another.flags);
      this.FlagText = Another.FlagText;
    }
    /// <summary>
    /// When you create a Flag object with a string, ToString() returns that exact string.
    /// </summary>
    public override string ToString()
    {//because it's immutable
      return FlagText;
    }
  }
}
