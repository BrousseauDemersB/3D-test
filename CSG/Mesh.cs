using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CoreXNA
{
    public class Mesh
    {
        public List<int[]> triangles;
        public List<Vector3> vertices;
        public List<List<double>> colors;

        public Mesh()
        {
        }
    }
}
