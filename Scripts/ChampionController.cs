using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Controls a single champion movement and combat
/// </summary>
public class ChampionController : MonoBehaviourPunCallbacks, IPunObservable
{
    //single player or master player
    public static int TEAMID_PLAYER = 0;
    //AI oponent
    public static int TEAMID_AI = 1;
    //slave player
    public static int SLAVE_PLAYER = 2;

    public GameObject levelupEffectPrefab;
    public GameObject projectileStart;

    [HideInInspector]
    public int gridType = 0;
    [HideInInspector]
    public int gridPositionX = 0;
    [HideInInspector]
    public int gridPositionZ = 0;

    [HideInInspector]
    ///Team of this champion, 
    ///in single mode, can be player = 0, or enemy = 1
    ///in multi mode, can be master = 0, or slave = 2
    public int teamID = 0;


    [HideInInspector]
    public Champion champion;

    [HideInInspector]
    ///Maximum health of the champion
    public float maxHealth = 0;

    [HideInInspector]
    ///current health of the champion 
    public float currentHealth = 0;

    [HideInInspector]
    ///Current damage of the champion deals with a attack
    public float currentDamage = 0;

    [HideInInspector]
    ///The upgrade level of the champion
    public int lvl = 1;

    private Map map;
    private GamePlayController gamePlayController;
    private MultiGamePlayController multiGamePlayController; 
    private AIopponent aIopponent;
    private ChampionAnimation championAnimation;
    private WorldCanvasController worldCanvasController;
    
    public NavMeshAgent navMeshAgent;

    private Vector3 gridTargetPosition;

    public bool _isDragged = false;

    public bool isAttacking = false;

    public bool isDead = false;

    public bool isInCombat = false;
    private float combatTimer = 0;

    public bool isStuned = false;
    private float stunTimer = 0;

    private List<Effect> effects;
    public AudioSource dieAudio;
    public AudioClip dieSelections;

    public Camera gameCamera;

    private float ratio = 1.0f;

    /// Start is called before the first frame update
    void Start()
    {
        //store scripts
        if(PhotonNetwork.IsConnected)
        {
            if(GameObject.Find("MultiPlayScripts") == null)
            {
                Debug.Log("MultiPlayScripts null");
                Destroy(this.gameObject);
                return;
            }
            map = GameObject.Find("MultiPlayScripts").GetComponent<Map>();
            //aIopponent = GameObject.Find("MultiPlayScripts").GetComponent<AIopponent>();
            //gamePlayController = GameObject.Find("MultiPlayScripts").GetComponent<GamePlayController>();
            multiGamePlayController = GameObject.Find("MultiPlayScripts").GetComponent<MultiGamePlayController>();
            worldCanvasController = GameObject.Find("MultiPlayScripts").GetComponent<WorldCanvasController>();
        }
        else
        {
            map = GameObject.Find("Scripts").GetComponent<Map>();
            aIopponent = GameObject.Find("Scripts").GetComponent<AIopponent>();
            gamePlayController = GameObject.Find("Scripts").GetComponent<GamePlayController>();
            //multiGamePlayController = GameObject.Find("Scripts").GetComponent<MultiGamePlayController>();
            worldCanvasController = GameObject.Find("Scripts").GetComponent<WorldCanvasController>();
        }
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        championAnimation = this.GetComponent<ChampionAnimation>();
        gameCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    private static float[] difRatios = new float[]{ 0.8f, 1.0f, 1.2f};

    /// <summary>
    /// When champion created Champion and teamID passed
    /// </summary>
    /// <param name="_champion"></param>
    /// <param name="_teamID"></param>
    ///

    public void Init(Champion _champion, int _teamID)
    {
        champion = _champion;
        teamID = _teamID;

        //store scripts
        if (PhotonNetwork.IsConnected)
        {
            map = GameObject.Find("MultiPlayScripts").GetComponent<Map>();
            //aIopponent = GameObject.Find("MultiPlayScripts").GetComponent<AIopponent>();
            //gamePlayController = GameObject.Find("MultiPlayScripts").GetComponent<GamePlayController>();
            multiGamePlayController = GameObject.Find("MultiPlayScripts").GetComponent<MultiGamePlayController>();
            worldCanvasController = GameObject.Find("MultiPlayScripts").GetComponent<WorldCanvasController>();
        }
        else
        {
            map = GameObject.Find("Scripts").GetComponent<Map>();
            aIopponent = GameObject.Find("Scripts").GetComponent<AIopponent>();
            gamePlayController = GameObject.Find("Scripts").GetComponent<GamePlayController>();
            //multiGamePlayController = GameObject.Find("Scripts").GetComponent<MultiGamePlayController>();
            worldCanvasController = GameObject.Find("Scripts").GetComponent<WorldCanvasController>();
        }
        navMeshAgent = this.GetComponent<NavMeshAgent>();
        championAnimation = this.GetComponent<ChampionAnimation>();

        //disable agent
        navMeshAgent.enabled = false;
        //set stats
        if (teamID == TEAMID_AI)
        {
            int code = GlobalGameData.getInstance().difficultyCode;
            ratio = difRatios[code];
        }
        maxHealth = champion.health * ratio;
        currentHealth = champion.health * ratio;
        currentDamage = champion.damage * ratio;
        worldCanvasController.AddHealthBar(this.gameObject);
        //init healthBar of remote player
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RemoteInitHealthBar", RpcTarget.Others);
        }

        effects = new List<Effect>();
    }

