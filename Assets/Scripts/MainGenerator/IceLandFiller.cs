using UnityEngine;

public class IceLandFiller : MonoBehaviour
{
    private Perlin perlin;

    private void Awake()
    {
        perlin = new Perlin();    
    }

    public BlockType[,] CreateSnowChunk(int xPosition, int yPosition)
    {
        BlockType[,] chunkArray = perlin.CreateEmptyTypeArray();
        if (/*yPosition == 0*/ yPosition < 3)
        {
            chunkArray = perlin.FillWholeChunk(BlockType.Empty);
        }
        else if (/*yPosition == 1*/ yPosition == 3)
        {
            chunkArray = perlin.FillWholeChunk(BlockType.Snow);
            //chunkArray = perlin.FillTransitionChunk(BlockType.Empty, BlockType.Snow, 5, 100);
        }
        else if (yPosition == 4)
        {
            chunkArray = perlin.FillWholeChunk(BlockType.Snow);
        }
        else if (yPosition == 5)
        {
            chunkArray = perlin.FillTransitionChunk(BlockType.Snow, BlockType.Stone, 16, 100);
        }
        else if (yPosition == 6)
        {
            chunkArray = perlin.FillWholeChunk(BlockType.Stone);
        }
        return chunkArray;
    }

    

}
