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

public class TCameraEditorWindow : EditorWindow
{
    [MenuItem("TCam/ToolBar")]
    static void Open()
    {
        EditorWindow.GetWindow<TCameraEditorWindow>().Init();
    }

    void OnDestroy()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
    }

    static TCameraEditorWindow()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    private void Init()
    {
        SceneView.onSceneGUIDelegate -= OnSceneGUI;
        SceneView.onSceneGUIDelegate += OnSceneGUI;
    }

    public static AnimBool animBool_vertex;
    public static AnimBool animBool_trangle;
    public static AnimBool animBool_editor;
    public static AnimBool animBool_other;

    public static bool FixedCheckerPositionToMeshSurface = true;
    void OnEnable()
    {
        animBool_vertex = new AnimBool(false);
        animBool_vertex.valueChanged.AddListener(Repaint);
        animBool_trangle = new AnimBool(false);
        animBool_trangle.valueChanged.AddListener(Repaint);
        animBool_editor = new AnimBool(true);
        animBool_editor.valueChanged.AddListener(Repaint);
        animBool_other = new AnimBool(true);
        animBool_other.valueChanged.AddListener(Repaint);
    }

    public static bool vertexEditorMode = false;
    public static bool trangleEditorMode = false;
    static void OnSceneGUI(SceneView sceneView)
    {
        OnEditorModeHotkey(sceneView);
        OnEditorModeSelect(sceneView);
    }
    /// <summary>
    /// 编辑模式下的快捷键
    /// </summary>
    /// <param name="sceneView"></param>
    static void OnEditorModeHotkey(SceneView sceneView)
    {
        if (!vertexEditorMode)
            return;

        int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
        if (Event.current.type == EventType.KeyDown &&
        Event.current.control == true &&
         Event.current.keyCode == KeyCode.Q
        )
        {
            GeneralVertex();
        }
        if (Event.current.type == EventType.KeyDown &&
            Event.current.control == true &&
             Event.current.keyCode == KeyCode.X
            )
        {
            CombineVerticesAsTrangle();
        }
    }

    /// <summary>
    /// 编辑模式下的选择过滤
    /// </summary>
    /// <param name="sceneView"></param>
    static void OnEditorModeSelect(SceneView sceneView)
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

            if (Selection.activeObject != null && !temp.Contains(Selection.activeObject))
            {
                temp.Add(Selection.activeObject);
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

            if (Selection.activeObject != null && !temp.Contains(Selection.activeObject))
            {
                temp.Add(Selection.activeObject);
            }
        }

        if (vertexEditorMode || trangleEditorMode)
        {
            Selection.objects = temp.ToArray();

            if (Selection.activeObject.name.LastIndexOf("CVertex") >= 0)
            {
                if (vertexEditorMode && trangleEditorMode)
                {
                }
                else if (vertexEditorMode)
                {
                }
                else if (trangleEditorMode) {
                    Selection.activeObject = null;
                }
            }

            if (Selection.activeObject.name.LastIndexOf("CTrangle") >= 0)
            {
                if (vertexEditorMode && trangleEditorMode)
                {
                }
                else if (vertexEditorMode)
                {
                    Selection.activeObject = null;
                }
                else if (trangleEditorMode)
                {
                }
            }

            if (Selection.activeObject != null && temp.Contains(Selection.activeObject))
            {

            }
            else
            {
                Selection.activeObject = temp.Count > 0 ? temp[0] : null;
            }
        }
    }

    static void GeneralVertex()
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

        var gobj = new GameObject(name, new System.Type[] { typeof(TCameraVertex) });

        Undo.RegisterCreatedObjectUndo(gobj, "Create TCameraVertex");

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

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        animBool_vertex.target = EditorGUILayout.BeginToggleGroup("顶点相关", animBool_vertex.target);
        if (EditorGUILayout.BeginFadeGroup(animBool_vertex.faded))
        { 
            if (GUILayout.Button(new GUIContent("生成Camera顶点", "<顶点>编辑模式下,激活快捷键<Ctrl + Q>")))
            {
                GeneralVertex();
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

                UnityEditor.Undo.RecordObject(tCamearMesh, "Add Trangle");

                for (int i = 0; i < tCameraTrangles.Count; i++)
                {
                    var trangle = tCameraTrangles[i];
                    if (!tCamearMesh.AddTrangle(trangle))
                    {
                        continue;
                    }
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
            string buttonStr = vertexEditorMode? "关闭<顶点>编辑模式" : "开启<顶点>编辑模式";
            if (GUILayout.Button(new GUIContent(buttonStr, "<顶点>编辑模式下,框选只会选择到<顶点>,而且激活生成顶点快捷键<Ctrl + Q> 和顶 点合并快捷键<Ctrl + X>")))
            {
                vertexEditorMode = !vertexEditorMode;
            }
            GUI.color = Color.white;

            GUI.color = trangleEditorMode? new Color(0f, 0.95f, 0.95f, 1f):Color.white;
            buttonStr = trangleEditorMode ? "关闭<三角形>编辑模式" : "开启<三角形>编辑模式";
            if (GUILayout.Button(new GUIContent(buttonStr, "开启后只会框选到<三角形>")))
            {
                trangleEditorMode = !trangleEditorMode;
            }
            GUI.color = Color.white;
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();
        

        if (GUILayout.Button(new GUIContent("合并顶点成三角形", "<顶点>编辑模式下,激活快捷键<Ctrl +X>")))
        {
            CombineVerticesAsTrangle();
        }

        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();


        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        animBool_other.target = EditorGUILayout.BeginToggleGroup("其他", animBool_other.target);

        if (EditorGUILayout.BeginFadeGroup(animBool_other.faded))
        {
            if (GUILayout.Button("帮助文档"))
            {
                WWW www = new WWW("https://docs.qq.com/doc/DY0JqTVFyWGRFSGdi");
                Application.OpenURL(www.url);
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (GUILayout.Button("创建镜头检测器"))
            {
                var objs = GameObject.FindObjectsOfType<SimpleController>();
                if (objs.Length > 0)
                {
                    util.SetIcon(objs[0].gameObject, util.Icon.CirclePurple);
                    EditorUtility.DisplayDialog("提醒", "场景中已经存在镜头检测器了，帮你标成圆形紫色了", "知道了");
                    return;
                }

                var checker = new GameObject("Checker",new Type[] { typeof(SimpleController)});
                Undo.RegisterCreatedObjectUndo(checker, "General Checker");
                util.SetIcon(checker, util.Icon.CirclePurple);


                TCameraMesh mesh;
                if (util.TryGetCameraMesh(out mesh))
                {
                    mesh.SetTarget(checker.transform);
                    checker.transform.SetParent(mesh.transform, false) ;
                    checker.transform.position = Vector3.zero;
                    //如果有顶点，放到第一个顶点那里去
                    var vertices = mesh.GetAllVertices();
                    if (vertices.Count > 0 && vertices[0]!= null)
                    {
                        checker.transform.position = vertices[0].transform.position;
                    }
                }
            }

            FixedCheckerPositionToMeshSurface = EditorGUILayout.ToggleLeft("贴合网格表面", FixedCheckerPositionToMeshSurface,GUILayout.MaxWidth(86));
            if (SimpleController.current != null)
            {
                SimpleController.current.FixedPositionToMeshsSurface = FixedCheckerPositionToMeshSurface;
            }


            EditorGUILayout.EndHorizontal();


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

#if TYOU_LAB

#endif
        }
        EditorGUILayout.EndFadeGroup();
        EditorGUILayout.EndToggleGroup();
        EditorGUILayout.EndVertical();

       
    }

    static void CombineVerticesAsTrangle()
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
            EditorUtility.DisplayDialog("提醒", "请选择至少3个顶点", "知道了");
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
