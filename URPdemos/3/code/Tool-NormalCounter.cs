using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ToolNormalCounter : EditorWindow
{
    private float process;
    [MenuItem("Tools/NormalCounter")]
    public static void Solve()
    {
        ToolNormalCounter solver = (ToolNormalCounter)GetWindow(typeof(ToolNormalCounter));
        solver.titleContent = new GUIContent("模型自动法线生成工具");
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
        if (GUILayout.Button("法线生成"))
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
        for (var j = 0; j < mesh.triangles.Length; j+=3)
        {
            Vector3 dot1 = mesh.vertices[mesh.triangles[j]];
            Vector3 dot2 = mesh.vertices[mesh.triangles[j+1]];
            Vector3 dot3 = mesh.vertices[mesh.triangles[j+2]];
            Vector3 vec1 = dot2 - dot1;
            Vector3 vec2 = dot3 - dot1;
            Vector3 normal = Vector3.Cross(vec1, vec2);
            mesh.normals[mesh.triangles[j]] = normal;
            mesh.normals[mesh.triangles[j+1]] = normal;
            mesh.normals[mesh.triangles[j+2]] = normal;
        }
        SaveMesh(mesh, obj.name);
        
    }
}
