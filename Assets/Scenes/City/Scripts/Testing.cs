using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapEnum;

//Sort of "Main" function to create and visualize the city map
public class Testing : MonoBehaviour{
    public static Testing Instance { private set; get; }

    [SerializeField] private CityVisual cityVisual;
    public Grid<GridNode> grid;

    private void Awake() {
        Instance = this;
    }

    // Start is called before the first frame update
    //Create and visualize the Map
    private void Start()
    {
        //num cells x, y, size, offset, element
        grid = new Grid<GridNode>(45, 45, 10f, Vector3.zero, (TileMapSprite tileType, Grid<GridNode> grid, int x, int y) => new GridNode(tileType, grid, x, y));
        cityVisual.SetGrid(grid);
    }
}
