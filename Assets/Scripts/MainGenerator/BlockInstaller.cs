using System.Collections.Generic;
using UnityEngine;

public class BlockInstaller : MonoBehaviour
{
    public GameObject block;
    [SerializeField] GameObject player;
    private Camera mainCamera;
    private BlockStorage storage;
    
    public float radius;
    public LayerMask layerMask;
    private float blockSize;
    private float chunkSize;

    public static BlockInstaller Instance { get; private set; }

    private void Awake()
    {
        mainCamera = Camera.main;
        if (Instance == null )
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        storage = BlockStorage.Instance;
        blockSize = storage.blockSize;
        chunkSize = storage.chunkSize;
    }

    // Update is called once per frame
    void Update()
    {
        OnMouseRightButtonClicked();
    }

    #region установка блоков пользователем

    /// <summary>
    /// реагирует на нажатие пользователем правой кнопки мыши (установка блоков) 
    /// </summary>
    private void OnMouseRightButtonClicked()
    {
        if (Input.GetMouseButtonDown(1) && Vector2.Distance(player.transform.position, mainCamera.ScreenToWorldPoint(Input.mousePosition)) <= 2)
        {
            Vector3 mousePosition = Input.mousePosition;
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            worldPosition.z = 0f;

            Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPosition, radius, layerMask);
            Collider2D collider = FindProperCollider(colliders, worldPosition);
            Vector2Int index = FindBlockIndex(collider, worldPosition);
            if (IfCanInstallBlock(collider, index))
            {
                float chunkPosX = collider.transform.position.x;
                float chunkPosY = collider.transform.position.y;
                GameObject newBlock = Instantiate(block, new Vector2(chunkPosX + index.x * blockSize, chunkPosY - index.y * blockSize), Quaternion.identity, collider.transform);
                newBlock.GetComponent<BlockScript>().Initialize(index.x, index.y, GetBlockType());
                newBlock.GetComponent<BlockScript>().BlockIsInstalled(BlockStorage.Instance.GameObjectToBlockType(block));
            }
        }
    }

    /// <summary>
    /// найти подходящий объект чанка среди ближайших найденных
    /// </summary>
    /// <param name="colliders">массив объектов-чанков</param>
    /// <param name="worldPosition">координаты нажатия</param>
    /// <returns></returns>
    private Collider2D FindProperCollider(Collider2D[] colliders, Vector3 worldPosition)
    {
        Collider2D result = colliders[0];
        List<Collider2D> properColliders = new List<Collider2D>();
        
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].transform.position.x <= worldPosition.x && colliders[i].transform.position.y >= worldPosition.y)
            {
                properColliders.Add(colliders[i]);
            }
        }
        float distance = 100f;
        foreach (Collider2D collider in properColliders)
        {
            float newDistance = Vector2.Distance(collider.transform.position, worldPosition);
            if ( newDistance <= distance )
            {
                result = collider;
                distance = newDistance;
            }
        }
        
        return result;
    }

    /// <summary>
    /// проверить позицию на возможность установки
    /// </summary>
    /// <param name="collider">объект-чанк</param>
    /// <param name="index">индекс блока в массиве</param>
    /// <returns></returns>
    private bool IfCanInstallBlock(Collider2D collider, Vector2Int index)
    {
        BlockType[,] blockTypes = collider.GetComponent<Chunk>()._chunkData._blockTypes;
        for (int i = index.x - 1; i <= index.x + 1; i += 2) 
        {
            if (i >= 0 && i < blockTypes.GetLength(0) && blockTypes[i, index.y] != BlockType.Empty)
            {
                return true;
            }
        }
        for (int i = index.y - 1; i <= index.y + 1; i += 2)
        {
            if (i >= 0 && i < blockTypes.GetLength(1) && blockTypes[index.x, i] != BlockType.Empty)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// найти индекс (позицию в массиве) для устанавливаемого блока
    /// </summary>
    /// <param name="collider">объект-чанк</param>
    /// <param name="worldPosition">коордната нажатия</param>
    /// <returns></returns>
    private Vector2Int FindBlockIndex(Collider2D collider, Vector3 worldPosition)
    {
        Vector3 offset = worldPosition - collider.transform.position;
        offset.x = Mathf.Abs(offset.x);
        offset.y = Mathf.Abs(offset.y);
        return new Vector2Int((int)(offset.x / blockSize), (int)(offset.y / blockSize));
    }

    /// <summary>
    /// вернуть id блока
    /// TODO: допилить!!!
    /// </summary>
    /// <returns></returns>
    private BlockType GetBlockType()
    {
        return BlockType.Water;
    }

    #endregion

    #region программные установка и удаление блоков
    public void InstantiateBlock(ChunkData newChunkData, int xPos, int yPos, BlockType blockType)
    {
        float xPosition = newChunkData.xNumber * chunkSize + xPos * blockSize;
        float yPosition = -newChunkData.yNumber * chunkSize - yPos * blockSize;
        GameObject chunk = FindChunkWithPosition(newChunkData.xNumber, newChunkData.yNumber);
        if (chunk != null)
        {
            GameObject newBlock = Instantiate(storage.BlockTypeToGameObject(blockType), new Vector2(xPosition, yPosition), Quaternion.identity, chunk.transform);
            newBlock.GetComponent<BlockScript>().Initialize(xPos, yPos, blockType);
            newBlock.GetComponent<BlockScript>().BlockIsInstalled(blockType);
        }
    }

    public void InstantiateBlock(int xPosition, int yPosition, int xPos, int yPos, BlockType blockType)
    {
        //float xPosition = newChunkData.xNumber * chunkSize + xPos * blockSize;
        //float yPosition = -newChunkData.yNumber * chunkSize - yPos * blockSize;

        float xRealPosition =  xPosition * chunkSize + xPos * blockSize;
        float yRealPosition = -yPosition * chunkSize - yPos * blockSize;

        GameObject chunk = FindChunkWithPosition(xPosition, yPosition);
        if (chunk != null)
        {
            GameObject newBlock = Instantiate(storage.BlockTypeToGameObject(blockType), new Vector2(xRealPosition, yRealPosition), Quaternion.identity, chunk.transform);
            newBlock.GetComponent<BlockScript>().Initialize(xPos, yPos, blockType);
            newBlock.GetComponent<BlockScript>().BlockIsInstalled(blockType);
        }
    }

    private GameObject FindChunkWithPosition(int xPosition, int yPosition)
    {
        Chunk[] allChunks = FindObjectsOfType<Chunk>();

        foreach(Chunk chunk in allChunks)
        {
            if (chunk._xPosition == xPosition && chunk._yPosition == yPosition)
            {
                return chunk.gameObject;
            }
        }

        return null;
    }


    public void DeleteBlock(ChunkData data, int xPos, int yPos)
    {
        GameObject chunk = FindChunkWithPosition(data.xNumber, data.yNumber);
        

        foreach (Transform child in chunk.transform)
        {
            BlockScript blockScript = child.GetComponent<BlockScript>();
            if (blockScript.xPosition == xPos && blockScript.yPosition == yPos)
            {
                Destroy(blockScript.gameObject);
                data._blockTypes[xPos, yPos] = BlockType.Empty;
                break;
            }
        }
    }

    public void DeleteBlock(int xPosition, int yPosition, int xPos, int yPos)
    {
        GameObject chunk = FindChunkWithPosition(xPosition, yPosition);

        foreach (Transform child in chunk.transform)
        {
            BlockScript blockScript = child.GetComponent<BlockScript>();
            if (blockScript.xPosition == xPos && blockScript.yPosition == yPos)
            {
                Destroy(blockScript.gameObject);
                chunk.GetComponent<Chunk>()._newChunkData._blockTypes[xPos, yPos] = BlockType.Empty;
                break;
            }
        }
    }

    #endregion 
}
