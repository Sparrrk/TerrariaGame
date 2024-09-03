using UnityEngine;

[System.Serializable]
public class ChunkData
{
    //[JsonIgnore]
    //public Vector2 _chunkPosition;

    public int xNumber;
    public int yNumber;

    [SerializeField]
    public BlockType[,] _blockTypes;

    public ChunkData(int xPosition, int yPosition, BlockType[,] blockTypes)
    {
        xNumber = xPosition;
        yNumber = yPosition;
        _blockTypes = blockTypes;
    }

    public ChunkData()
    {
        _blockTypes = new BlockType[16, 16];
    }

    public ChunkData(ChunkData data)
    {
        xNumber=data.xNumber;
        yNumber=data.yNumber;
        _blockTypes = CopyArray(data._blockTypes);
    }

    private BlockType[,] CopyArray(BlockType[,] blocks)
    {
        BlockType[,] result = new BlockType[16, 16];
        for (int i = 0; i < blocks.GetLength(0); i++)
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                result[i, j] = blocks[i, j];
            }
        }
        return result;
    }
}

public enum BlockType
{
    Empty = 1,
    Earth = 2,
    EarthGrass = 3,
    Stone = 4,
    Water = 5,
    Water80R = 6,
    Water80L = 7,
    Water60R = 8,
    Water60L = 9,
    Water40R = 10,
    Water40L = 11,
    Water20R = 12,
    Water20L = 13,
    WaterMimic = 14,
    Snow = 15,
    Ice = 16,
    Sand = 17
}
