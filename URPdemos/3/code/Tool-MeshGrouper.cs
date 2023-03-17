using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToolMeshGrouper : EditorWindow
{
    private float process;
    [MenuItem("Tools/MeshGrouper")]
    public static void Solve()
    {
        ToolMeshGrouper solver = (ToolMeshGrouper)EditorWindow.GetWindow(typeof(ToolMeshGrouper));
        solver.titleContent = new GUIContent("模型合并工具");
        solver.minSize = new Vector2(500f, 300f);
    }
    public void OnGUI()
    {
        GUILayout.Label("当前选择的物体有", EditorStyles.boldLabel);
        Object[] lst =Selection.objects;
        GUILayout.Label(string.Format("{0}个。", lst.Length));
        if (GUILayout.Button("Combine"))
        {
            List<Material> materials = new List<Material>();
            List<CombineInstance> combineInstances = new List<CombineInstance>();
            for(int i = 0; i < Selection.objects.Length; i++)
            {
                Debug.Log(i);
                GameObject g = Selection.objects[i] as GameObject;
                if (g == null) continue;
                Debug.Log(-i);
                MeshFilter filter = g.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                EditorUtility.DisplayProgressBar("Mesh Combine", "Combine " + filter.sharedMesh.name, (float)i + 1 / Selection.objects.Length);
                combineInstances.Add(new CombineInstance()
                {
                    mesh = filter.sharedMesh,
                    transform = filter.transform.localToWorldMatrix
                });
                MeshRenderer renderer = g.GetComponent<MeshRenderer>();
                materials.AddRange(renderer.sharedMaterials);
            }
            EditorUtility.ClearProgressBar();
            GameObject combine = new GameObject("Combined Mesh");
            MeshFilter meshFilter = combine.AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh { name = $"Combined Mesh" };
            meshFilter.sharedMesh.CombineMeshes(combineInstances.ToArray(),true);
            MeshRenderer meshRenderer = combine.AddComponent<MeshRenderer>();
            meshRenderer.materials = materials.ToArray();

        }
    }
    
}
