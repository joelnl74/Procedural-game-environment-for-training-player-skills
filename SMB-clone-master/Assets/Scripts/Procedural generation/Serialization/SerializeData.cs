using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

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

    public void SaveData(Dictionary<int, ChunkInformation> chunkInformation, GlobalPlayerResults globalPlayerResults, int version)
    {
        var jsonDictonary = JsonConvert.SerializeObject(chunkInformation);
        var jsonObject = JsonConvert.SerializeObject(globalPlayerResults);

        FirebaseManager.Instance.UpdateDatabase(jsonDictonary, version);
        FirebaseManager.Instance.UpdateGlobalResults(jsonObject);
    }

    public Dictionary<int, ChunkInformation> LoadData(int version)
    {
        var dictonary = FirebaseManager.Instance.GetData(version);
        var newDictonary = new Dictionary<int, ChunkInformation>();

        foreach(var KeyValue in dictonary)
        {
            if (KeyValue.Key == int.MaxValue)
            {
                continue;
            }

            newDictonary.Add(-KeyValue.Key, KeyValue.Value);
        }

        return newDictonary;
    }

    public bool GetSkippedTutorial()
    {
        return PlayerPrefs.GetInt("has_skipped_tutorial") == 1 ? true : false;
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
