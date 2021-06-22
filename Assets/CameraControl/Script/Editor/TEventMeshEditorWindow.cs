using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Reflection;
using System;
using util = TMesh.TCameraEditorUtility;
using Object = UnityEngine.Object;


namespace TMesh
{
    public class TEventMeshEditorWindow : TMeshEditorWindowBase<TEventVertex>
    {
        static TCameraEditorWindow current;

        [MenuItem("TMesh/Event Mesh %#T")]
        static void Open()
        {
            EditorWindow.GetWindow<TEventMeshEditorWindow>().Init();
        }

        public static AnimBoolHandle animBool_storyCamera;

        protected override void OnEnable()
        {
            base.OnEnable();
            animBool_storyCamera = new AnimBoolHandle("TCameraMesh_animBool_storyCamera", true);
            animBool_storyCamera.valueChanged.AddListener(Repaint);
        }

        protected override void OnSceneGUI(SceneView sceneView)
        {
            OnEditorModeHotkey<TCameraVertex>(sceneView);
            OnEditorModeSelect<TCameraVertex>(sceneView);
        }
        //DrawMeshConstructTool();
        //👇👇👇
        //OnDrawTool();
        //👇👇👇
        //DrawOtherTool();
        protected override void OnDrawTool()
        {

        }

    }
}

