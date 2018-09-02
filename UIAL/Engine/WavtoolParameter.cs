using System;
using System.Collections.Generic;
using System.Linq;
using zuoanqh.libzut;
using zuoanqh.UIAL.UST;

namespace zuoanqh.UIAL.Engine
{
    /// <summary>
    ///     Parameter meaning was deduced from experiments.
    ///     0: output file
    ///     1: input file
    ///     2: STP adjusted default 0
    ///     3: Length(ticks)@tempo+PreUtterance adjusted default VB
    ///     4: p1 default 0
    ///     5: p2 default 5
    ///     6: p3 default 35
    ///     7: v1 default 0
    ///     8: v2 default 100
    ///     9: v3 default 100
    ///     10:v4 default 0
    ///     11:Overlap Adjusted default VB
    ///     12:p4(optional) default 0
    ///     13:p5(optional) default 10, but not exist means 0.
    ///     14:v5(optional) default 100
    ///     Adjusted means multiplied by "velocity factor" (see CommonReference)
    ///     that took a bit to figure out.
    /// </summary>
    public class WavtoolParameter
    {
        /// <summary>
        ///     This constructor is used when you have velocity, and can't be bothered to calculate its effect on other parameters.
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="inputFile"></param>
        /// <param name="length"></param>
        /// <param name="tempo"></param>
        /// <param name="stp"></param>
        /// <param name="preUtterance"></param>
        /// <param name="overlap"></param>
        /// <param name="envelope"></param>
        /// <param name="velocity"></param>
        public WavtoolParameter(string outputFile, string inputFile, double stp, double length,
            double tempo, double preUtterance, double overlap, Envelope envelope, double velocity)
            : this(outputFile, inputFile, stp * Commons.GetEffectiveVelocityFactor(velocity),
                length, tempo, preUtterance * Commons.GetEffectiveVelocityFactor(velocity),
                overlap * Commons.GetEffectiveVelocityFactor(velocity), envelope) //now that's a mouthful.
        {
        }

        /// <summary>
        ///     This constructor is used when you have adjusted parameters.
        /// </summary>
        /// <param name="outputFile"></param>
        /// <param name="inputFile"></param>
        /// <param name="length"></param>
        /// <param name="tempo"></param>
        /// <param name="stpAdjusted"></param>
        /// <param name="preUtteranceAdjusted"></param>
        /// <param name="overlapAdjusted"></param>
        /// <param name="envelope"></param>
        public WavtoolParameter(string outputFile, string inputFile, double stpAdjusted, double length,
            double tempo, double preUtteranceAdjusted, double overlapAdjusted, Envelope envelope)
        {
            Args = new List<string>
                {outputFile, inputFile, stpAdjusted + "", length + "@" + tempo + "+" + preUtteranceAdjusted};
            var l = zusp.Split(envelope.ToString(), ",").ToList();
            if (l.Count > 7) l.RemoveAt(7); //remove the stupid percent mark
            Args.AddRange(l);
        }

        /// <summary>
        ///     Create a new instance with given parameters.
        /// </summary>
        /// <param name="args"></param>
        public WavtoolParameter(List<string> args)
        {
            Args = args;
        }

        /// <summary>
        ///     Create a new instance with empty parameters.
        /// </summary>
        public WavtoolParameter()
        {
            Args = new List<string>(12); //there's 12 parameters + 1 or 3 optional depending on the envelope.
        }

        public WavtoolParameter(string[] args)
            : this(args.ToList())
        {
        }

        /// <summary>
        ///     Don't forget this can have 12 to 15 elements.
        /// </summary>
        public List<string> Args { get; }

        /// <summary>
        ///     Yes, I realize this is a different order than resampler parameters, no, i did not make this up.
        /// </summary>
        public string OutputFile
        {
            get => Args[0];
            set => Args[0] = value;
        }

        public string InputFile
        {
            get => Args[1];
            set => Args[1] = value;
        }

        public double StpAdjusted
        {
            get => Convert.ToDouble(Args[2]);
            set => Args[2] = value + "";
        }

