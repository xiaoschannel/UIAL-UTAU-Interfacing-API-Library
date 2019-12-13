using zuoanqh.libzut;
using zuoanqh.libzut.FileIO;
using System.Collections.Generic;
using System.Linq;
using System;

namespace zuoanqh.UIAL.VB
{
  /// <summary>
  /// Model for prefix.map file.
  /// Note despite it is called prefix, it is applied as a suffix.
  /// TODO: what is note index rank?
  /// </summary>
  public class PrefixMap
  {
    /// <summary>
    /// Note name to prefix.
    /// </summary>
    public Dictionary<string, string> Map;

    public PrefixMap(string fPath):this(ByLineFileIO.ReadFileNoWhitespace(fPath, zuio.GetEncUde(fPath)).ToArray())
    { }

    /// <summary>
    /// Create an object with given data. Checks if all note names are present.
    /// </summary>
    /// <param name="Data">Lines in the file. For windows store compatibility.</param>
    public PrefixMap(string[] Data)
    {
      Map = new Dictionary<string, string>();
      var t = Data.Select((s)=>zusp.Split(s,"\t\t"));
      foreach (var p in t) Map.Add(p[0], p[1]);
    }


    /// <summary>
    /// Set everything in the given range to given mapping, both inclusive.
    /// </summary>
    /// <param name="From">Note name</param>
    /// <param name="To">Note name</param>
    /// <param name="Mapping"></param>
    public void SetRange(string From, string To, string Mapping)
    {
      SetRange(CommonReferences.NOTENAME_INDEX_RANK[From], CommonReferences.NOTENAME_INDEX_RANK[To], Mapping);
    }
    /// <summary>
    /// Set everything in given range to given mapping, both inclusive.
    /// </summary>
    /// <param name="From">Note index</param>
    /// <param name="To">Note index</param>
    /// <param name="Mapping"></param>
    private void SetRange(int From, int To, string Mapping)
    {
      if (From > To) { From ^= To; To ^= From; From ^= To; }

      for (int i = From; i <= To; i++) Map[CommonReferences.NOTENAMES[i]] = Mapping;
    }

    public override string ToString()
    {
      return String.Join("\r\n", CommonReferences.NOTENAMES.Reverse().Select(s => s + "\t\t" + Map[s])) + "\r\n";
    }
  }
}
