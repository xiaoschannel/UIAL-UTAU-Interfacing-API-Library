﻿using System;
using System.Linq;
using zuoanqh.libzut;

namespace zuoanqh.UIAL.UST
{
    /// <summary>
    ///     This models a vibrato.
    ///     Use ToString() to access the string format if you really have to.
    /// </summary>
    public class Vibrato
    {
        /// <summary>
        ///     Creates an object using UST format text
        /// </summary>
        /// <param name="vbrText"></param>
        public Vibrato(string vbrText)
        {
            Parameters = zusp.SplitAsIs(vbrText, ",")
                .Select(s => s.Equals("") ? 0 : Convert.ToDouble(s))
                .ToArray();
        }

        /// <summary>
        ///     (Deep) copy constructor.
        /// </summary>
        /// <param name="that"></param>
        public Vibrato(Vibrato that)
            : this(that.ToString())
        {
        }

        /// <summary>
        ///     Gives a full-length, everything else 0 Vibrato object.
        /// </summary>
        /// <param name="cycle"></param>
        /// <param name="depth"></param>
        public Vibrato(double cycle, double depth)
            : this("100 " + cycle + " " + depth + " 0 0 0 0 0")
        {
        }

        /// <summary>
        ///     Gives the default vibrato -- "65 180 35 20 20 0 0 0"
        /// </summary>
        public Vibrato() : this("65 180 35 20 20 0 0 0")
        {
        }

        /// <summary>
        ///     The 7 parameters, in order, are: length (% of declared length of note), cycle(ms), depth(cent), in(%), out(%),
        ///     phase(%), pitch(%).
        ///     We don't know what the 8th parameter does.
        ///     pitch is the "y offset", percentage on depth, while phase is the x offset.
        ///     length works even if it's >100%.
        /// </summary>
        public double[] Parameters { get; }

        /// <summary>
        ///     Length in percents. Default is 65.
        /// </summary>
        public double Length
        {
            get => Parameters[0];
            set => Parameters[0] = value;
        }

        /// <summary>
        ///     Inverse of frequency in ms. Default is 180.
        /// </summary>
        public double Cycle
        {
            get => Parameters[1];
            set => Parameters[1] = value;
        }

        /// <summary>
        ///     "Strength" of the vibrato in cents. Default is 35.
        /// </summary>
        public double Depth
        {
            get => Parameters[2];
            set => Parameters[2] = value;
        }

        /// <summary>
        ///     The linear fade-in part in percents. Default is 20.
        /// </summary>
        public double In
        {
            get => Parameters[3];
            set
            {
                if (value + Out > 100)
                    throw new ArgumentException(
                        string.Format("This is highly Illogical. In = {0}%, In + Out = {1}% >100%", value,
                            value + Out));
                Parameters[3] = value;
            }
        }

        /// <summary>
        ///     The linear face-out part in percents. Default is 20.
        /// </summary>
        public double Out
        {
            get => Parameters[4];
            set
            {
                if (value + In > 100)
                    throw new ArgumentException(
                        string.Format("This is highly Illogical. Out = {0}%, In + Out = {1}% >100%", value,
                            value + In));
                Parameters[4] = value;
            }
        }

        /// <summary>
        ///     The time-axis shift of sine wave, in percents. Default is 20.
        /// </summary>
        public double Phase
        {
            get => Parameters[5];
            set => Parameters[5] = value;
        }

        /// <summary>
        ///     The pitch shift of sine wave (why would you want to do this?), in percents (also why percents?), Default is 20.
        /// </summary>
        public double Pitch
        {
            get => Parameters[6];
            set => Parameters[6] = value;
        }

        /// <summary>
        ///     Access the Pitch parameter as cent.
        /// </summary>
        public double PitchAsCent
        {
            get => Pitch * 0.01 * Depth;
            set => Pitch = value / Depth * 100;
        }

        /// <summary>
        ///     Returns the last parameter which is useless AFAIK.
        /// </summary>
        public double EighthParameter
        {
            get => Parameters[7];
            set => Parameters[7] = value;
        }

        /// <summary>
        ///     Returns the magnitude of pitchBend in cents at given time.
        ///     Due to terrible interface given to us, length is required.
        ///     Note this will return 0 if time is outside range, will not throw exceptions.
        /// </summary>
        /// <param name="atTime">time since 0%</param>
        /// <param name="length">How long is "100%"</param>
        /// <returns></returns>
        public double Sample(double atTime, double length)
        {
            var len = length * Length; //vibrato length in ms
            var blank = length - len;
            if (atTime < blank || atTime > length) return 0; //just some house keeping.

            var rTime = atTime - blank; //relative time since start of vibrato.
            var unfaded = Pitch * Depth *
                          Math.Sin((rTime / Cycle + Phase) * zum.PHI); //GO TEAM PHI!!!

            var percentTime = rTime / len; //percent of whole vibrato time, before fade-in and fade-out

            double fadeEffect = 1; //effect of fade-in or fade-out
            if (percentTime < In / 100)
                fadeEffect = percentTime / (In / 100);
            else if (percentTime > (100 - Out) / 100)
                fadeEffect = (100 - percentTime) / (Out / 100);

            return unfaded * fadeEffect;
        }

        /// <summary>
        ///     Converts it back to its ust format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(" ", Parameters);
        }
    }
}