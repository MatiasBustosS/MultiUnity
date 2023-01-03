using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ClientHandler : MonoBehaviour
{
    public bool test = false;
    private NetworkHelper networkHelper;

    // int idJuego = -1;
    public string nombre = "";
    public int equipo = -1; //0 -> azul, 1 -> rojo, -1 -> sin equipo

    Dictionary<int,Jugador> Jugadores = new Dictionary<int, Jugador>();
    public int compi = -1; // ID del compi

    public UnityEvent CompiCambiado;

    private void Start()
    {
        DontDestroyOnLoad(this);
        CompiCambiado = new UnityEvent();
    }

    public bool StartClient(int localPort, string remoteIP, int remotePort, string nombreJug)
    {
        networkHelper = FindObjectOfType<NetworkHelper>();
        networkHelper.onConnect.AddListener(ConnectedToServer);
        networkHelper.onDisconnect.AddListener(DisconnectedFromServer);
        networkHelper.onMessageReceived.AddListener(ReceiveMessage);

        nombre = nombreJug;

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
            string[] args = mensaje.Split("_"); //args[0] tiene el tipo de mensaje, args[1] contenido
            switch(args[0]){
                case "Eq": // Avisa del equipo en el que estamos
                    PonerseEquipo(int.Parse(args[1])); // Nos ponemos en el equipo que toca
                    break;

                case "Comp": // Nos dice quién es nuestro compañero id,nombre
                    CambiarCompi(int.Parse(args[1]));
                    break;

                case "Jug": // Nos dice los datos del nuevo jugador
                    NuevoJugador(args[1]);
                    break;

                case "Nom": // Nos dice el nombre de un jugador
                    string[] args2 = args[1].Split(",");
                    CambiarNombre(int.Parse(args2[0]),args2[1]);
                    break;

                default:
                    break;
            }
            
        }

        // Example: Print message on chat
        // GameObject.FindWithTag("Chat").GetComponent<ChatController>().AddChatToChatOutput(message);
    }

    public void SendToServer(string message)
    {
        networkHelper.SendToServer(message);
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
        Jugadores.Add(j.id,j);
    }

    void CambiarNombre(int idJug, string nombreJug){
        Jugadores[idJug].nombre = nombreJug;
        if(idJug==compi) CompiCambiado.Invoke();
    }

    public string NombreCompi(){
        return Jugadores[compi].nombre;
    }
}
