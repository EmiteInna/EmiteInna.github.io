using System.Collections;
using System.Data;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class ScreenShatterController : MonoBehaviour
{
    public ComputeShader cs;
    public bool execution=false;
    Volume volume;
    ScreenShatterEffect eff;
    [Header("Parameter")]
    [Range(-1f, 1f)]
    public float Threshold=0;
    [Range(0, 5)]
    public float BumpScale=0.08f;
    public int TexWidth = 1920;
    public int TexHeight = 1080;
    [Header("RenderTextures&Materials&Arrays")]
    RenderTexture targetRT;
    RenderTexture ScreenShotRT;
    public RenderTexture screenRT;
    public Texture2D SpeedMap;
    public Texture2D NormalMap;
    RenderTexture speedMapRT;
    RenderTexture normalMapRT;
    RenderTexture freezeRT;
    int kl,kp,ks;
    [Header("Debug")]
    public GameObject debugOriginal;
    public GameObject debugTarget;
    // Start is called before the first frame update
    
    void OnEnable()
    {
        if (cs == null) return;
        if (screenRT == null) return;
        TexHeight= Screen.height/16*16+16;
        TexWidth= Screen.width/16*16+16;
        targetRT = RenderTexture.GetTemporary(TexWidth, TexHeight,0,RenderTextureFormat.ARGB32);
        targetRT.enableRandomWrite=true;
        targetRT.Create();
        ScreenShotRT = RenderTexture.GetTemporary(TexWidth, TexHeight, 0, RenderTextureFormat.ARGB32);
        ScreenShotRT.enableRandomWrite = true;
        ScreenShotRT.Create();
        // speedMapRT = RenderTexture.GetTemporary(SpeedMap.width,SpeedMap.height, 0, RenderTextureFormat.ARGB32);
        speedMapRT = RenderTexture.GetTemporary(TexWidth,TexHeight, 0, RenderTextureFormat.ARGB32);
        speedMapRT.enableRandomWrite = true;
        speedMapRT.Create();
        // normalMapRT = RenderTexture.GetTemporary(NormalMap.width, NormalMap.height, 0, RenderTextureFormat.ARGB32);
        normalMapRT = RenderTexture.GetTemporary(TexWidth, TexHeight, 0, RenderTextureFormat.ARGB32);
        normalMapRT.enableRandomWrite = true;
        normalMapRT.Create();
        freezeRT = RenderTexture.GetTemporary(TexWidth, TexHeight, 0, RenderTextureFormat.ARGB32);
        freezeRT.enableRandomWrite = true;
        freezeRT.Create();
        
        Graphics.Blit(SpeedMap, speedMapRT);
        Graphics.Blit(NormalMap, normalMapRT);
        //
        volume = GameObject.Find("Global Volume").GetComponent<Volume>();
        if(volume != null)
        {
            volume.profile.TryGet<ScreenShatterEffect>(out eff);
        }
        //
        kl = cs.FindKernel("CSMain");
        kp = cs.FindKernel("CSClear");
        ks = cs.FindKernel("CSSobel");

        cs.SetTexture(kl,"Result",targetRT);
        cs.SetTexture(kl, "SpeedMap", speedMapRT);
        cs.SetTexture(kl, "OriginalNormal", normalMapRT);
  
        cs.SetFloat("_TexHeight", TexHeight);
        cs.SetFloat("_TexWidth", TexWidth);
        cs.SetFloat("_SpeedMapWidth", SpeedMap.width);
        cs.SetFloat("_SpeedMapHeight", SpeedMap.height);
        cs.SetFloat("_NormalMapWidth", NormalMap.width);
        cs.SetFloat("_NormalMapHeight", NormalMap.height);
        cs.SetTexture(kp, "Result", targetRT);
        cs.SetTexture(ks, "Result", targetRT);
        Shatter();
    }
    private void OnDisable()
    {
       
        RenderTexture.ReleaseTemporary(targetRT);
        RenderTexture.ReleaseTemporary(ScreenShotRT);
        RenderTexture.ReleaseTemporary(speedMapRT);
        RenderTexture.ReleaseTemporary(normalMapRT);
        RenderTexture.ReleaseTemporary(freezeRT);
    }
    void Shatter()
    {
        
        execution = false;
        Threshold = 1;
        if (eff != null)
        {
            Graphics.Blit(screenRT, freezeRT);
            eff.shatterMap.value = freezeRT;
            eff.is_opened.value = true;
        }

    }
    // Update is called once per frame
    void Update()
    {
        if (cs == null) return;
        if (screenRT == null) return;
        UpdateController();
        if (execution)
        {
            Shatter();
        }
        cs.SetFloat("_Threshold", Threshold);
        cs.Dispatch(kp, TexWidth / 16, TexHeight / 16, 2);
        cs.Dispatch(kl, TexWidth /16, TexHeight / 16, 2);
        cs.Dispatch(ks, TexWidth / 16, TexHeight / 16, 2);

        if (eff != null)
        {
            eff.normalMap.value=targetRT;
            eff.bumpScale.value = BumpScale;
            //Graphics.Blit(targetRT, eff.normalMap.value);
        }
    }
    private void UpdateController()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            execution = true;
        }
        if (Input.GetKey(KeyCode.X))
        {
            Threshold -= 0.02f;
            Threshold = Mathf.Clamp(Threshold, -1, 1);
        }
    }
}
