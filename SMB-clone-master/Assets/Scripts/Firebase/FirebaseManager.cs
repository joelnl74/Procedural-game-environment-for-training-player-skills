using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System;

public class FirebaseManager : MonoSingleton<FirebaseManager>
{
    [Header("Firebase")]
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseDatabase database;

    private void Awake()
    {
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
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
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

    public void UpdateDatabase(string data)
    {
        var task = database.RootReference.Child("users").Child(user.UserId).Child("playerInfo").SetRawJsonValueAsync(data);
    }
}

