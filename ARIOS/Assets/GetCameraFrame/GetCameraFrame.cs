using System.Collections;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
public class GetCameraFrame : MonoBehaviour
{
    public RawImage camFrame;
    public ARCameraManager cameraManager;
    public TMP_Text imgSizeTxt;
    public TMP_Text errorTxt;
    public Button captureBtn;
    protected Texture2D cameraTexture;

    void Start()
    {
        cameraManager = GetComponent<ARCameraManager>();
        //cameraManager.frameReceived += OnCameraFrameReceived;
        captureBtn.onClick.AddListener(GetCurrentTexture);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }
    protected void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        // XRCpuImage를 통해 현재 프레임을 가져옴
        if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            StartCoroutine(ProcessImage(image));
        }
    }
    IEnumerator ProcessImage(XRCpuImage image)
    {
        // 이미지 변환 설정 (RGBA32 형식으로 변환)
        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.None
        };

        // 변환된 이미지 데이터를 저장할 NativeArray 생성
        var rawTextureData = new NativeArray<byte>(image.GetConvertedDataSize(conversionParams), Allocator.Temp);

        // 이미지 데이터를 변환하여 NativeArray에 저장
        try
        {
            image.Convert(conversionParams, rawTextureData);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Image conversion failed: {ex.Message}");
            errorTxt.text = ex.Message;
        }
        finally
        {
            // 이미지 사용 후 반드시 Dispose 호출
            image.Dispose();
        }

        // Texture2D가 없다면 생성
        if (cameraTexture == null || cameraTexture.width != image.width || cameraTexture.height != image.height)
        {
            cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
            imgSizeTxt.text = string.Format("W:{0} H:{1}", image.width, image.height);
        }

        // NativeArray 데이터를 Texture2D로 복사
        cameraTexture.LoadRawTextureData(rawTextureData);
        cameraTexture.Apply();

        // RawImage에 Texture 할당
        camFrame.texture = cameraTexture;

        // NativeArray 메모리 해제
        rawTextureData.Dispose();

        yield return null;
    }
    protected void GetCurrentTexture()
    {
        if (cameraManager == null)
        {
            return;
        }
        if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            try
            {
                var conversionParams = new XRCpuImage.ConversionParams
                {
                    inputRect = new RectInt(0, 0, image.width, image.height),
                    outputDimensions = new Vector2Int(image.width, image.height),
                    outputFormat = TextureFormat.RGB24,
                    transformation = XRCpuImage.Transformation.None,
                };
                int size = image.GetConvertedDataSize(conversionParams);
                var buffer = new NativeArray<byte>(size, Allocator.Temp);
                image.Convert(conversionParams, buffer);
                image.Dispose();
                if (cameraTexture == null)
                {
                    cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGB24, false);
                }
                imgSizeTxt.text = string.Format("W:{0} H:{1}", cameraTexture.width, cameraTexture.height);
                cameraTexture.LoadRawTextureData(buffer);
                cameraTexture.Apply();
            }
            catch (System.Exception e)
            {
                errorTxt.text = e.Message;
            }

            //0,0 ~시작지점 좌상단, 좌하단 우상단 우하단 구분
            for (int x = 0; x < 50; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    cameraTexture.SetPixel(x, y, Color.red);
                }
            }
            cameraTexture.Apply();

        }
    }
}
