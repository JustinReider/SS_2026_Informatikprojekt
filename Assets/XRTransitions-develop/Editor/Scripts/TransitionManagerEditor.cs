using System;
using System.Linq;
using System.Reflection;
using Scripts;
using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    [CustomEditor(typeof(TransitionManager))]
    public class TransitionManagerEditor : UnityEditor.Editor
    {
        private bool _foldout = true;

        public override void OnInspectorGUI()
        {
            var transitionManager = target as TransitionManager;

            if (transitionManager == null)
            {
                base.OnInspectorGUI();
                return;
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_mainCamera"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_leftEyeTransform"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rightEyeTransform"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("_currentContext"), new GUIContent("Current Context (Start)"));

            _foldout = EditorGUILayout.Foldout(_foldout, "Transitions");
            if (_foldout)
            {
                
                EditorGUILayout.Separator();

                EditorGUI.indentLevel++;
                var t = serializedObject.FindProperty("Transitions");
                for (int i = 0; i < t.arraySize; i++)
                {
                    var element = t.GetArrayElementAtIndex(i);
                    if (element.managedReferenceValue == null)
                    {
                        transitionManager.Transitions.Remove(element.managedReferenceValue as Transition);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(element, new GUIContent(element.managedReferenceValue.GetType().Name),
                            true);
                        GUILayout.Space(30);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Delete Transition", GUILayout.Width(120)))
                        {
                            transitionManager.Transitions.Remove(element.managedReferenceValue as Transition);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    }
                }
                if (GUILayout.Button("+"))
                {
                    var menu = new GenericMenu();
                    foreach (Type type in typeof(Transition).Assembly.GetTypes()
                                 .Where(t => t.IsSubclassOf(typeof(Transition))))
                    {
                        menu.AddItem(new GUIContent(type.Name),false, t =>
                        {
                            transitionManager.Transitions.Add(Activator.CreateInstance(t as Type) as Transition);
                            serializedObject.ApplyModifiedProperties();
                        },type);
                    }
                    menu.ShowAsContext();
                }
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();

            if (Application.isPlaying)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                EditorGUILayout.LabelField("Initialize Transition");
                foreach (Type type in typeof(Transition).Assembly.GetTypes()
                             .Where(t => t.IsSubclassOf(typeof(Transition))))
                {
                    var button = GUILayout.Button(type.Name.Replace("Transition", ""));
                    if (button)
                    {
                        transitionManager.InitializeTransitionType(type);
                    }
                }
            }
        }
    }
}