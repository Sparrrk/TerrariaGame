using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.FilePathAttribute;
using UnityEngine.UIElements;


/// <summary>
/// Данный класс создает целочисленые двумерные массивы, значение элементов которых соответствует id блоков в чанке
/// </summary>
public class ChunkFiller : MonoBehaviour
{
    private int _blocksInChunk = 16;
    private BlockType[,] blockTypes;

    private ForestFiller forestFiller;
    private DesertFiller desertFiller;
    private IceLandFiller iceLandFiller;
    private TransitionZoneFiller transitionZoneFiller;
    private BlockStorage storage;
    private List<Location> locations;
    

    private void Awake()
    {
        locations = MakeLocationOrder();
        forestFiller = GetComponent<ForestFiller>();
        desertFiller = GetComponent<DesertFiller>();
        iceLandFiller = GetComponent<IceLandFiller>();
        transitionZoneFiller = GetComponent<TransitionZoneFiller>();
        storage = BlockStorage.Instance;
    }

    public List<Location> GetLocationsOrder()
    {
        return locations;
    }

    /// <summary>
    /// перемешивает порядок следования биомов 
    /// </summary>
    /// <returns></returns>
    private List<Location> MakeLocationOrder()
    {
        List<Location> locations = new List<Location>() { Location.IceLand, Location.Desert, Location.Forest};
        List<Location> newLocations = new List<Location>();
        for (;locations.Count > 0;)
        {
            Location currentLocation = locations[Random.Range(0, locations.Count)];
            locations.Remove(currentLocation);
            newLocations.Add(currentLocation);
        }
        return newLocations;
    }

    public Location GetLocationByCoordinates(Vector2Int coord,  ref int zoneNumber)
    {
        int x = coord.x;
        int y = coord.y;
        Location result = Location.None;

        if (x < storage.firstBiomBorder)                                              { result = locations[0]; }
        else if (x >= storage.firstBiomBorder && x < storage.firstTransitionBorder)   { result = Location.TransitionZone; zoneNumber = 1; }
        else if (x >= storage.firstTransitionBorder && x < storage.secondBiomBorder)  { result = locations[1]; }
        else if (x >= storage.secondBiomBorder && x < storage.secondTransitionBorder) { result = Location.TransitionZone; zoneNumber = 2; }
        else if (x >= storage.secondTransitionBorder && x < storage.thirdBiomBorder)  { result = locations[2]; }

        if (result == Location.TransitionZone)
        {
            result = storage.LocationToTransitionZone(locations[zoneNumber - 1], locations[zoneNumber]);
        }

        return result;
    }

    public BlockType[,] CreateChunk(int xPosition, int yPosition)
    {
        int transitionIndex = 0;
        Location location = GetLocationByCoordinates(new Vector2Int(xPosition, yPosition), ref transitionIndex);
        
        blockTypes = FillChunk(location, new Vector2Int(xPosition, yPosition), transitionIndex);

        ChunkData data = new ChunkData(xPosition, yPosition, blockTypes);
        DataBaseHandler.AddWholeChunk(data);

        return blockTypes;
    }

    private BlockType[,] FillChunk(Location location, Vector2Int position, int transitionIndex)
    {
        if (transitionIndex != 0)
        {
            blockTypes = FillTransitionZone(location, position, transitionIndex);
        }
        else
        {
            if (location == Location.Forest) { blockTypes = forestFiller.CreateForestChunk(position.x, position.y); }
            else if (location == Location.Desert) { blockTypes = desertFiller.CreateDesertChunk(position.x, position.y); }
            else if (location == Location.IceLand) { blockTypes = iceLandFiller.CreateSnowChunk(position.x, position.y); }
        }
        return blockTypes;
    }

    private BlockType[,] FillTransitionZone(Location location, Vector2Int position, int transitionIndex)
    {
        if (position.y >= storage.surfaceLevel && position.y < storage.surfaceLevel + 3)
        {
            blockTypes = transitionZoneFiller.FillTheTransitionChunk(new Vector2Int(position.x, position.y),
            storage.TransitionZoneToBlocks(location)[0], storage.TransitionZoneToBlocks(location)[1], location);
        }
        else
        {
            if (locations[transitionIndex] == Location.Forest) { blockTypes = forestFiller.CreateForestChunk(position.x, position.y); }
            else if (locations[transitionIndex] == Location.Desert) { blockTypes = desertFiller.CreateDesertChunk(position.x, position.y);  }
            else if (locations[transitionIndex] == Location.IceLand) { blockTypes = iceLandFiller.CreateSnowChunk(position.x, position.y); }
        }

        return blockTypes;
    }
    
