using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class NetworkUI : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 300));
        
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Host"))
            {
                NetworkManager.Singleton.StartHost();
            }
            
            if (GUILayout.Button("Client (localhost)"))
            {
                // Connect to host on same machine
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                transport.SetConnectionData("127.0.0.1", 7777);
                NetworkManager.Singleton.StartClient();
            }
        }
        else
        {
            // Show status when connected
            GUILayout.Label($"Mode: {(NetworkManager.Singleton.IsHost ? "Host" : "Client")}");
            
            // Only show connected players count on server/host
            if (NetworkManager.Singleton.IsServer)
            {
                GUILayout.Label($"Connected Players: {NetworkManager.Singleton.ConnectedClients.Count}");
            }
            
            if (GUILayout.Button("Disconnect"))
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    NetworkManager.Singleton.Shutdown();
                }
                else if (NetworkManager.Singleton.IsClient)
                {
                    NetworkManager.Singleton.Shutdown();
                }
            }
        }
        
        GUILayout.EndArea();
    }
}