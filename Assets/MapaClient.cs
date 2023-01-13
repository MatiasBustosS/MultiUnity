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

    Vector3Int esquinaSupIzq = new Vector3Int(-7,6,0);
    Vector3Int esquinaInfDer = new Vector3Int(6,-7,0);

    // public int maxCajas = 20;
    // public int minCajas = 4;
    // int nCajas = 0;

    // bool BanderaSpawneada = false;
    // bool BanderaAgarrada = false;
    // Vector3 posBandera = new Vector3Int(-1000,-1000,-1000);

    // int puntosAzul = 0;
    // int puntosRojo = 0;


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
        Debug.Log(ch.tileRecibido);
        Debug.Log(tiles.Values);
        PonerTile(ch.tilePos,tiles[ch.tileRecibido],Objetos);
    }  

    void EnviarInput(){

    }

    void Awake()
    {
        var g = GameObject.FindWithTag("Handler");
        ch = g.GetComponent<ClientHandler>();
        ch.RecibirTile.AddListener(RecibirTile);
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
        // DEBUG
        if(Input.GetMouseButtonDown(0)){
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(CalcPos(worldPoint));
            var tile = ObtTile(worldPoint,Objetos);

            if(tile)
            {
                
            }
        }
    }
}
