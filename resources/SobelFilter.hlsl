Texture2D<float> inputTexture : register(t0);

//Constants
cbuffer Params : register(b0)
{
    int m;
    int epsilon;
    int iterations;
    float edgeThreshold;
    int padding1;
    int padding2;
    int padding3;
    int padding4;
};

struct BufferedPoint
{
    int X;
    int Y;
    uint CLUSTER_LABEL;
    uint EDGE_COUNT;
};

RWStructuredBuffer<BufferedPoint> OutputBuffer : register(u0);

//For the dbscan
RWTexture2D<int2> OutputTexture : register(u2);
RWStructuredBuffer<uint> OutputCounter : register(u1);
SamplerState samplerState : register(s0);

[numthreads(16, 16, 1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
    uint2 coord = int2(DTid.xy);

    

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
    
    float color = color_x * color_x + color_y * color_y;
    int color_thresh = int(color_x * color_x + color_y * color_y);
    if (color_thresh > edgeThreshold)
    {
        // Atomically increment the counter and get the index to store the pixel coordinates
        uint index = 0;
        InterlockedAdd(OutputCounter[0], 1, index);

        //No need to initialize other parameters other than the texture coordinates, this will be done in the second pass.
        BufferedPoint ret;
        ret.X = coord.x;
        ret.Y = coord.y;
        ret.CLUSTER_LABEL = 0;
        ret.EDGE_COUNT = 0;
        OutputBuffer[index] = ret;

        //Add it to the texture (for passes in DBScan)
        OutputTexture[coord] = index,0;
    }
}