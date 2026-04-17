using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float bottomOffset = 1f;
    [SerializeField] private float fireInterval = 3f;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private string enemyBulletKey = "EnemyBullet";

    private Camera mainCamera;
    private float nextFireTime;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        nextFireTime = Time.time + fireInterval;
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        HandleMovement();
        HandleShoot();
    }

    private void HandleMovement()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        float bottomBound = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y - bottomOffset;
        if (transform.position.y < bottomBound)
        {
            ReturnToPool();
        }
    }

    private void HandleShoot()
    {
        if (PoolManager.Instance != null && !string.IsNullOrWhiteSpace(enemyBulletKey) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireInterval;
            Transform spawnPoint = bulletSpawnPoint != null ? bulletSpawnPoint : transform;
            GameObject bulletObject = PoolManager.Instance.SpawnFromPool(enemyBulletKey, spawnPoint.position);
            if (bulletObject == null)
            {
                return;
            }

            Bullet bullet = bulletObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(Vector3.down, Bullet.BulletOwner.Enemy);
            }
        }
    }

    public void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnToPool(gameObject);
            return;
        }

        gameObject.SetActive(false);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Enemy collision with: " + collision.collider.name + " | Tag: " + collision.collider.tag);

        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Enemy hit Player -> GameOver");
            GameManager.Instance?.GameOver();
        }
    }
}
