using UnityEngine;

public class PongComputeShaderRunner : MonoBehaviour
{
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] int _size;
    
    RenderTexture _renderTexture;

    void Start()
    {
        _renderTexture = new RenderTexture(_size, _size, 24);
        _renderTexture.filterMode = FilterMode.Point;
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();

        var main = _computeShader.FindKernel("CSMain");
        _computeShader.SetTexture(main, "_Result", _renderTexture);
        _computeShader.GetKernelThreadGroupSizes(main, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize);
        _computeShader.Dispatch(main, _renderTexture.width / (int) xGroupSize, _renderTexture.height / (int) yGroupSize,
            1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_renderTexture, dest);
    }
}