using UnityEngine;

public class ForestFiller : MonoBehaviour
{
    private Perlin perlin;

    private void Awake()
    {
        perlin = new Perlin();
    }

    public BlockType[,] CreateForestChunk(int xPosition, int yPosition)
    {
        BlockType[,] chunkArray = perlin.CreateEmptyTypeArray();
        if (/*yPosition == 0*/ yPosition < 3)
        {
            chunkArray = perlin.FillWholeChunk(BlockType.Empty);
        }
        else if (/*yPosition == 1*/ yPosition == 3)
        {
            chunkArray = perlin.FillWholeChunk(BlockType.Earth);
            //chunkArray = FillSurfaceChunk();
        }
        else if (yPosition == 4)
        {
            chunkArray = perlin.FillWholeChunk(BlockType.Earth);
        }
        else if (yPosition == 5)
        {
            chunkArray = perlin.FillTransitionChunk(BlockType.Earth, BlockType.Stone, 16, 100);
        }
        else if (yPosition == 6)
        {
            chunkArray = perlin.FillWholeChunk(BlockType.Stone);
        }
        return chunkArray;
    }

    /// <summary>
    /// заполнить чанк блоками воздуха, травы и земли
    /// </summary>
    /// <returns></returns>
    private BlockType[,] FillSurfaceChunk()
    {
        BlockType[,] array = perlin.CreateEmptyTypeArray();
        int[] layerHeight = new int[array.GetLength(1)];
        int[] columnHeight = new int[array.GetLength(1)];
        perlin.FillWithPerlin(8, BlockType.Empty, layerHeight, columnHeight, array);
        AddTheGrass(array, columnHeight);
        perlin.FillWithPerlin(100, BlockType.Earth, layerHeight, columnHeight, array);
        return array;
    }

    private void AddTheGrass(BlockType[,] types, int[] columnHeight)
    {
        for (int i = 0; i < types.GetLength(0); i++)
        {
            if (columnHeight[i] < types.GetLength(1))
            {
                types[i, columnHeight[i]] = BlockType.EarthGrass;
                columnHeight[i]++;
            }
        }
    }
}
