using System.Collections.Generic;
using UnityEngine;
using SimplexNoise;

public class ChunkInitialFiller : MonoBehaviour
{
    [SerializeField] BlockStorage blockStorage;
    private int blocksInChunk;
    private float surfaceSeed;
    private int stoneLevel;
    private int surfaceLevel;

    private int chunksAmountX;
    private int chunksAmountY;

    private void Awake()
    {
        surfaceSeed = UnityEngine.Random.Range(0.02f, 0.08f); 
    }

    public void CreateTheWorld(int globalWidth, int globalHeight)
    {
        Initialize();
        BlockType[,] globalArray = new BlockType[globalWidth, globalHeight];
        for (int i = 0; i < globalWidth; i++)
        {
            for (int j = 0; j < globalHeight; j++)
            {
                globalArray[i, j] = BlockType.Empty;
            }
        }
        chunksAmountX = globalWidth / blocksInChunk;
        chunksAmountY = globalHeight / blocksInChunk;
        for (int i = 0; i < chunksAmountX; i++)
        {
            for (int j = 0; j < chunksAmountY; j++)
            {
                BlockType[,] chunk = InitialFillingChunk(i, j);
                CopyToGlobalArray(chunk, globalArray, i, j);
            }
        }
        DirtStoneMixer dirtStoneMixer = new DirtStoneMixer(blockStorage, globalArray);
        dirtStoneMixer.MixDirtAndStone(globalArray);
        SaveInDatabase(globalArray);
    }

    private void Initialize()
    {
        surfaceLevel = blockStorage.surfaceLevel;
        stoneLevel = blockStorage.stoneLevel;
        blocksInChunk = blockStorage.blocksInChunk;
    }

    private void CopyToGlobalArray(BlockType[,] localArray, BlockType[,] globalArray, int x, int y)
    {
        for (int i = 0; i < localArray.GetLength(0); i++)
        {
            for (int j = 0; j < localArray.GetLength(1); j++)
            {
                globalArray[x * blocksInChunk + i, y * blocksInChunk + j] = localArray[i, j];
            }
        }
    }

    private void CopyFromGlobalArray(BlockType[,] localArray, BlockType[,] globalArray, int x, int y)
    {
        for (int i = 0; i < localArray.GetLength(0); i++)
        {
            for (int j = 0; j < localArray.GetLength(1); j++)
            {
                localArray[i, j] = globalArray[x * blocksInChunk + i, y * blocksInChunk + j];
            }
        }
    }

    private BlockType[,] InitialFillingChunk(int x, int y)
    {
        BlockType[,] result = new BlockType[blocksInChunk, blocksInChunk];
        if (y < surfaceLevel)
        {
            result = CreateSolidArray(BlockType.Empty);
        }
        else if (y == surfaceLevel)
        {
            result = CreateTransitionZone(x, y, BlockType.Empty, BlockType.Earth);
        }
        else if (y > surfaceLevel && y < stoneLevel)
        {
            result = CreateSolidArray(BlockType.Earth);
        }
        else if (y == stoneLevel)
        {
            result = CreateTransitionZone(x, y, BlockType.Earth, BlockType.Stone);
        }
        else if (y > stoneLevel)
        {
            result = CreateSolidArray(BlockType.Stone);
        }
        return result;
    }

    private BlockType[,] CreateTransitionZone(int x, int y, BlockType upperBlock, BlockType lowerBlock)
    {
        BlockType[,] result = CreateSolidArray(lowerBlock);
        int[] heights = new int[result.GetLength(0)];
        for (int i = 0; i < heights.Length; i++)
        {
            heights[i] = (int)(Mathf.PerlinNoise((x * blocksInChunk + i) * surfaceSeed, y * blocksInChunk * surfaceSeed) * blocksInChunk);
            for (int j = 0; j < heights[i]; j++)
            {
                result[i, j] = upperBlock;
            }
        }
        return result;
    }

    private BlockType[,] CreateSolidArray(BlockType blockType)
    {
        BlockType[,] result = new BlockType[blocksInChunk, blocksInChunk];
        for (int i = 0; i < blocksInChunk; i++)
        {
            for (int j = 0; j < blocksInChunk; j++)
            {
                result[i, j] = blockType;
            }
        }
        return result;
    }

    private float[,] CreateSimplexArray(BlockType[,] globalArray, float scale)
    {
        int length = globalArray.GetLength(0);
        int height = globalArray.GetLength(1);
        float[,] result = new float[length, height];
        float maxValue = 0;
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < height; j++)
            {
                result[i, j] = Noise.CalcPixel2D(i, j, scale);
                if (result[i, j] > maxValue)
                    maxValue = result[i, j];
            }
        }
        for (int i = 0; i < length; i++)
        {
            for (int j = 0; j < height; j++)
            {
                result[i, j] /= maxValue;
            }
        }
        return result;
    }


    private void MixDirtAndStone(BlockType[,] globalArray)
    {
        float scale = 0.5f;
        float frequency = 0.27f;
        float randomShift = Random.Range(0f, 10f);
        int surface = (surfaceLevel + 1) * blocksInChunk;

        for (int i = 0; i < globalArray.GetLength(0); i++)
        {
            for (int j = surface; j < globalArray.GetLength(1); j++)
            {
                float perlinValue = Mathf.PerlinNoise(i * scale * frequency + randomShift, j * scale * frequency + randomShift);
                if (perlinValue > 0.52f && perlinValue < 0.58f && globalArray[i, j] == BlockType.Earth)
                {
                    globalArray[i, j] = BlockType.Stone;
                }
                else if (perlinValue > 0.51f && perlinValue < 0.59f && globalArray[i, j] == BlockType.Stone)
                {
                    globalArray[i, j] = BlockType.Earth;
                }
            }
        }
    }

    


    //private void MakeInclusions(BlockType[,] globalArray, BlockType inclusion, int y, float seed)
    //{
    //    float perlinValue;
    //    for (int i = 0; i < globalArray.GetLength(0); i++)
    //    {
    //        perlinValue = Mathf.PerlinNoise(i * seed, y * seed);
    //        if (/*perlinValue > 0.75f*/ perlinValue > 0.5f && perlinValue < 0.55f)
    //            globalArray[i, y] = inclusion;
    //    }
    //}
    
    private void GenerateInitialCaves(BlockType[,] globalArray)
    {
        float frequency = 0.09f;
        float caveSeed = Random.Range(1f, 10f);
        float perlinValue;
        for (int i = 0; i <  globalArray.GetLength(0); i++)
        {
            for (int j = 0; j < globalArray.GetLength(1); j++)
            {
                perlinValue = Mathf.PerlinNoise(i * frequency + caveSeed, j * frequency + caveSeed);
                if (j > (surfaceLevel + 2) * blocksInChunk) 
                {
                    if (perlinValue > 0.62f)
                    {
                        globalArray[i, j] = BlockType.Empty;
                    }
                }
            }
        }
    }

    private void SaveInDatabase(BlockType[,] globalArray)
    {
        for (int i = 0; i < chunksAmountX; i++)
        {
            List<ChunkData> chunkDatas = new List<ChunkData>();
            for (int j = 0; j < chunksAmountY; j++)
            {
                BlockType[,] array = new BlockType[blocksInChunk, blocksInChunk];
                CopyFromGlobalArray(array, globalArray, i, j);
                ChunkData chunkData = new ChunkData(i, j, array);
                
                chunkDatas.Add(chunkData);
            }
            DataBaseHandler.AddColumn(chunkDatas);
        }
    }
}
