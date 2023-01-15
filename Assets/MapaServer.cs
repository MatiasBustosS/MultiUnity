using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapaServer : MonoBehaviour
{
    private ServerHandler sh = null;
    public Tilemap Obstaculos;
    public Tilemap Objetos;
    public Tilemap Fondo;
    public Tile Caja;
    public Tile Obstaculo;
    public Tile Municion;
    public Tile Bandera;

    Vector3Int esquinaSupIzq = new Vector3Int(-7,6,0);
    Vector3Int esquinaInfDer = new Vector3Int(6,-7,0);

    public int maxCajas = 20;
    public int minCajas = 4;
    int nCajas = 0;

    bool BanderaSpawneada = false;
    public bool BanderaAgarrada = false;
    Vector3 posBandera = new Vector3Int(-1000,-1000,-1000);

    int puntosAzul = 0;
    int puntosRojo = 0;

    public PlayerControllerServer[] players;


    /**                  FUNCIONES TILES                    **/
    // Calcula el vector resultante de poner pos en el SR de la esquina superior izquierda
    // Asume que la esquina sup izq es el primer tile muro de Muros
    private Vector3Int CalcPos(Vector3 pos){
        // Vector3Int esquina = new Vector3Int(Obstaculos.origin.x,Obstaculos.origin.y+Obstaculos.size.y-1,0);
        // return new Vector3Int(esquina.x+pos.x,esquina.y-pos.y,0);
        return Obstaculos.WorldToCell(pos);
    }

    public void PonerTile(Vector3 pos, Tile tile, Tilemap tilemap){
        tilemap.SetTile(CalcPos(pos),tile);
        if(tile)
            sh.EnviarTile(pos,tile.name,tilemap.name);
        else
            sh.EnviarTile(pos,"",tilemap.name);

    }

    public void EliminarTile(Vector3 pos, Tilemap tilemap){
        PonerTile(pos,null,tilemap);
    }

    public TileBase ObtTile(Vector3 pos,Tilemap tilemap){
        return tilemap.GetTile(CalcPos(pos));
    }

    /** --------------------------------------------------- **/

    public void SpawnearMunicion(Vector3 pos){
        PonerTile(pos,Municion,Objetos);
    }

    public void SpawnearBandera(Vector3 pos){
        PonerTile(pos,Bandera,Objetos);
        BanderaSpawneada = true;
        posBandera = pos;
    }

    public bool EsUlti(Vector3 pos){
        return ObtTile(pos,Objetos)==Municion;
    }

    public bool EsBandera(Vector3 pos){
        return ObtTile(pos,Objetos)==Bandera;
    }

    public void DestruirCaja(Vector3 pos){
        if(ObtTile(pos,Obstaculos)==Caja){
            EliminarTile(pos,Obstaculos);

            // Decide si poner municion o una bandera y spawnea
            if(Random.Range(0f,1f)>0.75f){
                SpawnearMunicion(pos);
            }else if(!BanderaSpawneada && Random.Range(0f,1f)>0.85f){
                SpawnearBandera(pos);
            }

            nCajas--;
        }
    }

    // Spawnea n cajas en el mapa
    public void SpawnearCajas(int n){
        //Vector3Int esquina = new Vector3Int(-1,2,0);
        for(int i=0;i<n;i++){

            int x = Random.Range(esquinaSupIzq.x,esquinaInfDer.x+1);
            int y = Random.Range(esquinaInfDer.y,esquinaSupIzq.y+1);
            
            // No pongamos cajas donde ya hayan cosas
            // FALTA MIRAR QUE NO SE PONGAN ENCIMA DE UN PERSONAJE
            while(ObtTile(new Vector3Int(x,y,0),Obstaculos)!=null){
                x = Random.Range(esquinaSupIzq.x,esquinaInfDer.x+1);
                y = Random.Range(esquinaInfDer.y,esquinaSupIzq.y+1);
            }

            PonerTile(new Vector3Int(x,y,0),Caja,Obstaculos);
        }
        nCajas += n;
    }

    void ComprobarVictoria(){
        if(BanderaSpawneada && !BanderaAgarrada){
            TileBase t = ObtTile(posBandera,Fondo);
            if(t.name=="Azul"){
                puntosAzul++;
                Debug.Log("A");
                EliminarTile(posBandera,Objetos);
                BanderaSpawneada = false;
                BanderaAgarrada = false;

                if(puntosAzul==3) sh.Victoria(0);

                SpawnearCajas(maxCajas-nCajas);
            }else if(t.name=="Rojo"){
                puntosRojo++;

                EliminarTile(posBandera,Objetos);
                BanderaSpawneada = false;
                BanderaAgarrada = false;

                if(puntosRojo==3) sh.Victoria(1);

                SpawnearCajas(maxCajas-nCajas);
            }  
        }
    }    

    void EnviarOK(){
        sh.EnviarOK();
    }

    void LlegaInput(){
        switch(sh.TipoInput){
            case "Moverse":
                players[sh.idInput-1].Moverse(sh.InputVec3);
                break;

            case "Atacar":
                players[sh.idInput-1].Atacar();
                break;

            case "Ulti":
                players[sh.idInput-1].UsarUlti();
                break;

            case "Coger":
                players[sh.idInput-1].Coger();
                break;
        }
        
    }

    void EnviarPersonajes(){
        int i = 0;
        foreach(Jugador j in Utilidades.Jugadores.Values){
            sh.EnviarPersonaje(i,j.personaje);
            i++;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        int i = 1;
        foreach(PlayerControllerServer p in players){
            if(i>Utilidades.Jugadores.Count) break;
            p.mapa = this;
            Debug.Log(Utilidades.Jugadores[i].nombre);
            p.CambiarPersonaje((PlayerControllerServer.ClassType)Utilidades.Jugadores[i].personaje);
            p.playerID = i;
            i++;
        }
        if(Utilidades.Jugadores.Count==3) players[2].gameObject.SetActive(true);
        else if(Utilidades.Jugadores.Count==4){
            players[2].gameObject.SetActive(true);
            players[3].gameObject.SetActive(true);
        }
        var g = GameObject.FindWithTag("Handler");
        sh = g.GetComponent<ServerHandler>();
        sh.LlegaInputEvent.AddListener(LlegaInput);
        Random.InitState(System.DateTime.Now.Millisecond);
        SpawnearCajas(maxCajas);
        EnviarPersonajes();
        EnviarOK();
    }

    // Update is called once per frame
    void Update()
    {
        // Respawnear cajas
        if(nCajas==minCajas) SpawnearCajas(minCajas*2);

        ComprobarVictoria();

        // DEBUG
        if(Input.GetMouseButtonDown(0)){
            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Debug.Log(CalcPos(worldPoint));
            var tile = ObtTile(worldPoint,Obstaculos);

            if(tile)
            {
                DestruirCaja(worldPoint);
            }
        }
    }
}
