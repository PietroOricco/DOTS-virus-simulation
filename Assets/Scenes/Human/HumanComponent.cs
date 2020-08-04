using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct HumanComponent : IComponentData
{
    // Start is called before the first frame update
    public float hunger;
    public float sportivity;
    public float sociality;
    public float fatigue;
}
