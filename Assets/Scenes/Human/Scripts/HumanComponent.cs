using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct HumanComponent : IComponentData
{
    //status
    public enum need { 
        needForFood,
        needForSport,
        needForSociality,
        needToRest,
        none
    }
    // human needs
    public bool goingToNeedPlace;
    public need status;
    public float hunger;        //supermarket
    public float sportivity;    //park 
    public float sociality;     //pub
    public float fatigue;       //home
    // social behavior
}
