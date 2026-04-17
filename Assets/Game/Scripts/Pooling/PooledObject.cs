using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public string PoolKey { get; private set; }

    public void SetPoolKey(string poolKey)
    {
        PoolKey = poolKey;
    }
}
