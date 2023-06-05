using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> Team = new NetworkVariable<int>();
    public NetworkVariable<Color> ColorPlayer = new NetworkVariable<Color>();

    public Color Blanco;
    public Color Rojo;
    public Color Azul;

    private Renderer playerRenderer;

    private void Awake()
    {
        playerRenderer = GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            RequestInitialPositionServerRpc();
            Team.Value = 0; // Equipo sin asignar
            ColorPlayer.Value = Blanco;
        }
       
        if (!IsOwner)
        {
        // si no es propietario se sincroniza el color
        OnPlayerColorChanged(ColorPlayer.Value, ColorPlayer.Value);
        }
    }
    

     void OnPlayerColorChanged(Color previousValue, Color newValue){
    playerRenderer.materials[0].color = newValue;
    }   


    public void Mover()
    {
        if (IsOwner)
        {
            SubmitPositionServerRpc();
        }
    }

    [ServerRpc]
    void SubmitPositionServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = GetCentralPositionOnPlane();
        UpdatePlayerColorServerRpc();
    }

    [ServerRpc]
    void RequestInitialPositionServerRpc()
    {
        Position.Value = GetCentralPositionOnPlane();
        UpdatePositionClientRpc(Position.Value);
    }

    [ServerRpc]
    void RequestPositionChangeServerRpc(Vector3 direction)
    {
        Position.Value += direction;
        UpdatePositionClientRpc(Position.Value);
    }

    [ClientRpc]
    void UpdatePositionClientRpc(Vector3 newPosition)
    {
        if (!IsOwner)
            Position.Value = newPosition;
    }

    [ServerRpc]
    void UpdatePlayerColorServerRpc()
    {
        int currentTeam = GetTeamByPosition(Position.Value);
        if (currentTeam != Team.Value)
        {
            Team.Value = currentTeam;
            UpdatePlayerColorClientRpc(currentTeam);
        }
    }

    [ClientRpc]
    void UpdatePlayerColorClientRpc(int team)
    {
        if (IsOwner)
        {
            if (team == 0) // Sin equipo
                ColorPlayer.Value = Color.white;
            else if (team == 1) // Equipo 1
                ColorPlayer.Value = Color.blue;
            else if (team == 2) // Equipo 2
                ColorPlayer.Value = Color.red;
        }
    }

    static Vector3 GetCentralPositionOnPlane()
    {
        return new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-3f, 3f));
    }

    int GetTeamByPosition(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.1f);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Azul"))
                return 1; // Equipo 1
            else if (collider.CompareTag("Rojo"))
                return 2; // Equipo 2
            else if (collider.CompareTag("SinEquipo"))
                return 0; // Sin equipo (parte central)
        }

        return -1; // No se encontró un equipo válido
    }

    void OnCollisionEnter(Collision collision)
{
    if (!IsOwner)
        return;

    if (collision.collider.CompareTag("Azul"))
    {
        Debug.Log("Colisión con el tag Azul");
        ChangeTeam(1);
    }
    else if (collision.collider.CompareTag("Rojo"))
    {
        Debug.Log("Colisión con el tag Rojo");
        ChangeTeam(2);
    }
    else if (collision.collider.CompareTag("SinEquipo"))
    {
        Debug.Log("Colisión con el tag SinEquipo");
        ChangeTeam(0);
    }
}


    void ChangeTeam(int team)
    {
        if (team != Team.Value)
        {
            Team.Value = team;
            UpdatePlayerColorClientRpc(team);
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            Vector3 direction = Vector3.zero;

            if (Input.GetKeyDown(KeyCode.M))
            {
                RequestInitialPositionServerRpc();
            }

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                direction = Vector3.left;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                direction = Vector3.right;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                direction = Vector3.back;
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                direction = Vector3.forward;
            }

            if (direction != Vector3.zero)
            {
                RequestPositionChangeServerRpc(direction);
            }
        }

        transform.position = Position.Value;
        playerRenderer.material.color = ColorPlayer.Value;
    }
}

