using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIController : MonoBehaviour
{
    public Image InfoImage;
    public TMP_Text infoText;
    protected string infoMsg;
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

    void Update()
    {
        
    }
}
