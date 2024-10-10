using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavAgent : MonoBehaviour
{
    [SerializeField] protected NaviDemo naviDemo;
    public UIController uiController;
    public NaviArrow naviArrow;//ȭ��ǥ ǥ�ÿ� ������Ʈ
    [SerializeField] protected Node currentNode;//���� user�� �ִ� ���
    [SerializeField] protected Node nextNode; //�������� ������ ���(���� ������X)
    protected List<Node> closeList = new List<Node>();//�̹� �湮�� ��� ����Ʈ
    protected Vector3 nextDir;//���� ����� ����
    protected bool isFinish = false;//���� �������� �����ߴ���
    protected Queue<Node> desQue = new Queue<Node>();
    protected bool isNewdestList = false;//�� ��ΰ� ���������?
    protected bool searching = false;//Ž��������?
    public GameObject guidObj;
    void Start()
    {
        naviDemo.delSetPath = SetNewPath;
    }
    private void Update()
    {
        //Ž���� ���� ������Ʈ�ϸ� ���� parentList�� 0�̸� ���۾���
        //���ι��� �ܿ� ����Ʈ�� �ִ��� üũ(isNewdestList)
        if (isNewdestList == true && searching == false)
        {
            searching = true;
            isNewdestList = false;
            //Ž��
            StartCoroutine(Navigate());
        }
    }
    protected IEnumerator Navigate()
    {
        int cnt = desQue.Count;
        for (int i = 0; i < cnt; i++)
        {
            nextNode = desQue.Dequeue();
            //���� ��忡�� ���������� ������
            naviArrow.SetDestination(nextNode.transform.position, true);
            guidObj.transform.position = nextNode.transform.position;
            yield return new WaitUntil(() => currentNode.nodeId == nextNode.nodeId);
            closeList.Add(nextNode);
            //currentNode = nextNode;
        }
        //��� ��� �湮 ������ ����
        guidObj.SetActive(false);
        uiController.SetInfoMsg("Print");
        uiController.EnableMsgBox(true);
        //�ʱ�ȭ
        searching = false;
        isNewdestList = false;
        naviArrow.setdest = false;//������ ������ �����Ƿ� false�� ����
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
        isNewdestList = setNewPath; //���ο� ��� Ȯ��
    }

    //������ġ -> ���� ������ ->���� ���� ->������ġ ������Ʈ ->���� ������
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Node>() != null)
        {
            currentNode = other.GetComponent<Node>();
            naviDemo.startNode = other.GetComponent<Node>();
        }
    }
}
