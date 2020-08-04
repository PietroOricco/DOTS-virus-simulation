using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid
{

    public enum TileMapSprite
    {
        Road,
        Pub,
        Park,
        Home
    }


    private int width;
    private int heigth;
    private float size;
    private Vector3 origin;
    private TileMapSprite[,] gridArray;




    public Grid(int width, int heigth, float size, Vector3 origin)
    {
        this.width = width;
        this.heigth = heigth;
        this.size = size;
        this.origin = origin;

        gridArray = new TileMapSprite[width, heigth];






        for (int i = 0; i < gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < gridArray.GetLength(1); j++)
            {
               

                if (i % 3 == 1)
                {
                    gridArray.SetValue(TileMapSprite.Road, i, j);

                }
                else
                {
                    if (j % 3 == 1)
                    {
                        gridArray.SetValue(TileMapSprite.Road, i, j);

                    }
                    else
                    {

                        // 1-> house, 2-> park, 3-> pub
                        int random = Random.Range(1, 4);
                        if (random == 1)
                        {
                            gridArray.SetValue(TileMapSprite.Home, i, j);

                        }
                        else if (random == 2)
                        {
                            gridArray.SetValue(TileMapSprite.Park, i, j);

                        }
                        else if (random == 3)
                        {
                            gridArray.SetValue(TileMapSprite.Pub, i, j);

                        }
                        else
                        {
                            gridArray.SetValue(TileMapSprite.Road, i, j);
                        }
                    }
                }
                Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i, j + 1), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(i, j), GetWorldPosition(i + 1, j), Color.white, 100f);

            }
        }

    }

    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * size + origin;
    }


    public TileMapSprite GetValue(int x, int y)
    {
        if (x >= 0 && x < gridArray.GetLength(0) && y >= 0 && y < gridArray.GetLength(1))
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TileMapSprite);
        }
    }

    public TileMapSprite getObject(int x, int y)
    {
        return gridArray[x, y];
    }

    public int getWidth()
    {
        return width;
    }

    public int getHeigth()
    {
        return heigth;
    }

    public float getCellSize()
    {
        return size;
    }

}
