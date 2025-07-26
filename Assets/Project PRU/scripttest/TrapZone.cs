using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[RequireComponent(typeof(BoxCollider2D))]
public class TrapZone : MonoBehaviour
{
    [Header("Setting trap prefab")]
    [SerializeField] private GameObject trapPrefab;
    [SerializeField] private GameObject projectilePrefab; 

    [Header("setting trap spawn")]
    [SerializeField] private int maxTraps = 10;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private float trapLifetime = 10f;
    [SerializeField] private Color zoneColor = new Color(1f, 0f, 0f, 0.3f);

    [Header("Scale base on player")]
    [SerializeField] private bool adjustByPlayerCount = true;
    [SerializeField] private int trapsPerPlayer = 2;

    private BoxCollider2D zoneCollider;
    private List<GameObject> activeTraps = new List<GameObject>();

    [Header("Safe zone")]
    [SerializeField] private Transform safeZone;
    [SerializeField] private float safeZoneRadius = 5f;
    [SerializeField] private int minTrapsWhenSafe = 1;
    [SerializeField] private float normalFireRate = 1.5f; 
    [SerializeField] private float slowFireRate = 4f;    

    private int playersInSafeZone = 0;
    private bool isSafeZoneActive = false;

    void Start()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;

        if (trapPrefab == null)
        {
            Debug.LogError("missing trap prefab");
            return;
        }

        InvokeRepeating(nameof(SpawnTrap), 0f, spawnInterval);
    }
    void Update()
    {
        CheckSafeZone();
    }

    void CheckSafeZone()
    {
        if (safeZone == null) return;

        int count = 0;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(player.transform.position, safeZone.position);
            if (distance <= safeZoneRadius)
            {
                count++;
            }
        }

        if (count > 0 && !isSafeZoneActive)
        {
            ActivateSafeZone();
        }
        else if (count == 0 && isSafeZoneActive)
        {
            DeactivateSafeZone();
        }

        playersInSafeZone = count;
    }

    void ActivateSafeZone()
    {
        isSafeZoneActive = true;

        while (activeTraps.Count > minTrapsWhenSafe)
        {
            GameObject trap = activeTraps[activeTraps.Count - 1];
            activeTraps.RemoveAt(activeTraps.Count - 1);
            if (trap != null) Destroy(trap);
        }

        foreach (GameObject trap in activeTraps)
        {
            if (trap != null)
            {
                AutoShootingTrap shooter = trap.GetComponent<AutoShootingTrap>();
                if (shooter != null)
                {
                    shooter.SetFireRate(slowFireRate);
                }
            }
        }

        CancelInvoke(nameof(SpawnTrap));
        InvokeRepeating(nameof(SpawnTrap), 0f, slowFireRate);
    }

    void DeactivateSafeZone()
    {
        isSafeZoneActive = false;

        foreach (GameObject trap in activeTraps)
        {
            if (trap != null)
            {
                AutoShootingTrap shooter = trap.GetComponent<AutoShootingTrap>();
                if (shooter != null)
                {
                    shooter.SetFireRate(normalFireRate);
                }
            }
        }

        CancelInvoke(nameof(SpawnTrap));
        InvokeRepeating(nameof(SpawnTrap), 0f, spawnInterval);
    }

    void SpawnTrap()
    {
        activeTraps.RemoveAll(trap => trap == null);

        int currentMaxTraps = maxTraps;
        if (adjustByPlayerCount)
        {
            int playerCount = GameObject.FindGameObjectsWithTag("Player").Length;
            currentMaxTraps = maxTraps + (trapsPerPlayer * Mathf.Max(0, playerCount - 1));
        }
        if (isSafeZoneActive)
        {
            currentMaxTraps = minTrapsWhenSafe;
        }

        if (activeTraps.Count < currentMaxTraps)
        {
            Vector3 randomPos = GetRandomPositionInZone();
            GameObject newTrap = Instantiate(trapPrefab, randomPos, Quaternion.identity);

            AutoShootingTrap shooter = newTrap.GetComponent<AutoShootingTrap>();
            if (shooter == null)
            {
                shooter = newTrap.AddComponent<AutoShootingTrap>();
            }

       
            shooter.SetProjectilePrefab(projectilePrefab);

            if (isSafeZoneActive)
            {
                shooter.SetFireRate(slowFireRate);
            }
            else
            {
                shooter.SetFireRate(normalFireRate);
            }
            Destroy(newTrap, trapLifetime);

            activeTraps.Add(newTrap);
        }
    }

    Vector3 GetRandomPositionInZone()
    {
        Bounds bounds = zoneCollider.bounds;

        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = bounds.center.y; 
        float z = Random.Range(bounds.min.z, bounds.max.z);

        return new Vector3(x, y, z);
    }

    void OnDrawGizmos()
    {
        if (zoneCollider == null)
            zoneCollider = GetComponent<BoxCollider2D>();

        if (zoneCollider != null)
        {
            Gizmos.color = zoneColor;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(zoneCollider.offset, zoneCollider.size);
            Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 1f);
            Gizmos.DrawWireCube(zoneCollider.offset, zoneCollider.size);
            Gizmos.matrix = oldMatrix;
        }
        if (safeZone != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(safeZone.position, safeZoneRadius);
        }
    }
}
