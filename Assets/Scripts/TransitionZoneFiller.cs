using UnityEngine;

public class TransitionZoneFiller : MonoBehaviour
{
    [SerializeField] BlockStorage storage;
    private Perlin perlin;
    private int chunksInZone = 3;
    private Location currentLocation = Location.None;
    private int transitionZoneWidht;
    private BlockType[,] transitionArray;

    private int blocksInChunk;

    private void Awake()
    {
        perlin = new Perlin();
        blocksInChunk = storage.blocksInChunk;
        transitionZoneWidht = blocksInChunk * chunksInZone;
    }

    private void CreateTransitionZone(BlockType block1, BlockType block2, Location location)
    {
        if (currentLocation != location)
        {
            currentLocation = location;
            transitionArray = new BlockType[transitionZoneWidht, transitionZoneWidht];
            int[] layerHeight = new int[transitionZoneWidht];

            float seed = Random.Range(0.1f, 0.2f);
            for (int i = 0; i < transitionZoneWidht; i++)
            {
                layerHeight[i] = (int)(Mathf.PerlinNoise1D(seed * i) * (transitionZoneWidht - i));

                for (int j = 0; j < layerHeight[i]; j++)
                    transitionArray[i, j] = block1;
                
                for (int j = layerHeight[i]; j < transitionZoneWidht; j++)
                    transitionArray[i, j] = block2;
            }
        }
    }

    public BlockType[,] FillTheTransitionChunk(Vector2Int number, BlockType block1, BlockType block2, Location location)
    {
        CreateTransitionZone(block1, block2, location);

        number = DefineNumber(number);

        BlockType[,] result = CopyArray(number);

        return result;
    }

    private Vector2Int DefineNumber(Vector2Int number)
    {
        int x = number.x;
        int y = number.y;

        if (x >= storage.firstBiomBorder && x < storage.firstTransitionBorder) { x %= storage.firstBiomBorder; }
        else if (x >= storage.secondBiomBorder && x < storage.secondTransitionBorder) { x %= storage.secondBiomBorder; }

        y -= storage.surfaceLevel;
        return new Vector2Int(x, y);
    }


    private void VisualizeChunk(BlockType[,] array)
    {
        for (int i = 0; i < array.GetLength(0); i++)
        {
            Debug.Log(array[i, 0]);
        }
    }

    private BlockType[,] CopyArray(Vector2Int number)
    {
        BlockType[,] result = new BlockType[blocksInChunk, blocksInChunk];
        int xNumber = number.x;
        int yNumber = number.y;
        for (int i = 0; i < blocksInChunk; i++)
        {
            for (int j = 0; j < blocksInChunk; j++)
            {
                result[i, j] = transitionArray[xNumber * blocksInChunk + i, yNumber * blocksInChunk + j];
            }
        }
        return result;
    }
}

public enum Location
{
    None,

    Forest,

    IceLand,

    Desert,

    ForestDesert,

    DesertForest,

    DesertIceland,

    IceLandDesert,

    IceLandForest,

    ForestIceLand,

    TransitionZone
}
