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
    public class TCameraMesh : TMeshBase
    {
        [HideInInspector]
        public static TCameraMesh currentTCameraMesh;
        //[HideInInspector]
        // protected Dictionary<TCameraVertex, List<TTrangle>> vertex2TranglesDict = new Dictionary<TCameraVertex, List<TTrangle>>();

      
        /// <summary>
        /// will pass some args of tCameraVertex
        /// </summary>
        public CameraMeshEvent OnPositionChanged;
        public CameraMeshEventWithSplitArgs OnPositionChangedWithSplitArgs;
        /// <summary>
        /// will pass some args of tCameraVertex
        /// </summary>
        public CameraMeshComplexEvent OnComplexEvent;

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
            if (OnPositionChangedWithSplitArgs == null)
            {
                OnPositionChangedWithSplitArgs = new CameraMeshEventWithSplitArgs();
            }
            if (OnComplexEvent == null)
            {
                OnComplexEvent = new CameraMeshComplexEvent();
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
                    var tVertices = tri.vertices;
                    var eulerAngles = (tVertices[0] as TCameraVertex).EularAngle * weight[0] +
                        (tVertices[1] as TCameraVertex).EularAngle * weight[1] +
                        (tVertices[2] as TCameraVertex).EularAngle * weight[2];

                    var pivotPosition = (tVertices[0] as TCameraVertex).PivotPosition * weight[0] +
                        (tVertices[1] as TCameraVertex).PivotPosition * weight[1] +
                        (tVertices[2] as TCameraVertex).PivotPosition * weight[2];

                    //Add Other args

                    if (OnPositionChanged != null)
                    {
                        OnPositionChanged.Invoke(eulerAngles, pivotPosition);
                    }
                    if (OnPositionChangedWithSplitArgs != null)
                    {
                        OnPositionChangedWithSplitArgs.Invoke(eulerAngles.x, eulerAngles.y, eulerAngles.z);
                    }
                    if (OnComplexEvent != null)
                    {
                        OnComplexEvent.Invoke(eulerAngles, pivotPosition, weight);
                    }
                }
                return true;
            }
            return false;
        }

        public class CameraMeshEvent : UnityEvent<Vector3,Vector3> { }
        public class CameraMeshEventWithSplitArgs : UnityEvent<float, float, float> { }
        public class CameraMeshComplexEvent : UnityEvent<Vector3, Vector3, float[]> { }
    }
}
