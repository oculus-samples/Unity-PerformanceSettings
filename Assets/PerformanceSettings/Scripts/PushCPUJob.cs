// Copyright (c) Meta Platforms, Inc. and affiliates.

using Unity.Collections;
using Unity.Jobs;

namespace Meta.PerformanceSettings
{
    public struct PushCPUJob : IJobParallelFor
    {
        public float NumCycles;
        public NativeArray<int> Result;

        public void Execute(int x)
        {
            var ret = 0;
            for (var i = 0; i < NumCycles; ++i)
                if (i % 2 == 0)
                    ret += i;
                else
                    ret -= i;
            Result[x] = ret;
        }
    }
}
