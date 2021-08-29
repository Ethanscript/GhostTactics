using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LobbyUIController : MonoBehaviour
{
    public static string RoomNameString = "";

    static GameObject LobbyEntry;

    TMP_InputField RoomName;

    void Awake()
    {
        LobbyEntry = GameObject.Find("Canvas/LobbyEntry");
        RoomName = LobbyEntry.transform.Find("RoomName").GetComponent<TMP_InputField>();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetPlayerName(string value)
    {
        // #Important
        RoomNameString = RoomName.text;
    }
}
