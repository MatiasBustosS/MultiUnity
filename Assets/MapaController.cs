using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapaController : MonoBehaviour
{
    public Tilemap Objetos;
    public Tile Caja;
    public Tile Obstaculo;
    int nCajas = 20;

    // Calcula el vector resultante de poner pos en el SR de la esquina superior izquierda
    // Asume que la esquina sup izq es el primer tile muro de Muros
    private Vector3Int CalcPos(Vector3Int pos){
        Vector3Int esquina = new Vector3Int(Objetos.origin.x,Objetos.origin.y+Objetos.size.y-1,0);
        return new Vector3Int(esquina.x+pos.x,esquina.y-pos.y,0);
    }

    public void PonerTile(Vector3Int pos, Tile tile){
        Objetos.SetTile(CalcPos(pos),tile);
    }

    public void EliminarTile(Vector3Int pos){
        PonerTile(pos,null);
    }

    public TileBase ObtTile(Vector3Int pos){
        return Objetos.GetTile(CalcPos(pos));
    }

    public void SpawnearMunicion(Vector3Int pos){

    }

    public void DestruirCaja(Vector3Int pos){
        if(ObtTile(pos)==Caja){
            EliminarTile(pos);
            if(Random.Range(0f,1f)>0.65f){
                SpawnearMunicion(pos);
            }
        }
    }

    public void SpawnearCajas(){
        //Vector3Int esquina = new Vector3Int(2,1,0);
        for(int i=0;i<nCajas;i++){

            int x = Random.Range(2,Objetos.size.x-2);
            int y = Random.Range(1,Objetos.size.y-1);
            
            // No pongamos 
            while(ObtTile(new Vector3Int(x,y,0))!=null){
                x = Random.Range(2,Objetos.size.x-2);
                y = Random.Range(1,Objetos.size.y-1);
            }

            PonerTile(new Vector3Int(x,y,0),Caja);
        }
    }

    

    // Start is called before the first frame update
    void Start()
    {
        Random.InitState(System.DateTime.Now.Millisecond);
        SpawnearCajas();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
