using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class Toollhs : EditorWindow
{
    private float process;
    
    [MenuItem("Tools/Discretization")]
    public static void Solve()
    {
        Toollhs solver = (Toollhs)EditorWindow.GetWindow(typeof(Toollhs));
        solver.titleContent = new GUIContent("ͼ����ɫ��ɢ������");
        solver.minSize = new Vector2(500f, 300f);
    }
    public void OnGUI()
    {
        GUILayout.Label("��ǰѡ���ͼ����", EditorStyles.boldLabel);
        UnityEngine.Object[] sel=Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
        if (sel.Length == 1)
        {
            GUILayout.Label(string.Format("{0}", sel[0].name));
            GUILayout.Label(string.Format("{0}��ǰͼ�����ϢΪ��width��{1}��height��{2}����ʽΪ��{3}", "^w^", 
                (sel[0] as Texture2D).width, (sel[0] as Texture2D).height, (sel[0] as Texture2D).graphicsFormat));
        }
        else if (sel.Length == 0)
        {
            GUILayout.Label("��δѡ��ͼ��");
        }
        else
        {
            GUILayout.Label("ѡ���ͼ��̫��");
        }
        if (GUILayout.Button("���ҿ���"))
        {
             execution(sel[0] as Texture2D);
            //CreateSampleSprite();
        }
        GUILayout.Label(string.Format("����Ϊ��{0}%", process));
    }
    public static void SaveTex(Texture2D tex, string name)
    {
        if (tex == null) return;
        Debug.Log(string.Format("Out Image, width:{0}, height:{1},format:{2}", tex.width, tex.height, tex.graphicsFormat));
        string path = EditorUtility.SaveFilePanel("Save Image", "Assets/", name, "png");
        if (string.IsNullOrEmpty(path)) return;
        byte[] dataBytes = tex.EncodeToPNG();
        FileStream fileStream = File.Open(path, FileMode.OpenOrCreate);
        fileStream.Write(dataBytes, 0, dataBytes.Length);
        fileStream.Close();
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
    }
    public void execution(Texture2D obj)
    {
        //SaveTex(obj, "wocaonidema");
        //return;
        Texture2D newtex = new Texture2D(obj.width, obj.height) ;
        float[] discre = {0,18,32,43,61,82,98,123,140,162,186,205,231,999 };
        for(int i = 0; i < obj.height; i++)
        {
            for(int j = 0; j < obj.width; j++)
            {
                Color formerColor = obj.GetPixel(i, j);
                float val = formerColor.r * 256f;
                float res = 0;
                for(int k = 0; k < discre.Length; k++)
                {
                    if (discre[k] > val)
                    {
                        res=discre[k-1]/256f;
                        break;
                    }
                }
                newtex.SetPixel(i, j, new Color(res,res,res));
            }
        }
        newtex.Apply();
        SaveTex(newtex,obj.name);
    }
    
}
