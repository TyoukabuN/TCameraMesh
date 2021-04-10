using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TCam
{
    public class TCameraTrangle : MonoBehaviour
    {
        [SerializeField]
        public TCameraVertex[] camVertices;
        
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
            if (Vertices.Count < 3)
            {
                return;
            }
            var centroid = TCameraUtility.CalCentroid(Vertices.ToArray());
            transform.position = centroid;
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

        //    if (tCameraMesh.Target == null)
        //        return;

        //    if (Vertices.Count < 3)
        //            return;

        //    Mesh mesh = new Mesh();
        //    mesh.vertices = Vertices.ToArray();
        //    mesh.triangles = new int[] { 0, 1, 2 };
        //    mesh.RecalculateNormals();
        //    mesh.RecalculateBounds();

        //    Gizmos.color = Color.red;

        //    if (TCameraUtility.IsInsideTrangleS(mesh.vertices, tCameraMesh.Target.position))
        //    {
        //        Gizmos.color = Color.green;
        //    }
        //    Gizmos.DrawMesh(mesh);

        //    Gizmos.color = Color.white;

        //    Gizmos.DrawWireMesh(mesh);

        //}


        private void OnValidate()
        {
            RefreshVertices();
        }
    }
}
