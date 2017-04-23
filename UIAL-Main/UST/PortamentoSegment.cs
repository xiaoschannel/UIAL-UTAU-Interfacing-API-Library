namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// A data class that hopefully makes your life easier. 
  /// </summary>
  public class PortamentoSegment
  {
    public double PBW;
    public double PBY;
    public string PBM;

    public PortamentoSegment(double PBW, double PBY, string PBM)
    {
      this.PBW = PBW;
      this.PBY = PBY;
      this.PBM = PBM;
    }

    /// <summary>
    /// Creates an empty instance with 0 length, 0 pitchbend, s-curve.
    /// </summary>
    public PortamentoSegment() : this(0, 0, "") { }
  }
}
