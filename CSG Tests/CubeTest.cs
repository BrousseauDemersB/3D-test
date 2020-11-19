using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using static CoreXNA.Solids;

namespace CoreXNA.Test
{
    [TestClass]
	public class CubeTest : SolidTest
	{
		[TestMethod]
		public void Unit()
		{
			var s = Cube(1);
			Assert.AreEqual(6, s.Polygons.Count);
			AssertAcceptedStl(s, "CubeTest");
		}

		[TestMethod]
		public void UnitNonCentered()
		{
			var s = Cube(1, center: false);
			Assert.AreEqual(6, s.Polygons.Count);
			AssertAcceptedStl(s, "CubeTest");
		}

		[TestMethod]
		public void UnitCentered()
		{
			var s = Cube(1, center: true);
			Assert.AreEqual(6, s.Polygons.Count);
			var p0 = s.Polygons[0];
			Assert.IsTrue(p0.Plane.D >= 0.4);
			Assert.IsTrue(p0.Plane.D <= 0.6);
			AssertAcceptedStl(s, "CubeTest");
		}

		[TestMethod]
		public void BigRadius()
		{
			var s = Cube(1.0e12f);
			AssertAcceptedStl(s, "CubeTest");
		}
	}
}

