using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private Text _entryText;

    public void Configure(string name, int score)
    {
        _entryText.text = $"{name} : {score}";
    }
}
