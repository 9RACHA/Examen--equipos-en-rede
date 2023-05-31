using Unity.Netcode;
using UnityEngine;


    public class Player : NetworkBehaviour
    {
        // Variable de red que almacena la posición del jugador
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        // Método que se ejecuta cuando el jugador es creado en la red
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // El propietario del jugador solicita la posición inicial al servidor
                RequestInitialPositionServerRpc();
            }
        }

        // RPC que se ejecuta en el servidor para generar una posición inicial aleatoria
        [ServerRpc]
        void RequestInitialPositionServerRpc()
        {
            // Genera una posición aleatoria en un plano
            Position.Value = GetRandomPositionOnPlane();

            // Actualiza la posición en los clientes para que vean la misma posición inicial
            UpdatePositionClientRpc(Position.Value);
        }

        // RPC que se ejecuta en el servidor para solicitar el cambio de posición
        [ServerRpc]
        void RequestPositionChangeServerRpc(Vector3 direction)
        {
            // Actualiza la posición sumando la dirección especificada
            Position.Value += direction;

            // Actualiza la posición en los clientes para sincronizar el movimiento
            UpdatePositionClientRpc(Position.Value);
        }

        // RPC que se ejecuta en los clientes para sincronizar la posición
        [ClientRpc]
        void UpdatePositionClientRpc(Vector3 newPosition)
        {
            // Si el jugador no es el propietario, actualiza la posición recibida
            if (!IsOwner)
                Position.Value = newPosition;
        }

        // Genera una posición aleatoria en un plano
        static Vector3 GetRandomPositionOnPlane()
        {
            return new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        }

        void Update()
        {
            if (IsOwner)
            {
                Vector3 direction = Vector3.zero;

                // Detecta las flechas para determinar la dirección del movimiento
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                    direction = Vector3.left;
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                    direction = Vector3.right;
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    direction = Vector3.back;
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                    direction = Vector3.forward;
                

                // Si se presiona alguna tecla de flecha, solicita el cambio de posición al servidor
                if (direction != Vector3.zero)
                    RequestPositionChangeServerRpc(direction);
            }

            // Actualiza la posición del objeto en el mundo del juego basándose en el valor actual de la variable de red Position, 
            // asegurando que el movimiento se sincronice correctamente en todos los clientes
            transform.position = Position.Value;
        }
    }