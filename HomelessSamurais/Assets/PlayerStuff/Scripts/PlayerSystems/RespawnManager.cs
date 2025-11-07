using UnityEngine;
using Unity.Netcode;

public class RespawnManager : NetworkBehaviour
{
    [Header("Respawn Settings")]
    public Transform[] spawnPoints;
    public float respawnDelay = 3f;
    public float spawnHeight = 2f;
    
    private CharacterController characterController;
    private Health health;
    private bool isRespawning = false;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        characterController = GetComponent<CharacterController>();
        health = GetComponent<Health>();
        
        // If no spawn points assigned, create default ones (only on server)
        if (IsServer && (spawnPoints == null || spawnPoints.Length == 0))
        {
            CreateDefaultSpawnPoint();
        }
    }
    
    void CreateDefaultSpawnPoint()
    {
        GameObject spawnObj = new GameObject("DefaultSpawnPoint");
        spawnObj.transform.position = new Vector3(0, spawnHeight, 0);
        spawnPoints = new Transform[] { spawnObj.transform };
    }
    
    public void TriggerRespawn()
    {
        if (!IsOwner || isRespawning || !IsSpawned) return;
        
        RequestRespawnServerRpc();
    }
    
    [ServerRpc]
    void RequestRespawnServerRpc()
    {
        if (isRespawning) return;
        
        isRespawning = true;
        Invoke(nameof(ExecuteRespawn), respawnDelay);
    }
    
    void ExecuteRespawn()
    {
        if (!IsServer || spawnPoints == null || spawnPoints.Length == 0) 
        {
            isRespawning = false;
            return;
        }
        
        // Pick random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Disable controller to teleport
        if (characterController != null)
        {
            characterController.enabled = false;
        }
        
        // Set position
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        
        // Re-enable controller
        if (characterController != null)
        {
            characterController.enabled = true;
        }
        
        // Reset health
        if (health != null)
        {
            health.Respawn();
        }
        
        // Reset velocity
        FPController controller = GetComponent<FPController>();
        if (controller != null)
        {
            controller.ResetVerticalVelocity();
        }
        
        isRespawning = false;
        
        // Notify clients
        RespawnClientRpc(spawnPoint.position, spawnPoint.rotation);
    }
    
    [ClientRpc]
    void RespawnClientRpc(Vector3 position, Quaternion rotation)
    {
        if (IsOwner) return; // Owner already handled it
        
        transform.position = position;
        transform.rotation = rotation;
        
        isRespawning = false;
    }
}