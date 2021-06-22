/*
 * By TyoukabuN
 */
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace TMesh
{
    public abstract class TMeshBase: MonoBehaviour
    {
        public bool PowerOn = true;
        public bool GizmosOn = true;
        protected bool m_PerformanceOptimizationOn = true;
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
        public List<TTrangle> TCameraTrangles = new List<TTrangle>();
#if !TYOU_LAB
        [HideInInspector]
#endif
        public Transform Target;
#if TYOU_LAB
        [SerializeField]
#endif
        protected TTrangle m_CurrentTrangle;
        public TTrangle CurrentTrangle
        {
            get { return m_CurrentTrangle; }
            protected set { m_CurrentTrangle = value; }
        }

        public bool AddTrangle(TTrangle trangle)
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

        protected void CleanUp()
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


        protected bool IsTrangleValid(TTrangle tri)
        {
            if (tri == null)
                return false;

            if (!tri.PowerOn)
                return false;

            if (!tri.Valid())
                return false;

            return true;
        }


        protected abstract bool TrangleCheckAndProcess(TTrangle tri);

        protected void Update()
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


        protected void PerformanceOptimizationSetup()
        {
            //TODO
        }

        //public bool TryGetTranglesByVertex(TCameraVertex vertex, out TTrangle[] trangles)
        //{
        //    List<TTrangle> trangleList;
        //    if (vertex2TranglesDict.TryGetValue(vertex, out trangleList))
        //    {
        //        trangles = trangleList.ToArray();
        //        return true;
        //    }

        //    trangles = null;
        //    return false;
        //}
        public List<TVertex> GetAllVertices()
        {
            var vertices = new List<TVertex>();
            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i];

                if (tri == null)
                    break;

                for (int j = 0; j < tri.vertices.Length; j++)
                {
                    var ver = tri.vertices[j];

                    if (ver == null)
                        break;

                    vertices.Add(ver);
                }
            }

            return vertices;
        }



#if UNITY_EDITOR
        protected Dictionary<TTrangle, Mesh> tempMesh = new Dictionary<TTrangle, Mesh>();
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

                if (tempMesh == null)
                {
                    tempMesh = new Dictionary<TTrangle, Mesh>();
                }

                Mesh mesh = null;
                if (!tempMesh.ContainsKey(tri))
                {
                    tempMesh[tri] = new Mesh();
                }
                mesh = tempMesh[tri];
                mesh.vertices = tri.Vertices.ToArray();
                mesh.triangles = new int[] { 0, 1, 2 };
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                Gizmos.color = Color.red;
                if (Target != null)
                {
                    if (TCameraUtility.IsInsideTrangleS2(mesh.vertices, Target.position))
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
#endif

    }
}
