using System.Collections.Generic;
using System.Linq;
using System.Text;
using zuoanqh.libzut;
using zuoanqh.libzut.Data;
using zuoanqh.libzut.FileIO;

namespace zuoanqh.UIAL.UST
{
    /// <summary>
    ///     This class models a UST file.
    ///     Note:
    ///     %HOME% means the current user's home directory.
    ///     %VOICE% is the voicebank directory set in tools->option->path.
    ///     if the directory is not absolute, it's the directory relative to UTAU's install directory.
    ///     Also, FUCK SHIFT-JIS
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class USTFile
    {
        public const string KeyProjectName = "ProjectName";
        public const string KeyTempo = "Tempo";
        public const string KeyVoiceDir = "VoiceDir";
        public const string KeyOutFile = "OutFile";
        public const string KeyCacheDir = "CacheDir";

        /// <summary>
        ///     Wavtool, or "append"
        /// </summary>
        public const string KeyTool1 = "Tool1";

        /// <summary>
        ///     Resampler, or "resample"
        /// </summary>
        public const string KeyTool2 = "Tool2";

        public const string KeyMode2 = "Mode2";

        /// <summary>
        ///     Creates a ust from (absolute or relative) path given.
        ///     Note: Emperically, we found 8kb to be as good as whole file.
        /// </summary>
        /// <param name="fPath"></param>
        public USTFile(string fPath) //
            : this(ByLineFileIO
                .ReadFileNoWhitespace(fPath, zuio.GetEncUde(fPath, 8192, Encoding.GetEncoding("Shift_JIS"))).ToArray())
        {
        }

        /// <summary>
        ///     Creates a ust from string array.
        /// </summary>
        /// <param name="data"></param>
        public USTFile(IEnumerable<string> data)
        {
            TrackData = new List<List<USTNote>>();

            //split by tracks first. yes, that exists.
            //thank you c# for providing @.. escaping more escape character is just...
            var tracks = zusp.ListSplit(data.ToList(), @"\[#TRACKEND\]");
            var ls = zusp.ListSplit(tracks[0], @"\[#.*\]");

            //ls[0] is ust version, we only handled the newest.
            Version = ls[0][0];
            //ls[1] is other project info
            ProjectInfo = new DictionaryDataObject(zusp.ListToDictionary(ls[1], "="));

            TrackData.Add(new List<USTNote>());
            //rest of it is notes of this ust.
            for (var i = 2; i < ls.Count; i++)
            {
                var noteNum = i - 2; //LI:when i is 2, it's note 0
                TrackData[0].Add(new USTNote(ls[i]));
            }

            if (tracks.Count > 1) //yes there can be more than 1 tracks. not on windows versions though!
                foreach (var t in tracks.Skip(1)) //much better code after those special cases are gone now...
                    TrackData.Add(zusp.ListSplit(t, @"\[*\]").Select(n => new USTNote(n)).ToList());

            //now we need to fix portamentos if any.
            foreach (var track in TrackData)
                for (var i = 1; i < track.Count; i++) //sliding window of i-1, i
                    if (track[i].Portamento != null && !track[i].Portamento.HasValidPbs1()
                    ) //note this is [i-1] - [i] because it's relative to [i]
                        track[i].Portamento.Pbs[1] = track[i - 1].NoteNum - track[i].NoteNum;
        }

        /// <summary>
        ///     This is sort of a copy constructor. Yes, this will try to make deep copies of everything.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="projectInfo"></param>
        /// <param name="trackData"></param>
        public USTFile(string version, IDictionary<string, string> projectInfo,
            IEnumerable<IEnumerable<USTNote>> trackData)
        {
            Version = version;
            ProjectInfo = new DictionaryDataObject(projectInfo);
            TrackData = new List<List<USTNote>>();
            foreach (var t in trackData)
            {
                var myTrack = t.Select(n => new USTNote(n)).ToList();
                TrackData.Add(myTrack);
            }
        }

        public USTFile(string version, IDictionary<string, string> projectInfo, IEnumerable<USTNote> notes)
            : this(version, projectInfo, MakeTrackData(notes))
        {
        }

        public USTFile(USTFile that)
            : this(that.Version, that.ProjectInfo, that.TrackData)
        {
        }

        public DictionaryDataObject ProjectInfo { get; set; }

        /// <summary>
        ///     Yes, you can have more than 1 tracks. if you would like to pretend there's only one, use "Notes"
        /// </summary>
        public List<List<USTNote>> TrackData { get; set; }

        /// <summary>
        ///     Whatever is in [#VERSION] section.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        ///     Or BPM.
        /// </summary>
        public double Tempo
        {
            get => ProjectInfo.GetAsDouble(KeyTempo);
            set => ProjectInfo.Set(KeyTempo, value);
        }

        /// <summary>
        ///     Note this is useless on windows. we do however, provide a method converting multi-track files to multiple usts.
        /// </summary>
        //public int Tracks;
        public string ProjectName
        {
            get => ProjectInfo.Get(KeyProjectName);
            set => ProjectInfo.Set(KeyProjectName, value);
        }

        /// <summary>
        ///     See class comment for directory meaning.
        /// </summary>
        public string VoiceDir
        {
            get => ProjectInfo.Get(KeyVoiceDir);
            set => ProjectInfo.Set(KeyVoiceDir, value);
        }

        /// <summary>
        ///     See class comment for directory meaning.
        /// </summary>
        public string OutFile
        {
            get => ProjectInfo.Get(KeyOutFile);
            set => ProjectInfo.Set(KeyOutFile, value);
        }

        /// <summary>
        ///     See class comment for directory meaning.
        /// </summary>
        public string CacheDir
        {
            get => ProjectInfo.Get(KeyCacheDir);
            set => ProjectInfo.Set(KeyCacheDir, value);
        }

        /// <summary>
        ///     The wavtool used.
        /// </summary>
        public string Tool1
        {
            get => ProjectInfo.Get(KeyTool1);
            set => ProjectInfo.Set(KeyTool1, value);
        }

        /// <summary>
        ///     The sampler used.
        /// </summary>
        public string Tool2
        {
            get => ProjectInfo.Get(KeyTool2);
            set => ProjectInfo.Set(KeyTool2, value);
        }

        /// <summary>
        ///     Whether the project is in edit mode 2.
        ///     <para />
        ///     You probably want this to be true since mode 2 is the newer edit mode for UTAU.
        /// </summary>
        public bool Mode2
        {
            get => ProjectInfo.GetAsBoolean(KeyMode2);
            set => ProjectInfo.Set(KeyMode2, value);
        }

        /// <summary>
        ///     This is a shortcut that gives the first track.
        /// </summary>
        public List<USTNote> Notes
        {
            get => TrackData[0];
            set
            {
                if (TrackData == null)
                    TrackData = new List<List<USTNote>>();
                TrackData[0] = value;
            }
        }

        /// <summary>
        ///     Cheap trick to save code. or did i.
        /// </summary>
        /// <param name="notes"></param>
        /// <returns></returns>
        private static IEnumerable<IEnumerable<USTNote>> MakeTrackData(IEnumerable<USTNote> notes)
        {
            var l = new List<IEnumerable<USTNote>> {notes};
            return l;
        }

        /// <summary>
        ///     Converts it back to its ust format.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ToStringList()
        {
            yield return "[#VERSION]";
            yield return Version;
            yield return "[#SETTING]";
            foreach (var x in ProjectInfo.ToStringList("=")) yield return x;

            foreach (var notes in TrackData) //adding notes for each track.
            {
                for (var i = 0; i < notes.Count; i++)
                {
                    var n = notes[i];
                    var s = $"{i}";
                    while (s.Length < 4) s = $"0{s}";
                    yield return $"[#{s}]";
                    foreach (var x in n.ToStringList()) yield return x;
                }

                yield return "[#TRACKEND]";
            }
        }

        /// <summary>
        ///     Converts it back to its ust format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join("\r\n", ToStringList().ToArray()) + "\r\n";
        }
    }
}