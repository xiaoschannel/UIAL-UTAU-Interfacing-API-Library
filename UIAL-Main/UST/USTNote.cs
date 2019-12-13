using zuoanqh.libzut;
using zuoanqh.libzut.Data;
using System.Collections.Generic;
using System.Linq;
using System;

namespace zuoanqh.UIAL.UST
{
  /// <summary>
  /// This class models a note inside a ust. 
  /// Attributes "Envelope", "Portamento" and "Vibrato" are handled by separate, mutable classes due to their complexity.
  /// You can access their original text from here, or parsed data from those objects. They are more reliable(TODO: Find out what 3-years-ago me meant by this).
  /// Converting parsed object back to string won't preserve input.
  /// For flags, "FlagText" preserves input, while "Flags" provides convenience. "Flags"
  /// You can handle it yourselves, or add it to the settings.
  /// TODO: This comment is too long.
  /// TODO: Refactor original texts into respective classes
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
    /// This contains the name of all known attributes a note can have.
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
    /// Create a rest ("R") note.
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static USTNote MakeR(int length)
    { return new USTNote(length, "R", "C3"); }

    /// <summary>
    /// Create a note with default parameters.
    /// This will make a note of length 240, lyric "あ", note name "C3". 
    /// A note need to have at least these three parameters.
    /// </summary>
    /// <returns></returns>
    public static USTNote MakeDefault()
    {
      return new USTNote(240, "あ", "C3");
    }

    //for debugging
    //public List<string> TextRaw;

    /// <summary>
    /// Access the raw text of each attribute with their key.
    /// Please note you cannot access envelope, portamento or vibrato of the note here. 
    /// Their raw text is available from their respective attributes.
    /// </summary>
    public IReadOnlyDictionary<string, string> Attributes { get { return attributes; } }
    private DictionaryDataObject attributes;

    /// <summary>
    /// Mandatory. Ticks. Must >= 15.
    /// </summary>
    public int Length
    {
      get { return attributes.GetAsInt(KEY_LENGTH); }
      set { attributes.Set(KEY_PREUTTERANCE, value); } // FIXME
    }

    /// <summary>
    /// Mandatory. "R" means rest. Lyric combined with postfix should be in the oto-aliases.
    /// </summary>
    public string Lyric
    {
      get { return attributes[KEY_LYRIC]; }
      set { attributes.Set(KEY_PREUTTERANCE, value); } // FIXME
    }
    public bool IsRest()
    { return Lyric.Equals("R"); }

    /// <summary>
    /// Mandatory. Ranges from 24(C1) to 107(B7).
    /// </summary>
    public int NoteNum
    {
      get { return attributes.GetAsInt(KEY_NOTENUM); }
      set { attributes.Set(KEY_NOTENUM, value); }
    }

    /// <summary>
    /// Default: empty (VB value). In milliseconds. 
    /// This is rounded off to 3 digits after the decimal point by UTAU.
    /// </summary>
    public double PreUtterance
    {
      get { return attributes.GetAsDouble(KEY_PREUTTERANCE); }
      set { attributes.Set(KEY_PREUTTERANCE, value); }
    }

    /// <summary>
    /// For parsed flags, use "Flags" attribute.
    /// </summary>
    public string FlagText
    {
      get { return attributes[KEY_FLAGS]; }
      set { attributes.Set(KEY_FLAGS, value); }
    }

    /// <summary>
    /// Immutable.
    /// </summary>
    public Flags Flags
    {
      get { return new Flags(FlagText); }
      set { FlagText = value.FlagText; }
    }

    /// <summary>
    /// Percentage. This is rounded to an integer by UTAU.
    /// </summary>
    public int Intensity
    {
      get { return attributes.GetAsInt(KEY_INTENSITY); }
      set { attributes.Set(KEY_INTENSITY, value); }
    }
    /// <summary>
    /// Percentage. This is rounded to an integer by UTAU.
    /// </summary>
    public int Modulation
    {
      get { return attributes.GetAsInt(KEY_MODULATION); }
      set { attributes.Set(KEY_MODULATION, value); }
    }

    /// <summary>
    /// In milliseconds. This is rounded off to 3 digits after decimal point by UTAU. 
    /// </summary>
    public double VoiceOverlap
    {
      get { return attributes.GetAsDouble(KEY_VOICEOVERLAP); }
      set { attributes.Set(KEY_VOICEOVERLAP, value); }
    }

    /// <summary>
    /// Must be between 0 and 200. 
    /// </summary>
    public double Velocity
    {
      get { return attributes.GetAsDouble(KEY_VELOCITY); }
      set { attributes.Set(KEY_VELOCITY, value); }
    }
    /// <summary>
    /// Manipulate the velocity with the effective factor instead. 
    /// This should be between 0.5 (half as long, fastest) to 2 (twice as long, slowest)
    /// </summary>
    public double VelocityFactor
    {
      get { return CommonReferences.GetEffectiveVelocityFactor(Velocity); }
      set { Velocity = CommonReferences.GetVelocity(value); }
    }

    /// <summary>
    /// TODO: Compatible with what?
    /// This exists for compatibility, it converts the envelope back and forth to strings when you use it.
    /// </summary>
    public string EnvelopeText
    {
      get { return Envelope.ToString(); }
      set { Envelope = new Envelope(value); }
    }

    /// <summary>
    /// Default is "0,5,35,0,100,100,0,%", and can be created by "new Envelope()"
    /// </summary>
    public Envelope Envelope;

    /// <summary>
    /// Can be null. Null means the attribute does not exist.
    /// </summary>
    public Portamento Portamento;

    /// <summary>
    /// Can be null. Null means the attribute does not exist.
    /// </summary>
    public Vibrato Vibrato;

    // These exist in older usts. Parsing is to be implemented.
    ///// <summary>
    ///// This field is deprecated. Pitchbend type perhaps?
    ///// </summary>
    //public int PBType;
    //
    ///// <summary>
    ///// This is for mode 1 pitchbend.
    ///// </summary>
    //public List<int> Pitches;

    /// <summary>
    /// Create a note from text in the format of ust files.
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
    /// Create a note with minimum data and a default envelope.
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
    /// Create a note with minimum data and a default envelope, with note name (e.g. "C1") specified instead of "NoteNum" (e.g. 24).
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
      //TODO: implement this more efficiently
      //this.attributes = new DictionaryDataObject(another.attributes);
      //this.Envelope = new Envelope(another.Envelope);
      //if (another.Portamento != null) this.Portamento = new Portamento(another.Portamento);
      //if (another.Vibrato != null) this.Vibrato = new Vibrato(another.Vibrato);
    }

    /// <summary>
    /// Converts the note back to its ust format. 
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
    /// Converts the note back to its ust format.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
      return String.Join("\r\n", ToStringList().ToArray()) + "\r\n";
    }
  }
}
