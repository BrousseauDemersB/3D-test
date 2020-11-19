using System;

namespace Experiment.Cache
{
    public class FloatCache
    {
        private static Random Random = new Random();
        private int CacheSize = 2000;

        private readonly float[] ArrayOffset;
        private int CacheIndex;

        public FloatCache(float MinValue, float MaxValue)
        {
            CacheIndex = 0;

            float VarianceX = MaxValue - MinValue;

            if (VarianceX != 0)
            {
                ArrayOffset = new float[CacheSize];
                for (int i = CacheSize - 1; i >= 0; --i)
                {
                    ArrayOffset[i] = MinValue + (float)Random.NextDouble() * VarianceX;
                }
            }
            else
            {
                ArrayOffset = new float[] { MaxValue };
                CacheSize = 1;
            }
        }

        public float GetNextFloat()
        {
            if (CacheIndex >= CacheSize)
                CacheIndex = 0;

            return ArrayOffset[CacheIndex++];
        }
    }
}
