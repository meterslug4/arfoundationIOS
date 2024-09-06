using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
public class OfficeDemo : MonoBehaviour
{
    public delegate void DelSetPath(List<Node> prentList, bool setNewPath);
    public DelSetPath delSetPath;
    [SerializeField] protected RawImage miniMap;
    [SerializeField] protected GameObject user;
    [SerializeField] protected Dropdown dropdownMenu; //목적지 리스트
    protected Camera mainCamera;
    protected float yAxis;
    protected Dictionary<Destination, (string, Node)> dicDest = new Dictionary<Destination, (string, Node)>();
    [SerializeField]protected List<Node> destNodeList = new List<Node>();
    protected List<string> dropdownList = new List<string>();
    [SerializeField]protected Button findPathBtn;
    public List<Node> nodeList = new List<Node>();
    public List<Node> parentList = new List<Node>();
    //방문 했는지 여부 방문 = closeed 상태
    protected Dictionary<int, Node> dic_Close = new Dictionary<int, Node>();
    //길을 한번이라도 방문했는지? 발문한적 없으면 delfaultvalue로 방문한적있으면 => f=g+h
    protected Dictionary<int, Node> dic_Open = new Dictionary<int, Node>();
    protected Dictionary<int, Node> dic_parent = new Dictionary<int, Node>();//시작점부터 도착점까지의 노드가 순서대로 
    public Node startNode; //시작점이될 노드
    public Node endNode; //도착점이될 노드 


    //경로가 바뀌거나 재탐색인 경우 2번이상 연속 실행인경우 현재위치에서 다시 경로 계산하도록해야함
    public bool reFind = false;
    protected float defaultValue = 999999.0f;

    private void Awake()
    {
        //드롭다운 메뉴 설정
        mainCamera = Camera.main;
        //데모에 필요한값의이로 넣음
        dicDest.Add(Destination.DES1, ("윤태환", destNodeList[0]));
        dicDest.Add(Destination.DES2, ("전준표", destNodeList[1]));
        dicDest.Add(Destination.DES3, ("큰회의실", destNodeList[2]));
        dicDest.Add(Destination.DES4, ("출입구", destNodeList[3]));
        dicDest.Add(Destination.DES5, ("작은회의실", destNodeList[4]));

        dropdownList.Add("선택 안함");
        foreach(var v in dicDest.Values)
        {
            dropdownList.Add(v.Item1);
        }
    }
    void Start()
    {
        dropdownMenu.ClearOptions();
        dropdownMenu.AddOptions(dropdownList);
        dropdownMenu.onValueChanged.AddListener(delegate { DropdownValueChanged((Destination)dropdownMenu.value); });
        dropdownMenu.value = (int)Destination.NOTHING;

        findPathBtn.onClick.AddListener(PathFind);
    }
    void Update()
    {
        //위치가 튀는걸 대비 순수 이동거리만 필요
        //ardrive tracking ?
        Vector3 userPos = new Vector3(mainCamera.transform.position.x, 0, mainCamera.transform.position.z);
        user.transform.position = userPos;
        Quaternion cameraRot = mainCamera.transform.rotation;
        yAxis = cameraRot.eulerAngles.y;
        Vector3 userRot = user.transform.eulerAngles;
        userRot.y = yAxis;
        user.transform.eulerAngles = userRot;
    }
    //목적지 선택
    void DropdownValueChanged(Destination dest)
    {
        switch (dest)
        {
            case Destination.NOTHING:
                Debug.Log("선택 안함");
                endNode = null;
                break;
            case Destination.DES1:
                Debug.Log("목적지 : 윤태환");
                endNode = dicDest[Destination.DES1].Item2;
                break;
            case Destination.DES2:
                Debug.Log("목적지 : 전준표");
                endNode = dicDest[Destination.DES2].Item2;
                break;
            case Destination.DES3:
                Debug.Log("목적지 : 큰회의실");
                endNode = dicDest[Destination.DES3].Item2;
                break;
            case Destination.DES4:
                Debug.Log("목적지 : 출입구");
                endNode = dicDest[Destination.DES4].Item2;
                break;
            case Destination.DES5:
                Debug.Log("목적지 : 작은회의실");
                endNode = dicDest[Destination.DES5].Item2;
                break;
        }
    }


