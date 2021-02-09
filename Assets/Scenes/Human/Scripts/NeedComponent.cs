using Unity.Entities;

public struct NeedComponent : IComponentData
{
    public NeedType currentNeed;
}

public enum NeedType { 
    needForFood,
    needForSport,
    needForSociality,
    needToRest,
    needToWork,
    needForGrocery,
    none
}