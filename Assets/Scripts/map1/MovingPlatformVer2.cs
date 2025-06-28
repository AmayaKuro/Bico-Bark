using System.Collections;
using UnityEngine;

public class MovingPlatformVer2 : MonoBehaviour
{
    [Header("Platform Settings")]
    public bool moveOnStart = false;
    public float moveSpeed = 2f;
    public Transform[] waypoints;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        if (moveOnStart && waypoints.Length > 0)
        {
            StartMoving();
        }
    }

    public void StartMoving()
    {
        isMoving = true;
        StartCoroutine(MoveToWaypoints());
    }

    public void StopMoving()
    {
        isMoving = false;
        StopAllCoroutines();
    }
    
    IEnumerator MoveToWaypoints()
    {
        while (isMoving)
        {
            Transform target = waypoints[currentWaypointIndex];

            while (Vector3.Distance(transform.position, target.position) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    target.position,
                    moveSpeed * Time.deltaTime
                );
                yield return null;
            }

            // Next waypoint
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            yield return new WaitForSeconds(0.5f); // Pause at waypoint
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Parent player to platform
            other.transform.SetParent(transform);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Unparent player
            other.transform.SetParent(null);
        }
    }
}