        /// <summary>
        ///     This variable used to be called "ABunchOfStuffCrammedTogether" but i want to be more descriptive.
        /// </summary>
        public string LengthTempoPreUtteranceAdjusted
        {
            get => Args[3];
            set => Args[3] = value;
        }

        public double P1
        {
            get => Convert.ToDouble(Args[4]);
            set => Args[4] = value + "";
        }

        public double P2
        {
            get => Convert.ToDouble(Args[5]);
            set => Args[5] = value + "";
        }

        public double P3
        {
            get => Convert.ToDouble(Args[6]);
            set => Args[6] = value + "";
        }

        public double V1
        {
            get => Convert.ToDouble(Args[7]);
            set => Args[7] = value + "";
        }

        public double V2
        {
            get => Convert.ToDouble(Args[8]);
            set => Args[8] = value + "";
        }

        public double V3
        {
            get => Convert.ToDouble(Args[9]);
            set => Args[9] = value + "";
        }

        public double V4
        {
            get => Convert.ToDouble(Args[10]);
            set => Args[10] = value + "";
        }

        public double OverlapAdjusted
        {
            get => Convert.ToDouble(Args[11]);
            set => Args[11] = value + "";
        }

        public double P4
        {
            get => Convert.ToDouble(Args[12]);
            set
            {
                if (Args.Count < 13)
                    Args.Add(value + "");
                else
                    Args[12] = value + "";
            }
        }

        public double P5
        {
            get => Convert.ToDouble(Args[12]);
            set
            {
                if (Args.Count < 14)
                {
                    Args.Add("0"); //default p4
                    Args.Add(value + "");
                }
                else
                {
                    Args[13] = value + "";
                }
            }
        }

        public double V5
        {
            get => Convert.ToDouble(Args[12]);
            set
            {
                if (Args.Count < 15)
                {
                    Args.Add("0"); //default p4
                    Args.Add("0"); //default p5, so glad this doesn't happen a lot.
                    Args.Add(value + "");
                }
                else
                {
                    Args[14] = value + "";
                }
            }
        }

        /// <summary>
        ///     Creates an envelope object (with "%") using information here.
        ///     So yes, you want to get this once and use it a lot then set it.
        ///     Or you can just don't care. Prolly's gonna be okay since the amount of data is really small.
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
            get => new Envelope(string.Join(",", Args.Skip(4).Take(7)
                .Union(Enumerable.Repeat("%", 1))
                .Union(Args.Skip(12))));
            set
            {
                Args = Args.Take(4)
                    .Union(value.Parameters.Take(7).Select(s => s + ""))
                    .Union(Enumerable.Repeat(Args[11], 1))
                    .Union(value.Parameters.Skip(8).Select(s => s + ""))
                    .ToList(); //oh gees. well i did it at least... hope this works...
            }
        }

        /// <summary>
        ///     Length in ticks cut out from raw data.
        /// </summary>
        public double Length
        {
            //This is the first one.
            get => Convert.ToDouble(zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "@").First);
            set => LengthTempoPreUtteranceAdjusted =
                value + "@" + zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "@").Second;
        }

        /// <summary>
        ///     Tempo cut out from raw data.
        /// </summary>
        public double Tempo
        {
            //This is the one in the middle
            get => Convert.ToDouble(
                zusp.CutFirst(zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "@").Second, "+").First);
            set => LengthTempoPreUtteranceAdjusted = Length + "@" + value + "+" + PreUtteranceAdjusted;
        }

        /// <summary>
        ///     Length as milliseconds. Don't know why you need this(since you have the file already) but it's there!
        /// </summary>
        public double LengthMilliseconds
        {
            get => Commons.TicksToMilliseconds(Length, Tempo);
            set => Length = Commons.MillisecondsToTicks(value, Tempo);
        }

        /// <summary>
        ///     PreUtterance Adjusted cut out from raw data.
        /// </summary>
        public double PreUtteranceAdjusted
        {
            get => Convert.ToDouble(zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "+").Second);
//this is the one after + sign
            set => LengthTempoPreUtteranceAdjusted =
                zusp.CutFirst(LengthTempoPreUtteranceAdjusted, "+").First + "+" + value;
        }
    }
}