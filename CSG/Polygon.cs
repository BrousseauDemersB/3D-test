using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace CoreXNA
{
    /// <summary>
    /// Convex polygons comprised of vertices lying on a plane.
    /// </summary>
    public class Polygon
    {
        public readonly Vertex[] Vertices;
        public readonly Plane Plane;
        public Color PolygonColor;

        private bool BoundingSphereIsCached;
        private bool BoundingBoxIsCached;
        BoundingSphere cachedBoundingSphere;
        BoundingBox cachedBoundingBox;

        public Polygon(Color SolidColor, Vertex[] Vertices, Plane plane)
        {
            this.Vertices = Vertices;
            Plane = plane;
            this.PolygonColor = SolidColor;
        }

        public Polygon(Color SolidColor, params Vertex[] Vertices)
        {
            this.Vertices = Vertices;
            Plane = CSGHelper.FromVector3s(Vertices[0].Pos, Vertices[1].Pos, Vertices[2].Pos);
            this.PolygonColor = SolidColor;
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                if (!BoundingSphereIsCached)
                {
                    BoundingSphereIsCached = true;
                    var box = BoundingBox;
                    var middle = (box.Min + box.Max) * 0.5f;
                    var radius3 = box.Max - middle;
                    var radius = radius3.Length();
                    cachedBoundingSphere = new BoundingSphere { Center = middle, Radius = radius };
                }
                return cachedBoundingSphere;
            }
        }

        public BoundingBox BoundingBox
        {
            get
            {
                if (!BoundingBoxIsCached)
                {
                    BoundingBoxIsCached = true;
                    Vector3 minpoint, maxpoint;
                    Vertex[] vertices = this.Vertices;
                    int numvertices = vertices.Length;
                    if (numvertices == 0)
                    {
                        minpoint = new Vector3(0, 0, 0);
                    }
                    else
                    {
                        minpoint = vertices[0].Pos;
                    }
                    maxpoint = minpoint;
                    for (var i = 1; i < numvertices; i++)
                    {
                        var point = vertices[i].Pos;
                        minpoint = minpoint.Min(point);
                        maxpoint = maxpoint.Max(point);
                    }
                    cachedBoundingBox = new BoundingBox(minpoint, maxpoint);
                }
                return cachedBoundingBox;
            }
        }

        public Polygon Flipped()
        {
            Vertex[] ArrayNewVertex = new Vertex[Vertices.Length];
            for (int i = 0; i < Vertices.Length; i++)
            {
                ArrayNewVertex[Vertices.Length - i - 1] = Vertices[i].Flipped();
            }
            Plane newplane = Plane.Flipped();
            return new Polygon(PolygonColor, ArrayNewVertex, newplane);
        }
    }
}
