using System;
using System.Runtime.CompilerServices;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CoreXNA.Test
{
	public class SolidTest
	{
		protected void AssertAcceptedStl(Solid csg, string fixtureName, [CallerMemberName] string testName = "")
		{
			var aname = $"{fixtureName}.{testName}.stl";
			var rname = $"{fixtureName}.{testName}_.stl";
			var asmPath = System.Reflection.Assembly.GetCallingAssembly().Location;
			var repoPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(asmPath)));
			var resultsPath = Path.Combine(repoPath, "Results");
			if (!Directory.Exists(resultsPath))
			{
				Console.WriteLine("Test results could not be found at {0}", resultsPath);
				return;
			}

			var acceptedPath = Path.Combine(resultsPath, aname);
			var rejectedPath = Path.Combine(resultsPath, rname);
			File.Delete(rejectedPath);

			var testStl = csg.ToStlString(testName).Split(new String[] { "\r\n"}, StringSplitOptions.RemoveEmptyEntries);

			if (!File.Exists(acceptedPath))
			{
				File.WriteAllText(rejectedPath, csg.ToStlString (testName));
				Assert.Inconclusive("No results have been marked as accepted.");
			}
			else {
				var acceptedStl = File.ReadAllLines(acceptedPath);

				for (int i = 0; i < acceptedStl.Length; ++i)
					{
					if (testStl[i] != acceptedStl[i])
						{
						File.WriteAllText (rejectedPath, csg.ToStlString (testName));
						Assert.Fail ("Result differs from accepted.");
						return;
					}
				}
			}
		}
	}
}