    /// Update is called once per frame
    void Update()
    {
        //never undate others champions
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        if (_isDragged)
        {
            Ray ray;
            //Create a ray from the Mouse click position
            if (PhotonNetwork.IsConnected)
            {
                ray = gameCamera.ScreenPointToRay(Input.mousePosition);
            }
            else
            {
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            }
            
            //hit distance
            float enter = 100.0f;
            if (map.m_Plane.Raycast(ray, out enter))
            {
                //Get the point that is clicked
                Vector3 hitPoint = ray.GetPoint(enter);

                //new character position
                Vector3 p = new Vector3(hitPoint.x, 1.0f, hitPoint.z);

                //move champion
                //this.transform.position = Vector3.Lerp(this.transform.position, p, 0.1f);
                this.transform.position = p;
            }
        }
        else
        {
            if ((PhotonNetwork.IsConnected && multiGamePlayController.currentGameStage == GameStage.Preparation) || 
                (!PhotonNetwork.IsConnected && gamePlayController.currentGameStage == GameStage.Preparation))
            {
                //calc distance
                float distance = Vector3.Distance(gridTargetPosition, this.transform.position);

                if (distance > 0.25f)
                {
                    this.transform.position = Vector3.Lerp(this.transform.position, gridTargetPosition, 0.1f);
                }
                else
                {
                    this.transform.position = gridTargetPosition;
                }
            }
        }

        if (isInCombat && isStuned == false)
        {
            if(PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            {
                Debug.Log("is in combat");
            }
            if (target == null)
            {
                combatTimer += Time.deltaTime;
                if (combatTimer > 0.5f)
                {
                    Debug.Log("try to atack new target");
                    combatTimer = 0;

                    TryAttackNewTarget();
                }
            }

            //combat 
            if (target != null)
            {
                //rotate towards target
                this.transform.LookAt(target.transform, Vector3.up);

                if (target.GetComponent<ChampionController>().isDead == true) //target champion is alive
                {
                    //remove target if targetchampion is dead 
                    target = null;
                    navMeshAgent.isStopped = true;
                    Debug.Log("target is dead");
                }
                else
                {
                    if (isAttacking == false)
                    {
                        //calculate distance
                        float distance = Vector3.Distance(this.transform.position, target.transform.position);

                        //if we are close enough to attack 
                        if (distance < champion.attackRange)
                        {
                            Debug.Log("in range, attack");
                            DoAttack();
                        }
                        else
                        {
                            Debug.Log("out of range, step to target");
                            navMeshAgent.destination = target.transform.position;
                            navMeshAgent.isStopped = false;
                        }
                    }
                }
            }
        }

        //check for stuned effect
        if (isStuned)
        {
            stunTimer -= Time.deltaTime;

            if(stunTimer < 0)
            {
                isStuned = false;

                championAnimation.IsAnimated(true);

                if(target != null)
                {
                    //set pathfinder target
                    navMeshAgent.destination = target.transform.position;

                    navMeshAgent.isStopped = false;
                }
            }
        }
    }

    /// <summary>
    /// Set dragged when moving champion with mouse
    /// </summary>
    public bool IsDragged
    {
        get { return _isDragged; }
        set { _isDragged = value;}
    }

    /// <summary>
    /// Resets champion after combat is over
    /// </summary>
    public void Reset()
    {
        //set active
        this.gameObject.SetActive(true);

        //reset stats
        maxHealth = champion.health * lvl * ratio;
        currentHealth = champion.health * lvl * ratio;
        isDead = false;
        isInCombat = false;
        target = null;
        isAttacking = false;

        //reset position
        SetWorldPosition();
        SetWorldRotation();

        //remove all effects
        foreach (Effect e in effects)
        {
            e.Remove();
        }

        effects = new List<Effect>();

        //reset champiom of remote player
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RemoteReset", RpcTarget.Others);
        }
    }

