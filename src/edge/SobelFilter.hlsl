Texture2D<float> inputTexture : register(t0);
RWTexture2D<float> outputTexture : register(u0);

SamplerState samplerState : register(s0);

[numthreads(16, 16, 1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
    int2 coord = int2(DTid.xy);

    float3 sobel_x[3];
    sobel_x[0] = float3(-1, 0, 1);
    sobel_x[1] = float3(-2, 0, 2);
    sobel_x[2] = float3(-1, 0, 1);

    float3 sobel_y[3];
    sobel_y[0] = float3(-1, -2, -1);
    sobel_y[1] = float3(0, 0, 0);
    sobel_y[2] = float3(1, 2, 1);

    float color_x = 0.0;
    float color_y = 0.0;

    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float sample = inputTexture.Load(int3(coord + int2(x, y), 0));
            color_x += sample * sobel_x[y + 1][x + 1];
            color_y += sample * sobel_y[y + 1][x + 1];
        }
    }

    float color = sqrt(color_x * color_x + color_y * color_y);
    outputTexture[coord] = color;
}