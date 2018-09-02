using System.IO;

namespace zuoanqh.UIAL.VoiceBank
{
    /// <summary>
    ///     This class allows editing of a .frq file.
    /// </summary>
    public class Frq
    {
        public double[][] Data;

        /// <summary>
        ///     40 bytes of data, which we believe is made with (in order)
        ///     8 bytes of "FREQ0003",
        ///     12 bytes of data,
        ///     16 bytes of " speedwagon     ", (I tried to google but found nothing... anyone knows why?)
        ///     4 bytes of data of how many entries are there.
        /// </summary>
        public byte[] Header;

        public Frq(string fPath) : this(File.OpenRead(fPath))
        {
        }

        public Frq(Stream file)
        {
            var length = file.Length;
            Header = new byte[40]; //
            file.Read(Header, 0, Header.Length);
            var data = new byte[length - 40];
            file.Read(data, 40, data.Length);
        }
    }
}