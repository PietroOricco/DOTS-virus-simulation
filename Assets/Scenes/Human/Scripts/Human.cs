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

    private void Awake() {
        Instance = this;
    }
    private void Start(){
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(HumanComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(MoveSpeedComponent),
            typeof(PathFollow)
        );

        entityArray = new NativeArray<Entity>(1, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);


        for (int i = 0; i < entityArray.Length; i++){
            Entity entity = entityArray[i];

            entityManager.AddBuffer<PathPosition>(entity);

            //human component
            entityManager.SetComponentData(entity, new HumanComponent
            {
                status = HumanComponent.need.none,
                hunger = UnityEngine.Random.Range(0, 100f),
                sportivity = UnityEngine.Random.Range(0, 100f),
                sociality = UnityEngine.Random.Range(0, 100f),
                fatigue = UnityEngine.Random.Range(0, 100f)
            }); ;

            //plague component
            if(UnityEngine.Random.Range(0, 10f)>7){
                entityManager.AddComponentData(entity, new InfectionComponent{
                    infected=true
                });
            } 

            //speed
            entityManager.SetComponentData(entity, new MoveSpeedComponent { moveSpeedY = UnityEngine.Random.Range(-2f, 2f), moveSpeedX = UnityEngine.Random.Range(-2f, 2f), });

            //initial position
            entityManager.SetComponentData(entity, new Translation{
                Value = new float3((UnityEngine.Random.Range(0, 45 / 3)) * 30f + 10f + UnityEngine.Random.Range(0, 10f), (UnityEngine.Random.Range(0, 45 / 3)) * 30f + 10f + UnityEngine.Random.Range(0, 10f), 0)
            });

            //initial position
            entityManager.SetComponentData(entity, new PathFollow { 
                pathIndex = -1 
            });

            //graphics
            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = mesh,
                material = healthyMaterial,
            });
        }

        entityArray.Dispose();
    }
}
