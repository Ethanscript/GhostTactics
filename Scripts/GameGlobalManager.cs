using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameGlobalManager : MonoBehaviour
{
    [HideInInspector]
    private int difficultyCode;
    private GlobalGameData globalData;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Test Load Level1
    public void StartGame()
    {
        //GameObject.DontDestroyOnLoad(transform.gameObject);
        SceneManager.LoadScene("Level1");
    }

    //Choose difficulty
    public void ChooseDifficulty(int difficulty)
    {
        if(difficulty < 0 || difficulty >= 3)
        {
            difficulty = 1;
        }
        //saved the chosen difficulty to the globalData
        GlobalGameData.getInstance().difficultyCode = difficulty;
    }

    //Choose map
    public void ChooseMap(int level)
    {
        //jump to the chosen map
        if (level <= 0 || level > 5)
        {
            level = 1;
        }
        string map = "Level" + level.ToString();
        SceneManager.LoadScene(map);
    }
}
