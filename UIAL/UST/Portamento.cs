using System;
using System.Collections.Generic;
using System.Linq;
using zuoanqh.libzut;
using zuoanqh.libzut.Data;

namespace zuoanqh.UIAL.UST
{
    /// <summary>
    ///     Model of a portamento of a note.
    ///     No, we don't preserve input in verbatim, are you crazy?
    /// </summary>
    public class Portamento
    {
        //public string PBSAsText;
        /// <summary>
        ///     Write a function with this signature to accompany your designer-made curve type! which you probably wont do but...
        ///     it's there... just saying...
        /// </summary>
        /// <param name="time">time relative to beginning.</param>
        /// <param name="length">length of the entire segment.</param>
        /// <param name="magnitude">relative pitch change in cent.</param>
        /// <returns></returns>
        public delegate double CurveSegmentHandler(double time, double length, double magnitude);

        public const string PbmSCurve = "", PbmLinear = "s", PbmRCurve = "r", PbmJCurve = "j";

        public static readonly IReadOnlyList<string> VanillaCurveTypes = new[]
            {PbmSCurve, PbmLinear, PbmRCurve, PbmJCurve};

        /// <summary>
        ///     Implemented with cosine interpolation.
        /// </summary>
        public static readonly CurveSegmentHandler SCurveHandler = (time, length, magnitude) =>
            (1 - Math.Cos(zum.Bound(time, 0, length) / length)) / 2 * magnitude;

        /// <summary>
        ///     Implemented with linear interpolation.
        /// </summary>
        public static readonly CurveSegmentHandler LinearHandler = (time, length, magnitude) =>
            zum.Bound(time, 0, length) / length * magnitude;

        /// <summary>
        ///     Gives later half of the S curve
        /// </summary>
        public static readonly CurveSegmentHandler RCurveHandler = (time, length, magnitude) =>
            SCurveHandler(time + length, length * 2, magnitude * 2) - magnitude;

        /// <summary>
        ///     Gives first half of the S curve
        /// </summary>
        public static readonly CurveSegmentHandler JCurveHandler = (time, length, magnitude) =>
            SCurveHandler(time, length * 2, magnitude * 2);

        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<string, CurveSegmentHandler> _curveTypeHandlers =
            new Dictionary<string, CurveSegmentHandler>();

        static Portamento()
        {
            //adding vanilla curve handlers
            AddCurveType(PbmSCurve, SCurveHandler);
            AddCurveType(PbmLinear, LinearHandler);
            AddCurveType(PbmRCurve, RCurveHandler);
            AddCurveType(PbmJCurve, JCurveHandler);
        }

        /// <summary>
        ///     Only PBW is required to make a Portamento.
        ///     If you provide empty on the others, we will use the default value:
        ///     0;(invalid) for pbs, 0 for pby, "" or s-curve for pbm
        ///     If pbs does not have second parameter, it will be considered invalid as well.
        ///     it is outside this class or USTNote's power to get information about previous notes, please read UST's constructor
        ///     for its handling.
        /// </summary>
        /// <param name="pbs"></param>
        /// <param name="pbw"></param>
        /// <param name="pby"></param>
        /// <param name="pbm"></param>
        public Portamento(string pbw, string pbs, string pby, string pbm)
        {
            Segments = new List<PortamentoSegment>();

            if (pbs == null || pbs.Trim().Length == 0)
            {
                Pbs = new[] {0, double.NaN};
            }
            else
            {
                if (!pbs.Contains(";")) //starting y is 0
                {
                    if (!pbs.Contains(",")) //don't you just love working with legendary code
                        Pbs = new[] {Convert.ToDouble(pbs), double.NaN};
                    else
                        Pbs = zusp.Split(pbs, ",").Select(s => Convert.ToDouble(s)).ToArray();
                }
                else
                {
                    Pbs = zusp.Split(pbs, ";").Select(s => Convert.ToDouble(s)).ToArray();
                }
            }

            var w = zusp.SplitAsIs(pbw, ",")
                .Select(s => s.Equals("") ? 0 : Convert.ToDouble(s)) //empty entries means 0. 
                .ToList();

            var y = zusp.SplitAsIs(pby, ",")
                .Select(s => s.Equals("") ? 0 : Convert.ToDouble(s)) //why though? i wonder.
                .ToList();

            while (y.Count < w.Count - 1)
                y.Add(0); //-1 because last point must be 0 as far as utau's concern, which is stupid.

            var m = zusp.SplitAsIs(pbm, ",").ToList();

            while (m.Count < w.Count) m.Add(PbmSCurve);

            //initialize segments
            for (var i = 0; i < w.Count - 1; i++) //last segment is a special case.
                Segments.Add(new PortamentoSegment(w[i], y[i], m[i]));
            Segments.Add(new PortamentoSegment(w[w.Count - 1], 0, m[w.Count - 1]));

            //now fill the virtual arrays.
            Pbw = new VirtualArray<double>(i => Segments[i].Pbw, (i, v) => Segments[i].Pbw = v,
                () => Segments.Count);
            Pby = new VirtualArray<double>(i => Segments[i].Pby, (i, v) => Segments[i].Pbw = v,
                () => Segments.Count - 1);
            Pbm = new VirtualArray<string>(i => Segments[i].Pbm, (i, v) => Segments[i].Pbm = v,
                () => Segments.Count);
        }

