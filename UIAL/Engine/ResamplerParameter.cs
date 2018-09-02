﻿using System;
using System.Collections.Generic;
using System.Linq;
using zuoanqh.UIAL.UST;

namespace zuoanqh.UIAL.Engine
{
    /// <summary>
    ///     We have referenced parameter meanings from td_fnds's source code here.
    ///     0: input file
    ///     1: output file
    ///     2: note name
    ///     3: consonant velocity, default 100
    ///     4: flags
    ///     5: offset, default VB
    ///     6: length
    ///     (too lazy to continue)
    /// </summary>
    public class ResamplerParameter
    {
        /// <summary>
        ///     Create a new instance with given parameters.
        /// </summary>
        /// <param name="args"></param>
        public ResamplerParameter(params string[] args)
        {
            Args = args;
        }

        /// <summary>
        ///     Create a new instance with empty parameters.
        /// </summary>
        public ResamplerParameter()
        {
            Args = new string[13]; //there's 13 parameters
        }

        public ResamplerParameter(IEnumerable<string> args)
            : this(args.ToArray())
        {
        }

        public string[] Args { get; set; }

        public string InputFile
        {
            get => Args[0];
            set => Args[0] = value;
        }

        public string OutputFile
        {
            get => Args[1];
            set => Args[1] = value;
        }

        /// <summary>
        ///     This gives note's note name such as "C3", "F#4".
        ///     If you want the number in UST file, use "NoteNum", they works the same way.
        /// </summary>
        public string NoteName
        {
            get => Args[2];
            set => Args[2] = value;
        }

        /// <summary>
        ///     This gives the number in UST files, a.k.a "NoteNum". This works the same way as NoteName attribute.
        /// </summary>
        public int NoteNum
        {
            get => Commons.NoteNameIndexUst[NoteName];
            set => NoteName = Commons.GetNoteName(value);
        }

        /// <summary>
        ///     Velocity, from 0 to 200.
        /// </summary>
        public double Velocity
        {
            get => Convert.ToDouble(Args[3]);
            set => Args[3] = value + "";
        }

        public double VelocityFactor
        {
            get => Commons.GetEffectiveVelocityFactor(Velocity);
            set => Velocity = Commons.GetVelocity(value);
        }

        public string FlagText
        {
            get => Args[4];
            set => Args[4] = value;
        }

        /// <summary>
        ///     Please note Flags is immutable, so you need to reassign it like "Flags = Flags.SetFlagValue(blah blah blah)" after
        ///     set value. or you can change the string.
        /// </summary>
        public Flags Flags
        {
            get => new Flags(Args[4]);
            set => Args[4] = value.FlagText;
        }

        /// <summary>
        ///     In milliseconds.
        /// </summary>
        public double Offset
        {
            get => Convert.ToDouble(Args[5]);
            set => Args[5] = value + "";
        }

        /// <summary>
        ///     Length expected for output .wav file in milliseconds.
        /// </summary>
        public double OutputLength
        {
            get => Convert.ToDouble(Args[6]);
            set => Args[6] = value + "";
        }

        /// <summary>
        ///     Oto setting.
        /// </summary>
        public double ConsonantLength
        {
            get => Convert.ToDouble(Args[7]);
            set => Args[7] = value + "";
        }

        /// <summary>
        ///     Oto setting.
        /// </summary>
        public double Cutoff
        {
            get => Convert.ToDouble(Args[8]);
            set => Args[8] = value + "";
        }

        /// <summary>
        ///     for volume, in percentage.
        /// </summary>
        public double Intensity
        {
            get => Convert.ToDouble(Args[9]);
            set => Args[9] = value + "";
        }

        /// <summary>
        ///     In percentage.
        /// </summary>
        public double Modulation
        {
            get => Convert.ToDouble(Args[10]);
            set => Args[10] = value + "";
        }

        /// <summary>
        ///     There's a ! because there's a !, we don't know why, but everyone's using it that way so we had to do it that way to
        ///     make it work.
        /// </summary>
        public double Tempo
        {
            get => Convert.ToDouble(Args[11].Substring(1));
            set => Args[11] = "!" + value;
        }

        /// <summary>
        ///     To get the processed pitchebnd array, use GetPutchbendArray().
        /// </summary>
        public string PitchBendString
        {
            get => Args[12];
            set => Args[12] = value;
        }

        /// <summary>
        ///     This decodes the pitchBend string into actual pitchBends in unit of cents, zeroed at given register. They only goes
        ///     from -2048 to 2048.
        /// </summary>
        public int[] GetPitchBendArray()
        {
            return Commons.DecodePitchBends(PitchBendString);
        }

        /// <summary>
        ///     Use this if you only change one value in the pitchBend.
        ///     Use "SetPitchBends" for many values.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void SetPitchBend(int index, int value)
        {
            var array = Commons.DecodePitchBends(PitchBendString);
            array[index] = value;
            PitchBendString = Commons.EncodePitchBends(array);
        }

        /// <summary>
        /// </summary>
        /// <param name="newPitchBends"></param>
        public void SetPitchBends(int[] newPitchBends)
        {
            PitchBendString = Commons.EncodePitchBends(newPitchBends);
        }

        /// <summary>
        ///     Set a region in the original. Start and End both inclusive.
        ///     This is for when you have only the region you want to set.
        ///     if you have the entire array, use SetPitchBends(int[])
        /// </summary>
        /// <param name="start">Inclusive.</param>
        /// <param name="end">Inclusive.</param>
        /// <param name="pitchBends">the data for that region.</param>
        public void SetPitchBends(int start, int end, int[] pitchBends)
        {
            var array = Commons.DecodePitchBends(PitchBendString);
            for (var i = start; i <= end; i++) array[i] = pitchBends[i - start];
            PitchBendString = Commons.EncodePitchBends(array);
        }

        /// <summary>
        ///     This makes it shorter. Don't worry, data will be unchanged.
        /// </summary>
        public void RecodePitchBendString()
        {
            PitchBendString = Commons.EncodePitchBends(Commons.DecodePitchBends(PitchBendString));
        }
    }
}