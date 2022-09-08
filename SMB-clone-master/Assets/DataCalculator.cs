using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Linq;
using UnityEngine.UI;

public class UserData
{
    public GlobalPlayerResults globalPlayerResults;
    public Dictionary<int, ChunkInformation> _versionOneData;
    public Dictionary<int, ChunkInformation> _versionTwoData;
}

public class DataCalculator : MonoBehaviour
{
    [SerializeField] private InputField _v1InputField;
    [SerializeField] private InputField _v2InputField;
    [SerializeField] private TextAsset[] jsonFiles;

    private List<UserData> _userData = new List<UserData>();

    // Start is called before the first frame update
    void Start()
    {
       for (int i = 0; i < jsonFiles.Length; i++)
        {
            var jsonObject = (JObject)JToken.Parse(jsonFiles[i].text);
            var users = jsonObject["users"];
            var user = users[$"{i}"];
            var playerInfoData = user["playerInfo"];

            var userData = new UserData();
            userData.globalPlayerResults = playerInfoData["GlobalResults"].ToObject<GlobalPlayerResults>();
            userData._versionOneData = playerInfoData["version1"].ToObject<Dictionary<int, ChunkInformation>>();
            userData._versionTwoData = playerInfoData["version2"].ToObject<Dictionary<int, ChunkInformation>>();

            _userData.Add(userData);

        }

        var speedRunners = _userData.Where(x => x.globalPlayerResults.DidFailTraingVersionOne).ToList();
        var speedRunners2 = _userData.Where(x => x.globalPlayerResults.DidFailTrainingVersionTwo).ToList();;

        _v1InputField.text += $"{speedRunners.Concat(speedRunners2).Distinct().Count()}";
    }
}
