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
            PlayerControllerServer p =collision.GetComponent<PlayerControllerServer>();
            if (p && p.myTeam != Player.GetComponent<PlayerControllerServer>().myTeam)
            {
                p.Damage(_Damage);
                StartCoroutine(p.TrapEffect(gameObject));
            }
        }
    }
    
    IEnumerator destroyTrap()
    {
        yield return new WaitForSeconds(4f);

        Destroy(gameObject);
    }
}
