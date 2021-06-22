using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TraceBase : MonoBehaviour
{
    protected Rigidbody m_TargetRd;
    protected Rigidbody rigibody
    {
        get {
            if (m_TargetRd == null)
            {
                var rb = GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = gameObject.AddComponent<Rigidbody>();
                }
                m_TargetRd = rb;
            }

            return m_TargetRd;
        }
    }

    public GameObject Target;

    public float TraceForceDot = 10.0f;

    public float TorqueDot = 40.0f;

    public bool TransformUp = false;

    protected Vector3 tempVec3 = Vector3.zero;
    void FixedUpdate()
    {
        if (Target == null)
            return;

        var direction = Target.transform.position - this.transform.position;
        rigibody.AddForce(direction * TraceForceDot);

        var target_rot = Quaternion.identity;
        if (TransformUp)
        {
            target_rot = Quaternion.LookRotation(direction, transform.TransformVector(Vector3.up));
        }
        else
        {
            target_rot = Quaternion.LookRotation(direction);
        }
        var rot = target_rot * Quaternion.Inverse(this.transform.rotation);
        if (rot.w < 0f)
        { 
            rot.x *= -1;
            rot.y *= -1;
            rot.z *= -1;
            rot.w *= -1;
        }
        tempVec3.Set(rot.x, rot.y, rot.z);
        rigibody.AddTorque(tempVec3 * TorqueDot);
    }
}
