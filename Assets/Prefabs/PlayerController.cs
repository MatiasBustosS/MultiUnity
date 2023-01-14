using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;

#endif

public class PlayerController : MonoBehaviour
{
    [HideInInspector] public int playerID;
    public enum Team
    {
        team_1,
        team_2
    }
    public Team myTeam;

    [SerializeField] private bool canMove = true;

    [HideInInspector] public Vector2 gotoPosition;
    [SerializeField] private float speed = 5;
    [SerializeField] private float life = 5;
    [HideInInspector] public bool isAlive;
    public float bulletDamage;
    private float originalDamage;
    [SerializeField] private LayerMask obstacles;

    [HideInInspector] public bool Moving = false;
    [SerializeField] private Vector2 offsetPosition;
    [SerializeField] private float DistanceToMove = 1.5825f;
    [SerializeField] private float circleRadius = 0.65f;

    private Vector2 input;
    public enum LookAt
    {
        Up,
        Down,
        Left,
        Right
    }

    public LookAt lookAt;

    private Animator _animator;


    private bool CanShot =true;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject trap;
    [SerializeField] private float trapDamage;
    
    
    [Header("Character")] 
    
    [SerializeField] private bool UltiCharge = false;
    private bool UseUlti = false;
    public enum ClassType
    {
        Healer,
        Support,
        Damage,
        Tank
    }

    public ClassType myClass;

    private float heal;
    private float boostTime;
    private int objectsToCreate;
    private float MaxDamage;
    
    
    // VALORES PROPIOS DE LOS ROLES DE CADA PERSONAJES
    #region ChangeInspector

#if UNITY_EDITOR
    
    [CustomEditor(typeof(PlayerController))]
    public class ChangeInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PlayerController controller = (PlayerController)target;

            EditorGUILayout.BeginHorizontal();
            InspectorValues(controller);
            EditorGUILayout.EndHorizontal();

        }

        static void InspectorValues(PlayerController controller)
        {
            switch (controller.myClass)
            {
                case ClassType.Healer:
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("Heal", GUILayout.Width(130));
                    controller.heal = EditorGUILayout.FloatField(controller.heal);
                    EditorGUI.indentLevel++;
                    
                    break;
                case ClassType.Support:
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("ObjectsToCreate", GUILayout.Width(130));
                    controller.objectsToCreate = EditorGUILayout.IntField(controller.objectsToCreate);
                    EditorGUI.indentLevel++;
                    
                    break;
                case ClassType.Damage :
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("MaxDamage", GUILayout.Width(130));
                    controller.MaxDamage = EditorGUILayout.FloatField(controller.MaxDamage);
                    EditorGUI.indentLevel++;
                    
                    break;
                case ClassType.Tank:
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("TimeToBoost", GUILayout.Width(130));
                    controller.boostTime = EditorGUILayout.FloatField(controller.boostTime);
                    EditorGUI.indentLevel++;
                    
                    break;
            }
        }
    }

