﻿using System.IO;
using UnityEngine;

[System.Serializable]
public class Configuration
{
    public int numberOfHumans;
    public int numberOfInfects;

    public static Configuration CreateFromJSON()
    {
        string text = File.ReadAllText("./Conf/conf.txt");
        Debug.Log(text);
        return JsonUtility.FromJson<Configuration>(text);
    }
}