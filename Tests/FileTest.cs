using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using zuoanqh.UIAL.VB;
using zuoanqh.UIAL.UST;
using zuoanqh.UIAL.Extensions.Mrq4Cs;
using zuoanqh.libzut.FileIO;
using zuoanqh.libzut.Data;
//using zuoanqh.libzut;

namespace zuoanqh.UIAL.Testing
{
  [TestClass]
  public class FileTest
  {
    [TestInitialize()]
    public void Initialize()
    {
      Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);// because .net standard don't currently have sjis
      Logger.LogDirectory = @"D:\UIAL_Logger";
    }
    [TestMethod]
    public void TestOto()
    {
      //var a = PrefixMap.REGISTER_NAMES;
      Stopwatch w = new Stopwatch();
      string otos = Path.Combine(Directory.GetCurrentDirectory(), "dummyOtos");
      List<Oto> lib = new List<Oto>();

      Oto.CHECK_ENCODING = false;
      foreach (string s in Directory.GetFiles(otos))
      {
        var o = new Oto(s);
        lib.Add(o);
        Logger.Log("File: " + Path.GetFileName(s));
        Logger.Log("Encoding: " + zuio.GetEncUde(s));
        Logger.Log("Alias: " + o.AliasCount);
        Logger.Log("Extras: " + o.ExtrasCount);
        //if (o.ExtrasCount > 0)
          //Logger.Log("Extra Alias Counts: " + zusp.List(o.Extras.Keys.Select((k) => "\"" + k + "\" " + o.Extras[k].Count).ToArray()));
        Logger.Log("=====================================");
      }
      Console.WriteLine("Time: " + w.Elapsed.TotalMilliseconds + "ms");

      Logger.Save();
    }

    [TestMethod]
    public void TestUSTLength()
    {
      foreach (string fp in Directory.EnumerateFiles(@"D:\Test UST Archive\Full", "*", SearchOption.AllDirectories)
      .Where((s) => s.ToLower().EndsWith(".ust")))
      {
        USTFile f = new USTFile(fp);
        if (f.Notes.Count >= 140)
          File.Copy(fp, Path.Combine(@"D:\Test UST Archive\Longer 140 Notes", fp.Substring(3).Replace(@"\", "__").Substring(24)));

        if (f.Notes.Count > 0 && f.Notes.Max((n) => n.Portamento != null && n.Portamento.Segments.Count >= 2))
          File.Copy(fp, Path.Combine(@"D:\Test UST Archive\Tuned", fp.Substring(3).Replace(@"\", "__").Substring(24)));
      }
    }

    //[TestMethod]
    public void TestUSTEncoding()
    {
      string usts = @"D:\Test UST Archive";
      TallyDictionary<Encoding> count = new TallyDictionary<Encoding>();
      Stopwatch sw = new Stopwatch();
      sw.Start();
      int undetected = 0,detected=0;
      foreach (string f in Directory.EnumerateFiles(usts, "*.*", SearchOption.AllDirectories).
        Where((s) => (s.ToLower().EndsWith(".ust"))))
      {
        try
        {
          count.Add(zuio.GetEncUde(f,256));
          detected++;
        }
        catch (Exception e)
        {
          Logger.Log(f);
          Logger.Log(e.Message);
          undetected++;
        }
        
      }
      sw.Stop();
      Logger.Log("Took " + sw.ElapsedMilliseconds + "ms total. Good files: "+detected+" Undetected files: "+undetected);
      foreach(var e in count.Keys)
        Logger.Log(e.EncodingName + "\t" + count[e]);
      Logger.Save();
    }

    [TestMethod]
    public void TestUSTRead()
    {
      string usts = @"D:\Test UST Archive\Full";
      List<USTFile> lib = new List<USTFile>();
      Stopwatch sw = new Stopwatch();
      sw.Start();
      int count = 0;
      foreach (string f in Directory.EnumerateFiles(usts, "*.*", SearchOption.AllDirectories).
        Where((s) => (s.ToLower().EndsWith(".ust"))))
      {
        Logger.Log(f);
        lib.Add(new USTFile(f));
      }
      sw.Stop();
      Logger.Log("Took " + sw.ElapsedMilliseconds + "ms total, " + lib.Count + " files, " + sw.ElapsedMilliseconds / lib.Count + "ms per file");
      Logger.Save();
      //System.Diagnostics.Process.Start(Path.Combine(Directory.GetCurrentDirectory(), "testout.ust"));
    }

    [TestMethod]
    public void TestReadMrq()
    {
      var dic = Mrq.GetDictionary(@"D:\SourceTree\UTAU-Interfacing API Library\Tests\dummyMrqs\desc.mrq");

      foreach (var v in dic.Keys)
        Logger.Log(v + "\t" + string.Join(", ", dic[v]));

      Logger.Save();
      // var ent = Mrq.DoYourThingPlease(@"D:\SourceTree\UTAU-Interfacing API Library\Tests\dummyMrqs\desc.mrq");
    }
  }
}
