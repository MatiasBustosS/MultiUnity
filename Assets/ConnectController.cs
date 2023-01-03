using UnityEngine;
using UnityEngine.UI;

public class ConnectController : MonoBehaviour
{
    public int LocalPort;

    public string RemoteIP;
    public string NombreJugador = "";
    public int RemotePort;

    public Selectable[] disableOnStart;

    public void LocalPortChanged(string port)
    {
        if (int.TryParse(port, out var portNum))
            LocalPort = portNum;
    }

    public void RemotePortChanged(string port)
    {
        if (int.TryParse(port, out var portNum))
            RemotePort = portNum;
    }

    public void ServerChanged(string server)
    {
        RemoteIP = server;
    }

    public void NombreChanged(string nombre){
        NombreJugador = nombre;
    }

    public void StartServer()
    {
        if (FindObjectOfType<ServerHandler>().StartServer(LocalPort))
        {
            foreach (var obj in disableOnStart) obj.interactable = false;
        }
    }

    public void StartClient()
    {
        if (FindObjectOfType<ClientHandler>().StartClient(LocalPort, RemoteIP, RemotePort, NombreJugador))
        {
            foreach (var obj in disableOnStart) obj.interactable = false;
        }
    }
}
