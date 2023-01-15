using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapaClient : MonoBehaviour
{
    private ClientHandler ch = null;
    public Tilemap Obstaculos;
    public Tilemap Objetos;
    // public Tilemap Fondo;
    public Tile[] tilesArray;
    Dictionary<string,Tile> tiles;
    public GameObject Cargando;

    bool Moviendose = false;
    public bool BanderaAgarrada = false;
    bool BanderaSpawneada = false;
    Vector3 posBandera;

    public PlayerControllerClient[] players;



    /**                  FUNCIONES TILES                    **/
    // Calcula el vector resultante de poner pos en el SR de la esquina superior izquierda
    // Asume que la esquina sup izq es el primer tile muro de Muros
    private Vector3Int CalcPos(Vector3 pos){
        // Vector3Int esquina = new Vector3Int(Objetos.origin.x,Objetos.origin.y+Objetos.size.y-1,0);
        // return new Vector3Int(esquina.x+pos.x,esquina.y-pos.y,0);
        return Obstaculos.WorldToCell(pos);
    }

    public void PonerTile(Vector3 pos, Tile tile, Tilemap tilemap){
        tilemap.SetTile(CalcPos(pos),tile);
        
    }

    public void EliminarTile(Vector3 pos, Tilemap tilemap){
        PonerTile(pos,null,tilemap);
    }

    public TileBase ObtTile(Vector3 pos,Tilemap tilemap){
        return tilemap.GetTile(CalcPos(pos));
    }

    /** --------------------------------------------------- **/
    public void SpawnearBandera(Vector3 pos){
        PonerTile(pos,tiles["Bandera"],Objetos);
        BanderaSpawneada = true;
        posBandera = pos;
    }
    public bool EsBandera(Vector3 pos){
        return ObtTile(pos,Objetos)==tiles["Bandera"];
    }
    void RecibirTile(){
        if(ch.tileRecibido!=""){
            PonerTile(ch.tilePos,tiles[ch.tileRecibido],Obstaculos);
            if(ch.tileRecibido=="Bandera") BanderaSpawneada = true;
        }
        else
            EliminarTile(ch.tilePos,Obstaculos);
    }  

    void MostrarMapa(){
        Cargando.SetActive(false);
    }

    void GestionarInput(){
        if(Input.GetAxisRaw("Horizontal")!=0 && !Moviendose){
            ch.EnviarInput("Moverse",new Vector2(Input.GetAxisRaw("Horizontal"), 0));
            Moviendose = true;
        }else if(Input.GetAxisRaw("Vertical")!=0){
            ch.EnviarInput("Moverse",new Vector2(0, Input.GetAxisRaw("Vertical")));
            Moviendose = true;
        }else if(Moviendose){
            ch.EnviarInput("Moverse",new Vector2(0, 0));
            Moviendose = false;
        }

        // ch.EnviarInput("Moverse", new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")));

        if(Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Q)){
            ch.EnviarInput("Atacar", Vector2.zero);
        }

        if(Input.GetKeyDown(KeyCode.Space)){
            ch.EnviarInput("Ulti", Vector2.zero);
        }

        if(Input.GetKeyDown(KeyCode.E)){
            ch.EnviarInput("Coger", Vector2.zero);
        }
    }

    void LlegaInput(){
        switch(ch.TipoInput){
            case "Moverse":
                players[ch.idInput-1].Moverse(ch.InputVec3);
                break;

            case "Atacar":
                players[ch.idInput-1].Atacar();
                break;

            case "Ulti":
                players[ch.idInput-1].UsarUlti();
                break;

            case "Coger":
                players[ch.idInput-1].Coger();
                break;
        }
    }

    void LlegaPj(){
        players[ch.idPj].CambiarPersonaje(ch.Pj);
    }

    void Awake()
    {
        var g = GameObject.FindWithTag("Handler");
        ch = g.GetComponent<ClientHandler>();

        int i = 1;
        foreach(PlayerControllerClient p in players){
            if(i>Utilidades.Jugadores.Count) break;
            p.mapa = this;
            // p.CambiarPersonaje((PlayerControllerClient.ClassType)Utilidades.Jugadores[i].personaje);
            i++;
        }
        if(Utilidades.Jugadores.Count==3) players[2].gameObject.SetActive(true);
        else if(Utilidades.Jugadores.Count==4){
            players[2].gameObject.SetActive(true);
            players[3].gameObject.SetActive(true);
        }

        ch.RecibirTile.AddListener(RecibirTile);
        ch.MostrarMapaEvent.AddListener(MostrarMapa);
        ch.LlegaInputEvent.AddListener(LlegaInput);
        ch.LlegaPjEvent.AddListener(LlegaPj);

        Random.InitState(System.DateTime.Now.Millisecond);
        tiles = new Dictionary<string, Tile>();
        foreach(Tile t in tilesArray){
            tiles.Add(t.name,t);
        }
        // SpawnearCajas(maxCajas);
    }

    // Update is called once per frame
    void Update()
    {
        GestionarInput();
        
    }
}
