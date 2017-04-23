using zuoanqh.libzut;
using zuoanqh.libzut.Data;
using System.Collections.Generic;
using System.Linq;

namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// This class models a note inside a ust. When it comes to ust, we would like to say (again) FUCK SHIFT-JIS. 
  /// Please note attribute "Envelope", "Portamento" and "Vibrato" was handled by separate, mutable classes.
  /// You can access their text from here or parsed data from those objects, they do not preserve input, but are more reliable.
  /// "FlagText" preserves input, while "Flags" works perfectly with all known flags, all unknown flags with parameters,
  /// but will have problem with unknown no-parameter flags when they are next to another unknown flag because the grammar itself is fucked up. 
  /// You can handle it yourselves, or add it to the settings.
  /// </summary>
  public class USTNote
  {
    public const string KEY_LENGTH = "Length";
    public const string KEY_LYRIC = "Lyric";
    public const string KEY_NOTENUM = "NoteNum";
    public const string KEY_LABEL = "Label";

    public const string KEY_PREUTTERANCE = "PreUtterance";
    public const string KEY_VOICEOVERLAP = "VoiceOverlap";
    public const string KEY_STARTPOINT = "StartPoint";
    public const string KEY_VELOCITY = "Velocity";
    public const string KEY_INTENSITY = "Intensity";
    public const string KEY_MODULATION = "Modulation";
    public const string KEY_TEMPO = "Tempo";

    public const string KEY_ENVELOPE = "Envelope";
    public const string KEY_FLAGS = "Flags";

    public const string KEY_PBS = "PBS";
    public const string KEY_PBW = "PBW";
    public const string KEY_PBY = "PBY";
    public const string KEY_PBM = "PBM";
    public const string KEY_VBR = "VBR";

    /// <summary>
    /// This contains all attribute we know could exist in a note.
    /// </summary>
    public static readonly IReadOnlyList<string> KNOWN_ATTRIBUTE_NAMES;

    static USTNote()
    {
      KNOWN_ATTRIBUTE_NAMES = new string[] { KEY_LENGTH, KEY_LYRIC, KEY_NOTENUM,
        KEY_LABEL, KEY_PREUTTERANCE, KEY_VOICEOVERLAP, KEY_STARTPOINT,
        KEY_VELOCITY, KEY_INTENSITY, KEY_MODULATION, KEY_TEMPO, KEY_ENVELOPE,
        KEY_FLAGS, KEY_PBS, KEY_PBW, KEY_PBY, KEY_PBM, KEY_VBR }.ToList();
    }

    /// <summary>
    /// Make a rest note.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static USTNote MakeR(int length)
    { return new USTNote(length, "R", "C3"); }

    /// <summary>
    /// Make a note without specifying all the stupid parameters. This will make 240, "あ", "C3". 
    /// We does not have an empty constructor because it is required a note have that three fields.
    /// to preserve encapsulation we can't make an "empty" object.
    /// </summary>
    /// <returns></returns>
    public static USTNote MakeDefault()
    {
      return new USTNote(240, "あ", "C3");
    }

    //This was for debugging. we debugged. it works fine.
    //public List<string> TextRaw;

    /// <summary>
    /// Or you can access your attributes this way i guess.... not judging...
    /// Please note you cannot access envelope, portamento or vibrato of the note here. 
    /// If for compatibility reasons you prefer their text format, we have attributes for you.
    /// 
    /// </summary>
    public IReadOnlyDictionary<string, string> Attributes { get { return attributes; } }

    private DictionaryDataObject attributes;

    /// <summary>
    /// Must-have attribute. Ticks. Must >= 15.
    /// </summary>
    public int Length
    {
      get { return attributes.GetAsInt(KEY_LENGTH); }
      set { attributes.Set(KEY_PREUTTERANCE, value); }
    }

    /// <summary>
    /// Must-have (Duh). "R" triple-equals rest. Lyric combined with postfix should be in the oto-aliases.
    /// </summary>
    public string Lyric
    {
      get { return attributes[KEY_LYRIC]; }
      set { attributes.Set(KEY_PREUTTERANCE, value); }
    }
    public bool IsRest()
    { return Lyric.Equals("R"); }

    /// <summary>
    /// This ranges from 24(C1) to 107(B7). It's probably the god-damned mathematical convenience working up again.
    /// </summary>
    public int NoteNum
    {
      get { return attributes.GetAsInt(KEY_NOTENUM); }
      set { attributes.Set(KEY_NOTENUM, value); }
    }

    /// <summary>
    /// Default: empty (VB value). In milliseconds. This will be rounded off to 3 digits after decimal point by UTAU. 
    /// </summary>
    public double PreUtterance
    {
      get { return attributes.GetAsDouble(KEY_PREUTTERANCE); }
      set { attributes.Set(KEY_PREUTTERANCE, value); }
    }

    /// <summary>
    /// For a modern user experience, use "Flags" attribute. This is for when things broke.
    /// </summary>
    public string FlagText
    {
      get { return attributes[KEY_FLAGS]; }
      set { attributes.Set(KEY_FLAGS, value); }
    }

    /// <summary>
    /// Note Flags class is immutable. to apply (add or update) a flag value, use Flags = Flags.WithFlagValue()
    /// Yes, this is incredibly inefficient and goes text to flags to text to flags to text to flags for one edit. but it works. what more do you ask?
    /// </summary>
    public Flags Flags
    {
      get { return new Flags(FlagText); }
      set { FlagText = value.FlagText; }
    }

    /// <summary>
    /// Percentage. This will be rounded to an integer by UTAU.
    /// </summary>
    public int Intensity
    {
      get { return attributes.GetAsInt(KEY_INTENSITY); }
      set { attributes.Set(KEY_INTENSITY, value); }
    }
    /// <summary>
    /// Percentage. This will be rounded to an integer by UTAU.
    /// </summary>
    public int Modulation
    {
      get { return attributes.GetAsInt(KEY_MODULATION); }
      set { attributes.Set(KEY_MODULATION, value); }
    }

    /// <summary>
    /// In milliseconds. This will be rounded off to 3 digits after decimal point by UTAU. 
    /// </summary>
    public double VoiceOverlap
    {
      get { return attributes.GetAsDouble(KEY_VOICEOVERLAP); }
      set { attributes.Set(KEY_VOICEOVERLAP, value); }
    }

    /// <summary>
    /// This should be between 0 and 200. 
    /// </summary>
    public double Velocity
    {
      get { return attributes.GetAsDouble(KEY_VELOCITY); }
      set { attributes.Set(KEY_VELOCITY, value); }
    }
    /// <summary>
    /// This allow you to manipulate the velocity with desired factor value instead. This should be between 0.5 (half as long, fastest) to 2 (twice as long, slowest)
    /// </summary>
    public double VelocityFactor
    {
      get { return CommonReferences.GetEffectiveVelocityFactor(Velocity); }
      set { Velocity = CommonReferences.GetVelocity(value); }
    }

    /// <summary>
    /// This exists for compatibility, it converts the envelope back and forth to strings when you use it.
    /// </summary>
    public string EnvelopeText
    {
      get { return Envelope.ToString(); }
      set { Envelope = new Envelope(value); }
    }

    /// <summary>
    /// The envelope of the note. default is "0,5,35,0,100,100,0,%", or new Envelope()
    /// </summary>
    public Envelope Envelope;

    /// <summary>
    /// To access the strings, 
    /// </summary>
    public Portamento Portamento;

    /// <summary>
    /// This can be null, sorry. null means the attribute does not exist.
    /// </summary>
    public Vibrato Vibrato;

    //you know what, Maybe I'll just not do these, you can still use attributes.Get.
    ///// <summary>
    ///// This field is deprecated, we did not bother to find out what it means.
    ///// </summary>
    //public int PBType;
    //
    ///// <summary>
    ///// This is for mode 1. (that means you may ignore it) (that means please do ignore it)
    ///// </summary>
    //public List<int> Pitches;

    /// <summary>
    /// Create the note from raw text in format of ust files.
    /// </summary>
    /// <param name="list"></param>
    public USTNote(List<string> list)
    {
      //this.TextRaw = list;
      this.attributes = new DictionaryDataObject(zusp.ListToDictionary(list, "="));

      this.Envelope = attributes.ContainsKey(KEY_ENVELOPE) ? new Envelope(attributes[KEY_ENVELOPE]):new Envelope();
      attributes.Remove(KEY_ENVELOPE);

      if (!attributes.ContainsKey(KEY_PBW))
        this.Portamento = null;
      else
      {
        string pbw = attributes[KEY_PBW];
        string pbs = attributes.ContainsKey(KEY_PBS) ? attributes[KEY_PBS] : "0;";//0 and invalid. 
        string pby = attributes.ContainsKey(KEY_PBY) ? attributes[KEY_PBY] : "";//pby and pbm will be fixed by Portamento upon construction. 
        string pbm = attributes.ContainsKey(KEY_PBM) ? attributes[KEY_PBM] : "";
        this.Portamento = new Portamento(pbw, pbs, pby, pbm);
      }

      this.Vibrato = attributes.ContainsKey(KEY_VBR) ?  new Vibrato(attributes[KEY_VBR]): null ;
      attributes.Remove(KEY_VBR);
    }
    /// <summary>
    /// make a note with minimum data and default envelope.
    /// </summary>
    /// <param name="Length"></param>
    /// <param name="Lyric"></param>
    /// <param name="NoteNum"></param>
    public USTNote(int Length, string Lyric, int NoteNum)
    {
      this.attributes = new DictionaryDataObject();
      this.Length = Length;
      this.Lyric = Lyric;
      this.NoteNum = NoteNum;
      this.Envelope = new Envelope();
    }

    /// <summary>
    /// This will convert the NoteName to NoteNum.
    /// </summary>
    /// <param name="Length"></param>
    /// <param name="Lyric"></param>
    /// <param name="NoteName"></param>
    public USTNote(int Length, string Lyric, string NoteName)
          : this(Length, Lyric, CommonReferences.NOTENAME_INDEX_UST[NoteName])
    { }

    /// <summary>
    /// Deep copy constructor.
    /// </summary>
    /// <param name="another"></param>
    public USTNote(USTNote another)
      : this(another.ToStringList())
    {
      //giving up....
      //this.attributes = new DictionaryDataObject(another.attributes);
      //this.Envelope = new Envelope(another.Envelope);
      //if (another.Portamento != null) this.Portamento = new Portamento(another.Portamento);
      //if (another.Vibrato != null) this.Vibrato = new Vibrato(another.Vibrato);
    }

    /// <summary>
    /// Converts it back to its ust format. 
    /// </summary>
    /// <returns></returns>
    public List<string> ToStringList()
    {
      var ans = new List<string>();
      ans.AddRange(attributes.ToStringList("="));
      if (Vibrato != null) ans.Add(Vibrato.ToString());
      if (Portamento != null) ans.AddRange(Portamento.ToStringList());
      ans.Add(Envelope.ToString());
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
