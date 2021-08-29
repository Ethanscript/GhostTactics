using UnityEngine;


public class btn_Insane : MonoBehaviour
{
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
        GameGlobalManager gameGlobalManager = new GameGlobalManager();
        gameGlobalManager.ChooseDifficulty(2);
    }
}
