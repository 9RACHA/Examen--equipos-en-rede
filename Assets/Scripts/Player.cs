using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    //Sincronizar variables en todos los clientes conectados
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> Team = new NetworkVariable<int>();
    public NetworkVariable<Color> ColorPlayer = new NetworkVariable<Color>();

    public Color Blanco;
    public Color Rojo;
    public Color Azul;

    private Renderer r;

    //Inicializar la variable y establecer referencias a componentes necesarios
    private void Awake()
    {
        r = GetComponent<Renderer>();
    }

    void AplicarColorJugador()
    {
        r.materials[0].color = ColorPlayer.Value;
    }

    void Start()
    {
        AplicarColorJugador();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            SolicitarPosicionInicialServerRpc();
            Team.Value = 0; // Equipo sin asignar
            ColorPlayer.Value = Blanco;
        }

        if (!IsOwner)
        {
            // Si no es propietario, se sincroniza el color
            EnColorJugadorCambiado(ColorPlayer.Value, ColorPlayer.Value);
        }
    }

    void EnColorJugadorCambiado(Color valorAnterior, Color nuevoValor)
    {
        ActualizarColorJugadorCliente(nuevoValor);
    }

    public void Mover()
    {
        if (IsOwner)
        {
            EnviarPosicionServerRpc();
        }
    }

    //son llamados desde el cliente y se ejecutan en el servidor
    [ServerRpc]
    void EnviarPosicionServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = ObtenerPosicionCentralEnPlano();
        ActualizarColorJugadorServerRpc();
    }

    [ServerRpc]
    void SolicitarPosicionInicialServerRpc()
    {
        Position.Value = ObtenerPosicionCentralEnPlano();
        ActualizarPosicionClientRpc(Position.Value);
    }

    [ServerRpc]
    void SolicitarCambioPosicionServerRpc(Vector3 direccion)
    {
        Position.Value += direccion;
        ActualizarPosicionClientRpc(Position.Value);
    }

    [ServerRpc]
    void SolicitarCambioEquipoServerRpc(int equipo, ServerRpcParams rpcParams = default)
    {
    CambiarEquipo(equipo);
    }

    //Sincronizar las actualizaciones de posición y color entre el servidor y los clientes.
    [ClientRpc]
    void ActualizarPosicionClientRpc(Vector3 nuevaPosicion)
    {
        if (!IsOwner)
            Position.Value = nuevaPosicion;
    }

    [ServerRpc]
    void ActualizarColorJugadorServerRpc()
    {
        int equipoActual = ObtenerEquipoPorPosicion(Position.Value);
        if (equipoActual != Team.Value)
        {
            Team.Value = equipoActual;
            ActualizarColorJugadorClientRpc(equipoActual);
        }
    }

    [ClientRpc]
    void ActualizarColorJugadorClientRpc(int equipo)
    {
        if (equipo == 0) // Sin equipo
            ColorPlayer.Value = Color.white;
        else if (equipo == 1) // Equipo 1
            ColorPlayer.Value = Color.blue;
        else if (equipo == 2) // Equipo 2
            ColorPlayer.Value = Color.red;
    }

    void ActualizarColorJugadorCliente(Color color)
    {
        ColorPlayer.Value = color;
        r.materials[0].color = color;
    }

    static Vector3 ObtenerPosicionCentralEnPlano()
    {
        return new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-3f, 3f));
    }

    //determinar la posición y el equipo basados en la colisión
    int ObtenerEquipoPorPosicion(Vector3 posicion)
    {
        Collider[] colliders = Physics.OverlapSphere(posicion, 0.1f);

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
    if (IsOwner && IsClient)
    {
        if (collision.gameObject.CompareTag("Azul"))
        {
            Debug.Log("Colisión con el tag Azul");
            SolicitarCambioEquipoServerRpc(1);
        }
        else if (collision.gameObject.CompareTag("Rojo"))
        {
            Debug.Log("Colisión con el tag Rojo");
            SolicitarCambioEquipoServerRpc(2);
        }
        else if (collision.gameObject.CompareTag("SinEquipo"))
        {
            Debug.Log("Colisión con el tag SinEquipo");
            SolicitarCambioEquipoServerRpc(0);
        }
    }
    }

    //Cambia el equipo del jugador, verificando si el equipo está lleno y notificando si el movimiento está restringido
    void CambiarEquipo(int equipo)
    {
    if (equipo != Team.Value)
    {
        int maxJugadoresPorEquipo = 2; // Número máximo de jugadores por equipo
        int contadorEquipo = ContarJugadoresEnEquipo(equipo);

        if (contadorEquipo >= maxJugadoresPorEquipo)
        {
            Debug.Log($"El equipo {equipo} está lleno");
            NotificarMovimientoRestringido();
            return;
        }

        Team.Value = equipo;
        ActualizarColorJugadorClientRpc(equipo);

        // Actualizar el color del jugador cliente local
        if (IsOwner)
        {
            ActualizarColorJugadorCliente(ColorPlayer.Value);
        }
    }
    }

    // Cuenta el número de jugadores en cada equipo.
    int ContarJugadoresEnEquipo(int equipo)
    {
        int contador = 0;

        foreach (Player player in FindObjectsOfType<Player>())
        {
            if (player.Team.Value == equipo)
            {
                contador++;
            }
        }

        return contador;
    }


    void NotificarMovimientoRestringido()
    {
        // Método para notificar a los jugadores que no pueden moverse porque el equipo está lleno
        Debug.Log("No puedes moverte porque el equipo está lleno");
    }

   void Update(){
    if (IsOwner)
    {
        Vector3 direccion = Vector3.zero;

        if (Input.GetKeyDown(KeyCode.M))
        {
            SolicitarPosicionInicialServerRpc();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            direccion = Vector3.left;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            direccion = Vector3.right;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            direccion = Vector3.back;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            direccion = Vector3.forward;
        }

        if (direccion != Vector3.zero)
        {
            SolicitarCambioPosicionServerRpc(direccion);
        }
    }

    AplicarColorJugador(); 

    transform.position = Position.Value;

    // Mostrar la cantidad de jugadores en cada equipo
    int contadorEquipoAzul = ContarJugadoresEnEquipo(1);
    int contadorEquipoRojo = ContarJugadoresEnEquipo(2);
    int contadorSinEquipo = ContarJugadoresEnEquipo(0);

    Debug.Log($"Jugadores en el Equipo Azul: {contadorEquipoAzul}");
    Debug.Log($"Jugadores en el Equipo Rojo: {contadorEquipoRojo}");
}

}



