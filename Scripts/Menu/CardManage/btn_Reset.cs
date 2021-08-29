using UnityEngine;

public class btn_Reset : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void resetSelected()
    {
        GameObject content = transform.parent.GetChild(0).GetChild(0).GetChild(0).gameObject;
        Debug.Log(content.transform.childCount);
        for (int i = 0; i < content.transform.childCount; i++)
        {
            if (i <= content.GetComponent<CardMange>().min - 1)
            {
                content.transform.GetChild(i).GetChild(6).gameObject.GetComponent<ChampionCard>().show();
                content.transform.GetChild(i).gameObject.GetComponent<CardPrefab>().selected = true;
                continue;
            }
            content.transform.GetChild(i).GetChild(6).gameObject.GetComponent<ChampionCard>().hide();
            content.transform.GetChild(i).gameObject.GetComponent<CardPrefab>().selected = false;
            //content.GetComponent<CardMange>().selectednum = 0;
        }
        CardMange.selectednum = content.GetComponent<CardMange>().min;
        content.transform.GetComponent<CardMange>().checkNum();
    }
}