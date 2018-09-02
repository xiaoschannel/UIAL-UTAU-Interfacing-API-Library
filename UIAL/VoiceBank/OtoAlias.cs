using System;
using System.Linq;
using zuoanqh.libzut;

namespace zuoanqh.UIAL.VoiceBank
{
    public class OtoAlias : IComparable<OtoAlias>
    {
        /// <summary>
        ///     This would've been private. Please do not bully it.
        /// </summary>
        public double[] Numbers;

        public OtoAlias(string s)
        {
            var t = zusp.CutFirst(s, "=");
            var l = zusp.SplitAsIs(t.Second, ",");
            Numbers = l.Skip(1).Select(n => Convert.ToDouble(n)).ToArray();
            //TODO Error Handling: l.length==6
            FName = t.First;
            Alias = l[0];
        }

        public string FName { get; set; }
        public string Alias { get; set; }

        public double Offset
        {
            get => Numbers[0];
            set => Numbers[0] = value;
        }

        public double Consonant
        {
            get => Numbers[1];
            set => Numbers[1] = value;
        }

        public double Cutoff
        {
            get => Numbers[2];
            set => Numbers[2] = value;
        }

        public double PreUtterance
        {
            get => Numbers[3];
            set => Numbers[3] = value;
        }

        public double Overlap
        {
            get => Numbers[4];
            set => Numbers[4] = value;
        }

        /// <summary>
        ///     Compares the alias only.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(OtoAlias other)
        {
            //have no idea how to make this into a loop
            var n = string.Compare(Alias, other.Alias, StringComparison.Ordinal);
            if (n != 0) return n;
            n = string.Compare(FName, other.FName, StringComparison.Ordinal);
            if (n != 0) return n;
            n = (int) (Offset - other.Offset);
            if (n != 0) return n;
            n = (int) (Consonant - other.Consonant);
            if (n != 0) return n;
            n = (int) (Cutoff - other.Cutoff);
            if (n != 0) return n;
            n = (int) (PreUtterance - other.PreUtterance);
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
            return $"{FName}={Alias},{Offset},{Consonant},{Cutoff},{PreUtterance},{Overlap}";
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