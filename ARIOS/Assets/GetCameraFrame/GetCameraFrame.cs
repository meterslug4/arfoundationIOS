using System.Collections;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class GetCameraFrame : MonoBehaviour
{
    public Camera arCamera;
    public RawImage camFrame;//�ǽð� ī�޶� ������
    public RawImage captureImg;//ĸ�ĵ� ������ ����Ʈ Ŭ���� color�� ���� ĸ���̹���
    public ARCameraManager cameraManager;
    public TMP_Text imgSizeTxt;
    public TMP_Text errorTxt;
    public RenderTexture rt;
    protected Texture2D copyTexture2D;
    protected int screenW;
    protected int screenH;
    string info = string.Empty;
    public Button captureBtn;
    void Start()
    {
        if(arCamera == null)
        {
            arCamera = Camera.main;
        }
        cameraManager = GetComponent<ARCameraManager>();
        CreateRenderTexture();
        captureBtn.onClick.AddListener(OnClickCaptureFrame);
    }
    //Render Texture ���� 
    protected void CreateRenderTexture()
    {
        CheckScreenSize();
        if(rt != null)
        {
            rt.Release();
            Destroy(rt);
        }
        rt = new RenderTexture(screenW, screenH,24);
        rt.Create();
        
        arCamera.targetTexture = rt;
        //camFrame �Ҵ� X 
    }
    protected void OnClickCaptureFrame()
    {
        GetTexturePiexel();
        ChangePixelColor(copyTexture2D);
        captureImg.texture = copyTexture2D;// renderTexture���� �о�°��� capture texture �� 
    }
    protected void GetTexturePiexel()
    {
        if(camFrame == null)
        {
            return;
        }
        RenderTexture.active = rt;
        if (copyTexture2D == null)
        {
            copyTexture2D = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        }
        copyTexture2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        copyTexture2D.Apply();//render texture�� texutre 2d�� ��ȯ 
    }
    protected void ChangePixelColor(Texture2D texture)
    {
        if(texture == null)
        {
            return;
        }
        for(int x =0; x<100; x++)
        {
            for(int y=0; y<100; y++)
            {
                texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();
    }
    protected void GetPixelColor(Texture2D targetTexture,int x,int y)
    {

    }
    protected void CheckScreenSize()
    {
        screenW = Screen.width;
        screenH = Screen.height;
        info = string.Format("W:{0} H:{1}", screenW, screenH);
    }
    private void OnDestroy()
    {
        if (rt != null)
        {
            rt.Release();
            Destroy(rt);
        }
    }
}
