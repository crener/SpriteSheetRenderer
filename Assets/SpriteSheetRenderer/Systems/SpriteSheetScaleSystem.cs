﻿using Unity.Entities;
using Unity.Transforms;

namespace ECSSpriteSheetAnimation
{
    public class SpriteSheetScaleSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Dependency = Entities
                .WithBurst()
                .WithChangeFilter<Scale>()
                .ForEach((ref SpriteMatrix renderData, in Scale scale) =>
                {
                    renderData.matrix.w = scale.Value;
                })
                .ScheduleParallel(Dependency);
        }
    } 
}
