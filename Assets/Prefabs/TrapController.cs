using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapController : MonoBehaviour
{
    [HideInInspector] public float _Damage;
    [HideInInspector] public GameObject Player;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(destroyTrap());    //TEMPORIZADOR PARA DESTRUIR EL OBJETO
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject != Player)
        {
            if (collision.gameObject.GetComponent<PlayerControllerServer>().myTeam != Player.GetComponent<PlayerControllerServer>().myTeam)
            {
                collision.GetComponent<PlayerControllerServer>().Damage(_Damage);
                StartCoroutine(collision.GetComponent<PlayerControllerServer>().TrapEffect(gameObject));
            }
        }
    }
    
    IEnumerator destroyTrap()
    {
        yield return new WaitForSeconds(4f);

        Destroy(gameObject);
    }
}
