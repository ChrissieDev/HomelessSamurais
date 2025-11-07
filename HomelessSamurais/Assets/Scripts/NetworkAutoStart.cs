using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkAutoStart : MonoBehaviour
{
    void Start()
    {
        Debug.Log("NetworkAutoStart: Start() called");
        Debug.Log($"Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // Check what mode we should start in
        string mode = PlayerPrefs.GetString("NetworkMode", "");
        Debug.Log($"NetworkMode from PlayerPrefs: '{mode}'");
        
        if (mode == "Host")
        {
            Debug.Log("Starting as Host...");
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started!");
            
            // Debug spawn
            Invoke(nameof(DebugSpawn), 1f);
        }
        else if (mode == "Client")
        {
            string serverIP = PlayerPrefs.GetString("ServerIP", "127.0.0.1");
            Debug.Log($"Starting as Client, connecting to {serverIP}");
            
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(serverIP, 7777);
            
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client started!");
        }
        else
        {
            Debug.LogWarning($"Unknown or empty NetworkMode: '{mode}'");
        }
        
        // Clear the saved mode so it doesn't auto-connect next time
        PlayerPrefs.DeleteKey("NetworkMode");
        PlayerPrefs.Save();
    }
    
    void DebugSpawn()
    {
        Debug.Log($"Player prefab assigned: {NetworkManager.Singleton.NetworkConfig.PlayerPrefab != null}");
        Debug.Log($"Connected clients: {NetworkManager.Singleton.ConnectedClientsIds.Count}");
        Debug.Log($"Local client ID: {NetworkManager.Singleton.LocalClientId}");
        
        // Check if player exists
        var players = FindObjectsOfType<FPController>();
        Debug.Log($"Players in scene: {players.Length}");
    }
}