using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using zuoanqh.libzut.win.FileIO;
using System;
using zuoanqh.UIAL.UST;
using zuoanqh.libzut.Data;

namespace zuoanqh.UIAL.FileExtraction
{

  class Program
  {
    public static string[] InputLocations = { @"D:\Test UST Archive\Full" };

    /// <summary>
    /// These are the default locations of your voicebanks.
    /// </summary>
    //public static string[] InputLocations = { @"D:\Program Files (x86)\UTAU\voice", @"D:\Users\" + NAME + @"\AppData\Roaming\UTAU\voice" };

    /// <summary>
    /// Where the files will be copied to.
    /// </summary>
    public static string outputLocation1 = @"D:\Test UST Archive\Longer 140 Notes";
    public static string outputLocation2 = @"D:\Test UST Archive\Tuned";

    static void Main(string[] args)
    {
      FilterUSTs();
      //TrimUSTArchives();
      //ExtractVBFiles();
    }

    private static void FilterUSTs()
    {
      foreach (string fp in Directory.EnumerateFiles(InputLocations[0], "*", SearchOption.AllDirectories)
            .Where((s) => s.ToLower().EndsWith(".ust")))
      {
        USTFile f = new USTFile(fp);
        if (f.Notes.Count>=140)
          File.Copy(fp, Path.Combine(outputLocation1, fp.Substring(3).Replace(@"\", "__")));

        if (f.Notes.Count>0&& f.Notes.Max((n)=>n.Portamento!=null&&n.Portamento.Segments.Count>=2))
          File.Copy(fp, Path.Combine(outputLocation2, fp.Substring(3).Replace(@"\", "__")));
      }

      Logger.SaveAndOpen();
    }

    /// <summary>
    /// These are codes i used to process the ust test cases
    /// </summary>
    private static void TrimUSTArchives()
    {
      Stopwatch sw = new Stopwatch();
      sw.Start();
      foreach (string author in Directory.GetDirectories(InputLocations[0]))
      {
        Logger.Log("");
        Logger.Log("Processing: " + author);

        List<string> folderToDelete = new List<string>();
        foreach (string item in Directory.GetDirectories(author))
        {
          if (Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories)
            .Where((s) => s.ToLower().EndsWith(".ust")).Count() == 0)
          {//skip folders that have no ust files and mark them for deletion.
            folderToDelete.Add(item); 
            continue;
          }

          //Purging files that's not ust or .txt (which could be a readme)
          List<string> toDelete = Directory.EnumerateFiles(item, "*", SearchOption.AllDirectories)
            .Where((s) => (!s.ToLower().EndsWith(".ust") && !s.ToLower().EndsWith(".txt"))).ToList();

          foreach (string f in toDelete)
          {
            Logger.Log("\tPurging irrelavant file: " + f);
            File.Delete(f);
          }

          //Remove folders that's now empty.
          RemoveEmptyFoldersRecursively(item);

          // if this is the only folder and there's no other file, move the contents out. 
          // possible to have multiple layers.
          while (Directory.GetFiles(item).Count() <= 1
            && Directory.GetDirectories(item).Count() == 1)
          {
            String target = Directory.GetDirectories(item)[0];
            Logger.Log("\tMoving files out of directory: " + target);
            Directory.Move(target, target + "__t__");// in case there's a nested folder with the same name
            target += "__t__";
            Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(target, item);//Thank you VB
            Directory.Delete(target,true);
          }

        }

        foreach (string item in folderToDelete)
        {
          Logger.Log("\tRemoving folder that don't contain any ust: " + item);
          Directory.Delete(item, true);
        }

        //var folders = Directory.GetDirectories(author).ToList();
        //foreach (var v in folders)
        //  Directory.Move(v, v.Insert(v.LastIndexOf(@"\")+1, author.Substring(author.LastIndexOf(@"\") + 1) + "__"));
      }

      sw.Stop();
      Logger.Log("Time Spent: " + sw.Elapsed.TotalSeconds + " Seconds");
      Logger.SaveAndOpen();
    }

    private static void RemoveEmptyFoldersRecursively(string item)
    {
      List<string> marked = new List<string>();
      foreach (string dir in Directory.GetDirectories(item))
      {
        RemoveEmptyFoldersRecursively(dir);
        if (Directory.GetFiles(dir).Count() == 0 &&
            Directory.GetDirectories(dir).Count() == 0)
        {
          Logger.Log("\tPurging empty foler: " + dir);
          Directory.Delete(dir, false);
        }
      }
    }

    /// <summary>
    /// This is the code I used for extracting VB Files.
    /// </summary>
    private static void ExtractVBFiles()
    {
      Stopwatch sw = new Stopwatch();
      sw.Start();
      int locationCount = 0;
      int vbCount = 0;
      int fileCount = 0;
      List<string> files = new List<string>();

      foreach (string loc in InputLocations)//============================Location loop
      {
        if (!Directory.Exists(loc))
        {
          Logger.LogTimed("Directory does not exist: " + loc);
          continue;//skip this one
        }
        locationCount++;
        Logger.Log("===================================================================");
        Logger.LogTimed("Extracting from: " + loc);

        foreach (string vb in Directory.GetDirectories(loc))//=========VB loop
        {
          string vbname = Path.GetFileName(vb);
          Logger.Log("Extracting from VB: " + vbname);
          vbCount++;
          //Process direct files: character, prefix, direct oto(if not empty)
          foreach (string f in targetFileNames.Select((s) => Path.Combine(vb, s)))
          {
            if (File.Exists(f))
            {
              Logger.Log("\tExtracted VB root folder file: " + f);
              files.Add(f);
              fileCount++;
            }
          }

          //Process files from all subfolders
          foreach (string sf in Directory.GetDirectories(vb))
          {
            string foldername = Path.GetFileName(sf);
            Logger.Log("\tExtracting from VB folder: " + foldername);

            foreach (string f in targetFileNames.Select((s) => Path.Combine(sf, s)))
            {
              if (File.Exists(f))
              {
                Logger.Log("\t\tExtracted file: " + f);
                files.Add(f);
                fileCount++;
              }
            }
          }
        }

      }

      foreach (string f in files)
      {
        //now do something?
      }

      Logger.Log("===================================================================");
      Logger.Log("Locations Extracted   : " + locationCount);
      Logger.Log("Voicebanks Extracted  : " + vbCount);
      Logger.Log("Total Files Extracted : " + fileCount);
      sw.Stop();
      Logger.Log("Time Spent            : " + sw.Elapsed.TotalSeconds + " Seconds");
      Logger.SaveAndOpen();
    }

    /// <summary>
    /// Your account name.
    /// </summary>
    public const string NAME = "zuoanqh";



    /// <summary>
    /// Types of file you want it to grab.
    /// </summary>
    public static string[] targetFileNames = { @"prefix.map", @"oto.ini", @"character.txt" };

  }
}
