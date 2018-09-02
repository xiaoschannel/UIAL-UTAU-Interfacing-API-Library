using System.Collections.Generic;

namespace zuoanqh.UIAL.VoiceBank
{
    /// <summary>
    ///     This models an entire voicebank.
    /// </summary>
    public class VoiceBank
    {
        public CharacterTxt CharacterTxt;

        /// <summary>
        ///     the entry with key "" (empty string) is the oto in this voicebank's root folder.
        /// </summary>
        public Dictionary<string, Oto> Otos;

        public PrefixMap PrefixMap;

        public VoiceBank(string fPath)
        {
        }
    }
}