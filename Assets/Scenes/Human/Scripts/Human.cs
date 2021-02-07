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
    public static Configuration conf;

    [SerializeField] public Mesh mesh;
    [SerializeField] public Material healthyMaterial;
    [SerializeField] public Material sickMaterial;

    [SerializeField] public Material humanSpriteMaterial;
    public NativeArray<Vector2Int> houses;

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
        conf = Configuration.CreateFromJSON();
        int numberOfInfects = conf.numberOfInfects;
        //Population_counter.population = conf.numberOfHumans;
        entityArray = new NativeArray<Entity>(conf.numberOfHumans, Allocator.Temp);
        entityManager.CreateEntity(entityArchetype, entityArray);

        // Get grid size
        int gridWidth = Testing.Instance.grid.GetWidth();
        int gridHeight = Testing.Instance.grid.GetHeight();

        // Get houses
        List<Vector2Int> housesList = new List<Vector2Int>();
        var mapGrid = Testing.Instance.grid.GetGridByValue((GridNode gn)=>{return gn.GetTileType();});
        for(int i = 0; i < gridWidth; i++){
            for(int j = 0; j < gridHeight; j++){
                if(mapGrid[i+j*gridWidth]==TileMapEnum.TileMapSprite.Home||mapGrid[i+j*gridWidth]==TileMapEnum.TileMapSprite.Home2){
                    housesList.Add(new Vector2Int(i, j));
                }
            }
        }
        houses = housesList.ToNativeArray<Vector2Int>(Allocator.Persistent);

        //TODO model social responsibility
        for (int i = 0; i < entityArray.Length; i++){
            Entity entity = entityArray[i];
            var homePosition = houses[UnityEngine.Random.Range(0, houses.Length)];

            var friendPositions = new List<Vector2Int>();
            for(int h=0; h<UnityEngine.Random.Range(3,5); h++){
                friendPositions.Add(houses[UnityEngine.Random.Range(0, houses.Length)]);
            }

            //Vector3 position = new float3((UnityEngine.Random.Range(0, gridWidth)) * 10f + UnityEngine.Random.Range(0, 10f), (UnityEngine.Random.Range(0, gridHeight)) * 10f + UnityEngine.Random.Range(0, 10f), 0);
            Vector3 position = new float3(homePosition.x * 10f + UnityEngine.Random.Range(0, 10f), homePosition.y * 10f + UnityEngine.Random.Range(0, 10f), 0);

            entityManager.AddBuffer<PathPosition>(entity);

            //human component
            entityManager.SetComponentData(entity, new HumanComponent
            {
                hunger = UnityEngine.Random.Range(0, 10*60),
                sportivity = UnityEngine.Random.Range(0, 10 * 60),
                sociality = UnityEngine.Random.Range(0, 10 * 60),
                fatigue = UnityEngine.Random.Range(0, 10 * 60),
                socialResposibility = UnityEngine.Random.Range(0, 100f) / 100f,
                homePosition = homePosition
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
                if (symptomsProbability > 1 - conf.probabilityOfSymptomatic)
                {
                    mean = (0.5f + 1.5f) * 60 * 24 / 2;
                    sigma = (1.5f * 60 * 24 - mean) / 3;
                }
                else
                {
                    mean = (conf.minDaysInfectious + conf.maxDaysInfectious) * 60 * 24 / 2;
                    sigma = (conf.maxDaysInfectious * 60 * 24 - mean) / 3;
                }

                infectiousThreshold = GenerateNormalRandom(mean, sigma, conf.minDaysInfectious * 24 * 60, conf.maxDaysInfectious * 24 * 60);

                float humanDeathProbability = UnityEngine.Random.Range(0, 100);
                if (humanDeathProbability <= 1 - conf.probabilityOfDeath)
                {
                    mean = (conf.minDaysRecovered + conf.maxDaysRecovered) * 60 * 24 / 2;
                    sigma = (conf.maxDaysRecovered * 60 * 24 - mean) / 3;
                }

                recoveredThreshold = GenerateNormalRandom(mean, sigma, conf.minDaysRecovered * 24 * 60, conf.maxDaysRecovered * 24 * 60);

                mean = (conf.minDaysExposed + conf.maxDaysExposed) * 60 * 24 / 2;
                sigma = (conf.maxDaysExposed * 60 * 24 - mean) / 3;

                exposedThreshold = GenerateNormalRandom(mean, sigma, conf.minDaysExposed * 60 * 24, conf.maxDaysExposed * 60 * 24);

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
                Counter.initialInfectedCounter++;
                synthomatic_counter.initialSymptomaticCounter++;
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
                    mean = (conf.minDaysInfectious + conf.maxDaysInfectious) * 60 * 24 / 2;
                    sigma = (conf.maxDaysInfectious * 60 * 24 - mean) / 3;
                }

                infectiousThreshold = GenerateNormalRandom(mean, sigma, conf.minDaysInfectious * 24 * 60, conf.maxDaysInfectious * 24 * 60);

                float humanDeathProbability = UnityEngine.Random.Range(0, 100);
                if (humanDeathProbability <= 1 - conf.probabilityOfDeath)
                {
                    mean = (conf.minDaysRecovered + conf.maxDaysRecovered) * 60 * 24 / 2;
                    sigma = (conf.maxDaysRecovered * 60 * 24 - mean) / 3;
                }

                recoveredThreshold = GenerateNormalRandom(mean, sigma, conf.minDaysRecovered * 24 * 60, conf.maxDaysRecovered * 24 * 60);

                mean = (conf.minDaysExposed + conf.maxDaysExposed) * 60 * 24 / 2;
                sigma = (conf.maxDaysExposed * 60 * 24 - mean) / 3;

                exposedThreshold = GenerateNormalRandom(mean, sigma, conf.minDaysExposed * 60 * 24, conf.maxDaysExposed * 60 * 24);

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
        mapGrid.Dispose();
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
