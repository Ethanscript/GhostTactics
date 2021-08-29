using UnityEngine;
using UnityEngine.UI;

public class CardMange : MonoBehaviour
{
    static GameObject instanceCardPrefab;
    static Transform instanceTransform;
    [SerializeField]
    GameObject cardPrefab;
    WebWork web;

    private static int[] selected;
    [HideInInspector]
    public static int selectednum = 1;

    public int max = 4;
    public int min = 1;
    public static string key_selected = "selected";

    /*
    attribute's sprite
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
    public Sprite[] attributes;


    /*private void generateCards()
    {
        GameObject card = Instantiate(cardPrefab, transform);
        card.GetComponent<CardPrefab>().getChampion(0);
        card = Instantiate(cardPrefab, transform);
        card.GetComponent<CardPrefab>().getBackByAttr(1);
        card = Instantiate(cardPrefab, transform);
        card.GetComponent<CardPrefab>().getBackByAttr(2);
        card = Instantiate(cardPrefab, transform);
        card.GetComponent<CardPrefab>().getBackByAttr(3);
        for (int i = 1; i <= 15; i++)
        {
            GameObject cardd = Instantiate(cardPrefab, transform);
            cardd.GetComponent<CardPrefab>().getBackByAttr(0);
        }
    }*/


    public static void LoadCards(int[] card_list)
    {
        getRecSelected();
        int j = 0;
        ChampionCardInformation ci = ChampionCardInformation.getInstance();

        for (int i = 0; i < card_list.Length; i++)
        {
            GameObject cardd = Instantiate(instanceCardPrefab, instanceTransform);
            cardd.GetComponent<CardPrefab>().getChampion(card_list[i]);
            if (i == selected[j])
            {
                cardd.GetComponent<CardPrefab>().selected = true;
                cardd.transform.GetChild(6).gameObject.SetActive(true);
                Debug.Log("First set true");
                if (j < selected.Length - 1)
                {
                    j++;
                }
            }
        }
        //GlobalGameData.getInstance().cards = card_list;
    }

    void Awake()
    {
        instanceCardPrefab = cardPrefab;
        instanceTransform = transform;
        GameObject f = transform.parent.parent.parent.gameObject;
        f.transform.GetChild(4).GetComponent<Text>().text = genInf2(min, max);
        f.transform.GetChild(5).GetComponent<Text>().text = genInf(1, 3);
        f.transform.GetChild(4).GetComponent<Text>().color = Color.yellow;
        f.transform.GetChild(5).GetComponent<Text>().color = Color.yellow;
        web = gameObject.AddComponent<WebWork>();
        web.Init();

    }

    // Start is called before the first frame update
    void Start()
    {
        //generateCards();
        web.SendGet(EOPERATION.GET_CARDS, UserInfoStorage.user_id.ToString());
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool checkNum()
    {
        GameObject f = transform.parent.parent.parent.gameObject;
        if (selectednum > max)
        {
            selectednum = max;
            f.transform.GetChild(5).GetComponent<Text>().text = genInf(selectednum, 0);
            f.transform.GetChild(5).GetComponent<Text>().color = Color.red;
            return false;
        }
        else if (selectednum < min)
        {
            selectednum = min;
            f.transform.GetChild(5).GetComponent<Text>().text = genInf(selectednum, max - selectednum);
            f.transform.GetChild(5).GetComponent<Text>().color = Color.red;
            return false;
        }
        else
        {
            f.transform.GetChild(5).GetComponent<Text>().text = genInf(selectednum, max - selectednum);
            f.transform.GetChild(5).GetComponent<Text>().color = Color.yellow;
            return true;
        }
    }

    private string genInf(int cur, int more)
    {
        return "您当前：\n已经选择了" + cur + "张卡牌\n还可以选择" + more + "张卡牌";
    }

    private string genInf2(int min, int max)
    {
        return "提示：\n您至少要选择" + min + "张卡牌\n最多可以选择" + max + "张卡牌";
    }

    public void recordSelected()
    {
        string s = "";
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            if (gameObject.transform.GetChild(i).GetComponent<CardPrefab>().selected)
            {
                s = s + "_" + i;
            }
        }
        PlayerPrefs.SetString(key_selected, s.Substring(1, s.Length - 1));
        getRecSelected();
    }

    public static void getRecSelected()
    {
        Debug.Log("I am diaoyong!");
        string s = "";
        s = PlayerPrefs.GetString(key_selected, "");
        if (s.Equals(""))
        {
            selected = new int[1] { 0 };
        }
        else
        {
            string[] ss = s.Split('_');
            selected = new int[ss.Length];

            for (int i = 0; i < ss.Length; i++)
            {
                //Debug.Log("ss:" + ss[i]);
                int num = 0;
                int.TryParse(ss[i], out num);
                selected[i] = num;
                Debug.Log(num);
                //selected[i] = ss[i][0] - '0';
                //Debug.Log(ss[i][0]);
            }
            selectednum = selected.Length;
        }
        GlobalGameData.getInstance().cards = selected;
    }
}
