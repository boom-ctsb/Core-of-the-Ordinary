#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelBoundsAuto))]
public class LevelBoundsAutoEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelBoundsAuto script = (LevelBoundsAuto)target;

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto Set Bounds"))
        {
            Undo.RecordObject(script.gameObject, "Auto Set Bounds");
            script.AutoSetBounds();
            EditorUtility.SetDirty(script);
            if (!Application.isPlaying) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(script.gameObject.scene);
        }
        if (GUILayout.Button("Assign to CamFollow (Main Camera)"))
        {
            var box = script.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                var cam = Camera.main;
                if (cam != null)
                {
                    var camFollow = cam.GetComponent<CamFollow>();
                    if (camFollow != null)
                    {
                        camFollow.boundsBox = box;
                        camFollow.RefreshBounds();
                        Debug.Log("[LevelBoundsAutoEditor] Assigned bounds to CamFollow.");
                    }
                    else Debug.LogWarning("[LevelBoundsAutoEditor] CamFollow not found on Main Camera.");
                }
                else Debug.LogWarning("[LevelBoundsAutoEditor] Main Camera not found.");
            }
            else Debug.LogWarning("[LevelBoundsAutoEditor] No BoxCollider2D found on this GameObject.");
        }
        GUILayout.EndHorizontal();
    }
}
#endif