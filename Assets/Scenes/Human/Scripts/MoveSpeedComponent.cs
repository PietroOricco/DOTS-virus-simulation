﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct MoveSpeedComponent : IComponentData
{
    public float moveSpeedY;
    public float moveSpeedX;
}