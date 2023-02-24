using System.Collections;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class SnowScript : MonoBehaviour
{
    [Header("Parameter")]
    public Vector2 planeScale;
    public int texWidth = 1024;
    public int texHeight= 1024;
    public Color ColorSnow;
    public Color ColorTrail;
    public float Fade;
    public float TrailSize;
    public float TrailTime = 50;
    public ComputeShader cs;
    private float xUnit;
    private float yUnit;
    [Header("RenderTextures&Materials&Arrays")]
    public RenderTexture targetRT;
    public Texture2D Displacement;
    public Texture2D Normal;
    RenderTexture DisplacementRT;
    RenderTexture NormalRT;
    public Material snowLandMaterial;
    public Material snowLandGeoMaterial;

    private Vector4[] Datas;
    private Vector4[] LastDatas;
    private float[] Time;
    private float[] LastTime;
    private ComputeBuffer DataBuffer;
    private ComputeBuffer LastDataBuffer;
    private ComputeBuffer TimeBuffer;
    private ComputeBuffer LastTimeBuffer;
    Vector2 formerPosition;
    int kl,kp;
    [Header("Debug")]
    public GameObject debugOriginal;
    public GameObject debugTarget;
    // Start is called before the first frame update
    Vector2 getUnitPosition(Vector3 position)
    {
        float x= Mathf.Floor(position.x/xUnit);
        float y= Mathf.Floor(position.z/yUnit);

        return new Vector2(x, y);
    }
    Vector3 getWorldUnitPosition(Vector3 position)
    {
        float x = Mathf.Floor(position.x / xUnit) * xUnit;
        float z = Mathf.Floor(position.z / yUnit)*yUnit;
        return new Vector3(x, position.y, z);
    }
    void OnEnable()
    {
        

        DisplacementRT = RenderTexture.GetTemporary(Displacement.width, Displacement.height);
        DisplacementRT.enableRandomWrite=true;
        DisplacementRT.Create();
        Graphics.Blit(Displacement, DisplacementRT);
        NormalRT = RenderTexture.GetTemporary(Normal.width, Normal.height);
        NormalRT.enableRandomWrite = true;
        NormalRT.Create();
        Graphics.Blit(Normal, NormalRT);
        //
        int bufflen = texHeight * texWidth;
        Datas = new Vector4[bufflen];
        LastDatas = new Vector4[bufflen];
        Time = new float[bufflen];
        LastTime = new float[bufflen];


        DataBuffer = new ComputeBuffer(bufflen, 32);
        DataBuffer.SetData(Datas);
        LastDataBuffer = new ComputeBuffer(bufflen, 32);
        LastDataBuffer.SetData(LastDatas);
        TimeBuffer = new ComputeBuffer(bufflen, 32);
        TimeBuffer.SetData(Time);
        LastTimeBuffer = new ComputeBuffer(bufflen, 32);
        LastTimeBuffer.SetData(Time);
        //
        targetRT.enableRandomWrite = true;
        targetRT.Create();
        //
        xUnit = planeScale.x / (texWidth + 0.001f);
        yUnit = planeScale.y / (texHeight + 0.001f);
        formerPosition = getUnitPosition(transform.position);
        //
        kl = cs.FindKernel("CSMain");
        kp = cs.FindKernel("CSPassData");

        cs.SetTexture(kl,"Result",targetRT);
        cs.SetTexture(kl, "Displacement", DisplacementRT);
        cs.SetTexture(kl, "Normal", NormalRT);
        int disWidth = Displacement.width;
        int disHeight = Displacement.height;
        cs.SetInt("DisWidth", disWidth);
        cs.SetInt("DisHeight", disHeight);
        cs.SetTexture(kp, "Result", targetRT);

        cs.SetBuffer(kl, "DataBuffer", DataBuffer);
        cs.SetBuffer(kl, "LastDataBuffer", LastDataBuffer);
        cs.SetBuffer(kl, "TimeBuffer", TimeBuffer);
        cs.SetBuffer(kl, "LastTimeBuffer", LastTimeBuffer);
        cs.SetBuffer(kp, "DataBuffer", DataBuffer);
        cs.SetBuffer(kp, "LastDataBuffer", LastDataBuffer);
        cs.SetBuffer(kp, "TimeBuffer", TimeBuffer);
        cs.SetBuffer(kp, "LastTimeBuffer", LastTimeBuffer);

        cs.SetFloat("Fade", Fade);
        cs.SetFloat("TrailSize", TrailSize);
        cs.SetFloat("Width", (float)texWidth);
        cs.SetFloat("Height", (float)texHeight);
        cs.SetFloat("TrailTime", TrailTime);
    }
    private void OnDisable()
    {
        DataBuffer.Release();
        LastDataBuffer.Release();
        TimeBuffer.Release();
        LastTimeBuffer.Release();
        RenderTexture.ReleaseTemporary(DisplacementRT);
        RenderTexture.ReleaseTemporary(NormalRT);
    }
    // Update is called once per frame
    void Update()
    {
        cs.SetVector("FaceDirection",new Vector2(transform.forward.x,transform.forward.z));
        Vector2 nowPosition=getUnitPosition(transform.position);
        Vector2 moveVector=nowPosition-formerPosition;
        cs.SetVector("MoveVector", moveVector);
       // if(moveVector.sqrMagnitude>0)Debug.Log(moveVector);
        cs.Dispatch(kl, texWidth /16, texHeight / 16, 2);
        cs.Dispatch(kp, texWidth / 16, texHeight / 32, 1);

        formerPosition = nowPosition;
        if (snowLandMaterial != null)
        {
            Vector3 position = getWorldUnitPosition(transform.position - new Vector3(0, transform.localScale.y, 0));
            snowLandMaterial.SetVector("_PlayerPosition",position) ;
            snowLandMaterial.SetVector("_SnowRange", new Vector4(planeScale.x/2,transform.position.y, planeScale.y/2,0));
        }
        if (snowLandGeoMaterial != null)
        {
            Vector3 position = getWorldUnitPosition(transform.position - new Vector3(0, transform.localScale.y, 0));
            snowLandGeoMaterial.SetVector("_PlayerPosition", position);
            snowLandGeoMaterial.SetVector("_SnowRange", new Vector4(planeScale.x / 2, transform.position.y, planeScale.y / 2, 0));
        }
    }
}
