using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using System;
using Unity.Mathematics;

public class Human : MonoBehaviour
{
    NativeArray<Entity> entityArray;

    [SerializeField] private Mesh mesh;
    [SerializeField] private Material healthyMaterial;
    [SerializeField] private Material sickMaterial;
    private void Start(){
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(HumanComponent),
            typeof(PlagueComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(MoveSpeedComponent),
            typeof(PathFollow)
        );

        entityArray = new NativeArray<Entity>(10000, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);


        for (int i = 0; i < entityArray.Length; i++){
            Entity entity = entityArray[i];

            entityManager.AddBuffer<PathPosition>(entity);

            //human component
            entityManager.SetComponentData(entity, new HumanComponent
            {
                hunger = 50,
                sportivity = 50,
                sociality = 50,
                fatigue = 50
            });

            //plague component
            entityManager.SetComponentData(entity, new PlagueComponent { infected = false, infectionExposure=0, infectionThreshold=3 });

            //speed
            entityManager.SetComponentData(entity, new MoveSpeedComponent { moveSpeedY = UnityEngine.Random.Range(-2f, 2f), moveSpeedX = UnityEngine.Random.Range(-2f, 2f), });

            //initial position
            entityManager.SetComponentData(entity, new Translation{
                Value = new float3((UnityEngine.Random.Range(0, 100 / 3)) * 30f + 10f + UnityEngine.Random.Range(0, 10f), (1 + UnityEngine.Random.Range(0, 100 / 3)) * 30f + 10f + UnityEngine.Random.Range(0, 10f), 0)
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
        entityArchetype = entityManager.CreateArchetype(
            typeof(HumanComponent),
            typeof(InfectionComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(RenderBounds),
            typeof(MoveSpeedComponent)
        );
        Entity entitySick = entityManager.CreateEntity(entityArchetype);
        entityManager.SetComponentData(entitySick, new HumanComponent
        {
            hunger = 50,
            sportivity = 50,
            sociality = 50,
            fatigue = 50
        });

        //Infection component
        entityManager.SetComponentData(entitySick, new InfectionComponent { infected = true });

        //speed
        entityManager.SetComponentData(entitySick, new MoveSpeedComponent { moveSpeedY = UnityEngine.Random.Range(-2f, 2f), moveSpeedX = UnityEngine.Random.Range(-2f, 2f), });

        //initial position
        entityManager.SetComponentData(entitySick, new Translation
        {
            Value = new float3((UnityEngine.Random.Range(0, 100 / 3)) * 30f + 10f + UnityEngine.Random.Range(0, 10f), (1 + UnityEngine.Random.Range(0, 100 / 3)) * 30f + 10f + UnityEngine.Random.Range(0, 10f), 0)
        });

        //graphics
        entityManager.SetSharedComponentData(entitySick, new RenderMesh
        {
            mesh = mesh,
            material = sickMaterial,
        });
    }

}
