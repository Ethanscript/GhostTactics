using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public enum GameStage { Preparation, Combat, Loss, Win };

/// <summary>
/// Controlls most of the game logic and player interactions
/// </summary>
public class GamePlayController : MonoBehaviourPunCallbacks
{
    public static GamePlayController Instance;
    public Map map;
    public InputController inputController;
    public GameData gameData;
    public UIController uIController;
    public AIopponent aIopponent;
    public ChampionShop championShop;

    [HideInInspector]
    public GameObject[] ownChampionInventoryArray;
    [HideInInspector]
    public GameObject[] oponentChampionInventoryArray;
    [HideInInspector]
    public GameObject[,] gridChampionsArray;

    public GameStage currentGameStage;
    private float timer = 0;

    ///The time available to place champions
    public int PreparationStageDuration = 16;
    ///Maximum time the combat stage can last
    public int CombatStageDuration = 60;
    ///base gold value to get after every round
    public int baseGoldIncome = 5;

    public int currentChampionLimit = 3;
    public int currentChampionCount = 0;
    [HideInInspector]
    public int initCurGold = 5;
    public int currentGold = 5;
    public int initCurHP = 100;
    public int currentHP = 100;
    [HideInInspector]
    public int timerDisplay = 0;

    public Dictionary<ChampionType, int> championTypeCount;
    public List<ChampionBonus> activeBonusList;

    ///The damage that player takes when losing a round
    public int championDamage = 2;

    void Awake()
    {
        Instance = this;
    }

    /// Start is called before the first frame update
    void Start()
    {
        //multi mode not run
        if (PhotonNetwork.IsConnected)
        {
            return;
        }

        //set starting gamestage
        currentGameStage = GameStage.Preparation;

        //renew champion choose array
        GlobalGameData.getInstance().renewchosenChampionArray();

        //init arrays
        ownChampionInventoryArray = new GameObject[Map.inventorySize];
        oponentChampionInventoryArray = new GameObject[Map.inventorySize];
        gridChampionsArray = new GameObject[Map.hexMapSizeX, Map.hexMapSizeZ / 2];


        uIController.UpdateUI();
        //syncDataFromOponent();
    }

    /// Update is called once per frame
    void Update()
    {
        //multi mode not run
        if (PhotonNetwork.CurrentRoom != null)
        {
            return;
        }

        //manage game stage
        if (currentGameStage == GameStage.Preparation)
        {
            timer += Time.deltaTime;

            timerDisplay = (int)(PreparationStageDuration - timer);

            uIController.UpdateTimerText();

            if (timer > PreparationStageDuration)
            {
                timer = 0;

                OnGameStageComplate();
            }
        }
        else if (currentGameStage == GameStage.Combat)
        {
            timer += Time.deltaTime;

            timerDisplay = (int)timer;

            if (timer > CombatStageDuration)
            {
                timer = 0;

                OnGameStageComplate();
            }
        }
        //syncDataFromOponent();
    }




