using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCam;
public class SimpleController : MonoBehaviour
{
    public static SimpleController current;

    public Camera Camera;
    public Transform AxiY;
    public Transform AxiX;

    private string HorizontalAxi = "Horizontal";
    private string VerticalAxi = "Vertical";
    public float Speed = 0.4f;

    private bool m_FixedPositionToMeshsSurface = true;
    public bool FixedPositionToMeshsSurface
    {
        get {
            return m_FixedPositionToMeshsSurface;
        }
        set {
            m_FixedPositionToMeshsSurface = value;
        }
    }
    void Start()
    {
        current = this;

        TCameraMesh mesh;
        if (TCam.TCameraUtility.TryGetCameraMesh(out mesh))
        {
            mesh.SetTarget(transform);
            //mesh.OnPositionChanged.RemoveListener(OnPositionChanged);
            //mesh.OnPositionChanged.AddListener(OnPositionChanged);
            mesh.OnComplexEvent.RemoveListener(OnComplexEvent);
            mesh.OnComplexEvent.AddListener(OnComplexEvent);
        }

        if (AxiY == null)
        {
            var obj = new GameObject("AxiY");
            obj.transform.SetParent(transform);
            AxiY = obj.transform;
        }
        AxiY.transform.localPosition = Vector3.zero;

        if (AxiX == null)
        {
            var obj = new GameObject("AxiX");
            obj.transform.SetParent(AxiY);
            AxiX = obj.transform;
        }
        AxiX.transform.localPosition = Vector3.zero;

        if (Camera == null)
        {
            Camera = Camera.main;
            if (Camera == null)
            {
                var gobj = new GameObject("tempCamera");
                var camera = gobj.AddComponent<Camera>();
                Camera = camera;
            }
        }

        Camera.transform.SetParent(AxiX);
        Camera.transform.localPosition = Vector3.zero;
        Camera.transform.localRotation = Quaternion.identity;
    }

    void OnPositionChanged(Vector3 cameraEular, Vector3 pivotPosition)
    {
        AxiY.transform.localEulerAngles = new Vector3(0, cameraEular.y, 0);
        AxiX.transform.localEulerAngles = new Vector3(cameraEular.x, 0, 0);
        AxiX.localPosition = pivotPosition;

        Camera.transform.localEulerAngles = Vector3.zero;
        Camera.transform.localPosition = new Vector3(0, 0, -cameraEular.z);
    }

    void OnComplexEvent(Vector3 cameraEular, Vector3 pivotPosition, float[] weight)
    {
        OnPositionChanged(cameraEular, pivotPosition);

        if (FixedPositionToMeshsSurface)
        { 
            TCameraMesh mesh;
            if (TCam.TCameraUtility.TryGetCameraMesh(out mesh))
            {
                var tri = mesh.CurrentTrangle;
                transform.position = tri.camVertices[0].transform.position * weight[0] +
                    tri.camVertices[1].transform.position * weight[1] +
                    tri.camVertices[2].transform.position * weight[2];
            }
        }
    }

    void Update()
    {
        var h = Input.GetAxis(HorizontalAxi);
        var v = Input.GetAxis(VerticalAxi);

        transform.Translate(-Vector3.forward * v * Speed + -Vector3.right * h * Speed);
    }
}
