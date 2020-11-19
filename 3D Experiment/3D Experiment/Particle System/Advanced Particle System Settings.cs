using Experiment.Cache;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Experiment.ParticleSystem
{
    public class AdvancedParticleSystemSettings
    {
        private const int NumberOfSeeds = 20;
        private const int NumberOfValuesOverTime = 200;

        public readonly string TextureName;
        public readonly int MaxParticles;
        public readonly int NumberOfImages;
        public readonly BlendState BlendState;
        public readonly bool RotateTowardCamera;

        private readonly FloatCache DurationCache;
        private readonly Vector3Cache OffsetCache;

        private readonly Vector2ValuesOverTime[] ScaleValuesOverTime;
        private readonly Vector2Cache ScaleCache;
        private readonly Vector3Cache RotationCache;
        private readonly Vector3Cache SpeedCache;

        public AdvancedParticleSystemSettings(string TextureName, int MaxParticles, int NumberOfImages, BlendState BlendState, bool RotateTowardCamera)
        {
            this.TextureName = TextureName;
            this.MaxParticles = MaxParticles;
            this.NumberOfImages = NumberOfImages;
            this.BlendState = BlendState;
            this.RotateTowardCamera = RotateTowardCamera;
            
            DurationCache = new FloatCache(10f, 10f);
            OffsetCache = new Vector3Cache(new Vector3(-50f, -5f, -50f), new Vector3(50f, 0f, 5f));

            ScaleCache = new Vector2Cache(new Vector2(0.01f, 0.01f), new Vector2(0.1f, 0.1f));

            List<Vector2ValuesOverTime.ValueAtTime> ListScaleValuesOverTime = new List<Vector2ValuesOverTime.ValueAtTime>();

            ListScaleValuesOverTime.Add(new Vector2ValuesOverTime.ValueAtTime(0f, Vector2.Zero));
            ListScaleValuesOverTime.Add(new Vector2ValuesOverTime.ValueAtTime(1f, Vector2.One));

            ScaleValuesOverTime = new Vector2ValuesOverTime[NumberOfSeeds];
            for (int S = 0; S < NumberOfSeeds; ++S)
            {
                Vector2 Min = ScaleCache.GetNextVector2();
                Vector2 Max = ScaleCache.GetNextVector2();

                if (Max.X < Min.X)
                {
                    float Temp = Max.X;
                    Max.X = Min.X;
                    Min.X = Temp;
                }
                if (Max.Y < Min.Y)
                {
                    float Temp = Max.Y;
                    Max.Y = Min.Y;
                    Min.Y = Temp;
                }

                ScaleValuesOverTime[S] = new Vector2ValuesOverTime(5, Min, Max, ListScaleValuesOverTime);
            }
        }

        public float ComputeDuration()
        {
            return DurationCache.GetNextFloat();
        }

        public Vector3 ComputeOffset()
        {
            return OffsetCache.GetNextVector3();
        }

        public Vector2[] GetScaleValuesOverTime()
        {
            Vector2[] Output = new Vector2[NumberOfSeeds * NumberOfValuesOverTime];

            for (int S = 0; S < NumberOfSeeds; ++S)
            {
                Vector2[] ArrayValuesOverTime = ScaleValuesOverTime[S].GetValuesOverTime();
                for (int V = 0; V < ArrayValuesOverTime.Length; ++V)
                {
                    Output[S * NumberOfValuesOverTime + V] = ArrayValuesOverTime[V];
                }
            }

            return Output;
        }

        public Texture2D GetTexture(GraphicsDevice GraphicsDevice)
        {
            Texture2D OutTex = new Texture2D(GraphicsDevice, NumberOfSeeds, NumberOfValuesOverTime, false, SurfaceFormat.Vector4);

            Vector2[] Data = GetScaleValuesOverTime();

            Vector4[] Out = new Vector4[NumberOfSeeds * NumberOfValuesOverTime];

            for (int i = 0; i < Data.Length; i++)
            {
                Out[i] = new Vector4(Data[i].X, Data[i].Y, 0f, 0f);
            }

            OutTex.SetData(Out);

            return OutTex;
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
