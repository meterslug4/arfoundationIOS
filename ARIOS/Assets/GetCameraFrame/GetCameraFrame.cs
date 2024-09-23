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
    public RawImage camFrame;//실시간 카메라 프레임
    public RawImage captureImg;//캡쳐된 프레임 포인트 클라우드 color를 위한 캡쳐이미지
    public ARCameraManager cameraManager;
    public TMP_Text imgSizeTxt;
    public TMP_Text errorTxt;
    public RenderTexture rt;
    protected Texture2D cameraTexture;
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
        if(cameraManager == null)
        {
            cameraManager = GetComponent<ARCameraManager>();
        }
        cameraManager.frameReceived += OnReceivedCameraFrame;
        captureBtn.onClick.AddListener(OnClickCaptureFrame);
    }
    void OnReceivedCameraFrame(ARCameraFrameEventArgs args)
    {
        try
        {
            if(cameraManager != null)
            {
                if(cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
                {
                    info = string.Format("W:{0} H:{1}", image.width, image.height);
                    imgSizeTxt.text = info;
                    var conversionParams = new XRCpuImage.ConversionParams
                    {
                        inputRect = new RectInt(0,0,image.width,image.height),
                        outputDimensions = new Vector2Int(image.width,image.height),
                        outputFormat = TextureFormat.RGB24,
                        transformation = XRCpuImage.Transformation.None
                    };
                    if(cameraTexture == null)
                    {
                        cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGB24, false);
                    }
                    var rawTextureData = cameraTexture.GetRawTextureData<byte>();
                    image.Convert(conversionParams, new NativeArray<byte>(rawTextureData, Allocator.Temp));
                    cameraTexture.Apply();
                    captureImg.texture = cameraTexture;
                    image.Dispose();
                }
            }
        }
        catch(System.Exception e)
        {
            errorTxt.text = e.Message;
        }
    }
    protected void OnClickCaptureFrame()
    {
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
        imgSizeTxt.text = info;
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
