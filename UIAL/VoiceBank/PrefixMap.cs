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
        public PrefixMap(string fPath) : this(ByLineFileIO.ReadFileNoWhitespace(fPath, zuio.GetEncUde(fPath)).ToArray())
        {
        }

        /// <summary>
        ///     Create an object with given data. will check if all note names are present.
        /// </summary>
        /// <param name="data">
        ///     Lines in the file. Yes, you need to turn a file into lines. Because stupid windows store framework
        ///     whatever don't allow main thread to read file
        /// </param>
        public PrefixMap(IEnumerable<string> data)
        {
            Map = new Dictionary<string, string>();
            var t = data.Select(s => zusp.Split(s, "\t\t"));
            foreach (var p in t) Map.Add(p[0], p[1]);
        }

        /// <summary>
        ///     Note Name to Prefix.
        /// </summary>
        public Dictionary<string, string> Map { get; set; }


        /// <summary>
        ///     Set everything in given range to given mapping, both inclusive.
        /// </summary>
        /// <param name="from">Note Name</param>
        /// <param name="to">Note Name</param>
        /// <param name="mapping"></param>
        public void SetRange(string from, string to, string mapping)
        {
            SetRange(Commons.NoteNameIndexRank[from], Commons.NoteNameIndexRank[to], mapping);
        }

        /// <summary>
        ///     Set everything in given range to given mapping, both inclusive.
        /// </summary>
        /// <param name="from">Note Name</param>
        /// <param name="to">Note Name</param>
        /// <param name="mapping"></param>
        private void SetRange(int from, int to, string mapping)
        {
            //ensure order because sometimes you want to do C3 to C4, sometimes C4 to C3. ok?
            if (from > to)
            {
                from ^= to;
                to ^= from;
                from ^= to;
            }

            for (var i = from; i <= to; i++) Map[Commons.NoteNames[i]] = mapping;
        }

        public override string ToString()
        {
            return string.Join("\r\n", Commons.NoteNames.Reverse().Select(s => s + "\t\t" + Map[s])) + "\r\n";
        }
    }
}