using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using JetBrains.Annotations;
using System.Collections.Generic;

public static class DataBaseHandler
{
    public static SqliteConnection dbConnection;
    private static string path = Application.dataPath + "ChunkDB.bytes";

    public static void Initialize()
    {
        SetConnection();
        CreateTable();
        CreateIndex();
    }

    /// <summary>
    /// создать подключение к Ѕƒ
    /// </summary>
    private static void SetConnection()
    {
        dbConnection = new SqliteConnection("URI=file:" + path);
        dbConnection.Open();
    }

    /// <summary>
    /// создать таблицу
    /// </summary>
    private static void CreateTable()
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = @"CREATE TABLE IF NOT EXISTS ChunkTable (
        id INTEGER PRIMARY KEY,
        xChunkPosition INTEGER,
        yChunkPosition INTEGER,
        xIndex INTEGER,
        yIndex INTEGER,
        code INTEGER
        )";
        dbCommand.ExecuteNonQuery();
    }


    /// <summary>
    /// добавить информацию о блоке в базу данных
    /// </summary>
    /// <param name="xPos">x-индекс чанка</param>
    /// <param name="yPos">y-индекс чанка</param>
    /// <param name="xIndex">x-индекс блока в массиве</param>
    /// <param name="yIndex">y-индекс блока в массиве</param>
    /// <param name="code">id блока</param>
    private static void AddBlock(int xPos, int yPos, int xIndex, int yIndex, int code)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "INSERT INTO ChunkTable (xChunkPosition, yChunkPosition, xIndex, yIndex, code) VALUES (@xChunkPosition, @yChunkPosition, @xIndex, @yIndex, @code)";
            //"(@xChunkPosition, @yChunkPosition, @xIndex, @yIndex, @code)";
        dbCommand.Parameters.Add(new SqliteParameter("@xChunkPosition", xPos));
        dbCommand.Parameters.Add(new SqliteParameter("@yChunkPosition", yPos));
        dbCommand.Parameters.Add(new SqliteParameter("@xIndex", xIndex));
        dbCommand.Parameters.Add(new SqliteParameter("@yIndex", yIndex));
        dbCommand.Parameters.Add(new SqliteParameter("@code", code));
        dbCommand.ExecuteNonQuery();
    }


    /// <summary>
    /// добавить информацию о чанке в базу данных
    /// </summary>
    /// <param name="data">информаци€ о чанке</param>
    public static void AddChunk(ChunkData data)
    {
        BlockType[,] blocks = data._blockTypes;

        for (int i = 0; i < blocks.GetLength(0); i++)
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                AddBlock(data.xNumber, data.yNumber, i, j, (int)blocks[i, j]);
            }
        }
    }

    public static void AddWholeChunk(ChunkData data)
    {
        IDbTransaction transaction = dbConnection.BeginTransaction();

        BlockType[,] blocks = data._blockTypes;
        int xPos = data.xNumber;
        int yPos = data.yNumber;

        for (int i = 0; i < blocks.GetLength(0); i++)
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                IDbCommand dbCommand = dbConnection.CreateCommand();
                dbCommand.CommandText = "INSERT INTO ChunkTable (xChunkPosition, yChunkPosition, xIndex, yIndex, code) VALUES (@xChunkPosition, @yChunkPosition, @xIndex, @yIndex, @code)";
                dbCommand.Parameters.Add(new SqliteParameter("@xChunkPosition", xPos));
                dbCommand.Parameters.Add(new SqliteParameter("@yChunkPosition", yPos));
                dbCommand.Parameters.Add(new SqliteParameter("@xIndex", i));
                dbCommand.Parameters.Add(new SqliteParameter("@yIndex", j));
                dbCommand.Parameters.Add(new SqliteParameter("@code", (int)blocks[i, j]));
                dbCommand.ExecuteNonQuery();
            }
        }
        transaction.Commit();
    }

    private static void CreateIndex()
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "CREATE INDEX IF NOT EXISTS idx_chunk_coords ON ChunkTable(xChunkPosition, yChunkPosition)";
        dbCommand.ExecuteNonQuery();
    }

    public static void AddColumn(List<ChunkData> chunkDatas)
    {
        IDbTransaction transaction = dbConnection.BeginTransaction();

        foreach(ChunkData data in chunkDatas)
        {
            BlockType[,] blocks = data._blockTypes;
            int xPos = data.xNumber;
            int yPos = data.yNumber;

            for (int i = 0; i < blocks.GetLength(0); i++)
            {
                for (int j = 0; j < blocks.GetLength(1); j++)
                {
                    IDbCommand dbCommand = dbConnection.CreateCommand();
                    dbCommand.CommandText = "INSERT INTO ChunkTable (xChunkPosition, yChunkPosition, xIndex, yIndex, code) VALUES (@xChunkPosition, @yChunkPosition, @xIndex, @yIndex, @code)";
                    dbCommand.Parameters.Add(new SqliteParameter("@xChunkPosition", xPos));
                    dbCommand.Parameters.Add(new SqliteParameter("@yChunkPosition", yPos));
                    dbCommand.Parameters.Add(new SqliteParameter("@xIndex", i));
                    dbCommand.Parameters.Add(new SqliteParameter("@yIndex", j));
                    dbCommand.Parameters.Add(new SqliteParameter("@code", (int)blocks[i, j]));
                    dbCommand.ExecuteNonQuery();
                }
            }
        }
        transaction.Commit();
    }

    public static void UpdateChunk(ChunkData oldData, ChunkData newData)
    {
        if (dbConnection.State == ConnectionState.Open)
        {
            IDbTransaction transaction = dbConnection.BeginTransaction();
            int xPos = oldData.xNumber;
            int yPos = oldData.yNumber;
            BlockType[,] oldBlocks = oldData._blockTypes;
            BlockType[,] newBlocks = newData._blockTypes;

            for (int i = 0; i < oldBlocks.GetLength(0); i++)
            {
                for (int j = 0; j < oldBlocks.GetLength(1); j++)
                {
                    if (oldBlocks[i, j] != newBlocks[i, j])
                    {
                        UpdateOneBlock(xPos, yPos, i, j, newBlocks[i, j]);
                    }
                }
            }

            transaction.Commit();
        }
    }

    private static void UpdateOneBlock(int chunkPosX, int chunkPosY, int xPos, int yPos, BlockType blockType)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = "UPDATE ChunkTable SET code = @code WHERE xChunkPosition = @xChunkPosition AND yChunkPosition = @yChunkPosition AND xIndex = @xIndex AND yIndex = @yIndex";
        dbCommand.Parameters.Add(new SqliteParameter("@xChunkPosition", chunkPosX));
        dbCommand.Parameters.Add(new SqliteParameter("@yChunkPosition", chunkPosY));
        dbCommand.Parameters.Add(new SqliteParameter("@xIndex", xPos));
        dbCommand.Parameters.Add(new SqliteParameter("@yIndex", yPos));
        dbCommand.Parameters.Add(new SqliteParameter("@code", (int)blockType));
        dbCommand.ExecuteNonQuery();
    }

    /// <summary>
    /// загрузить информацию о чанке из базы данных
    /// </summary>
    /// <param name="xPos">x-индекс искомого чанка</param>
    /// <param name="yPos">y-индекс искомого чанка</param>
    /// <returns></returns>
    public static ChunkData LoadChunkFromDB(int xPos, int yPos)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = @"SELECT xIndex, yIndex, code FROM ChunkTable
            WHERE xChunkPosition = @xPosition AND yChunkPosition = @yPosition";
        dbCommand.Parameters.Add(new SqliteParameter("@xPosition", xPos));
        dbCommand.Parameters.Add(new SqliteParameter("@yPosition", yPos));
        IDataReader reader = dbCommand.ExecuteReader();
        ChunkData chunkData = new ChunkData();

        chunkData.xNumber = xPos;
        chunkData.yNumber = yPos;
        while (reader.Read())
        {
            int xIndex = reader.GetInt32(0);
            int yIndex = reader.GetInt32(1);
            int code = reader.GetInt32(2);
            chunkData._blockTypes[xIndex, yIndex] = (BlockType)code;
        }
        return chunkData;
    }

    public static void DeleteChunkFromDB(int xPos, int yPos)
    {
        IDbCommand dbCommand = dbConnection.CreateCommand();
        dbCommand.CommandText = @"DELETE FROM ChunkTable
                                  WHERE xChunkPosition = @xPosition AND yChunkPosition = @yPosition";
        dbCommand.Parameters.Add(new SqliteParameter("@xPosition", xPos));
        dbCommand.Parameters.Add(new SqliteParameter("@yPosition", yPos));
        dbCommand.ExecuteNonQuery();
    } 


    public static void CloseDB()
    {
        dbConnection.Close();
    }
}
