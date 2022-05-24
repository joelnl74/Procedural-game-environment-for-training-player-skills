using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

public struct LeaderBoardEntryViewModel
{
    public bool isOwn;
    public int score;
    public string userName;
}

public class FirebaseManager : MonoSingleton<FirebaseManager>
{
    public Action<List<LeaderBoardEntryViewModel>> OnLeaderBoardDataReceived;
    public bool setup;

    [Header("Firebase")]
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseDatabase database;

    private Dictionary<int, ChunkInformation> _data1 = new Dictionary<int, ChunkInformation>();
    private Dictionary<int, ChunkInformation> _data2 = new Dictionary<int, ChunkInformation>();
    private List<LeaderBoardEntryViewModel> _leaderboard;

    public void Setup()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(check =>
        {
            var dependencyStatus = check.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
                SignIn();

                setup = true;
            }
        });
    }

    public void UpdateDatabase(string data, int version)
    {
        if (setup == false)
        {
            return;
        }

        var task = database.RootReference.Child("users").Child(user.UserId).Child("playerInfo").Child($"version{version}").SetRawJsonValueAsync(data);
    }

    public void SetSkippedTutorial(bool skipped)
    {
        if (setup == false)
        {
            return;
        }

        var task = database.RootReference.Child("users").Child(user.UserId).Child("HasSkippedTutrial").SetValueAsync(skipped);
    }

    public void GetLeaderBoardAysncFromDataBase()
    {
        StartCoroutine(GetLeaderBoards());
    }

    public void LoadLeaderboardAsync()
    {
        if(_leaderboard != null)
        {
            OnLeaderBoardDataReceived?.Invoke(_leaderboard);

            return;
        }
    }

    public Dictionary<int, ChunkInformation> GetData(int version)
    {
        if (version == 1)
        {
            return _data1;
        }

        return _data2;
    }

    private IEnumerator GetLeaderBoards()
    {
        if (setup == false)
        {
            yield break;
        }

        yield return new WaitForSeconds(2);

        var task = database.RootReference.Child("users").GetValueAsync();

        yield return new WaitUntil(predicate: () => task.IsCompleted);

        var shot = task.Result;
        var leaderboard = new List<LeaderBoardEntryViewModel>();

        foreach(var snapshot in shot.Children)
        {
            var key = snapshot.Key;

            var snapShotData = snapshot.Child("playerInfo").Child($"version{1}");

            if (key == user.UserId)
            {
                var jsonValue1 = snapShotData;
                var jsonValue2 = snapshot.Child("playerInfo").Child($"version{2}");

                if (jsonValue1.Exists)
                    _data1 = JsonConvert.DeserializeObject<Dictionary<int, ChunkInformation>>(jsonValue1.GetRawJsonValue());
                if (jsonValue2.Exists)
                    _data2 = JsonConvert.DeserializeObject<Dictionary<int, ChunkInformation>>(jsonValue2.GetRawJsonValue());
            }

            var highestScore = 0;

            foreach(var items in snapShotData.Children)
            {
                var score = Convert.ToInt32(items.Child("difficultyScore").Value);

                if(score > highestScore)
                {
                    highestScore = score;
                }
            }

            leaderboard.Add(new LeaderBoardEntryViewModel { userName = key, score = highestScore, isOwn = key == user.UserId});
        }

        leaderboard = leaderboard.OrderByDescending(x => x.score).ToList();
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            
            user = auth.CurrentUser;
            Debug.Log("Signed in " + user.UserId);
        }
    }

    private void SignIn()
    {
       auth.SignInAnonymouslyAsync().ContinueWith(task => 
       {
       if (task.IsCanceled)
       {
               Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
               return;
       }
       if (task.IsFaulted)
       {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                return;
       }
            FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

           database = FirebaseDatabase.DefaultInstance;
       });
    }
}

