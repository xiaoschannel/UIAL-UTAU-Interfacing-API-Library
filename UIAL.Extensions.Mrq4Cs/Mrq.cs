using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections;

namespace zuoanqh.UIAL.Extensions.Mrq4Cs
{
  /// <summary>
  /// This is an alternative implementation of https://github.com/Sleepwalking/mrq, for C#. 
  /// You're probably looking for either:
  /// 1) var entry = Mrq.DoYourThingPlease(args[1]); //if you write your own resampler
  /// 2) var file = Mrq.GetDictionary(path); //if you write something else that use .mrq files.
  /// If you're one of the few unfortunate ones that need to use other methods, just remember, EVERYTHING changes the position of the FileStream. :)
  /// </summary>
  public class Mrq : IDisposable, IEnumerable<MrqEntry>
  {
    /// <summary>
    /// Remove all deleted entries and shrink all over-sized entries. 
    /// Actually we're just reading the file, deleting it, and writing it down again.
    /// </summary>
    /// <param name="Path"></param>
    public static void DefragmentFile(string Path)
    {
      var toWrite = GetDictionary(Path).Values.Where((p) => !p.IsMarkedDeleted);
      File.Delete(Path);
      using (var file = new Mrq(Path))//creates a new file. 
        foreach (var entry in toWrite)
          file.AppendEntry(entry);//yes, we will increment the count frequently. but i want my code to be A S T H E T I C
    }

    /// <summary>
    /// <3
    /// </summary>
    /// <param name="argssquarebracket1closingsquarebracket">aka "input file"</param>
    /// <returns></returns>
    public static MrqEntry DoYourThingPlease(string ArgsSquareBracket1ClosingSquareBracket)
    {
      string expectedMrqPath = Directory.GetFiles(Path.GetDirectoryName(ArgsSquareBracket1ClosingSquareBracket)).Where((s) => Path.GetExtension(s).Equals("mrq")).First();
      using (var file = new Mrq(expectedMrqPath))
      {
        var fname = Path.GetFileName(ArgsSquareBracket1ClosingSquareBracket);
        file.SeekUntilFileNameIs(fname);
        var entry = file.GetCurrentEntry();
        entry.FileName = fname;
        return entry;
      }
    }
    /// <summary>
    /// Process the entire file at once, and converts it into a dictionary. You're welcome.
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    public static Dictionary<string, MrqEntry> GetDictionary(string Path)
    {
      using (var file = new Mrq(Path))
        return file.ToDictionary();
    }

    public const int MRQ_VERSION = 1;

    private BinaryReader Reader;
    private BinaryWriter Writer;

    private FileStream Stream;

    /// <summary>
    /// The state this object can have. No, we read nentry and version when you create the object. it's O(1) dude.
    /// </summary>
    public enum MRQStreamState { BEGINNING_OF_ENTRY, AFTER_FILE_NAME, BEGINNING_OF_F0 }

    /// <summary>
    /// 
    /// </summary>
    public MRQStreamState State { get; private set; }

    /// <summary>
    /// The File's version.
    /// </summary>
    public int Version
    {
      get { return version; }//4 is where this number is located.
      set { PreserveStreamPositionWhileDoing(() => { Stream.Position = 4; Writer.Write(value); }); }
    }
    private int version;
    /// <summary>
    /// How many entries are inside.
    /// </summary>
    public int Count
    {
      get { return count; }//8 is where this number is located.
      set { PreserveStreamPositionWhileDoing(() => { Stream.Position = 8; Writer.Write(value); }); }
    }
    private int count;

    /// <summary>
    /// Opens the file for reading and writing. 
    /// </summary>
    /// <param name="Path"></param>
    public Mrq(string Path)
    {
      if (!File.Exists(Path))//make a new one.
      {
        using (BinaryWriter bw = new BinaryWriter(new FileStream(Path, FileMode.CreateNew), Encoding.Unicode))
        {
          bw.Write(new byte[] { 109, 114, 113, 32 }, 0, 4); //writes "mrq " in ASCII I think... too lazy to take the shortcut
          bw.Write(MRQ_VERSION);
          bw.Write(0);//starts with 0 entries.
        }
      }

      this.Stream = new FileStream(Path, FileMode.Open, FileAccess.ReadWrite);
      Reader = new BinaryReader(Stream, Encoding.Unicode);
      Writer = new BinaryWriter(Stream, Encoding.Unicode);//this is kinda important due to mrqs are in UTF-16.
      Stream.Position = 4;//skip the starting "mrq ".
      version = Reader.ReadInt32();
      count = Reader.ReadInt32();//note we did not use mutators because it's initialization.

      //now we're at the beginning of the first entry.
      State = MRQStreamState.BEGINNING_OF_ENTRY;
    }

