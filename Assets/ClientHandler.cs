using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ClientHandler : MonoBehaviour
{
    public bool test = false;
    private NetworkHelper networkHelper;

    public int idJuego = -1;
    public string nombre = "";
    public int equipo = -1; //0 -> azul, 1 -> rojo, -1 -> sin equipo
    public int personaje = -1;

    public Dictionary<int,Jugador> Jugadores = new Dictionary<int, Jugador>();
    public Dictionary<int,PlayerControllerClient.ClassType> Personajes = new Dictionary<int, PlayerControllerClient.ClassType>();
    int ultimo = -1;
    public int nombreCamb = -1;
    public int prep = -1;
    public int compi = -1; // ID del compi
    public int persCompi = -1;
    public bool eresCompi = false;

    // Eventos para no complicarnos la vida
    // public UnityEvent CompiCambiado;
    public UnityEvent JugadorNuevo;
    public UnityEvent JugadorPreparado;
    public UnityEvent FaseSeleccion;
    public UnityEvent PuedoEscoger;
    public UnityEvent NombreCambiado;

    public UnityEvent RecibirTile;
    public string tileRecibido;
    public string tilemapRecibido;
    public Vector3 tilePos;
    public UnityEvent MostrarMapaEvent;
    public UnityEvent LlegaInputEvent;
    public UnityEvent LlegaPjEvent;

    public int idInput;
    public string TipoInput;
    public Vector3 InputVec3;

    public int idPj;
    public PlayerControllerClient.ClassType Pj;

    private void Start()
    {
        DontDestroyOnLoad(this);
        // CompiCambiado = new UnityEvent();
        JugadorNuevo = new UnityEvent();
        JugadorPreparado = new UnityEvent();
        FaseSeleccion = new UnityEvent();
        PuedoEscoger = new UnityEvent();
        NombreCambiado = new UnityEvent();
        RecibirTile = new UnityEvent();
        MostrarMapaEvent = new UnityEvent();
        LlegaInputEvent = new UnityEvent();
        LlegaPjEvent = new UnityEvent();

    }

    public bool StartClient(int localPort, string remoteIP, int remotePort, string nombreJug)
    {
        networkHelper = FindObjectOfType<NetworkHelper>();
        networkHelper.onConnect.AddListener(ConnectedToServer);
        networkHelper.onDisconnect.AddListener(DisconnectedFromServer);
        networkHelper.onMessageReceived.AddListener(ReceiveMessage);

        nombre = nombreJug;

        Debug.Log(remotePort);

        return networkHelper.ConnectToServer(localPort, remoteIP, remotePort);
    }

    private void ConnectedToServer()
    {
        // Le decimos al servidor como nos llamamos
        SendToServer("Nom_"+nombre);
        
    }

    private void DisconnectedFromServer()
    {
    }

    private void ReceiveMessage(string message)
    {
        // Separamos el mensaje entero por si hay mas mensajes dentro separados por ;
        string[] mensajes = message.Split(';');

        foreach (var mensaje in mensajes)
        {
            Mensaje(mensaje);
            
        }

        // Example: Print message on chat
        // GameObject.FindWithTag("Chat").GetComponent<ChatController>().AddChatToChatOutput(message);
    }

    public void SendToServer(string message)
    {
        networkHelper.SendToServer(message);
    }


    void Mensaje(string mensaje){
        string[] args = mensaje.Split("_"); //args[0] tiene el tipo de mensaje, args[1] contenido
        string[] args2 = args[1].Split(",");
        switch(args[0]){
            case "Input":
                LlegaInput(args2);
                break;

            case "Personaje":
                LlegaPj(args2);
                break;

            case "Tile":
                LlegaTile(args2);
                break;

            case "OK":
                MostrarMapa();
                break;

            case "Juego":
                EmpezarJuego();
                break;

            case "Eq": // Avisa del equipo en el que estamos
                PonerseEquipo(int.Parse(args2[0])); // Nos ponemos en el equipo que toca
                idJuego = int.Parse(args2[1]);
                break;

            case "Comp": // Nos dice quién es nuestro compañero id,nombre
                CambiarCompi(int.Parse(args[1]));
                break;

            case "Jug": // Nos dice los datos del nuevo jugador
                NuevoJugador(args[1]);
                break;

            case "Nom": // Nos dice el nombre de un jugador
                CambiarNombre(int.Parse(args2[0]),args2[1]);
                break;

            case "Prep": // Nos dice qué jugador está preparado
                prep = int.Parse(args[1]);
                JugadorPreparado.Invoke();
                break;

            case "Esc": // Nos dice que podemos escoger personaje y el del compi si es que ya lo tiene
                if(int.Parse(args[1])!=-1) persCompi = int.Parse(args[1]);
                PuedoEscoger.Invoke();
                break;

            case "Pers": // Nos dice que ha empezado la fase de seleccion
                FaseSeleccion.Invoke();
                break;

            default:
                Debug.LogError("Mensaje no detectado");
                break;
        }
    }

    // FUNCIONES DEL JUEGO
    void PonerseEquipo(int eq){
        equipo = eq;
        SceneManager.LoadScene("ClientDEF");
    }

    void CambiarCompi(int idCompi){
        compi = idCompi;
    }

    void NuevoJugador(string datos){
        Jugador j = new Jugador();
        Debug.Log(datos);
        JsonUtility.FromJsonOverwrite(datos,j);
        if(!Jugadores.ContainsKey(j.id)){
            Jugadores.Add(j.id,j);
            ultimo = j.id;
            JugadorNuevo.Invoke();

        }
    }

    void CambiarNombre(int idJug, string nombreJug){
        Jugadores[idJug].nombre = nombreJug;
        nombreCamb = idJug;
        // if(idJug==compi) CompiCambiado.Invoke();
        NombreCambiado.Invoke();
    }

    public string NombreCompi(){
        return Jugadores[compi].nombre;
    }

    public void Preparado(){
        SendToServer("Prep_");
    }

    public Jugador UltimoJugador(){
        return Jugadores[ultimo];
    }

    public void Escoger(int idPersonaje){
        personaje = idPersonaje;
        SendToServer("Pers_"+idPersonaje);
    }

    void EmpezarJuego(){
        SceneManager.LoadScene("MapaClient");
    }

    void LlegaTile(string[] args){
        Debug.Log(args[0]);
        tilePos = Utilidades.FormatString(args[0]);
        tileRecibido = args[1];
        tilemapRecibido = args[2];
        RecibirTile.Invoke();
    }

    public void EnviarInput(string tipo, Vector3 pos){
        SendToServer("Input_"+tipo+","+Utilidades.FormatVector(pos));
    }

    void MostrarMapa(){
        MostrarMapaEvent.Invoke();
    }

    void LlegaInput(string[] args){
        TipoInput = args[0];
        InputVec3 = Utilidades.FormatString(args[1]);
        idInput = int.Parse(args[2]);
        LlegaInputEvent.Invoke();
    }

    void LlegaPj(string[] args){
        idPj = int.Parse(args[0]);
        Pj = (PlayerControllerClient.ClassType)int.Parse(args[1]);
        LlegaPjEvent.Invoke();
    }
}
