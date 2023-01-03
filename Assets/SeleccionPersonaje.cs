using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SeleccionPersonaje : MonoBehaviour
{
    private ServerHandler sh = null;
    private ClientHandler ch = null;

    public TextMeshProUGUI NombreJugador;
    public TextMeshProUGUI NombreCompi;

    bool compiCambiado = false;

    private void Awake()
    {
        var g = GameObject.FindWithTag("Handler");
        sh = g.GetComponent<ServerHandler>();
        ch = g.GetComponent<ClientHandler>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Ponemos el color de la camara segun el equipo
        Camera.main.backgroundColor = ch.equipo==0 ? new Color(0,0.67f,1) : new Color(0.93f,0.49f,0.196f);
        NombreJugador.text = ch.nombre;
    }

    // Update is called once per frame
    void Update()
    {
        if(!compiCambiado && ch.compi!=-1){
            NombreCompi.text = ch.NombreCompi();
            compiCambiado = true;
            Debug.Log("A");
        }
    }
}
