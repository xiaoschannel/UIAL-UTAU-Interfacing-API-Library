using System.Collections.Generic;

namespace zuoanqh.UIAL.VoiceBank
{
    /// <summary>
    ///     This models an entire voicebank.
    /// </summary>
    public class VoiceBank
    {
        public VoiceBank(string fPath)
        {
        }

        public CharacterTxt CharacterTxt { get; set; }

        /// <summary>
        ///     the entry with key "" (empty string) is the oto in this voicebank's root folder.
        /// </summary>
        public Dictionary<string, Oto> Otos { get; set; }

        public PrefixMap PrefixMap { get; set; }
    }
}