/*
 * By TyoukabuN
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TCam
{
    [ExecuteAlways]
    public class TCameraMesh : MonoBehaviour
    {
        public static TCameraMesh currentTCameraMesh;

        public bool PowerOn = true;
        public bool GizmosOn = true;
        public List<TCameraTrangle> TCameraTrangles = new List<TCameraTrangle>();

        public Transform Target;
        /// <summary>
        /// will pass an eularAngle
        /// </summary>
        public CameraMeshEvent OnPositionChanged;

        public void Awake()
        {
            if (currentTCameraMesh == null)
            { 
                currentTCameraMesh = this;
            }
            if (OnPositionChanged == null)
            {
                OnPositionChanged = new CameraMeshEvent();
            }
        }
        public bool AddTrangle(TCameraTrangle trangle) 
        {
            CleanUp();

            if (TCameraTrangles.Contains(trangle))
            {
                return false;
            }

            TCameraTrangles.Add(trangle);

            return true;
        }

        public void SetTarget(Transform target)
        {
            Target = target;
        }

        private void CleanUp()
        {
            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i];
                if (tri == null)
                { 
                    TCameraTrangles.RemoveAt(i);
                    i--;
                    continue;
                }

                if (!tri.Valid())
                {
                    TCameraTrangles.RemoveAt(i);
                    GameObject.DestroyImmediate(tri.gameObject);
                    i--;
                    continue;
                }
            }
        }

        void Update()
        {
            if (TCameraTrangles.Count < 1)
                return;

            CleanUp();

            if (Target == null)
                return;

            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i];

                if (tri == null)
                    break;

                if (!tri.Valid())
                    break;

                var vertices = tri.Vertices.ToArray(); 

                float[] weight;
                if (TCameraUtility.IsInsideTrangle(vertices, Target.position,out weight))
                {
                    if (PowerOn)
                    {
                        vertices = tri.Vertices.ToArray();
                        var eulerAngles = tri.camVertices[0].EularAngle * weight[0] +
                            tri.camVertices[1].EularAngle * weight[1] +
                            tri.camVertices[2].EularAngle * weight[2];

                        if (OnPositionChanged != null)
                        { 
                            OnPositionChanged.Invoke(eulerAngles);
                        }
                    }
                }
            }
        }

        void OnDrawGizmos()
        {
            if (!GizmosOn)
                return;

            if (TCameraTrangles.Count < 1)
                return;

            if (Target == null)
                return;

            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i];

                if (tri == null)
                    break;

                if (tri.Vertices.Count < 3)
                    break;

                Mesh mesh = new Mesh();
                mesh.vertices = tri.Vertices.ToArray();
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                Gizmos.color = Color.red;

                if (TCameraUtility.IsInsideTrangleS(mesh.vertices, Target.position))
                {
                    Gizmos.color = Color.green;
                }
                Gizmos.DrawMesh(mesh);

                Gizmos.color = Color.white;

                Gizmos.DrawWireMesh(mesh);

            }
        }

        public class CameraMeshEvent : UnityEvent<Vector3> { }
    }
}
