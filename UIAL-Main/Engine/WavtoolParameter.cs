using zuoanqh.libzut;
using System;
using System.Collections.Generic;
using System.Linq;
using zuoanqh.UIAL.UST;

namespace zuoanqh.UIAL.Engine
{
  /// <summary>
  /// Parameter meaning was deduced from experiments.
  /// 0: output file
  /// 1: input file
  /// 2: STP adjusted default 0
  /// 3: Length(ticks)@tempo+PreUtterance adjusted default VB
  /// 4: p1 default 0
  /// 5: p2 default 5
  /// 6: p3 default 35
  /// 7: v1 default 0
  /// 8: v2 default 100
  /// 9: v3 default 100
  /// 10:v4 default 0
  /// 11:Overlap Adjusted default VB
  /// 12:p4(optional) default 0
  /// 13:p5(optional) default 10, but not exist means 0.
  /// 14:v5(optional) default 100
  /// Adjusted means multiplied by "velocity factor" (see CommonReference)
  /// that took a bit to figure out.
  /// </summary>
  public class WavtoolParameter
  {

    /// <summary>
    /// Don't forget this can have 12 to 15 elements. 
    /// </summary>
    public List<string> Args;

    /// <summary>
    /// Yes, I realize this is a different order than resampler parameters, no, i did not make this up.
    /// </summary>
    public string OutputFile { get { return Args[0]; } set { Args[0] = value; } }
    public string InputFile { get { return Args[1]; } set { Args[1] = value; } }
    public double STPAdjusted { get { return Convert.ToDouble(Args[2]); } set { Args[2] = value + ""; } }
    /// <summary>
    /// This variable used to be called "ABunchOfStuffCrammedTogether" but i want to be more descriptive.
    /// </summary>
    public string LengthTempoPreUtteranceAdjusted { get { return Args[3]; } set { Args[3] = value; } }
    public double p1 { get { return Convert.ToDouble(Args[4]); } set { Args[4] = value + ""; } }
    public double p2 { get { return Convert.ToDouble(Args[5]); } set { Args[5] = value + ""; } }
    public double p3 { get { return Convert.ToDouble(Args[6]); } set { Args[6] = value + ""; } }
    public double v1 { get { return Convert.ToDouble(Args[7]); } set { Args[7] = value + ""; } }
    public double v2 { get { return Convert.ToDouble(Args[8]); } set { Args[8] = value + ""; } }
    public double v3 { get { return Convert.ToDouble(Args[9]); } set { Args[9] = value + ""; } }
    public double v4 { get { return Convert.ToDouble(Args[10]); } set { Args[10] = value + ""; } }
    public double OverlapAdjusted { get { return Convert.ToDouble(Args[11]); } set { Args[11] = value + ""; } }
    public double p4
    {
      get { return Convert.ToDouble(Args[12]); }
      set
      {
        if (Args.Count < 13)
          Args.Add(value + "");
        else
          Args[12] = value + "";
      }
    }
    public double p5
    {
      get { return Convert.ToDouble(Args[12]); }
      set
      {
        if (Args.Count < 14)
        {
          Args.Add("0");//default p4
          Args.Add(value + "");
        }
        else
          Args[13] = value + "";
      }
    }
    public double v5
    {
      get { return Convert.ToDouble(Args[12]); }
      set
      {
        if (Args.Count < 15)
        {
          Args.Add("0");//default p4
          Args.Add("0");//default p5, so glad this doesn't happen a lot.
          Args.Add(value + "");
        }
        else
          Args[14] = value + "";
      }
    }

