using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    [SerializeField] private CityVisual cityVisual;
    private Grid grid; 
    // Start is called before the first frame update
    private void Start()
    {
        Grid map= new Grid(100, 100, 10f, Vector3.zero);

        cityVisual.SetGrid(map);
    }
}
