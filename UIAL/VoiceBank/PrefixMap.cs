using System.Collections.Generic;
using System.Linq;
using zuoanqh.libzut;
using zuoanqh.libzut.FileIO;

namespace zuoanqh.UIAL.VoiceBank
{
    /// <summary>
    ///     Model for prefix.map file. Please note despite it saying prefix, it is applied as a postfix.
    /// </summary>
    public class PrefixMap
    {
        /// <summary>
        ///     Note Name to Prefix.
        /// </summary>
        public Dictionary<string, string> Map;

        public PrefixMap(string fPath) : this(ByLineFileIO.ReadFileNoWhitespace(fPath, zuio.GetEncUde(fPath)).ToArray())
        {
        }

        /// <summary>
        ///     Create an object with given data. will check if all note names are present.
        /// </summary>
        /// <param name="Data">
        ///     Lines in the file. Yes, you need to turn a file into lines. Because stupid windows store framework
        ///     whatever don't allow main thread to read file
        /// </param>
        public PrefixMap(string[] Data)
        {
            Map = new Dictionary<string, string>();
            var t = Data.Select(s => zusp.Split(s, "\t\t"));
            foreach (var p in t) Map.Add(p[0], p[1]);
        }


        /// <summary>
        ///     Set everything in given range to given mapping, both inclusive.
        /// </summary>
        /// <param name="From">Note Name</param>
        /// <param name="To">Note Name</param>
        /// <param name="Mapping"></param>
        public void SetRange(string From, string To, string Mapping)
        {
            SetRange(CommonReferences.NOTENAME_INDEX_RANK[From], CommonReferences.NOTENAME_INDEX_RANK[To], Mapping);
        }

        /// <summary>
        ///     Set everything in given range to given mapping, both inclusive.
        /// </summary>
        /// <param name="From">Note Name</param>
        /// <param name="To">Note Name</param>
        /// <param name="Mapping"></param>
        private void SetRange(int From, int To, string Mapping)
        {
            //ensure order because sometimes you want to do C3 to C4, sometimes C4 to C3. ok?
            if (From > To)
            {
                From ^= To;
                To ^= From;
                From ^= To;
            }

            for (var i = From; i <= To; i++) Map[CommonReferences.NOTENAMES[i]] = Mapping;
        }

        public override string ToString()
        {
            return string.Join("\r\n", CommonReferences.NOTENAMES.Reverse().Select(s => s + "\t\t" + Map[s])) + "\r\n";
        }
    }
}