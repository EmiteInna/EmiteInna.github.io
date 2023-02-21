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

    private float[] TimerFloats;
    private float[] LastTimerFloats;
    private ComputeBuffer TimerBuffer;
    private ComputeBuffer LastTimerBuffer;
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
        TimerFloats = new float[bufflen];
        LastTimerFloats = new float[bufflen];
        TimerBuffer = new ComputeBuffer(bufflen, 32);
        TimerBuffer.SetData(TimerFloats);
        LastTimerBuffer = new ComputeBuffer(bufflen, 32);
        LastTimerBuffer.SetData(LastTimerFloats);

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
        cs.SetBuffer(kl, "TimerBuffer", TimerBuffer);
        cs.SetBuffer(kl, "LastTimerBuffer", LastTimerBuffer);
        cs.SetBuffer(kp, "TimerBuffer", TimerBuffer);
        cs.SetBuffer(kp, "LastTimerBuffer", LastTimerBuffer);

        cs.SetVector("ColorTrail", ColorTrail);
        cs.SetFloat("Fade", Fade);
        cs.SetFloat("TrailSize", TrailSize);
        cs.SetFloat("Width", (float)texWidth);
        cs.SetFloat("Height", (float)texHeight);
        cs.SetFloat("TrailTime", TrailTime);

    }
    private void OnDisable()
    {
        TimerBuffer.Release();
        LastTimerBuffer.Release();
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 nowPosition=getUnitPosition(transform.position);
        Vector2 moveVector=nowPosition-formerPosition;
        moveVector.y *= texWidth;
        cs.SetVector("MoveVector", moveVector);
        
        cs.Dispatch(kl, texWidth /16, texHeight / 16, 2);
        cs.Dispatch(kp, texWidth / 16, texHeight / 32, 1);
  
        formerPosition = nowPosition;
        if (snowLandMaterial != null)
        {
            Vector3 position = getWorldUnitPosition(transform.position - new Vector3(0, transform.localScale.y, 0));
            snowLandMaterial.SetVector("_PlayerPosition",position) ;
            Debug.Log(position);
            snowLandMaterial.SetVector("_SnowRange", new Vector4(planeScale.x/2,0, planeScale.y/2,0));
        }
    }
}
