using zuoanqh.libzut;
using zuoanqh.libzut.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// Model of a portamento of a note.
  /// The input is not preserved in verbatim.
  /// TODO: The PBS[1] thing needs to be investigated
  /// </summary>
  public class Portamento
  {
    public const string PBM_S_CURVE = "", PBM_LINEAR = "s", PBM_R_CURVE = "r", PBM_J_CURVE = "j";
    public static readonly IReadOnlyList<string> VANILLA_CURVE_TYPES;
    /// <summary>
    /// Prototype for curve interpolation functions
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
    {
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
    {
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
    /// New types can be added through the AddCurveType method.
    /// </summary>
    public static IReadOnlyDictionary<string, CurveSegmentHandler> CurveTypeHandlers { get { return CurveTypeHandlers; } }
    private static Dictionary<string, CurveSegmentHandler> curveTypeHandlers;

    /// <summary>
    /// Add your own curve type!
    /// </summary>
    /// <param name="PBMIdentifier">cannot contain space</param>
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

      //add vanilla curve handlers
      AddCurveType(PBM_S_CURVE, SCurveHandler);
      AddCurveType(PBM_LINEAR, LinearHandler);
      AddCurveType(PBM_R_CURVE, RCurveHandler);
      AddCurveType(PBM_J_CURVE, JCurveHandler);
    }
    /// <summary>
    /// This happens when pbs does not have a second number. 
    /// To keep everything minimal, we made the design decision to always have non-empty pbs1, 
    /// i.e. relative pitch difference with previous note in 10-cents
    /// </summary>
    /// <returns></returns>
    public bool HasValidPBS1()
    { return !double.IsNaN(PBS[1]); }

    public string PBSText { get { return PBS[0] + ";" + PBS[1]; } }
    /// <summary>
    /// [0] is the time difference relative to start of note (not envelope)
    /// [1] is pitch difference relative to this note in 10-cents. 
    /// This does not have a valid default and will be NaN if not present when creating the portamento.
    /// </summary>
    public double[] PBS;

    /// <summary>
    /// This is the actual place data is stored.
    /// 
    /// This must be public because there's one PBY less than other parameters -- The last segment will always have a PBY of 0.
    /// </summary> 
    public List<PortamentoSegment> Segments;
    /// <summary>
    /// Lengths of pitchbends. All units in ms, default is 0.
    /// </summary>
    public VirtualArray<double> PBW;
    public string PBWText { get { return String.Join(" ", PBW.Select((s) => (s.Equals(0)) ? ("") : (s + "")).ToArray()); } }
    /// <summary>
    /// Magnitude of pitchbends. All units are in 10-cents
    /// </summary>
    public VirtualArray<double> PBY;
    public string PBYText { get { return String.Join(" ", PBY.Select((s) => (s.Equals(0)) ? ("") : (s + "")).ToArray()); } }
    /// <summary>
    /// Interpolation type (Mode?) identifiers.
    /// </summary>
    public VirtualArray<string> PBM;

    public string PBMText { get { return String.Join(" ", PBM.ToArray()); } }

    /// <summary>
    /// Returns the change in a given segment of pitchbend line.
    /// </summary>
    /// <param name="SegmentIndex">Starts at 0.</param>
    /// <returns></returns>
    private double MagnitudeAt(int SegmentIndex)
    {
      if (SegmentIndex == 0)//first point starts at 0 pitchbend
      {
        if (double.IsNaN(PBS[1]))
          throw new InvalidOperationException("Please provide PBS[1] before sampling.");
        return PBY[0] - PBS[1];
        //note utau always uses the previous note's pitch rather than pbs[1],
        //i.e. pbs[1] is always equivalant to 0.
        //the implementation here trades exact reproducibility with the intended effect of setting pbs[1].
      }

      if (SegmentIndex < PBW.Length - 1)//middle points. segment 1 is between the 2nd and 3rd point, or pby's 0 and 1
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
    /// it is outside this class or USTNote's scope to get information on previous notes
    /// please read UST's constructor for how it's handled.
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
          if (!PBS.Contains(","))
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
        .Select((s) => (s.Equals("")) ? (0) : (Convert.ToDouble(s)))
        .ToList();

      while (y.Count < w.Count - 1) y.Add(0);//-1 because last point must be 0.

      var m = zusp.SplitAsIs(PBM, ",").ToList();

      while (m.Count < w.Count) m.Add(PBM_S_CURVE);

      //initialize segments
      for (int i = 0; i < w.Count - 1; i++)//last segment is a special case.
        Segments.Add(new PortamentoSegment(w[i], y[i], m[i]));
      Segments.Add(new PortamentoSegment(w[w.Count - 1], 0, m[w.Count - 1]));

      //fill the virtual arrays
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
    /// Returns magnitude at time with respect to the start of the pitchbend.
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public double SampleAtTime(double time)
    {
      if (time < 0) return 0;//housekeeping
      int seg = 0;//which segment is the requested time located at
      double rTime = time;//relative time to start of current segment
      while (rTime > PBW[seg])
      {
        rTime -= PBW[seg];
        seg++;
        if (seg >= PBW.Length) return 0;//error
      }
      return curveTypeHandlers[PBM[seg]].Invoke(rTime, PBW[seg], MagnitudeAt(seg));
    }
    /// <summary>
    /// Converts each part of this Portamento back to its ust format.
    /// Since empty is recorded as 0 internally, this does not reproduce the string used to create the object.
    /// It will however, always be functionally equivalent.
    /// </summary>
    /// <returns></returns>
    public List<string> ToStringList()
    {
      List<string> ans = new List<string> { USTNote.KEY_PBS + "=" + PBSText, USTNote.KEY_PBW + "=" + PBWText, USTNote.KEY_PBY + "=" + PBYText, USTNote.KEY_PBM + "=" + PBMText };
      return ans;
    }

    /// <summary>
    /// Converts it back to its ust format. 
    /// Since empty is recorded as 0 internally, this does not reproduce the string used to create the object.
    /// It will however, always be functionally equivalent.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Join("\r\n", ToStringList().ToArray()) + "\r\n";
    }
  }
}
