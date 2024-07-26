using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages object pooling for efficient object reuse.
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; } // Singleton instance of ObjectPool

    [System.Serializable]
    public class Pool
    {
        public string poolTag;                  // Tag to identify the pool
        public GameObject[] prefabVariants;     // Array of prefab variants to use in the pool
        public int poolSize;                    // Size of the pool
        public int disableDelay;                // Delay before disabling objects
        public Transform parentTransform;       // Parent transform to organize instantiated objects
    }

    [SerializeField] private List<Pool> pools;  // List of pools
    private Dictionary<string, Queue<GameObject>> poolDictionary; // Dictionary to store pools by tag

    void Awake()
    {
        // Create a singleton instance of ObjectPool
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Initialize all object pools
        InitializePools(); 
    }

    /// <summary>
    /// Initializes the object pools.
    /// </summary>
    private void InitializePools()
    {
        // Create a dictionary to store pools by tag
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (var pool in pools)
        {
            // Create a queue to store objects in the pool
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.poolSize; i++)
            {
                // Randomly select a prefab variant from the array
                int randomIndex = pool.prefabVariants.Length > 1 ? Random.Range(0, pool.prefabVariants.Length) : 0;

                // Instantiate prefab variant
                GameObject obj = Instantiate(pool.prefabVariants[randomIndex], pool.parentTransform); 
                obj.SetActive(false); // Deactivate the object
                objectPool.Enqueue(obj); // Add the object to the pool
            }
            // Add the pool to the dictionary
            poolDictionary.Add(pool.poolTag, objectPool); 
        }
    }

    /// <summary>
    /// Spawns an object from the pool.
    /// </summary>
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.TryGetValue(tag, out var objectPool))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return null;
        }
        
        GameObject objectToSpawn = objectPool.Dequeue(); // Get the next object from the pool
        objectToSpawn.SetActive(true); // Activate the object
        objectToSpawn.transform.SetPositionAndRotation(position, rotation); // Set the position and rotation
        objectPool.Enqueue(objectToSpawn); // Re-enqueue the object for future use

        return objectToSpawn;
    }

    /// <summary>
    /// Disables an object from the pool after a delay.
    /// </summary>
    public void DisableFromPool(string tag, GameObject objectToDisable, int destroyTime)
    {
        if (!poolDictionary.TryGetValue(tag, out var objectPool))
        {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist.");
            return;
        }
        // Start coroutine to disable the object after a delay
        StartCoroutine(DisableObjectAfterTime(objectToDisable, destroyTime)); 
    }

    /// <summary>
    /// Disables an object after a delay.
    /// </summary>
    private IEnumerator DisableObjectAfterTime(GameObject objectToDisable, int destroyTime)
    {
        yield return new WaitForSeconds(destroyTime); // Wait for the specified delay
        objectToDisable.SetActive(false); // Deactivate the object
    }
}