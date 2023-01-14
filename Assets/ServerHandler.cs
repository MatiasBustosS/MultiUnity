using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class Jugador{
    public int id;
    public string nombre;
    public int equipo;
    public int personaje;
}

public class ServerHandler : MonoBehaviour
{
    private NetworkHelper networkHelper;

    List<int> EquipoAzul = new List<int>();
    int nAzul = 0;
    List<int> EquipoRojo = new List<int>();
    int nRojo = 0;

    // Probablemente se pueda usar una lista, pero como solo van a haber 4 tampoco pasa nada
    Dictionary<int,Jugador> Jugadores = new Dictionary<int, Jugador>();
    int nJugadores = 0;
    int nPreparados = 0;
    int nEscogidos = 0;

    public bool empezado = false;
    public bool JuegoEmpezado = false;

    public UnityEvent LlegaInputEvent;
    public int idInput;
    public string TipoInput;
    public Vector3 InputVec3;

    private void Start()
    {
        DontDestroyOnLoad(this);
        LlegaInputEvent = new UnityEvent();
    }

    public bool StartServer(int localPort)
    {
        networkHelper = FindObjectOfType<NetworkHelper>();
        networkHelper.onHostAdded.AddListener(ServerStarted);
        networkHelper.onHostRemoved.AddListener(ServerStopped);
        networkHelper.onConnectClient.AddListener(ClientConnected);
        networkHelper.onDisconnectClient.AddListener(ClientDisconnected);
        networkHelper.onMessageReceivedFrom.AddListener(ReceiveMessage);
        return networkHelper.MakeServer(localPort);
    }

    private List<int> ConnectedClients => networkHelper.connectionIds;

    private void ServerStarted()
    {
        
    }

    private void ServerStopped()
    {
    }

    private void ClientConnected(int id)
    {
        if(!empezado){
            // En cuanto se conecta un cliente, creamos el jugador, le decimos los que ya hay y a qué equipo va
            CrearJugador(id,"Jugador"+id);
            PonerEquipo(id);
            EnviarJugadores(id);

        }
    }

    private void ClientDisconnected(int arg0)
    {
    }

    private void ReceiveMessage(string message, int from)
    {
        // Separamos el mensaje entero por si hay mas mensajes dentro separados por ;
        string[] mensajes = message.Split(';');

        foreach (var mensaje in mensajes)
        {
            string[] args = mensaje.Split("_"); //args[0] tiene el tipo de mensaje, args[1] contenido
            string[] args2 = args[1].Split(",");
            switch(args[0]){
                case "Input":
                    LlegaInput(from,args2);
                    break;

                case "Nom": // Avisa del equipo en el que estamos
                    CambiarNombre(from,args[1]);
                    break;

                case "Prep":
                    Preparado(from);
                    break;

                case "Pers":
                    PonerPersonaje(from,int.Parse(args[1]));
                    break;

                default:
                    break;
            }
        }


        // Example: Print message on chat
        // GameObject.FindWithTag("Chat").GetComponent<ChatController>().AddChatToChatOutput(from + " -> " + message);

        // Example: relay all messages
        // SendToAllExcept(from + " -> " + message, from);
    }

    public void SendToClient(int id, string message)
    {
        networkHelper.SendToOne(id, message);
    }

    public void SendToAllExcept(string message, int exceptId)
    {
        networkHelper.SendToAllExcept(message, exceptId);
    }

    public void SendToAll(string message)
    {
        networkHelper.SendToAll(message);
    }

    // FUNCIONES JUEGO

    // Crea un jugador y notifica al resto
    void CrearJugador(int id,string nombre){
        Jugador j = new Jugador();
        j.id = id;
        j.nombre = nombre;
        Jugadores.Add(id,j);
        nJugadores++;
    }

    // Envia todos los jugadores actuales al id
    void EnviarJugadores(int id){
        foreach(Jugador j in Jugadores.Values){
            if(j.id!=id) SendToClient(id,"Jug_"+JsonUtility.ToJson(j));
        }
    }

    // Cambia el nombre del jugador id y avisa al resto
    void CambiarNombre(int id,string nombre){
        if(nombre!=""){
            Jugadores[id].nombre = nombre;

            // Avisamos a todos del nombre
            SendToAllExcept("Nom_"+id+","+nombre,id);
        }
    }

    // Decide en qué equipo va id y notifica al jugador y a su compañero
    void PonerEquipo(int id){
        int num = -1;
        if(nAzul<=nRojo){
            if(nAzul==1){
                int idCompi = EquipoAzul[0];
                SendToClient(idCompi,"Comp_"+id);
                SendToClient(id,"Comp_"+idCompi);
            }
            EquipoAzul.Add(id);
            nAzul++;
            num = 0;
        }else{
            if(nRojo==1){
                int idCompi = EquipoRojo[0];
                SendToClient(idCompi,"Comp_"+id);
                SendToClient(id,"Comp_"+idCompi);
            }
            EquipoRojo.Add(id);
            nRojo++;
            num = 1;
        }

        // Avisamos al jugador de su equipo
        Jugadores[id].equipo = num;
        SendToClient(id,"Eq_"+num);
        // Avisamos a todos del nuevo jugador (excepto a este)
        SendToAllExcept("Jug_"+JsonUtility.ToJson(Jugadores[id]),id);
    }

    // Avisa a todos de que id está preparado y mira si hay que empezar partida
    void Preparado(int id){
        nPreparados++;
        SendToAllExcept("Prep_"+id,id);
        if(nPreparados==nJugadores){
            EmpezarSeleccion();
        }
    }

    // Empieza la fase de seleccion de personajes
    void EmpezarSeleccion(){
        empezado = true;
        // Avisamos a todos que empieza la fase de seleccion, pero solo el primero de cada equipo puede escoger
        SendToAll("Pers_");
        SendToClient(1,"Esc_-1");
        SendToClient(2,"Esc_-1");
    }

    // Pone el personaje escogido al 
    void PonerPersonaje(int id, int pers){
        Jugadores[id].personaje = pers;
        if(id==1){
            SendToClient(3,"Esc_"+pers);
        }else if(id==2){
            SendToClient(4,"Esc_"+pers);
        }
        nEscogidos++;
        if(nEscogidos==nJugadores){
            SendToAll("Juego_");
            JuegoEmpezado = true;
            SceneManager.LoadScene("MapaServer");
        }
    }

    

    public void EnviarTile(Vector3 pos, string tile, string tilemap){
        SendToAll("Tile_"+Utilidades.FormatVector(pos)+","+tile+","+tilemap);
    }

    public void EnviarOK(){
        SendToAll("OK_");
    }

    void LlegaInput(int id,string[] args){
        TipoInput = args[0];
        InputVec3 = Utilidades.FormatString(args[1]);
        idInput = id;
        LlegaInputEvent.Invoke();
    }

}
