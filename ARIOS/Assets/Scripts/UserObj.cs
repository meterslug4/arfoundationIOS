
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UserObj : MonoBehaviour
{
    [SerializeField]protected Demo officeDemo;
    public ShowDirection showDirection;//ȭ��ǥ ǥ�ÿ� ������Ʈ
    [SerializeField]protected Node currentNode;//���� user�� �ִ� ���
    [SerializeField]protected Node nextNode; //�������� ������ ���(���� ������X)
    protected List<Node> closeList = new List<Node>();//�̹� �湮�� ��� ����Ʈ
    protected Vector3 nextDir;//���� ����� ����
    protected bool isFinish = false;//���� �������� �����ߴ���
    protected Queue<Node> desQue = new Queue<Node>();
    protected bool isNewdestList = false;//�� ��ΰ� ���������?
    protected bool searching = false;//Ž��������?

    void Start()
    {
        officeDemo.delSetPath = SetNewPath;
    }
    private void Update()
    {
        //Ž���� ���� ������Ʈ�ϸ� ���� parentList�� 0�̸� ���۾���
        //���ι��� �ܿ� ����Ʈ�� �ִ��� üũ(isNewdestList)
        if (isNewdestList == true && searching ==false)
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
        for (int i=0; i<cnt; i++)
        {
            nextNode = desQue.Dequeue();
            //���� ��忡�� ���������� ������
            showDirection.SetDestination(nextNode.transform.position,true);
            yield return new WaitUntil(() => currentNode.nodeId == nextNode.nodeId);
            closeList.Add(nextNode);
            //currentNode = nextNode;
        }
        //��� ��� �湮 ������ ����
        //�ʱ�ȭ
        searching = false;
        isNewdestList = false;
        showDirection.setdest = false;//������ ������ �����Ƿ� false�� ����
        showDirection.EnableArrow(false);
        closeList.Clear();
        //yield return null;
    }
    protected void SetNewPath(List<Node>parentList,bool setNewPath)
    {
        desQue.Clear();
        for(int i=0; i<parentList.Count; i++)
        {
            desQue.Enqueue(parentList[i]);
        }
        isNewdestList = setNewPath; //���ο� ��� Ȯ��
    }
    
    //������ġ -> ���� ������ ->���� ���� ->������ġ ������Ʈ ->���� ������
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Node>()!= null)
        {
            currentNode = other.GetComponent<Node>();
            officeDemo.startNode = other.GetComponent<Node>();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        
    }
}
