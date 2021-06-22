using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TMesh
{
    [HelpURL("https://docs.qq.com/doc/DY0JqTVFyWGRFSGdi")]
    public class TTrangle : MonoBehaviour
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

        protected List<Vector3> m_Vertices;
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
        protected void RefreshVertices()
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

        protected void OnValidate()
        {
            RefreshVertices();
            MoveToCentroid();
        }
    }
}
