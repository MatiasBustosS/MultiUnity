using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float Speed = 5;

    [HideInInspector] public Vector2 direction;
    [HideInInspector] public GameObject Player;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        StartCoroutine(destroyBullet());
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(direction.x * Speed * Time.deltaTime,direction.y * Speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject != Player)
        {
            Debug.Log("choque");
            Destroy(gameObject);
        }
        Debug.Log("choque");
    }

    IEnumerator destroyBullet()
    {
        yield return new WaitForSeconds(4f);

        Destroy(gameObject);
    }
}
