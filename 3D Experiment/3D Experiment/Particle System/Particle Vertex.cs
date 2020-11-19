using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Experiment.ParticleSystem
{
    struct ParticleVertex
    {
        public Vector3 Position;
        public Vector2 UV;
        public float CreationTime;
        public float EndTime;
        public Vector2 Scale;
        public Vector3 Rotation;
        public Vector3 Speed;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 3 + sizeof(float) * 2, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 3 + sizeof(float) * 2 + sizeof(float), VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(sizeof(float) * 3 + sizeof(float) * 2 + sizeof(float) + sizeof(float), VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 3),
            new VertexElement(sizeof(float) * 3 + sizeof(float) * 2 + sizeof(float) + sizeof(float) + sizeof(float) * 2, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 4),
            new VertexElement(sizeof(float) * 3 + sizeof(float) * 2 + sizeof(float) + sizeof(float) + sizeof(float) * 2 + sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 5)
         );

        public const int SizeInBytes = sizeof(float) * 3 + sizeof(float) * 2 + sizeof(float) + sizeof(float) + sizeof(float) * 2 + sizeof(float) * 3 + sizeof(float) * 3;
    }
}
