using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapEnum;


//
public class CityVisual : MonoBehaviour
{
    private Grid<GridNode> grid;
    private Mesh mesh;


    private void Awake()
    {
        mesh = new Mesh(); 
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetGrid (Grid<GridNode> grid)
    {
        this.grid = grid;
        UpdateVisual();

    }

    private void UpdateVisual()
    {
        //create mesh for the whole map
        MeshUtils.CreateEmptyMeshArrays(grid.GetWidth() * grid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);
        //create proper color for each cell
        for (int i = 0; i<grid.GetWidth(); i++)
        {
            for (int j=0; j<grid.GetHeight(); j++)
            {
                int index = i * grid.GetHeight() + j;
                Vector3 quadsize = new Vector3(1, 1) * grid.GetCellSize();
                TileMapSprite value = grid.GetGridObject(i, j).GetTileType();
                Vector2 gridValueUV00, gridValueUV11;

                //based on type of cell, decide color
                if (value == TileMapSprite.Road){
                    gridValueUV00 = new Vector2(1f/3f,1f/3f);
                    gridValueUV11 = new Vector2(2f/3f,2f/3f);
                }
                else if (value == TileMapSprite.Park){
                    gridValueUV00 = new Vector2(1f/3f, 2f/3f);
                    gridValueUV11 = new Vector2(2f/3f, 1);
                }
                else if (value == TileMapSprite.Home){
                    gridValueUV00 = new Vector2(2f/3f, 0);
                    gridValueUV11 = new Vector2(1, 1f/3f);
                }
                else if (value == TileMapSprite.Pub){
                    gridValueUV00 = new Vector2(0, 2f/3f);
                    gridValueUV11 = new Vector2(1f/3f, 1);
                }
                else if (value == TileMapSprite.Supermarket){
                    gridValueUV00 = new Vector2(1f/3f, 0);
                    gridValueUV11 = new Vector2(2f/3f, 1f/3f);
                }
                else{
                    gridValueUV00 = new Vector2(1f/3f, 2f/3f);
                    gridValueUV11 = new Vector2(2f/3f, 1);
                }
                //set mesh created and position offset (center of the cell)
                MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(i,j)+quadsize*0.5f, 0f, quadsize, gridValueUV00, gridValueUV11);

            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }




}
