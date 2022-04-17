using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private Text _entryText;

    public void Configure(string name, int score, int position)
    {
        _entryText.text = $"{position} : {name} : {score}";
    }
}
