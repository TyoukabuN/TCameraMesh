using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TCam
{
    public class TCameraTrangle : MonoBehaviour
    {
        public bool PowerOn = true;

#if !TYOU_LAB
        [HideInInspector]
#endif
        [SerializeField]
        public TCameraVertex[] camVertices;

        public TCameraVertex this[int index]
        {
            get 
            { 
                return camVertices[index]; 
            }
        }

        private List<Vector3> m_Vertices;
        public List<Vector3> Vertices
        {
            get {
                //if (m_Vertices == null)
                //{
                    RefreshVertices();
                //}
                return m_Vertices;
            }
        }

        public void MoveToCentroid()
        {
            if (Vertices == null)
                return;

            if (Vertices.Count < 3)
                return;

            var centroid = TCameraUtility.CalCentroid(Vertices.ToArray());
            transform.position = centroid;
        }

        public bool Valid()
        {
            if (Vertices.Count < 3)
                return false;

            for (int i = 0; i < camVertices.Length; i++)
            {
                if (camVertices[i] == null)
                    return false;
            }

            return true;
        }
        private void RefreshVertices()
        {
            if (camVertices == null)
            {
                return;
            }
            m_Vertices = new List<Vector3>();
            for (int i = 0; i < camVertices.Length; i++)
            {
                if (camVertices[i] != null)
                { 
                    m_Vertices.Add(camVertices[i].transform.position);
                }
            }
        }

        //void OnDrawGizmos()
        //{
        //    var tCameraMesh = GameObject.FindObjectOfType<TCameraMesh>();
        //    if (tCameraMesh == null)
        //        return;

        //    if (Vertices.Count < 3)
        //        return;

        //    Mesh mesh = new Mesh();
        //    mesh.vertices = Vertices.ToArray();
        //    mesh.triangles = new int[] { 0, 1, 2 };
        //    mesh.RecalculateNormals();
        //    mesh.RecalculateBounds();

        //    Gizmos.color = Color.red;


        //    if (tCameraMesh.Target != null)
        //    {
        //        if (TCameraUtility.IsInsideTrangleS(mesh.vertices, tCameraMesh.Target.position))
        //        {
        //            Gizmos.color = Color.green;
        //        }
        //    }
        //    Gizmos.DrawMesh(mesh);

        //    Gizmos.color = Color.white;

        //    if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11 ||
        //        SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D12)
        //    {
        //        Gizmos.DrawWireMesh(mesh);
        //    }
        //    else
        //    {
        //        Gizmos.DrawLine(mesh.vertices[0], mesh.vertices[1]);
        //        Gizmos.DrawLine(mesh.vertices[0], mesh.vertices[2]);
        //        Gizmos.DrawLine(mesh.vertices[1], mesh.vertices[2]);
        //    }
        //}


        private void OnValidate()
        {
            RefreshVertices();
            MoveToCentroid();
        }
    }
}
