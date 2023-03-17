using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToolModelSolver : EditorWindow
{
    private float process;
    [MenuItem("Tools/ModelSover")]
    public static void Solve()
    {
        ToolModelSolver solver = (ToolModelSolver)EditorWindow.GetWindow(typeof(ToolModelSolver));
        solver.titleContent = new GUIContent("模型顶点法线平滑工具");
        solver.minSize = new Vector2(500f, 300f);
    }
    public void OnGUI()
    {
        GUILayout.Label("当前选择的物体是", EditorStyles.boldLabel);
        GameObject obj=Selection.activeGameObject;
        if (obj != null)
            GUILayout.Label(string.Format("{0}",obj.name));
        else
        {
            GUILayout.Label("");
        }
        if (GUILayout.Button("法线平滑"))
        {
            execution(obj);
        }
        GUILayout.Label(string.Format("进度为：{0}%", process));
    }
    public static void SaveMesh(Mesh mesh, string name)
    {
        string path = EditorUtility.SaveFilePanel("Save Meshes", "Assets/", name, "asset");
        if (string.IsNullOrEmpty(path)) return;
        path = FileUtil.GetProjectRelativePath(path);
        Mesh meshToSave = Object.Instantiate(mesh) as Mesh;
        AssetDatabase.CreateAsset(meshToSave, path);
        AssetDatabase.SaveAssets();
    }
    public void execution(GameObject obj)
    {
        if (obj == null) return;
        process = 0;
        Mesh mesh = null;
        if (obj.GetComponent<MeshFilter>() != null) mesh = obj.GetComponent<MeshFilter>().sharedMesh;
        if (obj.GetComponent<SkinnedMeshRenderer>() != null) mesh = obj.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        if (mesh == null) return;
        var averageNormal = new Dictionary<Vector3, Vector3>();
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            if (!averageNormal.ContainsKey(mesh.vertices[j]))
            {
                averageNormal.Add(mesh.vertices[j], mesh.normals[j]);
            }
            else
            {
                averageNormal[mesh.vertices[j]] += mesh.normals[j];
            }
            process = (float)j / (float)mesh.vertexCount * 0.5f;
        }
        List<Color> colors = new List<Color>();
        for (var j = 0; j < mesh.vertexCount; j++)
        {
            Vector3 v = averageNormal[mesh.vertices[j]].normalized;
            colors.Add(new Color(v.x * 0.5f + 0.5f, v.y * 0.5f + 0.5f, v.z * 0.5f + 0.5f));
            process = (float)j / (float)mesh.vertexCount * 0.5f+0.5f;
        }
        mesh.colors = colors.ToArray();
        process = 1;
        SaveMesh(mesh, "name");
    }
}
