using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TMP_Text roomName = GameObject.Find("Canvas/RoomPanel/RoomName").transform.GetComponent<TMP_Text>();
            roomName.text = "RN: " + PhotonNetwork.CurrentRoom.Name;

            TMP_Text playerName = GameObject.Find("Canvas/RoomPanel/Player1/Player1Name").transform.GetComponent<TMP_Text>();
            playerName.text = "Player1: " + PhotonNetwork.NickName;

            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                TMP_Text player2Name = GameObject.Find("Canvas/RoomPanel/Player2/Player2Name").transform.GetComponent<TMP_Text>();
                player2Name.text = "Player2: " + PhotonNetwork.PlayerList[1].NickName;
                TMP_Text stateText = GameObject.Find("Canvas/RoomPanel/StateText").transform.GetComponent<TMP_Text>();
                stateText.text = "Ready!";
            }
        }
        else
        {
            TMP_Text roomName = GameObject.Find("Canvas/RoomPanel/RoomName").transform.GetComponent<TMP_Text>();
            roomName.text = "RN: " + PhotonNetwork.CurrentRoom.Name;

            GameObject.Find("Canvas/RoomPanel/StartButton").SetActive(false);

            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                TMP_Text player1Name = GameObject.Find("Canvas/RoomPanel/Player1/Player1Name").transform.GetComponent<TMP_Text>();
                player1Name.text = "Player1: " + PhotonNetwork.PlayerList[0].NickName;
                TMP_Text player2Name = GameObject.Find("Canvas/RoomPanel/Player2/Player2Name").transform.GetComponent<TMP_Text>();
                player2Name.text = "Player2: " + PhotonNetwork.PlayerList[1].NickName;
                TMP_Text stateText = GameObject.Find("Canvas/RoomPanel/StateText").transform.GetComponent<TMP_Text>();
                stateText.text = "Ready!";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Photon Callbacks


    /// <summary>
    /// Called when the local player left the room. We need to load the launcher scene.
    /// </summary>
    public override void OnLeftRoom()
    {
        GameObject.Find("Canvas").SetActive(false);
        AsyncOperation op = SceneManager.LoadSceneAsync("Scenes/Lobby");
        op.allowSceneActivation = true;
    }

    public override void OnPlayerEnteredRoom(Player other)
    {
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerEnteredRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
            {
                GameObject.Find("Canvas/RoomPanel/StartButton").SetActive(true);
                TMP_Text player1Name = GameObject.Find("Canvas/RoomPanel/Player1/Player1Name").transform.GetComponent<TMP_Text>();
                player1Name.text = "Player1: " + PhotonNetwork.NickName;
                TMP_Text player2Name = GameObject.Find("Canvas/RoomPanel/Player2/Player2Name").transform.GetComponent<TMP_Text>();
                player2Name.text = "Player2: " + other.NickName;
                TMP_Text stateText = GameObject.Find("Canvas/RoomPanel/StateText").transform.GetComponent<TMP_Text>();
                stateText.text = "Ready!";
            }

            // LoadArena();
        }
    }


    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects


        if (PhotonNetwork.IsMasterClient)
        {
            Debug.LogFormat("OnPlayerLeftRoom IsMasterClient {0}", PhotonNetwork.IsMasterClient); // called before OnPlayerLeftRoom

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                GameObject.Find("Canvas/RoomPanel/StartButton").SetActive(true);
                TMP_Text player1Name = GameObject.Find("Canvas/RoomPanel/Player1/Player1Name").transform.GetComponent<TMP_Text>();
                player1Name.text = "Player1: " + PhotonNetwork.NickName;
                TMP_Text player2Name = GameObject.Find("Canvas/RoomPanel/Player2/Player2Name").transform.GetComponent<TMP_Text>();
                player2Name.text = "Player2: ???";
                TMP_Text stateText = GameObject.Find("Canvas/RoomPanel/StateText").transform.GetComponent<TMP_Text>();
                stateText.text = "Waiting...";
            }

            // LoadArena();
        }
    }


    #endregion


    #region Public Methods


    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void LoadArena()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
        }
        Debug.LogFormat("PhotonNetwork : Loading Level : {0}", PhotonNetwork.CurrentRoom.PlayerCount);
        PhotonNetwork.LoadLevel("Room for " + PhotonNetwork.CurrentRoom.PlayerCount);
    }

    public void StartGame()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1 || !PhotonNetwork.IsMasterClient)
        {
            return;
        }
        //get random map for multigame
        int mapLevel = Random.Range(1, 6);
        PhotonNetwork.LoadLevel("Scenes/Multi_Level" + mapLevel.ToString());
    }


    #endregion
}
