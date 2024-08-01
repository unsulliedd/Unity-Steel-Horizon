using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Threading.Tasks;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }
    public string joinCode;
    public Allocation allocation;
    public JoinAllocation joinAllocation;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(4);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log($"Relay server created with join code: {joinCode}");
            return joinCode;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError($"Failed to create Relay server: {ex.Message}");
            return null;
        }
    }

    public async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Joined relay server.");
            return joinAllocation;
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError($"Failed to join Relay server: {ex.Message}");
            return null;
        }
    }
}
