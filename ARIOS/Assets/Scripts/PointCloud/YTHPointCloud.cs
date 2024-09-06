using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;
using System.IO;
public class YTHPointCloud : MonoBehaviour
{
    public ARPointCloudManager pointCloudManager;
    public Button pcdSaveBtn;
    protected List<Vector3> pointCloudPosList = new List<Vector3>();
    protected string saveFilePath;
    void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "PCD.bin");
        pointCloudManager.pointCloudsChanged += OnPointCloudChanged;
        pcdSaveBtn.onClick.AddListener(SavePCDToBIN);
    }
    protected void OnPointCloudChanged(ARPointCloudChangedEventArgs eventArgs)
    {
        foreach(ARPointCloud pointCloud in eventArgs.added)
        {
            if(pointCloud.positions.HasValue)
            {
                pointCloudPosList.AddRange(pointCloud.positions.Value);
            }
        }
        foreach (ARPointCloud pointCloud in eventArgs.updated)
        {
            if (pointCloud.positions.HasValue)
            {
                pointCloudPosList.AddRange(pointCloud.positions.Value);
            }
        }
    }
    protected void SavePCDToBIN()
    {
        using (BinaryWriter writer = new BinaryWriter(File.Open(saveFilePath, FileMode.Create)))
        {
            // ����Ʈ�� ������ ���� ����
            writer.Write(pointCloudPosList.Count);

            // �� ����Ʈ ��ǥ�� ����
            foreach (Vector3 point in pointCloudPosList)
            {
                writer.Write(point.x);
                writer.Write(point.y);
                writer.Write(point.z);
            }
        }
    }
    protected List<Vector3> LoadVector3ListFromBinary()
    {
        List<Vector3> list = new List<Vector3>();

        if (File.Exists(saveFilePath))
        {
            using (BinaryReader reader = new BinaryReader(File.Open(saveFilePath, FileMode.Open)))
            {
                // ����� ����Ʈ�� ũ�⸦ ���� ����
                int count = reader.ReadInt32();

                // �� ��ǥ�� �о� ����Ʈ�� �߰�
                for (int i = 0; i < count; i++)
                {
                    float x = reader.ReadSingle();
                    float y = reader.ReadSingle();
                    float z = reader.ReadSingle();
                    list.Add(new Vector3(x, y, z));
                }
            }
        }
        else
        {
            Debug.LogWarning("No Vector3 list data file found!");
        }

        return list;
    }
}
