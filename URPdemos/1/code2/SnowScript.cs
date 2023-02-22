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

    public Material snowLandMaterial;

    private Vector4[] Datas;
    private Vector4[] LastDatas;
    private ComputeBuffer DataBuffer;
    private ComputeBuffer LastDataBuffer;
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
        //
        int bufflen = texHeight * texWidth;
        Datas = new Vector4[bufflen];
        LastDatas = new Vector4[bufflen];
        DataBuffer = new ComputeBuffer(bufflen, 32);
        DataBuffer.SetData(Datas);
        LastDataBuffer = new ComputeBuffer(bufflen, 32);
        LastDataBuffer.SetData(LastDatas);

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


        cs.SetVector("ColorSnow", ColorSnow);

        cs.SetTexture(kl,"Result",targetRT);
        cs.SetTexture(kp, "Result", targetRT);

        cs.SetBuffer(kl, "DataBuffer", DataBuffer);
        cs.SetBuffer(kl, "LastDataBuffer", LastDataBuffer);
        cs.SetBuffer(kp, "DataBuffer", DataBuffer);
        cs.SetBuffer(kp, "LastDataBuffer", LastDataBuffer);


        cs.SetVector("ColorTrail", ColorTrail);
        cs.SetFloat("Fade", Fade);
        cs.SetFloat("TrailSize", TrailSize);
        cs.SetFloat("Width", (float)texWidth);
        cs.SetFloat("Height", (float)texHeight);
        cs.SetFloat("TrailTime", TrailTime);
        cs.SetVector("Unit", new Vector2(xUnit, yUnit));
    }
    private void OnDisable()
    {
        DataBuffer.Release();
        LastDataBuffer.Release();

    }
    // Update is called once per frame
    void Update()
    {
        Vector2 nowPosition=getUnitPosition(transform.position);
        Vector2 moveVector=nowPosition-formerPosition;
        cs.SetVector("MoveVector", moveVector);
        if(moveVector.sqrMagnitude>0)Debug.Log(moveVector);
        cs.Dispatch(kl, texWidth /16, texHeight / 16, 2);
        cs.Dispatch(kp, texWidth / 16, texHeight / 32, 1);

        formerPosition = nowPosition;
        if (snowLandMaterial != null)
        {
            Vector3 position = getWorldUnitPosition(transform.position - new Vector3(0, transform.localScale.y, 0));
            snowLandMaterial.SetVector("_PlayerPosition",transform.position) ;
            snowLandMaterial.SetVector("_SnowRange", new Vector4(planeScale.x/2,0, planeScale.y/2,0));
        }
    }
}