    /// <summary>
    /// Adds champion from shop to inventory
    /// </summary>
    public bool BuyChampionFromShop(Champion champion)
    {
        //get first empty inventory slot
        int emptyIndex = -1;
        for (int i = 0; i < ownChampionInventoryArray.Length; i++)
        {
            if (ownChampionInventoryArray[i] == null)
            {
                emptyIndex = i;
                break;
            }
        }

        //return if no slot to add champion
        if (emptyIndex == -1)
            return false;

        //we dont have enought gold return
        if (currentGold < champion.cost)
            return false;

        //instantiate champion prefab
        GameObject championPrefab = null;
        if (PhotonNetwork.IsConnected == true)
        {
            championPrefab = PhotonNetwork.Instantiate(champion.prefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
        }
        else
        {
            championPrefab = Instantiate(champion.prefab);
        }

        //get championController
        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        //setup chapioncontroller
        championController.Init(champion, ChampionController.TEAMID_PLAYER);

        //set grid position
        championController.SetGridPosition(Map.GRIDTYPE_OWN_INVENTORY, emptyIndex, -1);

        //set position and rotation
        championController.SetWorldPosition();
        championController.SetWorldRotation();


        //store champion in inventory array
        StoreChampionInArray(Map.GRIDTYPE_OWN_INVENTORY, map.ownTriggerArray[emptyIndex].gridX, -1, championPrefab);




        //only upgrade when in preparation stage
        if (currentGameStage == GameStage.Preparation)
            TryUpgradeChampion(champion); //upgrade champion


        //deduct gold
        currentGold -= champion.cost;

        //set gold on ui
        uIController.UpdateUI();

        //return true if succesful buy
        return true;
    }


    /// <summary>
    /// Check all champions if a upgrade is possible
    /// </summary>
    /// <param name="champion"></param>
    private void TryUpgradeChampion(Champion champion)
    {
        //check for champion upgrade
        List<ChampionController> championList_lvl_1 = new List<ChampionController>();
        List<ChampionController> championList_lvl_2 = new List<ChampionController>();

        for (int i = 0; i < ownChampionInventoryArray.Length; i++)
        {
            //there is a champion
            if (ownChampionInventoryArray[i] != null)
            {
                //get character
                ChampionController championController = ownChampionInventoryArray[i].GetComponent<ChampionController>();

                //check if is the same type of champion that we are buying
                if (championController.champion == champion)
                {
                    if (championController.lvl == 1)
                        championList_lvl_1.Add(championController);
                    else if (championController.lvl == 2)
                        championList_lvl_2.Add(championController);
                }
            }

        }

        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    //get character
                    ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                    //check if is the same type of champion that we are buying
                    if (championController.champion == champion)
                    {
                        if (championController.lvl == 1)
                            championList_lvl_1.Add(championController);
                        else if (championController.lvl == 2)
                            championList_lvl_2.Add(championController);
                    }
                }

            }
        }

        //if we have 3 we upgrade a champion and delete rest
        if (championList_lvl_1.Count > 2)
        {
            //upgrade
            championList_lvl_1[2].UpgradeLevel();

            //remove from array
            RemoveChampionFromArray(championList_lvl_1[0].gridType, championList_lvl_1[0].gridPositionX, championList_lvl_1[0].gridPositionZ);
            RemoveChampionFromArray(championList_lvl_1[1].gridType, championList_lvl_1[1].gridPositionX, championList_lvl_1[1].gridPositionZ);

            //destroy gameobjects
            Destroy(championList_lvl_1[0].gameObject);
            Destroy(championList_lvl_1[1].gameObject);

            //we upgrade to lvl 3
            if (championList_lvl_2.Count > 1)
            {
                //upgrade
                championList_lvl_1[2].UpgradeLevel();

                //remove from array
                RemoveChampionFromArray(championList_lvl_2[0].gridType, championList_lvl_2[0].gridPositionX, championList_lvl_2[0].gridPositionZ);
                RemoveChampionFromArray(championList_lvl_2[1].gridType, championList_lvl_2[1].gridPositionX, championList_lvl_2[1].gridPositionZ);

                //destroy gameobjects
                Destroy(championList_lvl_2[0].gameObject);
                Destroy(championList_lvl_2[1].gameObject);
            }
        }



        currentChampionCount = GetChampionCountOnHexGrid();

        //update ui
        uIController.UpdateUI();

    }

    private GameObject draggedChampion = null;
    private TriggerInfo dragStartTrigger = null;

    /// <summary>
    /// When we start dragging champions on map
    /// </summary>
    public void StartDrag()
    {
        if (currentGameStage != GameStage.Preparation)
            return;

        //get trigger info
        TriggerInfo triggerinfo = inputController.triggerInfo;
        //if mouse cursor on trigger
        if (triggerinfo != null)
        {
            dragStartTrigger = triggerinfo;

            GameObject championGO = GetChampionFromTriggerInfo(triggerinfo);

            if (championGO != null)
            {
                //show indicators
                map.ShowIndicators();

                draggedChampion = championGO;

                //isDragging = true;

                championGO.GetComponent<ChampionController>().IsDragged = true;
                //Debug.Log("STARTDRAG");
            }

        }
    }

    /// <summary>
    /// When we stop dragging champions on map
    /// </summary>
    public void StopDrag()
    {
        //hide indicators
        map.HideIndicators();

        int championsOnField = GetChampionCountOnHexGrid();


        if (draggedChampion != null)
        {
            //set dragged
            draggedChampion.GetComponent<ChampionController>().IsDragged = false;

            //get trigger info
            TriggerInfo triggerinfo = inputController.triggerInfo;

            //if mouse cursor on trigger
            if (triggerinfo != null)
            {
                //get current champion over mouse cursor
                GameObject currentTriggerChampion = GetChampionFromTriggerInfo(triggerinfo);

                //there is another champion in the way
                if (currentTriggerChampion != null)
                {
                    //store this champion to start position
                    StoreChampionInArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ, currentTriggerChampion);

                    //store this champion to dragged position
                    StoreChampionInArray(triggerinfo.gridType, triggerinfo.gridX, triggerinfo.gridZ, draggedChampion);
                }
                else
                {
                    //we are adding to combat field
                    if (triggerinfo.gridType == Map.GRIDTYPE_HEXA_MAP)
                    {
                        //only add if there is a free spot or we adding from combatfield
                        if (championsOnField < currentChampionLimit || dragStartTrigger.gridType == Map.GRIDTYPE_HEXA_MAP)
                        {
                            //remove champion from dragged position
                            RemoveChampionFromArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ);

                            //add champion to dragged position
                            StoreChampionInArray(triggerinfo.gridType, triggerinfo.gridX, triggerinfo.gridZ, draggedChampion);

                            if (dragStartTrigger.gridType != Map.GRIDTYPE_HEXA_MAP)
                                championsOnField++;
                        }
                    }
                    else if (triggerinfo.gridType == Map.GRIDTYPE_OWN_INVENTORY)
                    {
                        //remove champion from dragged position
                        RemoveChampionFromArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ);

                        //add champion to dragged position
                        StoreChampionInArray(triggerinfo.gridType, triggerinfo.gridX, triggerinfo.gridZ, draggedChampion);

                        if (dragStartTrigger.gridType == Map.GRIDTYPE_HEXA_MAP)
                            championsOnField--;
                    }



                }




            }


            CalculateBonuses();

            currentChampionCount = GetChampionCountOnHexGrid();

            //update ui
            uIController.UpdateUI();


            draggedChampion = null;
        }


    }


    /// <summary>
    /// Get champion gameobject from triggerinfo
    /// </summary>
    /// <param name="triggerinfo"></param>
    /// <returns></returns>
    private GameObject GetChampionFromTriggerInfo(TriggerInfo triggerinfo)
    {
        GameObject championGO = null;

        if (triggerinfo.gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            championGO = ownChampionInventoryArray[triggerinfo.gridX];
        }
        else if (triggerinfo.gridType == Map.GRIDTYPE_OPONENT_INVENTORY)
        {
            championGO = oponentChampionInventoryArray[triggerinfo.gridX];
        }
        else if (triggerinfo.gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            championGO = gridChampionsArray[triggerinfo.gridX, triggerinfo.gridZ];
        }

        return championGO;
    }

    /// <summary>
    /// Store champion gameobject in array
    /// </summary>
    /// <param name="triggerinfo"></param>
    /// <param name="champion"></param>
    private void StoreChampionInArray(int gridType, int gridX, int gridZ, GameObject champion)
    {
        //assign current trigger to champion
        ChampionController championController = champion.GetComponent<ChampionController>();
        championController.SetGridPosition(gridType, gridX, gridZ);

        if (gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            ownChampionInventoryArray[gridX] = champion;
        }
        else if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            gridChampionsArray[gridX, gridZ] = champion;
        }
    }

    /// <summary>
    /// Remove champion from array
    /// </summary>
    /// <param name="triggerinfo"></param>
    private void RemoveChampionFromArray(int type, int gridX, int gridZ)
    {
        if (type == Map.GRIDTYPE_OWN_INVENTORY)
        {
            ownChampionInventoryArray[gridX] = null;
        }
        else if (type == Map.GRIDTYPE_HEXA_MAP)
        {
            gridChampionsArray[gridX, gridZ] = null;
        }
    }

    /// <summary>
    /// Returns the number of champions we have on the map
    /// </summary>
    /// <returns></returns>
    private int GetChampionCountOnHexGrid()
    {
        int count = 0;
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    count++;
                }
            }
        }

        return count;
    }

    /// <summary>
    /// Calculates the bonuses we have currently
    /// </summary>
    private void CalculateBonuses()
    {
        //init dictionary
        championTypeCount = new Dictionary<ChampionType, int>();

        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    //get champion
                    Champion c = gridChampionsArray[x, z].GetComponent<ChampionController>().champion;

                    if (championTypeCount.ContainsKey(c.type1))
                    {
                        int cCount = 0;
                        championTypeCount.TryGetValue(c.type1, out cCount);

                        cCount++;

                        championTypeCount[c.type1] = cCount;
                    }
                    else
                    {
                        championTypeCount.Add(c.type1, 1);
                    }

                    if (championTypeCount.ContainsKey(c.type2))
                    {
                        int cCount = 0;
                        championTypeCount.TryGetValue(c.type2, out cCount);

                        cCount++;

                        championTypeCount[c.type2] = cCount;
                    }
                    else
                    {
                        championTypeCount.Add(c.type2, 1);
                    }

                }
            }
        }

        activeBonusList = new List<ChampionBonus>();

        foreach (KeyValuePair<ChampionType, int> m in championTypeCount)
        {
            ChampionBonus championBonus = m.Key.championBonus;

            //have enough champions to get bonus
            if (m.Value >= championBonus.championCount)
            {
                activeBonusList.Add(championBonus);
            }
        }

    }

    /// <summary>
    /// Resets all champion stats and positions
    /// </summary>
    private void ResetChampions()
    {
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    //get character
                    ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                    //reset
                    championController.Reset();
                }

            }
        }
    }

    /// <summary>
    /// Called when a game stage is finished
    /// </summary>
    private void OnGameStageComplate()
    {
        //tell ai that stage complated
        aIopponent.OnGameStageComplate(currentGameStage);

        if (currentGameStage == GameStage.Preparation)
        {
            //set new game stage
            currentGameStage = GameStage.Combat;

            //show indicators
            map.HideIndicators();

            //hide timer text
            uIController.SetTimerTextActive(false);


            if (draggedChampion != null)
            {
                //stop dragging    
                draggedChampion.GetComponent<ChampionController>().IsDragged = false;
                draggedChampion = null;
            }


            for (int i = 0; i < ownChampionInventoryArray.Length; i++)
            {
                //there is a champion
                if (ownChampionInventoryArray[i] != null)
                {
                    //get character
                    ChampionController championController = ownChampionInventoryArray[i].GetComponent<ChampionController>();

                    //start combat
                    championController.OnCombatStart();
                }
            }

            //start own champion combat
            for (int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    //there is a champion
                    if (gridChampionsArray[x, z] != null)
                    {
                        //get character
                        ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                        //start combat
                        championController.OnCombatStart();
                    }

                }
            }


            //check if we start with 0 champions
            if (IsAllChampionDead())
                EndRound();


        }
        else if (currentGameStage == GameStage.Combat)
        {
            //totall damage player takes
            int damage = 0;
            //iterate champions
            //start champion combat
            for (int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    //there is a champion
                    if (gridChampionsArray[x, z] != null)
                    {
                        //get character
                        ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                        //calculate player damage for every champion
                        if (championController.currentHealth > 0)
                            damage += championDamage;
                    }

                }
            }
            //UI takes damage
            aIopponent.TakeDamage(damage);

            //set new game stage
            currentGameStage = GameStage.Preparation;

            //show timer text
            uIController.SetTimerTextActive(true);

            //reset champion
            ResetChampions();

            //go through all champion infos
            for (int i = 0; i < gameData.championsArray.Length; i++)
            {
                TryUpgradeChampion(gameData.championsArray[i]);
            }


            //add gold
            currentGold += CalculateIncome();

            //set gold ui
            uIController.UpdateUI();

            //refresh shop ui
            championShop.RefreshShop(true);

            //check if we win or lose
            if (currentHP <= 0)
            {
                GameOver(false);
            }
            else if (aIopponent.currentHP <= 0)
            {
                GameOver(true);
            }

        }
    }

    /// <summary>
    /// Returns the number of gold we should recieve
    /// </summary>
    /// <returns></returns>
    private int CalculateIncome()
    {
        int income = 0;

        //banked gold
        int bank = (int)(currentGold / 10);


        income += baseGoldIncome;
        income += bank;

        return income;
    }

    /// <summary>
    /// Incrases the available champion slots by 1
    /// </summary>
    public void Buylvl()
    {
        //return if we dont have enough gold
        if (currentGold < 4)
            return;

        if (currentChampionLimit < 9)
        {
            //incrase champion limit
            currentChampionLimit++;

            //decrase gold
            currentGold -= 4;

            //update ui
            uIController.UpdateUI();

        }

    }

    /// <summary>
    /// Called when round was lost
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {

        currentHP -= damage;

        uIController.UpdateUI();

    }

    /// <summary>
    /// Called when Game was lost
    /// </summary>
    public void RestartGame()
    {
        //remove champions
        for (int i = 0; i < ownChampionInventoryArray.Length; i++)
        {
            //there is a champion
            if (ownChampionInventoryArray[i] != null)
            {
                //get character
                ChampionController championController = ownChampionInventoryArray[i].GetComponent<ChampionController>();

                Destroy(championController.gameObject);
                ownChampionInventoryArray[i] = null;
            }

        }

        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    //get character
                    ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                    Destroy(championController.gameObject);
                    gridChampionsArray[x, z] = null;
                }

            }
        }

        //renew champion choose array
        GlobalGameData.getInstance().renewchosenChampionArray();

        //reset stats
        currentHP = initCurHP;
        currentGold = initCurGold;
        currentGameStage = GameStage.Preparation;
        currentChampionLimit = 3;
        currentChampionCount = GetChampionCountOnHexGrid();

        uIController.UpdateUI();

        //restart ai
        aIopponent.Restart();

        //show hide ui
        uIController.ShowGameScreen();


    }


    /// <summary>
    /// Ends the round
    /// </summary>
    public void EndRound()
    {
        timer = CombatStageDuration - 3; //reduce timer so game ends fast
    }


    /// <summary>
    /// Called when a champion killd
    /// </summary>
    public void OnChampionDeath()
    {
        bool allDead = IsAllChampionDead();

        if (allDead)
            EndRound();
    }


    /// <summary>
    /// Returns true if all the champions are dead
    /// </summary>
    /// <returns></returns>
    private bool IsAllChampionDead()
    {
        int championCount = 0;
        int championDead = 0;
        //start own champion combat
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    //get character
                    ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();


                    championCount++;

                    if (championController.isDead)
                        championDead++;

                }

            }
        }

        if (championDead == championCount)
            return true;

        return false;

    }

    /// <summary>
    /// Change gameStage to Win or Loss and show UI 
    /// </summary>
    /// <param name="isWin"></param>
    public void GameOver(bool isWin)
    {
        //Player wins
        if (isWin)
        {
            currentGameStage = GameStage.Win;
            uIController.ShowWinScreen();
        }
        //UI wins
        else
        {
            currentGameStage = GameStage.Loss;
            uIController.ShowLossScreen();
        }
    }

    public void syncDataFromOponent()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ExitGames.Client.Photon.Hashtable syncTable = new ExitGames.Client.Photon.Hashtable();
            GameObject[] opoarr = new GameObject[3];
            syncTable.Add("GameStage", currentGameStage.ToString());
            syncTable.Add("currentHP", currentHP.ToString());
            syncTable.Add("timer", timer.ToString());
            PhotonNetwork.CurrentRoom.SetCustomProperties(syncTable);

        }
        else
        {
            object rec1, rec2, rec3;
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameStage", out rec1);
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("currentHP", out rec2);
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("timer", out rec3);
            //GameStage stage = System.Convert.ToInt32((string)rec1);
            int hp = System.Convert.ToInt32((string)rec2);
            float timers = (float)System.Convert.ToDouble((string)rec3);
            Debug.Log("Receiver1:" + (string)rec1);
            Debug.Log("Receiver2:" + (string)rec2);
            Debug.Log("Receiver3:" + (string)rec3);
            //Debug.Log("Receiver1:" + stage);
            //Debug.Log("Receiver2:" + hp);
            Debug.Log("Receiver3:" + timers);
        }
    }
}
