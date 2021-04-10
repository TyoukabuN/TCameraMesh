using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCam;
public class SimpleController : MonoBehaviour
{
    private string HorizontalAxi = "Horizontal";
    private string VerticalAxi = "Vertical";
    public float Speed = 2.0f;
    void Start()
    {
        TCameraMesh mesh;
        if (TCam.TCameraUtility.TryGetCameraMesh(out mesh))
        {
            mesh.SetTarget(transform);
            mesh.OnPositionChanged.RemoveListener(OnPositionChanged);
            mesh.OnPositionChanged.AddListener(OnPositionChanged);
        }
    }

    void OnPositionChanged(Vector3 cameraEular)
    {
        Camera.main.transform.eulerAngles = cameraEular;
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
