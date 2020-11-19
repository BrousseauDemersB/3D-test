using Microsoft.Xna.Framework;

namespace CoreXNA
{
    public struct Vertex
    {
        /// <summary>
        /// The world position of this vertex.
        /// </summary>
        public readonly Vector3 Pos;

        /// <summary>
        /// The texture coordinate of this vertex.
        /// </summary>
		public readonly Vector2 Tex;

        /// <summary>
        /// Initializes a new <see cref="T:Csg.Vertex"/> at a given position and with a given texture coordinate.
        /// </summary>
        /// <param name="pos">World position</param>
        /// <param name="tex">Texture coordinate</param>
        public Vertex(Vector3 pos, Vector2 tex)
        {
            Pos = pos;
            Tex = tex;
        }

        /// <summary>
        /// Get a flipped version of this vertex.
        /// May return the same object if no changes are needed to flip.
        /// </summary>
        public Vertex Flipped()
        {
            return this;
        }

        public override string ToString() => Pos.ToString();

        /// <summary>
        /// Left multiplies the position of this vertex with the given matrix.
        /// The texture coordinate is unchanged.
        /// </summary>
        /// <returns>A new transformed vertex.</returns>
        /// <param name="matrix4x4">The transformation.</param>
        public Vertex Transform(Matrix matrix4x4)
        {
            Vector3 newpos = Vector3.Transform(Pos, matrix4x4);
            return new Vertex(newpos, Tex);
        }
    }
}