    /// <summary>
    /// Creates an envelope object (with "%") using information here.
    /// So yes, you want to get this once and use it a lot then set it. 
    /// Or you can just don't care. Prolly's gonna be okay since the amount of data is really small.
    /// </summary>
    public Envelope Envelope
    {
      /*
        on the magic numbers here:
        4 is because there's 4 elements before the first part of envelope.
        7 is how many non-optional elements from envelope we have.
        first 12(4+7+1) elements in Args is non-optional, elements after are the optional ones.
        You can probably figure out the rest.
        I do agree the Enumerable.Repeat("", 1) part is a bit silly, but it makes it stay in one line, no loops.
      */
      get
      {
        return new Envelope(String.Join(",", Args.Skip(4).Take(7)
          .Union(Enumerable.Repeat("%", 1))
          .Union(Args.Skip(12))));
      }
      set
      {
        Args = Args.Take(4)
          .Union(value.Parameters.Take(7).Select((s) => s + ""))
          .Union(Enumerable.Repeat(Args[11], 1))
          .Union(value.Parameters.Skip(8).Select((s) => s + "")).ToList();//oh gees. well i did it at least... hope this works...
      }
    }
    /// <summary>
    /// Length in ticks cut out from raw data.
    /// </summary>
    public double Length
    {//This is the first one.
      get { return Convert.ToDouble(zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "@").First); }
      set { LengthTempoPreUtteranceAdjusted = value + "@" + zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "@").Second; }
    }
    /// <summary>
    /// Tempo cut out from raw data.
    /// </summary>
    public double Tempo
    {//This is the one in the middle
      get { return Convert.ToDouble(zusp.CutFirst(zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "@").Second, "+").First); }
      set { LengthTempoPreUtteranceAdjusted = Length + "@" + value + "+" + PreUtteranceAdjusted; }
    }
    /// <summary>
    /// Length as milliseconds. Don't know why you need this(since you have the file already) but it's there!
    /// </summary>
    public double LengthMilliseconds
    {
      get { return CommonReferences.TicksToMilliseconds(Length, Tempo); }
      set { Length = CommonReferences.MillisecondsToTicks(value, Tempo); }
    }
    /// <summary>
    /// PreUtterance Adjusted cut out from raw data.
    /// </summary>
    public double PreUtteranceAdjusted
    {
      get { return Convert.ToDouble(zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "+").Second); }//this is the one after + sign
      set { LengthTempoPreUtteranceAdjusted = zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "+").First + "+" + value; }
    }

    /// <summary>
    /// This constructor is used when you have velocity, and can't be bothered to calculate its effect on other parameters.
    /// </summary>
    /// <param name="OutputFile"></param>
    /// <param name="InputFile"></param>
    /// <param name="Length"></param>
    /// <param name="Tempo"></param>
    /// <param name="STP"></param>
    /// <param name="PreUtterance"></param>
    /// <param name="Overlap"></param>
    /// <param name="Envelope"></param>
    /// <param name="Velocity"></param>
    public WavtoolParameter(string OutputFile, string InputFile, double STP, double Length,
      double Tempo, double PreUtterance, double Overlap, Envelope Envelope, double Velocity)
      : this(OutputFile, InputFile, STP * CommonReferences.GetEffectiveVelocityFactor(Velocity),
          Length, Tempo, PreUtterance * CommonReferences.GetEffectiveVelocityFactor(Velocity),
          Overlap * CommonReferences.GetEffectiveVelocityFactor(Velocity), Envelope)//now that's a mouthful.
    { }
    /// <summary>
    /// This constructor is used when you have adjusted parameters.
    /// </summary>
    /// <param name="OutputFile"></param>
    /// <param name="InputFile"></param>
    /// <param name="Length"></param>
    /// <param name="Tempo"></param>
    /// <param name="STPAdjusted"></param>
    /// <param name="PreUtteranceAdjusted"></param>
    /// <param name="OverlapAdjusted"></param>
    /// <param name="Envelope"></param>
    public WavtoolParameter(string OutputFile, string InputFile, double STPAdjusted, double Length,
      double Tempo, double PreUtteranceAdjusted, double OverlapAdjusted, Envelope Envelope)
    {
      Args = new List<string> { OutputFile, InputFile, STPAdjusted + "", Length + "@" + Tempo + "+" + PreUtteranceAdjusted };
      var l = zusp.Split(Envelope.ToString(), ",").ToList();
      if (l.Count > 7) l.RemoveAt(7);//remove the stupid percent mark
      Args.AddRange(l);
    }

    /// <summary>
    /// Create a new instance with given parameters.
    /// </summary>
    /// <param name="Args"></param>
    public WavtoolParameter(List<string> Args)
    {
      this.Args = Args;
    }
    /// <summary>
    /// Create a new instance with empty parameters.
    /// </summary>
    public WavtoolParameter()
    {
      Args = new List<string>(12);//there's 12 parameters + 1 or 3 optional depending on the envelope.
    }

    public WavtoolParameter(string[] Args)
      : this(Args.ToList())
    { }
  }
}
