using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SerializeData
{
    private const string saveDataKey = "PCG-SAVE-KEY";
    private bool hasSkippedTutorial;

    public SerializeData()
    {
        if (PlayerPrefs.HasKey("has_skipped_tutorial"))
        {
            hasSkippedTutorial = GetSkippedTutorial();
        }
    }

    public bool CheckSafe(int version)
    {
        var destination = Application.persistentDataPath + $"/save{version}.dat";

        return File.Exists(destination);
    }

    public void DeleteSave(int version)
    {
        var destination = Application.persistentDataPath + $"/save{version}.dat";

        if (File.Exists(destination))
        {
            File.Delete(destination);
        }
    }

    public void SaveData(Dictionary<int, ChunkInformation> chunkInformation, int version)
    {
        var destination = Application.persistentDataPath + $"/save{version}.dat";

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
        FirebaseManager.Instance.UpdateDatabase(jsonDictonary, version);

        file.Close();
    }

    public Dictionary<int, ChunkInformation> LoadData(int version)
    {
        // Load JSON file
        string json = "";
        string destination = Application.persistentDataPath + $"/save{version}.dat";

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

    public bool GetSkippedTutorial()
    {
        return hasSkippedTutorial;
    }

    public bool ContainsSkippedTutorial()
    {
        return PlayerPrefs.HasKey("has_skipped_tutorial");
    }

    public void SetSkippedTutorial(bool skipped)
    {
        hasSkippedTutorial = skipped;
        PlayerPrefs.SetInt("has_skipped_tutorial", skipped == true ? 1 : 0);
    }
}
