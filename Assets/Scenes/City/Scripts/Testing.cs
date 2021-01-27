using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TileMapEnum;
using System.Xml;
using System.IO;

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
        XmlDocument mapDocument = new XmlDocument();
        Configuration conf = Configuration.CreateFromJSON();
        var mapFileName = conf.map;

        mapDocument.Load(@"./Conf/Maps/"+mapFileName);

        // Select a single node
        XmlNode mapNode = mapDocument.SelectSingleNode("map");
        var width = int.Parse(mapNode.Attributes["width"].Value);
        var height = int.Parse(mapNode.Attributes["height"].Value);

        XmlNode mapDataNode = mapNode.SelectSingleNode("layer").SelectSingleNode("data");
        var csvMap = mapDataNode.InnerText;

        int[,] array2Dmap = new int[height, width];
        var curLine = height;

        var lines = csvMap.Split('\n');
        foreach (var line in lines){
            if(curLine==height){
                curLine--;
                continue;
            }
            var values = line.Split(',');

            var i = 0;

            foreach (var item in values)
            {
                if(item.Length!=0)
                    if(item[0]!='\r'){
                        array2Dmap[curLine,i++] = int.Parse(item);
                    }
            }
            curLine--;
        }

        //num cells x, y, size, offset, element
        grid = new Grid<GridNode>(width, height, 10f, Vector3.zero, array2Dmap, (TileMapSprite tileType, Grid<GridNode> grid, int x, int y) => new GridNode(tileType, grid, x, y));
        cityVisual.SetGrid(grid);
    }
}
