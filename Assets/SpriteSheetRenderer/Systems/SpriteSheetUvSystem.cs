﻿using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
public class SpriteSheetUvJobSystem : JobComponentSystem
{
    [BurstCompile]
    private struct UpdateJobChunk : IJobChunk
    {
        [NativeDisableParallelForRestriction]
        public DynamicBuffer<SpriteIndexBuffer> indexBuffer;
        [ReadOnly]
        public int bufferEntityID;

        [ReadOnly]
        public ComponentTypeHandle<SpriteIndex> data;
        [ReadOnly]
        public ComponentTypeHandle<BufferHook> hook;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var chunkSpriteIndices = chunk.GetNativeArray(data);
            var chunkBufferHooks = chunk.GetNativeArray(hook);

            for (int i = 0; i < chunk.Count; i++)
            {
                if (bufferEntityID == chunkBufferHooks[i].bufferEntityID)
                {
                    indexBuffer[chunkBufferHooks[i].bufferID] = chunkSpriteIndices[i].Value;
                }
            }
        }
    }

    private EntityQuery m_EntityQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_EntityQuery = GetEntityQuery(
            ComponentType.ReadOnly<SpriteIndex>(),
            ComponentType.ReadOnly<BufferHook>());
        m_EntityQuery.SetChangedVersionFilter(ComponentType.ReadOnly<SpriteIndex>());

    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var buffers = DynamicBufferManager.GetIndexBuffers();
        NativeArray<JobHandle> jobs = new NativeArray<JobHandle>(buffers.Length, Allocator.Temp);
        for (int i = 0; i < buffers.Length; i++)
        {
            inputDeps = new UpdateJobChunk
            {
                indexBuffer = buffers[i],
                bufferEntityID = i,
                data = GetComponentTypeHandle<SpriteIndex>(isReadOnly: true),
                hook = GetComponentTypeHandle<BufferHook>(isReadOnly: true)
            }.Schedule(m_EntityQuery, inputDeps);
            jobs[i] = inputDeps;
        }
        JobHandle.CompleteAll(jobs);
        jobs.Dispose();
        return inputDeps;
    }
}