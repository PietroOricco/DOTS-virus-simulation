/* 
    ------------------- Code Monkey -------------------

    Thank you for downloading this package
    I hope you find it useful in your projects
    If you have any questions let me know
    Cheers!

               unitycodemonkey.com
    --------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapEnum;
using Unity.Collections;

//Class of the city map
public class Grid<TGridObject> {

    public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;
    public class OnGridObjectChangedEventArgs : EventArgs {
        public int x;
        public int y;
    }

    //params
    private int width;
    private int height;
    private float cellSize;
    private Vector3 originPosition;
    private TGridObject[,] gridArray;

    //constructor with generics
    public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<TileMapSprite, Grid<TGridObject>, int, int, TGridObject> createGridObject) {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.originPosition = originPosition;

        gridArray = new TGridObject[width, height];

        //populate the map, setting fixed roads and the other elements randomly
        for (int x = 0; x < gridArray.GetLength(0); x++){
            for (int y = 0; y < gridArray.GetLength(1); y++){
                if (x % 3 == 1) gridArray[x, y] = createGridObject(TileMapSprite.Road, this, x, y);
                else{
                    if (y % 3 == 1)
                        gridArray[x, y] = createGridObject(TileMapSprite.Road, this, x, y);
                    else{
                        // 1-> house, 2-> park, 3-> pub, 4->supermarket
                        int random = UnityEngine.Random.Range(1, 5);
                        switch(random){
                            case 1:
                                gridArray[x, y] = createGridObject(TileMapSprite.Home, this, x, y);
                                break;
                            case 2:
                                gridArray[x, y] = createGridObject(TileMapSprite.Park, this, x, y);
                                break;
                            case 3:
                                gridArray[x, y] = createGridObject(TileMapSprite.Pub, this, x, y);
                                break;
                            case 4:
                                gridArray[x, y] = createGridObject(TileMapSprite.Supermarket, this, x, y);
                                break;
                            default:
                                gridArray[x, y] = createGridObject(TileMapSprite.Road, this, x, y);
                                break;
                        }
                    }
                }
            }
        }

        //debug show the cell borders, set to false since colors work
        bool showDebug = false;
        if (showDebug) {
            TextMesh[,] debugTextArray = new TextMesh[width, height];

            for (int x = 0; x < gridArray.GetLength(0); x++) {
                for (int y = 0; y < gridArray.GetLength(1); y++) {
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                }
            }
            Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
            Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

            OnGridObjectChanged += (object sender, OnGridObjectChangedEventArgs eventArgs) => {
                debugTextArray[eventArgs.x, eventArgs.y].text = gridArray[eventArgs.x, eventArgs.y]?.ToString();
            };
        }
    }

    //some getters and setters
    public int GetWidth() {
        return width;
    }

    public int GetHeight() {
        return height;
    }

    public float GetCellSize() {
        return cellSize;
    }

    public Vector3 GetWorldPosition(int x, int y) {
        return new Vector3(x, y) * cellSize + originPosition;
    }

    public void GetXY(Vector3 worldPosition, out int x, out int y) {
        x = Mathf.FloorToInt((worldPosition - originPosition).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - originPosition).y / cellSize);

    }

    public void SetGridObject(int x, int y, TGridObject value) {
        //check if position is inside the grid
        if (x >= originPosition.x  && y >= originPosition.y && x < width && y < height) {
            gridArray[x, y] = value;
            if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
        }
    }

    public void TriggerGridObjectChanged(int x, int y) {
        if (OnGridObjectChanged != null) OnGridObjectChanged(this, new OnGridObjectChangedEventArgs { x = x, y = y });
    }

    public void SetGridObject(Vector3 worldPosition, TGridObject value) {
        int x, y;
        GetXY(worldPosition, out x, out y);
        SetGridObject(x, y, value);
    }

    public TGridObject GetGridObject(int x, int y) {
        if (x >= originPosition.x && y >= originPosition.y && x < width && y < height) {
            return gridArray[x, y];
        } else {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObject(Vector3 worldPosition) {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetGridObject(x, y);
    }

    //create a NativeArray of grid elements, so that we can deal with it in our entities/jobs
    //way to have a copy without passing the object
    public NativeArray<TileMapSprite> GetGridByValue(Func<TGridObject, TileMapSprite> convert){
        NativeArray<TileMapSprite> grid = new NativeArray<TileMapSprite>(GetWidth()*GetHeight(), Allocator.Persistent);
        for(int i = 0; i < GetWidth(); i++){
            for(int j = 0; j < GetHeight(); j++){
                grid[i+j*GetWidth()] = convert(gridArray[i,j]);
            }
        }
        return grid;
    } 

}
