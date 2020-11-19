using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Experiment.Cache
{
    public class Vector2ValuesOverTime
    {
        private const int NumberOfValuesOverTime = 20;
        private Vector2[] ArrayValues;

        public Vector2ValuesOverTime(float LifetimeInSeconds, Vector2 MinValue, Vector2 MaxValue, List<ValueAtTime> ListValueAtTime)
        {
            ListValueAtTime = ListValueAtTime.OrderBy(x => x.Time).ToList();
            ArrayValues = new Vector2[NumberOfValuesOverTime];
            int StartIndex = 0;
            Vector2 ValueInterval = MaxValue - MinValue;

            Vector2 StartValue = MinValue;

            if (ListValueAtTime.Count > 0 && ListValueAtTime[0].Time == 0f)
            {
                StartValue = ListValueAtTime[0].PercentValue * MinValue;
            }

            for (int V = 0; V < ListValueAtTime.Count; V++)
            {
                Vector2 FinalTimeValue = (ListValueAtTime[V].PercentValue * ValueInterval) + MinValue;
                int EndIndex;

                if (V == ListValueAtTime.Count - 1 || ListValueAtTime[V].Time == LifetimeInSeconds)
                {
                    EndIndex = ArrayValues.Length;
                }
                else
                {
                    EndIndex = (int)(ListValueAtTime[V].Time / LifetimeInSeconds * ArrayValues.Length);
                }

                ComputeValues(StartIndex, EndIndex, StartValue, FinalTimeValue);

                StartIndex = EndIndex;
                StartValue = FinalTimeValue;
            }

            if (StartIndex != ArrayValues.Length - 1)
            {
                ComputeValues(StartIndex, ArrayValues.Length, MinValue, MaxValue);
            }
        }

        private void ComputeValues(int StartIndex, int EndIndex, Vector2 StartValue, Vector2 EndValue)
        {
            int IndexInterval = EndIndex - StartIndex;
            Vector2 ValueInterval = StartValue - EndValue;

            for (int i = StartIndex; i < EndIndex; ++i)
            {
                Vector2 NormalizedFinalTimeValue = StartValue + ValueInterval * i / IndexInterval;

                ArrayValues[i] = NormalizedFinalTimeValue;
            }
        }

        public Vector2[] GetValuesOverTime()
        {
            return ArrayValues;
        }

        public struct ValueAtTime
        {
            public float Time;
            public Vector2 PercentValue;

            public ValueAtTime(float Time, Vector2 PercentValue)
            {
                this.Time = Time;
                this.PercentValue = PercentValue;
            }
        }
    }
}
