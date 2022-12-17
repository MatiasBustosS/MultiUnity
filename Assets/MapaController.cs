using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapaController : MonoBehaviour
{
    public Tilemap tilemap;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(tilemap.GetTile(new Vector3Int(0,0,0))); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
