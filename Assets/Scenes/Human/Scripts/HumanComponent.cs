using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;

public struct HumanComponent : IComponentData
{
    // human needs
    public float hunger;        
    public float sportivity;     
    public float sociality;     
    public float fatigue;

    public float grocery;
    public float work;
    // social behavior
    public float socialResposibility;
    //home
    public Vector2Int homePosition;
    public Vector2Int officePosition;
}
