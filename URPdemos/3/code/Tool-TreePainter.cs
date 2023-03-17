using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class TreePainter : EditorWindow
{
    public static TreePainter Instance { get; private set; }
    public GameObject[] Plants=new GameObject[6];
    Texture[] TexObjects=new Texture[6];
    public int PlantSelect = 0;
    GameObject AddObject = null;


    public float brushSize = 0;
    public float scaleRandom = 0;
    public float density = 0;
    public float scaleBase = 1;


    public bool enabled = false;
    public int type = 0;
    public bool discarding = false;
    //打开窗口
    [MenuItem("Tools/Tree Painter")]
    static void Open()
    {
        var window = (TreePainter)EditorWindow.GetWindowWithRect(typeof(TreePainter), new Rect(0, 0, 386, 500), false, "Tree Painter");
        window.Show();
    }
    void OnInspectorUpdate()
    {
        Repaint();
    }

    void OnGUI()
    {
        if (!Instance) Instance = this;
        GUILayout.Space(20);
        GUILayout.Label("添加资产", GUILayout.Width(125));
        AddObject= (GameObject)EditorGUILayout.ObjectField("", AddObject, typeof(GameObject), false, GUILayout.Width(160));
        if (GUILayout.Button("+", GUILayout.Width(40)))
        {
            for (int i = 0; i < 6; i++)
            {
                if (Plants[i] == null)
                {
                    Plants[i] = AddObject;
                    break;
                }
            }
        }
        GUILayout.BeginHorizontal();
        for(int i = 0; i < 6; i++)
        {
            if (Plants[i] != null)
            {
                GUILayout.Label(Plants[i].name, GUILayout.Width(125));
            }
            else
            {
                GUILayout.Label("NULL", GUILayout.Width(125));
            }
        }
        GUILayout.EndHorizontal();
        for (int i = 0; i < 6; i++)
        {
            if (Plants[i] != null)
                TexObjects[i] = AssetPreview.GetAssetPreview(Plants[i]) as Texture;
            else TexObjects[i] = null;
        }
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical("box", GUILayout.Width(347));
        PlantSelect = GUILayout.SelectionGrid(PlantSelect, TexObjects, 6, GUILayout.Width(330), GUILayout.Height(55));
        GUILayout.BeginHorizontal();

        for (int i = 0; i < 6; i++)
        {
            if (GUILayout.Button("—", GUILayout.Width(52)))
            {
                Plants[i] = null;
            }
        }

        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.BeginVertical("box", GUILayout.Width(347));
        GUILayout.BeginHorizontal();
        GUILayout.Label("设置", GUILayout.Width(145));
        GUILayout.EndHorizontal();

        brushSize = (int)EditorGUILayout.Slider("笔刷大小", brushSize, 1, 100);
        scaleRandom = EditorGUILayout.Slider("随机大小(+/-)", scaleRandom, 0.05f, 1f);
        scaleBase = (int)EditorGUILayout.Slider("基础大小", scaleBase, 0.1f, 10);
        density = (int)EditorGUILayout.Slider("密度", density, 1, 100);
        
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();

        if(GUILayout.Button("停止")){
            type=0;
            enabled=false;
        }
        if (GUILayout.Button("笔刷"))
        {
            type = 1;
            enabled=true;
        }
        if (GUILayout.Button("橡皮"))
        {
            type = 2;
            enabled = true;
        }
        if(GUILayout.Button("草刷"))
        {
            type = 3;
            enabled = true;
        }
        if(GUILayout.Button("撤销"))
        {
            discarding=true;
        }
        GUILayout.EndHorizontal();
        if (type==0)
        {
            GUILayout.Label("当前状态：关闭。");
        }
        else if(type==1)
        {
            GUILayout.Label("当前状态：笔刷。");
        }else if (type == 2)
        {
            GUILayout.Label("当前状态：橡皮。");
        }else if (type == 3)
        {
            GUILayout.Label("当前状态：草刷。");
        }

        GUILayout.BeginVertical();
        GameObject[] lst;
        //lst = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab) as GameObject[];
        lst = Selection.gameObjects;
        //Object[] lst;
        //lst = Selection.GetFiltered(typeof(GameObject), SelectionMode.ExcludePrefab);
        if (lst != null)
        {
            GUILayout.Label(string.Format("当前选择的物体有{0}个。", lst.Length));
        }
        else
        {
            GUILayout.Label("当前没有选择物体。");
        }
        if (GUILayout.Button("替换为选定物体"))
        {
            if (Plants[PlantSelect] != null && lst != null)
            {
                for (int i = 0; i < lst.Length; i++)
                {
                    Vector3 pos = lst[i].transform.position;
                    Quaternion rot = lst[i].transform.rotation;
                    Vector3 scale = lst[i].transform.localScale;
                    DestroyImmediate(lst[i]);
                    GameObject g = Instantiate(Plants[PlantSelect], pos, rot);
                    g.transform.localScale = scale;
                }
            }
        }
        GUILayout.EndVertical();
    }
}