using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using zuoanqh.libzut;
using zuoanqh.libzut.Data;

namespace zuoanqh.UIAL
{
    public class CommonReferences
    {
        /// <summary>
        ///     This seems to be the default for utau.
        /// </summary>
        public static readonly double TICKS_PER_BEAT = 480;

        private static readonly double CONVERSION_RATE = 60000 / TICKS_PER_BEAT;

        /// <summary>
        ///     Encoding of the 13th parameter given to resamplers.
        /// </summary>
        public static readonly string PITCHBEND_ENCODING =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        /// <summary>
        ///     All possible note names from C1 to B7.
        /// </summary>
        public static readonly IReadOnlyList<string> NOTENAMES;

        public static readonly string NOTENAME_HIGHEST;
        public static readonly string NOTENAME_LOWEST;

        /// <summary>
        ///     This reverse the mapping of NOTENAMES.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, int> NOTENAME_INDEX_RANK;

        /// <summary>
        ///     This converts note names into NoteNums.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, int> NOTENAME_INDEX_UST;

        static CommonReferences()
        {
            NOTENAMES = new[]
            {
                "C1", "C#1", "D1", "D#1", "E1", "F1", "F#1", "G1", "G#1", "A1", "A#1", "B1",
                "C2", "C#2", "D2", "D#2", "E2", "F2", "F#2", "G2", "G#2", "A2", "A#2", "B2",
                "C3", "C#3", "D3", "D#3", "E3", "F3", "F#3", "G3", "G#3", "A3", "A#3", "B3",
                "C4", "C#4", "D4", "D#4", "E4", "F4", "F#4", "G4", "G#4", "A4", "A#4", "B4",
                "C5", "C#5", "D5", "D#5", "E5", "F5", "F#5", "G5", "G#5", "A5", "A#5", "B5",
                "C6", "C#6", "D6", "D#6", "E6", "F6", "F#6", "G6", "G#6", "A6", "A#6", "B6",
                "C7", "C#7", "D7", "D#7", "E7", "F7", "F#7", "G7", "G#7", "A7", "A#7", "B7"
            }.ToList();

            NOTENAME_HIGHEST = NOTENAMES[NOTENAMES.Count - 1];
            NOTENAME_LOWEST = NOTENAMES[0];

            var indexrank = new Dictionary<string, int>();
            var indexust = new Dictionary<string, int>();

            for (var i = 0; i < NOTENAMES.Count; i++)
            {
                indexrank.Add(NOTENAMES[i], i);
                indexust.Add(NOTENAMES[i], i + 24); //0 is 24 in USTs.
            }

            NOTENAME_INDEX_RANK = indexrank;
            NOTENAME_INDEX_UST = indexust;
        }

        private CommonReferences()
        {
        }

        public static double TicksToMilliseconds(double Ticks, double BPM)
        {
            return Ticks * CONVERSION_RATE / BPM;
        }

        public static double MillisecondsToTicks(double Milliseconds, double BPM)
        {
            return Milliseconds * BPM / CONVERSION_RATE;
        }

        /// <summary>
        ///     Return the effect of Velocity in multiplier -- 1 means 100%.
        /// </summary>
        /// <param name="Velocity"></param>
        /// <returns></returns>
        public static double GetEffectiveVelocityFactor(double Velocity)
        {
            return 2 * Math.Pow(0.5, Velocity / 100);
        }

        /// <summary>
        ///     Convert from a multiplier back to its velocity value.
        /// </summary>
        /// <param name="EffectiveVelocityFactor"></param>
        /// <returns></returns>
        public static double GetVelocity(double EffectiveVelocityFactor)
        {
            return Math.Log(Math.Pow(EffectiveVelocityFactor / 2, 100), 0.5);
        }

        /// <summary>
        ///     Converts a character to its encoded number.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int GetEncoding(char c)
        {
            return PITCHBEND_ENCODING.IndexOf(c);
        }

        /// <summary>
        ///     Encode a sequence of pitchbend magnitudes into the string for resampler's 13th parameter.
        /// </summary>
        /// <param name="Pitchbends"></param>
        /// <returns></returns>
        public static string EncodePitchbends(int[] Pitchbends)
        {
            //first fold it into pairs of (pitch, times repeated)
            var l = new List<Pair<int, int>>();
            var current = 0;
            while (current < Pitchbends.Length)
            {
                var count = 1;

                //count how many time the current element has appeared
                for (var i = current + 1; i < Pitchbends.Length; i++)
                {
                    if (Pitchbends[i] != Pitchbends[current]) break;
                    count++;
                }

                l.Add(new Pair<int, int>(Pitchbends[current], count));
                current += count;
            }

            //now encode that string.
            var ans = new StringBuilder();
            foreach (var v in l)
            {
                //convert things back.
                var val = v.First;
                if (val < 0) val += 4096; //again some magic defined by original encoding algorithm

                //segment the two digits out
                ans.Append(PITCHBEND_ENCODING[val / 64])
                    .Append(PITCHBEND_ENCODING[val % 64]);

                if (v.Second > 2) //if repeated, add that bit.
                    ans.Append("#").Append(v.Second).Append("#");
                else if (v.Second == 2) //had to do some testing to find this out.
                    ans.Append(ans[ans.Length - 2]).Append(ans[ans.Length - 2]);
            }

            return ans.ToString();
        }

        /// <summary>
        ///     Decode the string given to resampler's 13th parameter back to pitchbend magnitudes.
        /// </summary>
        /// <param name="PitchbendString"></param>
        /// <returns></returns>
        public static int[] DecodePitchbends(string PitchbendString)
        {
            // Step one: segment the string.
            var segments = new List<string>();
            var input = PitchbendString;

            while (true)
            {
                string s;
                if (input.Length < 2) break;
                if (input.Length > 2 && input[2] == '#') //for repeated cases
                {
                    s = Regex.Match(input, @"..#[\d]+").Value;
                    input = zusp.Drop(input, s.Length + 1); //skip the second "#" mark
                } //for a "single".
                else
                {
                    s = zusp.Left(input, 2);
                    input = zusp.Drop(input, s.Length);
                }

                segments.Add(s);
            }

            // Step two: convert it into a linked list
            var l = new LinkedList<int>();
            foreach (var s in segments)
            {
                //convert two-digit code back to an integer.
                var i = GetEncoding(s[0]) * 64 + GetEncoding(s[1]);
                if (i >= 2048) i -= 4096; //i know, that IS weird, but that is what we need to work with.

                if (s.Contains("#")) //if repeated add that many times.
                    for (var j = 0; j < Convert.ToInt32(zusp.Drop(s, 3)); j++)
                        l.AddLast(i);
                else //else one time.
                    l.AddLast(i);
            }

            // Step three: convert it into an array.
            return l.ToArray();
        }

        /// <summary>
        ///     Converts NoteNum into its note name.
        /// </summary>
        /// <param name="NoteNum">C1 is 24.</param>
        /// <returns></returns>
        public static string GetNoteName(int NoteNum)
        {
            return NOTENAMES[NoteNum - 24];
        }
    }
}