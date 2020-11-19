using System;
using Microsoft.Xna.Framework;

namespace Experiment.Cache
{
    public class Vector3Cache
    {
        private static Random Random = new Random();
        private int CacheSize = 2000;

        private readonly Vector3[] ArrayOffset;
        private int CacheIndex;

        public Vector3Cache(Vector3 MinValue, Vector3 MaxValue)
        {
            ArrayOffset = new Vector3[CacheSize];
            CacheIndex = 0;

            float VarianceX = MaxValue.X - MinValue.X;
            float VarianceY = MaxValue.Y - MinValue.Y;
            float VarianceZ = MaxValue.Z - MinValue.Z;

            if (VarianceX == 0f && VarianceY == 0f && VarianceZ == 0f)
            {
                ArrayOffset = new Vector3[] { MaxValue };
                CacheSize = 1;
                return;
            }

            if (VarianceX != 0)
            {
                for (int i = CacheSize - 1; i >= 0; --i)
                {
                    ArrayOffset[i].X = MinValue.X + (float)Random.NextDouble() * VarianceX;
                }
            }
            else
            {
                for (int i = CacheSize - 1; i >= 0; --i)
                {
                    ArrayOffset[i].X = MaxValue.X;
                }
            }

            if (VarianceY != 0)
            {
                for (int i = CacheSize - 1; i >= 0; --i)
                {
                    ArrayOffset[i].Y = MinValue.Y + (float)Random.NextDouble() * VarianceY;
                }
            }
            else
            {
                for (int i = CacheSize - 1; i >= 0; --i)
                {
                    ArrayOffset[i].Y = MaxValue.Y;
                }
            }

            if (VarianceZ != 0)
            {
                for (int i = CacheSize - 1; i >= 0; --i)
                {
                    ArrayOffset[i].Z = MinValue.Z + (float)Random.NextDouble() * VarianceZ;
                }
            }
            else
            {
                for (int i = CacheSize - 1; i >= 0; --i)
                {
                    ArrayOffset[i].Z = MaxValue.Z;
                }
            }
        }

        public Vector3 GetNextVector3()
        {
            if (CacheIndex >= CacheSize)
                CacheIndex = 0;

            return ArrayOffset[CacheIndex++];
        }
    }
}
