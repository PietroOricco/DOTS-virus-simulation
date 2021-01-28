using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


public class Recovered_counter : MonoBehaviour
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
        counterText.text = "Recovered: " + Interlocked.Read(ref ContagionSystem.recoveredCounter); ;

    }
}
