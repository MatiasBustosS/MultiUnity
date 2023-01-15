using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float Speed = 5;

    [HideInInspector] public float _Damage;
    [HideInInspector] public Vector2 direction;
    [HideInInspector] public GameObject Player;
    [HideInInspector] public MapaServer Mapa = null;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(destroyBullet());    //TEMPORIZADOR PARA DESTRUIR EL OBJETO
    }

    // Update is called once per frame
    void Update()
    {
        // MOVIEMIENTO
        rb.velocity = new Vector2(direction.x * Speed * Time.deltaTime,direction.y * Speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject == Player) return;
        if (collision.gameObject.CompareTag("Player") && collision.gameObject != Player)
        {
            PlayerControllerServer p = collision.GetComponent<PlayerControllerServer>();
            if(p) p.Damage(_Damage);
        }
        else if(collision.gameObject.CompareTag("Obstaculo")){
            // Debug.Log(transform.position+direction);
            if(Mapa) Mapa.DestruirCaja(transform.position+new Vector3(direction.x,direction.y,0));
        }
        Destroy(gameObject);
    }

    IEnumerator destroyBullet()
    {
        yield return new WaitForSeconds(4f);

        Destroy(gameObject);
    }
}
