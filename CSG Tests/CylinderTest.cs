using Microsoft.VisualStudio.TestTools.UnitTesting;
using static CoreXNA.Solids;

namespace CoreXNA.Test
{
	[TestClass]
	public class CylinderTest : SolidTest
	{
		[TestMethod]
		public void Unit()
		{
			var solid = Cylinder(1, 1);
			Assert.AreEqual(36, solid.Polygons.Count);
			AssertAcceptedStl(solid, "CylinderTest");
		}
		
		[TestMethod]
		public void UnitCentered()
		{
			var solid = Cylinder(1, 1, center: true);
			Assert.AreEqual(36, solid.Polygons.Count);
			var pm = solid.Polygons[solid.Polygons.Count-2];
			Assert.IsTrue(pm.Plane.D >= 0.9);
			Assert.IsTrue(pm.Plane.D <= 1.1);
			AssertAcceptedStl(solid, "CylinderTest");
		}

		[TestMethod]
		public void BigRadius()
		{
			var solid = Cylinder(1.0e12f, 1);
			AssertAcceptedStl(solid, "CylinderTest");
		}

		[TestMethod]
		public void BigRadiusCentered()
		{
			var solid = Cylinder(1.0e12f, 1, center: true);
			var pm = solid.Polygons[solid.Polygons.Count-2];
			Assert.IsTrue(pm.Plane.D >= 0.9e12);
			Assert.IsTrue(pm.Plane.D <= 1.1e12);
			AssertAcceptedStl(solid, "CylinderTest");
		}
	}
}

