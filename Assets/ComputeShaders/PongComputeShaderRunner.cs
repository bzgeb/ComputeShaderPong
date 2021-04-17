using UnityEngine;

public class PongComputeShaderRunner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] int _size;
    [SerializeField] Color _backgroundColor;
    
    [Header("Paddle")]
    [SerializeField] Color _paddleColor;
    [SerializeField] Vector2 _paddlePosition = new Vector2(256, 10);
    [SerializeField] Vector2 _paddleSize = new Vector2(32, 8);
    [SerializeField] float _paddleSpeed = 400f;
    
    [Header("Ball")]
    [SerializeField] Vector2 _ballPosition;
    [SerializeField] float _ballSize = 5f;
    [SerializeField] Color _ballColor;
    [SerializeField] float _ballInitialSpeed = 250f;

    RenderTexture _renderTexture;

    int _updateKernel;
    int _drawKernel;

    ComputeBuffer _paddleBuffer;
    ComputeBuffer _ballBuffer;

    struct Ball
    {
        public Vector4 Position;
        public Vector2 Velocity;
    }

    void OnEnable()
    {
        _renderTexture = new RenderTexture(_size, _size, 24)
        {
            filterMode = FilterMode.Point, 
            enableRandomWrite = true
        };
        _renderTexture.Create();

        _drawKernel = _computeShader.FindKernel("Draw");
        _updateKernel = _computeShader.FindKernel("Update");

        _computeShader.SetFloat("_Resolution", _size);
        _computeShader.SetTexture(_drawKernel, "_Result", _renderTexture);
        _computeShader.SetVector("_BackgroundColor", _backgroundColor);

        _computeShader.SetVector("_PaddleColor", _paddleColor);

        _paddleBuffer = new ComputeBuffer(1, 4 * sizeof(float));
        _paddleBuffer.SetData(new[] {new Vector4(_paddlePosition.x, _paddlePosition.y, _paddleSize.x, _paddleSize.y)});

        _ballBuffer = new ComputeBuffer(1, 6 * sizeof(float));
        Ball ball = new Ball
        {
            Position = new Vector4(_ballPosition.x, _ballPosition.y, _ballSize, 0f),
            Velocity = new Vector2(0.5f, 0.5f).normalized * _ballInitialSpeed
        };
        _ballBuffer.SetData(new[] {ball});

        _computeShader.SetBuffer(_updateKernel, "_Paddle", _paddleBuffer);
        _computeShader.SetBuffer(_drawKernel, "_Paddle", _paddleBuffer);

        _computeShader.SetBuffer(_updateKernel, "_Ball", _ballBuffer);
        _computeShader.SetBuffer(_drawKernel, "_Ball", _ballBuffer);

        _computeShader.SetFloat("_Input", 0f);

        _computeShader.SetVector("_BallColor", _ballColor);

        _computeShader.GetKernelThreadGroupSizes(_drawKernel, out uint xGroupSize, out uint yGroupSize, out _);
        _computeShader.Dispatch(_drawKernel, _renderTexture.width / (int) xGroupSize,
            _renderTexture.height / (int) yGroupSize, 1);
    }

    void Update()
    {
        var input = Input.GetAxisRaw("Horizontal") * _paddleSpeed;

        _computeShader.SetFloat("_Input", input);
        _computeShader.SetFloat("_DeltaTime", Time.deltaTime);

        _computeShader.Dispatch(_updateKernel, 1, 1, 1);

        _computeShader.GetKernelThreadGroupSizes(_drawKernel, out uint xGroupSize, out uint yGroupSize, out _);
        _computeShader.Dispatch(_drawKernel, _renderTexture.width / (int) xGroupSize,
            _renderTexture.height / (int) yGroupSize, 1);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(_renderTexture, dest);
    }

    void OnDisable()
    {
        _paddleBuffer.Dispose();
        _ballBuffer.Dispose();
    }
}