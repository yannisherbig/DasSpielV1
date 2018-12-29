using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainScript : MonoBehaviour {

    public Terrain terrain;
    protected int[][][] terrainBackup;
    int numDetails;

    void Start()
    {
        terrain = Terrain.activeTerrain;
        numDetails = terrain.terrainData.detailPrototypes.Length;
        terrainBackup = new int[numDetails][][];
        CreateBackup();
    }

    void Update()
    {

    }

    void CreateBackup()
    {
        Debug.Log("starting terrain details backup...");
        int[][] detailLayer;
        for (int layerNum = 0; layerNum < numDetails; layerNum++)
        {
            detailLayer = ToJaggedArray(terrain.terrainData.GetDetailLayer(0, 0, terrain.terrainData.detailWidth, terrain.terrainData.detailHeight, layerNum));
            terrainBackup[layerNum] = detailLayer;
        }
    }

    void OnDestroy()
    {
        for (int layerNum = 0; layerNum < numDetails; layerNum++)
        {
            terrain.terrainData.SetDetailLayer(0, 0, layerNum, To2D(terrainBackup[layerNum]));
        }     
    }

    
    public static T[][] ToJaggedArray<T>(T[,] twoDimensionalArray)
    {
        int rowsFirstIndex = twoDimensionalArray.GetLowerBound(0);
        int rowsLastIndex = twoDimensionalArray.GetUpperBound(0);
        int numberOfRows = rowsLastIndex - rowsFirstIndex + 1;

        int columnsFirstIndex = twoDimensionalArray.GetLowerBound(1);
        int columnsLastIndex = twoDimensionalArray.GetUpperBound(1);
        int numberOfColumns = columnsLastIndex - columnsFirstIndex + 1;

        T[][] jaggedArray = new T[numberOfRows][];
        for (int i = 0; i < numberOfRows; i++)
        {
            jaggedArray[i] = new T[numberOfColumns];

            for (int j = 0; j < numberOfColumns; j++)
            {
                jaggedArray[i][j] = twoDimensionalArray[i + rowsFirstIndex, j + columnsFirstIndex];
            }
        }
        return jaggedArray;
    }

    public static T[,] To2D<T>(T[][] source)
    {
        try
        {
            int FirstDim = source.Length;
            int SecondDim = source.GroupBy(row => row.Length).Single().Key; // throws InvalidOperationException if source is not rectangular

            var result = new T[FirstDim, SecondDim];
            for (int i = 0; i < FirstDim; ++i)
                for (int j = 0; j < SecondDim; ++j)
                    result[i, j] = source[i][j];

            return result;
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("The given jagged array is not rectangular.");
        }
    }

}
