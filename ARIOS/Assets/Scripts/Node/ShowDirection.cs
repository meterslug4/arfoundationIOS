
using UnityEngine;
using UnityEngine.UI;

public class ShowDirection : MonoBehaviour
{
    public Image arrowImg;
    public UserObj user;
    protected Vector3 destPosition;
    public bool setdest = false;
    [SerializeField]protected float arrowDist = 0.8f;
    protected float objH; //���̸� �����ֱ����� ��
    void Start()
    {
        EnableArrow(false);
        arrowDist = 0.8f;
        objH = 0.8f;
        setdest = false;
    }
    private void Update()
    {
        ShowDisplay();
    }
    protected void ShowDisplay()
    {
        if(setdest)
        {
            arrowImg.enabled = true;
            Vector3 tempCurrentUserPos = 
                new Vector3(user.transform.position.x, objH, user.transform.position.z);
            Vector3 tempTargetPos = 
                new Vector3(destPosition.x, objH, destPosition.z);

            Vector3 targetDir = tempTargetPos - tempCurrentUserPos;
            Vector3 nDir = targetDir.normalized;
            transform.position = tempCurrentUserPos +  nDir * arrowDist;
            Vector3 tempDir = new Vector3(destPosition.x, objH, destPosition.z);
            //���� X�� ����
            transform.LookAt(tempDir);
            //�ٶ󺸴� ������ �������� ��ġ
        }
    }
    public void EnableArrow(bool b)
    {
        arrowImg.enabled = b;
    }
    public void SetDestination(Vector3 destPos,bool setPos)
    {
        destPosition = destPos;
        setdest = setPos;
    }
}
