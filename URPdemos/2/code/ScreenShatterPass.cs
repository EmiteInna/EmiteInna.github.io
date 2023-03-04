using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

class ScreenShatterPass : ScriptableRenderPass
{
    static readonly string k_RenderTag = "Render ScreenShatter Effects";
    static readonly int MainTexId = Shader.PropertyToID("_MainTex");
    static readonly int TempTargetId = Shader.PropertyToID("_TempTargetColorTint");

    ScreenShatterEffect volume;
    Material material;
    RenderTargetIdentifier currentTarget;
    public ScreenShatterPass(RenderPassEvent evt, Shader shader1)
    {
        renderPassEvent = evt;
        var shader = shader1;
        if (shader == null)
        {
            Debug.LogError("Missing shader-ScreenShatter");
            return;
        }
        material = CoreUtils.CreateEngineMaterial(shader1);
    }
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        
        if (material == null) { Debug.LogError("Failed-ScreenShatter"); return; }
        if (!renderingData.cameraData.postProcessEnabled) return;
        var stack = VolumeManager.instance.stack;

        volume = stack.GetComponent<ScreenShatterEffect>();
        if (volume == null) return;
        if (volume.is_opened.value == false) return;
        volume.load(material);
        var cmd = CommandBufferPool.Get(k_RenderTag);
        Render(cmd, ref renderingData);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
    void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ref var cameraData = ref renderingData.cameraData;
        var camera = cameraData.camera;
        var source = currentTarget;//camera picture
        var destination = TempTargetId;//temp rt
        int rtW = renderingData.cameraData.camera.scaledPixelWidth;
        int rtH = renderingData.cameraData.camera.scaledPixelHeight;
        cmd.GetTemporaryRT(destination, rtW, rtH,
            0, FilterMode.Bilinear, RenderTextureFormat.Default);
        cmd.SetGlobalTexture(MainTexId, source);
        cmd.Blit(source, destination);
        cmd.Blit(destination, source, material, 0);
    }
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
    }
    public void Setup(in RenderTargetIdentifier currentTarget)
    {
        this.currentTarget = currentTarget;
    }
}