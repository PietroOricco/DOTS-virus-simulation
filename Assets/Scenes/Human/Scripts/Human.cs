using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using System;
using Unity.Mathematics;

public class Human : MonoBehaviour{
    public static Human Instance { private set; get; }
    NativeArray<Entity> entityArray;

    [SerializeField] public Mesh mesh;
    [SerializeField] public Material healthyMaterial;
    [SerializeField] public Material sickMaterial;

    [SerializeField] public Material humanSpriteMaterial;

    private void Awake() {
        Instance = this;
    }
    private void Start(){
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(HumanComponent),
            typeof(Translation),
            typeof(MoveSpeedComponent),
            typeof(PathFollow),
            typeof(QuadrantEntity),
            typeof(SpriteSheetAnimation_Data)
        );

        entityArray = new NativeArray<Entity>(100000, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        //TODO model social responsibility
        for (int i = 0; i < entityArray.Length; i++){
            Entity entity = entityArray[i];

            Vector3 position = new float3((UnityEngine.Random.Range(0, 45 / 3)) * 30f + 10f + UnityEngine.Random.Range(0, 10f), (UnityEngine.Random.Range(0, 45 / 3)) * 30f + 10f + UnityEngine.Random.Range(0, 10f), 0);

            entityManager.AddBuffer<PathPosition>(entity);

            //human component
            entityManager.SetComponentData(entity, new HumanComponent
            {
                hunger = UnityEngine.Random.Range(0, 100f),
                sportivity = UnityEngine.Random.Range(0, 100f),
                sociality = UnityEngine.Random.Range(0, 100f),
                fatigue = UnityEngine.Random.Range(0, 100f),
                socialResposibility = UnityEngine.Random.Range(0, 100f) / 100f,
                infectionCounter = 0
            }); ;

            //components depending on infection
            float uvWidth = 1f;
            float uvHeight = 1f/2;
            float uvOffsetX = 0f;
            if(UnityEngine.Random.Range(0, 1f)<0.0001){
                entityManager.AddComponentData(entity, new InfectionComponent{//TODO add to archetype
                    infected=true
                });
                //graphics
                float uvOffsetY = 0.0f;
                SpriteSheetAnimation_Data spriteSheetAnimationData;
                spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
                spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                //quadrant
                entityManager.SetComponentData(entity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.Sick });
            }
            else{
                entityManager.AddComponentData(entity, new InfectionComponent{
                    infected=false
                });
                //graphics
                float uvOffsetY = 0.5f;
                SpriteSheetAnimation_Data spriteSheetAnimationData;
                spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
                spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                //quadrant
                entityManager.SetComponentData(entity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.Healthy });
            }

            //speed
            entityManager.SetComponentData(entity, new MoveSpeedComponent { moveSpeedY = UnityEngine.Random.Range(0.5f, 2f), moveSpeedX = UnityEngine.Random.Range(0.5f, 2f), });

            //initial position
            entityManager.SetComponentData(entity, new Translation{
                Value = position
            });

            entityManager.SetComponentData(entity, new PathFollow { 
                pathIndex = -1 
            });
        }

        entityArray.Dispose();
    }
}
