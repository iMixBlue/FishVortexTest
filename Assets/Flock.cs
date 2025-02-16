using UnityEngine;
using UnityEngine.VFX;
using System.Runtime.InteropServices;

public class Flocking : MonoBehaviour
{
	public ComputeShader cs = null;
    GraphicsBuffer _positionBuffer;
    GraphicsBuffer _velocityBuffer;
    //Instancing
	public int totalFishNum = 3000;
    public Vector2 fishRandomScale = new Vector2(1.0f,1.5f);
    //Flocking
    public Vector2 speedRange = new Vector2(1.0f,5.0f);
    public float maxSteerForce = 60.0f;
    public float alignment = 3.0f;
    public float alignmentRadius = 0.2f;
    public float cohesion = 1.0f;
    public float cohesionRadius = 0.2f;
    public float separation = 3.0f;
    public float separationRadius = 0.3f;
    //Vortex
    public Vector3 topAnchor = new Vector3(0, 7, 0);
    public Vector3 bottomAnchor = new Vector3(0, 2, 0);
    public float topRadius = 5.0f;
    public float bottomRadius = 0.5f;
    [Range(0.0f,1.0f)]
    public float eye = 0.1f;
    public float stiffness = 1.0f;
    public float swirlStrength = 3.0f;

    void Start()
    {
        var vfx = GetComponent<VisualEffect>();
        vfx.Reinit();
        vfx.SetFloat("TotalFishNum", totalFishNum);

        _positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, totalFishNum, Marshal.SizeOf(typeof(Vector3)));
        _velocityBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, totalFishNum, Marshal.SizeOf(typeof(Vector3)));

        var positionArray = new Vector3[totalFishNum];
        var velocityArray = new Vector3[totalFishNum];
        for (var i = 0; i < totalFishNum; i++)
        {
            positionArray[i] = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 3.0f;
            var theta = Random.Range(-Mathf.PI, Mathf.PI);
            var phi = Mathf.Asin(Random.Range(-1f, 1f));
            velocityArray[i] = new Vector3(Mathf.Cos(phi) * Mathf.Cos(theta), Mathf.Cos(phi) * Mathf.Sin(theta), Mathf.Sin(phi)) * (speedRange.x + speedRange.y) * 0.5f;
        }
        _positionBuffer.SetData(positionArray);
        _velocityBuffer.SetData(velocityArray);
    }

    void FixedUpdate()
    {
        var vfx = GetComponent<VisualEffect>();
        vfx.SetVector2("FishRandomScale", fishRandomScale);

        int kernelID = cs.FindKernel("MainCS");
        cs.SetBuffer(kernelID, "_PositionBuffer", _positionBuffer);
        cs.SetBuffer(kernelID, "_VelocityBuffer", _velocityBuffer);
        cs.SetInt("_totalFishNum", totalFishNum);
        //Flocking
        cs.SetVector("_SpeedRange", speedRange);
        cs.SetFloat("_MaxSteerForce", maxSteerForce);
        cs.SetFloat("_Alignment", alignment);
        cs.SetFloat("_AlignmentRadius", alignmentRadius);
        cs.SetFloat("_Cohesion", cohesion);
        cs.SetFloat("_CohesionRadius", cohesionRadius);
        cs.SetFloat("_Separation", separation);
        cs.SetFloat("_SeparationRadius", separationRadius);
        //Vortex
        cs.SetVector("_TopAnchor", topAnchor);
        cs.SetVector("_BottomAnchor", bottomAnchor);
        cs.SetFloat("_TopRadius", topRadius);
        cs.SetFloat("_BottomRadius", bottomRadius);
        cs.SetFloat("_Eye", eye);
        cs.SetFloat("_Stiffness", stiffness);
        cs.SetFloat("_SwirlStrength", swirlStrength);
        cs.SetFloat("_DeltaTime", Time.deltaTime);
        
        int threadGroupSize = totalFishNum / 256 + 1;
        cs.Dispatch(kernelID, threadGroupSize, 1, 1);
        if (_positionBuffer != null) vfx.SetGraphicsBuffer("PositionBuffer", _positionBuffer);
        if (_velocityBuffer != null) vfx.SetGraphicsBuffer("VelocityBuffer", _velocityBuffer);
    }
    void OnDestroy()
    {
        if (_positionBuffer != null) { _positionBuffer.Release(); _positionBuffer = null; }
        if (_velocityBuffer != null) { _velocityBuffer.Release(); _velocityBuffer = null; }
    }
}