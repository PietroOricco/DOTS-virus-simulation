/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */
/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

// Basic System, split into just two slices
[UpdateAfter(typeof(SpriteSheetAnimation_Animate))]
[DisableAutoCreation]
public class SpriteSheetRenderer_Basic : ComponentSystem {

    private struct RenderData {
        public Entity entity;
        public float3 position;
        public Matrix4x4 matrix;
        public Vector4 uv;
    }

    [BurstCompile]
    private struct CullAndSortJob : IJobForEachWithEntity<Translation, SpriteSheetAnimation_Data> {

        public float yTop_1; // Top most cull position
        public float yTop_2; // Second slice from top
        public float yBottom; // Bottom most cull position

        public NativeQueue<RenderData>.ParallelWriter nativeQueue_1;
        public NativeQueue<RenderData>.ParallelWriter nativeQueue_2;

        public void Execute(Entity entity, int index, ref Translation translation, ref SpriteSheetAnimation_Data spriteSheetAnimationData) {
            float positionY = translation.Value.y;
            if (positionY > yBottom && positionY < yTop_1) {
                // Valid position
                RenderData renderData = new RenderData {
                    entity = entity,
                    position = translation.Value,
                    matrix = spriteSheetAnimationData.matrix,
                    uv = spriteSheetAnimationData.uv
                };

                if (positionY < yTop_2) {
                    nativeQueue_2.Enqueue(renderData);
                } else {
                    nativeQueue_1.Enqueue(renderData);
                }
            }
        }

    }
    
    [BurstCompile]
    private struct NativeQueueToArrayJob : IJob {

        public NativeQueue<RenderData> nativeQueue;
        public NativeArray<RenderData> nativeArray;

        public void Execute() {
            int index = 0;
            RenderData renderData;
            while (nativeQueue.TryDequeue(out renderData)) {
                nativeArray[index] = renderData;
                index++;
            }
        }

    }
    
    [BurstCompile]
    private struct SortByPositionJob : IJob {

        public NativeArray<RenderData> sortArray;

        public void Execute() {
            for (int i = 0; i < sortArray.Length; i++) {
                for (int j = i+1; j < sortArray.Length; j++) {
                    if (sortArray[i].position.y < sortArray[j].position.y) {
                        // Swap
                        RenderData tmp = sortArray[i];
                        sortArray[i] = sortArray[j];
                        sortArray[j] = tmp;
                    }
                }
            }
        }
    }
    
    [BurstCompile]
    private struct FillArraysParallelJob : IJobParallelFor {

        [ReadOnly] public NativeArray<RenderData> nativeArray;
        [NativeDisableContainerSafetyRestriction] public NativeArray<Matrix4x4> matrixArray;
        [NativeDisableContainerSafetyRestriction] public NativeArray<Vector4> uvArray;
        public int startingIndex;

        public void Execute(int index) {
            RenderData renderData = nativeArray[index];
            matrixArray[startingIndex + index] = renderData.matrix;
            uvArray[startingIndex + index] = renderData.uv;
        }

    }


