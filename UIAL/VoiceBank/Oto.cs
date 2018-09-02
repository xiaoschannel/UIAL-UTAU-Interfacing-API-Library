using System.Collections.Generic;
using System.Linq;
using zuoanqh.libzut;
using zuoanqh.libzut.FileIO;

namespace zuoanqh.UIAL.VoiceBank
{
    public class Oto
    {
        /// <summary>
        ///     This is meant to be true at all time, except for debugging.
        /// </summary>
        public static bool CHECK_ENCODING = true;

        public Dictionary<string, OtoAlias> Aliases;

        public OtoAlias[] AliasesOrdered;
        public Dictionary<string, List<OtoAlias>> Extras;

        public Oto(string fPath) : this(ByLineFileIO.ReadFileNoWhitespace(fPath, zuio.GetEncUde(fPath)).ToArray())
        {
        }

        public Oto(string[] Data)
        {
            Aliases = new Dictionary<string, OtoAlias>();
            Extras = new Dictionary<string, List<OtoAlias>>();

            AliasesOrdered =
                Data.Select(s => new OtoAlias(s)).ToArray(); //This will ensure the order from the file is preserved.

            UpdateAliasesAndExtras();
        }

        public int AliasCount => Aliases.Count;

        public int LineCount => AliasesOrdered.Length;

        public int ExtrasCount => LineCount - AliasCount;

        private void UpdateAliasesAndExtras()
        {
            Aliases.Clear();
            Extras.Clear();
            foreach (var a in AliasesOrdered)
                if (Aliases.ContainsKey(a.Alias)) //deal with repeated aliases.
                {
                    //Logger.Log("Repeated Alias\t" + t.Alias);
                    if (!Extras.ContainsKey(a.Alias)) //initialize if needed.
                        Extras.Add(a.Alias, new List<OtoAlias>());
                    Extras[a.Alias].Add(a);
                }
                else //normal case
                {
                    Aliases.Add(a.Alias, a);
                }
        }

        public OtoAlias GetAlias(string AliasName)
        {
            return Aliases[AliasName];
        }

        public List<OtoAlias> GetAliases(string AliasName)
        {
            var ans = new List<OtoAlias> {Aliases[AliasName]};
            if (Extras.ContainsKey(AliasName))
                ans.AddRange(Extras[AliasName]);

            return ans;
        }

        public string GetCommonPostfix()
        {
            return zusp.GetCommonPostfix(Aliases.Keys.ToList());
        }

        public void ChangeCommonPostfix(string NewPostfix)
        {
            var postfix = GetCommonPostfix();
            foreach (var a in AliasesOrdered)
                a.Alias = zusp.Left(a.Alias, a.Alias.Length - postfix.Length);
            UpdateAliasesAndExtras();
        }

        public List<string> ToStringList()
        {
            var ans = new List<string>();
            foreach (var a in AliasesOrdered)
                ans.Add(a.ToString());
            return ans;
        }

        public override string ToString()
        {
            return string.Join("\r\n", ToStringList().ToArray()) + "\r\n";
        }
    }
}