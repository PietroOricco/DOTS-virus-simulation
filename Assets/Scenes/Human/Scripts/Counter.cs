﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Counter : MonoBehaviour
{

    public static int infectedCounter=0;
    public static Text counterText;
    // Start is called before the first frame update
    void Start()
    {
        counterText = GetComponent<Text>();
    }

   

    // Update is called once per frame
    void Update()
    {
         counterText.text = "Exposed: " + infectedCounter;
    }
}