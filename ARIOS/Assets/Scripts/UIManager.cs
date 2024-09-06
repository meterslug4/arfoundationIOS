using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIManager : MonoBehaviour
{
    //목적지 선택 드롭다운
    //미니맵
    //길찾기 버튼
    public Button findPathBtn;
    public TMP_Dropdown dropdownMenu;
    public Demo demo;
    public GameObject arArrow; //ar화면에서 보일 화살표
    public GameObject arrow; //미니맵에서만 보일 화살표

    private void Start()
    {
        
    }
    public void SetDropdownMenu(List<string> dropdownList)
    {
        dropdownMenu.ClearOptions();
        dropdownMenu.AddOptions(dropdownList);
        dropdownMenu.onValueChanged.AddListener(DropdownValueChanged);
    }
    void DropdownValueChanged(int index)
    {
        string selected = dropdownMenu.options[index].text;
        switch (selected)
        {
            case CONST.NOT:
                demo.endNode = null;
                break;
            case CONST.YOON:
                demo.endNode = demo.dicDest[CONST.YOON];
                break;
            case CONST.JEON:
                demo.endNode = demo.dicDest[CONST.JEON];
                break;
            case CONST.CONFERENCE1:
                demo.endNode = demo.dicDest[CONST.CONFERENCE1];
                break;
            case CONST.EXIT:
                demo.endNode = demo.dicDest[CONST.EXIT];
                break;
            case CONST.CONFERENCE2:
                demo.endNode = demo.dicDest[CONST.CONFERENCE2];
                break;
        }
    }
}
