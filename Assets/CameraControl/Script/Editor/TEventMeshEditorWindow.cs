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
        static TEventMeshEditorWindow current;

        [MenuItem("TMesh/Event Mesh %#G")]
        static void Open()
        {
            if (!current)
            {
                current = EditorWindow.GetWindow<TEventMeshEditorWindow>();
                current.Init();
            }
            else
            {
                current.Close();
                current = null;
            }
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

