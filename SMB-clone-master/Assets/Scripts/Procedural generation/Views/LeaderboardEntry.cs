using UnityEngine;
using UnityEngine.UI;

public class LeaderboardEntry : MonoBehaviour
{
    [SerializeField] private Text _entryText;

    public void Configure(string name, int score, int position, bool isOwn)
    {
        _entryText.text = $"{position} : {name} : {score}";
        _entryText.color = isOwn ? _entryText.color = Color.green : _entryText.color;
    }
}
