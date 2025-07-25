using UnityEngine;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseMapLoader : MonoBehaviour
{
    DatabaseReference dbReference;
    public RoomManager roomManager; // Assign via Inspector

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase connected.");
                //FetchMaps();
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    void FetchMaps()
    {
        dbReference.Child("maps").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    foreach (var map in snapshot.Children)
                    {
                        string scenePath = map.Child("scene").Value.ToString();
                        // Push to the new gameSceneList
                        roomManager.subGameScenes.Add(scenePath);

                        Debug.Log($"Map added to gameSceneList: {scenePath}");
                    }
                }
                else
                {
                    Debug.LogWarning("No maps found in Firebase.");
                }
            }
            else
            {
                Debug.LogError("Failed to fetch maps from Firebase.");
            }
        });
    }
}
