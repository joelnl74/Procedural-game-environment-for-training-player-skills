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
    public bool dataReceived;

    [Header("Firebase")]
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseDatabase database;

    private Dictionary<int, ChunkInformation> _data1 = new Dictionary<int, ChunkInformation>();
    private Dictionary<int, ChunkInformation> _data2 = new Dictionary<int, ChunkInformation>();
    private List<LeaderBoardEntryViewModel> _leaderboard;

    public void Setup()
    {
        if (setup)
        {
            return;
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(check =>
        {
            var dependencyStatus = check.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
                SignIn();
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
        StartCoroutine(GetData());
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

    private IEnumerator GetData()
    {
        if (setup == false || dataReceived == true)
        {
            yield break;
        }

        var task = database.RootReference.Child("users").Child(user.UserId).Child("playerInfo").GetValueAsync();

        yield return new WaitUntil(predicate: () => task.IsCompleted);

        var shot = task.Result;

        if (shot != null)
        {
            var jsonValue1 = shot.Child($"version{1}");
            var jsonValue2 = shot.Child($"version{2}");

            if (jsonValue1.Exists)
                _data1 = JsonConvert.DeserializeObject<Dictionary<int, ChunkInformation>>(jsonValue1.GetRawJsonValue());
            if (jsonValue2.Exists)
                _data2 = JsonConvert.DeserializeObject<Dictionary<int, ChunkInformation>>(jsonValue2.GetRawJsonValue());
        }

        dataReceived = true;
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

           setup = true;
       });
    }
}

