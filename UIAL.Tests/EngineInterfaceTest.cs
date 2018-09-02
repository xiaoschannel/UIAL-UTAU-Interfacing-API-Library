using Microsoft.VisualStudio.TestTools.UnitTesting;
using zuoanqh.UIAL.Engine;

namespace zuoanqh.UIAL.Tests
{
  [TestClass]
  public class EngineInterfaceTest
  {
    [TestMethod]
    public void TestPB()
    {
      var param = new ResamplerParameter();
      string[] testCases = new string[] {
        "1E#14#1T18244B5N6T7K7r7z#2#7y7x7w7v7t7s7q7o7m7k7j7h7g7e7d7d7c#2#7e7k7s748H8X8q8+9U9q+A+W+q++/P/f/r/1/8//AA#3#ABABACADAEAFAGAHAIAJAKALAM#4#ALALAJAIAGAEACAA/9/6/3/0/x/u/s/p/n/m/l/k#2#/l/m/n/p/r/t/v/y/1/4/7//ACAFAIALAOARATAWAYAZAaAbAc#2#AbAaAYAXAVASAQANAKAHAEAA/9/6/3/0/x/u/s/q/o/m/l/k#2#/l/l/n/o/q/s/v/x/0/3/6/+ABAEAHALAOAQATAVAXAZAaAbAc#2#AbAZAYAWATARAOAMAJAGAEAB/+/8/6/4/2/0/z/y/x#5#/y/z/0/1",
        "AJAWAvBOBxCTCvDBDHDI#13#DGC3CaBxA/AI/S+g929Y9I9H#14#9V9y+Y/B/j/6",
        "84#38#85#2#8686878788#2#89898+#5#9A9O9m+F+o/K/m/5//",
        "AA#43#" };
      foreach (string s in testCases)
      {
        param.PitchBendString = s;
        //this is necessary because the original algorithm is inconsistent.

        string recoded = Constants.EncodePitchBends(Constants.DecodePitchBends(param.PitchBendString));
        Assert.AreEqual(recoded, Constants.EncodePitchBends(Constants.DecodePitchBends(recoded)));
      }
    }

    [TestMethod]
    public void TestPBEfficency()
    {
      var param = new ResamplerParameter() { PitchBendString = "1E#14#1T18244B5N6T7K7r7z#2#7y7x7w7v7t7s7q7o7m7k7j7h7g7e7d7d7c#2#7e7k7s748H8X8q8+9U9q+A+W+q++/P/f/r/1/8//AA#3#ABABACADAEAFAGAHAIAJAKALAM#4#ALALAJAIAGAEACAA/9/6/3/0/x/u/s/p/n/m/l/k#2#/l/m/n/p/r/t/v/y/1/4/7//ACAFAIALAOARATAWAYAZAaAbAc#2#AbAaAYAXAVASAQANAKAHAEAA/9/6/3/0/x/u/s/q/o/m/l/k#2#/l/l/n/o/q/s/v/x/0/3/6/+ABAEAHALAOAQATAVAXAZAaAbAc#2#AbAZAYAWATARAOAMAJAGAEAB/+/8/6/4/2/0/z/y/x#5#/y/z/0/1" };
      for (int i = 0; i < 10000; i++)
        Constants.EncodePitchBends(Constants.DecodePitchBends(param.PitchBendString));
    }


  }
}
