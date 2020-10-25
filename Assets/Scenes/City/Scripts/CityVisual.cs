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
                Vector2 gridValueUV;

                //based on type of cell, decide color
                if (value == TileMapSprite.Road){
                    gridValueUV = Vector2.zero;
                }
                else if (value == TileMapSprite.Park){
                    gridValueUV = Vector2.one;
                }
                else if (value == TileMapSprite.Home){
                    gridValueUV = new Vector2(0.5f,0.5f);
                }
                else if (value == TileMapSprite.Pub){
                    gridValueUV = new Vector2(0.25f, 0.25f);
                }
                else if (value == TileMapSprite.Supermarket){
                    gridValueUV = new Vector2(0.75f, 0.75f);
                }
                else{
                    gridValueUV = Vector2.zero;
                }
                //set mesh created and position offset (center of the cell)
                MeshUtils.AddToMeshArrays(vertices, uv, triangles, index, grid.GetWorldPosition(i,j)+quadsize*0.5f, 0f, quadsize, gridValueUV, gridValueUV);

            }
        }
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }




}
