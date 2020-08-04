using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct PlagueComponent : IComponentData
{
    public Boolean infected;
    public int infectionProb;
}
