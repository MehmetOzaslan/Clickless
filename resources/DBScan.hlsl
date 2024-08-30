struct BufferedPoint
{
	int X;
	int Y;
	uint CLUSTER_LABEL;
	uint EDGE_COUNT;
};

//Obtained through the sobel shader.
RWStructuredBuffer<BufferedPoint> OutputBuffer : register(u0);

RWStructuredBuffer<uint> PointCount : register(u1);
RWTexture2D<int2> PointTexture : register(u2);


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


//Use smaller datatypes.
//https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-scalar

[numthreads(256,1,1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
	uint ID = DTid.x;

	BufferedPoint current = OutputBuffer[ID];

	int2 coord = int2(current.X, current.Y);
	//TODO: Slight optimization on how the points are clustered by initializing them to an epsilon grid.
	//int clusterID = current.x / epsilon + 

	//First Pass to get the edge counts
	//Ideally would use spatial indexing.
	//Essentially a nxn convolution
	int edgeCount = -1;//-1 for self counting.
	for (int y1 = -epsilon; y1 <= epsilon; y1++)
	{
		for (int x1 = -epsilon; x1 <= epsilon; x1++)
		{
			int2 sample = PointTexture.Load((coord + int2(x1, y1)));

			if (sample.x > 0) {
				edgeCount += 1;
			}
		}
	}

	GroupMemoryBarrierWithGroupSync();
	PointTexture[coord] = int2(0, edgeCount);
	GroupMemoryBarrierWithGroupSync();

	//Second pass to get the cores and edges.
	//NOTE: Noise is labeled as 0 here.
	int clusterLabel = 0;
	if (edgeCount >= m) {
		clusterLabel = ID;
	}

	for (int y2 = -epsilon; y2 <= epsilon; y2++)
	{
		for (int x2 = -epsilon; x2 <= epsilon; x2++)
		{
			int2 sample = PointTexture.Load((coord + int2(x2, y2)));
			int sampleEdgeCount = sample.y;

			if (sampleEdgeCount >= m) {
				clusterLabel = ID;
			}
		}
	}

	GroupMemoryBarrierWithGroupSync();
	PointTexture[coord] = int2(clusterLabel, edgeCount);
	GroupMemoryBarrierWithGroupSync();


	//Expand the min outwards by the number of iterations. This limits the size of our selections and is essentially BFS.
	//Doing some rough calculations, if we have a 500x500 area, with an epsilon of 10,
	//the area would be fully ID'd after 500 / (e) iterations, or 50 iteration of the outer loop.
	//Realistically, we'd expect buttons to be about ~ 100 pixels.

	for (int l = 0; l < iterations; l++) {

		int currentLabel = 1;

		//Epsilon scan
		for (int y3 = -epsilon; y3 <= epsilon; y3++)
		{
			//Skip over any points labelled as noisy.
			if (clusterLabel <= 0) {
				break;
			}

			for (int x3 = -epsilon; x3 <= epsilon; x3++)
			{
				int comparedLabel = PointTexture.Load(coord + int2(x3, y3)).x;
				currentLabel = max(comparedLabel, currentLabel);
			}
		}
		//Could probably reduce this to one barrier with a swapchain.
		GroupMemoryBarrierWithGroupSync();
		PointTexture[coord] = int2(currentLabel, edgeCount); // This works.
		GroupMemoryBarrierWithGroupSync();

	}

	//Finally write the values in the texture to the buffer of points.
	int2 sample = PointTexture.Load(coord);

	BufferedPoint resultFinal;
	resultFinal.X = coord.x;
	resultFinal.Y = coord.y;
	resultFinal.CLUSTER_LABEL = sample.x;
	resultFinal.EDGE_COUNT = sample.y;
	OutputBuffer[ID] = resultFinal;
}