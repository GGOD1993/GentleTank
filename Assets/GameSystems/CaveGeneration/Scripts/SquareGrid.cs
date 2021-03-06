﻿using UnityEngine;

public class SquareGrid
{
    public Square[,] squares;

    public SquareGrid(TileType[,] map)
    {
        int nodeCountX = map.GetLength(0);
        int nodeCountY = map.GetLength(1);
        float mapWidth = nodeCountX ;
        float mapHeight = nodeCountY ;

        ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

        for (int x = 0; x < nodeCountX; x++)
        {
            for (int y = 0; y < nodeCountY; y++)
            {
                Vector3 pos = new Vector3(-mapWidth / 2 + x  + 1 / 2, 0, -mapHeight / 2 + y  + 1 / 2);
                controlNodes[x, y] = new ControlNode(pos, map[x, y] == TileType.Wall);
            }
        }

        squares = new Square[nodeCountX - 1, nodeCountY - 1];   //因为不需要多出外边没有的点，所有最大值减一。
        for (int x = 0; x < nodeCountX - 1; x++)
        {
            for (int y = 0; y < nodeCountY - 1; y++)
            {
                squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                if (x == 0 || y == 0 || x == nodeCountX - 2 || y == nodeCountY - 2)
                    squares[x, y].isBorder = true;
            }
        }

    }
}

