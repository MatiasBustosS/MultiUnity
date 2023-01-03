using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Jugador{
    public int id;
    public string nombre;
    public int equipo;
}
public class Mensaje{
    public string tipo;
    
}

public class ServerHandler : MonoBehaviour
{
    public bool test = false;
    private NetworkHelper networkHelper;

    List<int> EquipoAzul = new List<int>();
    int nAzul = 0;
    List<int> EquipoRojo = new List<int>();
    int nRojo = 0;

    // Probablemente se pueda usar una lista, pero como solo van a haber 4 tampoco pasa nada
    Dictionary<int,Jugador> Jugadores = new Dictionary<int, Jugador>();

    private void Start()
    {
        DontDestroyOnLoad(this);
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
        if (!test) SceneManager.LoadScene("Server");
    }

    private void ServerStopped()
    {
    }

    private void ClientConnected(int id)
    {
        // En cuanto se conecta un cliente, creamos el jugador y le decimos a qué equipo va
        CrearJugador(id,"Jugador"+id);
        PonerEquipo(id);
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
            switch(args[0]){
                case "Nom": // Avisa del equipo en el que estamos
                    CambiarNombre(from,args[1]);
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

    void CrearJugador(int id,string nombre){
        Jugador j = new Jugador();
        j.id = id;
        j.nombre = nombre;
        Jugadores.Add(id,j);

        // Avisamos a todos del nuevo jugador (excepto a este)
        SendToAllExcept("Jug_"+JsonUtility.ToJson(j),id);
    }

    void CambiarNombre(int id,string nombre){
        if(nombre!=""){
            Jugadores[id].nombre = nombre;

            // Avisamos a todos del nombre
            SendToAllExcept("Nom_"+id+","+nombre,id);
        }
    }

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
    }

}
