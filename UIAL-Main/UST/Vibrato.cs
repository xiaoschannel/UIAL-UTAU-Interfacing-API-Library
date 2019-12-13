using zuoanqh.libzut;
using System;
using System.Linq;

namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// This models a the vibrato of a note. 
  /// </summary>
  public class Vibrato
  {
    /// <summary>
    /// The 7 parameters, in order, are: length (% of declared length of note), cycle(ms), depth(cent), in(%), out(%), phase(%), pitch(%).
    /// It's unclear what the 8th parameter does. TODO: find out
    /// pitch is the "y offset", percentage on depth, while phase is the x offset.
    /// length works even if it's >100%.
    /// </summary>
    public double[] Parameters;

    /// <summary>
    /// Length in percents. Default is 65.
    /// </summary>
    public double Length { get { return Parameters[0]; } set { Parameters[0] = value; } }
    /// <summary>
    /// Inverse of frequency in ms. Default is 180. 
    /// </summary>
    public double Cycle { get { return Parameters[1]; } set { Parameters[1] = value; } }
    /// <summary>
    /// "Strength" of the vibrato in cents. Default is 35.
    /// </summary>
    public double Depth { get { return Parameters[2]; } set { Parameters[2] = value; } }
    /// <summary>
    /// The linear fade-in part. In percentage. Default is 20.
    /// </summary>
    public double In
    {
      get { return Parameters[3]; }
      set
      {
        if ((value + Out) > 100)
          throw new ArgumentException(String.Format("Invalid Parameter. In = {0}%, In + Out = {1}% >100%", value, value + Out));
        Parameters[3] = value;
      }
    }
    /// <summary>
    /// The linear face-out part. In percentage. Default is 20.
    /// </summary>
    public double Out
    {
      get { return Parameters[4]; }
      set
      {
        if ((value + In) > 100)
          throw new ArgumentException(String.Format("Invalid Parameter. Out = {0}%, In + Out = {1}% >100%", value, value + In));
        Parameters[4] = value;
      }
    }
    /// <summary>
    /// The time-axis shift of the sine wave, in percentage. Default is 20.
    /// </summary>
    public double Phase { get { return Parameters[5]; } set { Parameters[5] = value; } }
    /// <summary>
    /// The pitch shift of the sine wave (uncommon in practice), in percentage, Default is 20.
    /// TODO: percentage of WHAT?
    /// </summary>
    public double Pitch { get { return Parameters[6]; } set { Parameters[6] = value; } }

    /// <summary>
    /// Access the Pitch parameter as cent.
    /// </summary>
    public double PitchAsCent
    {
      get { return Pitch * 0.01 * Depth; }
      set { Pitch = (value / Depth) * 100; }
    }

    /// <summary>
    /// Returns the last parameter. Its purpose is unclear.
    /// </summary>
    public double EighthParameter { get { return Parameters[7]; } set { Parameters[7] = value; } }

    /// <summary>
    /// Creates an object using UST format text
    /// </summary>
    /// <param name="VBRText"></param>
    public Vibrato(string VBRText)
    {
      Parameters = zusp.SplitAsIs(VBRText, ",")
        .Select((s) => s.Equals("")?0:Convert.ToDouble(s))
        .ToArray();
    }

    /// <summary>
    /// (Deep) copy constructor. 
    /// </summary>
    /// <param name="that"></param>
    public Vibrato(Vibrato that)
      : this(that.ToString())
    { }

    /// <summary>
    /// Gives a full-length, everything else 0 Vibrato object.
    /// </summary>
    /// <param name="Cycle"></param>
    /// <param name="Depth"></param>
    public Vibrato(double Cycle, double Depth)
      : this("100 " + Cycle + " " + Depth + " 0 0 0 0 0")
    { }

    /// <summary>
    /// Gives the default vibrato -- "65 180 35 20 20 0 0 0"
    /// </summary>
    public Vibrato() : this("65 180 35 20 20 0 0 0") { }

    /// <summary>
    /// Returns the magnitude of pitchbend in cents at given time. 
    /// Since everything is in percentage, the length is required.
    /// Note this will return 0 if time is outside range, will not throw exceptions.
    /// </summary>
    /// <param name="AtTime">time since 0%</param>
    /// <param name="Length">How long is "100%"</param>
    /// <returns></returns>
    public double Sample(double AtTime, double Length)
    {
      // TODO: refactor this to work without length and create a wrapper overload
      double len = Length * this.Length;//vibrato length in ms
      double blank = Length - len;
      if (AtTime < blank || AtTime > Length) return 0;//just some house keeping.

      double rTime = AtTime - blank;//relative time since start of vibrato.
      double unfaded = Pitch * Depth *
        Math.Sin(((rTime / Cycle + Phase) * zum.PHI));//GO TEAM PHI!!!

      double percentTime = rTime / len;//percent of whole vibrato time, before fade-in and fade-out

      double fadeEffect = 1;//effect of fade-in or fade-out
      if (percentTime < (In / 100))
        fadeEffect = percentTime / (In / 100);
      else if (percentTime > ((100 - Out) / 100))
        fadeEffect = (100 - percentTime) / (Out / 100);

      return unfaded * fadeEffect;
    }

    /// <summary>
    /// Converts it back to its ust format. 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Join(" ", Parameters);
    }
  }
}
