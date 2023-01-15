using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;

#endif

public class PlayerControllerServer : MonoBehaviour
{
    [HideInInspector] public int playerID;
    public Slider lifeBar;
    public TextMeshProUGUI nameTag;

    public String playerName;
    public enum Team
    {
        team_1,
        team_2
    }
    public Team myTeam;

    [SerializeField] private bool canMove = true;

    [SerializeField] private float timeToRespawn = 1;

    [HideInInspector] public Vector2 gotoPosition;
    [SerializeField] private float speed = 5;
    [SerializeField] private float totalLife;
    private float actualLife;
    [HideInInspector] public bool isAlive;
    public float bulletDamage;
    private float originalDamage;
    [SerializeField] private LayerMask obstacles;

    [HideInInspector] public bool Moving = false;
    [SerializeField] private Vector2 offsetPosition;
    [SerializeField] private float DistanceToMove = 1.5825f;
    [SerializeField] private float circleRadius = 0.65f;
    private float axisHorizontal = 0;
    private float axisVertical = 0;

    private Vector2 input;
    public enum LookAt
    {
        Up,
        Down,
        Left,
        Right
    }

    public LookAt lookAt;

    public Animator _animator = null;


    private bool CanShot =true;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject trap;
    private float trapDamage = 1;
    
    
    [Header("Character")] 
    
    [SerializeField] private bool UltiCharge = false;
    private Vector2 initialPos;
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

    public MapaServer mapa;
    public bool tieneBandera = false;
    private ServerHandler sh;
    
    // VALORES PROPIOS DE LOS ROLES DE CADA PERSONAJES
    #region ChangeInspector

#if UNITY_EDITOR
    
    [CustomEditor(typeof(PlayerControllerServer))]
    public class ChangeInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            PlayerControllerServer controller = (PlayerControllerServer)target;

