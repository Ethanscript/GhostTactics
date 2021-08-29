using System;
using UnityEngine;

/*
    attribute
    0 frost
    1 fire
    2 nature
    3 thunder
    4 water
*/
/*Champions
     * 0 frost knight
     * 1 fire knight
     * 2 nature knight
     * 3 frost wizard
     * 4 fire wizard 
     * 5 nature wizard
     * 6 frost archer
     * 7 fire archer
     * 8 nature archer
     * 9 water knight
     * 10 water wizard
     * 11 water archer
    */
/*public struct CardInf
{
    string name;
    int attr;
    int num;
    string descrption;
}*/
public class ChampionCardInformation : MonoBehaviour
{
    [HideInInspector]
    public static Cards cards;
    private static ChampionCardInformation _instance;
    //private static ChampionCardInformation _instance = new ChampionCardInformation();
    public static ChampionCardInformation getInstance()
    {
        if (_instance == null)
        {
            Debug.Log("FUCK");
        }
        else
        {
            Debug.Log("WOCAO");
        }
        return _instance;
    }




    private void Awake()
    {
        _instance = this;
        string json = Resources.Load<TextAsset>("CardInformation").text;
        Debug.Log("Champion" + json);
        cards = JsonUtility.FromJson<Cards>(json);
        //Debug.Log(cards.list[1].name);
    }
}

[Serializable]
public class CardInf
{
    public string name;
    public int attr;
    public int num;
    public string description;
    public string image;
}

[Serializable]
public class Cards
{
    public CardInf[] list;
}
