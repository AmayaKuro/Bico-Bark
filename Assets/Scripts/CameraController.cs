using System.Collections.Generic;
using UnityEngine;

public class GroupCameraController : MonoBehaviour
{
    public float smoothTime = 0.5f;
    //public float minZoom = 5f;
    //public float maxZoom = 8f;
    //public float zoomLimiter = 10f;
    public Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 velocity;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        List<Transform> players = GetPlayerTransforms();
        if (players.Count == 0) return;

        Move(players);
        //Zoom(players);
    }

    List<Transform> GetPlayerTransforms()
    {
        // Adjust the tag or method as needed for your player objects
        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
        List<Transform> playerTransforms = new List<Transform>();
        foreach (var obj in playerObjects)
            playerTransforms.Add(obj.transform);
        return playerTransforms;
    }

    void Move(List<Transform> players)
    {
        Vector3 centerPoint = GetCenterPoint(players);
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    //void Zoom(List<Transform> players)
    //{
    //    float newZoom = Mathf.Lerp(minZoom, maxZoom, GetGreatestDistance(players) / zoomLimiter);
    //    cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime);
    //}

    float GetGreatestDistance(List<Transform> players)
    {
        var bounds = new Bounds(players[0].position, Vector3.zero);
        foreach (var player in players)
            bounds.Encapsulate(player.position);
        return Mathf.Max(bounds.size.x, bounds.size.y);
    }

    Vector3 GetCenterPoint(List<Transform> players)
    {
        if (players.Count == 1)
            return players[0].position;

        var bounds = new Bounds(players[0].position, Vector3.zero);
        foreach (var player in players)
            bounds.Encapsulate(player.position);
        return bounds.center;
    }
}