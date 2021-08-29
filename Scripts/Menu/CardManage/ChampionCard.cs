using UnityEngine;

public class ChampionCard : MonoBehaviour
{

    void Awake()
    {
        gameObject.SetActive(false);
        Debug.Log("First set false");
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onClick()
    {
        CardMange f = transform.parent.parent.GetComponent<CardMange>();
        CardPrefab p = transform.parent.GetComponent<CardPrefab>();
        if (gameObject.activeSelf)
        {

            CardMange.selectednum--;
            if (!f.checkNum())
            {
                p.selected = true;
                return;
            }
            Debug.Log("Set False!");
            gameObject.SetActive(false);
        }
        else
        {
            CardMange.selectednum++;
            if (!f.checkNum())
            {
                p.selected = false;
                return;
            }
            gameObject.SetActive(true);
            Debug.Log("Set true!");
        }

    }

    public void hide()
    {
        gameObject.SetActive(false);
    }

    public void show()
    {
        gameObject.SetActive(true);
    }


}
