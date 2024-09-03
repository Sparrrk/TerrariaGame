using System.Collections.Generic;
using UnityEngine;

public class BlockStorage : MonoBehaviour
{
    public float chunkSize = 5.12f;
    public int blocksInChunk = 16;
    public float blockSize = 0.32f;
    public int worldLength = 50;
    public int worldDepth = 50;
    public int surfaceLevel = 5;
    public int stoneLevel = 25;
    public float worldSeed;

    public GameObject Earth;
    public GameObject Stone;
    public GameObject EarthGrass;
    public GameObject Snow;
    public GameObject Sand;
    public GameObject Water;
    public GameObject WaterMimic;
    public GameObject Water80Right;
    public GameObject Water80Left;
    public GameObject Water60Right;
    public GameObject Water60Left;
    public GameObject Water40Right;
    public GameObject Water40Left;
    public GameObject Water20Right;
    public GameObject Water20Left;

    public static BlockStorage Instance { get; private set; }
    private  Dictionary<BlockType, GameObject> TypesToObjects;
    private Dictionary<GameObject, BlockType> ObjectsToTypes;
    private Dictionary<Location, BlockType> TypesByLocations;
    private Dictionary<Direction, Vector2Int> VectorsByDirections;

    public int firstBiomBorder = 7;
    public int firstTransitionBorder = 10;
    public int secondBiomBorder = 17;
    public int secondTransitionBorder = 20;
    public int thirdBiomBorder = 30;


    private void CreateDicts()
    {
        TypesToObjects = new Dictionary<BlockType, GameObject>()
        {
            { BlockType.Empty, null },
            { BlockType.Earth, Earth },
            { BlockType.Stone, Stone },
            { BlockType.EarthGrass, EarthGrass },
            { BlockType.Snow, Snow },
            { BlockType.Sand, Sand },
            { BlockType.Water, Water },
            { BlockType.WaterMimic, WaterMimic },
            { BlockType.Water80R, Water80Right },
            { BlockType.Water80L, Water80Left },
            { BlockType.Water60R, Water60Right },
            { BlockType.Water60L, Water60Left },
            { BlockType.Water40R, Water40Right },
            { BlockType.Water40L, Water40Left },
            { BlockType.Water20R, Water20Right },
            { BlockType.Water20L, Water20Left },
        };

        ObjectsToTypes = new Dictionary<GameObject, BlockType>()
        {
            { Earth, BlockType.Earth },
            { Stone, BlockType.Stone },
            { EarthGrass, BlockType.EarthGrass },
            { Snow, BlockType.Snow },
            { Sand, BlockType.Sand },
            { Water, BlockType.Water },
            { WaterMimic, BlockType.WaterMimic },
            { Water80Right, BlockType.Water80R },
            { Water80Left , BlockType.Water80L },
            { Water60Right , BlockType.Water60R },
            { Water60Left , BlockType.Water60L },
            { Water40Right , BlockType.Water40R },
            { Water40Left , BlockType.Water40L },
            { Water20Right , BlockType.Water20R },
            { Water20Left , BlockType.Water20L },
        };

        TypesByLocations = new Dictionary<Location, BlockType>()
        {
            { Location.Forest, BlockType.Earth },
            { Location.Desert, BlockType.Sand },
            { Location.IceLand, BlockType.Snow }
        };

        VectorsByDirections = new Dictionary<Direction, Vector2Int>()
        {
            {Direction.None, new Vector2Int(0, 0) },
            {Direction.Up, new Vector2Int(0, -1) },
            {Direction.Down, new Vector2Int(0, 1)},
            {Direction.Left, new Vector2Int(-1, 0)},
            {Direction.Right, new Vector2Int(1, 0)},
            {Direction.UpLeft, new Vector2Int(-1, -1) },
            {Direction.UpRight, new Vector2Int(1, -1) },
            {Direction.DownLeft, new Vector2Int(-1, 1) },
            {Direction.DownRight, new Vector2Int(1, 1) }
        };
    }

