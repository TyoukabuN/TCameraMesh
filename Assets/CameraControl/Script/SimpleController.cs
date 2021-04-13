using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCam;
public class SimpleController : MonoBehaviour
{
    public Camera Camera;
    public Transform CameraPivot;

    private string HorizontalAxi = "Horizontal";
    private string VerticalAxi = "Vertical";
    public float Speed = 0.4f;
    void Start()
    {
        TCameraMesh mesh;
        if (TCam.TCameraUtility.TryGetCameraMesh(out mesh))
        {
            mesh.SetTarget(transform);
            mesh.OnPositionChanged.RemoveListener(OnPositionChanged);
            mesh.OnPositionChanged.AddListener(OnPositionChanged);
        }

        if (CameraPivot == null)
        {
            var pivot = new GameObject("piovt");
            pivot.transform.SetParent(transform);
            CameraPivot = pivot.transform;
        }
        CameraPivot.transform.localPosition = Vector3.zero;

        if (Camera == null)
        {
            Camera = Camera.main;
            if (Camera == null)
            {
                var gobj = new GameObject("tempPiovt");
                var camera = gobj.AddComponent<Camera>();
                Camera = camera;
            }
        }

        Camera.transform.SetParent(CameraPivot);
        Camera.transform.localPosition = Vector3.zero;
        Camera.transform.localRotation = Quaternion.identity;
    }

    void OnPositionChanged(Vector3 cameraEular, Vector3 pivotPosition)
    {
        Camera.transform.eulerAngles = new Vector3(cameraEular.x, Camera.transform.eulerAngles.y, -Camera.transform.eulerAngles.z);
        Camera.transform.localPosition = new Vector3(0, 0, -cameraEular.z);

        CameraPivot.transform.eulerAngles = new Vector3(0, cameraEular.y, 0);
        CameraPivot.localPosition = pivotPosition;
    }

    void Update()
    {
        var h = Input.GetAxis(HorizontalAxi);
        var v = Input.GetAxis(VerticalAxi);

        transform.Translate(-Vector3.forward * v * Speed + -Vector3.right * h * Speed);
    }

    public void ApplyInput(float moveInput)
    {
        Move(moveInput);
        //Ture(input);
    }

    public void Move(float input)
    {
    }

    public void Ture(float input)
    {
    }


}
