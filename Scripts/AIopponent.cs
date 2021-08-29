using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Controlls Enemy champions
/// </summary>
public class AIopponent : MonoBehaviour
{
    public ChampionShop championShop;
    public Map map;
    public UIController uIController;
    public GamePlayController gamePlayController;

    public GameObject[,] gridChampionsArray;

    public Dictionary<ChampionType, int> championTypeCount;
    public List<ChampionBonus> activeBonusList;

    public int initCurHP = 100;
    public int currentHP = 100;
    ///The damage that player takes when losing a round
    public int championDamage = 2;
    private int difficultyCode;
    public int currentChampionLimit = 6;

    void Start()
    {
        //multi game not need AI
        if (PhotonNetwork.IsConnected)
        {
            return;
        }
        difficultyCode = GlobalGameData.getInstance().difficultyCode;
    }


    /// <summary>
    /// Called when map is created
    /// </summary>


    public void OnMapReady()
    {
        //multi game not need AI
        if (PhotonNetwork.IsConnected)
        {
            return;
        }
        gridChampionsArray = new GameObject[Map.hexMapSizeX, Map.hexMapSizeZ / 2];

        AddEnemy();
        // AddRandomChampion();
    }

    /// <summary>
    /// Called when a stage is finished
    /// </summary>
    /// <param name="stage"></param>
    public void OnGameStageComplate(GameStage stage)
    {
        if (stage == GameStage.Preparation)
        {
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

                        //start combat
                        championController.OnCombatStart();
                    }

                }
            }
        }

        if (stage == GameStage.Combat)
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

            //player takes damage
            gamePlayController.TakeDamage(damage);

            ResetChampions();

            AddEnemy();
            //  AddRandomChampion();
        }
    }

    /// <summary>
    /// Returns empty position in the map grid
    /// </summary>
    /// <param name="emptyIndexX"></param>
    /// <param name="emptyIndexZ"></param>
    private void GetEmptySlot(out int emptyIndexX, out int emptyIndexZ)
    {
        emptyIndexX = -1;
        emptyIndexZ = -1;

        //get first empty inventory slot
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                if (gridChampionsArray[x, z] == null)
                {
                    emptyIndexX = x;
                    emptyIndexZ = z;
                    break;
                }
            }
        }
    }

    public void AddEnemy()
    {
        int championsOnField = GetChampionCountOnHexGrid();
        int code = difficultyCode;
        int cnt = 0;
        float randy = Random.Range(0, 1);
        if (code < 1) cnt = 1;
        else if (code == 1)
        {
            cnt = randy > 0.5 ? 1 : 2;
        }
        else
        {
            cnt = randy > 0.7 ? 3 : 2;
        }
        for (int i = 0; i < cnt; i++)
        {
            if (championsOnField >= currentChampionLimit)
            {
                break;
            }
            AddRandomChampion();
            championsOnField++;
        }
    }
    /// <summary>
    /// Creates and adds a new random champion to the map
    /// </summary>
    public void AddRandomChampion()
    {
        //get an empty slot
        int indexX;
        int indexZ;
        GetEmptySlot(out indexX, out indexZ);

        //dont add champion if there is no empty slot
        if (indexX == -1 || indexZ == -1)
            return;

        Champion champion = championShop.GetRandomChampionForAI();

        //instantiate champion prefab
        GameObject championPrefab = Instantiate(champion.prefab);
        //GameObject championPrefab = PhotonNetwork.Instantiate(champion.prefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);

        //add champion to array
        gridChampionsArray[indexX, indexZ] = championPrefab;

        //get champion controller
        ChampionController championController = championPrefab.GetComponent<ChampionController>();

        //setup chapioncontroller
        championController.Init(champion, ChampionController.TEAMID_AI);

        //set grid position
        championController.SetGridPosition(Map.GRIDTYPE_HEXA_MAP, indexX, indexZ + 4);

        //set position and rotation
        championController.SetWorldPosition();
        championController.SetWorldRotation();

        //check for champion upgrade
        List<ChampionController> championList_lvl_1 = new List<ChampionController>();
        List<ChampionController> championList_lvl_2 = new List<ChampionController>();

        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    //get character
                    ChampionController cc = gridChampionsArray[x, z].GetComponent<ChampionController>();

                    //check if is the same type of champion that we are buying
                    if (cc.champion == champion)
                    {
                        if (cc.lvl == 1)
                            championList_lvl_1.Add(cc);
                        else if (cc.lvl == 2)
                            championList_lvl_2.Add(cc);
                    }
                }

            }
        }

        //if we have 3 we upgrade a champion and delete rest
        if (championList_lvl_1.Count == 3)
        {
            //upgrade
            championList_lvl_1[2].UpgradeLevel();

            //destroy gameobjects
            Destroy(championList_lvl_1[0].gameObject);
            Destroy(championList_lvl_1[1].gameObject);

            //we upgrade to lvl 3
            if (championList_lvl_2.Count == 2)
            {
                //upgrade
                championList_lvl_1[2].UpgradeLevel();

                //destroy gameobjects
                Destroy(championList_lvl_2[0].gameObject);
                Destroy(championList_lvl_2[1].gameObject);
            }
        }


        CalculateBonuses();
    }

    /// <summary>
    /// Resets all owned champions on the grid 
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

                    //set position and rotation
                    championController.Reset();



                }

            }
        }
    }

    /// <summary>
    /// Called when a game finished and needs restart
    /// </summary>
    public void Restart()
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

                    Destroy(championController.gameObject);
                    gridChampionsArray[x, z] = null;

                }

            }
        }

        currentHP = initCurHP;

        uIController.UpdateUI();

        AddEnemy();
        //AddRandomChampion();
    }

    /// <summary>
    /// Called when champion health goes belove 0
    /// </summary>
    public void OnChampionDeath()
    {
        bool allDead = IsAllChampionDead();

        if (allDead)
            gamePlayController.EndRound();
    }


    /// <summary>
    /// Checks if all champion is dead
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
    /// Calculates champion bonuses
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
    /// Called when round was lost
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {

        currentHP -= damage;

        uIController.UpdateUI();

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
}
