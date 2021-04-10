using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TCam;
using util = TCam.TCameraEditorUtility;

public class TCameraEdtorWindow : EditorWindow
{
    [MenuItem("TCam/ToolBar")]
    static void Open()
    {
        EditorWindow.GetWindow<TCameraEdtorWindow>().Init();
    }

    private void Init()
    { 

    }

    void OnGUI()
    {
        if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal(UnityEditor.EditorStyles.helpBox);

            GUILayout.Label("游戏运行中不显示", new GUIStyle() { alignment = TextAnchor.MiddleCenter }, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            return;
        }

        if (UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal(UnityEditor.EditorStyles.helpBox);

            GUILayout.Label("Prefab Mode中不显示", new GUIStyle() { alignment = TextAnchor.MiddleCenter }, GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            return;
        }

        EditorGUILayout.BeginVertical();

        //EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("生产Camera顶点"))
        {
            var tCameraVertexs = GameObject.FindObjectsOfType<TCameraVertex>();

            //Selection.activeObject = SceneView.currentDrawingSceneView;
            var sceneView = SceneView.currentDrawingSceneView;
            if (sceneView == null)
            {
                sceneView = SceneView.lastActiveSceneView;
            }
            Camera sceneCam = sceneView.camera;
            Vector3 spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 10f));


            var name = "CVertex";
            if (tCameraVertexs.Length > 0)
            {
                name = string.Format("CVertex ({0})", tCameraVertexs.Length);
            }

            var gobj = new GameObject(name,new System.Type[]{typeof(TCameraVertex)});
            gobj.transform.position = spawnPos;
        }


        if (GUILayout.Button("选中所有顶点"))
        {
            var tCameraVertexs = GameObject.FindObjectsOfType<TCameraVertex>();

            var gobjs = new List<Object>();
            for(int i=0;i<tCameraVertexs.Length;i++)
            {
                gobjs.Add(tCameraVertexs[i].gameObject);
            }

            Selection.objects = gobjs.ToArray();
        }

        if (GUILayout.Button("标记所有顶点"))
        {
            var tCameraVertexs = GameObject.FindObjectsOfType<TCameraVertex>();

            var gobjs = new List<Object>();
            for (int i = 0; i < tCameraVertexs.Length; i++)
            {
                var gobj = tCameraVertexs[i].gameObject;
                util.SetIcon(gobj, util.Icon.DiamondYellow);
            }
        }

        if (GUILayout.Button("选中所有三角形"))
        {
            var objs = GameObject.FindObjectsOfType<TCameraTrangle>();

            var gobjs = new List<Object>();
            for (int i = 0; i < objs.Length; i++)
            {
                gobjs.Add(objs[i].gameObject);
            }

            Selection.objects = gobjs.ToArray();
        }

        if (GUILayout.Button("标记所有三角形"))
        {
            var objs = GameObject.FindObjectsOfType<TCameraTrangle>();

            var gobjs = new List<Object>();
            for (int i = 0; i < objs.Length; i++)
            {
                var gobj = objs[i].gameObject;
                util.SetIcon(gobj, util.Icon.DiamondTeal);
            }
        }

        if (GUILayout.Button("将所有三角形Gobj移动至重心"))
        {
            var objs = GameObject.FindObjectsOfType<TCameraTrangle>();
            for (int i = 0; i < objs.Length; i++)
            {
                objs[i].MoveToCentroid();
            }

        }

        if (GUILayout.Button("将三角形加入网格"))
        {
            if (Selection.gameObjects.Length <= 0)
            {
                EditorUtility.DisplayDialog("提醒", "请选择至少1个三角形", "知道了");
                return;
            }
            var tCameraTrangles = new List<TCameraTrangle>();
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                var gobj = Selection.gameObjects[i];
                var vertex = gobj.GetComponent<TCameraTrangle>();
                if (vertex == null)
                    continue;

                tCameraTrangles.Add(vertex);
            }

            TCameraMesh tCamearMesh = null;

            if (!util.TryGetCameraMesh(out tCamearMesh))
            {
                return;
            }

            for (int i = 0; i < tCameraTrangles.Count; i++)
            {
                var trangle = tCameraTrangles[i];
                if (!tCamearMesh.AddTrangle(trangle))
                {
                    continue;
                }
                GameObject.DestroyImmediate(trangle);
            }


        }

        //EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("合并顶点成三角形"))
        {
            if (Selection.gameObjects.Length <= 0)
            {
                EditorUtility.DisplayDialog("提醒", "请选择至少3个顶点", "知道了");
                return;
            }
            if (Selection.gameObjects.Length > 3)
            {
                EditorUtility.DisplayDialog("提醒", "请选择的顶点不能超过3个", "知道了");
                return;
            }

            var tCameraVertexList = new List<TCameraVertex>();
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                var gobj = Selection.gameObjects[i];
                var vertex = gobj.GetComponent<TCameraVertex>();
                if (vertex == null)
                    continue;

                if (tCameraVertexList.Count == 3)
                    break;

                tCameraVertexList.Add(vertex);
            }

            if (tCameraVertexList.Count < 3)
            {
                EditorUtility.DisplayDialog("提醒", "请选择至少3个顶点","知道了");
                return;
            }

            if (util.IsTrangleExists(tCameraVertexList.ToArray()))
            {
                EditorUtility.DisplayDialog("提醒", "已存在拥有相同顶点的三角形", "知道了");
                return;
            }


            TCameraEditorUtility.TryNewTrangleFormVertices(tCameraVertexList.ToArray());
        }

        EditorGUILayout.EndVertical();
    }   
    

}