    protected override void OnUpdate() {
        NativeQueue<RenderData> nativeQueue_1 = new NativeQueue<RenderData>(Allocator.TempJob);
        NativeQueue<RenderData> nativeQueue_2 = new NativeQueue<RenderData>(Allocator.TempJob);

        Camera camera = Camera.main;
        float3 cameraPosition = camera.transform.position;
        float yBottom = cameraPosition.y - camera.orthographicSize;
        float yTop_1 = cameraPosition.y + camera.orthographicSize;
        float yTop_2 = cameraPosition.y + 0f;

        CullAndSortJob cullAndSortJob = new CullAndSortJob {
            yBottom = yBottom,
            yTop_1 = yTop_1,
            yTop_2 = yTop_2,

            nativeQueue_1 = nativeQueue_1.AsParallelWriter(),
            nativeQueue_2 = nativeQueue_2.AsParallelWriter(),
        };
        JobHandle jobHandle = cullAndSortJob.Schedule(this);
        jobHandle.Complete();

        // Convert Queues into Arrays for Sorting
        NativeArray<RenderData> nativeArray_1 = new NativeArray<RenderData>(nativeQueue_1.Count, Allocator.TempJob);
        NativeArray<RenderData> nativeArray_2 = new NativeArray<RenderData>(nativeQueue_2.Count, Allocator.TempJob);

        NativeArray<JobHandle> jobHandleArray = new NativeArray<JobHandle>(2, Allocator.TempJob);
        
        NativeQueueToArrayJob nativeQueueToArrayJob_1 = new NativeQueueToArrayJob {
            nativeQueue = nativeQueue_1,
            nativeArray = nativeArray_1,
        };
        jobHandleArray[0] = nativeQueueToArrayJob_1.Schedule();
        
        NativeQueueToArrayJob nativeQueueToArrayJob_2 = new NativeQueueToArrayJob {
            nativeQueue = nativeQueue_2,
            nativeArray = nativeArray_2,
        };
        jobHandleArray[1] = nativeQueueToArrayJob_2.Schedule();

        JobHandle.CompleteAll(jobHandleArray);

        nativeQueue_1.Dispose();
        nativeQueue_2.Dispose();

        // Sort arrays by position
        SortByPositionJob sortByPositionJob_1 = new SortByPositionJob {
            sortArray = nativeArray_1,
        };
        jobHandleArray[0] = sortByPositionJob_1.Schedule();
        
        SortByPositionJob sortByPositionJob_2 = new SortByPositionJob {
            sortArray = nativeArray_2,
        };
        jobHandleArray[1] = sortByPositionJob_2.Schedule();
        
        JobHandle.CompleteAll(jobHandleArray);

        int visibleEntityTotal = nativeArray_1.Length + nativeArray_2.Length;

        // Grab sliced arrays and merge them all into one
        NativeArray<Matrix4x4> matrixArray = new NativeArray<Matrix4x4>(visibleEntityTotal, Allocator.TempJob);
        NativeArray<Vector4> uvArray = new NativeArray<Vector4>(visibleEntityTotal, Allocator.TempJob);
        
        FillArraysParallelJob fillArraysParallelJob_1 = new FillArraysParallelJob {
            nativeArray = nativeArray_1,
            matrixArray = matrixArray,
            uvArray = uvArray,
            startingIndex = 0,
        };
        jobHandleArray[0] = fillArraysParallelJob_1.Schedule(nativeArray_1.Length, 10);
        
        FillArraysParallelJob fillArraysParallelJob_2 = new FillArraysParallelJob {
            nativeArray = nativeArray_2,
            matrixArray = matrixArray,
            uvArray = uvArray,
            startingIndex = nativeArray_1.Length,
        };
        jobHandleArray[1] = fillArraysParallelJob_2.Schedule(nativeArray_2.Length, 10);
        
        JobHandle.CompleteAll(jobHandleArray);
        
        jobHandleArray.Dispose();

        nativeArray_1.Dispose();
        nativeArray_2.Dispose();

        // Draw
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        Vector4[] uv = new Vector4[1];
        Mesh quadMesh = GameHandler.GetInstance().quadMesh;
        Material material = GameHandler.GetInstance().walkingSpriteSheetMaterial;
        int shaderPropertyId = Shader.PropertyToID("_MainTex_UV");

        int sliceCount = 1023;
        Matrix4x4[] matrixInstancedArray = new Matrix4x4[sliceCount];
        Vector4[] uvInstancedArray = new Vector4[sliceCount];

        for (int i = 0; i < visibleEntityTotal; i+=sliceCount) {
            int sliceSize = math.min(visibleEntityTotal - i, sliceCount);

            NativeArray<Matrix4x4>.Copy(matrixArray, i, matrixInstancedArray, 0, sliceSize);
            NativeArray<Vector4>.Copy(uvArray, i, uvInstancedArray, 0, sliceSize);

            materialPropertyBlock.SetVectorArray(shaderPropertyId, uvInstancedArray);

            Graphics.DrawMeshInstanced(
                quadMesh,
                0,
                material,
                matrixInstancedArray,
                sliceSize,
                materialPropertyBlock
            );
        }

        matrixArray.Dispose();
        uvArray.Dispose();
    }

}
*/