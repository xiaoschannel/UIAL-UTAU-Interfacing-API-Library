using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using zuoanqh.libzut;
using zuoanqh.libzut.Data;

namespace zuoanqh.UIAL
{
    public static class Commons
    {
        /// <summary>
        ///     This seems to be the default for utau.
        /// </summary>
        public const double TicksPerBeat = 480;

        private const double ConversionRate = 60000 / TicksPerBeat;

        /// <summary>
        ///     Encoding of the 13th parameter given to resamplers.
        /// </summary>
        public const string PitchBendEncoding =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";

        /// <summary>
        ///     All possible note names from C1 to B7.
        /// </summary>
        public static readonly IReadOnlyList<string> NoteNames = new[]
        {
            "C1", "C#1", "D1", "D#1", "E1", "F1", "F#1", "G1", "G#1", "A1", "A#1", "B1",
            "C2", "C#2", "D2", "D#2", "E2", "F2", "F#2", "G2", "G#2", "A2", "A#2", "B2",
            "C3", "C#3", "D3", "D#3", "E3", "F3", "F#3", "G3", "G#3", "A3", "A#3", "B3",
            "C4", "C#4", "D4", "D#4", "E4", "F4", "F#4", "G4", "G#4", "A4", "A#4", "B4",
            "C5", "C#5", "D5", "D#5", "E5", "F5", "F#5", "G5", "G#5", "A5", "A#5", "B5",
            "C6", "C#6", "D6", "D#6", "E6", "F6", "F#6", "G6", "G#6", "A6", "A#6", "B6",
            "C7", "C#7", "D7", "D#7", "E7", "F7", "F#7", "G7", "G#7", "A7", "A#7", "B7"
        };

        public static readonly string NoteNameHighest = NoteNames.Last();
        public static readonly string NoteNameLowest = NoteNames.First();

        /// <summary>
        ///     This reverse the mapping of NOTENAMES.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, int> NoteNameIndexRank =
            NoteNames.Select((x, i) => (x, i)).ToDictionary(x => x.Item1, x => x.Item2);

        /// <summary>
        ///     This converts note names into NoteNums.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, int> NoteNameIndexUst =
            NoteNames.Select((x, i) => (x, i + 24)).ToDictionary(x => x.Item1, x => x.Item2);

        public static double TicksToMilliseconds(double ticks, double bpm)
        {
            return ticks * ConversionRate / bpm;
        }

        public static double MillisecondsToTicks(double milliseconds, double bpm)
        {
            return milliseconds * bpm / ConversionRate;
        }

        /// <summary>
        ///     Return the effect of Velocity in multiplier -- 1 means 100%.
        /// </summary>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public static double GetEffectiveVelocityFactor(double velocity)
        {
            return 2 * Math.Pow(0.5, velocity / 100);
        }

        /// <summary>
        ///     Convert from a multiplier back to its velocity value.
        /// </summary>
        /// <param name="effectiveVelocityFactor"></param>
        /// <returns></returns>
        public static double GetVelocity(double effectiveVelocityFactor)
        {
            return Math.Log(Math.Pow(effectiveVelocityFactor / 2, 100), 0.5);
        }

        /// <summary>
        ///     Converts a character to its encoded number.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static int GetEncoding(char c)
        {
            return PitchBendEncoding.IndexOf(c);
        }

        /// <summary>
        ///     Encode a sequence of pitchBend magnitudes into the string for resampler's 13th parameter.
        /// </summary>
        /// <param name="pitchBends"></param>
        /// <returns></returns>
        public static string EncodePitchBends(int[] pitchBends)
        {
            //first fold it into pairs of (pitch, times repeated)
            var l = new List<Pair<int, int>>();
            var current = 0;
            while (current < pitchBends.Length)
            {
                var count = 1;

                //count how many time the current element has appeared
                for (var i = current + 1; i < pitchBends.Length; i++)
                {
                    if (pitchBends[i] != pitchBends[current]) break;
                    count++;
                }

                l.Add(new Pair<int, int>(pitchBends[current], count));
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
                ans.Append(PitchBendEncoding[val / 64])
                    .Append(PitchBendEncoding[val % 64]);

                if (v.Second > 2) //if repeated, add that bit.
                    ans.Append("#").Append(v.Second).Append("#");
                else if (v.Second == 2) //had to do some testing to find this out.
                    ans.Append(ans[ans.Length - 2]).Append(ans[ans.Length - 2]);
            }

            return ans.ToString();
        }

        /// <summary>
        ///     Decode the string given to resampler's 13th parameter back to pitchBend magnitudes.
        /// </summary>
        /// <param name="pitchBendString"></param>
        /// <returns></returns>
        public static int[] DecodePitchBends(string pitchBendString)
        {
            // Step one: segment the string.
            var segments = new List<string>();
            var input = pitchBendString;

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
        /// <param name="noteNum">C1 is 24.</param>
        /// <returns></returns>
        public static string GetNoteName(int noteNum)
        {
            return NoteNames[noteNum - 24];
        }
    }
}