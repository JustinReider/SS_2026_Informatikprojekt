// Assets/Editor/ReplaceToPrefab.cs
using UnityEditor;
using UnityEngine;

public class ReplaceToPrefab : EditorWindow
{
    public GameObject prefab;

    [MenuItem("Tools/Replace With Prefab")]
    static void Open() => GetWindow<ReplaceToPrefab>();

    void OnGUI()
    {
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

        if (GUILayout.Button("Replace Selection"))
        {
            var selected = Selection.gameObjects;
            Undo.RegisterCompleteObjectUndo(selected, "Replace with Prefab");

            foreach (var go in selected)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, go.transform.parent);
                instance.transform.SetPositionAndRotation(go.transform.position, go.transform.rotation);
                instance.transform.localScale = go.transform.localScale;
                instance.name = go.name;
                Undo.DestroyObjectImmediate(go);
            }
        }
    }
}
