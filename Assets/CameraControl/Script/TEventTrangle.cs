using System.Collections;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using UnityEngine;
using UnityEditor;
using UnityEngine.Events;
using System;
using TriggerCondition = TMesh.TEvent.TriggerCondition;

namespace TMesh
{
    [Serializable]
    public class TEventTrangle : TTrangle
    {
        public List<TEvent> Events = new List<TEvent>();

        public List<TEventTrangle> Childs = new List<TEventTrangle>();


        private TEventTrangle _parent;
        private Transform _lastParentTrans = null;
        public TEventTrangle parent
        {
            get {
                if (!transform.parent)
                    return null;

                if (_lastParentTrans != transform.parent)
                { 
                    _parent = transform.parent.GetComponent<TEventTrangle>();
                    _lastParentTrans = transform.parent;
                }

                return _parent; 
            }
            set { SetParent(value); }
        }
        public void SetParent(TEventTrangle parent)
        {
            if (parent == null)
            {
                _parent = null;
                return;
            }
            parent.AddChild(this);
        }
        public void AddChild(TEventTrangle obj)
        {
            if (!Childs.Contains(obj))
            {
                Childs.Add(obj);

                obj.parent = this;

                if (this.parent == this)
                {
                    this.parent = null;
                }
            }
        }
        public TEventTrangle GetChild(int index)
        {
            return Childs[index];
        }

        public override void Tick()
        {
            if (parent)
            {
                parent.Tick();
                return;
            }
            foreach (var tevent in Events)
            {
                tevent.Tick();
            }
        }
        public override void OnEnterTrangle()
        {
            if (parent)
            {
                parent.OnEnterTrangle();
                return;
            }
            foreach (var tevent in Events)
            {
                tevent.OnEnter();
            }
        }
        public override void OnExitTrangle() 
        {
            if (parent)
            {
                parent.OnExitTrangle();
                return;
            }
            foreach (var tevent in Events)
            {
                tevent.OnExit();
            }
        }
    }

    [CustomEditor(typeof(TEventTrangle))]
    public class TEventTrangleEditor : Editor
    {
        public enum EventTypeForEditor
        {
            Timeline,
            Animation,
            Do,
        }

        public enum ConditionTypeForEditor
        {
            进入网格时,
            离开网格时,
            在网格里等待一下,
        }

        EventTypeForEditor display = EventTypeForEditor.Timeline;


        public static TEventTrangle obj;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            SerializedProperty listIterator = serializedObject.FindProperty("Events");

            obj = target as TEventTrangle;

