using Unity.Netcode;
using UnityEngine;

    public class GameManager : NetworkBehaviour
    {
        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 300));
            
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                StartButtons();
            }
            else 
            {
                SubmitNewPosition();
            }

            GUILayout.EndArea();
        }

        static void StartButtons()
        {
            if (GUILayout.Button("Host")) NetworkManager.Singleton.StartHost();
            if (GUILayout.Button("Client")) NetworkManager.Singleton.StartClient();
        }

        static void SubmitNewPosition(){
            if (GUILayout.Button("Equipo Azul"))
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                var player = playerObject.GetComponent<Player>();
                player.Mueve(1);
            }

            if (GUILayout.Button("Sin equipo"))
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                var player = playerObject.GetComponent<Player>();
                player.Mueve(0);
            }

            if (GUILayout.Button("Equipo Rojo"))
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
                var player = playerObject.GetComponent<Player>();
                player.Mueve(2);
            }
        }

        static void StatusLabels()
        {
            
            var mode = NetworkManager.Singleton.IsHost ?
                "Host" : NetworkManager.Singleton.IsClient ? "Client" : "Client";

            GUILayout.Label("Transport: " +
                NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name);
            GUILayout.Label("Mode: " + mode);
        }    
    }
