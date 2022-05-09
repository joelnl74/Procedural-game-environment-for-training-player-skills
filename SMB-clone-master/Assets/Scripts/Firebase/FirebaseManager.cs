using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

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

    public void LoadLeaderboardAsync()
    {
        StartCoroutine(GetLeaderBoards());
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
            var jsonValue = snapshot.Child("playerInfo").Child($"version{1}");
            var highestScore = 0;

            foreach(var items in jsonValue.Children)
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

        OnLeaderBoardDataReceived?.Invoke(leaderboard);
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

