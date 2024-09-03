using UnityEngine;

public class BlockScript : MonoBehaviour
{
    
    public delegate void blockStateChanged(int xPos, int yPos, BlockType newId);
    /// <summary>
    /// событие, сигнализирующее о изменении состояния блока. newState = true - блок установлен, newState = false - блок удален со сцены 
    /// </summary>
    public event blockStateChanged onBlockStateChanged;

    /// <summary>
    /// x-позиция блока относительно положения чанка
    /// </summary>
    public int xPosition;
    /// <summary>
    /// y-позиция блока относительно положения чанка
    /// </summary>
    public int yPosition;
    public BlockType _blockType;
    public bool initialized = false;
    Chunk parent;
    /// <summary>
    /// позиция чанка(родительского объекта)
    /// </summary>
    private int chunkPosX;
    /// <summary>
    /// позиция чанка(родительского объекта)
    /// </summary>
    private int chunkPosY;

    private void Awake()
    {
        parent = transform.parent.GetComponent<Chunk>();
    }

    public void Initialize(int xPos, int yPos, BlockType blockType)
    {
        xPosition = xPos;
        yPosition = yPos;
        _blockType = blockType;
        parent.SubscribeBlock(this);
        initialized = true;
    }

    // Start is called before the first frame update
    private void Start()
    {
        parent = transform.parent.GetComponent<Chunk>();
        chunkPosX = parent._xPosition;
        chunkPosY = parent._yPosition;
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            onBlockStateChanged?.Invoke(xPosition, yPosition, BlockType.Empty);
            Destroy(gameObject);
        }
    }

    public void BlockIsInstalled(BlockType blockType)
    {
        onBlockStateChanged?.Invoke(xPosition, yPosition, blockType);
    }

}
