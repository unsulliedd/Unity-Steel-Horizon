using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class CharacterGroup
{
    public string groupName;
    public GameObject[] characters;
    public int spawnCount;
    public Transform[] spawnPoints;
}

public class WorldAIManager : MonoBehaviour
{
    public static WorldAIManager instance;

    [Header("Debug")]
    [SerializeField] private bool despawnCharacters = false;
    [SerializeField] private bool respawnCharacters = false;

    [Header("Character Groups")]
    [SerializeField] private CharacterGroup[] characterGroups;

    private Dictionary<string, Vector3[]> groupSpawnPoints;
    public List<GameObject> spawnedInCharacters = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Ensure this persists across scene loads
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeGroupSpawnPoints();
    }

    void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            StartCoroutine(WaitForSceneToLoadAndSpawnCharacters());
            Debug.Log("Operation successful");
        }
    }

    void Update()
    {
        if (respawnCharacters)
        {
            respawnCharacters = false;
            SpawnAllCharacters();
        }

        if (despawnCharacters)
        {
            despawnCharacters = false;
            DespawnAllCharacters();
        }
    }

    private void InitializeGroupSpawnPoints()
    {
        groupSpawnPoints = new Dictionary<string, Vector3[]>();

        foreach (var group in characterGroups)
        {
            Vector3[] spawnPositions = new Vector3[group.spawnPoints.Length];
            for (int i = 0; i < group.spawnPoints.Length; i++)
            {
                spawnPositions[i] = group.spawnPoints[i].position;
            }
            groupSpawnPoints[group.groupName] = spawnPositions;
        }
    }

    IEnumerator WaitForSceneToLoadAndSpawnCharacters()
    {
        while (!SceneManager.GetActiveScene().isLoaded)
        {
            yield return null;
        }

        SpawnAllCharacters();
    }

    private void SpawnAllCharacters()
    {
        foreach (var group in characterGroups)
        {
            if (groupSpawnPoints.TryGetValue(group.groupName, out Vector3[] spawnPositions))
            {
                int spawnIndex = 0;
                for (int i = 0; i < group.spawnCount; i++)
                {
                    var character = group.characters[i % group.characters.Length];
                    Vector3 spawnPosition = spawnPositions[spawnIndex % spawnPositions.Length];
                    GameObject instantiatedCharacter = Instantiate(character, spawnPosition, Quaternion.identity);
                    instantiatedCharacter.GetComponent<NetworkObject>().Spawn();
                    spawnedInCharacters.Add(instantiatedCharacter);
                    spawnIndex++;
                }
            }
            else
            {
                Debug.LogWarning($"Spawn points for group {group.groupName} not found.");
            }
        }
    }

    private void DespawnAllCharacters()
    {
        foreach (var character in spawnedInCharacters)
        {
            character.GetComponent<NetworkObject>().Despawn();
        }

        spawnedInCharacters.Clear();
    }
}
