using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct HumanComponent : IComponentData
{
    // human needs
    public float hunger;        //supermarket
    public float sportivity;    //park 
    public float sociality;     //pub
    public float fatigue;       //home
    // social behavior
}
