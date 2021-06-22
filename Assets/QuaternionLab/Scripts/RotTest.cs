using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class RotTest : MonoBehaviour
{
    public Transform RefTrans;
    public Vector3 RefAxi;

    public Transform Rotator;

    public static RotTest current;

    protected Quaternion targetRot = Quaternion.identity;

    public float Slerp_T = 0.5f;

    protected void OnEnable()
    {
        if (current == null)
        { 
            current = this;
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (Rotator == null)
            return;

        Rotator.rotation = Quaternion.Slerp(Rotator.rotation, targetRot, Slerp_T);
    }

    public void Test(Quaternion rotation)
    {
        targetRot = rotation;
    }
}

[CustomEditor(typeof(RotTest))]
public class RotTestEditor: Editor
{
    SerializedProperty RefTrans;
    SerializedProperty Rotator;

    public Vector4 qtn = Vector4.zero;

    protected void OnEnable()
    {
        RefTrans = serializedObject.FindProperty("RefTrans");
        Rotator = serializedObject.FindProperty("Rotator");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(RefTrans);
        EditorGUILayout.PropertyField(Rotator);
        qtn = EditorGUILayout.Vector4Field("",qtn);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("读取rotation") && RotTest.current)
        {
            var trans = RotTest.current.RefTrans.rotation;
            qtn.Set(trans.x, trans.y, trans.z, trans.w);
        }
        if (GUILayout.Button("旋转"))
        {
            DoRot();
        }
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();
    }

    public void DoRot()
    {
        if (RotTest.current == null)
            return;

        RotTest.current.Test(new Quaternion(qtn.x, qtn.y, qtn.z, qtn.w));
    }


    public class RotTestEditorWindow : EditorWindow
    {
        public List<Vector4> qtns = new List<Vector4>();
        public Vector4 refQtn = Vector4.zero;
        protected float Slerp_T = 0.5f;


        [MenuItem("Test/Quaternion %#R")]
        static void Open()
        {
            GetWindow<RotTestEditorWindow>();
        }
        protected void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                var count = EditorGUILayout.IntField("Qutarnion Count", qtns.Count);
                if (count != qtns.Count)
                {
                    if (count > qtns.Count)
                    {
                        while (qtns.Count != count)
                        {
                            try
                            {
                                if (count > qtns.Count)
                                {
                                    qtns.Add(Vector4.one);
                                }
                                else
                                {
                                    qtns.RemoveAt(qtns.Count - 1);
                                }
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }
                    count = qtns.Count;
                }

                for (int i = 0; i < qtns.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        qtns[i] = EditorGUILayout.Vector4Field("", qtns[i]);

                        if (GUILayout.Button("读取rotation") && RotTest.current)
                        {
                            var trans = RotTest.current.RefTrans.rotation;
                            qtns[i] = new Vector4(trans.x, trans.y, trans.z, trans.w);
                        }
                        if (GUILayout.Button("旋转"))
                        {
                            DoRot(qtns[i]);
                        }
                        if (GUILayout.Button("C"))
                        {
                            qtns.Add(qtns[i]);
                        }
                        if (GUILayout.Button("Del"))
                        {
                            qtns.RemoveAt(i);
                            i--;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
            }

            if (RotTest.current)
            { 
                var refRot = RotTest.current.RefTrans.rotation;
                refQtn.Set(refRot.x, refRot.y, refRot.z, refRot.w);
                EditorGUILayout.Vector4Field("", refQtn);

                var t = EditorGUILayout.FloatField("Slerp t: ", RotTest.current.Slerp_T);
                RotTest.current.Slerp_T = t;
            }
        }



        public void DoRot(Vector4 qtn)
        {
            if (RotTest.current == null)
                return;

            RotTest.current.Test(new Quaternion(qtn.x, qtn.y, qtn.z, qtn.w));
        }
    }
}

