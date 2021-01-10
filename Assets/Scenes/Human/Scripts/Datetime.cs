using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Datetime : MonoBehaviour
{
    public float REAL_SECONDS_PER_INGAME_MINUTE = 1f;
    private float total_minutes = 0;
    public static Text datetimeText;

    // Start is called before the first frame update
    void Start()
    {
        datetimeText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        total_minutes += Time.deltaTime / REAL_SECONDS_PER_INGAME_MINUTE;
        int days = Mathf.FloorToInt(total_minutes/(24*60));
        int hours = Mathf.FloorToInt((total_minutes%(24*60))/60);
        int minutes = Mathf.FloorToInt(((total_minutes%(24*60))%60));
        datetimeText.text = "Time passed: "+days+"d "+hours+"h "+minutes+"m";

    }
}
