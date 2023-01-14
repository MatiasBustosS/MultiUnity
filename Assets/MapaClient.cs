using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapaClient : MonoBehaviour
{
    private ClientHandler ch = null;
    public Tilemap Objetos;
    // public Tilemap Fondo;
    public Tile[] tilesArray;
    Dictionary<string,Tile> tiles;
    public GameObject Cargando;

    bool Moviendose = false;


    /**                  FUNCIONES TILES                    **/
    // Calcula el vector resultante de poner pos en el SR de la esquina superior izquierda
    // Asume que la esquina sup izq es el primer tile muro de Muros
    private Vector3Int CalcPos(Vector3 pos){
        // Vector3Int esquina = new Vector3Int(Objetos.origin.x,Objetos.origin.y+Objetos.size.y-1,0);
        // return new Vector3Int(esquina.x+pos.x,esquina.y-pos.y,0);
        return Objetos.WorldToCell(pos);
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

    void RecibirTile(){
        if(ch.tileRecibido!="")
            PonerTile(ch.tilePos,tiles[ch.tileRecibido],Objetos);
        else
            EliminarTile(ch.tilePos,Objetos);
    }  

    void MostrarMapa(){
        Cargando.SetActive(false);
    }

    void GestionarInput(){
        // DEBUG
        // if(Input.GetMouseButtonDown(0)){
        //     Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     ch.EnviarInput("Clic",worldPoint);
        // }

        if(Input.GetAxisRaw("Horizontal")!=0){
            ch.EnviarInput("Moverse",new Vector2(Input.GetAxisRaw("Horizontal"), 0));
            Moviendose = true;
        }else if(Input.GetAxisRaw("Vertical")!=0){
            ch.EnviarInput("Moverse",new Vector2(0, Input.GetAxisRaw("Vertical")));
            Moviendose = true;
        }else if(Moviendose){
            ch.EnviarInput("Moverse",new Vector2(0, 0));
            Moviendose = false;
        }

        if(Input.GetKeyDown(KeyCode.Mouse0)){
            ch.EnviarInput("Atacar", Vector2.zero);
        }

        if(Input.GetKeyDown(KeyCode.Space)){
            ch.EnviarInput("Ulti", Vector2.zero);
        }
    }

    void Awake()
    {
        var g = GameObject.FindWithTag("Handler");
        ch = g.GetComponent<ClientHandler>();

        ch.RecibirTile.AddListener(RecibirTile);
        ch.MostrarMapaEvent.AddListener(MostrarMapa);

        Random.InitState(System.DateTime.Now.Millisecond);
        tiles = new Dictionary<string, Tile>();
        foreach(Tile t in tilesArray){
            tiles.Add(t.name,t);
            Debug.Log(t.name);
        }
        // SpawnearCajas(maxCajas);
    }

    // Update is called once per frame
    void Update()
    {
        GestionarInput();
        
    }
}
