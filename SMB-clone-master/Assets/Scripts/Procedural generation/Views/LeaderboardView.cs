using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardView : MonoBehaviour
{
    [SerializeField] private GameObject _leaderboardEntry;
    [SerializeField] private GameObject _root;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        FirebaseManager.Instance.OnLeaderBoardDataReceived += Configure;
        closeButton.onClick.AddListener(() => 
        {
            EnableDisable(false);
        });
    }

    public void Configure(List<LeaderBoardEntry> entries)
    {
        foreach(var entry in entries)
        {
            var go = Instantiate(_leaderboardEntry, _root.transform);
            go.GetComponent<LeaderboardEntry>().Configure(entry.userName, entry.score);
        }
    }

    public void EnableDisable(bool enable)
    {
        canvasGroup.interactable = enable;
        canvasGroup.alpha = enable == true ? 1 : 0;
        canvasGroup.blocksRaycasts = enable;
    }
}
