using System;
using System.Collections.Generic;
using System.Text;

namespace zuoanqh.UIAL.Extensions.Mrq4Cs
{
  /// <summary>
  /// Please note our MrqEntry also have a filename, kind of like the "mrq_fenum"
  /// </summary>
  public class MrqEntry
  {
    /// <summary>
    /// The name of the file this entry corresponds to
    /// </summary>
    public string FileName;

    /// <summary>
    /// We don't know what this is for yet.
    /// </summary>
    public int NHop;

    /// <summary>
    /// Alias of Fs.
    /// </summary>
    public int SamplingRate { get { return Fs; } set { Fs = value; } }

    /// <summary>
    /// This seems to mean the sampling rate of the .wav file in voice bank, so I added an alias.
    /// </summary>
    public int Fs;

    /// <summary>
    /// The most important part! the data!
    /// </summary>
    public float[] F0;

    /// <summary>
    /// it is exactly f0.length -- This exists to look beautiful on your code.
    /// </summary>
    public int Nf0 { get { return F0.Length; } }

    /// <summary>
    /// The "size" as stored in the file.
    /// </summary>
    public int Size;

    /// <summary>
    /// the "size" needed to store this entry.
    /// Calculated on the fly.
    /// </summary>
    public int NeededSize { get { return (Nf0 + 3) * 4; } }

    /// <summary>
    /// Returns whether this entry is marked as deleted. A deleted entry's file name starts with '\0'. 
    /// A more efficient way is of course just check those bytes. but yeah....
    /// </summary>
    /// <returns></returns>
    public bool IsMarkedDeleted { get { return FileName[0] == '\0'; } }

    /// <summary>
    /// This does not initialize the object, use at own risk!
    /// </summary>
    public MrqEntry()
    { }

    /// <summary>
    /// Initializes the object with an empty array of given length.
    /// </summary>
    /// <param name="Nf0">Length of F0 array</param>
    public MrqEntry(int Nf0)
    {
      F0 = new float[Nf0];
    }

  }

}
