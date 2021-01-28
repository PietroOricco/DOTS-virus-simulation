﻿using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class Asynthomatic_counter : MonoBehaviour
{
    public static int asynthomatic = 0;
    public static Text counterText;

    // Start is called before the first frame update
    void Start()
    {
        counterText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        counterText.text = "Asynthomatic: " + Interlocked.Read(ref ContagionSystem.asymptomaticCounter); ;

    }
}
