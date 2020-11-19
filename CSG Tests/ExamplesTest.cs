using Microsoft.VisualStudio.TestTools.UnitTesting;
using static CoreXNA.Solids;

namespace CoreXNA.Test
{
	[TestClass]
	public class ExamplesTest : SolidTest
	{
		[TestMethod]
		public void OpenJsCadLogo()
		{
			var r =
				Union(
					Difference(
						Cube(size: 3, center: true),
						Sphere(r: 2, center: true)
					),
					Intersection(
						Sphere(r: 1.3f, center: true),
						Cube(size: 2.1f, center: true)
					)
				).Translate(0, 0, 1.5f).Scale(10);
			AssertAcceptedStl(r, "Examples");
		}
	}
}

