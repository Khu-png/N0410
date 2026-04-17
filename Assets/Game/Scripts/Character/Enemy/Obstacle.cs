using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float bottomOffset = 1f;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
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

        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        float bottomBound = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).y - bottomOffset;
        if (transform.position.y < bottomBound)
        {
            ReturnToPool();
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
        Debug.Log("Obstacle collision with: " + collision.collider.name + " | Tag: " + collision.collider.tag);

        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log("Obstacle hit Player -> GameOver");
            GameManager.Instance?.GameOver();
        }
    }
}