    public void MakeTheSurface(int[] mountainPositions, int worldLength)
    {
        int surfaceLevel = storage.surfaceLevel;

        Perlin perlin = new Perlin();
        for (int i = 1; i < worldLength; i++)
        {
            if (!mountainPositions.Contains<int>(i) && !mountainPositions.Contains<int>(i - 1))
            {
                ChunkData chunkData = DataBaseHandler.LoadChunkFromDB(i, surfaceLevel);
                ChunkData newData = new ChunkData(chunkData);
                BlockType[,] types = newData._blockTypes;
                perlin.FillWithPerlin(4, BlockType.Empty, new int[_blocksInChunk], new int[_blocksInChunk], types);
                DataBaseHandler.UpdateChunk(chunkData, newData);
            }
        }
    }

    private BlockType[,] CreateEmptyTypeArray(int size)
    {
        BlockType[,] types = new BlockType[size, size];
        for (int i = 0; i < types.GetLength(0); i++)
        {
            for (int j = 0; j < types.GetLength(1); j++)
            {
                types[i, j] = BlockType.Empty;
            }
        }
        return types;
    }
}

public class Perlin
{
    private int _blocksInChunk;

    public Perlin()
    {
        _blocksInChunk = BlockStorage.Instance.blocksInChunk;
    }

    public void FillWithPerlin(int thickness, BlockType blockType, int[] layerHeight, int[] columnHeight, BlockType[,] types)
    {
        float seed = Random.Range(0.1f, 0.3f);
        for (int i = 0; i < _blocksInChunk; i++)
        {
            layerHeight[i] = (int)(Mathf.PerlinNoise1D(i * seed) * thickness);
            columnHeight[i] = CreateColumn(i, layerHeight[i], columnHeight[i], blockType, types);
        }
    }

    private int CreateColumn(int xIndex, int layerHeight, int columnHeight, BlockType blockType, BlockType[,] types)
    {
        for (int i = 0; i < layerHeight && columnHeight < _blocksInChunk; i++, columnHeight++)
        {
            types[xIndex, columnHeight] = blockType;
        }
        return columnHeight;
    }

    public BlockType[,] CreateEmptyTypeArray()
    {
        BlockType[,] types = new BlockType[_blocksInChunk, _blocksInChunk];
        for (int i = 0; i < types.GetLength(0); i++)
        {
            for (int j = 0; j < types.GetLength(1); j++)
            {
                types[i, j] = BlockType.Empty;
            }
        }
        return types;
    }

    public BlockType[,] CreateEmptyTypeArray(int size)
    {
        BlockType[,] types = new BlockType[size, size];
        for (int i = 0; i < types.GetLength(0); i++)
        {
            for (int j = 0; j < types.GetLength(1); j++)
            {
                types[i, j] = BlockType.Empty;
            }
        }
        return types;
    }

    public BlockType[,] FillWholeChunk(BlockType blockType)
    {
        BlockType[,] types = CreateEmptyTypeArray();
        for (int i = 0; i < _blocksInChunk; i++)
        {
            for (int j = 0; j < _blocksInChunk; j++)
            {
                types[i, j] = blockType;
            }
        }
        return types;
    }

    public BlockType[,] CreateGaussian(BlockType block)
    {
        BlockType[,] array = CreateEmptyTypeArray();
        float[] values = new float[array.GetLength(0)];
        for (int i = 0; i < values.Length; i++)
        {
            float fisrt = Random.value;
            float second = Random.value;

            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(fisrt)) * Mathf.Sin(2 * Mathf.PI * second);
            values[i] = randStdNormal;
        }
        return array;
    }

    public BlockType[,] FillTransitionChunk(BlockType block1, BlockType block2, int block1Thickness, int block2Thickness)
    {
        BlockType[,] types = CreateEmptyTypeArray();
        int[] layerHeight = new int[_blocksInChunk];
        int[] columnHeight = new int[_blocksInChunk];
        FillWithPerlin(block1Thickness, block1, layerHeight, columnHeight, types);
        FillWithPerlin(block2Thickness, block2, layerHeight, columnHeight, types);
        return types;
    }
}
