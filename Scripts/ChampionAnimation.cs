using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Controls champion animations
/// </summary>
public class ChampionAnimation : MonoBehaviourPunCallbacks, IPunObservable
{

    private GameObject characterModel;
    public Animator animator;
    private ChampionController championController;
    public AudioSource attackAudio;
    public AudioClip attackSelections;
    private Vector3 lastFramePosition;
    /// Start is called before the first frame update
    void Start()
    {
        //get character model
        characterModel = this.transform.Find("character").gameObject;

        //get animator
        animator = characterModel.GetComponent<Animator>();
        championController = this.transform.GetComponent<ChampionController>();
    }

    /// Update is called once per frame
    void Update()
    {
        //never undate others champions
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        //calculate speed
        float movementSpeed = (this.transform.position - lastFramePosition).magnitude / Time.deltaTime;

        //set movement speed on animator controller
        animator.SetFloat("movementSpeed", movementSpeed);

        //store last frame position
        lastFramePosition = this.transform.position;
    }

    /// <summary>
    /// tells animation to attack or stop attacking
    /// </summary>
    /// <param name="b"></param>
    public void DoAttack(bool b)
    {
        animator.SetBool("isAttacking", b);
        attackAudio.clip = attackSelections;
        if (!attackAudio.isPlaying)
        {
            attackAudio.Play();
        }

    }

    /// <summary>
    /// Called when attack animation finished
    /// </summary>
    public void OnAttackAnimationFinished()
    {
        //never undate others champions
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        {
            return;
        }
        animator.SetBool("isAttacking", false);

        championController.OnAttackAnimationFinished();

        //Debug.Log("OnAttackAnimationFinished");

    }

    /// <summary>
    /// sets animation state
    /// </summary>
    /// <returns></returns>
    public void IsAnimated(bool b)
    {
        animator.enabled = b;
    }

    #region IPunObservable implementation
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            // We own this player: send the others our data
            // stream.SendNext(animator);
        }
        else
        {
            // Network player, receive data
            // this.animator = (Animator)stream.ReceiveNext();
        }
    }
    #endregion
}