    /// <summary>
    /// Reset the position of the pointer.
    /// </summary>
    public void SeekToBeginning()
    {
      Stream.Position = 12;//"mrq " + version + count is 12 bytes total.
      State = MRQStreamState.BEGINNING_OF_ENTRY;
    }

    /// <summary>
    /// This preserves the position the stream have before invoking the action and restores it afterwards. At the cost of one seek of course.
    /// </summary>
    /// <param name="Action"></param>
    public void PreserveStreamPositionWhileDoing(Action Action)
    {
      long t = Stream.Position;
      Action.Invoke();
      Stream.Position = t;
    }

    /// <summary>
    /// Requires: State == MRQStreamState.BEGINNING_OF_ENTRY
    /// Returns: the name of current entry.
    /// Side Effect: move the pointer over the file name section.
    /// </summary>
    /// <returns></returns>
    public string GetFileName()
    {
      ThrowExceptionIfNotInState(MRQStreamState.BEGINNING_OF_ENTRY);
      int len = Reader.ReadInt32();//we're changing the state, hence two lines not one.
      State = MRQStreamState.AFTER_FILE_NAME;
      return new string(Reader.ReadChars(len));
    }

    /// <summary>
    /// Requires: State == MRQStreamState.BEGINNING_OF_ENTRY
    /// </summary>
    public void SkipFileName()
    {
      ThrowExceptionIfNotInState(MRQStreamState.BEGINNING_OF_ENTRY);
      int len = Reader.ReadInt32();
      Stream.Position += len * 2;//TODO: this assumes all chars have 2 bytes which seems to be not the case.
      State = MRQStreamState.AFTER_FILE_NAME;
    }

    /// <summary>
    /// Helper. Reduces redundant code.
    /// </summary>
    private void EnsureAfterFileNameState()
    {
      if (State == MRQStreamState.BEGINNING_OF_ENTRY)
        SkipFileName();

      ThrowExceptionIfNotInState(MRQStreamState.AFTER_FILE_NAME);
    }
    /// <summary>
    /// Helper. Reduces redundant code.
    /// </summary>
    private void ThrowExceptionIfNotInState(MRQStreamState State)
    {
      if (this.State != State)
        throw new InvalidOperationException("Invalid State:" + this.State);
    }

    /// <summary>
    /// Requires: the pointer did not past nf0.
    /// Moves the pointer to the beginning of the next entry.
    /// </summary>
    public void SkipCurrentEntry()
    {
      EnsureAfterFileNameState();

      int size = Reader.ReadInt32();
      Stream.Position += size;//convenient!
      State = MRQStreamState.BEGINNING_OF_ENTRY;
    }

    /// <summary>
    /// Reads size, f0, fs, nhop and place the state at BEGINNING_OF_F0.
    /// </summary>
    /// <param name="temp">if provided with null, a new object is returned. otherwise modify this object. I know this is terrible practice hence it's private.</param>
    /// <returns></returns>
    private MrqEntry GetEntryInfo(MrqEntry temp)
    {
      ThrowExceptionIfNotInState(MRQStreamState.AFTER_FILE_NAME);

      if (temp == null)
        temp = new MrqEntry();

      temp.Size = Reader.ReadInt32();
      temp.F0 = new float[Reader.ReadInt32()];
      temp.SamplingRate = Reader.ReadInt32();
      temp.NHop = Reader.ReadInt32();

      State = MRQStreamState.BEGINNING_OF_F0;
      return temp;
    }

    /// <summary>
    /// returns a MrqEntry with filename if possible. if you already know the name, do SkipFileName().
    /// </summary>
    /// <returns></returns>
    public MrqEntry GetCurrentEntry()
    {
      MrqEntry res = new MrqEntry();
      if (State == MRQStreamState.BEGINNING_OF_ENTRY)
        res.FileName = GetFileName();//otherwise it will be the default: null.

      ThrowExceptionIfNotInState(MRQStreamState.AFTER_FILE_NAME);

      GetEntryInfo(res);

      for (int i = 0; i < res.F0.Length; i++)
        res.F0[i] = Reader.ReadSingle();

      int excessSpace = res.Size - res.NeededSize;//This happens when you override a deleted entry.
      if (excessSpace > 0)//don't want to do an operation if it's not needed even though i know it's probably handled on the other side.
        Stream.Position += excessSpace;

      State = MRQStreamState.BEGINNING_OF_ENTRY;
      return res;
    }

