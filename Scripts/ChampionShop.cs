using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Creates and stores champions available, XP and LVL purchase
/// </summary>
public class ChampionShop : MonoBehaviour
{
    public UIController uIController;
    public GamePlayController gamePlayController;
    public MultiGamePlayController multiGamePlayController;
    public GameData gameData;

    ///Array to store available champions to purchase
    private Champion[] availableChampionArray;

    /// Start is called before the first frame update
    void Start()
    {
        RefreshShop(true);
    }

    /// Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// Gives a level up the player
    /// </summary>
    public void BuyLvl()
    {
        if (PhotonNetwork.IsConnected)
        {
            multiGamePlayController.Buylvl();
        }
        else
        {
            gamePlayController.Buylvl();
        }
    }

    /// <summary>
    /// Refreshes shop with new random champions
    /// </summary>
    public void RefreshShop(bool isFree)
    {
        //return if we dont have enough gold
        if (PhotonNetwork.IsConnected && multiGamePlayController.currentGold < 2 && isFree == false)
            return;
        else if (!PhotonNetwork.IsConnected && gamePlayController.currentGold < 2 && isFree == false)
            return;

        //init array
        availableChampionArray = new Champion[5];

        //fill up shop
        for (int i = 0; i < availableChampionArray.Length; i++)
        {
            //get a random champion
            Champion champion = GetRandomChampionInfo();

            //store champion in array
            availableChampionArray[i] = champion;

            //load champion to ui
            uIController.LoadShopItem(champion, i);

            //show shop items
            uIController.ShowShopItems();
        }

        //decrase gold
        
         if(isFree == false && PhotonNetwork.IsConnected)
            multiGamePlayController.currentGold -= 2;
        else if (isFree == false && !PhotonNetwork.IsConnected)
            gamePlayController.currentGold -= 2;

        //update ui
        uIController.UpdateUI();
    }

    /// <summary>
    /// Called when ui champion frame clicked
    /// </summary>
    /// <param name="index"></param>
    public void OnChampionFrameClicked(int index)
    {
        bool isSucces = false;
        if (PhotonNetwork.IsConnected)
        {
            isSucces = multiGamePlayController.BuyChampionFromShop(availableChampionArray[index]);
        }
        else
        {
            isSucces = gamePlayController.BuyChampionFromShop(availableChampionArray[index]);
        }
        if (isSucces)
            uIController.HideChampionFrame(index);
    }

    /// <summary>
    /// Returns a random champion
    /// </summary>
    public Champion GetRandomChampionInfo()
    {
        GlobalGameData.getInstance().renewchosenChampionArray();
        
        int index = GlobalGameData.getInstance().cards.Length;
        int rand = Random.Range(0, index);
        while (true)
        {
            if (GlobalGameData.getInstance().chosenChampion[GlobalGameData.getInstance().cards[rand]] == 1)
            {
                break;
            }
            else
            {
                rand = Random.Range(0, index);
            }
        }
        return gameData.championsArray[GlobalGameData.getInstance().cards[rand]];
        
    }

    /// <summary>
    /// Returns a random champion
    /// </summary>
    public Champion GetRandomChampionForAI()
    {
        GlobalGameData.getInstance().renewchosenChampionArray();
        int range = (GlobalGameData.getInstance().difficultyCode + 1) * 3;
        int rand = Random.Range(0, range);

        //return from array
        return gameData.championsArray[rand];
    }
}
