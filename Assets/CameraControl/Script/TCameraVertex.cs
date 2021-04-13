using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TCam
{
    public class TCameraVertex : MonoBehaviour
    {
#if !TYOU_LAB
        [HideInInspector]
#endif
        public TCameraTrangle trangle;
        public Vector3 EularAngle = Vector3.zero;
        public Vector3 PivotPosition = Vector3.zero;
        public float x
        {
            get {
                return transform.position.x;
            }
        }

        public float y
        {
            get
            {
                return transform.position.y;
            }
        }

        public float z
        {
            get
            {
                return transform.position.z;
            }
        }

        public float this[int index] 
        {
            get {
                return transform.position[index];
            }
        }
    }
}
