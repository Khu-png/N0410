using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{
    [SerializeField] private float playerMoveSpeed = 5f;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private string playerBulletKey = "PlayerBullet";
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireCooldown = 0.2f;
    [SerializeField] private Vector2 screenPadding = new Vector2(0.5f, 0.75f);

    private Camera mainCamera;
    private bool canControl;
    private float nextFireTime;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (!canControl)
        {
            return;
        }

        HandleMovement();
        HandleShooting();
    }

    public void SetControlState(bool enabled)
    {
        canControl = enabled;
        if (!enabled)
        {
            nextFireTime = 0f;
        }
    }

    public void ResetToCenter()
    {
        transform.position = Vector3.zero;
    }

    private void HandleMovement()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        Vector3 moveDirection = Vector3.zero;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            moveDirection += Vector3.left;
        }

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            moveDirection += Vector3.right;
        }

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            moveDirection += Vector3.down;
        }

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            moveDirection += Vector3.up;
        }

        moveDirection = moveDirection.normalized;
        transform.position += moveDirection * playerMoveSpeed * Time.deltaTime;

        ClampInsideScreen();
    }

    private void HandleShooting()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (!Keyboard.current.spaceKey.isPressed || Time.time < nextFireTime)
        {
            return;
        }

        nextFireTime = Time.time + fireCooldown;
        Transform spawnPoint = bulletSpawnPoint != null ? bulletSpawnPoint : transform;

        if (PoolManager.Instance != null && !string.IsNullOrWhiteSpace(playerBulletKey))
        {
            GameObject pooledBullet = PoolManager.Instance.SpawnFromPool(playerBulletKey, spawnPoint.position);
            if (pooledBullet == null)
            {
                return;
            }

            Bullet bullet = pooledBullet.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(Vector3.up, Bullet.BulletOwner.Player);
            }

            return;
        }

        if (bulletPrefab != null)
        {
            GameObject bulletObject = Instantiate(bulletPrefab, spawnPoint.position, Quaternion.identity);
            Bullet bullet = bulletObject.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Initialize(Vector3.up, Bullet.BulletOwner.Player);
            }
        }
    }

    private void ClampInsideScreen()
    {
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
        Vector3 clampedPosition = transform.position;

        clampedPosition.x = Mathf.Clamp(clampedPosition.x, min.x + screenPadding.x, max.x - screenPadding.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, min.y + screenPadding.y, max.y - screenPadding.y);
        clampedPosition.z = 0f;

        transform.position = clampedPosition;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player collision with: " + collision.collider.name + " | Tag: " + collision.collider.tag);

        if (collision.collider.CompareTag("Enemy"))
        {
            Debug.Log("Player hit Enemy -> GameOver");
            GameManager.Instance?.GameOver();
        }
    }
}
