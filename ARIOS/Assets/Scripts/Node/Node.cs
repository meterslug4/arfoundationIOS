using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

[System.Serializable]
public class Node : MonoBehaviour
{
    public TMP_Text text;
    public Vector3 pos;
    public string nodeName;

    public List<RelatedData> relatedDataList = new List<RelatedData>();

    //���� ��忡�� ���� ���� �̵��ϴµ� ��� ����� ��������� id�� Ű���ؼ� ����
    public int nodeId;
    public float G = 0.0f;//���������� �ش���ǥ���� �̵��ϴµ� ��� ��� �������� ���� ��ο� ���� �޶���
    public float H = 0.0f;//���������� �󸶳� ������� �������� ����,������
    public float F;//���� ���� �������� ����
    public void SetNode(int id, Vector3 position)
    {
        nodeId = id;
        pos = position;
        transform.position = pos;
    }
    public void SetNode(int id, Vector3 position, float f)
    {
        nodeId = id;
        pos = position;
        transform.position = pos;
        F = f;
    }
    public void SetNodeName(string name)
    {
        nodeName = name;
    }
    public void SetH(Vector3 pos, Vector3 targetPos)
    {
        Vector3 v = targetPos - pos;
        H = v.sqrMagnitude;
    }
    public void ResetF(float f)
    {
        F = f;
    }
    public float SetLinkNodeCost(Vector3 pos, Vector3 targetPos)
    {
        return Vector3.Distance(targetPos, pos);
    }
    /// <summary>
    /// ��ǥ ������ ��� ���� �̵� ���
    /// </summary>
    /// <param name="goalPos"></param>
    /// <returns></returns>
    public float SetLinkNodeCost(Vector3 goalPos)
    {
        return Vector3.Distance(goalPos, this.transform.position);
    }
    //���� ��带 �������� �Ÿ� ��ʷ� �ӽ÷� ��� ����Ʈ ����
    //������ ����ɼ� ����
    public void ChangeBoxColliderSize(Vector3 vec)
    {
        BoxCollider bc = transform.GetComponent<BoxCollider>();
        bc.size = vec;
    }
    public void SetText(string s)
    {
        text.text = s;
    }

}