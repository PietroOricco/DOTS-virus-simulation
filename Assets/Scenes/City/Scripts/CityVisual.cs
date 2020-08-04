using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CityVisual : MonoBehaviour
{
    private Grid grid;
    private Mesh mesh;


    private void Awake()
    {
        mesh = new Mesh(); 
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetGrid (Grid grid)
    {
        this.grid = grid;
        UpdateVisual();

    }

    private void UpdateVisual()
    {
        MeshUtils.CreateEmptyMeshArrays(grid.getWidth() * grid.getHeigth(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);
        for (int i = 0; i<grid.getWidth(); i++)
        {
            for (int j=0; j<grid.getHeigth(); j++)
            {
                int index = i * grid.getHeigth() + j;
                Vector3 quadsize = new Vector3(1, 1) * grid.getCellSize();
                Grid.TileMapSprite value = grid.getObject(i, j);
                Vector2 gridValueUV;
                if (value == Grid.TileMapSprite.Road)
                {
                    gridValueUV = Vector2.zero;
                }
                else if (value == Grid.TileMapSprite.Park)
                {
                    gridValueUV = Vector2.one;
                }
                else if (value == Grid.TileMapSprite.Home)
                {
                    gridValueUV = new Vector2(0.5f,0.5f);
                }
                else if (value == Grid .TileMapSprite.Pub)
                {
                    gridValueUV = new Vector2(0.25f, 0.25f);
                }
                else
                {
                    gridValueUV = Vector2.zero;

                }
                MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(i,j)+quadsize*0.5f, 0f, quadsize, gridValueUV, gridValueUV);

            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }




}
