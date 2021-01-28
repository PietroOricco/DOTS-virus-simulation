using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class Death_counter : MonoBehaviour
{
    public static Text counterText;
    // Start is called before the first frame update
    void Start()
    {
        counterText = GetComponent<Text>();
    }



    // Update is called once per frame
    void Update()
    {
        counterText.text = "Deaths: " + Interlocked.Read(ref ContagionSystem.deathCounter); ;
    }
}
