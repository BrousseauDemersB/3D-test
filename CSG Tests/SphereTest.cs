using Microsoft.VisualStudio.TestTools.UnitTesting;
using static CoreXNA.Solids;

namespace CoreXNA.Test
{
	[TestClass]
	public class SphereTest : SolidTest
	{
		[TestMethod]
		public void Unit()
		{
			var sphere = Sphere(1);
			Assert.AreEqual(72, sphere.Polygons.Count);
			var p0 = sphere.Polygons[0];
			Assert.IsTrue(p0.Plane.D >= 0.9);
			Assert.IsTrue(p0.Plane.D <= 1.1);
			AssertAcceptedStl(sphere, "SphereTest");
		}

		[TestMethod]
		public void BigRadius()
		{
			var sphere = Sphere(1.0e12f);
			var p0 = sphere.Polygons[0];
			Assert.IsTrue(p0.Plane.D >= 0.9e12);
			Assert.IsTrue(p0.Plane.D <= 1.1e12);
			AssertAcceptedStl(sphere, "SphereTest");
		}
	}
}