#endif

    #endregion

    void Start()
    {
        gotoPosition = transform.position;
        _animator = GetComponent<Animator>();
        originalDamage = bulletDamage;
        
        switch (myClass)
        {
            // SETEAR SPRITES Y ANIMACIONES DEPENDIENDO EL ROL
            case ClassType.Healer:
                _animator.SetInteger("Character",1);
                break;
            case ClassType.Damage:
                _animator.SetInteger("Character",2);
                break;
            case ClassType.Tank:
                _animator.SetInteger("Character",3);
                break;
            case ClassType.Support:
                _animator.SetInteger("Character",4);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        if (input.y == 0)
        {
            input.x = Input.GetAxisRaw("Horizontal") * DistanceToMove;
            
            if (!Moving)
            {
                _animator.SetFloat("Horizontal", input.x);
                _animator.SetFloat("Vertical", 0);
                
            }
            
            if (input.x < 0)
            {
                lookAt = LookAt.Left;
                _animator.SetFloat("LookAt", 1);

            }

            if (input.x > 0)
            {
                lookAt = LookAt.Right;
                _animator.SetFloat("LookAt", 2);

            }

        }

        if (input.x == 0)
        {
            input.y = Input.GetAxisRaw("Vertical") * DistanceToMove;

            if (!Moving)
            {
                _animator.SetFloat("Vertical", input.y);
                _animator.SetFloat("Horizontal", 0);
            }
            

            if (input.y < 0)
            {
                lookAt = LookAt.Down;
                _animator.SetFloat("LookAt", 0);

            }
            if (input.y > 0)
            {
                lookAt = LookAt.Up;
                _animator.SetFloat("LookAt", 3);

            }
        }


        if (canMove)
        {

            if (Moving)
            {
                transform.position = Vector3.MoveTowards(transform.position,
                    new Vector3(gotoPosition.x, gotoPosition.y, transform.position.z), speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, gotoPosition) == 0)
                {
                    Moving = false;
                    _animator.SetBool("Move", false);
                }
            }

            if ((input.x != 0 || input.y != 0) && !Moving)
            {
                Vector2 puntoEvaluar = new Vector2(transform.position.x, transform.position.y) + offsetPosition + input;

                _animator.SetFloat("Horizontal", input.x);
                _animator.SetFloat("Vertical", input.y);

                if (!Physics2D.OverlapCircle(puntoEvaluar, circleRadius, obstacles))
                {
                    _animator.SetBool("Move", true);
                    Moving = true;
                    gotoPosition += input;
                }
            }

            if (CanShot)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    CanShot = false;
                    var bulletAux = Instantiate(bullet, transform.position, Quaternion.identity);
                    bulletAux.GetComponent<Bullet>().Player = gameObject;
                    bulletAux.GetComponent<Bullet>()._Damage = bulletDamage;

                    switch (lookAt)
                    {
                        case LookAt.Down:
                            bulletAux.GetComponent<Bullet>().direction = new Vector2(0, -1);
                            break;
                        case LookAt.Up:
                            bulletAux.GetComponent<Bullet>().direction = new Vector2(0, 1);
                            break;
                        case LookAt.Right:
                            bulletAux.GetComponent<Bullet>().direction = new Vector2(1, 0);
                            break;
                        case LookAt.Left:
                            bulletAux.GetComponent<Bullet>().direction = new Vector2(-1, 0);
                            break;

                    }

                    bulletDamage = originalDamage;

                    StartCoroutine(BulletTime());
                }
            }
        }

        if (UltiCharge)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                UseUlti = true;
                UltiAttack();
                UltiCharge = false;
            }
        }
    }

    IEnumerator BulletTime()
    {
        yield return new WaitForSeconds(1f);

        CanShot = true;

    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void UltiAttack()
    {
        
        switch (myClass)
        {
            case ClassType.Healer:
                HealFunction();
                break;
            
            case ClassType.Support:
                PutUltiTrap();
                break;
            
            case ClassType.Tank:
                StartCoroutine(UltiTank());
                break;
            
            case ClassType.Damage:
                bulletDamage = MaxDamage;
                break;
        }
        
    }

    public void Damage(float _damage)
    {
        if (myClass == ClassType.Tank && UseUlti)
        {
            UseUlti = false;
        }
        else
        {
            life = Mathf.Clamp(life - _damage, 0, 5);
        }
    }

    private void HealFunction()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
                
        foreach (var otherplayer in players)
        {
            if (otherplayer.GetComponent<PlayerController>().myTeam == myTeam)
            {
                otherplayer.GetComponent<PlayerController>().life = Mathf.Clamp(0, 5,life+1);;
            }
        }
    }

    private void PutUltiTrap()
    {
        Vector2 look = input; 
        
        switch (lookAt)
        {
            case LookAt.Down:
                look = new Vector2(0, -1);
                break;
            case LookAt.Up:
                look = new Vector2(0, 1);
                break;
            case LookAt.Right:
                look = new Vector2(1, 0);
                break;
            case LookAt.Left:
                look = new Vector2(-1, 0);
                break;
        }
        
        Debug.Log("asdjhbasjd   ");
        
        Vector2 putTrap = new Vector2(transform.position.x, transform.position.y) + offsetPosition + look;

        if (!Physics2D.OverlapCircle(putTrap, circleRadius, obstacles))
        {
            Debug.Log("Trampa Colocada");
            var _trap = Instantiate(trap, putTrap, Quaternion.identity);
            _trap.GetComponent<TrapController>()._Damage = trapDamage;
        }
    }

    public IEnumerator TrapEffect()
    {
        canMove = false;
        
        yield return new WaitForSeconds(2f);

        canMove = true;
    }

    private IEnumerator UltiTank()
    {
        yield return new WaitForSeconds(boostTime);
        UseUlti = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gotoPosition+offsetPosition, circleRadius);
    }
}
