using System;
using System.Collections;
using System.Collections.Generic;
using System.EnterpriseServices;
using UnityEngine;

public class WaterFlowScript : MonoBehaviour
{
    [SerializeField] BlockType nextBlockRight;
    [SerializeField] BlockType nextBlockLeft;

    [SerializeField] int typeOfCurrentBlock;
    [SerializeField] BlockType typeOfMotherBlock;

    private BlockStorage blockStorage;
    [SerializeField] GameObject waterBlock;
    private BlockInstaller blockInstaller;
    private ChunkNotifier notifier;
    private Chunk parent;

    public ChunkData data;
    private BlockType[,] blockTypes;
    private BlockType thisType;

    private int _xPos;
    private int _yPos;
    private int _xPosition;
    private int _yPosition;
    private int blocksInChunk;
    private float blockSize;
    private BlockScript blockScript;
    private bool initialized = false;
    Dictionary<Vector2Int, ChunkData> loadedChunks;

    private static List<BlockType> incompleteBlocks = new List<BlockType>{ BlockType.Water80R, BlockType.Water80L, BlockType.Water60R, BlockType.Water60L, BlockType.Water40R, BlockType.Water40L, BlockType.Water20R, BlockType.Water20L };

    // Start is called before the first frame update
    void Start()
    {
        blockStorage = BlockStorage.Instance;
        parent = transform.parent.GetComponent<Chunk>();
        blockInstaller = BlockInstaller.Instance;
        notifier = ChunkNotifier.Instance;
        loadedChunks = notifier.loadedChunks;
        data = parent._newChunkData;
        blockTypes = data._blockTypes;
        blocksInChunk = blockStorage.blocksInChunk;
        blockSize = blockStorage.blockSize;
        blockScript = GetComponent<BlockScript>();
    }

    public void Initialize()
    {
        _xPos = GetComponent<BlockScript>().xPosition;
        _yPos = GetComponent<BlockScript>().yPosition;
        _xPosition = parent._xPosition; 
        _yPosition = parent._yPosition;
        thisType = blockTypes[_xPos, _yPos];
        initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (blockScript.initialized && !initialized)
        {
            Initialize();
            StartCoroutine(UpdateTheCells());
        }
    }

    private IEnumerator UpdateTheCells()
    {
        yield return new WaitForSeconds(0.5f);
        UpdateTheCellBelow();
        yield return new WaitForSeconds(0.1f);
        UpdateCellsLRight();
        UpdateCellsLeft();
        yield return new WaitForSeconds(0.1f);
        DeleteWaterBlocks();
        StartCoroutine(UpdateTheCells());
    }

    private void UpdateTheCellBelow()
    {
        if (loadedChunks.ContainsKey(new Vector2Int(_xPosition, _yPosition + (_yPos + 1) / blocksInChunk )))
        {
            int yChunkPosition = _yPosition + (_yPos + 1) / blocksInChunk;
            BlockType blockBelow = loadedChunks[new Vector2Int(_xPosition, _yPosition + (_yPos + 1) / blocksInChunk)]._blockTypes[_xPos, (_yPos + 1) % blocksInChunk];
            if (blockBelow == BlockType.Empty)
            {
                blockInstaller.InstantiateBlock(_xPosition, yChunkPosition, _xPos, (_yPos + 1) % blocksInChunk, BlockType.WaterMimic);
            }
            else if (incompleteBlocks.Contains(blockBelow))
            {
                blockInstaller.DeleteBlock(_xPosition, yChunkPosition, _xPos, (_yPos + 1) % blocksInChunk);
                blockInstaller.InstantiateBlock(_xPosition, yChunkPosition, _xPos, (_yPos + 1) % blocksInChunk, BlockType.WaterMimic);
            }
        }
    }

    private bool IfCanInstantiateBlockRightAndLeft()
    {
        if (loadedChunks.ContainsKey(new Vector2Int(_xPosition, _yPosition + (_yPos + 1) / blocksInChunk)))                                                                    // Если чанк с блоком снизу загружен на сцену
        {
            BlockType blockBelow = loadedChunks[new Vector2Int(_xPosition, _yPosition + (_yPos + 1) / blocksInChunk)]._blockTypes[_xPos, (_yPos + 1) % blocksInChunk];         // Узнать тип блока снизу
            if (blockBelow != BlockType.Empty && blockBelow != BlockType.Water && !incompleteBlocks.Contains(blockBelow) && blockBelow != BlockType.WaterMimic)                                                      // Если блок снизу не пустой и не вода
            {
                return true;
            }
        }
        return false;
    }

