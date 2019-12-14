namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// A data class. 
  /// </summary>
  public class PortamentoSegment
  {
    /// <summary>
    /// Length
    /// </summary>
    public double PBW;
    /// <summary>
    /// Pitchbend
    /// </summary>
    public double PBY;
    /// <summary>
    /// Curve Type Identifier
    /// </summary>
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
