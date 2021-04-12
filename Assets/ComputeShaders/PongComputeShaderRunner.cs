using UnityEngine;

public class PongComputeShaderRunner : MonoBehaviour
{
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] int _size;
    [SerializeField] Color _backgroundColor;
    [SerializeField] Color _paddleColor;
    [SerializeField] Vector2 _paddlePosition;
    [SerializeField] Vector2 _paddleSize;
    [SerializeField] float _paddleSpeed = 100f;

    RenderTexture _renderTexture;

    void Start()
    {
        _renderTexture = new RenderTexture(_size, _size, 24);
        _renderTexture.filterMode = FilterMode.Point;
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();

        var main = _computeShader.FindKernel("CSMain");
        _computeShader.SetTexture(main, "_Result", _renderTexture);
        _computeShader.SetVector("_BackgroundColor", _backgroundColor);
        _computeShader.SetVector("_PaddleColor", _paddleColor);
        _computeShader.SetVector("_Paddle",
            new Vector4(_paddlePosition.x, _paddlePosition.y, _paddleSize.x, _paddleSize.y));
        _computeShader.GetKernelThreadGroupSizes(main, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize);
        _computeShader.Dispatch(main, _renderTexture.width / (int) xGroupSize, _renderTexture.height / (int) yGroupSize,
            1);
    }

    void Update()
    {
        _paddlePosition.x += Input.GetAxisRaw("Horizontal") * _paddleSpeed * Time.deltaTime;
        _computeShader.SetVector("_Paddle",
            new Vector4(_paddlePosition.x, _paddlePosition.y, _paddleSize.x, _paddleSize.y));
        var main = _computeShader.FindKernel("CSMain");
        _computeShader.GetKernelThreadGroupSizes(main, out uint xGroupSize, out uint yGroupSize, out uint zGroupSize);
        _computeShader.Dispatch(main, _renderTexture.width / (int) xGroupSize, _renderTexture.height / (int) yGroupSize,
            1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_renderTexture, dest);
    }
}