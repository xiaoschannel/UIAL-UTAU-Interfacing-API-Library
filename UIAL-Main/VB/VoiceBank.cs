using System.Collections.Generic;

namespace zuoanqh.UIAL.VB
{
  /// <summary>
  /// This models an entire voicebank.
  /// </summary>
  public class VoiceBank
  {
    public PrefixMap PrefixMap;

    /// <summary>
    /// the entry with key "" (empty string) is the oto in this voicebank's root folder.
    /// </summary>
    public Dictionary<string, Oto> Otos;

    public CharacterTxt CharacterTxt;

    public VoiceBank(string fPath)
    { }
  }
}
