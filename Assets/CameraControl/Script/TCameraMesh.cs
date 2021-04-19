/*
 * By TyoukabuN
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace TCam
{
    [ExecuteAlways]

    public class TCameraMesh : MonoBehaviour
    {
        [HideInInspector]
        public static TCameraMesh currentTCameraMesh;
        [HideInInspector]
        private Dictionary<TCameraVertex, List<TCameraTrangle>> vertex2TranglesDict = new Dictionary<TCameraVertex, List<TCameraTrangle>>();

        public bool PowerOn = true;
        public bool GizmosOn = true;
        private bool m_PerformanceOptimizationOn = true;
        public bool PerformanceOptimizationOn
        {
            set
            {
                m_PerformanceOptimizationOn = value;

                if (!m_PerformanceOptimizationOn)
                {
                    for (int i = 0; i < TCameraTrangles.Count; i++)
                    {
                        var tri = TCameraTrangles[i];

                        if (tri == null)
                            break;

                        tri.PowerOn = true;
                    }
                }
                else
                { 
                    //TODO
                }
            }
            get { return m_PerformanceOptimizationOn; }
        }
#if !TYOU_LAB
        [HideInInspector]
#endif
        public float ValidRadius = 2.0f;
#if !TYOU_LAB
        [HideInInspector]
#endif
        public List<TCameraTrangle> TCameraTrangles = new List<TCameraTrangle>();
#if !TYOU_LAB
        [HideInInspector]
#endif
        public Transform Target;
#if TYOU_LAB
        [SerializeField]
#endif
        private TCameraTrangle m_CurrentTrangle;
        public TCameraTrangle CurrentTrangle {
            get { return m_CurrentTrangle; }
            private set { m_CurrentTrangle = value; }
        }
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

        private bool IsTrangleValid(TCameraTrangle tri)
        {
            if (tri == null)
                return false;

            if (!tri.PowerOn)
                return false;

            if (!tri.Valid())
                return false;

            return true;
        }

        private bool TrangleCheckAndProcess(TCameraTrangle tri)
        {
            var vertices = tri.Vertices.ToArray();

            float[] weight;
            if (TCameraUtility.IsInsideTrangle(vertices, Target.position, out weight))
            {
                if (PowerOn)
                {
                    vertices = tri.Vertices.ToArray();
                    var eulerAngles = tri.camVertices[0].EularAngle * weight[0] +
                        tri.camVertices[1].EularAngle * weight[1] +
                        tri.camVertices[2].EularAngle * weight[2];

                    var pivotPosition = tri.camVertices[0].PivotPosition * weight[0] +
                        tri.camVertices[1].PivotPosition * weight[1] +
                        tri.camVertices[2].PivotPosition * weight[2];

                    if (OnPositionChanged != null)
                    {
                        OnPositionChanged.Invoke(eulerAngles, pivotPosition);
                    }
                }
                return true;
            }
            return false;
        }


        void Update()
        {
            Profiler.BeginSample("TCamreraMesh");

            if (TCameraTrangles.Count < 1)
            {
                Profiler.EndSample();
                return;
            }

            CleanUp();

            if (Target == null)
            { 
                Profiler.EndSample();
                return;
            }

            if (PerformanceOptimizationOn)
            {
                PerformanceOptimizationSetup();
            }

            if (CurrentTrangle != null)
            {
                if (IsTrangleValid(CurrentTrangle) && TrangleCheckAndProcess(CurrentTrangle))
                {
                    Profiler.EndSample();
                    return;
                }

                CurrentTrangle = null;
            }

            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i];

                if (IsTrangleValid(tri) && TrangleCheckAndProcess(tri))
                {
                    CurrentTrangle = tri;
                    break;
                }
            }
            Profiler.EndSample();
        }

        private void PerformanceOptimizationSetup()
        {

        }

        public bool TryGetTranglesByVertex(TCameraVertex vertex, out TCameraTrangle[] trangles)
        {
            List<TCameraTrangle> trangleList;
            if (vertex2TranglesDict.TryGetValue(vertex,out trangleList))
            {
                trangles = trangleList.ToArray();
                return true;
            }

            trangles = null;
            return false;
        }
        public List<TCameraVertex> GetAllVertices()
        {
            var vertices = new List<TCameraVertex>();
            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i];

                if (tri == null)
                    break;

                for (int j = 0; j < tri.camVertices.Length; j++)
                {
                    var ver = tri.camVertices[j];

                    if (ver == null)
                        break;

                    vertices.Add(ver);
                }
            }

            return vertices;
        }

        void OnDrawGizmos()
        {
            if (!GizmosOn)
                return;

            if (TCameraTrangles.Count < 1)
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
                if (Target != null)
                {
                    if (TCameraUtility.IsInsideTrangleS(mesh.vertices, Target.position))
                    {
                        Gizmos.color = Color.green;
                    }
                }

                Gizmos.DrawMesh(mesh);

                Gizmos.color = Color.white;

                if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 ||
                    SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
                {
                    Gizmos.DrawWireMesh(mesh);
                }
                else
                {
                    Gizmos.DrawLine(mesh.vertices[0], mesh.vertices[1]);
                    Gizmos.DrawLine(mesh.vertices[0], mesh.vertices[2]);
                    Gizmos.DrawLine(mesh.vertices[1], mesh.vertices[2]);
                }

            }
        }

        public class CameraMeshEvent : UnityEvent<Vector3,Vector3> { }
    }
}
