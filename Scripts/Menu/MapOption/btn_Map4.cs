using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class btn_Map4 : MonoBehaviour
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
        gameGlobalManager.ChooseMap(4);
    }
}