    public Location BlocksToTransitionZone(BlockType block1, BlockType block2)
    {
        Location location = Location.None;
        if (block1 == BlockType.Earth && block2 == BlockType.Sand) { location = Location.ForestDesert; }
        else if (block1 == BlockType.Earth && block2 == BlockType.Snow) { location = Location.ForestIceLand; }
        else if (block1 == BlockType.Snow && block2 == BlockType.Sand) { location = Location.IceLandDesert; }
        else if (block1 == BlockType.Snow && block2 == BlockType.Earth) { location = Location.IceLandForest; }
        else if (block1 == BlockType.Sand && block2 == BlockType.Earth) { location = Location.DesertForest; }
        else if (block1 == BlockType.Sand && block2 == BlockType.Snow) { location = Location.DesertIceland; }

        return location;
    }

    public Location BlocksToLocation(BlockType block)
    {
        Location result = Location.None;
        
        if (block == BlockType.Earth) { result = Location.Forest; }
        else if (block == BlockType.Sand) { result = Location.Desert; }
        else if (block == BlockType.Snow) {  result = Location.IceLand;}
        
        return result;
    }

    public Location LocationToTransitionZone(Location location1, Location location2)
    {
        Location result = Location.None;

        if (location1 == Location.Forest && location2 == Location.IceLand) { result = Location.ForestIceLand; }
        else if (location1 == Location.Forest && location2 == Location.Desert) { result = Location.ForestDesert; }
        else if (location1 == Location.IceLand && location2 == Location.Desert) { result = Location.IceLandDesert; }
        else if (location1 == Location.IceLand && location2 == Location.Forest) { result = Location.IceLandForest; }
        else if (location1 == Location.Desert && location2 == Location.IceLand) { result = Location.DesertIceland; }
        else if (location1 == Location.Desert && location2 == Location.Forest) { result = Location.DesertForest; }

        return result;
    }

    public BlockType[] TransitionZoneToBlocks(Location location)
    {
        BlockType block1 = BlockType.Earth;
        BlockType block2 = BlockType.Earth;

        if (location == Location.ForestDesert) { block1 = BlockType.Earth; block2 = BlockType.Sand; }
        else if (location == Location.ForestIceLand) { block1 = BlockType.Earth; block2 = BlockType.Snow; }
        else if (location == Location.DesertIceland) { block1 = BlockType.Sand; block2 = BlockType.Snow; }
        else if (location == Location.DesertForest) { block1 = BlockType.Sand; block2 = BlockType.Earth; }
        else if (location == Location.IceLandForest) { block1 = BlockType.Snow; block2 = BlockType.Earth; }
        else if (location == Location.IceLandDesert) { block1 = BlockType.Snow; block2 = BlockType.Sand; }

        return new BlockType[] { block1, block2 };
    } 

    public GameObject LocationToObjects(Location location)
    {
        return TypesToObjects[TypesByLocations[location]];
    }


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        worldSeed = Random.value;
        CreateDicts();
    }

    /// <summary>
    /// вернуть блок(GameObject) согласно id
    /// </summary>
    /// <param name="blockType">id блока</param>
    /// <returns></returns>
    public GameObject BlockTypeToGameObject(BlockType blockType)
    {
        return TypesToObjects[blockType];
    }

    /// <summary>
    /// вернуть id блока согласно типу блока
    /// </summary>
    /// <param name="block">объект-блок</param>
    /// <returns></returns>
    public BlockType GameObjectToBlockType(GameObject block)
    {
        return ObjectsToTypes[block];
    }

    public BlockType LocationToBlockType(Location location)
    {
        return TypesByLocations[location];
    }

    public Vector2Int DirectionToVectors(Direction direction)
    {
        return VectorsByDirections[direction];
    }

}

public enum Direction
{
    None,

    Up,

    Down,

    Left,

    Right,
    
    UpRight,

    DownRight,

    UpLeft,

    DownLeft
}
