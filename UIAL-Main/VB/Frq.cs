using System.IO;

namespace zuoanqh.UIAL.VB
{
  /// <summary>
  /// This class allows editing of a .frq file.
  /// TODO: write file
  /// </summary>
  public class Frq
  {
    /// <summary>
    /// 40 bytes of data, which we believe is made with (in order) 
    /// 8 bytes of "FREQ0003", 
    /// 12 bytes of data, 
    /// 16 bytes of " speedwagon     ", (No idea what it does)
    /// 4 bytes of data on how many entries are there.
    /// </summary>
    public byte[] Header;

    public double[][] Data;

    public Frq(string fPath) : this(File.OpenRead(fPath)) { }

    public Frq(Stream file)
    {
      long length = file.Length;
      Header = new byte[40];//
      file.Read(Header, 0, Header.Length);
      byte[] data = new byte[length - 40];
      file.Read(data, 40, data.Length);
    }
  }
}

