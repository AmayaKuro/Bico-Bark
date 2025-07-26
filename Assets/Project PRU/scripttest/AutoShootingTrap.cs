using System.Collections;
using UnityEngine;

public class AutoShootingTrap : MonoBehaviour
{
    [Header(" setting shoot")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireRate = 1.5f;
    [SerializeField] private float projectileSpeed = 10f;
    [SerializeField] private float damage = 10f;

    [Header("setting target")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private ShootingMode mode = ShootingMode.NearestPlayer;

    public enum ShootingMode
    {
        NearestPlayer,    
        RandomDirection, 
        AllDirections,   
        FirstDetected     
    }

    private float nextFireTime;
    private Transform currentTarget;

    // set projectile 
    public void SetProjectilePrefab(GameObject prefab)
    {
        if (prefab != null)
            projectilePrefab = prefab;
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }


    void Fire()
    {
        if (projectilePrefab == null) return;

        Vector3 shootDirection = GetShootDirection();
        if (shootDirection == Vector3.zero) return;

        // create ammo
        Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(shootDirection));

        // Setup ammo
        TrapProjectile projScript = projectile.GetComponent<TrapProjectile>();
        if (projScript == null)
        {
            projScript = projectile.AddComponent<TrapProjectile>();
        }
        projScript.Setup(damage, projectileSpeed, shootDirection);
    }
    public void SetFireRate(float newFireRate)
    {
        fireRate = newFireRate;
        nextFireTime = Time.time + fireRate;
    }
    Vector3 GetShootDirection()
    {
        switch (mode)
        {
            case ShootingMode.NearestPlayer:
                GameObject nearest = FindNearestPlayer();
                if (nearest != null)
                {
                    Vector3 dir = (nearest.transform.position - transform.position).normalized;
                    dir.y = 0; 
                    return dir;
                }
                return GetRandomDirection();

            case ShootingMode.RandomDirection:
                return GetRandomDirection();

            case ShootingMode.AllDirections:
                
                StartCoroutine(FireAllDirections());
                return Vector3.zero; 

            case ShootingMode.FirstDetected:
                if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.position) > detectionRange)
                {
                    currentTarget = FindNearestPlayer()?.transform;
                }
                if (currentTarget != null)
                {
                    Vector3 dir = (currentTarget.position - transform.position).normalized;
                    dir.y = 0;
                    return dir;
                }
                return Vector3.zero;

            default:
                return GetRandomDirection();
        }
    }

    Vector3 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    GameObject FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject nearest = null;
        float minDistance = detectionRange;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = player;
            }
        }

        return nearest;
    }
    
    IEnumerator FireAllDirections()
    {
        int directions = 8;
        float angleStep = 360f / directions;

        for (int i = 0; i < directions; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));

            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(direction));

            TrapProjectile projScript = projectile.GetComponent<TrapProjectile>();
            if (projScript == null)
            {
                projScript = projectile.AddComponent<TrapProjectile>();
            }
            projScript.Setup(damage, projectileSpeed, direction);

            yield return new WaitForSeconds(0.05f);
        }
    }

    void OnDrawGizmosSelected()
    {
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}