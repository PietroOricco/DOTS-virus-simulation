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
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

// Old System, Single Threaded
[UpdateAfter(typeof(SpriteSheetAnimation_Animate))]
[DisableAutoCreation]
public class SpriteSheetRenderer_Old : ComponentSystem {

    protected override void OnUpdate() {
        EntityQuery entityQuery = GetEntityQuery(typeof(Translation), typeof(SpriteSheetAnimation_Data));

        NativeArray<SpriteSheetAnimation_Data> animationDataArray = entityQuery.ToComponentDataArray<SpriteSheetAnimation_Data>(Allocator.TempJob);
        NativeArray<Translation> translationArray = entityQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        for (int i = 0; i < translationArray.Length; i++) {
            for (int j = i+1; j < translationArray.Length; j++) {
                if (translationArray[i].Value.y < translationArray[j].Value.y) {
                    // Swap
                    Translation tmp = translationArray[i];
                    translationArray[i] = translationArray[j];
                    translationArray[j] = tmp;
                    
                    SpriteSheetAnimation_Data tmpAnim = animationDataArray[i];
                    animationDataArray[i] = animationDataArray[j];
                    animationDataArray[j] = tmpAnim;
                }
            }
        }

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        Vector4[] uv = new Vector4[1];
        Camera camera = Camera.main;
        Mesh quadMesh = GameHandler.GetInstance().quadMesh;
        Material material = GameHandler.GetInstance().walkingSpriteSheetMaterial;
        int shaderPropertyId = Shader.PropertyToID("_MainTex_UV");

        int sliceCount = 1023;
        for (int i = 0; i < animationDataArray.Length; i+=sliceCount) {
            int sliceSize = math.min(animationDataArray.Length - i, sliceCount);

            List<Matrix4x4> matrixList = new List<Matrix4x4>();
            List<Vector4> uvList = new List<Vector4>();
            for (int j = 0; j < sliceSize; j++) {
                SpriteSheetAnimation_Data spriteSheetAnimationData = animationDataArray[i + j];
                matrixList.Add(spriteSheetAnimationData.matrix);
                uvList.Add(spriteSheetAnimationData.uv);
            }

            materialPropertyBlock.SetVectorArray(shaderPropertyId, uvList);

            Graphics.DrawMeshInstanced(
                quadMesh,
                0,
                material,
                matrixList,
                materialPropertyBlock
            );
        }

        animationDataArray.Dispose();
        translationArray.Dispose();
    }

}
*/