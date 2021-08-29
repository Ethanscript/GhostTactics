using UnityEngine;
using UnityEngine.UI;

public class CardPrefab : MonoBehaviour
{
    [HideInInspector]
    public bool selected = false;
    /*public Sprite attribute;
    public Sprite frame;
    public Sprite background;
    public Sprite picture;
    public Sprite text;
    public Sprite Name;*/


    /*
    attribute's sprite
    0 frost
    1 fire
    2 nature
    3 thunder
     */

    private string genPath(string num, string where)
    {
        return "TCG_Card_Elemental_Design/Card_Color_" + num + "/Face_Card_Color_" + num + "/Face_Card_Color_" + num + "_" + where;
    }
    public void getChampion(int n)
    {
        Debug.Log("emmm");
        string num;
        int type = ChampionCardInformation.cards.list[n].attr;
        //frost use color_11
        if (type == 0)
        {
            num = "11";
        }
        //fire use color_12
        else if (type == 1)
        {
            num = "12";
        }
        //nature use color_16
        else if (type == 2)
        {
            num = "16";
        }
        //thunder use color_05
        else if (type == 3)
        {
            num = "05";
        }
        //water use color_10
        else
        {
            num = "10";
        }

        //Debug.Log(genPath(num, "Back"));
        //Sprite tmp = (Sprite)Resources.Load<Sprite>("Correct");
        ///Sprite tmp = Resources.Load<Sprite>("TCG_Card_Elemental_Design/Card_Color_11/Face_Card_Color_11/Face_Card_Color_11_Back");
        //Debug.Log(tmp);
        transform.GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(genPath(num, "Back"));
        transform.GetChild(1).GetComponent<Image>().sprite = Resources.Load<Sprite>(genPath(num, "Frame"));
        transform.GetChild(2).GetComponent<Image>().sprite = Resources.Load<Sprite>(genPath(num, "Logo"));
        transform.GetChild(3).GetComponent<Image>().sprite = Resources.Load<Sprite>(genPath(num, "Place_Picture"));
        transform.GetChild(4).GetComponent<Image>().sprite = Resources.Load<Sprite>(genPath(num, "Place_Text"));
        transform.GetChild(4).GetChild(0).GetComponent<Text>().text = ChampionCardInformation.cards.list[n].description;
        transform.GetChild(5).GetComponent<Image>().sprite = Resources.Load<Sprite>(genPath(num, "Ribbon"));
        transform.GetChild(5).GetChild(0).GetComponent<Text>().text = ChampionCardInformation.cards.list[n].name;
        transform.GetChild(3).GetChild(0).GetComponent<Image>().sprite = Resources.Load<Sprite>(ChampionCardInformation.cards.list[n].image);
    }

    public void setName(string name)
    {
        transform.GetChild(5).GetChild(0).GetComponent<Text>().text = name;
    }

    public void setDescription(string d)
    {
        transform.GetChild(5).GetChild(0).GetComponent<Text>().text = d;
    }

    public void changeSelected()
    {
        selected = !selected;
    }

}
