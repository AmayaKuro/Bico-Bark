using UnityEngine;
using System.Collections.Generic;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseMapLoader : MonoBehaviour
{
    private DatabaseReference dbReference;
    public RoomManager roomManager; // Assign via Inspector

    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                // Ensure Firebase uses your google-services.json config
                dbReference = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase connected.");

                if (roomManager.subGameScenes == null || roomManager.subGameScenes.Count == 0)
                {
                    Debug.Log("gameSceneList is null or empty, adding maps to database.");
                    AddMapsToDatabase();
                }
                else
                {
                    Debug.Log("gameSceneList has data, fetching maps to update.");
                    FetchMaps();
                }
            }
            else
            {
                Debug.LogError($"Cannot resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    void AddMapsToDatabase()
    {
        List<string> mapScenes = new List<string> { "map1", "map2", "map3", "map4" };
        Dictionary<string, object> maps = new Dictionary<string, object>();

        for (int i = 0; i < mapScenes.Count; i++)
        {
            Dictionary<string, object> mapData = new Dictionary<string, object>
            {
                { "scene", mapScenes[i] }
            };
            maps[$"map{i + 1}"] = mapData;
        }

        dbReference.Child("maps").UpdateChildrenAsync(maps).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                Debug.Log("Maps successfully added to Firebase.");
                FetchMaps();
            }
            else
            {
                Debug.LogError("Failed to add maps to Firebase: " + task.Exception);
            }
        });
    }

    void FetchMaps()
    {
        dbReference.Child("maps").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    if (roomManager.subGameScenes == null)
                    {
                        roomManager.subGameScenes = new List<string>();
                    }

                    foreach (var map in snapshot.Children)
                    {
                        string scenePath = map.Child("scene").Value.ToString();
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
                Debug.LogError("Failed to fetch maps from Firebase: " + task.Exception);
            }
        });
    }
}
