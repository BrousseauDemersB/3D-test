using System;
using Microsoft.Xna.Framework;

namespace Experiment.Cache
{
    public class Vector2Cache
    {
        private static Random Random = new Random();
        private int CacheSize = 2000;

        private readonly Vector2[] ArrayOffset;
        private int CacheIndex;

        public Vector2Cache(Vector2 MinValue, Vector2 MaxValue)
            : this(MinValue, MaxValue, 2000)
        {
        }

        public Vector2Cache(Vector2 MinValue, Vector2 MaxValue, int CacheSize)
        {
            this.CacheSize = CacheSize;

            ArrayOffset = new Vector2[CacheSize];
            CacheIndex = 0;

            float VarianceX = MaxValue.X - MinValue.X;
            float VarianceY = MaxValue.Y - MinValue.Y;

            if (VarianceX == 0f && VarianceY == 0f)
            {
                ArrayOffset = new Vector2[] { MaxValue };
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
        }

        public Vector2 GetNextVector2()
        {
            if (CacheIndex >= CacheSize)
                CacheIndex = 0;

            return ArrayOffset[CacheIndex++];
        }
    }
}
