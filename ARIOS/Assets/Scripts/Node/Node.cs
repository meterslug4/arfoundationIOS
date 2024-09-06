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

    //현재 노드에서 인접 노드로 이동하는데 드는 비용을 인접노드의 id를 키로해서 저장
    public int nodeId;
    public float G = 0.0f;//시작점에서 해당좌표까지 이동하는데 드는 비용 작을수록 좋음 경로에 따라 달라짐
    public float H = 0.0f;//목적지에서 얼마나 가까운지 작을수록 좋음,고정값
    public float F;//최종 점수 작을수록 좋음
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
    /// 목표 지점과 노드 간의 이동 비용
    /// </summary>
    /// <param name="goalPos"></param>
    /// <returns></returns>
    public float SetLinkNodeCost(Vector3 goalPos)
    {
        return Vector3.Distance(goalPos, this.transform.position);
    }
    //현재 노드를 기준으로 거리 비례로 임시로 노드 리스트 정리
    //기준은 변경될수 있음
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