    /// <summary>
    /// Assign new grid position
    /// </summary>
    /// <param name="_gridType"></param>
    /// <param name="_gridPositionX"></param>
    /// <param name="_gridPositionZ"></param>
    public void SetGridPosition(int _gridType, int _gridPositionX, int _gridPositionZ)
    {
        gridType = _gridType;
        gridPositionX = _gridPositionX;
        gridPositionZ = _gridPositionZ;
        //set new target when changing grid position
        gridTargetPosition = GetWorldPosition();
    }

  /// <summary>
  /// Convert grid position to world position
  /// </summary>
  /// <returns></returns>
    public Vector3 GetWorldPosition()
    {
        //get world position
        Vector3 worldPosition = Vector3.zero;

        if (gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            worldPosition = map.ownInventoryGridPositions[gridPositionX];
        }
        else if(gridType == Map.GRIDTYPE_OPONENT_INVENTORY)
        {
            worldPosition = map.oponentInventoryGridPositions[gridPositionX];
        }
        else if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            worldPosition = map.mapGridPositions[gridPositionX, gridPositionZ];
        }
        return worldPosition;
    }

    /// <summary>
    /// Move to corrent world position
    /// </summary>
    public void SetWorldPosition()
    {
        navMeshAgent.enabled = false;

        //get world position
        Vector3 worldPosition = GetWorldPosition();

        this.transform.position = worldPosition;
        
        gridTargetPosition = worldPosition;
    }

    /// <summary>
    /// Set correct rotation
    /// </summary>
    public void SetWorldRotation()
    {
        Vector3 rotation = Vector3.zero;

        if (teamID == 0)
        {
            rotation = new Vector3(0, 200, 0);
        }
        else if (teamID == 1 || teamID == 2)
        {
            rotation = new Vector3(0, 20, 0);
        }

        this.transform.rotation = Quaternion.Euler(rotation);
    }

    /// <summary>
    /// Upgrade champion lvl
    /// </summary>
    public void UpgradeLevel()
    {
        //incrase lvl
        lvl++;

        float newSize = 1;
        maxHealth = champion.health * ratio;
        currentHealth = champion.health * ratio;

        if (lvl == 2)
        {
            newSize = 1.5f;
            maxHealth = champion.health * 2 * ratio;
            currentHealth = champion.health * 2 * ratio;
            currentDamage = champion.damage * 2 * ratio;
        }
           
        if (lvl == 3)
        {
            newSize = 2f;
            maxHealth = champion.health * 3 * ratio;
            currentHealth = champion.health * 3 * ratio;
            currentDamage = champion.damage * 3 * ratio;
        }

        //set size
        this.transform.localScale = new Vector3(newSize, newSize, newSize);

        //instantiate level up effect
        GameObject levelupEffect = Instantiate(levelupEffectPrefab);

        //set position
        levelupEffect.transform.position = this.transform.position;

        //destroy effect after finished
        Destroy(levelupEffect, 1.0f);

        //display level effect to remote player
        if(PhotonNetwork.IsConnected)
        {
            photonView.RPC("RemoteDisplayLevelUpEffect", RpcTarget.Others);
        }
    }

    public GameObject target;
    /// <summary>
    /// Find the a champion the the closest world position
    /// </summary>
    /// <returns></returns>
    private GameObject FindTarget()
    {
        GameObject closestEnemy = null;
        float bestDistance = 1000;

        //single player, master player or slave player find target
        if (teamID == TEAMID_PLAYER || teamID == SLAVE_PLAYER)
        {
            for (int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    //muti mode, master player find slave player for target
                    //or slave player find master player for target
                    if (PhotonNetwork.IsConnected)
                    {
                        if (multiGamePlayController.oponentGridChampionsArray[x, z] != null)
                        {
                            ChampionController championController = multiGamePlayController.
                                oponentGridChampionsArray[x, z].GetComponent<ChampionController>();

                            if (championController.isDead == false)
                            {
                                //calculate distance
                                float distance = Vector3.Distance(this.transform.position, 
                                    multiGamePlayController.oponentGridChampionsArray[x, z].transform.position);

                                //if new this champion is closer then best distance
                                if (distance < bestDistance)
                                {
                                    bestDistance = distance;
                                    closestEnemy = multiGamePlayController.oponentGridChampionsArray[x, z];
                                }
                            }
                        }
                    }
                    //single mode, find AI for target
                    else
                    {
                        if (aIopponent.gridChampionsArray[x, z] != null)
                        {
                            ChampionController championController = aIopponent.gridChampionsArray[x, z].GetComponent<ChampionController>();

                            if (championController.isDead == false)
                            {
                                //calculate distance
                                float distance = Vector3.Distance(this.transform.position, aIopponent.gridChampionsArray[x, z].transform.position);

                                //if new this champion is closer then best distance
                                if (distance < bestDistance)
                                {
                                    bestDistance = distance;
                                    closestEnemy = aIopponent.gridChampionsArray[x, z];
                                }
                            }
                        }
                    }
                }
            }
        }
        //AI find target
        else if (teamID == TEAMID_AI)
        {
            for (int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    if (gamePlayController.gridChampionsArray[x, z] != null)
                    {
                        ChampionController championController = gamePlayController.gridChampionsArray[x, z].GetComponent<ChampionController>();

                        if (championController.isDead == false)
                        {
                            //calculate distance
                            float distance = Vector3.Distance(this.transform.position, gamePlayController.gridChampionsArray[x, z].transform.position);

                            //if new this champion is closer then best distance
                            if (distance < bestDistance)
                            {
                                bestDistance = distance;
                                closestEnemy = gamePlayController.gridChampionsArray[x, z];
                            }
                        } 
                    }
                }
            }
        }

        return closestEnemy;
    }

    /// <summary>
    /// Looks for new target to attack if there is any
    /// </summary>
    private void TryAttackNewTarget()
    {
        //find closest enemy
        target = FindTarget();

        //if target found
        if (target != null)
        {
            //set pathfinder target
            navMeshAgent.destination = target.transform.position;
            navMeshAgent.isStopped = false;
            //sync navMeshAgent

        }
    }

    /// <summary>
    /// Called when gamestage.combat starts
    /// </summary>
    public void OnCombatStart()
    {
        IsDragged = false;

        this.transform.position = gridTargetPosition;
       
        //in combat grid
        if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            isInCombat = true;

            navMeshAgent.enabled = true;
            //sync navMeshAgent


            TryAttackNewTarget();

        }
    }

   
    /// <summary>
    /// Start attack against enemy champion
    /// </summary>
    private void DoAttack()
    {
        isAttacking = true;

        //stop navigation
        navMeshAgent.isStopped = true;
        //sync navMeshAgent


        championAnimation.DoAttack(true);
    }

    /// <summary>
    /// Called when attack animation finished
    /// </summary>
    public void OnAttackAnimationFinished()
    {
        //never undate others champions
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        //Debug.Log("attack animition finished");
        isAttacking = false;

        if (target != null)
        {
            //get enemy target champion
            ChampionController targetChamoion = target.GetComponent<ChampionController>();

            List<ChampionBonus> activeBonuses = null;

            if (PhotonNetwork.IsConnected)
                activeBonuses = multiGamePlayController.activeBonusList;
            else if(!PhotonNetwork.IsConnected && teamID == TEAMID_PLAYER)
                activeBonuses = gamePlayController.activeBonusList;
            else if (teamID == TEAMID_AI)
                activeBonuses = aIopponent.activeBonusList;

            float d = 0;
            foreach (ChampionBonus b in activeBonuses)
            {
                d += b.ApplyOnAttack(this, targetChamoion);
            }

            //deal damage
            float damage = d + currentDamage;
            bool isTargetDead = false;
            if (PhotonNetwork.IsConnected)
            {
                //photonView.RPC("RemoteOnGotHit", RpcTarget.Others, damage);
                targetChamoion.TargetRemoteGotHit(damage);
                isTargetDead = targetChamoion.isDead;
            }
            else
            {
                isTargetDead = targetChamoion.OnGotHit(damage);
            }

            //target died from attack
            if (isTargetDead)
                TryAttackNewTarget();

            //create projectile if have one
            if(champion.attackProjectile != null && projectileStart != null)
            {
                GameObject projectile = Instantiate(champion.attackProjectile);
                projectile.transform.position = projectileStart.transform.position;
                projectile.GetComponent<Projectile>().Init(target);
                //create projectile for remote player
                if (PhotonNetwork.IsConnected)
                {
                    photonView.RPC("RemoteCreateProjectile", RpcTarget.Others);
                }
            }
        }
    }

    /// <summary>
    /// Called when this champion takes damage
    /// </summary>
    /// <param name="damage"></param>
    public bool OnGotHit(float damage)
    {
        List<ChampionBonus> activeBonuses = null;

        if (PhotonNetwork.IsConnected)
            activeBonuses = multiGamePlayController.activeBonusList;
        else if (!PhotonNetwork.IsConnected && teamID == TEAMID_PLAYER)
            activeBonuses = gamePlayController.activeBonusList;
        else if (teamID == TEAMID_AI)
            activeBonuses = aIopponent.activeBonusList;

        foreach (ChampionBonus b in activeBonuses)
        {
            damage = b.ApplyOnGotHit(this, damage);
        }
       
        currentHealth -= damage;

        
        //death
        if(currentHealth <= 0)
        {
            this.gameObject.SetActive(false);
            isDead = true;
            Debug.Log("Self Champion Die");
            //tell reomte player I die
            if(PhotonNetwork.IsConnected)
            {
                Debug.Log("Sender call champion dead");
                photonView.RPC("RemoteOnDeath", RpcTarget.Others);
            }

            dieAudio.clip = dieSelections;
            //Debug.Log("aaa!");
            dieAudio.Play();

            if(PhotonNetwork.IsConnected)
            {
                multiGamePlayController.OnChampionDeath();
            }
            else
            {
                aIopponent.OnChampionDeath();
                gamePlayController.OnChampionDeath();
            }
        }

        //add floating text
        worldCanvasController.AddDamageText(this.transform.position + new Vector3(0, 2.5f, 0), damage);
        //add floating text for remote player
        if(PhotonNetwork.IsConnected)
        {
            photonView.RPC("RemoteAddDamageText", RpcTarget.Others, damage);
        }

        return isDead;
    }

    /// <summary>
    /// Called when this champion get stuned
    /// </summary>
    /// <param name="stunEffectPrefab"></param>
    public void OnGotStun(float duration)
    {
        isStuned = true;
        stunTimer = duration;

        championAnimation.IsAnimated(false);

        navMeshAgent.isStopped = true;
        //sync navMeshAgent

    }

    /// <summary>
    /// Called when this champion get healed
    /// </summary>
    /// <param name="stunEffectPrefab"></param>
    public void OnGotHeal(float f)
    {
        currentHealth += f;
    }

    /// <summary>
    /// Add effect to this champion
    /// </summary>
    public void AddEffect(GameObject effectPrefab, float duration)
    {
        if (effectPrefab == null)
            return;

        //look for effect
        bool foundEffect = false;
        foreach (Effect e in effects)
        {
            if(effectPrefab == e.effectPrefab)
            {
                e.duration = duration;
                foundEffect = true;
            }
        }

        //not found effect
        if(foundEffect == false)
        {
            Effect effect = this.gameObject.AddComponent<Effect>();
            effect.Init(effectPrefab, this.gameObject, duration);
            effects.Add(effect); 
        }
    }

    /// <summary>
    /// Remove effect when expired
    /// </summary>
    public void RemoveEffect(Effect effect)
    {
        effects.Remove(effect);
        effect.Remove();
    }

    /// <summary>
    /// oponent call targetChamoion and hit it
    /// </summary>
    public void TargetRemoteGotHit(float damage)
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RemoteOnGotHit", RpcTarget.Others, damage);
        }
    }

    /// <summary>
    /// destroy remote champion
    /// </summary>
    public void OnRemoteDestroyChampion()
    {
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RemoteDestroyChampion", RpcTarget.Others);
        }
    }

    /// <summary>
    /// Remote Version: Called when this champion init healthBar
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    public void RemoteInitHealthBar()
    {
        //find worldCanvasController and add healthBar for remote player
        if(worldCanvasController == null)
        {
            worldCanvasController = GameObject.Find("MultiPlayScripts").GetComponent<WorldCanvasController>();
        }
        worldCanvasController.AddHealthBar(this.gameObject);
        //Debug.LogError("RPC CALL BACK RemoteInitHealthBar");
    }

    /// <summary>
    /// Remote Version: Called when this champion show damageText
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    public void RemoteAddDamageText(float damage)
    {
        worldCanvasController.AddDamageText(this.transform.position + new Vector3(0, 2.5f, 0), damage);
        //Debug.LogError("RPC CALL BACK RemoteAddDamageText");
    }

    /// <summary>
    /// Remote Version: Called when this champion takes damage
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    public void RemoteOnGotHit(float damage)
    {
        OnGotHit(damage);
        //Debug.LogError("RPC CALL BACK RemoteOnGotHit");
    }

    /// <summary>
    /// Remote Version: Called when this champion dies
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    public void RemoteOnDeath()
    {
        this.gameObject.SetActive(false);
        isDead = true;
        Debug.Log("sync Champion Die");
        //Debug.LogError("RPC CALL BACK RemoteOnDeath");
    }

    // <summary>
    /// Remote Version: Called when this champion is reset
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    public void RemoteReset()
    {
        this.gameObject.SetActive(true);
        isDead = false;
        Debug.Log("sync Champion Reset");
        //Debug.LogError("RPC CALL BACK RemoteOnDeath");
    }

    // <summary>
    /// Remote Version: Called when levelUp effect is displayed
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    public void RemoteDisplayLevelUpEffect()
    {
        //instantiate level up effect
        GameObject levelupEffect = Instantiate(levelupEffectPrefab);
        //set position
        levelupEffect.transform.position = this.transform.position;
        //destroy effect after finished
        Destroy(levelupEffect, 1.0f);
        //Debug.LogError("RPC CALL BACK RemoteDisplayLevelUpEffect");
    }

    // <summary>
    /// Remote Version: Called when has projectile to create
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    public void RemoteCreateProjectile()
    {
        GameObject projectile = Instantiate(champion.attackProjectile);
        projectile.transform.position = projectileStart.transform.position;
        projectile.GetComponent<Projectile>().Init(target);
        //Debug.LogError("RPC CALL BACK RemoteCreateProjectile");
    }

    // <summary>
    /// Remote Version: Called when champion upgrade and destroy unused champion
    /// </summary>
    /// <param name="damage"></param>
    [PunRPC]
    public void RemoteDestroyChampion()
    {
        Destroy(this.gameObject);
        //Debug.LogError("RPC CALL BACK RemoteCreateProjectile");
    }

    //photonView sync Method
    #region IPunObservable implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(gridType);
            stream.SendNext(gridPositionX);
            stream.SendNext(gridPositionZ);
            stream.SendNext(currentHealth);
            stream.SendNext(maxHealth);
            Debug.Log("sender champion gridType: " + gridType);
            Debug.Log("sender champion position: " + gridPositionX + " " + gridPositionZ);
            // Debug.Log("sned " + this.gameObject.name +  "curHealth :" + currentHealth);
            // stream.SendNext(isDead);
            // stream.SendNext(navMeshAgent.isStopped);
            // stream.SendNext(navMeshAgent.enabled);
            // stream.SendNext(navMeshAgent.destination);
        }
        else
        {
            // Network player, receive data
            int oldGridType = this.gridType;
            int oldGridPositionX = this.gridPositionX;
            int oldGridPositionY = this.gridPositionZ;
            this.gridType = (int)stream.ReceiveNext();
            this.gridPositionX = (int)stream.ReceiveNext();
            this.gridPositionZ = (int)stream.ReceiveNext();
            this.currentHealth = (float)stream.ReceiveNext();
            this.maxHealth = (float)stream.ReceiveNext();
            Debug.Log("receive champion gridType: " + gridType);
            Debug.Log("receive champion position: " + gridPositionX + " " + gridPositionZ);
            Debug.Log("receive " + this.gameObject.name + "curHealth :" + currentHealth);
            // this.isDead = (bool)stream.ReceiveNext();
            if (oldGridType == Map.GRIDTYPE_OWN_INVENTORY)
            {
                MultiGamePlayController.Instance.oponentChampionInventoryArray[oldGridPositionX] = null;
            }
            else if (oldGridType == Map.GRIDTYPE_HEXA_MAP)
            {
                MultiGamePlayController.Instance.oponentGridChampionsArray[oldGridPositionX, oldGridPositionY] = null;
            }
            if (gridType == Map.GRIDTYPE_OWN_INVENTORY)
            {
                MultiGamePlayController.Instance.oponentChampionInventoryArray[gridPositionX] = gameObject;
            }
            else if (gridType == Map.GRIDTYPE_HEXA_MAP)
            {
                MultiGamePlayController.Instance.oponentGridChampionsArray[gridPositionX, gridPositionZ] = gameObject;
            }
            // navMeshAgent.isStopped = (bool)stream.ReceiveNext();
            // navMeshAgent.enabled = (bool)stream.ReceiveNext();
            // navMeshAgent.destination = (Vector3)stream.ReceiveNext();
        }
    }
    #endregion
}
