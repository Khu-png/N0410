using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [System.Serializable]
    public class PoolEntry
    {
        public string key;
        public GameObject prefab;
        public int amount = 10;
    }

    public static PoolManager Instance { get; private set; }

    [Header("Pools")]
    [SerializeField] private PoolEntry[] elements;

    [Header("Spawner")]
    [SerializeField] private Player player;
    [SerializeField] private string[] flyingEnemyKeys;
    [SerializeField] private string[] obstacleKeys;
    [SerializeField] private float flyingEnemySpawnInterval = 3f;
    [SerializeField] private float obstacleSpawnInterval = 1.2f;
    [SerializeField] private float topOffset = 1f;
    [SerializeField] private float sidePadding = 0.5f;

    private readonly Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Camera mainCamera;
    private bool canSpawn;
    private float nextFlyingSpawnTime;
    private float nextObstacleSpawnTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        mainCamera = Camera.main;

        if (player == null)
        {
            player = FindFirstObjectByType<Player>();
        }

        InitializePools();
    }

    private void Update()
    {
        if (!canSpawn)
        {
            return;
        }

        if (flyingEnemyKeys != null && flyingEnemyKeys.Length > 0 && Time.time >= nextFlyingSpawnTime)
        {
            SpawnFlyingEnemy();
            nextFlyingSpawnTime = Time.time + flyingEnemySpawnInterval;
        }

        if (obstacleKeys != null && obstacleKeys.Length > 0 && Time.time >= nextObstacleSpawnTime)
        {
            SpawnObstacle();
            nextObstacleSpawnTime = Time.time + obstacleSpawnInterval;
        }
    }

    public void SetSpawnState(bool enabled)
    {
        canSpawn = enabled;
        nextFlyingSpawnTime = Time.time + flyingEnemySpawnInterval;
        nextObstacleSpawnTime = Time.time + obstacleSpawnInterval;
    }

    public GameObject SpawnFromPool(string key, Vector3 position)
    {
        if (!pools.TryGetValue(key, out Queue<GameObject> pool))
        {
            Debug.LogWarning($"Pool key not found: {key}");
            return null;
        }

        GameObject spawnedObject = pool.Count > 0 ? pool.Dequeue() : CreateNewObject(key);
        if (spawnedObject == null)
        {
            return null;
        }

        spawnedObject.transform.SetPositionAndRotation(position, Quaternion.identity);
        spawnedObject.SetActive(true);
        return spawnedObject;
    }

    public void ReturnToPool(GameObject pooledObject)
    {
        if (pooledObject == null)
        {
            return;
        }

        PooledObject pooledData = pooledObject.GetComponent<PooledObject>();
        if (pooledData == null || string.IsNullOrEmpty(pooledData.PoolKey) || !pools.ContainsKey(pooledData.PoolKey))
        {
            pooledObject.SetActive(false);
            return;
        }

        pooledObject.SetActive(false);
        pooledObject.transform.SetParent(transform);
        pools[pooledData.PoolKey].Enqueue(pooledObject);
    }

    public void ReturnAllToPool()
    {
        PooledObject[] pooledObjects = FindObjectsByType<PooledObject>(FindObjectsSortMode.None);
        foreach (PooledObject pooledObject in pooledObjects)
        {
            if (pooledObject == null || !pooledObject.gameObject.activeInHierarchy)
            {
                continue;
            }

            ReturnToPool(pooledObject.gameObject);
        }
    }

    private void InitializePools()
    {
        pools.Clear();

        if (elements == null)
        {
            return;
        }

        foreach (PoolEntry entry in elements)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.key) || entry.prefab == null)
            {
                continue;
            }

            Queue<GameObject> queue = new Queue<GameObject>();
            pools[entry.key] = queue;

            for (int i = 0; i < entry.amount; i++)
            {
                GameObject pooledObject = CreateNewObject(entry.key);
                if (pooledObject != null)
                {
                    queue.Enqueue(pooledObject);
                }
            }
        }
    }

    private GameObject CreateNewObject(string key)
    {
        PoolEntry entry = GetEntry(key);
        if (entry == null || entry.prefab == null)
        {
            return null;
        }

        GameObject pooledObject = Instantiate(entry.prefab, transform);
        pooledObject.SetActive(false);

        PooledObject pooledData = pooledObject.GetComponent<PooledObject>();
        if (pooledData == null)
        {
            pooledData = pooledObject.AddComponent<PooledObject>();
        }

        pooledData.SetPoolKey(key);
        return pooledObject;
    }

    private PoolEntry GetEntry(string key)
    {
        if (elements == null)
        {
            return null;
        }

        foreach (PoolEntry entry in elements)
        {
            if (entry != null && entry.key == key)
            {
                return entry;
            }
        }

        return null;
    }

    private void SpawnFlyingEnemy()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        string selectedKey = flyingEnemyKeys[Random.Range(0, flyingEnemyKeys.Length)];
        Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 1f, 0f));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        float randomX = Random.Range(topLeft.x + sidePadding, topRight.x - sidePadding);
        float spawnY = topLeft.y + topOffset;

        SpawnFromPool(selectedKey, new Vector3(randomX, spawnY, 0f));
    }

    private void SpawnObstacle()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        string selectedKey = obstacleKeys[Random.Range(0, obstacleKeys.Length)];
        Vector3 topLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 1f, 0f));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        float randomX = Random.Range(topLeft.x + sidePadding, topRight.x - sidePadding);
        float spawnY = topLeft.y + topOffset;

        SpawnFromPool(selectedKey, new Vector3(randomX, spawnY, 0f));
    }
}