    protected void ClearAll()
    {
        nodeList.Clear();
        parentList.Clear();
        dic_Close.Clear();
        dic_parent.Clear();
        //이미 기존에 한번 경로 탐색으로인해 각 노드들의 F값은 변경되어 있는상태
        //오픈리스트는 노드맵 구성시에 생성하므로 비우지는않고 필요한 값만 초기화한다
        //dic_Open의 F값은 전부 다시 처음 상태로 초기화
        foreach (var v in dic_Open.Values)
        {
            //오픈리스트에 추가되어 있던 F값을 초기값으로 돌린다
            v.ResetF(defaultValue);
        }
    }
    protected void PathFind()
    {
        reFind = true;
        if (reFind)
        {
            ClearAll();
        }
        //추가 사항
        //startNode가 null일때 반경안에서 제일 가까운 노드를 찾은후에 제일 가까운 노드를 기준으로
        //길찾기를 시행하고 노드가 없는 현재 위치에서 startNode로 가도록 direction을 설정한다
        //노드가 없는 예외 상황에서 노드 없이도 돌아가도록 처리해주기위함
        if (startNode != null)
        {
            //startNode의 H값
            //startNode에서 목적지 까지의 거리
            startNode.SetH(startNode.transform.position, endNode.transform.position);

            //우선순위 큐를 통해서 최적의 값을 뽑는다.
            //우선순위 큐에 노드정보를 넣어서 트리를 구성한다
            Debug.Log("Insert PQ");
            InsertQueue(startNode);
        }

        //가장 적절한것만 탐색해서 저장하는 딕셔너리 순서대로 연결하면 찾은길이된다
        //처음에는 startNode에서 시작하니 startNode는 루프를 돌기전에  넣어주고 시작
        if (!dic_parent.ContainsKey(startNode.nodeId))
        {
            Debug.Log("시작 노드 추가");
            dic_parent.Add(startNode.nodeId, startNode);
        }

        while (nodeList.Count > 0)
        {
            //제일 좋은 후보를 찾는다
            Node node = DeQueue();
            if (!dic_parent.ContainsKey(node.nodeId))
            {
                Debug.Log(node.nodeId);
                dic_parent.Add(node.nodeId, node);
            }
            //꺼내온 노드의 인접 노드들을 dic_Open에 초기화 해서 넣어놓는다
            //현재 노드에서 다음에 방문할 후보군들 세팅
            for (int i = 0; i < node.relatedDataList.Count; i++)
            {
                if (!dic_Open.ContainsKey(node.relatedDataList[i].adjacentNodes.nodeId))
                {
                    dic_Open.Add(node.relatedDataList[i].adjacentNodes.nodeId,
                        node.relatedDataList[i].adjacentNodes);
                }
            }
            Debug.Log(string.Format("{0}가 가장 비용이 적은 값이다 꺼내온다---", node));
            //동일한 좌표를 여러 경로로 찾아서 더빠른 경우를 찾아 이미 방문된경우(close 된 경우) 스킵
            //어떤 노드인지 알아서 index에 접근할수 있어야함
            if (dic_Close.ContainsKey(node.nodeId))
            {
                //이미 close dictionary에 추가된 값이라면? 스킵
                //Debug.Log(string.Format("{0}은 이미 방문한곳이다 넘어간다",node));
                continue;
            }
            //방문한적이 없다? ->close dic에 추가해서 방문한것 체크 다음번에 방문안하도록
            dic_Close.Add(node.nodeId, node);
            //목적지에 도착했으면 종료
            if (node.pos == endNode.pos)
            {
                Debug.Log("목적지 도착");
                break;
            }
            //이동할수 있는 노드인지 확인해서 예약(open)
            //현재 이웃노드는 전부 이동가능한곳(벽으로 막힌곳 없음) ,방문했는지 안했는지는 모름
            //node의 비용 계산

            //인접노드와의 비용은 이미 계산되어 있음

            //인접한 이동가능한 노드들에 관하여 먼저 처리
            for (int i = 0; i < node.relatedDataList.Count; i++)
            {
                //string s1= string.Format("{0}가 다음 으로 이동할수있는 후보군이다", node.linkNodeList[i]);
                //Debug.Log(s1);
                //이동 가능한 인접 노드들의 대한 비용 계산
                //새로운 길에 대한 값 계산
                //인접한 노드의 id 다음번에 갈 노드의 id가된다
                int nextNodeId = node.relatedDataList[i].adjacentNodes.nodeId;
                //현재 노드에서 다음 노드로 이동하는데 드는 비용(거리)
                float G = node.G + node.relatedDataList[i].g;
                //노드의 휴리스틱 값
                //다음 인접 노드에서 목적지까지의 휴리스틱 값
                Vector3 v = endNode.pos - node.relatedDataList[i].adjacentNodes.pos;
                float sqr = v.sqrMagnitude;
                float H = sqr;
                dic_Open[nextNodeId].H = H;//인접노드의 휴리스틱 값 변경

                //다음에 이동가능한 인접노드의 F(G+H)값이 다른경로에서 발견했으면 스킵
                //dic_Open의 기본값은 max value 이기떄문에 아직 찾은적이 없다면 무조건 크다 
                //작다면 이미 방문하여 뭔가 값이 들어가있는 경우이고 작을수록 비용이 적은것이기에
                //더 작은 값이 들어오는것아니면 스킵한다(작은것 우선순위)
                if (dic_Open[nextNodeId].F < G + H)
                {
                    //string s2 = string.Format("{0}까지의 이동 비용은 기존보다 작지 않다 의미없음 넘어간다", node.linkNodeList[i]);
                    //Debug.Log(s2);
                    continue;
                }
                //위에 경우에 안걸리면 가장 좋은 경우이므로 open에 값을 변경한다
                dic_Open[nextNodeId].F = G + H;
                dic_Open[nextNodeId].G = G;
                //string s3 = string.Format("{0}의 F값을 변경 적절한값 {1}", dic_Open[nextNodeId],
                //    dic_Open[nextNodeId].F);
                //Debug.Log(s3);
                //우선순위 큐에도 넣어줌
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
        //탐색된 경로들인 parentList와 새로운 경로가 탐색되어 값을 주었을때  = ture
        delSetPath.Invoke(parentList, true);
    }
    protected void InsertQueue(Node node)
    {
        nodeList.Add(node);//추가
        //마지막에 추가된값부터 비교해 나간다.
        //count  1,2,3...
        //now    0,1,2...
        int now = nodeList.Count - 1;
        while (now > 0)//now == 0일떄는 리스트에 갯수가 1개만 있으니 비교할필요가 없다 now가 1일때부터 값을 비교
        {
            //now의 부모 노드와 비교를해야함
            //now 노드의 부모 노드를 알아야함
            //다음 비교 대상은 now의 부모 노드가됨
            int next = (now - 1) / 2; //자신의 부모노드의 index값이 다음에 비교한 index값이 된다
            //now index의 값이 next index의 값보다 작으면
            if (nodeList[now].F > nodeList[next].F)
            {
                //교환할필요 없이 루프 종료
                break;
            }
            //값 교환
            Node temp = nodeList[now];
            nodeList[now] = nodeList[next];
            nodeList[next] = temp;
            //값 교환후 now index를 값이 변경된 next index값으로 변경하고
            //계속해서 루프를 벗어날때까지
            //값을 교환해 나감
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

        //가장 작은값 혹은 가장큰값이 root 자리로 오도록 기준에 맞게 다시 정렬
        //root (nodeList[0])만 뽑으면되도록
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
            //왼쪽값과 비교를하더라도 오른쪽값과도 비교를한다 
            //nextIndex가 이미 left로 변경된상태라하더라도 right와 한번더 비교한다.
            if (rightChildIndex <= lastIndex && nodeList[nextIndex].F > nodeList[rightChildIndex].F)
            {
                nextIndex = rightChildIndex;
            }
            //leftChildIndex rightChildIndex와 비교를해봤지만 위에 if문에서 걸리지 않고
            //nextIndex가 현재 nowIndex에서 변경이 없는경우는 두개의 child노드 모두 더 작은 값이니
            //값을 교환할 필요가겂다 => while문 빠져나옴
            if (nextIndex == nowIndex)
            {
                //더이상 nextIndex에 변경이 일어나지않고 child노드가 없거나 자신보다 작은값이
                //없을경우에는 노드의 재배치가 끝난것이므로 while문 종료
                break;
            }
            //while을 빠져나온게 아니라면 left나 righrt중에 변경이생겨서 nextIndex에 변경이 있음
            //값을 교환해줌
            Node temp = nodeList[nowIndex];
            nodeList[nowIndex] = nodeList[nextIndex];
            nodeList[nextIndex] = temp;
            //값 교환후에 nowIndex를 교환한 nextIndex로 변경해야
            //다시 나의 child node와 비교하면서 검사를 실행하니 nowIndex값을 nextIndex값으로 변경한다.
            nowIndex = nextIndex;
        }
        //맨처음 저장해두었던 root의 값은 정상적으로 반환
        return returnValue;
    }

}
public enum Destination
{
    NOTHING =0,
    DES1= 1,
    DES2 =2,
    DES3 = 3,
    DES4 = 4,
    DES5 =5,
}

