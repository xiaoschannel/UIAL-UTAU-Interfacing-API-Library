using zuoanqh.libzut;
using System;
using System.Collections.Generic;
using System.Linq;

namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// This class models a envelope. While the entire idea is bizarre, I'm putting down some code to hopefully make your life easier.
  /// </summary>
  public class Envelope
  {
    public static readonly double DEFAULT_V4 = 0;
    public static readonly double DEFAULT_P5 = 10;
    public static readonly double DEFAULT_V5 = 100;

    /// <summary>
    /// Please note if a parameter does not exist, it will be NaN.
    /// Parameters are in the order of p1, p2, p3, v1, v2, v3, v4, p4, p5, v5.
    /// To get the "raw" data, use ToString(). 
    /// To interact with parameters separately, use their name.
    /// </summary>
    public IReadOnlyList<double> Parameters { get { return param.ToList(); } }

    /// <summary>
    /// Encapsulated data.
    /// </summary>
    private double[] param;
    /// <summary>
    /// Length of the "blank" before the sound in ms. Default is 0.
    /// </summary>
    public double p1 { get { return param[0]; } set { param[0] = value; } }
    /// <summary>
    /// Volume in percent. Default is 0.
    /// </summary>
    public double v1 { get { return param[3]; } set { param[3] = value; } }
    /// <summary>
    /// Time between p1 and p2 in ms. Default while not meaningful, is 5.
    /// </summary>
    public double p2 { get { return param[1]; } set { param[1] = value; } }
    /// <summary>
    /// Volume in percent. Default is 100.
    /// </summary>
    public double v2 { get { return param[4]; } set { param[4] = value; } }
    /// <summary>
    /// Time before p4 in ms. Default while not meaningful, is 35.
    /// </summary>
    public double p3 { get { return param[2]; } set { param[2] = value; } }
    /// <summary>
    /// Volume in percent. Default is 100.
    /// </summary>
    public double v3 { get { return param[5]; } set { param[5] = value; } }
    /// <summary>
    /// Length of the "blank" at the end in ms. Note this is relative to the length of note this envelope will be applied to.
    /// </summary>
    public double p4 { get { return param[7]; } set { param[7] = value; } }
    public bool HasP4 { get { return !double.IsNaN(p4); } }
    /// <summary>
    /// Volume in percent. Default is 0. Optional (NaN means 0).
    /// </summary>
    public double v4 { get { return param[6]; } set { param[6] = value; } }

    /// <summary>
    /// Time after p2 in ms. Optional (NaN means 0). Default is 10.
    /// </summary>
    public double p5 { get { return param[8]; } set { param[8] = value; } }
    public bool HasP5 { get { return !double.IsNaN(p5); } }

    /// <summary>
    /// Volume in percent. Default is 100. Optional (NaN means 100).
    /// </summary>
    public double v5 { get { return param[9]; } set { param[9] = value; } }
    public bool HasV5 { get { return !double.IsNaN(v5); } }
    /// <summary>
    /// This removes p5 from data (rather than set it to default).
    /// </summary>
    public void RemoveP5()
    {
      p5 = double.NaN;
      v5 = double.NaN;
    }

    /// <summary>
    /// This method make the highest point in envelope 100, and return how much it have scaled everything.
    /// </summary>
    public double Normalize()
    {
      throw new NotImplementedException();
    }
    /// <summary>
    /// This is the threshold we consider two v-value equal.
    /// </summary>
    public static readonly double EPSILON = 0.1;

    /// <summary>
    /// This method tries to zero p values. 
    /// This can fix some envelope problems, so TRY IT!
    /// </summary>
    public void ZeroPValues()
    {
      if (HasP5)
      {
        if (!HasV5) RemoveP5();
        if (HasV5 && Math.Abs(v5 - v2) < EPSILON) p5 = 0;
      }
      if (Math.Abs(v2 - v1) < EPSILON) p2 = 0;
      if (Math.Abs(v3 - v4) < EPSILON) p3 = 0;
    }

    /// <summary>
    /// Please note it seems this does absolutely nothing, so maybe you shouldn't spend time on it.
    /// </summary>
    public bool HasPercentMark;

    /// <summary>
    /// Check if the envelope is valid given its length in ms.
    /// </summary>
    /// <param name="Length"></param>
    /// <returns></returns>
    public bool IsValidWith(double Length)
    {
      return Length > (p1 + p2 + p3 + (HasP4 ? p4 : 0) + (HasP5 ? p5 : 0));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="Data">As in UST's format.</param>
    public Envelope(string Data)
    {
      param = new double[10];

      var ls = zusp.SplitAsIs(Data, ",")
        .Select((s) => s.Trim()).ToArray();//gotta trim that string

      if (ls.Length < 7) throw new ArgumentException("Malformed envelope, have " + ls.Length + " parts only, requires 7 or more.");

      for (int i = 0; i < 7; i++)//7 is where the "%" is
        param[i] = Convert.ToDouble(ls[i]);

      for (int i = 7; i < 10; i++)//assume there is none of the optionals now
        param[i] = double.NaN;

      if (ls.Length >= 8)
      {
        HasPercentMark = ls[7].Equals("%");
        if (ls.Length >= 9)//if there are optionals add them back.
        {
          if(!ls[8].Equals("")) p4 = Convert.ToDouble(ls[8]);
          if (ls.Length >= 10)
          {
            if (!ls[9].Equals("")) p5 = Convert.ToDouble(ls[9]);
            if (ls.Length >= 11)
              if (!ls[10].Equals("")) v5 = Convert.ToDouble(ls[10]);
          }
        }
      }
      else
        HasPercentMark = false;
    }
    /// <summary>
    /// Creates the default envelope used in utau: 0,5,35,0,100,100,0,%
    /// </summary>
    public Envelope() : this("0,5,35,0,100,100,0,%")
    { }

    /// <summary>
    /// (Deep) copy constructor. 
    /// </summary>
    /// <param name="that"></param>
    public Envelope(Envelope that) : this(that.ToString())
    { }

    /// <summary>
    /// Converts it back to original format.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      var l = new List<object> { p1, p2, p3, v1, v2, v3, v4, (HasPercentMark) ? ("%") : ("") };
      double effectiveP4 = HasP4 ? p4 : 0;
      double effectiveP5 = HasP5 ? p5 : 0;
      double effectiveV5 = HasV5 ? v5 : 100;
      if (HasV5)
      {
        l.Add(effectiveP4);
        l.Add(effectiveP5);
        l.Add(effectiveV5);
      }
      else if (HasP5)//but not v5
      {
        l.Add(effectiveP4);
        l.Add(effectiveP5);
      }
      else if (HasP4)//but not v5 or p5
      {
        l.Add(effectiveP4);
      }

      return zusp.List(",", l.ToArray());
    }
  }
}
