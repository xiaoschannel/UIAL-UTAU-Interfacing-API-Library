using System;
using System.Linq;
using zuoanqh.libzut;

namespace zuoanqh.UIAL.VoiceBank
{
    public class OtoAlias : IComparable<OtoAlias>
    {
        public string FName, Alias;

        /// <summary>
        ///     This would've been private. Please do not bully it.
        /// </summary>
        public double[] numbers;

        public OtoAlias(string s)
        {
            var t = zusp.CutFirst(s, "=");
            var l = zusp.SplitAsIs(t.Second, ",");
            numbers = l.Skip(1).Select(n => Convert.ToDouble(n)).ToArray();
            //TODO Error Handling: l.length==6
            FName = t.First;
            Alias = l[0];
        }

        public double Offset
        {
            get => numbers[0];
            set => numbers[0] = value;
        }

        public double Consonant
        {
            get => numbers[1];
            set => numbers[1] = value;
        }

        public double Cutoff
        {
            get => numbers[2];
            set => numbers[2] = value;
        }

        public double Preutterance
        {
            get => numbers[3];
            set => numbers[3] = value;
        }

        public double Overlap
        {
            get => numbers[4];
            set => numbers[4] = value;
        }

        /// <summary>
        ///     Compares the alias only.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(OtoAlias other)
        {
            //have no idea how to make this into a loop
            var n = Alias.CompareTo(other.Alias);
            if (n != 0) return n;
            n = FName.CompareTo(other.FName);
            if (n != 0) return n;
            n = (int) (Offset - other.Offset);
            if (n != 0) return n;
            n = (int) (Consonant - other.Consonant);
            if (n != 0) return n;
            n = (int) (Cutoff - other.Cutoff);
            if (n != 0) return n;
            n = (int) (Preutterance - other.Preutterance);
            if (n != 0) return n;
            n = (int) (Overlap - other.Overlap);
            return n;
        }

        /// <summary>
        ///     Returns the representation in oto.ini file.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}={1},{2},{3},{4},{5},{6}", FName, Alias, Offset, Consonant, Cutoff, Preutterance,
                Overlap);
        }

        /// <summary>
        ///     TODO: Why there's no ICloneable interface?
        /// </summary>
        /// <returns></returns>
        public OtoAlias Clone()
        {
            return new OtoAlias(ToString());
        }
    }
}