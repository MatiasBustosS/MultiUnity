using System.Collections;
using System.Collections.Generic;
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
                    EditorGUILayout.LabelField("Heal", GUILayout.Width(150));
                    float Heal = controller.heal;
                    EditorGUI.indentLevel++;
                    
                    break;
                case ClassType.Support:
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("ObjectsToCreate", GUILayout.Width(150));
                    int ObjectsToCreate = controller.objectsToCreate;
                    EditorGUI.indentLevel++;
                    
                    break;
                case ClassType.Damage :
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("MaxDamage", GUILayout.Width(150));
                    float MaxDamage = controller.MaxDamage;
                    EditorGUI.indentLevel++;
                    
                    break;
                case ClassType.Tank:
                    
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField("TimeToBoost", GUILayout.Width(150));
                    float TimeToBoost = controller.boostTime;
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
    }

    // Update is called once per frame
    void Update()
    {
        if (input.y == 0)
        {
            input.x = Input.GetAxisRaw("Horizontal") * DistanceToMove;
        }

        if (input.x == 0)
        {
            input.y = Input.GetAxisRaw("Vertical") * DistanceToMove;
        }
        

        if (Moving)
        {
            transform.position = Vector2.MoveTowards(transform.position, gotoPosition, speed * Time.deltaTime);

            if (Vector2.Distance(transform.position, gotoPosition) == 0)
            {
                Moving = false;
            }
        }

        if ((input.x != 0 || input.y != 0) && !Moving)
        {
            Vector2 puntoEvaluar = new Vector2(transform.position.x, transform.position.y) + offsetPosition + input;

            if (!Physics2D.OverlapCircle(puntoEvaluar, circleRadius, obstacles))
            {
                Moving = true;
                gotoPosition += input;
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
