using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    [SerializeField] BlockStorage blockStorage;
    private Dictionary<Vector2Int, ChunkData> oldChunks = new Dictionary<Vector2Int, ChunkData>();
    private Dictionary<Vector2Int, ChunkData> newChunks = new Dictionary<Vector2Int, ChunkData>();

    private List<Vector2Int> positions = new List<Vector2Int>();
    private int StepAmount = 3;
    private int blocksInChunk = 0;
    private int cavesAmount = 3;

    private void Awake()
    {
        blocksInChunk = blockStorage.blocksInChunk;
    }

    public void AddTheCaves()
    {
        for (int i = 0; i < cavesAmount * 2; i++)
        {
            PickRandomChunk();
        }
        while(positions.Count > 0)
        {
            List<Vector2Int> cavePositions = DefineCavePosition(positions);
            CreateTheCave(cavePositions[0], cavePositions[1]);
        }
    }

    private List<Vector2Int> DefineCavePosition(List<Vector2Int> positions)
    {
        Vector2Int first = positions[0];
        Vector2Int second = positions[1];
        float minDistance = Vector2.Distance(first, second);
        for (int i = 2; i < positions.Count; i++)
        {
            if (Vector2.Distance(first, positions[i]) < minDistance)
            {
                minDistance = Vector2.Distance(first, positions[i]);
                second = positions[i];
            }
        }
        Debug.Log("cave position = " + first);
        Debug.Log("cave position = " + second);
        positions.Remove(first);
        positions.Remove(second);
        return new List<Vector2Int>() { first, second };
    }

    public void GenerateCavesInChunk()
    {
        for (int i = 1; i < blockStorage.worldLength - 1; i++)
        {
            for (int j = blockStorage.surfaceLevel; j < blockStorage.worldDepth - 1; j++)
            {
                GenerateCaveInChunk(i, j, blockStorage.blocksInChunk, 0.1f);
            }
        }
    }

    private void GenerateCaveInChunk(int xPos, int yPos, int blocksInChunk, float seed)
    {
        int octaves = 2;
        ChunkData oldChunk = DataBaseHandler.LoadChunkFromDB(xPos, yPos);
        ChunkData newChunk = new ChunkData(oldChunk);
        BlockType[,] array = newChunk._blockTypes;
        float perlinVar = 0f;

        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                perlinVar = 0;
                float totalAmplitude = 0;
                float frequency = 1f;
                float amplitude = 1f;
                for (int k = 0; k < octaves; k++)
                {
                    float x = (xPos * blocksInChunk + i) * seed * frequency;
                    float y = (yPos * blocksInChunk + j) * seed * frequency;

                    perlinVar += Mathf.PerlinNoise(x, y) * amplitude;

                    frequency *= 2;
                    totalAmplitude += amplitude;
                    amplitude /= 2;
                }
                perlinVar /= totalAmplitude;
                if (perlinVar > 0.4 && perlinVar < 0.6 || perlinVar > 0.8)
                {
                    array[i, j] = BlockType.Empty;
                }
            }
        }
        SmoothCave(ref newChunk);
        DataBaseHandler.UpdateChunk(oldChunk, newChunk);
    }

    private void SmoothCave(ref ChunkData chunkData)
    {
        int adjacentCellCount;
        BlockType[,] array = chunkData._blockTypes;
        //BlockType[,] array = new BlockType[oldArray.GetLength(0), oldArray.GetLength(1)];
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                adjacentCellCount = CountAdjacentCells(array, i, j);
                if (IsItBorder(i, j) && adjacentCellCount < 3)
                {
                    array[i, j] = BlockType.Empty;
                }
                else if (IsItBorder(i, j) && adjacentCellCount > 3)
                {
                    array[i, j] = BlockType.Earth;
                }
                else if (adjacentCellCount > 4)
                {
                    array[i, j] = BlockType.Earth;
                }
                else if (adjacentCellCount < 4)
                {
                    array[i, j] = BlockType.Empty;
                }
                else
                {
                    continue;
                }
            }
        }
        chunkData._blockTypes = array;

    }

    private bool IsItBorder(int x, int y)
    {
        return (x == 0 || x >= 15 || y == 0 || y >= 15);
    }

    private int CountAdjacentCells(BlockType[,] array, int x, int y)
    {
        int count = 0;
        for (int i = x - 1; i <= x + 1; i++)
        {
            for (int j = y - 1; j <= y + 1; j++)
            {
                if (i < 0 || i >= array.GetLength(0) || j < 0 || j >= array.GetLength(1))
                {
                    continue;
                }
                else if (array[i, j] != BlockType.Empty && (i != x || j != y) )
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void CreateTheCave(Vector2Int first, Vector2Int second)
    {
        ChunkData firstData = DataBaseHandler.LoadChunkFromDB(first.x, first.y);
        //ChunkData secondData = DataBaseHandler.LoadChunkFromDB(second.x, second.y);

        Vector2Int direction;
        oldChunks.Add(first, firstData);
        ChunkData currentChunk = new ChunkData(firstData);
        int currentX = blockStorage.blocksInChunk / 2;
        int currentY = blockStorage.blocksInChunk / 2;

        while (true)
        {
            direction = DefineDirection(new Vector2Int(currentChunk.xNumber, currentChunk.yNumber), second, currentX, currentY);
            if (direction.sqrMagnitude < 5) break;
            MakeStep(direction, ref currentChunk, ref currentX, ref currentY);
        }
        RemoveUnnecessaryCells();
        UpdateAllChunks();
        ClearAllDictionaries();
    }

    private void PickRandomChunk()
    {
        int x = Random.Range(1, blockStorage.worldLength - 2);
        int y = Random.Range(blockStorage.surfaceLevel, blockStorage.worldDepth - 2);
        if (positions.Contains(new Vector2Int(x, y)))
        {
            PickRandomChunk();
        }
        else
        {
            //ChunkData chunkData = DataBaseHandler.LoadChunkFromDB(x, y);
            positions.Add(new Vector2Int(x, y));
            //oldChunks.Add(new Vector2Int(x, y), chunkData);
        }
    }

    private Vector2Int DefineDirection(Vector2Int startPoint, Vector2Int endPoint, int currentX, int currentY)
    {
        int distanceInChunksX = endPoint.x - startPoint.x;
        int distanceInChunksY = endPoint.y - startPoint.y;

        int distanceInBlocksX = distanceInChunksX * blocksInChunk + (blocksInChunk / 2 - currentX);
        int distanceInBlocksY = distanceInChunksY * blocksInChunk + (blocksInChunk / 2 - currentY);

        return new Vector2Int(distanceInBlocksX, distanceInBlocksY);
    }

    private void MakeStep(Vector2Int direction, ref ChunkData chunk, ref int currentX, ref int currentY)
    {
        MakeDefinedStep(direction, ref chunk, ref currentX, ref currentY);
        MakeUndefinedStep(ref chunk, ref currentX, ref currentY);
    }

    private void MakeDefinedStep(Vector2Int direction, ref ChunkData chunk, ref int currentX, ref int currentY)
    {
        chunk._blockTypes[currentX, currentY] = BlockType.Empty;
        if (direction.y == 0 || Random.value < Mathf.Abs(direction.x / direction.y))
        {
            currentX += direction.x / Mathf.Abs(direction.x);
        }
        if (direction.x == 0 || Random.value < Mathf.Abs(direction.y / direction.x))
        {
            currentY += direction.y / Mathf.Abs(direction.y);
        }

        CheckTheBorders(ref chunk, ref currentX, ref currentY);
    }

    private void MakeUndefinedStep(ref ChunkData chunk, ref int currentX, ref int currentY)
    {
        Vector2Int shift = new Vector2Int(0, 0);
        for (int i = 0; i < StepAmount; i++)
        {
            chunk._blockTypes[currentX, currentY] = BlockType.Empty;
            switch (Random.Range(0, 4))
            {
                case 0: { shift = new Vector2Int(1, 0); break; }
                case 1: { shift = new Vector2Int(-1, 0); break; }
                case 2: { shift = new Vector2Int(0, 1); break; }
                case 3: { shift = new Vector2Int(0, -1); break; }
            }
            currentX += shift.x;
            currentY += shift.y;
            CheckTheBorders(ref chunk, ref currentX, ref currentY);
        }
    }

    private void RemoveUnnecessaryCells()
    {
        foreach (KeyValuePair<Vector2Int, ChunkData> keyValuePair in newChunks)
        {
            RemoveUnnecessaryCellsInChunk(keyValuePair.Value);
        }
    }

    private void RemoveUnnecessaryCellsInChunk(ChunkData chunk)
    {
        int emptyCellsAmount = 0;
        BlockType[,] array = chunk._blockTypes;
        for (int i = 0; i < array.GetLength(0); i++)
        {
            for (int j = 0; j < array.GetLength(1); j++)
            {
                emptyCellsAmount = 0;
                if (array[i, j] != BlockType.Empty)
                    emptyCellsAmount = CountAdjacentCell(array, i, j);
                if (emptyCellsAmount >= 13)
                    array[i, j] = BlockType.Empty;
            }
        }
    }

    private int CountAdjacentCell(BlockType[,] array, int currentX, int currentY)
    {
        int counter = 0;
        int radius = 2;
        for (int i = currentX - radius; i < currentX + radius; i++)
        {
            for (int j = currentY - radius; j < currentY + radius; j++)
            {
                if (i < 0 || i >= blocksInChunk || j < 0 || j >= blocksInChunk)
                    continue;

                if (array[i, j] == BlockType.Empty)
                    counter++;
            }
        }
        return counter;
    }

    private void CheckTheBorders(ref ChunkData chunk, ref int currentX, ref int currentY)
    {
        int stepX = 0;
        int stepY = 0;

        if (currentX >= 16) { stepX = 1; currentX = 0; }
        else if (currentX < 0) { stepX = -1; currentX = 15; }

        if (currentY >= 16) { stepY = 1; currentY = 0; }
        else if (currentY < 0) { stepY = -1; currentY = 15; }

        if (stepX != 0 || stepY != 0)
        {
            SaveChunk(chunk);
            GetNextChunk(ref chunk, new Vector2Int(stepX, stepY));
        }
    }

    private void SaveChunk(ChunkData chunk)
    {
        if (!newChunks.ContainsKey(new Vector2Int(chunk.xNumber, chunk.yNumber)))
            newChunks.Add(new Vector2Int(chunk.xNumber, chunk.yNumber), chunk);
    }

    private void GetNextChunk(ref ChunkData currentChunk, Vector2Int direction)
    {
        Vector2Int position = new Vector2Int(currentChunk.xNumber + direction.x, currentChunk.yNumber + direction.y);

        if (newChunks.ContainsKey(position))
        {
            currentChunk = newChunks[position];
        }
        else
        {
            currentChunk = DataBaseHandler.LoadChunkFromDB(position.x, position.y);
            oldChunks.Add(position, currentChunk);
            currentChunk = new ChunkData(currentChunk);
        }
    }

    private void UpdateAllChunks()
    {
        foreach (KeyValuePair<Vector2Int, ChunkData> keyValuePair in newChunks)
        {
            DataBaseHandler.UpdateChunk(oldChunks[keyValuePair.Key], keyValuePair.Value);
        }
    }

    private void ClearAllDictionaries()
    {
        oldChunks.Clear();
        newChunks.Clear();
    }

}
