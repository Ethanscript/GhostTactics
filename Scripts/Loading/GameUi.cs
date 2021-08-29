using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUi : MonoBehaviour
{
    [SerializeField] RectTransform FxHolder;
    [SerializeField] Image CircleImg;
    [SerializeField] Text txtProgress;
    [SerializeField] [Range(0, 1)] float progress = 0f;



    // Update is called once per frame
    void Update()
    {
        CircleImg.fillAmount = progress;
        txtProgress.text = Mathf.Floor(progress * 100).ToString();
        FxHolder.rotation = Quaternion.Euler(new Vector3(0f, 0f, -progress * 360));
    }
}
