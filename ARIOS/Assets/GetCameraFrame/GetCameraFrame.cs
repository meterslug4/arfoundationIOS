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
        // XRCpuImage�� ���� ���� �������� ������
        if (cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            StartCoroutine(ProcessImage(image));
        }
    }
    IEnumerator ProcessImage(XRCpuImage image)
    {
        // �̹��� ��ȯ ���� (RGBA32 �������� ��ȯ)
        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.None
        };

        // ��ȯ�� �̹��� �����͸� ������ NativeArray ����
        var rawTextureData = new NativeArray<byte>(image.GetConvertedDataSize(conversionParams), Allocator.Temp);

        // �̹��� �����͸� ��ȯ�Ͽ� NativeArray�� ����
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
            // �̹��� ��� �� �ݵ�� Dispose ȣ��
            image.Dispose();
        }

        // Texture2D�� ���ٸ� ����
        if (cameraTexture == null || cameraTexture.width != image.width || cameraTexture.height != image.height)
        {
            cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
            imgSizeTxt.text = string.Format("W:{0} H:{1}", image.width, image.height);
        }

        // NativeArray �����͸� Texture2D�� ����
        cameraTexture.LoadRawTextureData(rawTextureData);
        cameraTexture.Apply();

        // RawImage�� Texture �Ҵ�
        camFrame.texture = cameraTexture;

        // NativeArray �޸� ����
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

            //0,0 ~�������� �»��, ���ϴ� ���� ���ϴ� ����
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
