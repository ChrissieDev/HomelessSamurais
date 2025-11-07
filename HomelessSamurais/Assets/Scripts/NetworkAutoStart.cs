using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkAutoStart : MonoBehaviour
{
    void Start()
    {
        Debug.Log("NetworkAutoStart: Start() called");
        
        // Check what mode we should start in
        string mode = PlayerPrefs.GetString("NetworkMode", "");
        Debug.Log($"NetworkMode from PlayerPrefs: '{mode}'");
        
        if (mode == "Host")
        {
            Debug.Log("Starting as Host...");
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host started!");
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
}