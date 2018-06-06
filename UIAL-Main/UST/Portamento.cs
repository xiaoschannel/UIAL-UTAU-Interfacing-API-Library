using zuoanqh.libzut;
using zuoanqh.libzut.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// Model of a portamento of a note.
  /// No, we don't preserve input in verbatim, are you crazy?
  /// </summary>
  public class Portamento
  {
    public const string PBM_S_CURVE = "", PBM_LINEAR = "s", PBM_R_CURVE = "r", PBM_J_CURVE = "j";
    public static readonly IReadOnlyList<string> VANILLA_CURVE_TYPES;
    //public string PBSAsText;
    /// <summary>
    /// Write a function with this signature to accompany your designer-made curve type! which you probably wont do but... it's there... just saying...
    /// </summary>
    /// <param name="Time">time relative to beginning.</param>
    /// <param name="Length">length of the entire segment.</param>
    /// <param name="Magnitude">relative pitch change in cent.</param>
    /// <returns></returns>
    public delegate double CurveSegmentHandler(double Time, double Length, double Magnitude);

    /// <summary>
    /// Implemented with cosine interpolation.
    /// </summary>
    public static readonly CurveSegmentHandler SCurveHandler = (Time, Length, Magnitude) =>
    {//yes. i know. lots of brackets.
      return ((1 - Math.Cos((zum.Bound(Time, 0, Length) / Length))) / 2) * Magnitude;
    };

    /// <summary>
    /// Implemented with linear interpolation.
    /// </summary>
    public static readonly CurveSegmentHandler LinearHandler = (Time, Length, Magnitude) =>
    {
      return (zum.Bound(Time, 0, Length) / Length) * Magnitude;
    };

    /// <summary>
    /// Gives later half of the S curve
    /// </summary>
    public static readonly CurveSegmentHandler RCurveHandler = (Time, Length, Magnitude) =>
    {//yes I'm THAT lazy. 
      return SCurveHandler(Time + Length, Length * 2, Magnitude * 2) - Magnitude;
    };

    /// <summary>
    /// Gives first half of the S curve
    /// </summary>
    public static readonly CurveSegmentHandler JCurveHandler = (Time, Length, Magnitude) =>
    {
      return SCurveHandler(Time, Length * 2, Magnitude * 2);
    };

    /// <summary>
    /// please use add/remove method to edit this.
    /// </summary>
    public static IReadOnlyDictionary<string, CurveSegmentHandler> CurveTypeHandlers { get { return CurveTypeHandlers; } }
    private static Dictionary<string, CurveSegmentHandler> curveTypeHandlers;

    /// <summary>
    /// Add your own curve type! 
    /// Note your identifier cannot contain space.
    /// </summary>
    /// <param name="PBMIdentifier"></param>
    /// <param name="SegmentHandler"></param>
    public static void AddCurveType(string PBMIdentifier, CurveSegmentHandler SegmentHandler)
    {
      if (curveTypeHandlers.ContainsKey(PBMIdentifier))
        throw new ArgumentException("You cannot modify existing curve type: " + PBMIdentifier);
      if (PBMIdentifier.Contains(" "))
        throw new ArgumentException("Identifier contains space at: " + PBMIdentifier.IndexOf(" "));

      curveTypeHandlers.Add(PBMIdentifier, SegmentHandler);
    }

    static Portamento()
    {
      VANILLA_CURVE_TYPES = new string[] { PBM_S_CURVE, PBM_LINEAR, PBM_R_CURVE, PBM_J_CURVE }.ToList();

      curveTypeHandlers = new Dictionary<string, CurveSegmentHandler>();

      //adding vanilla curve handlers
      AddCurveType(PBM_S_CURVE, SCurveHandler);
      AddCurveType(PBM_LINEAR, LinearHandler);
      AddCurveType(PBM_R_CURVE, RCurveHandler);
      AddCurveType(PBM_J_CURVE, JCurveHandler);
    }
    /// <summary>
    /// This happens when pbs does not have a second number. 
    /// To keep everything minimal, we made the design decision you must the correct value, 
    /// i.e. relative pitch difference with previous note in 10-cents
    /// </summary>
    /// <returns></returns>
    public bool HasValidPBS1()
    { return !double.IsNaN(PBS[1]); }

    public string PBSText { get { return PBS[0] + ";" + PBS[1]; } }
    /// <summary>
    /// [0] is the time difference relative to start of note (not envelope, duh)
    /// [1] is pitch difference relative to this note in 10-cents. 
    /// This does not have a valid default and will be NaN if not present when creating the portamento.
    /// </summary>
    public double[] PBS;

    /// <summary>
    /// I can't keep the encapsulation because there's one PBY less than other parameters and i don't know how to handle it.
    /// So enjoy public members! yay! Good thing is this is the only place data is stored and everything else is functional.
    /// By default, last segment will have PBY of 0 just so you know.
    /// </summary> 
    public List<PortamentoSegment> Segments;
    /// <summary>
    /// All units in ms, default is 0.
    /// </summary>
    public VirtualArray<double> PBW;
    public string PBWText { get { return String.Join(" ", PBW.Select((s) => (s.Equals(0)) ? ("") : (s + "")).ToArray()); } }
    /// <summary>
    /// All units are in 10-cents
    /// </summary>
    public VirtualArray<double> PBY;
    public string PBYText { get { return String.Join(" ", PBY.Select((s) => (s.Equals(0)) ? ("") : (s + "")).ToArray()); } }

    public VirtualArray<string> PBM;

    public string PBMText { get { return String.Join(" ", PBM.ToArray()); } }

    /// <summary>
    /// Returns the change in a given segment of pitchbend line.
    /// </summary>
    /// <param name="SegmentIndex">Starts at 0.</param>
    /// <returns></returns>
    private double MagnitudeAt(int SegmentIndex)
    {
      if (SegmentIndex == 0)//first point starts at 0
      {
        if (double.IsNaN(PBS[1]))
          throw new InvalidOperationException("Please provide PBS[1] before sampling.");
        return PBY[0] - PBS[1];
        //note this^ does not happen in vanilla, UTAU actually always use previous note's pitch rather than pbs.
        //Hence PBS[1] actually means nothing except for display purpouse in utau. which is a very bad practice.
        //By fixing it, we might make some extreme cases sounds differently, but we think this is for the better.
      }

      if (SegmentIndex < PBW.Length - 1)//middle points. segment 1 is between 2nd and 3rd point, or pby's 0 and 1
        return PBY[SegmentIndex] - PBY[SegmentIndex - 1];
      if (SegmentIndex == PBW.Length - 1)//last point ends at 0, hence 0 minus last y
        return -PBY[PBY.Length - 1];

      throw new IndexOutOfRangeException("Segment " + SegmentIndex + " does not exist.");
    }

    /// <summary>
    /// Only PBW is required to make a Portamento. 
    /// If you provide empty on the others, we will use the default value:
    /// 0;(invalid) for pbs, 0 for pby, "" or s-curve for pbm
    /// If pbs does not have second parameter, it will be considered invalid as well. 
    /// it is outside this class or USTNote's power to get information about previous notes, please read UST's constructor for its handling.
    /// </summary>
    /// <param name="PBS"></param>
    /// <param name="PBW"></param>
    /// <param name="PBY"></param>
    /// <param name="PBM"></param>
    public Portamento(string PBW, string PBS, string PBY, string PBM)
    {
      Segments = new List<PortamentoSegment>();

      if (PBS == null || PBS.Trim().Length == 0)
        this.PBS = new double[] { 0, double.NaN };
      else
      {
        if (!PBS.Contains(";"))//starting y is 0
        {
          if (!PBS.Contains(","))//don't you just love working with legendary code
            this.PBS = new double[] { Convert.ToDouble(PBS), double.NaN };
          else
            this.PBS = zusp.Split(PBS, ",").Select((s) => Convert.ToDouble(s)).ToArray();
        }
        else
          this.PBS = zusp.Split(PBS, ";").Select((s) => Convert.ToDouble(s)).ToArray();
      }

      var w = zusp.SplitAsIs(PBW, ",")
        .Select((s) => (s.Equals("")) ? (0) : (Convert.ToDouble(s)))//empty entries means 0. 
        .ToList();

      var y = zusp.SplitAsIs(PBY, ",")
        .Select((s) => (s.Equals("")) ? (0) : (Convert.ToDouble(s)))//why though? i wonder.
        .ToList();

      while (y.Count < w.Count - 1) y.Add(0);//-1 because last point must be 0 as far as utau's concern, which is stupid.

      var m = zusp.SplitAsIs(PBM, ",").ToList();

      while (m.Count < w.Count) m.Add(PBM_S_CURVE);

      //initialize segments
      for (int i = 0; i < w.Count - 1; i++)//last segment is a special case.
        Segments.Add(new PortamentoSegment(w[i], y[i], m[i]));
      Segments.Add(new PortamentoSegment(w[w.Count - 1], 0, m[w.Count - 1]));

      //now fill the virtual arrays.
      this.PBW = new VirtualArray<double>((i) => (Segments[i].PBW), (i, v) => Segments[i].PBW = v, () => Segments.Count);
      this.PBY = new VirtualArray<double>((i) => (Segments[i].PBY), (i, v) => Segments[i].PBW = v, () => Segments.Count - 1);
      this.PBM = new VirtualArray<string>((i) => (Segments[i].PBM), (i, v) => Segments[i].PBM = v, () => Segments.Count);

    }

    /// <summary>
    /// (Deep) copy constructor. 
    /// </summary>
    /// <param name="that"></param>
    public Portamento(Portamento that)
      : this(that.PBWText, that.PBSText, that.PBYText, that.PBMText)
    { }


    /// <summary>
    /// Returns magnitude at time with respect to start of the pitchbend.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public double SampleAtTime(double time)
    {
      if (time < 0) return 0;//housekeeping
      int seg = 0;//which segment is the required point located
      double rTime = time;//relative time to start of current interval
      while (rTime > PBW[seg])
      {
        rTime -= PBW[seg];
        seg++;
        if (seg >= PBW.Length) return 0;//if we went through the whole thing. no i don't want to write two conditions and check after the loop. too much typing.
      }
      return curveTypeHandlers[PBM[seg]].Invoke(rTime, PBW[seg], MagnitudeAt(seg));
    }

    /// <summary>
    /// Converts it back to its ust format. 
    /// Again, while the data will mean the same, it will look different. 
    /// Because the using empty string to mean "0" is just not very readable.
    /// </summary>
    /// <returns></returns>
    public List<string> ToStringList()
    {
      List<string> ans = new List<string> { USTNote.KEY_PBS + "=" + PBSText, USTNote.KEY_PBW + "=" + PBWText, USTNote.KEY_PBY + "=" + PBYText, USTNote.KEY_PBM + "=" + PBMText };
      return ans;
    }

    public override string ToString()
    {
      return String.Join("\r\n", ToStringList().ToArray()) + "\r\n";
    }
  }
}
