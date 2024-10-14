using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIController : MonoBehaviour
{
    public Image InfoImage;
    public TMP_Text infoText; //알림 역할 메세지
    public TMP_Text imageInfoText; //이미지 인식 디버깅요 메세지
    protected string infoMsg;
    protected string imgInfoMsg;
    void Start()
    {
        InfoImage.gameObject.SetActive(false);
    }
    public void SetInfoMsg(string msg)
    {
        infoMsg = msg;
    }
    public void EnableMsgBox(bool on)
    {
        InfoImage.gameObject.SetActive(on);
        infoText.text = infoMsg;
    }
    public void SetImgInfoMsg(string msg)
    {
        imgInfoMsg = msg;
        imageInfoText.text = imgInfoMsg;
    }

    void Update()
    {
        
    }
}
