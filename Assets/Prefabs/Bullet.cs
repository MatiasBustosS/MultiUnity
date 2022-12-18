using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    private Rigidbody2D rb;
    [SerializeField] private float Speed = 5;

    [HideInInspector] public Vector2 direction;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
            rb.velocity = new Vector2(direction.x * Speed * Time.deltaTime,direction.y * Speed * Time.deltaTime);
            
            Debug.Log(direction);
    }
}
