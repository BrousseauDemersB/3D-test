using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using static CoreXNA.Solids;

namespace CoreXNA.Test
{
	[TestClass]
	public class IntersectTest : SolidTest
	{
		[TestMethod]
		public void UnitSphere_UnitSphere()
		{
			var sphere1 = Sphere(1, new Vector3(-0.5f, 0, 0));
			var sphere2 = Sphere(1, new Vector3(0.5f, 0, 0));
			var r = sphere1.Intersect(sphere2);
			Assert.AreEqual(32, r.Polygons.Count);
			Assert.IsTrue(r.IsCanonicalized);
			Assert.IsTrue(r.IsRetesselated);
			AssertAcceptedStl(r, "IntersectTest");
		}

		[TestMethod]
		public void UnitSphere_NoOverlap_UnitSphere()
		{
			var sphere1 = Sphere(1, new Vector3(-50, 0, 0));
			var sphere2 = Sphere(1, new Vector3(50, 0, 0));
			var r = sphere1.Intersect(sphere2);
			Assert.AreEqual(0, r.Polygons.Count);
			Assert.IsTrue(r.IsCanonicalized);
			Assert.IsTrue(r.IsRetesselated);
			AssertAcceptedStl(r, "IntersectTest");
		}

		[TestMethod]
		public void CoplanarExact()
		{
			var solid1 = Cube(4, new Vector3(-2, 0, 0));
			var solid2 = Cube(4, new Vector3(2, 0, 0));
			var r = solid1.Intersect(solid2);
			Assert.AreEqual(0, r.Polygons.Count);
			Assert.IsTrue(r.IsCanonicalized);
			Assert.IsTrue(r.IsRetesselated);
			AssertAcceptedStl(r, "IntersectTest");
		}

		[TestMethod]
		public void CoplanarInset()
		{
			var solid1 = Cube(4, new Vector3(-2, 0, 0));
			var solid2 = Cube(2, new Vector3(1, 0, 0));
			var r = solid1.Intersect(solid2);
			Assert.AreEqual(0, r.Polygons.Count);
			Assert.IsTrue(r.IsCanonicalized);
			Assert.IsTrue(r.IsRetesselated);
			AssertAcceptedStl(r, "IntersectTest");
		}
	}
}

