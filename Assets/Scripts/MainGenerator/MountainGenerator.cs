using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MountainGenerator : MonoBehaviour
{
    [SerializeField] ChunkFiller chunkFiller;
    private BlockStorage storage;
    private int _blocksInChunk;
    private Perlin perlin;

    private bool isInitialized = false;
    

    private void Initialize()
    {
        storage = BlockStorage.Instance;
        _blocksInChunk = storage.blocksInChunk;
        perlin = new Perlin();
        isInitialized = true;
    }
    

    private BlockType[,] CreateTheMountain(BlockType blockType)
    {
        int MountainSize = Random.Range(_blocksInChunk + 1, _blocksInChunk * 2);
        BlockType[,] array = perlin.CreateEmptyTypeArray(MountainSize);
        int[] mountainHeights = new int[MountainSize];
        int radius = (int)(MountainSize / 2);

        float normalHeight;
        for (int i = 0; i < MountainSize; i++)
        {
            normalHeight = Mathf.Pow( Mathf.Pow(radius, 2) - Mathf.Pow(i - radius, 2), 2 ) / Mathf.Pow(radius, 4);
            mountainHeights[i] = (int)(normalHeight * MountainSize + 1) ;
        }

        for (int i = 0; i < array.GetLength(0); i++)
        {
            //Debug.Log("number: " + i + "height: " + mountainHeights[i]);
            for (int j = MountainSize - 1, k = 0; k < mountainHeights[i] && j >= 0; k++, j--)
            {
                array[i, j] = blockType;
            }
        }

        return array;
    }

    private void FillOneMountain(int mountainPosition, BlockType[,] mountainArray)
    {
        //считаем количество чанков, которые нам необходимо загрузить из БД для создания горы (горы имеют рандомный размер)
        int length = mountainArray.GetLength(0);
        int amountOfChunks = (length - 1) / _blocksInChunk + 1;

        //создаем двумерный массив под загружаемые чанки
        BlockType[,][,] types = new BlockType[amountOfChunks, amountOfChunks][,];
        BlockType[,][,] oldChunks = new BlockType[amountOfChunks, amountOfChunks][,];

        //для каждого чанка создаем пустой массив
        for (int i = 0; i < types.GetLength(0); i++)
        {
            for (int j = 0; j < types.GetLength(1); j++)
            {
                types[i, j] = DataBaseHandler.LoadChunkFromDB(mountainPosition + i, 3 + j - amountOfChunks)._blockTypes;
                oldChunks[i, j] = DataBaseHandler.LoadChunkFromDB(mountainPosition + i, 3 + j - amountOfChunks)._blockTypes;
            }
        }

        for (int extI = 0; extI < types.GetLength(0); extI++)
        {
            int[] counter = new int[_blocksInChunk];
            for (int extJ = types.GetLength(1) - 1; extJ >= 0; extJ--)
            {
                for (int intI = 0; intI < types[extI, extJ].GetLength(0) && extI * _blocksInChunk + intI < mountainArray.GetLength(0); intI++)
                {
                    for (int intJ = types[extI, extJ].GetLength(1) - 1; intJ >= 0 && counter[intI] < mountainArray.GetLength(1) ; intJ--)
                    {
                        types[extI, extJ][intI, intJ] = mountainArray[extI * _blocksInChunk + intI, mountainArray.GetLength(1) - 1 - counter[intI]];

                        counter[intI]++;
                    }
                }
                DataBaseHandler.UpdateChunk(new ChunkData(mountainPosition + extI, 3 + extJ - amountOfChunks, oldChunks[extI, extJ]),
                                            new ChunkData(mountainPosition + extI, 3 + extJ - amountOfChunks, types[extI, extJ]));
            }
        }
    }

    public int[] FillWithMountains()
    {
        if (!isInitialized)
            Initialize();

        BlockType[,] array = null;
        int worldLength = storage.worldLength;
        int transitionIndex = 0;
        BlockType blockType;
        List<Location> locations = chunkFiller.GetLocationsOrder();

        int[] mountainPositions = new int[4];

        for (int i = 0; i < mountainPositions.Length; i++)
        {
            
            
            int mountainPosition = Random.Range(1, worldLength - 2);
            if (!mountainPositions.Contains<int>(mountainPosition))
            {
                mountainPositions[i] = mountainPosition;
                Location location = chunkFiller.GetLocationByCoordinates(new Vector2Int(mountainPosition, 1), ref transitionIndex );

                if (transitionIndex == 0) { blockType = storage.LocationToBlockType(location); }
                else { location = locations[transitionIndex - 1]; blockType = storage.LocationToBlockType(location); }

                array = CreateTheMountain(blockType);
                FillOneMountain(mountainPosition, array);
            }
            else
            {
                i--;
            }
        }

        return mountainPositions;
    }
}
