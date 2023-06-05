using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour{

    // Variables de red para la posición, el equipo y el color del jugador
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public NetworkVariable<int> Team = new NetworkVariable<int>();
    public NetworkVariable<Color> ColorPlayer = new NetworkVariable<Color>();

    // Colores disponibles para los equipos y el color blanco
    public Color Blanco;
    public Color Rojo;
    public Color Naranja;
    public Color Rosa;
    public Color Azul;
    public Color AzulOscuro;
    public Color AzulClaro;

    private Renderer r; // Referencia al componente Renderer del jugador

    // Listas de colores disponibles para cada equipo
    private List<Color> coloresEquipo1 = new List<Color>();
    private List<Color> coloresEquipo2 = new List<Color>();

    private void Awake(){
        r = GetComponent<Renderer>(); // Obtener el componente Renderer del jugador

        // Inicializar los colores disponibles para cada equipo
        coloresEquipo1.Add(Rojo);
        coloresEquipo1.Add(Naranja);
        coloresEquipo1.Add(Rosa);

        coloresEquipo2.Add(Azul);
        coloresEquipo2.Add(AzulOscuro);
        coloresEquipo2.Add(AzulClaro);
    }

    // Aplicar el color del jugador al material del Renderer
    void AplicarColorJugador()
    {
        r.materials[0].color = ColorPlayer.Value;
    }

    void Start()
    {
        // Suscribirse al evento OnValueChanged de ColorPlayer
        ColorPlayer.OnValueChanged += ColorPlayerValueChanged;
        AplicarColorJugador();
    }

    // Método llamado cuando el jugador es spawneado en la red
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
            ColorJugadorCambiado(ColorPlayer.Value, ColorPlayer.Value);
        }
    }

    // Maneja el evento OnValueChanged de ColorPlayer
    void ColorPlayerValueChanged(Color oldValue, Color newValue){
    Debug.Log("El valor de ColorPlayer ha cambiado de " + oldValue + " a " + newValue);

    // Cambiar el material del objeto del jugador cuando el valor de ColorPlayer cambia
    if (r != null && r.materials.Length > 0)
    {
        r.materials[0].color = newValue;
    }
    }

    // Método llamado cuando el color del jugador cambia en el servidor
    void ColorJugadorCambiado(Color valorAnterior, Color nuevoValor){
        ActualizarColorJugadorCliente(nuevoValor);
    }

    // Método para solicitar mover al jugador (solo llamado por el cliente dueño del objeto)
    public void Mover(){
        if (IsOwner)
        {
            EnviarPosicionServerRpc();
        }
    }

    // Método RPC para enviar la posición actual del jugador al servidor
    [ServerRpc]
    void EnviarPosicionServerRpc(ServerRpcParams rpcParams = default)
    {
        Position.Value = ObtenerPosicionCentralEnPlano();
        ActualizarColorJugadorServerRpc();
    }

    // Solicita la posición inicial del jugador al servidor
    [ServerRpc]
    void SolicitarPosicionInicialServerRpc()
    {
        Position.Value = ObtenerPosicionCentralEnPlano();
        ActualizarPosicionClientRpc(Position.Value);
    }

    // Solicita el cambio de posición del jugador al servidor
    [ServerRpc]
    void SolicitarCambioPosicionServerRpc(Vector3 direccion)
    {
        Position.Value += direccion;
        ActualizarPosicionClientRpc(Position.Value);
    }

    // Solicitar el cambio de equipo del jugador al servidor
    [ServerRpc]
    void SolicitarCambioEquipoServerRpc(int equipo, ServerRpcParams rpcParams = default)
    {
        CambiarEquipo(equipo);
    }

    // Actualiza la posición del jugador en los clientes
    [ClientRpc]
    void ActualizarPosicionClientRpc(Vector3 nuevaPosicion)
    {
        if (!IsOwner)
            Position.Value = nuevaPosicion;
    }

    // Método RPC que actualiza el color del jugador en los clientes
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

    // Actualizar el color del jugador en un cliente específico
    [ClientRpc]
    void ActualizarColorJugadorClientRpc(int equipo)
    {
        if (equipo == 0) // Sin equipo
            ColorPlayer.Value = Color.white;
        else if (equipo == 1) // Equipo 1 (Rojo)
            ColorPlayer.Value = ObtenerColorAleatorioEquipo(coloresEquipo1);
        else if (equipo == 2) // Equipo 2 (Azul)
            ColorPlayer.Value = ObtenerColorAleatorioEquipo(coloresEquipo2);
    }

    // Método para obtener un color aleatorio de un equipo específico
    Color ObtenerColorAleatorioEquipo(List<Color> coloresEquipo)
    {
        if (coloresEquipo.Count == 0)
        {
            Debug.LogWarning("No hay colores disponibles en el equipo");
            return Color.white;
        }

        int indiceColorAleatorio = Random.Range(0, coloresEquipo.Count);
        Color colorAleatorio = coloresEquipo[indiceColorAleatorio];
        coloresEquipo.RemoveAt(indiceColorAleatorio);
        return colorAleatorio;
    }

    // Actualizar el color del jugador en un cliente específico y en el Renderer
    void ActualizarColorJugadorCliente(Color color)
    {
        ColorPlayer.Value = color;
        r.materials[0].color = color;
    }

    // Método para obtener una posición central aleatoria en un plano
    static Vector3 ObtenerPosicionCentralEnPlano()
    {
        return new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-3f, 3f));
    }

    // Obtiene el equipo correspondiente a una posición dada
    int ObtenerEquipoPorPosicion(Vector3 posicion)
    {
        Collider[] colliders = Physics.OverlapSphere(posicion, 0.1f);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Azul"))
                return 2; // Equipo 2 (Azul)
            else if (collider.CompareTag("Rojo"))
                return 1; // Equipo 1 (Rojo)
            else if (collider.CompareTag("SinEquipo"))
                return 0; // Sin equipo (parte centro)
        }

        return -1; // No se encontró un equipo válido
    }

    // Método para manejar las colisiones del jugador
    void OnCollisionEnter(Collision collision)
    {
        if (IsOwner && IsClient)
        {
            if (collision.gameObject.CompareTag("Azul"))
            {
                Debug.Log("Colisión con el tag Azul");
                SolicitarCambioEquipoServerRpc(2);
            }
            else if (collision.gameObject.CompareTag("Rojo"))
            {
                Debug.Log("Colisión con el tag Rojo");
                SolicitarCambioEquipoServerRpc(1);
            }
            else if (collision.gameObject.CompareTag("SinEquipo"))
            {
                Debug.Log("Colisión con el tag SinEquipo");
                SolicitarCambioEquipoServerRpc(0);
            }
        }
    }

    //Cambiar de equipo
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

            if (IsOwner)
            {
                ActualizarColorJugadorCliente(ColorPlayer.Value);
            }
        }
    }

    // Cuenta el número de jugadores en un equipo
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

    // Notifica al jugador que su movimiento está restringido debido a que el equipo está lleno
    void NotificarMovimientoRestringido()
    {
        Debug.Log("No puedes moverte porque el equipo está lleno");
    }

    void Update()
    {
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
        int contadorEquipoAzul = ContarJugadoresEnEquipo(2);
        int contadorEquipoRojo = ContarJugadoresEnEquipo(1);
        int contadorSinEquipo = ContarJugadoresEnEquipo(0);
        Debug.Log($"Jugadores en el Equipo Azul: {contadorEquipoAzul}");
        Debug.Log($"Jugadores en el Equipo Rojo: {contadorEquipoRojo}");
    }
}