            EditorGUILayout.BeginHorizontal();
            InspectorValues(controller);
            EditorGUILayout.EndHorizontal();

        }

        static void InspectorValues(PlayerControllerServer controller)
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
        var g = GameObject.FindWithTag("Handler");
        sh = g.GetComponent<ServerHandler>();
        initialPos = transform.position;
        gotoPosition = transform.position;
        // _animator = GetComponent<Animator>();
        originalDamage = bulletDamage;
        
        isAlive = true;

        totalLife = 5;
        nameTag.text = playerName;
        actualLife = totalLife;
        lifeBar.value = actualLife;

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
        if((ClassType)Utilidades.Jugadores[playerID].personaje != myClass) CambiarPersonaje((ClassType)Utilidades.Jugadores[playerID].personaje);
        if(isAlive){
            
            if(tieneBandera){
                nameTag.color = Color.red;
            }else{
                nameTag.color = Color.white;
            }

            if (input.y == 0)
            {
                input.x = axisHorizontal * DistanceToMove;
                
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
                input.y = axisVertical * DistanceToMove;

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
                transform.position = Vector3.MoveTowards(transform.position,
                    new Vector3(gotoPosition.x, gotoPosition.y, transform.position.z), speed * Time.deltaTime);

                if (Vector2.Distance(transform.position, gotoPosition) == 0)
                {
                    Moving = false;
                    _animator.SetBool("Move", false);
                    sh.EnviarPos(playerID,transform.position);
                }
            }

            if(canMove){
            
                if ((input.x != 0 || input.y != 0) && !Moving)
                {
                    Vector2 puntoEvaluar = new Vector2(transform.position.x, transform.position.y) + offsetPosition + input;

                    _animator.SetFloat("Horizontal", input.x);
                    _animator.SetFloat("Vertical", input.y);

                    if (!Physics2D.OverlapCircle(puntoEvaluar, circleRadius, obstacles) && mapa.ObtTile(puntoEvaluar,mapa.Obstaculos)==null)
                    {
                        _animator.SetBool("Move", true);
                        Moving = true;
                        gotoPosition += input;

                        // Si hay ulti, se pone
                        if(mapa.EsUlti(puntoEvaluar)){
                            UltiCharge = true;
                            mapa.EliminarTile(puntoEvaluar,mapa.Objetos);
                        }
                    }
                    // else{
                    //     Moving = false;
                    //     Debug.Log("guachin te encontraste con algo");
                    // }
                }
            }
        }
            // if (CanShot)
            // {
            //     if (Input.GetKeyDown(KeyCode.Mouse0))
            //     {
            //         CanShot = false;
            //         var bulletAux = Instantiate(bullet, transform.position, Quaternion.identity);
            //         bulletAux.GetComponent<Bullet>().Player = gameObject;
            //         bulletAux.GetComponent<Bullet>()._Damage = bulletDamage;

            //         switch (lookAt)
            //         {
            //             case LookAt.Down:
            //                 bulletAux.GetComponent<Bullet>().direction = new Vector2(0, -1);
            //                 break;
            //             case LookAt.Up:
            //                 bulletAux.GetComponent<Bullet>().direction = new Vector2(0, 1);
            //                 break;
            //             case LookAt.Right:
            //                 bulletAux.GetComponent<Bullet>().direction = new Vector2(1, 0);
            //                 break;
            //             case LookAt.Left:
            //                 bulletAux.GetComponent<Bullet>().direction = new Vector2(-1, 0);
            //                 break;

            //         }

            //         bulletDamage = originalDamage;

            //         StartCoroutine(BulletTime());
            //     }
            // }
        

        // if (UltiCharge)
        // {
        //     if (Input.GetKey(KeyCode.Space))
        //     {
        //         UseUlti = true;
        //         UltiAttack();
        //         UltiCharge = false;
        //     }
        // }
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
            actualLife = Mathf.Clamp(actualLife - _damage, 0, 5);
        }
        
        lifeBar.value = actualLife;

        if (actualLife <= 0)
        {
            StartCoroutine(PlayerRespawn());
        }

        sh.Damage(playerID,_damage);
    }

    private void HealFunction()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
                
        foreach (var otherplayer in players)
        {   PlayerControllerServer p = otherplayer.GetComponent<PlayerControllerServer>();
            if (p.myTeam == myTeam)
            {
                p.actualLife = Mathf.Clamp(0, 5,actualLife+1);;
                p.lifeBar.value = p.actualLife;
            }
        }
        
        lifeBar.value = actualLife;
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
        
        Vector2 putTrap = new Vector2(transform.position.x, transform.position.y) + offsetPosition + look;

        if (mapa.ObtTile(putTrap, mapa.Obstaculos)==null)
        {
            Debug.Log("Trampa Colocada");
            var _trap = Instantiate(trap, putTrap, Quaternion.identity);
            _trap.GetComponent<TrapController>().Player = gameObject;
            _trap.GetComponent<TrapController>()._Damage = trapDamage;
            sh.Trampa(putTrap);
        }else{
            Debug.Log("El tile de la trampa: "+mapa.ObtTile(putTrap, mapa.Obstaculos));
            Debug.Log("Hay algo? "+Physics2D.OverlapCircle(putTrap, circleRadius, obstacles));
        }
    }

    public IEnumerator TrapEffect(GameObject trap)
    {
        canMove = false;
        sh.EfectoTrampa(playerID);
        // actualLife = totalLife;
        
        yield return new WaitForSeconds(2f);

        canMove = true;
        // sh.EfectoTrampa(playerID);

        if (trap)
        {
            Destroy(trap);
        }
    }

    private IEnumerator UltiTank()
    {
        yield return new WaitForSeconds(boostTime);
        UseUlti = false;
    }

    public void Moverse(Vector2 mov){
        axisHorizontal = mov.x;
        axisVertical = mov.y;
    }

    public void UsarUlti(){
        if (UltiCharge && isAlive)
        {
            
            UseUlti = true;
            UltiAttack();
            UltiCharge = false;
            
        }
    }

    public void Atacar(){
        if (canMove && CanShot && isAlive)
        {
            
            CanShot = false;
            var bulletAux = Instantiate(bullet, transform.position, Quaternion.identity);
            bulletAux.GetComponent<Bullet>().Player = gameObject;
            bulletAux.GetComponent<Bullet>()._Damage = bulletDamage;
            bulletAux.GetComponent<Bullet>().Mapa = mapa;

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

    public void Coger(){
        Vector2 pos = new Vector2(transform.position.x, transform.position.y);
        if(mapa.EsBandera(pos) && !tieneBandera){
            tieneBandera = true;
            mapa.EliminarTile(pos,mapa.Objetos);
            mapa.BanderaAgarrada = true;
            sh.Bandera(playerID);
        }else if(tieneBandera){
            tieneBandera = false;
            mapa.SpawnearBandera(pos);
            mapa.BanderaAgarrada = false;
            sh.Bandera(playerID);

        }
    }

    public void CambiarPersonaje(ClassType c){
        Debug.Log(_animator.name);
        myClass = c;
        switch (c)
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
        _animator.SetBool("Move", true);
    }

    private IEnumerator PlayerRespawn()
    {
        isAlive = false;
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<BoxCollider2D>().enabled = false;

        transform.GetChild(0).GetComponent<Canvas>().enabled = false;
        
        gotoPosition = initialPos;
        transform.position = initialPos;

        lookAt = LookAt.Down;
        _animator.SetFloat("LookAt", 0);
        

        yield return new WaitForSeconds(timeToRespawn);

        transform.GetChild(0).GetComponent<Canvas>().enabled = true;
        
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<BoxCollider2D>().enabled = true;

        actualLife = totalLife;
        lifeBar.value = actualLife;
        
        isAlive = true;

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(gotoPosition+offsetPosition, circleRadius);
    }
}
