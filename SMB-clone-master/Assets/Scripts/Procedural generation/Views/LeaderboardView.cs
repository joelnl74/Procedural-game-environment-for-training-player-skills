using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardView : MonoBehaviour
{
    [SerializeField] private GameObject _leaderboardEntry;
    [SerializeField] private GameObject _root;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button closeButton;

    private bool fetched;
    private FirebaseManager firebaseManager;

    private void Start()
    {
        // firebaseManager = FirebaseManager.Instance;
        // firebaseManager.OnLeaderBoardDataReceived += Configure;
        closeButton.onClick.AddListener(() => 
        {
            EnableDisable(false);
        });
    }

    private void OnDestroy()
    {
        if (firebaseManager == null)
        {
            return;
        }

        firebaseManager.OnLeaderBoardDataReceived -= Configure;
    }

    public void LoadData()
    {
        if (fetched)
        {
            return;
        }

        firebaseManager.LoadLeaderboardAsync();

        fetched = true;
    }

    public void Configure(List<LeaderBoardEntryViewModel> entries)
    {
        for (var index = 0; index < entries.Count; index++)
        {
            var go = Instantiate(_leaderboardEntry, _root.transform);
            go.GetComponent<LeaderboardEntry>().Configure(entries[index].userName, entries[index].score, index + 1, entries[index].isOwn);
        }
    }

    public void EnableDisable(bool enable)
    {
        canvasGroup.interactable = enable;
        canvasGroup.alpha = enable == true ? 1 : 0;
        canvasGroup.blocksRaycasts = enable;

        if (enable)
        {
            LoadData();
        }
    }
}
