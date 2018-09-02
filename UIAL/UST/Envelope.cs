using System;
using System.Collections.Generic;
using System.Linq;
using zuoanqh.libzut;

namespace zuoanqh.UIAL.UST
{
    /// <summary>
    ///     This class models a envelope. While the entire idea is bizarre, I'm putting down some code to hopefully make your
    ///     life easier.
    /// </summary>
    public class Envelope
    {
        /// <summary>
        ///     This is the threshold we consider two v-value equal.
        /// </summary>
        public const double Epsilon = 0.1;

        public static readonly double DefaultV4 = 0;
        public static readonly double DefaultP5 = 10;
        public static readonly double DefaultV5 = 100;

        /// <summary>
        ///     Encapsulated data.
        /// </summary>
        private readonly double[] _param;

        /// <summary>
        /// </summary>
        /// <param name="data">As in UST's format.</param>
        public Envelope(string data)
        {
            _param = new double[10];

            var ls = zusp.SplitAsIs(data, ",")
                .Select(s => s.Trim()).ToArray(); //gotta trim that string

            if (ls.Length < 7)
                throw new ArgumentException(
                    "Malformed envelope, have " + ls.Length + " parts only, requires 7 or more.");

            for (var i = 0; i < 7; i++) //7 is where the "%" is
                _param[i] = Convert.ToDouble(ls[i]);

            for (var i = 7; i < 10; i++) //assume there is none of the optionals now
                _param[i] = double.NaN;

            if (ls.Length >= 8)
            {
                HasPercentMark = ls[7].Equals("%");
                if (ls.Length < 9) return;
                if (!ls[8].Equals("")) P4 = Convert.ToDouble(ls[8]);
                if (ls.Length < 10) return;
                if (!ls[9].Equals("")) P5 = Convert.ToDouble(ls[9]);
                if (ls.Length < 11) return;
                if (!ls[10].Equals(""))
                    V5 = Convert.ToDouble(ls[10]);
            }
            else
            {
                HasPercentMark = false;
            }
        }

        /// <summary>
        ///     Creates the default envelope used in utau: 0,5,35,0,100,100,0,%
        /// </summary>
        public Envelope() : this("0,5,35,0,100,100,0,%")
        {
        }

        /// <summary>
        ///     (Deep) copy constructor.
        /// </summary>
        /// <param name="that"></param>
        public Envelope(Envelope that) : this(that.ToString())
        {
        }

        /// <summary>
        ///     Please note it seems this does absolutely nothing, so maybe you shouldn't spend time on it.
        /// </summary>
        public bool HasPercentMark { get; }

        /// <summary>
        ///     Please note if a parameter does not exist, it will be NaN.
        ///     Parameters are in the order of p1, p2, p3, v1, v2, v3, v4, p4, p5, v5.
        ///     To get the "raw" data, use ToString().
        ///     To interact with parameters separately, use their name.
        /// </summary>
        public IEnumerable<double> Parameters => _param.ToList();

        /// <summary>
        ///     Length of the "blank" before the sound in ms. Default is 0.
        /// </summary>
        public double P1
        {
            get => _param[0];
            set => _param[0] = value;
        }

        /// <summary>
        ///     Volume in percent. Default is 0.
        /// </summary>
        public double V1
        {
            get => _param[3];
            set => _param[3] = value;
        }

        /// <summary>
        ///     Time between p1 and p2 in ms. Default while not meaningful, is 5.
        /// </summary>
        public double P2
        {
            get => _param[1];
            set => _param[1] = value;
        }

        /// <summary>
        ///     Volume in percent. Default is 100.
        /// </summary>
        public double V2
        {
            get => _param[4];
            set => _param[4] = value;
        }

        /// <summary>
        ///     Time before p4 in ms. Default while not meaningful, is 35.
        /// </summary>
        public double P3
        {
            get => _param[2];
            set => _param[2] = value;
        }

        /// <summary>
        ///     Volume in percent. Default is 100.
        /// </summary>
        public double V3
        {
            get => _param[5];
            set => _param[5] = value;
        }

        /// <summary>
        ///     Length of the "blank" at the end in ms. Note this is relative to the length of note this envelope will be applied
        ///     to.
        /// </summary>
        public double P4
        {
            get => _param[7];
            set => _param[7] = value;
        }

        public bool HasP4 => !double.IsNaN(P4);

        /// <summary>
        ///     Volume in percent. Default is 0. Optional (NaN means 0).
        /// </summary>
        public double V4
        {
            get => _param[6];
            set => _param[6] = value;
        }

        /// <summary>
        ///     Time after p2 in ms. Optional (NaN means 0). Default is 10.
        /// </summary>
        public double P5
        {
            get => _param[8];
            set => _param[8] = value;
        }

        public bool HasP5 => !double.IsNaN(P5);

        /// <summary>
        ///     Volume in percent. Default is 100. Optional (NaN means 100).
        /// </summary>
        public double V5
        {
            get => _param[9];
            set => _param[9] = value;
        }

        public bool HasV5 => !double.IsNaN(V5);

        /// <summary>
        ///     This removes p5 from data (rather than set it to default).
        /// </summary>
        public void RemoveP5()
        {
            P5 = double.NaN;
            V5 = double.NaN;
        }

        /// <summary>
        ///     This method make the highest point in envelope 100, and return how much it have scaled everything.
        /// </summary>
        public double Normalize()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This method tries to zero p values.
        ///     This can fix some envelope problems, so TRY IT!
        /// </summary>
        public void ZeroPValues()
        {
            if (HasP5)
            {
                if (!HasV5) RemoveP5();
                if (HasV5 && Math.Abs(V5 - V2) < Epsilon) P5 = 0;
            }

            if (Math.Abs(V2 - V1) < Epsilon) P2 = 0;
            if (Math.Abs(V3 - V4) < Epsilon) P3 = 0;
        }

        /// <summary>
        ///     Check if the envelope is valid given its length in ms.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool IsValidWith(double length)
        {
            return length > P1 + P2 + P3 + (HasP4 ? P4 : 0) + (HasP5 ? P5 : 0);
        }

        /// <summary>
        ///     Converts it back to original format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var l = new List<object> {P1, P2, P3, V1, V2, V3, V4, HasPercentMark ? "%" : ""};
            var effectiveP4 = HasP4 ? P4 : 0;
            var effectiveP5 = HasP5 ? P5 : 0;
            var effectiveV5 = HasV5 ? V5 : 100;
            if (HasV5)
            {
                l.Add(effectiveP4);
                l.Add(effectiveP5);
                l.Add(effectiveV5);
            }
            else if (HasP5) //but not v5
            {
                l.Add(effectiveP4);
                l.Add(effectiveP5);
            }
            else if (HasP4) //but not v5 or p5
            {
                l.Add(effectiveP4);
            }

            return string.Join(",", l.ToArray());
        }
    }
}