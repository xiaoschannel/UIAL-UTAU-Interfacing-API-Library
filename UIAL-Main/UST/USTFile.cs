using zuoanqh.libzut;
using zuoanqh.libzut.Data;
using zuoanqh.libzut.FileIO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// This class models a UST file.
  /// Note: 
  /// %HOME% means the current user's home directory.
  /// %VOICE% is the voicebank directory set in tools->option->path.
  /// if the directory is not absolute, it's the directory relative to UTAU's install directory.
  /// Also, FUCK SHIFT-JIS
  /// </summary>
  public class USTFile
  {
    public const string KEY_PROJECT_NAME = "ProjectName";
    public const string KEY_TEMPO = "Tempo";
    public const string KEY_VOICE_DIR = "VoiceDir";
    public const string KEY_OUT_FILE = "OutFile";
    public const string KEY_CACHE_DIR = "CacheDir";
    /// <summary>
    /// Wavtool, or "append"
    /// </summary>
    public const string KEY_TOOL1 = "Tool1";
    /// <summary>
    /// Resampler, or "resample"
    /// </summary>
    public const string KEY_TOOL2 = "Tool2";
    public const string KEY_MODE2 = "Mode2";

    /// <summary>
    /// Whatever is in [#VERSION] section. 
    /// </summary>
    public string Version;

    /// <summary>
    /// Or BPM.
    /// </summary>
    public double Tempo { get { return ProjectInfo.GetAsDouble(KEY_TEMPO); } set { ProjectInfo.Set(KEY_TEMPO, value); } }
    /// <summary>
    /// Note this is useless on windows. we do however, provide a method converting multi-track files to multiple usts.
    /// </summary>
    //public int Tracks;
    public string ProjectName { get { return ProjectInfo.Get(KEY_PROJECT_NAME); } set { ProjectInfo.Set(KEY_PROJECT_NAME, value); } }
    /// <summary>
    /// See class comment for directory meaning.
    /// </summary>
    public string VoiceDir { get { return ProjectInfo.Get(KEY_VOICE_DIR); } set { ProjectInfo.Set(KEY_VOICE_DIR, value); } }
    /// <summary>
    /// See class comment for directory meaning.
    /// </summary>
    public string OutFile { get { return ProjectInfo.Get(KEY_OUT_FILE); } set { ProjectInfo.Set(KEY_OUT_FILE, value); } }
    /// <summary>
    /// See class comment for directory meaning.
    /// </summary>
    public string CacheDir { get { return ProjectInfo.Get(KEY_CACHE_DIR); } set { ProjectInfo.Set(KEY_CACHE_DIR, value); } }
    /// <summary>
    /// The wavtool used.
    /// </summary>
    public string Tool1 { get { return ProjectInfo.Get(KEY_TOOL1); } set { ProjectInfo.Set(KEY_TOOL1, value); } }
    /// <summary>
    /// The sampler used.
    /// </summary>
    public string Tool2 { get { return ProjectInfo.Get(KEY_TOOL2); } set { ProjectInfo.Set(KEY_TOOL2, value); } }
    /// <summary>
    /// Whether the project is in edit mode 2.
    /// <para />You probably want this to be true since mode 2 is the newer edit mode for UTAU. 
    /// </summary>
    public bool Mode2 { get { return ProjectInfo.GetAsBoolean(KEY_MODE2); } set { ProjectInfo.Set(KEY_MODE2, value); } }



    /// <summary>
    /// Yes, you can have more than 1 tracks. if you would like to pretend there's only one, use "Notes"
    /// </summary>
    public List<List<USTNote>> TrackData;
    /// <summary>
    /// This is a shortcut that gives the first track. 
    /// </summary>
    public List<USTNote> Notes
    {
      get { return TrackData[0]; }
      set
      {
        if (TrackData == null)
          TrackData = new List<List<USTNote>>();
        TrackData[0] = value;
      }
    }

    public DictionaryDataObject ProjectInfo;

    /// <summary>
    /// Creates a ust from (absolute or relative) path given.
    /// Note: Emperically, we found 8kb to be as good as whole file.
    /// </summary>
    /// <param name="fPath"></param>
    public USTFile(string fPath) //
      : this(ByLineFileIO.ReadFileNoWhitespace(fPath, zuio.GetEncUde(fPath,8192, Encoding.GetEncoding("Shift_JIS"))).ToArray())
    { }

    /// <summary>
    /// Creates a ust from string array.
    /// </summary>
    /// <param name="data"></param>
    public USTFile(string[] data)
    {
      TrackData = new List<List<USTNote>>();

      //split by tracks first. yes, that exists.
      //thank you c# for providing @.. escaping more escape character is just...
      List<List<string>> tracks = zusp.ListSplit(data.ToList(), @"\[#TRACKEND\]");
      List<List<string>> ls = zusp.ListSplit(tracks[0], @"\[#.*\]");

      //ls[0] is ust version, we only handled the newest.
      Version = ls[0][0];
      //ls[1] is other project info
      ProjectInfo = new DictionaryDataObject(zusp.ListToDictionary(ls[1], "="));

      TrackData.Add(new List<USTNote>());
      //rest of it is notes of this ust.
      for (int i = 2; i < ls.Count; i++)
      {
        int notenum = i - 2; //LI:when i is 2, it's note 0
        TrackData[0].Add(new USTNote(ls[i]));
      }
      if (tracks.Count > 1)//yes there can be more than 1 tracks. not on windows versions though!
        foreach (var t in tracks.Skip(1))//much better code after those special cases are gone now...
          TrackData.Add(zusp.ListSplit(t, @"\[*\]").Select((n) => new USTNote(n)).ToList());

      //now we need to fix portamentos if any.
      foreach (var track in TrackData)
        for (int i = 1; i < track.Count; i++)//sliding window of i-1, i
          if (track[i].Portamento != null && !track[i].Portamento.HasValidPBS1())//note this is [i-1] - [i] because it's relative to [i]
            track[i].Portamento.PBS[1] = track[i - 1].NoteNum - track[i].NoteNum;
    }

    /// <summary>
    /// This is sort of a copy constructor. Yes, this will try to make deep copies of everything.
    /// </summary>
    /// <param name="Version"></param>
    /// <param name="ProjectInfo"></param>
    /// <param name="TrackData"></param>
    public USTFile(string Version, IDictionary<string, string> ProjectInfo, List<List<USTNote>> TrackData)
    {
      this.Version = Version;
      this.ProjectInfo = new DictionaryDataObject(ProjectInfo);
      this.TrackData = new List<List<USTNote>>();
      foreach (var t in TrackData)
      {
        var myTrack = new List<USTNote>();
        foreach (var n in t)
          myTrack.Add(new USTNote(n));
        this.TrackData.Add(myTrack);
      }
    }

    /// <summary>
    /// Cheap trick to save code. or did i.
    /// </summary>
    /// <param name="Notes"></param>
    /// <returns></returns>
    private static List<List<USTNote>> MakeTrackData(List<USTNote> Notes)
    {
      var l = new List<List<USTNote>> { Notes };
      return l;
    }

    public USTFile(string Version, IDictionary<string, string> ProjectInfo, List<USTNote> Notes)
      : this(Version, ProjectInfo, MakeTrackData(Notes))
    { }

    public USTFile(USTFile that)
      : this(that.Version, that.ProjectInfo, that.TrackData)
    { }

    /// <summary>
    /// Converts it back to its ust format. 
    /// </summary>
    /// <returns></returns>
    public List<string> ToStringList()
    {
      var ans = new List<string> { "[#VERSION]", Version, "[#SETTING]" };
      ans.AddRange(ProjectInfo.ToStringList("="));

      foreach (var Notes in TrackData)//adding notes for each track.
      {
        for (int i = 0; i < Notes.Count; i++)
        {
          USTNote n = Notes[i];
          string s = "" + i;
          while (s.Length < 4) s = "0" + s;
          ans.Add("[#" + s + "]");
          foreach (var l in n.ToStringList())
            ans.Add(l);
        }
        ans.Add("[#TRACKEND]");
      }
      return ans;
    }

    /// <summary>
    /// Converts it back to its ust format. 
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return zusp.List("\r\n", ToStringList().ToArray()) + "\r\n";
    }
  }
}