    private void UpdateCellsLRight()
    {
        if (IfCanInstantiateBlockRightAndLeft())                                                                                                                           // Если установка блока справа возможна
        {
            if (loadedChunks.ContainsKey(new Vector2Int(_xPosition + (_xPos + 1) / blocksInChunk, _yPosition)) && nextBlockRight != BlockType.Empty)                       // если чанк c блоком справа загружен на сцену и текущий блок может "литься дальше"
            {
                int xChunkPosition = _xPosition + (_xPos + 1) / blocksInChunk;
                BlockType blockRight = loadedChunks[new Vector2Int(xChunkPosition, _yPosition)]._blockTypes[(_xPos + 1) % blocksInChunk, _yPos];                           // Узнать тип блока справа
                if (blockRight == BlockType.Empty)                                                                                                                         // Если блок пустой и блок
                {
                    blockInstaller.InstantiateBlock(xChunkPosition, _yPosition, (_xPos + 1) % blocksInChunk, _yPos, nextBlockRight);                                       // Создать блок воды справа
                }
                else if (incompleteBlocks.IndexOf(blockRight) > incompleteBlocks.IndexOf(nextBlockRight))                                                                  // Если справа блок воды, уровень которого меньше уровня устанавливаемого блока
                {
                    blockInstaller.DeleteBlock(xChunkPosition, _yPosition, (_xPos + 1) % blocksInChunk, _yPos);                                                            // Удалить блок справа
                    blockInstaller.InstantiateBlock(xChunkPosition, _yPosition, (_xPos + 1) % blocksInChunk, _yPos, nextBlockRight);                                       // Создать блок воды справа
                }
            }
        }
    }

    private void UpdateCellsLeft()
    {
        if (IfCanInstantiateBlockRightAndLeft())                                                                                                                                                      // Если установка блока слева возможна
        {
            if (loadedChunks.ContainsKey(new Vector2Int(_xPosition - (blocksInChunk - _xPos) / blocksInChunk, _yPosition)) && nextBlockLeft != BlockType.Empty)                                       // если чанк с блоком справа загружен на сцену и текущий блок может "литься дальше"
            {
                int xChunkPosition = _xPosition - (blocksInChunk - _xPos) / blocksInChunk;                                                                                                            // определить номер чанка для блока слева (нужен для граничного случая)
                BlockType blockLeft = loadedChunks[new Vector2Int(_xPosition - (blocksInChunk - _xPos) / blocksInChunk, _yPosition)]._blockTypes[(blocksInChunk - 1 + _xPos) % blocksInChunk, _yPos]; // узнать тип блока слева
                if (blockLeft == BlockType.Empty)                                                                                                                                                     // если блок слева пустой
                {
                    blockInstaller.InstantiateBlock(xChunkPosition, _yPosition, (blocksInChunk - 1 + _xPos) % blocksInChunk, _yPos, nextBlockLeft);                                                   // создать блок воды слева
                }
                else if (incompleteBlocks.IndexOf(blockLeft) > incompleteBlocks.IndexOf(nextBlockLeft))                                                                                               // если слева блок воды, уровень которого меньше уровня устанавливаемого блока
                {
                    blockInstaller.DeleteBlock(xChunkPosition, _yPosition, (blocksInChunk - 1 + _xPos) % blocksInChunk, _yPos);                                                                       // удалить блок слева
                    blockInstaller.InstantiateBlock(xChunkPosition, _yPosition, (blocksInChunk - 1 + _xPos) % blocksInChunk, _yPos, nextBlockLeft);                                                   // Создать блок воды слева
                }
            }
        }
    }

    private void DeleteWaterBlocks()
    {
        switch(typeOfCurrentBlock)
        {
            case 0:
                StartCoroutine(DeleteSourceWaterBlock());
                break;

            case 1:
                StartCoroutine(DeleteIncompleteLeftBlock());
                break;

            case 2:
                StartCoroutine(DeleteIncompleteRightBlock());
                break;

            case 3:
                StartCoroutine(DeleteMimicWaterBlock());
                break;
        }
    }

