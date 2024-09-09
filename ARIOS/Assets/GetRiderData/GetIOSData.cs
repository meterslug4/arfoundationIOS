using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.IO;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using Unity.Collections;


public class GetIOSData : MonoBehaviour
{
    public ARPointCloudManager pcdManager; //ARFoundation Point cloud Manager
    protected int saveFileCnt = 0;
    protected List<Vector3> pcdList = new List<Vector3>();//point cloud position
    protected List<Color> colorList = new List<Color>();
    //protected int idx = 0;
    protected int listCnt = 200000;
    protected string saveFilePath;
    protected string saveColorPath;

    public ARCameraManager arCameraManager;
    public Camera arCam;
    protected List<Vector3> cameraPath = new List<Vector3>(); //camera position list
    protected float interval = 1.0f;
    protected float saveTime = 0.0f;
    protected Texture2D cameraTexture;

    protected int skip;


    public Button saveBtn;//��������� pcdList(20������ �����ʾƵ� ��������) cameraPosList���� ����
    protected bool isSave = true;
    private void Awake()
    {
        skip = 20;
        arCam = Camera.main;
        pcdManager = GameObject.FindObjectOfType<ARPointCloudManager>();
        if (pcdManager != null)
        {
            pcdManager.pointCloudsChanged += PointCloudsChanged;
        }
    }
    void Start()
    {
        cameraPath.Add(arCam.transform.position);
        saveBtn.onClick.AddListener(Save);
    }
    private void Update()
    {
        //AddCameraPositions();
    }
    private void OnEnable()
    {
        arCameraManager.frameReceived += OnCameraFrameReceived;
    }
    private void OnDisable()
    {
        arCameraManager.frameReceived -= OnCameraFrameReceived;
    }
    void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
    {
        if(Time.frameCount % skip !=0)
        {
            return;
        }
        if (!arCameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
        {
            return;
        }

        // ī�޶� �̹����� ó���ϴ� �ڵ�
        // XRCpuImage�� Texture2D�� ��ȯ�Ͽ� ��� ����
        var conversionParams = new XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, image.width, image.height),
            outputDimensions = new Vector2Int(image.width, image.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = XRCpuImage.Transformation.None
        };

        cameraTexture = new Texture2D(image.width, image.height, TextureFormat.RGBA32, false);
        var rawTextureData = cameraTexture.GetRawTextureData<byte>();
        image.Convert(conversionParams, new NativeArray<byte>(rawTextureData, Allocator.Temp));
        cameraTexture.Apply();

        image.Dispose();
    }
    protected void AddCameraPositions()
    {
        if(isSave)
        {
            saveTime += Time.deltaTime;
            if(saveTime >= interval)
            {
                cameraPath.Add(arCam.transform.position);
                saveTime = 0.0f;
            }
        }
    }
    protected void PointCloudsChanged(ARPointCloudChangedEventArgs args)
    {
        if(Time.frameCount % skip != 0)
        {
            return;
        }
        if(isSave)
        {
            foreach(ARPointCloud pointCloud in args.added)
            {
                if(pointCloud.positions.HasValue)
                {
                    pcdList.AddRange(pointCloud.positions.Value);
                    cameraPath.Add(arCam.transform.position);
                    for (int i=0; i<pcdList.Count; i++)
                    {
                        Vector3 screenPoint = arCam.WorldToScreenPoint(pcdList[i]);
                        int pixelX = (int)screenPoint.x;
                        int pixelY = (int)screenPoint.y;
                        try
                        {
                            if (pixelX >= 0 && pixelX < cameraTexture.width && pixelY >= 0 && pixelY < cameraTexture.height)
                            {
                                Color pointColor = cameraTexture.GetPixel(pixelX, pixelY);  // �ش� 2D ��ǥ�� ���� ���� ����
                                colorList.Add(pointColor);
                            }
                        }
                        catch(System.Exception e)
                        {
                            Color pointColor = Color.black;
                            colorList.Add(pointColor);
                        }
                    }
                }
            }

            foreach(ARPointCloud pointCloud in args.updated)
            {
                if (pointCloud.positions.HasValue)
                {
                    pcdList.AddRange(pointCloud.positions.Value);
                    cameraPath.Add(arCam.transform.position);
                    for (int i = 0; i < pcdList.Count; i++)
                    {
                        Vector3 screenPoint = arCam.WorldToScreenPoint(pcdList[i]);
                        int pixelX = (int)screenPoint.x;
                        int pixelY = (int)screenPoint.y;

                        try
                        {
                            if (pixelX >= 0 && pixelX < cameraTexture.width && pixelY >= 0 && pixelY < cameraTexture.height)
                            {
                                Color pointColor = cameraTexture.GetPixel(pixelX, pixelY);  // �ش� 2D ��ǥ�� ���� ���� ����
                                colorList.Add(pointColor);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Color pointColor = Color.black;
                            colorList.Add(pointColor);
                        }
                    }
                }
            }

            UpdatePCDFile();
        }
    }
    protected void UpdatePCDFile()
    {
        if(pcdList.Count == listCnt)
        {
            SavePCDToBIN(); //���̳ʸ��� ������
            saveFileCnt += 1;//����ī��� 1����
            pcdList.Clear();//����Ʈ �ʱ�ȭ
            colorList.Clear();
            cameraPath.Clear();
        }
    }
    protected void SavePCDToBIN()
    {
        string fileName = string.Format("PCD{0}.bin", saveFileCnt);
        saveFilePath = Path.Combine(Application.persistentDataPath, fileName);
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveFilePath, FileMode.Create)))
        {
            // ����Ʈ�� ������ ���� ����
            writer.Write(pcdList.Count);
            // �� ����Ʈ ��ǥ�� ����
            foreach (Vector3 point in pcdList)
            {
                writer.Write(point.x);
                writer.Write(point.y);
                writer.Write(point.z);
            }
        }
        string colorFileName = string.Format("PCDColor{0}.bin", saveFileCnt);
        saveColorPath = Path.Combine(Application.persistentDataPath, colorFileName);
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveColorPath, FileMode.Create)))
        {
            // Color ����Ʈ�� ������ ����
            writer.Write(colorList.Count);

            // �� Color�� r, g, b, a ���� ����
            foreach (Color color in colorList)
            {
                writer.Write(color.r);
                writer.Write(color.g);
                writer.Write(color.b);
                writer.Write(color.a);
            }
        }
    }
    protected void Save()
    {
        if(isSave)
        {
            isSave = false;
            string pcdFileName = "LastPcd.bin";
            string pcdSavePath = Path.Combine(Application.persistentDataPath, pcdFileName);
            using (BinaryWriter writer = new BinaryWriter(File.Open(pcdSavePath, FileMode.Create)))
            {
                // ����Ʈ�� ������ ���� ����
                writer.Write(pcdList.Count);
                // �� ����Ʈ ��ǥ�� ����
                foreach (Vector3 point in pcdList)
                {
                    writer.Write(point.x);
                    writer.Write(point.y);
                    writer.Write(point.z);
                }
            }
            string colorFileName = "LastColor.bin";
            string saveColorPath = Path.Combine(Application.persistentDataPath, colorFileName);
            using (BinaryWriter writer = new BinaryWriter(File.Open(saveColorPath, FileMode.Create)))
            {
                // Color ����Ʈ�� ������ ����
                writer.Write(colorList.Count);

                // �� Color�� r, g, b, a ���� ����
                foreach (Color color in colorList)
                {
                    writer.Write(color.r);
                    writer.Write(color.g);
                    writer.Write(color.b);
                    writer.Write(color.a);
                }
            }

            string camFileName = "camPath.bin";
            string camSavePath = Path.Combine(Application.persistentDataPath, camFileName);
            using (BinaryWriter writer = new BinaryWriter(File.Open(camSavePath, FileMode.Create)))
            {
                // ����Ʈ�� ������ ���� ����
                writer.Write(cameraPath.Count);

                // �� ����Ʈ ��ǥ�� ����
                foreach (Vector3 point in cameraPath)
                {
                    writer.Write(point.x);
                    writer.Write(point.y);
                    writer.Write(point.z);
                }
            }
        }
    }
}

