using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SerializeData
{
    private const string saveDataKey = "PCG-SAVE-KEY";

    public bool CheckSafe()
    {
        var destination = Application.persistentDataPath + "/save.dat";

        return File.Exists(destination);
    }

    public void DeleteSate()
    {
        var destination = Application.persistentDataPath + "/save.dat";

        FileStream file;

        if (File.Exists(destination))
        {
            File.Delete(destination);
        }
    }

    public void SaveData(Dictionary<int, ChunkInformation> chunkInformation)
    {
        var destination = Application.persistentDataPath + "/save.dat";

        FileStream file;

        if (File.Exists(destination))
        {
            file = File.OpenWrite(destination);
        }
        else
        {
            file = File.Create(destination);
        }

        file.Flush();

        var jsonDictonary = JsonConvert.SerializeObject(chunkInformation);
        var bf = new BinaryFormatter();

        bf.Serialize(file, jsonDictonary);

        file.Close();
    }

    public Dictionary<int, ChunkInformation> LoadData()
    {
        // Load JSON file
        string json = "";
        string destination = Application.persistentDataPath + "/save.dat";

        FileStream file;

        if (File.Exists(destination) == false)
        {
            return null;
        }

        file = File.OpenRead(destination);

        var bf = new BinaryFormatter();
        json = (string)bf.Deserialize(file);

        var dictonary = JsonConvert.DeserializeObject<Dictionary<int, ChunkInformation>>(json);
        var newDictonary = new Dictionary<int, ChunkInformation>();

        foreach(var KeyValue in dictonary)
        {
            newDictonary.Add(-KeyValue.Key, KeyValue.Value);
        }

        return newDictonary;
    }
}
