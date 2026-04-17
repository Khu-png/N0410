using UnityEngine;

public class Bullet : MonoBehaviour
{
    public enum BulletOwner
    {
        Player,
        Enemy
    }

    [SerializeField] private float speed = 10f;
    [SerializeField] private float destroyOffset = 1f;

    private Camera mainCamera;
    private Vector3 moveDirection = Vector3.up;
    private BulletOwner owner = BulletOwner.Player;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (moveDirection == Vector3.zero)
        {
            moveDirection = Vector3.up;
        }
    }

    private void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        Vector3 min = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f));
        Vector3 max = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

        if (transform.position.y > max.y + destroyOffset ||
            transform.position.y < min.y - destroyOffset ||
            transform.position.x < min.x - destroyOffset ||
            transform.position.x > max.x + destroyOffset)
        {
            ReturnToPool();
        }
    }

    public void Initialize(Vector3 direction, BulletOwner bulletOwner)
    {
        moveDirection = direction.normalized;
        owner = bulletOwner;
        if (moveDirection != Vector3.zero)
        {
            transform.up = moveDirection;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {

        if (owner == BulletOwner.Player && other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                Debug.Log("Player bullet hit Enemy");
                enemy.ReturnToPool();
                GameManager.Instance?.AddScore(1);
                ReturnToPool();
                return;
            }

            Obstacle obstacle = other.GetComponent<Obstacle>();
            if (obstacle != null)
            {
                Debug.Log("Player bullet hit Obstacle");
                obstacle.ReturnToPool();
                GameManager.Instance?.AddScore(1);
                ReturnToPool();
                return;
            }
        }

        if (owner == BulletOwner.Enemy && other.CompareTag("Player"))
        {
            Debug.Log("Enemy bullet hit Player");
            ReturnToPool();
            GameManager.Instance?.GameOver();
        }
    }

    private void ReturnToPool()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnToPool(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}
