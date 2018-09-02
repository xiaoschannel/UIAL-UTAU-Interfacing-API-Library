namespace zuoanqh.UIAL.UST
{
    /// <summary>
    ///     A data class that hopefully makes your life easier.
    /// </summary>
    public class PortamentoSegment
    {
        public PortamentoSegment(double pbw, double pby, string pbm)
        {
            Pbw = pbw;
            Pby = pby;
            Pbm = pbm;
        }

        /// <summary>
        ///     Creates an empty instance with 0 length, 0 pitchBend, s-curve.
        /// </summary>
        public PortamentoSegment() : this(0, 0, "")
        {
        }

        public string Pbm { get; set; }
        public double Pbw { get; set; }
        public double Pby { get; set; }
    }
}