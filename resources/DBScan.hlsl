struct BufferedPoint
{
	int X;
	int Y;
	uint CLUSTER_LABEL;
	uint EDGE_COUNT;
};

//Obtained through the sobel shader.
StructuredBuffer<BufferedPoint> InputBuffer : register(t0);
RWStructuredBuffer<BufferedPoint> OutputBuffer : register(u0);
RWStructuredBuffer<uint> PointCount : register(u1);

//Constants
cbuffer Params : register(b0)
{
	uint m;
	uint epsilon;
	uint iterations;
	uint padding;
};


//TODO:
//Use smaller datatypes.
//https://learn.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-scalar

[numthreads(256,1,1)]
void CSMain(uint3 DTid : SV_DispatchThreadID)
{
	uint ID = DTid.x;
	BufferedPoint coord = InputBuffer[ID];
	int edgeCount = 0;

	//First Pass to get the edge counts
	//Ideally would use spatial indexing.
	for (int i = 0; i < PointCount[0]; i++)
	{
		BufferedPoint compared = InputBuffer[i];
		float dist = abs((compared.X - coord.X) + (compared.Y - coord.Y));
		if (dist < epsilon)
		{
			edgeCount += 1;
		}
	}

	BufferedPoint result;
	result.X = InputBuffer[ID].X;
	result.Y = InputBuffer[ID].Y;
	result.CLUSTER_LABEL = ID;
	result.EDGE_COUNT = edgeCount;
	OutputBuffer[ID] = result;

	AllMemoryBarrier();


	//Second pass to get the cores and edges.
	int noiseLabel = -1;

	for (int j = 0; j < PointCount[0]; j++)
	{
		BufferedPoint compared = OutputBuffer[j];

		float dist = abs((compared.X - coord.X) + (compared.Y - coord.Y));

		if (edgeCount >= m) {
			noiseLabel = ID;
		}

		if (compared.EDGE_COUNT >= m && dist < epsilon) {
			noiseLabel = ID;
		}
	}

	AllMemoryBarrier();

	BufferedPoint result2;
	result2.X = InputBuffer[ID].X;
	result2.Y = InputBuffer[ID].Y;
	result2.CLUSTER_LABEL = noiseLabel;
	result2.EDGE_COUNT = edgeCount;
	OutputBuffer[ID] = result2;

	AllMemoryBarrier();


	//Expand the min outwards by the number of iterations. This limits the size of our selections since this is basically bfs.

	for (int l = 0; l < iterations; l++) {

		BufferedPoint current = InputBuffer[ID];
		int minID = current.CLUSTER_LABEL;

		//In desperate need of a spatial index.
		for (int k = 0; k < PointCount[0]; k++)
		{
			if (minID == -1) {
				break;
			}

			BufferedPoint compared = OutputBuffer[j];

			float dist = abs((compared.X - coord.X) + (compared.Y - coord.Y));
			if (compared.CLUSTER_LABEL == -1 && dist > epsilon) {
				break;
			}

			minID = min(minID, compared.CLUSTER_LABEL);
		}

		AllMemoryBarrier();

		BufferedPoint resultFinal;
		resultFinal.X = InputBuffer[ID].X;
		resultFinal.Y = InputBuffer[ID].Y;
		resultFinal.CLUSTER_LABEL = minID;
		resultFinal.EDGE_COUNT = edgeCount;
		OutputBuffer[ID] = resultFinal;

		AllMemoryBarrier();
	}


}