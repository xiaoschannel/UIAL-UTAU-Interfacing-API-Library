using System.Collections.Generic;
using System.Linq;
using zuoanqh.libzut.Data;

namespace zuoanqh.UIAL.UST
{
    /// <summary>
    ///     This class models a note inside a ust. When it comes to ust, we would like to say (again) FUCK SHIFT-JIS.
    ///     Please note attribute "Envelope", "Portamento" and "Vibrato" was handled by separate, mutable classes.
    ///     You can access their text from here or parsed data from those objects, they do not preserve input, but are more
    ///     reliable.
    ///     "FlagText" preserves input, while "Flags" works perfectly with all known flags, all unknown flags with parameters,
    ///     but will have problem with unknown no-parameter flags when they are next to another unknown flag because the
    ///     grammar itself is fucked up.
    ///     You can handle it yourselves, or add it to the settings.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class USTNote
    {
        public const string KeyLength = "Length";
        public const string KeyLyric = "Lyric";
        public const string KeyNoteNum = "NoteNum";
        public const string KeyLabel = "Label";

        public const string KeyPreUtterance = "PreUtterance";
        public const string KeyVoiceOverlap = "VoiceOverlap";
        public const string KeyStartPoint = "StartPoint";
        public const string KeyVelocity = "Velocity";
        public const string KeyIntensity = "Intensity";
        public const string KeyModulation = "Modulation";
        public const string KeyTempo = "Tempo";

        public const string KeyEnvelope = "Envelope";
        public const string KeyFlags = "Flags";

        public const string KeyPbs = "PBS";
        public const string KeyPbw = "PBW";
        public const string KeyPby = "PBY";
        public const string KeyPbm = "PBM";
        public const string KeyVbr = "VBR";

        /// <summary>
        ///     This contains all attribute we know could exist in a note.
        /// </summary>
        public static readonly IReadOnlyList<string> KnownAttributeNames = new[]
        {
            KeyLength, KeyLyric, KeyNoteNum,
            KeyLabel, KeyPreUtterance, KeyVoiceOverlap, KeyStartPoint,
            KeyVelocity, KeyIntensity, KeyModulation, KeyTempo, KeyEnvelope,
            KeyFlags, KeyPbs, KeyPbw, KeyPby, KeyPbm, KeyVbr
        };

        private readonly DictionaryDataObject _attributes;

        /// <summary>
        ///     The envelope of the note. default is "0,5,35,0,100,100,0,%", or new Envelope()
        /// </summary>
        public Envelope Envelope;

        /// <summary>
        ///     To access the strings,
        /// </summary>
        public Portamento Portamento;

        /// <summary>
        ///     This can be null, sorry. null means the attribute does not exist.
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
        ///     Create the note from raw text in format of ust files.
        /// </summary>
        /// <param name="list"></param>
        public USTNote(IEnumerable<string> list)
        {
            //this.TextRaw = list;
            _attributes = new DictionaryDataObject(list.Select(x => x.Split(new[] {'='}, 2))
                .ToDictionary(x => x[0], x => x[1]));

            Envelope = _attributes.ContainsKey(KeyEnvelope) ? new Envelope(_attributes[KeyEnvelope]) : new Envelope();
            _attributes.Remove(KeyEnvelope);

            if (!_attributes.ContainsKey(KeyPbw))
            {
                Portamento = null;
            }
            else
            {
                var pbw = _attributes[KeyPbw];
                var pbs = _attributes.ContainsKey(KeyPbs) ? _attributes[KeyPbs] : "0;"; //0 and invalid. 
                var pby = _attributes.ContainsKey(KeyPby)
                    ? _attributes[KeyPby]
                    : ""; //pby and pbm will be fixed by Portamento upon construction. 
                var pbm = _attributes.ContainsKey(KeyPbm) ? _attributes[KeyPbm] : "";
                Portamento = new Portamento(pbw, pbs, pby, pbm);
            }

            Vibrato = _attributes.ContainsKey(KeyVbr) ? new Vibrato(_attributes[KeyVbr]) : null;
            _attributes.Remove(KeyVbr);
        }

        /// <inheritdoc />
        public USTNote(params string[] list) : this(list.AsEnumerable())
        {
        }

        /// <summary>
        ///     make a note with minimum data and default envelope.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="lyric"></param>
        /// <param name="noteNum"></param>
        public USTNote(int length, string lyric, int noteNum)
        {
            _attributes = new DictionaryDataObject();
            Length = length;
            Lyric = lyric;
            NoteNum = noteNum;
            Envelope = new Envelope();
        }

        /// <summary>
        ///     This will convert the NoteName to NoteNum.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="lyric"></param>
        /// <param name="noteName"></param>
        public USTNote(int length, string lyric, string noteName)
            : this(length, lyric, Commons.NoteNameIndexUst[noteName])
        {
        }

        /// <summary>
        ///     Deep copy constructor.
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
        ///     Make a rest note.
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        // ReSharper disable once IntroduceOptionalParameters.Global
        public USTNote(int length) : this(length, "R", "C3")
        {
        }

        /// <summary>
        ///     Make a note without specifying all the stupid parameters. This will make 240, "あ", "C3".
        ///     We does not have an empty constructor because it is required a note have that three fields.
        ///     to preserve encapsulation we can't make an "empty" object.
        /// </summary>
        /// <returns></returns>
        public USTNote() : this(240, "あ", "C3")
        {
        }

        //This was for debugging. we debugged. it works fine.
        //public List<string> TextRaw;

        /// <summary>
        ///     Or you can access your attributes this way i guess.... not judging...
        ///     Please note you cannot access envelope, portamento or vibrato of the note here.
        ///     If for compatibility reasons you prefer their text format, we have attributes for you.
        /// </summary>
        public IReadOnlyDictionary<string, string> Attributes => _attributes;

        /// <summary>
        ///     Must-have attribute. Ticks. Must >= 15.
        /// </summary>
        public int Length
        {
            get => _attributes.GetAsInt(KeyLength);
            set => _attributes.Set(KeyPreUtterance, value);
        }

        /// <summary>
        ///     Must-have (Duh). "R" triple-equals rest. Lyric combined with postfix should be in the oto-aliases.
        /// </summary>
        public string Lyric
        {
            get => _attributes[KeyLyric];
            set => _attributes.Set(KeyPreUtterance, value);
        }

        /// <summary>
        ///     This ranges from 24(C1) to 107(B7). It's probably the god-damned mathematical convenience working up again.
        /// </summary>
        public int NoteNum
        {
            get => _attributes.GetAsInt(KeyNoteNum);
            set => _attributes.Set(KeyNoteNum, value);
        }

        /// <summary>
        ///     Default: empty (VB value). In milliseconds. This will be rounded off to 3 digits after decimal point by UTAU.
        /// </summary>
        public double PreUtterance
        {
            get => _attributes.GetAsDouble(KeyPreUtterance);
            set => _attributes.Set(KeyPreUtterance, value);
        }

        /// <summary>
        ///     For a modern user experience, use "Flags" attribute. This is for when things broke.
        /// </summary>
        public string FlagText
        {
            get => _attributes[KeyFlags];
            set => _attributes.Set(KeyFlags, value);
        }

        /// <summary>
        ///     Note Flags class is immutable. to apply (add or update) a flag value, use Flags = Flags.WithFlagValue()
        ///     Yes, this is incredibly inefficient and goes text to flags to text to flags to text to flags for one edit. but it
        ///     works. what more do you ask?
        /// </summary>
        public Flags Flags
        {
            get => new Flags(FlagText);
            set => FlagText = value.FlagText;
        }

        /// <summary>
        ///     Percentage. This will be rounded to an integer by UTAU.
        /// </summary>
        public int Intensity
        {
            get => _attributes.GetAsInt(KeyIntensity);
            set => _attributes.Set(KeyIntensity, value);
        }

        /// <summary>
        ///     Percentage. This will be rounded to an integer by UTAU.
        /// </summary>
        public int Modulation
        {
            get => _attributes.GetAsInt(KeyModulation);
            set => _attributes.Set(KeyModulation, value);
        }

        /// <summary>
        ///     In milliseconds. This will be rounded off to 3 digits after decimal point by UTAU.
        /// </summary>
        public double VoiceOverlap
        {
            get => _attributes.GetAsDouble(KeyVoiceOverlap);
            set => _attributes.Set(KeyVoiceOverlap, value);
        }

        /// <summary>
        ///     This should be between 0 and 200.
        /// </summary>
        public double Velocity
        {
            get => _attributes.GetAsDouble(KeyVelocity);
            set => _attributes.Set(KeyVelocity, value);
        }

        /// <summary>
        ///     This allow you to manipulate the velocity with desired factor value instead. This should be between 0.5 (half as
        ///     long, fastest) to 2 (twice as long, slowest)
        /// </summary>
        public double VelocityFactor
        {
            get => Commons.GetEffectiveVelocityFactor(Velocity);
            set => Velocity = Commons.GetVelocity(value);
        }

        /// <summary>
        ///     This exists for compatibility, it converts the envelope back and forth to strings when you use it.
        /// </summary>
        public string EnvelopeText
        {
            get => Envelope.ToString();
            set => Envelope = new Envelope(value);
        }

        public bool IsRest => Lyric.Equals("R");

        /// <summary>
        ///     Converts it back to its ust format.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> ToStringList()
        {
            foreach (var s in _attributes.ToStringList("=")) yield return s;

            if (Vibrato != null) yield return Vibrato.ToString();
            if (Portamento != null)
                foreach (var porta in Portamento.ToStringList())
                    yield return porta;

            yield return Envelope.ToString();
        }

        /// <summary>
        ///     Converts it back to its ust format.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join("\r\n", ToStringList()) + "\r\n";
        }
    }
}