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
        float mean, sigma;

        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(HumanComponent),
            typeof(Translation),
            typeof(MoveSpeedComponent),
            typeof(PathFollow),
            typeof(QuadrantEntity),
            typeof(SpriteSheetAnimation_Data)
        );

        //Extract configuration from json file
        Configuration conf = Configuration.CreateFromJSON();
        int numberOfInfects = conf.numberOfInfects;
        Population_counter.population = conf.numberOfHumans;
        entityArray = new NativeArray<Entity>(conf.numberOfHumans, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        //TODO model social responsibility
        for (int i = 0; i < entityArray.Length; i++){
            Entity entity = entityArray[i];

            Vector3 position = new float3((UnityEngine.Random.Range(0, 20)) * 10f + UnityEngine.Random.Range(0, 10f), (UnityEngine.Random.Range(0, 15)) * 10f + UnityEngine.Random.Range(0, 10f), 0);

            entityManager.AddBuffer<PathPosition>(entity);

            //human component
            entityManager.SetComponentData(entity, new HumanComponent
            {
                hunger = UnityEngine.Random.Range(0, 10*60),
                sportivity = UnityEngine.Random.Range(0, 10 * 60),
                sociality = UnityEngine.Random.Range(0, 10 * 60),
                fatigue = UnityEngine.Random.Range(0, 10 * 60),
                socialResposibility = UnityEngine.Random.Range(0, 100f) / 100f,
            }) ;

            //Time Scale
            Time.timeScale = conf.timeScale;

            //components depending on infection
            float uvWidth = 1f;
            float uvHeight = 1f/5;
            float uvOffsetX = 0f;
            
            if(numberOfInfects > 0){
                numberOfInfects--;

                float symptomsProbability = UnityEngine.Random.Range(0, 100);
                float infectiousThreshold, recoveredThreshold, exposedThreshold;
                if(symptomsProbability > 1 - conf.probabilityOfSymptomatic)
                {
                    mean = (0.5f + 1.5f) * 60 * 24 / 2;
                    sigma = (1.5f * 60 * 24 - mean) / 3;
                }
                else
                {
                    mean = (2 + 4) * 60 * 24 / 2;
                    sigma = (4 * 60 * 24 - mean) / 3;
                }

                infectiousThreshold = GenerateNormalRandom(mean, sigma, 2 * 24 * 60, 4 * 24 * 60);

                float humanDeathProbability = UnityEngine.Random.Range(0, 100);
                if (humanDeathProbability <= 1 - conf.probabilityOfDeath)
                {
                    mean = (30 + 45) * 60 * 24 / 2;
                    sigma = (45 * 60 * 24 - mean) / 3;
                }

                recoveredThreshold = GenerateNormalRandom(mean, sigma, 30 * 24 * 60, 45 * 24 * 60);

                mean = (3 + 6) * 60 * 24 / 2;
                sigma = (6 * 60 * 24 - mean) / 3;

                exposedThreshold = GenerateNormalRandom(mean, sigma, 3 * 60 * 24, 6 * 60 * 24);

                entityManager.AddComponentData(entity, new InfectionComponent{//TODO add to archetype
                    infected=true,
                    status = Status.infectious,
                    contagionCounter = 0,
                    infectiousCounter = 0,
                    exposedCounter = 0,
                    recoveredCounter = 0,
                    symptomatic = true,

                    globalSymptomsProbability = conf.probabilityOfSymptomatic,
                    globalDeathProbability = conf.probabilityOfDeath,

                    humanSymptomsProbability = symptomsProbability,
                    humanDeathProbability = humanDeathProbability,

                    infectiousThreshold = infectiousThreshold,
                    exposedThreshold = exposedThreshold,
                    recoveredThreshold = recoveredThreshold
                });
                Counter.infectedCounter++;
                synthomatic_counter.synthomatic++;
                //graphics
                float uvOffsetY = 0.0f;
                SpriteSheetAnimation_Data spriteSheetAnimationData;
                spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
                spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                //quadrant
                entityManager.SetComponentData(entity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.exposed });
            }
            else{

                float symptomsProbability = UnityEngine.Random.Range(0, 100);
                float infectiousThreshold, recoveredThreshold, exposedThreshold;
                if (symptomsProbability > 1 - conf.probabilityOfSymptomatic)
                {
                    mean = (0.5f + 1.5f) * 60 * 24 / 2;
                    sigma = (1.5f * 60 * 24 - mean) / 3;
                }
                else
                {
                    mean = (2 + 4) * 60 * 24 / 2;
                    sigma = (4 * 60 * 24 - mean) / 3;
                }

                infectiousThreshold = GenerateNormalRandom(mean, sigma, 2 * 24 * 60, 4 * 24 * 60);

                float humanDeathProbability = UnityEngine.Random.Range(0, 100);
                if (humanDeathProbability <= 1 - conf.probabilityOfDeath)
                {
                    mean = (30 + 45) * 60 * 24 / 2;
                    sigma = (45 * 60 * 24 - mean) / 3;
                }

                recoveredThreshold = GenerateNormalRandom(mean, sigma, 30 * 24 * 60, 45 * 24 * 60);

                mean = (3 + 6) * 60 * 24 / 2;
                sigma = (6 * 60 * 24 - mean) / 3;

                exposedThreshold = GenerateNormalRandom(mean, sigma, 3 * 60 * 24, 6 * 60 * 24);

                entityManager.AddComponentData(entity, new InfectionComponent{
                    infected=false,
                    status = Status.susceptible,
                    contagionCounter = 0,
                    infectiousCounter = 0,
                    exposedCounter = 0,
                    recoveredCounter = 0,

                    globalSymptomsProbability = conf.probabilityOfSymptomatic,
                    globalDeathProbability = conf.probabilityOfDeath,

                    humanSymptomsProbability = symptomsProbability,
                    humanDeathProbability = humanDeathProbability,

                    infectiousThreshold = infectiousThreshold,
                    exposedThreshold = exposedThreshold,
                    recoveredThreshold = recoveredThreshold
                });
                //graphics
                float uvOffsetY = 0.2f;
                SpriteSheetAnimation_Data spriteSheetAnimationData;
                spriteSheetAnimationData.uv = new Vector4(uvWidth, uvHeight, uvOffsetX, uvOffsetY);
                spriteSheetAnimationData.matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
                //quadrant
                entityManager.SetComponentData(entity, new QuadrantEntity { typeEnum = QuadrantEntity.TypeEnum.susceptible });
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

    public static float GenerateNormalRandom(float mean, float sigma, float min, float max)
    {
        float rand1 = UnityEngine.Random.Range(0.0f, 1.0f);
        float rand2 = UnityEngine.Random.Range(0.0f, 1.0f);

        float n = Mathf.Sqrt(-2.0f * Mathf.Log(rand1)) * Mathf.Cos((2.0f * Mathf.PI) * rand2);

        float generatedNumber = (mean + sigma * n);

        generatedNumber = Mathf.Clamp(generatedNumber, min, max);

        return generatedNumber;
    }
}
