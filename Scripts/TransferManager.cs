using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TransferManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TransferOpponentHp(int hp)
    {
        photonView.RPC("hpByRPC", RpcTarget.Others, hp);
    }

    public void TransferTimer(float time)
    {
        photonView.RPC("timerByRPC", RpcTarget.Others, time);
    }

    public void TransferGameStage(GameStage stage)
    {
        photonView.RPC("gameStageByRPC", RpcTarget.Others, stage);
    }

    public void callSlaveOnGameStageComplate()
    {
        photonView.RPC("callSlaveOnGameStageComplateByRPC", RpcTarget.Others);
    }

    public void callMasterForLeavingRoom()
    {
        photonView.RPC("prepareLeavingRoomByRPC", RpcTarget.Others);
    }

    public void callRemoteRestart()
    {
        photonView.RPC("restartByRPC", RpcTarget.Others);
    }

    [PunRPC]
    void hpByRPC(int hp)
    {
        int oldHP = MultiGamePlayController.Instance.currentHP;
        MultiGamePlayController.Instance.currentHP = hp;
        //Debug.Log("reciveHp: " + MultiGamePlayController.Instance.currentHP);
    }

    [PunRPC]
    void timerByRPC(float time)
    {
        MultiGamePlayController.Instance.timer = time;
        //Debug.Log("reciveTimer: " + MultiGamePlayController.Instance.timer);
    }

    [PunRPC]
    void gameStageByRPC(GameStage stage)
    {
        MultiGamePlayController.Instance.currentGameStage = stage;
    }

    [PunRPC]
    void callSlaveOnGameStageComplateByRPC()
    {
        MultiGamePlayController.Instance.OnGameStageComplate();
    }

    [PunRPC]
    void prepareLeavingRoomByRPC()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Scenes/Room");
        }
    }

    [PunRPC]
    void restartByRPC()
    {
        // MultiGamePlayController.Instance.RestartGame();
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Scenes/TestMultiScene");
        }
    }
}
