using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class SeleccionPersonaje : MonoBehaviour
{
    private ClientHandler ch;

    public TextMeshProUGUI NombreJugador;
    public TextMeshProUGUI NombreCompi;
    public TextMeshProUGUI Equipo;
    public SpriteRenderer Fondo;

    public TextMeshProUGUI NombreJugadorRival;
    public TextMeshProUGUI NombreCompiRival;
    public TextMeshProUGUI EquipoRival;
    public SpriteRenderer FondoRival;

    public Button BotonPreparado;
    public GameObject BotonesPersonajes;
    Button[] botones;

    int seleccionado = -1;
    bool rivalPuesto = false;
    bool rivalNombre = false;

    private void Awake()
    {
        var g = GameObject.FindWithTag("Handler");
        ch = g.GetComponent<ClientHandler>();
    }

    // --- FUNCIONES PRE-JUEGO ---
    // void CambiarCompi(){
    //     NombreCompi.text = ch.NombreCompi();
    // }

    void CambiarNombres(){
        Jugador j = ch.Jugadores[ch.nombreCamb];
        if(j.equipo!=ch.equipo){
            if(!rivalNombre){
                NombreJugadorRival.text = j.nombre;
                rivalNombre = true;
            }else{
                NombreCompiRival.text = j.nombre;
            }
        }else{
            if(!ch.eresCompi){
                NombreCompi.text = j.nombre;
            }
        }
    }

    void JugadorNuevo(){
        Jugador j = ch.UltimoJugador();
        if(j.equipo!=ch.equipo){
            if(!rivalPuesto){
                NombreJugadorRival.text = j.nombre;
                BotonPreparado.interactable = true;
                rivalPuesto = true;
                if(ch.equipo==1 || (ch.equipo==0 && ch.eresCompi)) rivalNombre = true;
            }else{
                NombreCompiRival.text = j.nombre;
            }
        }else{
            if(!ch.eresCompi){
                NombreCompi.text = j.nombre;
            }else{
                NombreJugador.text = j.nombre;
            }
        }
    }

    public void Preparado(){
        BotonPreparado.interactable = false;
        // NombreJugador.color = Color.green;
        ch.Preparado();
    }

    void OtroPreparado(){
        // HAY QUE CAMBIARLO
        // if(ch.prep==2){
        //     NombreJugadorRival.color = Color.green;
        // }else if(ch.prep==3){
        //     NombreCompi.color = Color.green;
        // }else{
        //     NombreCompiRival.color = Color.green;
        // }
    }

    void PonerNombres(){
        foreach(Jugador j in ch.Jugadores.Values){
            if(j.equipo!=ch.equipo){
                if(!rivalPuesto){
                    NombreJugadorRival.text = j.nombre;
                    BotonPreparado.interactable = true;
                    rivalPuesto = true;
                    rivalNombre = true;
                }else{
                    NombreCompiRival.text = j.nombre;
                }
            }else{
                NombreJugador.text = j.nombre;
            }
        }
    }

    // Empieza la fase de seleccion, pero dependiendo del jugador puede escoger o no
    void EmpezarSeleccion(){
        Destroy(BotonPreparado.gameObject);
        BotonesPersonajes.SetActive(true);
    }

    void SePuedeEscoger(){
        int i=0;
        foreach(Button b in botones){
            if(ch.persCompi!=i) b.interactable = true;
            i++;
        }
    }

    public void SeleccionarPersonaje(int idPersonaje){
        SePuedeEscoger();
        seleccionado = idPersonaje;
        botones[idPersonaje].interactable = false;
    }

    public void BloquearPersonaje(){
        if(seleccionado!=-1){
            Destroy(BotonesPersonajes);
            ch.Escoger(seleccionado);

        }
    }
    // ----------------------------------------

    // Start is called before the first frame update
    void Start()
    {
        botones = BotonesPersonajes.GetComponentsInChildren<Button>();
        BotonPreparado.interactable = false;

        // Eventos
        ch.JugadorNuevo.AddListener(JugadorNuevo);
        ch.JugadorPreparado.AddListener(OtroPreparado);
        ch.FaseSeleccion.AddListener(EmpezarSeleccion);
        ch.PuedoEscoger.AddListener(SePuedeEscoger);
        ch.NombreCambiado.AddListener(CambiarNombres);

        // Miramos el orden, igual eres el que ha llegado más tarde y se te tiene que poner debajo
        if(ch.compi==-1){
            NombreJugador.text = ch.nombre;
            // ch.CompiCambiado.AddListener(CambiarCompi);
        }else{
            NombreCompi.text = ch.nombre;
            ch.eresCompi = true;
        }

        // Cambiamos la estética segun el equipo
        if(ch.equipo==1){
            Equipo.text = "Equipo rojo";
            EquipoRival.text = "Equipo azul";
            Color aux = Fondo.color;
            Fondo.color = FondoRival.color;
            FondoRival.color = aux;
        }

        // Para los que llegan tarde
        PonerNombres();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
