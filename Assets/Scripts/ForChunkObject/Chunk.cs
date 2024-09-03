using UnityEngine;

/// <summary>
/// Родительский объект для всех блоков в чанке
/// </summary>
public class Chunk : MonoBehaviour
{
    private int _blocksInChunk;
    private float _blockSize;

    public int _xPosition;
    public int _yPosition;
    private BlockStorage _storage;

    public ChunkData _chunkData;
    public ChunkData _newChunkData;

    private void Awake()
    {
        _storage = BlockStorage.Instance;
        _blocksInChunk = _storage.blocksInChunk;
        _blockSize = _storage.blockSize;
        _chunkData = new ChunkData();
        _newChunkData = new ChunkData();
    }

    /// <summary>
    /// передать координаты чанка (и другие параметры) для дальнейшей загрузки блоков
    /// </summary>
    /// <param name="blocksInChunk">количество блоков в чанке</param>
    /// <param name="blockSize">размер одного блока в единицах Unity</param>
    /// <param name="xPosition">x-индекс этого чанка</param>
    /// <param name="yPosition">y-индекс этого чанка</param>
    public ChunkData Initialize(int xPosition, int yPosition)
    {
        _xPosition = xPosition;
        _yPosition = yPosition;
        LoadChunk();
        return _newChunkData;
    }

    public void SubscribeBlock(BlockScript blockScript)
    {
        blockScript.onBlockStateChanged += OnBlockChanged;
    }

    public void UnsubscribeBlock(BlockScript blockScript)
    {
        blockScript.onBlockStateChanged -= OnBlockChanged;
    }

    /// <summary>
    /// загрузить массив объектов-блоков как дочерние объекты
    /// </summary>
    private void LoadChunk()
    {
        _chunkData = DataBaseHandler.LoadChunkFromDB(_xPosition, _yPosition);
        BlockType[,] blockTypes = _chunkData._blockTypes;
        CopyArray(_chunkData, _newChunkData);
        GameObject block;
        for (int i = 0; i < blockTypes.GetLength(0); i++)
        {
            for (int j = 0; j < blockTypes.GetLength(1); j++)
            {
                block = _storage.BlockTypeToGameObject(blockTypes[i, j]);
                if (block != null)
                {
                    GameObject newBlock = Instantiate(block, new Vector2(_xPosition * _blockSize * _blocksInChunk + i * _blockSize, -_yPosition * _blockSize * _blocksInChunk - j * _blockSize), Quaternion.identity, gameObject.transform);
                    newBlock.GetComponent<BlockScript>().Initialize(i, j, blockTypes[i, j]);
                }
            }
        }
    }

    private void CopyArray(ChunkData oldArray, ChunkData newArray)
    {
        newArray.xNumber = _xPosition;
        newArray.yNumber = _yPosition;
        BlockType[,] oldTypes = oldArray._blockTypes;
        BlockType[,] newTypes = newArray._blockTypes;
        for (int i = 0; i < oldTypes.GetLength(0); i++)
        {
            for (int j = 0; j < oldTypes.GetLength(1); j++)
            {
                newTypes[i, j] = oldTypes[i, j];
            }
        }
    }

    private void OnBlockChanged(int blockPositionX, int blockPositionY, BlockType newType)
    {
        _newChunkData._blockTypes[blockPositionX, blockPositionY] = newType;
    }

    private void OnDestroy()
    {
        if (_newChunkData != null && _chunkData != null)  
            DataBaseHandler.UpdateChunk(_chunkData, _newChunkData);
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            UnsubscribeBlock(gameObject.transform.GetChild(i).GetComponent<BlockScript>());
        }
    }

    private void OnApplicationQuit()
    {
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            UnsubscribeBlock(gameObject.transform.GetChild(i).GetComponent<BlockScript>());
        }
        if (_newChunkData != null && _chunkData != null)
            DataBaseHandler.UpdateChunk(_chunkData, _newChunkData);
    }

    #region вспомогательные функции

    private void VisualizeChunk(ChunkData data)
    {
        BlockType[,] types = data._blockTypes;
        for (int i = 0; i < types.GetLength(0); i++)
        {
            for (int j = 0; j < types.GetLength(1); j++)
            {
                Debug.Log(types[i, j]);
            }
        }
    }
    #endregion

}