            if (obj.Events.Count > 0)
            {
                for (int i = 0; i < obj.Events.Count; i++)
                {
                    EditorGUI.BeginChangeCheck();

                    var eventObject = obj.Events[i];
                    var type = eventObject.GetType();

                    var eventElement = listIterator.GetArrayElementAtIndex(i);

                    TriggerEventType eventType = (TriggerEventType)eventElement.FindPropertyRelative("eventType").enumValueIndex;

                    SerializedProperty playableDirector = eventElement.FindPropertyRelative("playableDirector");
                    SerializedProperty animation = eventElement.FindPropertyRelative("animation");
                    SerializedProperty onTrigger = eventElement.FindPropertyRelative("onTrigger");
                    TriggerCondition condition = (TriggerCondition)(eventElement.FindPropertyRelative("condition").enumValueIndex);
                    var useUnscaledTime = eventElement.FindPropertyRelative("useUnscaledTime");
                    var waitingSceond = eventElement.FindPropertyRelative("waitingSceond");


                    //public PlayableDirector playableDirector;
                    //public Animation animation;
                    //public OnTargetEvent onTrigger = new OnTargetEvent();


                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    EditorGUILayout.BeginVertical();
                    if (eventType == TriggerEventType.Timeline)
                    {
                        EditorGUILayout.PropertyField(playableDirector, new GUIContent("Playable Director"));
                    }
                    else if (eventType == TriggerEventType.Animation)
                    {
                        EditorGUILayout.PropertyField(animation, new GUIContent("Animation"));
                    }
                    else if (eventType == TriggerEventType.Do)
                    {
                        EditorGUILayout.PropertyField(onTrigger, new GUIContent("要触发的函数"));
                    }

                    condition = (TriggerCondition)EditorGUILayout.EnumPopup(new GUIContent("触发条件"),(ConditionTypeForEditor)condition);
                    if (condition == TEvent.TriggerCondition.WaitASecond)
                    {
                        EditorGUILayout.PropertyField(useUnscaledTime, new GUIContent("不受TimeScaleT影响"));
                        EditorGUILayout.PropertyField(waitingSceond, new GUIContent("等待时间"));
                    }
                    EditorGUILayout.EndVertical();

                    if (GUILayout.Button("删除"))
                    {
                        Undo.RecordObject(obj, "TEvent Remove Event");
                        obj.Events.RemoveAt(i);
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (EditorGUI.EndChangeCheck())
                    {
                        eventObject.condition = condition;
                    }

                };
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            EditorGUILayout.LabelField("要添加的类型:", GUILayout.Width(30f*2.7f));
            display = (EventTypeForEditor)EditorGUILayout.EnumPopup(display, GUILayout.Width(80));
            if (GUILayout.Button("添加事件"))
            { 
                switch (display)
                {
                    case EventTypeForEditor.Timeline:
                        Undo.RecordObject(obj, "TEvent Add PlayableDirector Event");
                        obj.Events.Add(new TEvent(TriggerEventType.Timeline));
                        break;
                    case EventTypeForEditor.Animation:
                        Undo.RecordObject(obj, "TEvent Add Animation Event");
                        obj.Events.Add(new TEvent(TriggerEventType.Animation));
                        break;
                    case EventTypeForEditor.Do:
                        Undo.RecordObject(obj, "TEvent Add Animation Event");
                        obj.Events.Add(new TEvent(TriggerEventType.Do));
                        break;
                    default:
                        break; 
                }

                EditorUtility.SetDirty(obj);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(obj);
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();


        }
    }

    [Serializable]
    public class OnTargetEvent : UnityEvent
    {
    }

    [Serializable]
    public enum TriggerEventType
    {
        Timeline,
        Animation,
        Do,
    }

    [Serializable]
    public class TEvent
    {
        public TriggerEventType eventType = TriggerEventType.Timeline;
        public TEvent(TriggerEventType eventType)
        {
            this.eventType = eventType;
        }
        public PlayableDirector playableDirector;
        public Animation animation;
        public OnTargetEvent onTrigger = new OnTargetEvent();

        public TTrangle trangle;

        public bool isDone = false;

        public int CanTriggerTimes = 1;
        public enum TriggerCondition : int
        {
            Enter,
            Exit,
            WaitASecond,
        }

        public TriggerCondition condition = TriggerCondition.Enter;

        public string error = string.Empty;

        public int enterCount = 0;


        public bool useUnscaledTime = true;

        public float waitingSceond = 0f;

        private float waitingCounter = 0f;

        public virtual bool Tick()
        {
            if (isDone)
                return false;

            if (!trangle)
                return false;

            if (!string.IsNullOrEmpty(error))
                return false;

            if (condition == TriggerCondition.WaitASecond)
            {
                if (waitingSceond <= 0f)
                {
                    return true;
                }

                waitingCounter += useUnscaledTime?Time.unscaledTime:Time.deltaTime;

                if (waitingCounter >= waitingSceond)
                {
                    OnCondition(TriggerCondition.WaitASecond);
                    return true;
                }
            }

            return false;
        }

        private bool OnCondition(TriggerCondition condition)
        {
            if (isDone)
                return false;

            if (this.condition != condition)
                return false;

            Done();
            return true;
        }

        public virtual bool OnEnter()
        {
            enterCount++;
            return OnCondition(TriggerCondition.Enter);
        }
        public virtual bool OnExit()
        {
            return OnCondition(TriggerCondition.Exit);
        }

        public virtual void Done()
        {
            if (eventType == TriggerEventType.Timeline)
            {
                playableDirector.Play();
            }
            else if (eventType == TriggerEventType.Animation)
            {
                animation.Play();
            }
            else if (eventType == TriggerEventType.Do)
            {
                onTrigger.Invoke();
            }

            isDone = true;
        }

        public void Dispose()
        {
        }
    }

}
