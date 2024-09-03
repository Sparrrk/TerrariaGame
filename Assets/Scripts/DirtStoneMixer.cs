using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirtStoneMixer
{
    private int surfaceLevel;
    private int blocksInChunk;
    private int width;
    private int height;

    public DirtStoneMixer(BlockStorage storage, BlockType[,] globalArray)
    {
        surfaceLevel = storage.surfaceLevel;
        blocksInChunk = storage.blocksInChunk;
        width = globalArray.GetLength(0);
        height = globalArray.GetLength(1);
    }

    public void MixDirtAndStone(BlockType[,] globalArray)
    {
        MixUpperLevel(globalArray);
        DeleteTooSmallClusters(globalArray, 10);
        MixBottomLevel(globalArray);
        DeleteTooSmallClusters(globalArray, 4);
        ApplyGaussianBlur(globalArray);

    }

    private void MixUpperLevel(BlockType[,] globalArray)
    {
        for (int i = 0; i < globalArray.GetLength(0); i++)
        {
            for (int j = surfaceLevel * blocksInChunk; j <= 11 * blocksInChunk; j++)
            {
                float result = 0;
                float frequency = 1;
                for (int counter = 1; counter < 6; counter++, frequency++)
                {
                    float perlinValue = Mathf.PerlinNoise(i * 0.05f * frequency, j * 0.05f * frequency);
                    result += perlinValue / frequency;
                }
                result /= 2f;
                if (result > 0.65f && globalArray[i, j] == BlockType.Earth)
                {
                    globalArray[i, j] = BlockType.Stone;
                }    
            }
        }
    }

    private void MixBottomLevel(BlockType[,] globalArray)
    {
        for (int i = 0; i < globalArray.GetLength(0); i++)
        {
            for (int j = 11 * blocksInChunk + 1; j <=  20 * blocksInChunk; j++)
            {
                float result = 0;
                float frequency = 1;
                float gradientfactor = Mathf.Lerp(0.6f, 1.0f, (float)j / (20 * blocksInChunk));
                for (int counter = 1; counter < 7; counter++, frequency++)
                {
                    float perlinValue = Mathf.PerlinNoise(i * 0.15f * frequency, j * 0.15f * frequency);
                    result += perlinValue / frequency;
                }
                result /= 2f;
                if (result * gradientfactor > 0.55f && globalArray[i, j] == BlockType.Earth)
                {
                    globalArray[i, j] = BlockType.Stone;
                }
            }
        }
    }

    private void ApplyGaussianBlur(BlockType[,] globalArray)
    {
        BlockType[,] resultArray = new BlockType[globalArray.GetLength(0), globalArray.GetLength(1)];

        for (int i = 1; i < width - 1; i++)
        {
            for (int j = (surfaceLevel + 1) * blocksInChunk; j <= 20 * blocksInChunk; j++)
            {
                float sum = 0;
                for (int k = i - 1; k <= i + 1; k++)
                {
                    for (int l = j - 1; l <= j + 1; l++)
                    {
                        if (globalArray[k, l] == BlockType.Stone)
                            sum++;
                    }
                }
                if (sum / 9 >= 0.5f)
                    resultArray[i, j] = BlockType.Stone;
                else
                    resultArray[i, j] = BlockType.Earth;

            }
        }
        for ( int i = 1; i < width - 1; i++)
        {
            for (int j = (surfaceLevel + 1) * blocksInChunk; j <= 20 * blocksInChunk; j++)
            {
                globalArray[i, j] = resultArray[i, j];
            }
        }
    }


    private void DeleteTooSmallClusters(BlockType[,] globalArray, int minimumSize)
    {
        bool[,] visited = new bool[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (globalArray[i, j] == BlockType.Stone && !visited[i, j])
                {
                    List<Vector2Int> clusterBlock = new List<Vector2Int>();

                    int clusterSize = GetClusterSize(globalArray, i, j, visited, clusterBlock);

                    //Debug.Log("Cluster Size = " + clusterSize);

                    if (clusterSize < minimumSize)
                    {
                        foreach (Vector2Int block in clusterBlock)
                        {
                            globalArray[block.x, block.y] = BlockType.Earth;
                        }
                    }
                }
            }
        }
    }

    private int GetClusterSize(BlockType[,] globalArray, int x, int y, bool[,] visited, List<Vector2Int> clusterBLocks)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(x, y));
        visited[x, y] = true;

        int size = 0;

        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            size++;
            clusterBLocks.Add(current);

            Vector2Int[] directions = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

            foreach (Vector2Int direction in directions)
            {
                int nx = current.x + direction.x;
                int ny = current.y + direction.y;

                if ( nx >= 0 && nx < width && ny >= 0 && ny < height && globalArray[nx, ny] == BlockType.Stone && !visited[nx, ny])
                {
                    stack.Push(new Vector2Int(nx, ny));
                    visited[nx, ny] = true;
                }
            }
        }

        return size;
    }
}
