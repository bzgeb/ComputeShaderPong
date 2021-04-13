using System;
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
    [SerializeField] Vector2 _ballPosition;
    [SerializeField] float _ballSize;
    [SerializeField] Color _ballColor;

    RenderTexture _renderTexture;

    int _updateKernel;
    int _drawKernel;

    ComputeBuffer _paddleBuffer;
    ComputeBuffer _ballBuffer;

    struct Ball
    {
        public Vector4 position;
        public Vector2 velocity;
    }

    void OnEnable()
    {
        _renderTexture = new RenderTexture(_size, _size, 24);
        _renderTexture.filterMode = FilterMode.Point;
        _renderTexture.enableRandomWrite = true;
        _renderTexture.Create();

        _drawKernel = _computeShader.FindKernel("Draw");
        _updateKernel = _computeShader.FindKernel("Update");

        _computeShader.SetFloat("_Resolution", _size);
        _computeShader.SetTexture(_drawKernel, "_Result", _renderTexture);
        _computeShader.SetVector("_BackgroundColor", _backgroundColor);

        _computeShader.SetVector("_PaddleColor", _paddleColor);

        _paddleBuffer = new ComputeBuffer(1, 4 * 4);
        _paddleBuffer.SetData(new[] {new Vector4(_paddlePosition.x, _paddlePosition.y, _paddleSize.x, _paddleSize.y)});

        _ballBuffer = new ComputeBuffer(1, 6 * 4);
        Ball ball = new Ball
        {
            position = new Vector4(_ballPosition.x, _ballPosition.y, _ballSize, 0f),
            velocity = new Vector2(0.5f, 0.5f).normalized
        };
        _ballBuffer.SetData(new []{ball});

        _computeShader.SetBuffer(_updateKernel, "_Paddle", _paddleBuffer);
        _computeShader.SetBuffer(_drawKernel, "_Paddle", _paddleBuffer);
        
        _computeShader.SetBuffer(_updateKernel, "_Ball", _ballBuffer);
        _computeShader.SetBuffer(_drawKernel, "_Ball", _ballBuffer);

        _computeShader.SetFloat("_Input", 0f);

        _computeShader.SetVector("_BallColor", _ballColor);
        _computeShader.SetVector("_BallVelocity", new Vector4(0.5f, 0.5f, 0f, 0f).normalized);

        _computeShader.GetKernelThreadGroupSizes(_drawKernel, out uint xGroupSize, out uint yGroupSize,
            out uint zGroupSize);
        _computeShader.Dispatch(_drawKernel, _renderTexture.width / (int) xGroupSize,
            _renderTexture.height / (int) yGroupSize, 1);
    }

    void Update()
    {
        var input = Input.GetAxisRaw("Horizontal") * _paddleSpeed * Time.deltaTime;

        _computeShader.SetFloat("_Input", input);

        _computeShader.GetKernelThreadGroupSizes(_updateKernel, out uint xGroupSize, out uint yGroupSize, out _);
        _computeShader.Dispatch(_updateKernel, 1, 1, 1);

        _computeShader.GetKernelThreadGroupSizes(_drawKernel, out xGroupSize, out yGroupSize, out _);
        _computeShader.Dispatch(_drawKernel, _renderTexture.width / (int) xGroupSize,
            _renderTexture.height / (int) yGroupSize, 1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_renderTexture, dest);
    }

    void OnDestroy()
    {
        _paddleBuffer.Dispose();
        _ballBuffer.Dispose();
    }
}