    private IEnumerator DeleteSourceWaterBlock()
    {
        Vector2Int belowChunk = new Vector2Int(_xPosition, _yPosition + (_yPos + 1) / blocksInChunk);                            // определить координаты чанка для блока снизу;
        Vector2Int leftChunk = new Vector2Int(_xPosition - (blocksInChunk - _xPos) / blocksInChunk, _yPosition);                 // определить координаты чанка для блока слева;
        Vector2Int rightChunk = new Vector2Int(_xPosition + (_xPos + 1) / blocksInChunk, _yPosition);                            // определить координаты чанка для блока справа;
        int counter = 0;                                                                                                         // инициализировать счетчик "пустых блоков-соседей";
        if (loadedChunks.ContainsKey(belowChunk))                                                                                // если чанк с блоком снизу загружен на сцену
        {
            BlockType blockBelow = loadedChunks[belowChunk]._blockTypes[_xPos, (_yPos + 1) % blocksInChunk];                     // определить тип блока снизу;
            if (blockBelow == BlockType.WaterMimic || blockBelow == BlockType.Empty)                                             // если блок снизу - вода или пустой блок
                counter++;                                                                                                       // +счетчик (блок воды не может "цепляться" за пустоту);
        }
        if (loadedChunks.ContainsKey(rightChunk))                                                                                // если чанк с блоком справа загружен на сцену
        {
            BlockType blockRight = loadedChunks[rightChunk]._blockTypes[(_xPos + 1) % blocksInChunk, _yPos];                     // определить тип блока справа;
            if (blockRight == BlockType.WaterMimic || incompleteBlocks.Contains(blockRight) || blockRight == BlockType.Empty)    // если блок справа  - вода (в любом виде) или пустой блок
                counter++;                                                                                                       // +счетчик;
        }
        if (loadedChunks.ContainsKey(leftChunk))                                                                                 // если чанк с блоком слева загружен на сцену
        {
            BlockType blockLeft = loadedChunks[leftChunk]._blockTypes[(blocksInChunk - 1 + _xPos) % blocksInChunk, _yPos];       // определить тип блока слева;
            if (blockLeft == BlockType.WaterMimic || incompleteBlocks.Contains(blockLeft) || blockLeft == BlockType.Empty)       // если блок слева - вода (в любом виде) или пустой блок
                counter++;                                                                                                       // +счетчик;
        }
        if (counter == 3)                                                                                                        // если счетчик = 3 (цепляться блоку не за что)
        {
            yield return new WaitForSeconds(0.2f);
            blockInstaller.DeleteBlock(_xPosition, _yPosition, _xPos, _yPos);                                                    // удалить данный блок
        }
    }

    /// <summary>
    /// удалить неполные блоки воды при выполнении условий
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeleteIncompleteLeftBlock()
    {
        Vector2Int rightChunk = new Vector2Int(_xPosition + (_xPos + 1) / blocksInChunk, _yPosition);                            // определить координаты чанка для блока справа
        if (loadedChunks.ContainsKey(rightChunk))                                                                                // если чанк с блоком справа загружен на сцену
        {
            BlockType blockRight = loadedChunks[rightChunk]._blockTypes[(_xPos + 1) % blocksInChunk, _yPos];                     // определить тип блока справа
            if (blockRight != typeOfMotherBlock && blockRight != BlockType.Water)                                                // если справа нет "родительского" блока (который породил данный блок)
            {
                yield return new WaitForSeconds(0.2f);                                                                           // ждать 0.2 сек (чтобы все блоки воды не исчезали мгновенно)
                blockInstaller.DeleteBlock(_xPosition, _yPosition, _xPos, _yPos);                                                // удалить данный блок 
            }
        }
    }

    /// <summary>
    /// удалить неполные блоки воды при выполнении условий
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeleteIncompleteRightBlock()
    {
        Vector2Int leftChunk = new Vector2Int(_xPosition - (blocksInChunk - _xPos) / blocksInChunk, _yPosition);                 // определить координаты чанка для блока справа
        if (loadedChunks.ContainsKey(leftChunk))                                                                                 // если чанк с блоком слева загружен на сцену
        {
            BlockType blockLeft = loadedChunks[leftChunk]._blockTypes[(blocksInChunk + _xPos - 1) % blocksInChunk, _yPos];       // определить тип блока слева
            if (blockLeft != typeOfMotherBlock && blockLeft != BlockType.Water)                                                  // если слева нет "родительского" блока (который породил данный блок
            {
                yield return new WaitForSeconds(0.2f);                                                                           // ждать 0.2 сек (чтобы все блоки воды не исчезали мгновенно)
                blockInstaller.DeleteBlock(_xPosition, _yPosition, _xPos, _yPos);                                                // удалить данный блок
            }
        }
    }


    /// <summary>
    /// удалить полный блок воды, не являющийся источником
    /// </summary>
    /// <returns></returns>
    private IEnumerator DeleteMimicWaterBlock()
    {
        Vector2Int upperChunk = new Vector2Int(_xPosition, _yPosition - (blocksInChunk - _yPos) / blocksInChunk);       
        if (loadedChunks.ContainsKey(upperChunk))
        {
            BlockType upperBlock = loadedChunks[upperChunk]._blockTypes[_xPos, (blocksInChunk + _yPos - 1) % blocksInChunk];
            if (upperBlock != BlockType.Water && upperBlock != BlockType.WaterMimic && !incompleteBlocks.Contains(upperBlock))
            {
                yield return new WaitForSeconds(0.2f);
                blockInstaller.DeleteBlock(_xPosition, _yPosition, _xPos, _yPos);
            }
        }
    }

}
