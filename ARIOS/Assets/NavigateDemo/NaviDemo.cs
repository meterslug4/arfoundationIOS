using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class NaviDemo : MonoBehaviour
{
    protected Camera mainCam;
    public GameObject agent; //3D 맵에서 움직여질 게임 오브젝트
    protected float yAxis;
    protected GameObject nodeRoot;
    public Button pathFindBtn;

    protected bool reFind = false;
    protected float defaultValue = 999999.0f;
    public delegate void DelSetPath(List<Node> prentList, bool setNewPath);
    public DelSetPath delSetPath;
    public Node startNode; //시작점이될 노드
    public Node endNode; //도착점이될 노드 
    public List<Node> nodeList = new List<Node>();
    public List<Node> parentList = new List<Node>();
    protected Dictionary<int, Node> dic_Open = new Dictionary<int, Node>();
    protected Dictionary<int, Node> dic_Close = new Dictionary<int, Node>();
    protected Dictionary<int, Node> dic_parent = new Dictionary<int, Node>();//시작점부터 도착점까지의 노드가 순서대로 
    public Dictionary<string, Node> dicDest = new Dictionary<string, Node>();
    private void Awake()
    {
        mainCam = Camera.main;
        nodeRoot = GameObject.Find("NodeRoot");
    }
    void Start()
    {
        pathFindBtn.onClick.AddListener(PathFind);
    }
    private void Update()
    {
        MoveAgent();
    }
    //처음 시작시에 바라보는 방향과 위치를 설정 
    protected void SetStartPosition()
    {

    }
    protected void MoveAgent()
    {
        Vector3 camPos = mainCam.transform.position;
        Vector3 agentPos = new Vector3(camPos.x, 0, camPos.z);
        agent.transform.position = agentPos;
        //Quaternion camRot = mainCam.transform.rotation;
        //yAxis = camRot.eulerAngles.y;
        //Vector3 userRot = agent.transform.eulerAngles;
        //userRot.y = yAxis;
        //agent.transform.eulerAngles = userRot;
    }

    protected void PathFind()
    {
        reFind = true;
        if (reFind)
        {
            ClearAll();
        }
        if (startNode != null)
        {
            startNode.SetH(startNode.transform.position, endNode.transform.position);
            InsertQueue(startNode);
        }
        if (!dic_parent.ContainsKey(startNode.nodeId))
        {
            Debug.Log("시작 노드 추가");
            dic_parent.Add(startNode.nodeId, startNode);
        }

        while (nodeList.Count > 0)
        {
            Node node = DeQueue();
            if (!dic_parent.ContainsKey(node.nodeId))
            {
                Debug.Log(node.nodeId);
                dic_parent.Add(node.nodeId, node);
            }
            for (int i = 0; i < node.relatedDataList.Count; i++)
            {
                if (!dic_Open.ContainsKey(node.relatedDataList[i].adjacentNodes.nodeId))
                {
                    dic_Open.Add(node.relatedDataList[i].adjacentNodes.nodeId,
                        node.relatedDataList[i].adjacentNodes);
                }
            }
            if (dic_Close.ContainsKey(node.nodeId))
            {
                continue;
            }
            dic_Close.Add(node.nodeId, node);
            //목적지에 도착했으면 종료
            if (node.pos == endNode.pos)
            {
                Debug.Log("목적지 도착");
                break;
            }
            for (int i = 0; i < node.relatedDataList.Count; i++)
            {
                int nextNodeId = node.relatedDataList[i].adjacentNodes.nodeId;
                float G = node.G + node.relatedDataList[i].g;
                Vector3 v = endNode.pos - node.relatedDataList[i].adjacentNodes.pos;
                float sqr = v.sqrMagnitude;
                float H = sqr;
                dic_Open[nextNodeId].H = H;//인접노드의 휴리스틱 값 변경
                if (dic_Open[nextNodeId].F < G + H)
                {
                    continue;
                }
                dic_Open[nextNodeId].F = G + H;
                dic_Open[nextNodeId].G = G;
                InsertQueue(dic_Open[nextNodeId]);
            }
        }
        //시작노부터 엔드노드까지 그려주기
        foreach (var v in dic_parent.Values)
        {
            Debug.Log(v);
            MeshRenderer mesh = v.gameObject.GetComponent<MeshRenderer>();
            if (v.nodeId == startNode.nodeId)
            {
                mesh.material.color = Color.red;
            }
            if (v.nodeId == endNode.nodeId)
            {
                mesh.material.color = Color.blue;
            }
            if (v.nodeId != startNode.nodeId && v.nodeId != endNode.nodeId)
            {
                mesh.material.color = Color.green;
            }
            parentList.Add(v);
        }
        delSetPath.Invoke(parentList, true);
    }

    protected void InsertQueue(Node node)
    {
        nodeList.Add(node);//추가
        int now = nodeList.Count - 1;
        while (now > 0)//now == 0일떄는 리스트에 갯수가 1개만 있으니 비교할필요가 없다 now가 1일때부터 값을 비교
        {
            int next = (now - 1) / 2; //자신의 부모노드의 index값이 다음에 비교한 index값이 된다
            if (nodeList[now].F > nodeList[next].F)
            {
                break;
            }
            Node temp = nodeList[now];
            nodeList[now] = nodeList[next];
            nodeList[next] = temp;
            now = next;
        }
    }
    //F값을 비교해서 더 가장 작은것을 우선순위로 빼옴
    protected Node DeQueue()
    {
        Node returnValue = nodeList[0];//가장 상위노드를 반환
        int lastIndex = nodeList.Count - 1;//최상위 루트로 이동시킬 마지막 인덱스
        nodeList[0] = nodeList[lastIndex];//마지막 값을 root로 이동
        nodeList.RemoveAt(lastIndex);//마지막 값은 제거
        lastIndex -= 1;//마지막 값이 사라졌으면 lastIndex값도 하나 줄인다.
        int nowIndex = 0; //마지막 노드가 0번 index로 이동했으므로 now를 0으로 설정
        while (true)
        {
            //nowIndex의 값과 childnode의 값을 비교한다.
            int leftChildIndex = 2 * nowIndex + 1;//왼쪽 노드
            int rightChildIndex = 2 * nowIndex + 2;//오른쪽 노드

            //nextIndex는 비교대상이다.
            int nextIndex = nowIndex; //nextIndex를 변화해가면서 계속 비교해나간다

            if (leftChildIndex <= lastIndex && nodeList[nextIndex].F > nodeList[leftChildIndex].F)
            {
                nextIndex = leftChildIndex; //leftIndex의 값보다 작으면 nextIndex를 일단 leftIndex로 변경
            }
            if (rightChildIndex <= lastIndex && nodeList[nextIndex].F > nodeList[rightChildIndex].F)
            {
                nextIndex = rightChildIndex;
            }
            if (nextIndex == nowIndex)
            {
                break;
            }
            Node temp = nodeList[nowIndex];
            nodeList[nowIndex] = nodeList[nextIndex];
            nodeList[nextIndex] = temp;
            nowIndex = nextIndex;
        }
        return returnValue;
    }
    protected void ClearAll()
    {
        nodeList.Clear();
        parentList.Clear();
        dic_Close.Clear();
        dic_parent.Clear();
        foreach (var v in dic_Open.Values)
        {
            //오픈리스트에 추가되어 있던 F값을 초기값으로 돌린다
            v.ResetF(defaultValue);
        }
    }
}
