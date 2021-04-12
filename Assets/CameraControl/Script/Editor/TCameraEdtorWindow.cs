using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Reflection;
using System;
using TCam;
using util = TCam.TCameraEditorUtility;
using Object = UnityEngine.Object;

public class TCameraEdtorWindow : EditorWindow
{
    [MenuItem("TCam/ToolBar")]
    static void Open()
    {
        EditorWindow.GetWindow<TCameraEdtorWindow>().Init();
    }

    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }



    private void Init()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private static bool group_trangle = false;

    public static AnimBool animBool_vertex;
    public static AnimBool animBool_trangle;
    public static AnimBool animBool_editor;

    void OnEnable()
    {
        animBool_vertex = new AnimBool(true);
        animBool_vertex.valueChanged.AddListener(Repaint);
        animBool_trangle = new AnimBool(true);
        animBool_trangle.valueChanged.AddListener(Repaint);
        animBool_editor = new AnimBool(true);
        animBool_editor.valueChanged.AddListener(Repaint);
    }

    public static bool vertexEditorMode = false;
    public static bool trangleEditorMode = false;
    static void OnSceneGUI(SceneView sceneView)
    {
        if (Selection.objects.Length <= 0)
        {
            return;
        }
        List<Object> temp = new List<Object>();

        if (vertexEditorMode)
        { 
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                Object obj = Selection.objects[i];
                if (obj.name.LastIndexOf("CVertex") >= 0)
                {
                    temp.Add(obj);
                }
            }
        }

        if (trangleEditorMode)
        {
            for (int i = 0; i < Selection.objects.Length; i++)
            {
                Object obj = Selection.objects[i];
                if (obj.name.LastIndexOf("CTrangle") >= 0)
                {
                    temp.Add(obj);
                }
            }
        }

        if (vertexEditorMode || trangleEditorMode)
        { 
            Selection.objects = temp.ToArray();
            Selection.activeObject = temp.Count > 0 ? temp[0] : null;
        }
    }
    void OnGUI()
    {

        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        animBool_vertex.target = EditorGUILayout.BeginToggleGroup("顶点相关", animBool_vertex.target);
        if (EditorGUILayout.BeginFadeGroup(animBool_vertex.faded))
        { 
            if (GUILayout.Button("生成Camera顶点"))
            {
                var tCameraVertexs = GameObject.FindObjectsOfType<TCameraVertex>();

                //Selection.activeObject = SceneView.currentDrawingSceneView;
                var sceneView = SceneView.currentDrawingSceneView;
                if (sceneView == null)
                {
                    sceneView = SceneView.lastActiveSceneView;
                }
                Camera sceneCam = sceneView.camera;
                Vector3 spawnPos = sceneCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 30f));


                var name = "CVertex";
                if (tCameraVertexs.Length > 0)
                {
                    name = string.Format("CVertex ({0})", tCameraVertexs.Length);
                }

                var gobj = new GameObject(name,new System.Type[]{typeof(TCameraVertex)});

                Undo.RecordObject(gobj, "Create TCameraVertex");

                gobj.transform.position = spawnPos;

                util.SetIcon(gobj, util.Icon.DiamondYellow);

                Selection.activeObject = gobj;

                TCameraMesh tCamearMesh = null;
                if (util.TryGetCameraMesh(out tCamearMesh))
                {
                    gobj.transform.SetParent(tCamearMesh.transform, false);

                    /*
                     * 将刚生成出来的顶点移动到最后的三角形的顶点所在平面上去
                     * 如果没有就移动了
                     */
                    if (tCamearMesh.TCameraTrangles.Count > 0)
                    {
                        var trangle = tCamearMesh.TCameraTrangles[tCamearMesh.TCameraTrangles.Count - 1];
                        Vector3 a = trangle[2].transform.position - trangle[0].transform.position;
                        Vector3 b = trangle[1].transform.position - trangle[0].transform.position;
                        Vector3 c = sceneCam.transform.position - trangle[0].transform.position;
                        Vector3 p = spawnPos - trangle[0].transform.position;

                        var normal = Vector3.Cross(a, b);
                        var cos = Vector3.Dot(c.normalized, normal.normalized);
                        var perpendicular = normal.normalized * c.magnitude * cos;

                        var parallel = c - perpendicular;
                        //var project = parallel + trangle[0].transform.position;

                        parallel = parallel + trangle[0].transform.position;
                        //var project = Vector3.Cross(normal,Vector3.Cross(c, normal)) + trangle[0].transform.position;

                        var pc = parallel - sceneCam.transform.position;
                        var pp = spawnPos - sceneCam.transform.position;
                        cos = Vector3.Dot(pc.normalized, pp.normalized);
                        var pp2Len = pc.magnitude / cos;

                        gobj.transform.position = pp.normalized * pp2Len + sceneCam.transform.position;
                    }

                }

                
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

        }

        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        animBool_trangle.target = EditorGUILayout.BeginToggleGroup("三角形相关", animBool_trangle.target);

        if (EditorGUILayout.BeginFadeGroup(animBool_trangle.faded))
        {


            if (GUILayout.Button("将所有三角形Gobj移动至重心"))
            {
                MoveAllTrangleGobjToCentroid();
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
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();



        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        animBool_editor.target = EditorGUILayout.BeginToggleGroup("编辑模式", animBool_editor.target);

        if (EditorGUILayout.BeginFadeGroup(animBool_editor.faded))
        {
            GUI.color = vertexEditorMode ? new Color(0f, 0.95f, 0.95f, 1f) : Color.white;
            string buttonStr = vertexEditorMode? "关闭顶点编辑模式" : "开启顶点编辑模式";
            if (GUILayout.Button(buttonStr))
            {
                vertexEditorMode = !vertexEditorMode;
            }
            GUI.color = Color.white;

            GUI.color = trangleEditorMode? new Color(0f, 0.95f, 0.95f, 1f):Color.white;
            buttonStr = trangleEditorMode ? "关闭三角形编辑模式" : "开启三角形编辑模式";
            if (GUILayout.Button(buttonStr))
            {
                trangleEditorMode = !trangleEditorMode;
            }
            GUI.color = Color.white;
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();


        

        if (GUILayout.Button("合并顶点成三角形"))
        {
            if (Selection.gameObjects.Length <= 0)
            {
                EditorUtility.DisplayDialog("提醒", "请选择至少3个顶点", "知道了");
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


            TCameraTrangle trangle;
            if (TCameraEditorUtility.TryNewTrangleFormVertices(tCameraVertexList.ToArray(), out trangle))
            {
                util.SetIcon(trangle.gameObject, util.Icon.DiamondTeal);
            }
        }

        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("选择网格对象"))
        {
            var objs = GameObject.FindObjectsOfType<TCameraMesh>();

            var gobjs = new List<Object>();
            for (int i = 0; i < objs.Length; i++)
            {
                gobjs.Add(objs[i].gameObject);
            }

            Selection.objects = gobjs.ToArray();
        }

        if (GUILayout.Button("显示/隐藏所有标记"))
        {
            if (!maskSwitch)
            {
                MaskAllVertex();
                MaskAllTrangle();
                MoveAllTrangleGobjToCentroid();
                maskSwitch = true;
                return;
            }

            maskSwitch = false;
            var tCameraVertexs = GameObject.FindObjectsOfType<TCameraVertex>();

            var gobjs = new List<Object>();
            for (int i = 0; i < tCameraVertexs.Length; i++)
            {
                var gobj = tCameraVertexs[i].gameObject;

                Undo.RecordObject(gobj, "Clear All TCameraVertex Mask");

                util.CleanIcon(gobj);
            }

            var objs = GameObject.FindObjectsOfType<TCameraTrangle>();

            gobjs = new List<Object>();
            for (int i = 0; i < objs.Length; i++)
            {
                var gobj = objs[i].gameObject;

                Undo.RecordObject(gobj, "Clear All TCameraTrangle Mask");

                util.CleanIcon(gobj);
            }
        }

        if (GUILayout.Button("显示/隐藏网格"))
        {
            TCameraMesh tCamearMesh = null;

            if (util.TryGetCameraMesh(out tCamearMesh))
            {
                tCamearMesh.GizmosOn = !tCamearMesh.GizmosOn;
                SceneView.RepaintAll();
            }
        }

        if (GUILayout.Button("Init"))
        {
            Init();
        }
    }

    private static void MoveAllTrangleGobjToCentroid()
    {
        var objs = GameObject.FindObjectsOfType<TCameraTrangle>();

        var gobjs = new List<Object>();
        for (int i = 0; i < objs.Length; i++)
        {
            var gobj = objs[i].gameObject;
            util.SetIcon(gobj, util.Icon.DiamondTeal);
        }

        objs = GameObject.FindObjectsOfType<TCameraTrangle>();
        for (int i = 0; i < objs.Length; i++)
        {
            objs[i].MoveToCentroid();
        }
    }

    private static void MaskAllTrangle()
    {
        var objs = GameObject.FindObjectsOfType<TCameraTrangle>();

        var gobjs = new List<Object>();
        for (int i = 0; i < objs.Length; i++)
        {
            var gobj = objs[i].gameObject;
            util.SetIcon(gobj, util.Icon.DiamondTeal);
        }
    }
    private static bool maskSwitch = false;
    private static void MaskAllVertex()
    {
        var tCameraVertexs = GameObject.FindObjectsOfType<TCameraVertex>();

        var gobjs = new List<Object>();
        for (int i = 0; i < tCameraVertexs.Length; i++)
        {
            var gobj = tCameraVertexs[i].gameObject;
            Undo.RecordObject(gobj, "Mask All TCameraVertex");
            util.SetIcon(gobj, util.Icon.DiamondYellow);
        }
    }
}
