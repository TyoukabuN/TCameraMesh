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

    static Vector2 scroll_value = Vector2.zero;

    public static AnimBool animBool_vertex;
    public static AnimBool animBool_trangle;
    public static AnimBool animBool_editor;
    public static AnimBool animBool_storyCamera;
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
        animBool_storyCamera = new AnimBool(true);
        animBool_storyCamera.valueChanged.AddListener(Repaint);
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
                else if (trangleEditorMode)
                {
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
        scroll_value = EditorGUILayout.BeginScrollView(scroll_value);

        EditorGUILayout.BeginVertical();
        {
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
                    for (int i = 0; i < tCameraVertexs.Length; i++)
                    {
                        gobjs.Add(tCameraVertexs[i].gameObject);
                    }

                    Selection.objects = gobjs.ToArray();
                }

            }

            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
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
        }


        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            animBool_editor.target = EditorGUILayout.BeginToggleGroup("编辑模式", animBool_editor.target);

            if (EditorGUILayout.BeginFadeGroup(animBool_editor.faded))
            {
                GUI.color = vertexEditorMode ? new Color(0f, 0.95f, 0.95f, 1f) : Color.white;
                string buttonStr = vertexEditorMode ? "关闭<顶点>编辑模式" : "开启<顶点>编辑模式";
                if (GUILayout.Button(new GUIContent(buttonStr, "<顶点>编辑模式下,框选只会选择到<顶点>,而且激活生成顶点快捷键<Ctrl + Q> 和顶 点合并快捷键<Ctrl + X>")))
                {
                    vertexEditorMode = !vertexEditorMode;
                }
                GUI.color = Color.white;

                GUI.color = trangleEditorMode ? new Color(0f, 0.95f, 0.95f, 1f) : Color.white;
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
        }



        if (GUILayout.Button(new GUIContent("合并顶点成三角形", "<顶点>编辑模式下,激活快捷键<Ctrl +X>")))
        {
            CombineVerticesAsTrangle();
        }

        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();


        //#if TYOU_LAB
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            animBool_storyCamera.target = EditorGUILayout.BeginToggleGroup("剧情镜头校对", animBool_storyCamera.target);

            if (EditorGUILayout.BeginFadeGroup(animBool_storyCamera.faded))
            {
                storyCamera = EditorGUILayout.ObjectField("剧情镜头", storyCamera, typeof(Camera), true) as Camera;
                storyAvatar = EditorGUILayout.ObjectField("剧情Avatar", storyAvatar, typeof(Transform), true) as Transform;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                {
                    tcamTrangle = EditorGUILayout.ObjectField("三角形", tcamTrangle, typeof(TCameraTrangle), true) as TCameraTrangle;
                    tcamVertex = EditorGUILayout.ObjectField("顶点", tcamVertex, typeof(TCameraVertex), true) as TCameraVertex;
                    //sign = EditorGUILayout.Vector2Field("符号<Sign>", sign);
                    EditorGUILayout.EndVertical();
                }
                if (GUILayout.Button("校对"))
                {
                    if (!storyCamera)
                        return;
                    if (!storyAvatar)
                        return;

                    cameraTrans = null;
                    Root = null;
                    AxiY = null;
                    AxiX = null;


                    if (Root == null)
                    {
                        var obj = new GameObject("Root");
                        Root = obj.transform;
                    }
                    Root.transform.localPosition = Vector3.zero;

                    if (AxiY == null)
                    {
                        var obj = new GameObject("AxiY");
                        obj.transform.SetParent(Root);
                        obj.transform.position = storyAvatar.position;
                        AxiY = obj.transform;
                    }
                    // AxiY.transform.localPosition = Vector3.zero;

                    if (AxiX == null)
                    {
                        var obj = new GameObject("AxiX");
                        obj.transform.SetParent(AxiY);
                        AxiX = obj.transform;
                    }
                    AxiX.transform.localPosition = Vector3.zero;

                    if (cameraTrans == null)
                    {
                        var gobj = new GameObject("tempCamera");
                        cameraTrans = gobj.transform;
                    }
                    cameraTrans.transform.SetParent(AxiX);
                    cameraTrans.transform.localPosition = Vector3.zero;
                    cameraTrans.transform.localRotation = Quaternion.identity;

                    AxiY.transform.localEulerAngles = new Vector3(0, storyCamera.transform.localEulerAngles.y, 0);
                    AxiX.transform.localEulerAngles = new Vector3(storyCamera.transform.localEulerAngles.x, 0, 0);

                    float disZ = Vector3.Distance(storyAvatar.position, storyCamera.transform.position);
                    var vecPctoP1 = storyAvatar.position - storyCamera.transform.position;
                    vecPctoP1.Normalize();
                    var angle1 = Vector3.Angle(vecPctoP1, storyCamera.transform.forward);
                    disZ *= Mathf.Cos(angle1 * Mathf.Deg2Rad);
                    cameraTrans.transform.localPosition = new Vector3(0, 0, -disZ);

                    AxiX.transform.position += storyCamera.transform.position - cameraTrans.transform.position;

                    var eularAngle = new Vector3(storyCamera.transform.localEulerAngles.x, storyCamera.transform.localEulerAngles.y, disZ);
                    var pivotPosition = AxiX.transform.localPosition;

                    bool isModifyTrangle = false;
                    bool isModifyVertex = false;
                    if (tcamTrangle)
                    {
                        foreach (var vertex in tcamTrangle.camVertices)
                        {
                            if (vertex)
                            {
                                vertex.EularAngle = eularAngle;
                                vertex.PivotPosition = pivotPosition;
                                isModifyVertex = true;
                            }
                        }
                        isModifyTrangle = true;
                    }

                    if (tcamVertex)
                    {
                        tcamVertex.EularAngle = eularAngle;
                        tcamVertex.PivotPosition = pivotPosition;
                        isModifyVertex = true;
                    }


                    var res = EditorUtility.DisplayDialog("校对结束",
                        string.Format("avatar世界坐标:{0}\n剧情镜头角度:{1}\n剧情镜头偏移:{2}\n有无修改三角形：{3}\n有无修改顶点：{4}\n",
                        storyAvatar.transform.position.ToString("f5"), eularAngle.ToString("f5"), pivotPosition.ToString("f5"), isModifyTrangle ? "有" : "无", isModifyVertex ? "有" : "无"),
                        "ok");

                    if (res)
                    {
                        DestroyImmediate(Root);
                    }
                }

            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
        }
        //#endif

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        {
            animBool_other.target = EditorGUILayout.BeginToggleGroup("其他", animBool_other.target);

            if (EditorGUILayout.BeginFadeGroup(animBool_other.faded))
            {
                if (GUILayout.Button("帮助文档"))
                {
                    OpenDocument();
                }

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    if (GUILayout.Button("创建镜头检测器"))
                    {
                        var objs = GameObject.FindObjectsOfType<SimpleController>();
                        if (objs.Length > 0)
                        {
                            util.SetIcon(objs[0].gameObject, util.Icon.CirclePurple);
                            EditorUtility.DisplayDialog("提醒", "场景中已经存在镜头检测器了，帮你标成圆形紫色了", "知道了");
                            return;
                        }

                        var checker = new GameObject("Checker", new Type[] { typeof(SimpleController) });
                        Undo.RegisterCreatedObjectUndo(checker, "General Checker");
                        util.SetIcon(checker, util.Icon.CirclePurple);


                        TCameraMesh mesh;
                        if (util.TryGetCameraMesh(out mesh))
                        {
                            mesh.SetTarget(checker.transform);
                            checker.transform.SetParent(mesh.transform, false);
                            checker.transform.position = Vector3.zero;
                            //如果有顶点，放到第一个顶点那里去
                            var vertices = mesh.GetAllVertices();
                            if (vertices.Count > 0 && vertices[0] != null)
                            {
                                checker.transform.position = vertices[0].transform.position;
                            }
                        }
                    }

                    FixedCheckerPositionToMeshSurface = EditorGUILayout.ToggleLeft("贴合网格表面", FixedCheckerPositionToMeshSurface, GUILayout.MaxWidth(86));
                    if (SimpleController.current != null)
                    {
                        SimpleController.current.FixedPositionToMeshsSurface = FixedCheckerPositionToMeshSurface;
                    }
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
            }
            EditorGUILayout.EndFadeGroup();
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }
    private Transform cameraTrans;
    private Transform Root;
    private Transform AxiY;
    private Transform AxiX;
    private Vector2 sign = Vector2.one;
    private Camera storyCamera = null;
    private Transform storyAvatar = null;
    private TCameraVertex tcamVertex = null;
    private TCameraTrangle tcamTrangle = null;

    static void OpenDocument()
    {
        WWW www = new WWW("https://docs.qq.com/doc/DY0JqTVFyWGRFSGdi");
        Application.OpenURL(www.url);
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
