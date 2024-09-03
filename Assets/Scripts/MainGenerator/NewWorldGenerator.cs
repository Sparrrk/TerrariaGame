using UnityEngine;

public class NewWorldGenerator : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] GameObject chunkPosition;
    [SerializeField] int _worldLength = 50;
    [SerializeField] int _worldDepth = 50;
    [SerializeField] int _blocksInChunk = 16;

    private float _chunkSize;
    private BlockStorage _blockStorage;

    private ChunkInitialFiller initialFiller;

    private void Awake()
    {
        _blockStorage = BlockStorage.Instance;
        initialFiller = GetComponent<ChunkInitialFiller>();
    }

    private void Start()
    {
        _chunkSize = _blockStorage.chunkSize;
        WorldGenerator();
    }

    /// <summary>
    /// Сгенерировать игровой мир, состоящий из массива чанков
    /// </summary>
    private void WorldGenerator()
    {
        if (!System.IO.File.Exists(Application.dataPath + "ChunkDB.bytes"))
        {
            DataBaseHandler.Initialize();
            initialFiller.CreateTheWorld(_worldLength * _blocksInChunk, _worldDepth * _blocksInChunk);
        }
        else
        {
            DataBaseHandler.Initialize();
        }
        Vector2Int playerPos = GetPlayerChunkPosition();
        LoadAdjacentChunks(playerPos);
    }

    /// <summary>
    /// Определяет, в каком чанке находится игрок</summary>
    /// <returns>вектор, соответствующий положению игрока</returns>
    private Vector2Int GetPlayerChunkPosition()
    {
        Vector2 position = player.gameObject.transform.position;
        int x = (int)(position.x / _chunkSize);
        int y = (int)(position.y / _chunkSize);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// выгружает из базы данных несколько наиболее близких к игроку чанков
    /// </summary>
    /// <param name="playerPos">позиция игрока</param>
    private void LoadAdjacentChunks(Vector2Int playerPos)
    {
        ChunkNotifier notifier = GetComponent<ChunkNotifier>();
        for (int i = playerPos.x - 1; i <= playerPos.x + 1; i++)
        {
            for (int j = playerPos.y - 1; j <= playerPos.y + 1; j++)
            {
                if (i >= 0 && i < _worldLength && j >= 0 && j < _worldDepth)
                    notifier.LoadOneChunk(i, j);
            }
        } 
    }

    private void OnApplicationQuit()
    {
        CloseDBFile();
    }

    #region вспомогательные функции

    /// <summary>
    /// удалить файл базы данных, содержащий информацию о чанках
    /// </summary>
    private void CloseDBFile()
    {
        DataBaseHandler.CloseDB();
    }

    #endregion
}
