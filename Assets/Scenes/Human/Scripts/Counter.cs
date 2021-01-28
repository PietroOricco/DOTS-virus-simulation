using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class Counter : MonoBehaviour
{
    public static int initialInfectedCounter=0;
    public static Text counterText;
    // Start is called before the first frame update
    void Start()
    {
        counterText = GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        counterText.text = "Exposed: " + Interlocked.Read(ref ContagionSystem.infectedCounter);
    }
}
