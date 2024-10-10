using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour
{
    [SerializeField] protected NaviDemo naviDemo;
    public UIController uiController;
    public NaviArrow naviArrow;//화살표 표시용 오브젝트
    [SerializeField] protected Node currentNode;//현재 user가 있는 노드
    [SerializeField] protected Node nextNode; //다음으로 가야할 노드(최종 목적지X)
    protected List<Node> closeList = new List<Node>();//이미 방문한 노드 리스트
    protected Vector3 nextDir;//다음 노드의 방향
    protected bool isFinish = false;//최종 목적지에 도착했는지
    protected Queue<Node> desQue = new Queue<Node>();
    protected bool isNewdestList = false;//새 경로가 설정됬는지?
    protected bool searching = false;//탐색중인지?
    public GameObject guidObj;
    void Start()
    {
        naviDemo.delSetPath = SetNewPath;
    }
    private void Update()
    {
        //탐색후 부터 업데이트하며 시작 parentList가 0이면 시작안함
        //새로받은 겨올 리스트가 있는지 체크(isNewdestList)
        if (isNewdestList == true && searching == false)
        {
            searching = true;
            isNewdestList = false;
            //탐색
            StartCoroutine(Navigate());
        }
    }
    protected IEnumerator Navigate()
    {
        int cnt = desQue.Count;
        for (int i = 0; i < cnt; i++)
        {
            nextNode = desQue.Dequeue();
            //현재 노드에서 도착지까지 가야함
            naviArrow.SetDestination(nextNode.transform.position, true);
            guidObj.transform.position = nextNode.transform.position;
            yield return new WaitUntil(() => currentNode.nodeId == nextNode.nodeId);
            closeList.Add(nextNode);
            //currentNode = nextNode;
        }
        //모든 노드 방문 목적지 도착
        guidObj.SetActive(false);
        uiController.SetInfoMsg("Print");
        uiController.EnableMsgBox(true);
        //초기화
        searching = false;
        isNewdestList = false;
        naviArrow.setdest = false;//설정된 목적지 없으므로 false로 변경
        naviArrow.EnableArrow(false);
        closeList.Clear();
        //yield return null;
    }
    protected void SetNewPath(List<Node> parentList, bool setNewPath)
    {
        desQue.Clear();
        for (int i = 0; i < parentList.Count; i++)
        {
            desQue.Enqueue(parentList[i]);
        }
        isNewdestList = setNewPath; //새로운 경로 확인
    }

    //현재위치 -> 다음 목적지 ->도착 유뮤 ->현재위치 업데이트 ->다음 목적지
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Node>() != null)
        {
            currentNode = other.GetComponent<Node>();
            naviDemo.startNode = other.GetComponent<Node>();
        }
    }
}
