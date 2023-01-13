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

    [HideInInspector] public Vector2 gotoPosition;
    [SerializeField] private float speed = 5;
    [SerializeField] private float life = 5;
    [HideInInspector] public bool isAlive;
    public float bulletDamage;
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
    
    [Header("Character")] 
    
    [SerializeField] private bool UltiCharge = false;
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
                    //float Heal = controller.heal;
                    EditorGUI.indentLevel++;
                    
                    break;
                case ClassType.Support:
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("ObjectsToCreate", GUILayout.Width(130));
                    controller.objectsToCreate = EditorGUILayout.IntField(controller.objectsToCreate);
                    //int ObjectsToCreate = controller.objectsToCreate;
                    EditorGUI.indentLevel++;
                    
                    break;
                case ClassType.Damage :
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("MaxDamage", GUILayout.Width(130));
                    controller.MaxDamage = EditorGUILayout.FloatField(controller.MaxDamage);
                    //float MaxDamage = controller.MaxDamage;
                    EditorGUI.indentLevel++;
                    
                    break;
                case ClassType.Tank:
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("TimeToBoost", GUILayout.Width(130));
                    controller.boostTime = EditorGUILayout.FloatField(controller.boostTime);
                    //float TimeToBoost = controller.boostTime;
                    EditorGUI.indentLevel++;
                    
                    break;
            }
        }
    }

#endif

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        gotoPosition = transform.position;
        _animator = GetComponent<Animator>();
        
        switch (myClass)
        {
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
        
        
        
        if (Moving)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(gotoPosition.x, gotoPosition.y, transform.position.z), speed * Time.deltaTime);
            
            
            
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
                var bulletAux = Instantiate(bullet,transform.position, Quaternion.identity);

                bulletAux.GetComponent<Bullet>().Player = gameObject;
                
                switch (lookAt)
                {
                    case LookAt.Down:
                        bulletAux.GetComponent<Bullet>().direction = new Vector2(0,-1);
                        break;
                    case LookAt.Up:
                        bulletAux.GetComponent<Bullet>().direction = new Vector2(0,1);
                        break;
                    case LookAt.Right:
                        bulletAux.GetComponent<Bullet>().direction = new Vector2(1,0);
                        break;
                    case LookAt.Left:
                        bulletAux.GetComponent<Bullet>().direction = new Vector2(-1,0);
                        break;
                    
                }

                StartCoroutine(BulletTime());
            }
        }
        

        if (UltiCharge)
        {
            if (Input.GetKey(KeyCode.Space))
            {
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

    private void UltiAttack()
    {
        switch (myClass)
        {
            case ClassType.Healer:
                var players = GameObject.FindGameObjectsWithTag("Player");

                foreach (var otherplayer in players)
                {
                    if (otherplayer.GetComponent<PlayerController>().myTeam == myTeam)
                    {
                        otherplayer.GetComponent<PlayerController>().HealFunction(heal);
                    }

                    HealFunction(heal);
                }
                break;
            
            case ClassType.Support:
                break;
            
            case ClassType.Tank:
                break;
            
            case ClassType.Damage:
                break;
        }
    }

    public void Damage(float _damage)
    {
        
    }

    public void HealFunction(float _heal)
    {
        
    }
    
    

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gotoPosition+offsetPosition, circleRadius);
    }
}
