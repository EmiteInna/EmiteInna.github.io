using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ScreenShatterEffect : VolumeComponent
{
    [Header("Settings")]
    
    public BoolParameter is_opened = new BoolParameter(false);
    public FloatParameter bumpScale = new FloatParameter(1);
    public RenderTextureParameter normalMap = new RenderTextureParameter(null);
    public RenderTextureParameter shatterMap = new RenderTextureParameter(null);
    public void load(Material mat)
    {
        mat.SetFloat("_BumpScale", bumpScale.value);
        mat.SetTexture("_NormalMap", normalMap.value);
        mat.SetTexture("_ShatterMap", shatterMap.value);
    }
}