    /// <summary>
    /// I hope this name is self-explanatory.
    /// </summary>
    /// <param name="FileName"></param>
    public void SeekUntilFileNameIs(string FileName)
    {
      while (!GetFileName().Equals(FileName)) SkipCurrentEntry();
    }

    /// <summary>
    /// Returns nf0, that is, how many numbers are in f0, which is again, possible because of the "deleted entry" idea.
    /// Feel free to ignore the return value.
    /// </summary>
    /// <returns></returns>
    public int SkipToF0()
    {
      EnsureAfterFileNameState();

      Stream.Position += 4;
      int nf0 = Reader.ReadInt32();
      State = MRQStreamState.BEGINNING_OF_F0;
      return nf0;
    }

    /// <summary>
    /// Returns whether calling OverrideCurrentEntry(MrqEntry) will ABSOLUTELY corrupt the file.
    /// </summary>
    /// <param name="With"></param>
    /// <returns></returns>
    public bool CanOverrideCurrentEntry(MrqEntry With)
    {
      int plotSize = -1;//what could go wrong?
      PreserveStreamPositionWhileDoing(() =>
      {
        EnsureAfterFileNameState();

        plotSize = Reader.ReadInt32();
      });
      return plotSize > With.NeededSize;
    }

    /// <summary>
    /// For efficency, this does not check whether the filenames are the same -- that also means you don't have to make them the same! also means you have a real chance to corrupt the file here, yay~
    /// If you dont know whether it will fit, use CanOverrideCurrentEntry(MrqEntry) to find out. but i bet you know.
    /// This overrides everything else. It will check nf0 and size and throw exceptions if the new entry does not fit.
    /// </summary>
    /// <param name="With"></param>
    public void OverrideCurrentEntry(MrqEntry With)
    {
      EnsureAfterFileNameState();
      int plotSize = Reader.ReadInt32();
      Writer.Write(With.Nf0);
      Writer.Write(With.SamplingRate);
      Writer.Write(With.NHop);
      foreach (float f in With.F0)
        Writer.Write(f);

    }

    /// <summary>
    /// We recommend delete/append -> defragment once in a while if you don't know what you're doing. I mean who writes f0 every millisecond for fun?
    /// </summary>
    /// <param name="Entry"></param>
    public void AppendEntry(MrqEntry Entry)
    {
      Stream.Position = Stream.Length;//seek to end of file.
      Writer.Write(Entry.FileName.Length);
      Writer.Write(Entry.FileName);//TODO: again we can have a character of 4 bytes here and screws things up.
      Writer.Write(Entry.NeededSize);//this could be a new entry! Hence ignore old size.
      Writer.Write(Entry.Nf0);
      Writer.Write(Entry.Fs);
      Writer.Write(Entry.NHop);
      foreach (float f in Entry.F0)
        Writer.Write(f);

      this.Count++;
    }

    /// <summary>
    /// Requires: State == MRQStreamState.BEGINNING_OF_ENTRY
    /// Change the first char of the filename of current entry to '\0', marking it deleted.
    /// </summary>
    public void MarkCurrentEntryDeleted()
    {
      ThrowExceptionIfNotInState(MRQStreamState.BEGINNING_OF_ENTRY);

      Stream.Position += 4;
      Writer.Write('\0');
      Stream.Position -= 6;//4+2 for the char. we did this because there's enough potential problem with 4-byte chars already.

      SkipCurrentEntry();
    }

    public void Dispose()
    {
      ((IDisposable)Stream).Dispose();
    }

    /// <summary>
    /// A customary method that does the right thing.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, MrqEntry> ToDictionary()
    {
      return this.ToDictionary((e) => e.FileName);
    }

    IEnumerator<MrqEntry> IEnumerable<MrqEntry>.GetEnumerator()
    {
      while (Stream.Position != Stream.Length)
        yield return GetCurrentEntry();
    }

    public IEnumerator GetEnumerator()
    {
      return GetEnumerator();
    }
  }

}
