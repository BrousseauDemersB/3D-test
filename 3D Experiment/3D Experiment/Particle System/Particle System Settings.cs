using Experiment.Cache;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Experiment.ParticleSystem
{
    public class ParticleSystemSettings
    {
        public readonly string TextureName;
        public readonly int MaxParticles;
        public readonly int NumberOfImages;
        public readonly BlendState BlendState;
        public readonly bool RotateTowardCamera;

        private readonly FloatCache DurationCache;
        private readonly Vector3Cache OffsetCache;
        private readonly Vector2Cache ScaleCache;
        private readonly Vector3Cache RotationCache;
        private readonly Vector3Cache SpeedCache;

        public ParticleSystemSettings(string TextureName, int MaxParticles, int NumberOfImages, BlendState BlendState, bool RotateTowardCamera)
        {
            this.TextureName = TextureName;
            this.MaxParticles = MaxParticles;
            this.NumberOfImages = NumberOfImages;
            this.BlendState = BlendState;
            this.RotateTowardCamera = RotateTowardCamera;
            
            DurationCache = new FloatCache(10f, 10f);
            OffsetCache = new Vector3Cache(new Vector3(-50f, -5f, -50f), new Vector3(50f, 0f, 5f));
            ScaleCache = new Vector2Cache(new Vector2(0.01f, 0.01f), new Vector2(0.1f, 0.1f));
            RotationCache = new Vector3Cache(new Vector3(0f, -2f, -1f), new Vector3(0f, 2f, 1f));
            SpeedCache = new Vector3Cache(new Vector3(0f, -10f, 0f), new Vector3(0f, -25f, 0f));
        }

        public float ComputeDuration()
        {
            return DurationCache.GetNextFloat();
        }

        public Vector3 ComputeOffset()
        {
            return OffsetCache.GetNextVector3();
        }

        public Vector2 ComputeScale()
        {
            return ScaleCache.GetNextVector2();
        }

        public Vector3 ComputeRotation()
        {
            return RotationCache.GetNextVector3();
        }

        public Vector3 ComputeSpeed()
        {
            return SpeedCache.GetNextVector3();
        }
    }
}
