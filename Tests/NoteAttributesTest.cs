using Microsoft.VisualStudio.TestTools.UnitTesting;
using zuoanqh.UIAL;
using zuoanqh.UIAL.UST;

namespace zuoanqh.UIAL.Testing
{
  [TestClass]
  public class NoteAttributesTest
  {
    [TestMethod]
    public void TestVelocity()
    {
      Assert.AreEqual(CommonReferences.GetEffectiveVelocityFactor(0), 2);
      Assert.AreEqual(CommonReferences.GetEffectiveVelocityFactor(100), 1);
      Assert.AreEqual(CommonReferences.GetEffectiveVelocityFactor(200), 0.5);
      Assert.AreEqual(CommonReferences.GetVelocity(2), 0);
      Assert.AreEqual(CommonReferences.GetVelocity(1), 100);
      Assert.AreEqual(CommonReferences.GetVelocity(0.5), 200);
    }

    [TestMethod]
    public void TestFlagEditing()
    {
      Flags f = new Flags("g-5H50Mt100B3");
      Assert.IsTrue(f.HasFlag("g"));
      Assert.IsFalse(f.HasFlag("Mf"));
      Assert.AreEqual(f.GetFlagsFirstValue("Mt"), 100);
      Assert.AreEqual(f.GetFlagsFirstValue("B"), 3);

      f = f.WithFlag("Mt", -50);
      Assert.AreEqual(f.GetFlagsFirstValue("Mt"), -50);

      f = new Flags("g-5H50Mt100MeME34.5678GB3N");
      Assert.IsTrue(f.HasFlag("Me"));
      Assert.IsTrue(f.HasFlag("ME"));
      Assert.IsTrue(f.HasFlag("G"));
      Assert.IsTrue(f.HasFlag("B"));
      Assert.IsTrue(f.HasFlag("N"));

      f = f.WithoutFlag("B");
      Assert.IsFalse(f.HasFlag("B"));
      Assert.IsTrue(f.HasFlag("Me"));
      Assert.IsTrue(f.HasFlag("ME"));
      Assert.IsTrue(f.HasFlag("G"));
      Assert.IsTrue(f.HasFlag("N"));

      f = f.WithFlag("B", 3);
      Assert.IsTrue(f.HasFlag("B"));
      Assert.IsTrue(f.HasFlag("Me"));
      Assert.IsTrue(f.HasFlag("ME"));
      Assert.IsTrue(f.HasFlag("G"));
      Assert.IsTrue(f.HasFlag("N"));
    }

    [TestMethod]
    public void TestFlagUnknownHandling()
    {
      var f = new Flags("g-5H50Mt100B3asdfa123.1231sfwqga5.112233eg23ger1.23sdfge123ar");
      Assert.IsTrue(f.HasFlag("ar"));
      Assert.AreEqual(f.GetFlagsFirstValue("ger"), 1.23);
    }

    [TestMethod]
    public void TestFlagEfficency()
    {
      var f = new Flags("g-5H50Mt100MeMo-78.345GB3N");//we expect worst flags to be about this long

      for (int i = 0; i < 100; i++)//let's see how long it takes to process 100 typical usts.
        for (int j = 0; j < 700; j++)//we expect one ust file have about 700 notes
          f = f.WithFlag("Mt", -50);
    }

    [TestMethod]
    public void TestEnvelope()
    {
      Envelope e = new Envelope("0,5,35,0,100,100,0,%,0,10,100");
      Assert.AreEqual(e.p1, 0);
      Assert.AreEqual(e.p2, 5);
      Assert.AreEqual(e.p3, 35);
      Assert.AreEqual(e.v1, 0);
      Assert.AreEqual(e.v2, 100);
      Assert.AreEqual(e.v3, 100);
      Assert.AreEqual(e.v4, 0);
      Assert.IsTrue(e.HasPercentMark);
      Assert.IsTrue(e.HasP4);
      Assert.IsTrue(e.HasP5);
      Assert.IsTrue(e.HasV5);
      Assert.AreEqual(e.p4, 0);
      Assert.AreEqual(e.p5, 10);
      Assert.AreEqual(e.v5, 100);

      e.RemoveP5();
      Assert.IsFalse(e.HasP5);
      Assert.IsFalse(e.HasV5);
      e = new Envelope("0,5,35,0,100,100,0");
      Assert.IsFalse(e.HasPercentMark);
      Assert.IsFalse(e.HasP4);
      Assert.IsFalse(e.HasP5);
      Assert.IsFalse(e.HasV5);
      e = new Envelope("0,5,35,0,100,100,0,");
      Assert.IsFalse(e.HasPercentMark);
      Assert.IsFalse(e.HasP5);
      Assert.IsFalse(e.HasV5);
      e = new Envelope("0,5,35,0,100,100,0,,0,10");
      Assert.IsFalse(e.HasPercentMark);
      Assert.IsTrue(e.HasP5);
      Assert.IsFalse(e.HasV5);
      e = new Envelope("0,5,35,0,100,100,0,%,0");
      Assert.IsFalse(e.HasP5);
      Assert.IsFalse(e.HasV5);
    }
  }
}
