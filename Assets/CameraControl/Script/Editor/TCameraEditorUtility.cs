using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace TCam
{ 
    public class TCameraEditorUtility
    {
        public static bool TryGetCameraMesh(out TCameraMesh tCameraMesh)
        {
            tCameraMesh = GameObject.FindObjectOfType<TCameraMesh>();

            if (tCameraMesh == null)
            {
                GameObject gobj = new GameObject("TCameraMesh");
                gobj.transform.position = Vector3.zero;
                tCameraMesh = gobj.AddComponent<TCameraMesh>();
                SetIcon(gobj, Icon.DiamondPurple);
            }

            if (tCameraMesh != null)
            {
                TCameraMesh.currentTCameraMesh = tCameraMesh;
            }

            return tCameraMesh != null;
        }


        private static TCameraVertex[] book;
        private static Dictionary<TCameraVertex, bool> book2;
        private static bool TryGetClockwiseOrderLoop(TCameraVertex[] vertices,int step = 0)
        {
            if (step == 3)
            {
                return TCameraUtility.IsPositiveOrder(book);
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                var vertex = vertices[i];
                if (book2.ContainsKey(vertex))
                    continue;

                book[step] = vertex;
                book2[vertex] = true;

                step++;
                if (TryGetClockwiseOrderLoop(vertices, step))
                {
                    return true;
                }
                book2.Remove(vertex);
                step--;
            }

            return false;
        }

        public static bool TryGetClockwiseOrder(TCameraVertex[] vertices,out TCameraVertex[] res)
        {
            book = new TCameraVertex[3];
            book2 = new Dictionary<TCameraVertex, bool>();
            var res1 = TryGetClockwiseOrderLoop(vertices);
            
            res = book;

            return res1;
        }
        public static bool TryNewTrangleFormVertices(TCameraVertex[] vertices,out TCameraTrangle trangle)
        {
            trangle = null;
            TCameraMesh tCamearMesh = null;

            if (!TryGetCameraMesh(out tCamearMesh))
            {
                return false;
            }

            TCameraVertex[] positiveOrder;
            if (!TryGetClockwiseOrder(vertices,out positiveOrder))
            {
                return false;
            }

            var tCameraTrangle = GameObject.FindObjectsOfType<TCameraTrangle>();

            var name = "CTrangle";
            if (tCameraTrangle.Length > 0)
            {
                name = string.Format("CTrangle ({0})", tCameraTrangle.Length);
            }


            var gobj = new GameObject(name,typeof(TCameraTrangle));
            UnityEditor.Undo.RegisterCreatedObjectUndo(gobj, "New Trangle");

            trangle = gobj.GetComponent<TCameraTrangle>();
            trangle.camVertices = positiveOrder;

            gobj.transform.SetParent(tCamearMesh.transform, false);
            trangle.MoveToCentroid();


            UnityEditor.Undo.RecordObject(tCamearMesh, "Add Trangle");

            if (!tCamearMesh.AddTrangle(trangle))
            {
                GameObject.DestroyImmediate(trangle);
                return false;
            }
            return true;
        }

        public static bool IsTrangleExists(TCameraVertex[] tCameraVertices)
        {
            TCameraMesh tCamearMesh = null;

            if (!TryGetCameraMesh(out tCamearMesh))
            {
                return true;
            }

            if (tCamearMesh.TCameraTrangles.Count <= 0)
            {
                return false;
            }

            var dict = new Dictionary<TCameraVertex, bool>();
            for (int i = 0; i < tCameraVertices.Length; i++)
            {
                var vertex = tCameraVertices[i];
                dict[vertex] = true;
            }

            for (int i = 0; i < tCamearMesh.TCameraTrangles.Count; i++)
            {
                var trangle = tCamearMesh.TCameraTrangles[i];

                if (trangle.camVertices.All((vertex) => dict.ContainsKey(vertex)))
                {
                    return true;
                }
            }

            return false;
        }

        public enum LabelIcon
        {
            Gray = 0,
            Blue,
            Teal,
            Green,
            Yellow,
            Orange,
            Red,
            Purple
        }

        public enum Icon
        {
            CircleGray = 0,
            CircleBlue,
            CircleTeal,
            CircleGreen,
            CircleYellow,
            CircleOrange,
            CircleRed,
            CirclePurple,
            DiamondGray,
            DiamondBlue,
            DiamondTeal,
            DiamondGreen,
            DiamondYellow,
            DiamondOrange,
            DiamondRed,
            DiamondPurple
        }

        private static GUIContent[] labelIcons;
        private static GUIContent[] largeIcons;

        public static void SetIcon(GameObject gObj, LabelIcon icon)
        {
            if (labelIcons == null)
            {
                labelIcons = GetTextures("sv_label_", string.Empty, 0, 8);
            }

            SetIcon(gObj, labelIcons[(int)icon].image as Texture2D);
        }

        public static void SetIcon(GameObject gObj, Icon icon)
        {
            if (largeIcons == null)
            {
                largeIcons = GetTextures("sv_icon_dot", "_pix16_gizmo", 0, 16);
            }

            SetIcon(gObj, largeIcons[(int)icon].image as Texture2D);
        }

        private static void SetIcon(GameObject gObj, Texture2D texture)
        {
            var ty = typeof(EditorGUIUtility);
            var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Invoke(null, new object[] { gObj, texture });
        }

        public static void CleanIcon(GameObject gObj)
        {
            var ty = typeof(EditorGUIUtility);
            var mi = ty.GetMethod("SetIconForObject", BindingFlags.NonPublic | BindingFlags.Static);
            mi.Invoke(null, new object[] { gObj, null });
        }

        private static GUIContent[] GetTextures(string baseName, string postFix, int startIndex, int count)
        {
            GUIContent[] guiContentArray = new GUIContent[count];

            for (int index = 0; index < count; ++index)
            {
                guiContentArray[index] = EditorGUIUtility.IconContent(baseName + (object)(startIndex + index) + postFix);
            }

            return guiContentArray;
        }

    }
}
