using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public enum Status
{
    susceptible,
    exposed,
    infectious,
    recovered,
    removed
}
public struct InfectionComponent : IComponentData
{
    public Boolean infected;
    public Boolean symptomatic; 
    
    public Status status;

    public float contagionCounter;
    public float infectiousCounter; // counter to track infection exposure, if > threshold human become infected
    public float exposedCounter;
    public float recoveredCounter;

    public float globalSymptomsProbability;
    public float globalDeathProbability;
    
    public float humanSymptomsProbability;
    public float humanDeathProbability;

    public float infectiousThreshold;
    public float exposedThreshold;
    public float recoveredThreshold;
}