        /// <summary>
        ///     (Deep) copy constructor.
        /// </summary>
        /// <param name="that"></param>
        public Portamento(Portamento that)
            : this(that.PbwText, that.PbsText, that.PbyText, that.PbmText)
        {
        }

        public VirtualArray<string> Pbm { get; }

        /// <summary>
        ///     [0] is the time difference relative to start of note (not envelope, duh)
        ///     [1] is pitch difference relative to this note in 10-cents.
        ///     This does not have a valid default and will be NaN if not present when creating the portamento.
        /// </summary>
        public double[] Pbs { get; }

        /// <summary>
        ///     All units in ms, default is 0.
        /// </summary>
        public VirtualArray<double> Pbw { get; }

        /// <summary>
        ///     All units are in 10-cents
        /// </summary>
        public VirtualArray<double> Pby { get; }

        /// <summary>
        ///     I can't keep the encapsulation because there's one PBY less than other parameters and i don't know how to handle
        ///     it.
        ///     So enjoy public members! yay! Good thing is this is the only place data is stored and everything else is
        ///     functional.
        ///     By default, last segment will have PBY of 0 just so you know.
        /// </summary>
        public List<PortamentoSegment> Segments { get; }

        /// <summary>
        ///     please use add/remove method to edit this.
        /// </summary>
        public static IReadOnlyDictionary<string, CurveSegmentHandler> CurveTypeHandlers => _curveTypeHandlers;

        public string PbsText => $"{Pbs[0]};{Pbs[1]}";

        public string PbwText
        {
            get { return string.Join(" ", Pbw.Select(s => s.Equals(0) ? "" : s + "").ToArray()); }
        }

        public string PbyText
        {
            get { return string.Join(" ", Pby.Select(s => s.Equals(0) ? "" : s + "").ToArray()); }
        }

        public string PbmText => string.Join(" ", Pbm.ToArray());

        /// <summary>
        ///     Add your own curve type!
        ///     Note your identifier cannot contain space.
        /// </summary>
        /// <param name="pbmIdentifier"></param>
        /// <param name="segmentHandler"></param>
        public static void AddCurveType(string pbmIdentifier, CurveSegmentHandler segmentHandler)
        {
            if (_curveTypeHandlers.ContainsKey(pbmIdentifier))
                throw new ArgumentException("You cannot modify existing curve type: " + pbmIdentifier);
            if (pbmIdentifier.Contains(" "))
                throw new ArgumentException("Identifier contains space at: " +
                                            pbmIdentifier.IndexOf(" ", StringComparison.Ordinal));

            _curveTypeHandlers.Add(pbmIdentifier, segmentHandler);
        }

        /// <summary>
        ///     This happens when pbs does not have a second number.
        ///     To keep everything minimal, we made the design decision you must the correct value,
        ///     i.e. relative pitch difference with previous note in 10-cents
        /// </summary>
        /// <returns></returns>
        public bool HasValidPbs1()
        {
            return !double.IsNaN(Pbs[1]);
        }

        /// <summary>
        ///     Returns the change in a given segment of pitchBend line.
        /// </summary>
        /// <param name="segmentIndex">Starts at 0.</param>
        /// <returns></returns>
        private double MagnitudeAt(int segmentIndex)
        {
            if (segmentIndex == 0) //first point starts at 0
            {
                if (double.IsNaN(Pbs[1]))
                    throw new InvalidOperationException("Please provide PBS[1] before sampling.");
                return Pby[0] - Pbs[1];
                //note this^ does not happen in vanilla, UTAU actually always use previous note's pitch rather than pbs.
                //Hence PBS[1] actually means nothing except for display purpouse in utau. which is a very bad practice.
                //By fixing it, we might make some extreme cases sounds differently, but we think this is for the better.
            }

            if (segmentIndex < Pbw.Length - 1) //middle points. segment 1 is between 2nd and 3rd point, or pby's 0 and 1
                return Pby[segmentIndex] - Pby[segmentIndex - 1];
            if (segmentIndex == Pbw.Length - 1) //last point ends at 0, hence 0 minus last y
                return -Pby[Pby.Length - 1];

            throw new IndexOutOfRangeException("Segment " + segmentIndex + " does not exist.");
        }


        /// <summary>
        ///     Returns magnitude at time with respect to start of the pitchBend.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public double SampleAtTime(double time)
        {
            if (time < 0) return 0; //housekeeping
            var seg = 0; //which segment is the required point located
            var rTime = time; //relative time to start of current interval
            while (rTime > Pbw[seg])
            {
                rTime -= Pbw[seg];
                seg++;
                if (seg >= Pbw.Length)
                    return
                        0; //if we went through the whole thing. no i don't want to write two conditions and check after the loop. too much typing.
            }

            return _curveTypeHandlers[Pbm[seg]].Invoke(rTime, Pbw[seg], MagnitudeAt(seg));
        }

        /// <summary>
        ///     Converts it back to its ust format.
        ///     Again, while the data will mean the same, it will look different.
        ///     Because the using empty string to mean "0" is just not very readable.
        /// </summary>
        /// <returns></returns>
        public List<string> ToStringList()
        {
            var ans = new List<string>
            {
                USTNote.KeyPbs + "=" + PbsText, USTNote.KeyPbw + "=" + PbwText, USTNote.KeyPby + "=" + PbyText,
                USTNote.KeyPbm + "=" + PbmText
            };
            return ans;
        }

        public override string ToString()
        {
            return string.Join("\r\n", ToStringList().ToArray()) + "\r\n";
        }
    }
}