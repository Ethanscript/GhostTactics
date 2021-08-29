using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controlls player input
/// </summary>
public class InputController : MonoBehaviour
{
    public GamePlayController gamePlayController;
    public Camera gameCamera;

    //map script
    public Map map;


    public LayerMask triggerLayer;

    //declare ray starting position var
    private Vector3 rayCastStartPosition;

    // Start is called before the first frame update
    void Start()
    {
        //set position of ray starting point to trigger objects
        rayCastStartPosition = new Vector3(0, 20, 0);
    }

    //to store mouse position
    private Vector3 mousePosition;

    
    [HideInInspector]
    public TriggerInfo triggerInfo = null;

    /// Update is called once per frame
    void Update()
    {
        triggerInfo = null;
        map.resetIndicators();

        //declare rayhit
        RaycastHit hit;
        Ray ray;

        //convert mouse screen position to ray
        if (PhotonNetwork.IsConnected)
        {
            ray = gameCamera.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        

        //Debug.Log("MousePosition: " + Input.mousePosition);
        //Debug.Log("Ray: " + ray.direction);

        //if ray hits something
        if (Physics.Raycast(ray, out hit, 100f, triggerLayer, QueryTriggerInteraction.Collide))
        {
            //Debug.Log("Hit: " + hit.transform.position);
;
            //get trigger info of the  hited object
            triggerInfo = hit.collider.gameObject.GetComponent<TriggerInfo>();

            //this is a trigger
            if(triggerInfo != null)
            {
                //get indicator
                GameObject indicator = map.GetIndicatorFromTriggerInfo(triggerInfo);

                //set indicator color to active
                indicator.GetComponent<MeshRenderer>().material.color = map.indicatorActiveColor;

                Debug.Log("triggerInfo: " + triggerInfo);
            }
            else
                map.resetIndicators(); //reset colors
        }
               

        if (Input.GetMouseButtonDown(0))
        {
            if (PhotonNetwork.IsConnected)
                MultiGamePlayController.Instance.StartDrag();
            else
                GamePlayController.Instance.StartDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (PhotonNetwork.IsConnected)
                MultiGamePlayController.Instance.StopDrag();
            else
                GamePlayController.Instance.StopDrag();
        }

        //store mouse position
        mousePosition = Input.mousePosition;
    }
}
