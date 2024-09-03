using System.Collections.Generic;
using UnityEngine;

public class ChunkNotifier : MonoBehaviour
{
    [SerializeField] GameObject chunkPosition;

    public Dictionary<Vector2Int, ChunkData> loadedChunks = new Dictionary<Vector2Int, ChunkData>();

    public static ChunkNotifier Instance { get; private set; }

    private float _chunkSize;

    private void Awake()
    {
        _chunkSize = BlockStorage.Instance.chunkSize;
        if (Instance == null )
        {
            Instance = this;
            DontDestroyOnLoad( gameObject );
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// подписывает метод "OnEventHappened" на события, передаваемые чанками
    /// </summary>
    public void SubscribeChunk(DistanceChecker distanceChecker)
    {
        distanceChecker.PlayerPositionChanged += OnEventHappened;
        
    }

    /// <summary>
    /// Метод, реагирующий на события, передаваемые объектами - чанками. При достаточном приближении игрока к чанку выгружает на сцену
    /// соседние чанки, при удалении – удаляет чанк со сцены.
    /// </summary>
    /// <param name="xPos">позиция x вызывающего чанка</param>
    /// <param name="yPos">позиция y вызывающего чанка</param>
    private void OnEventHappened(int xPos, int yPos, bool playerGetsClose, GameObject sender)
    {
        if (playerGetsClose)
        {
            LoadAdjacentChunks(xPos, yPos);
        }
        else
        {
            DeleteChunk(xPos, yPos, sender);
        }
    }


    /// <summary>
    /// загружает на сцену несколько наиболее близких чанков
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="yPos"></param>
    private void LoadAdjacentChunks(int xPos, int yPos)
    {
        for (int i = xPos - 1; i <= xPos + 1; i++)
        {
            for (int  j = yPos - 1; j <= yPos + 1; j++)
            {
                if (!loadedChunks.ContainsKey(new Vector2Int(i, j)) && i >= 0 && j >= 0 )
                {
                    LoadOneChunk(i, j);
                }
            }
        }
    }



    /// <summary>
    /// загружает на сцену один чанк с выбранными координатами
    /// </summary>
    /// <param name="chunkPositionX">позиция чанка по оси x</param>
    /// <param name="chunkPositionY">позиция чанка по оси y</param>
    public void LoadOneChunk(int chunkPositionX, int chunkPositionY)
    {
        GameObject chunk = Instantiate(chunkPosition, new Vector2(_chunkSize * chunkPositionX, -_chunkSize * chunkPositionY), Quaternion.identity);
        DistanceChecker distanceChecker = chunk.GetComponent<DistanceChecker>();
        distanceChecker.Initialize(chunkPositionX, chunkPositionY);
        ChunkData data = chunk.GetComponent<Chunk>().Initialize(chunkPositionX, chunkPositionY);
        loadedChunks.Add(new Vector2Int(chunkPositionX, chunkPositionY), data);
    }

    private void DeleteChunk(int xPos, int yPos, GameObject sender)
    {
        sender.GetComponent<DistanceChecker>().PlayerPositionChanged -= OnEventHappened;
        Destroy(sender);
        loadedChunks.Remove(new Vector2Int(xPos, yPos));
    }
}
