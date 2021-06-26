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
#if UNITY_EDITOR
    [ExecuteAlways]
#endif
    [DisallowMultipleComponent]
    [HelpURL("https://docs.qq.com/doc/DY0JqTVFyWGRFSGdi")]
    public class TEventMesh : TMeshBase
    {
        [HideInInspector]
        public static TEventMesh current;

        public void Awake()
        {
            if (current == null)
            {
                current = this;
            }
        }

        protected override bool TrangleCheckAndProcess(TTrangle tri)
        {
            var vertices = tri.Vertices.ToArray();

            float[] weight;
            if (TCameraUtility.IsInsideTrangle(vertices, Target.position, out weight))
            {
                if (PowerOn)
                {

                }
                return true;
            }
            return false;
        }


        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            if (!GizmosOn)
                return;

            if (TCameraTrangles.Count < 1)
                return;

            for (int i = 0; i < TCameraTrangles.Count; i++)
            {
                var tri = TCameraTrangles[i] as TEventTrangle;

                if (!tri.transform.parent)
                    continue;

                if (!tri.transform.parent.GetComponent<TEventTrangle>())
                    continue;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(tri.transform.position, tri.transform.parent.position);
                Gizmos.color = Color.white;
            }
        }
    }
}
