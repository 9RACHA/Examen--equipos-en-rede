using UnityEngine;
using UnityEngine.Networking;
using Unity.Netcode;
using Unity.Netcode.Components;
using System.Collections.Generic;

public class Player : NetworkBehaviour {

    public float velocidadMovimiento = 4f;
    public Rigidbody rb;
    public Renderer r;
    public int equipoAnterior;

    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    public NetworkVariable<Color> color;

    public static List<GameObject> equipoAzul;
    public static List<GameObject> equipoRojo;
    
   // public static NetworkList<GameObject> equipoRojo;

    //public NetworkList(IEnumerable<T> values = null, NetworkVariableReadPermission readPerm = NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission writePerm = NetworkVariableWritePermission.Server);
   
    public override void OnNetworkSpawn(){
        if (IsOwner)
        {
            RequestInitialPositionServerRpc();
        }
    }

    [ServerRpc]
    void RequestInitialPositionServerRpc(){
        Position.Value = GetRandomPositionOnPlane();
        UpdatePositionClientRpc(Position.Value);
    }

    [ServerRpc]
    void RequestPositionChangeServerRpc(Vector3 direction){
        Position.Value += direction;
        UpdatePositionClientRpc(Position.Value);
    }

    [ClientRpc]
    void UpdatePositionClientRpc(Vector3 newPosition){
        if (!IsOwner)
        {
            Position.Value = newPosition;
        }

    [ServerRpc]
    void SubmitPositionRequestServerRpc(int equipo = 0, ServerRpcParams rpcParams = default){
        Position.Value = GetRandomPositionOnPlane();
        EquipoSeleccionado(equipo);

        if (equipo == 1)
        {
            equipoAzul.Add(gameObject);
        }
        else if( equipo == 2) {
            equipoRojo.Add(gameObject);
        }

        Debug.Log("Integrantes azules:" + equipoAzul.Count);
        Debug.Log("Integrantes rojos:" + equipoRojo.Count);
    }

     void Mueve(int equipo = 0) {
        if (equipoAzul.Count >= 2 && equipo == 1) {
            equipo = equipoAnterior;
            Debug.Log("Equipo azul lleno");
        }
            else if (equipoRojo.Count >= 2 && equipo == 2){
                equipo = equipoAnterior;
                Debug.Log("Equipo rojo lleno");
            } else {
                equipoAnterior = equipo;
                SubmitPositionRequestServerRpc(equipo);
            }
        } 


    void EquipoSeleccionado(int equipo = 0){
        if (equipo == 0){
            color.Value = Color.white;
    } else if (equipo == 1){
        color.Value = Color.blue;
    } else if (equipo == 2){
        color.Value = Color.red;
    }
    Debug.Log("El valor del equipo es " + equipo);
    }

   static Vector3 GetRandomPositionOnPlane(int equipo = 0)
        {
            if (equipo == 0)
            {
                return new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-3f, 3f));
            }
            else if (equipo == 1)
            {
                return new Vector3(Random.Range(-3f, -1f), 1f, Random.Range(-3f, 3f));
            }
            else
            {
                return new Vector3(Random.Range(1f, 3f), 1f, Random.Range(-3f, 3f));
            }
        }

    void OnPositionChanged(Vector3 antiguaPos, Vector3 nuevaPos){
        transform.position = Position.Value;
    }

    void OnColorChanged(Color antiguoColor, Color nuevoColor) {
        r.material.color = color.Value;
    }

    
     void Awake(){
        r = GetComponent<Renderer>();
        equipoAzul = new List<GameObject>();
        equipoRojo = new List<GameObject>();
    }

     void Start() {
        rb = GetComponent<Rigidbody>();
        Position.OnValueChanged += OnPositionChanged;
        color.OnValueChanged += OnColorChanged;
    }

     void Update() {

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

                // Si se ha presionado alguna tecla de flecha, solicita el cambio de posición al servidor
                if (direction != Vector3.zero)
                    RequestPositionChangeServerRpc(direction);
            }

            // Actualiza la posición del objeto en el mundo del juego basándose en el valor actual de la variable de red Position, 
            // asegurando que el movimiento se sincronice correctamente en todos los clientes
            transform.position = Position.Value;

    }
    }
}
