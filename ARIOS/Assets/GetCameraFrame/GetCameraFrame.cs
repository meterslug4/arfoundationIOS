using System;
using TMPro;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
    protected Texture2D cameraTexture;
    protected int screenW;
    protected int screenH;
    string info = string.Empty;
    public Button captureBtn;
    public Canvas uiCanvas;
    bool first = true;
    //아이패드 screen 해상도 2388, 1688
    void Start()
    {
        if (arCamera == null)
        {
            arCamera = Camera.main;
        }
        if (cameraManager == null)
        {
            cameraManager = GetComponent<ARCameraManager>();
        }
        CheckScreenSize();
        CanvasScaler cs = uiCanvas.GetComponent<CanvasScaler>();
        //cs.uiScaleMode = ScaleMode.
        cameraManager.frameReceived += OnReceivedCameraFrame;
        captureBtn.onClick.AddListener(OnClickCaptureFrame);
    }
    unsafe void OnReceivedCameraFrame(ARCameraFrameEventArgs args)
    {
        try
        {
            if (cameraManager != null)
            {
                if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
                {
                    return;
                }
                if (first)
                {
                    first = false;
                    info += string.Format("imgW:{0} imgH:{1}", image.width, image.height);
                }
                imgSizeTxt.text = info;

                var conversionParams = new XRCpuImage.ConversionParams
                {
                    inputRect = new RectInt(0, 0, image.width, image.height),
                    outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
                    outputFormat = TextureFormat.RGB24,
                    transformation = XRCpuImage.Transformation.None
                };


                int size = image.GetConvertedDataSize(conversionParams);
                var buffer = new NativeArray<byte>(size, Allocator.Temp);
                image.Convert(conversionParams, new IntPtr(buffer.GetUnsafePtr()), buffer.Length);
                //var rawTextureData = cameraTexture.GetRawTextureData<byte>();
                //image.Convert(conversionParams, new NativeArray<byte>(rawTextureData, Allocator.Temp));
                cameraTexture.Apply();
                captureImg.rectTransform.sizeDelta =
                    new Vector2(conversionParams.outputDimensions.x, conversionParams.outputDimensions.y);
                captureImg.rectTransform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                captureImg.texture = cameraTexture;
                image.Dispose();

                if (cameraTexture == null)
                {
                    cameraTexture = new Texture2D
                            (
                            conversionParams.outputDimensions.x,
                            conversionParams.outputDimensions.y,
                            conversionParams.outputFormat,
                            false
                            );
                }
                cameraTexture.LoadRawTextureData(buffer);
                buffer.Dispose();
            }
        }
        catch (System.Exception e)
        {
            errorTxt.text = e.Message;
        }
    }
    protected void OnClickCaptureFrame()
    {
    }
    protected void ChangePixelColor(Texture2D texture)
    {
        if (texture == null)
        {
            return;
        }
        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                texture.SetPixel(x, y, Color.black);
            }
        }
        texture.Apply();
    }
    protected void GetPixelColor(Texture2D targetTexture, int x, int y)
    {

    }
    protected void CheckScreenSize()
    {
        screenW = Screen.width;
        screenH = Screen.height;
        info = string.Format("SW:{0} SH:{1}\n", screenW, screenH);
    }
}
