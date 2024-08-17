﻿using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.IO;
using SharpDX.Direct3D;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.Collections.Generic;

using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Buffer = SharpDX.Direct3D11.Buffer;
using Dbscan;
using System.Linq;

namespace Clickless
{

    // Parameters to send to the GPU
    [StructLayout(LayoutKind.Sequential)]
    struct Params
    {
        public int m;
        public int epsilon;
        public int iterations;
        public int padding; //Necessary for the buffer
    };

    // Data to and from the GPU
    // NOTE: for resolutions larger than 32k pixels this needs to be changed.
    [StructLayout(LayoutKind.Sequential)]
    public struct BufferedPoint : IPointData
    {
        public int X;
        public int Y;
        public uint CLUSTER_LABEL;
        public uint EDGE_COUNT;

        public Dbscan.Point Point => new Dbscan.Point(X, Y);
    }

    class ImageRectDetectComputeShader : ImageToRectEngine
    {

        private Params shaderParams;

        private Device device;
        private ComputeShader sobelShader;
        private ComputeShader dbScan;

        private DeviceContext context;
        private Texture2D inputTexture;
        private Texture2D outputTexture;
        private Buffer outputBuffer;
        private Buffer outputCounter;
        private Buffer paramBuffer;

        private int bufferSize = 0;
        private const int bufferSizeScalarReduction = 4;


        UnorderedAccessView outputTextureUAV;// Register u2
        UnorderedAccessView outputBufferUAV; // Register u0
        UnorderedAccessView outputCounterUAV;// Register u1

        ShaderResourceView inputTextureSRV;

        private string SobelFilterFilePath { get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "SobelFilter.cso"); }
        private string DBScanFilePath { get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "DBScan.cso"); }

        public ImageRectDetectComputeShader()
        {
            device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            context = device.ImmediateContext;

            //Load in the bytecode for the shaders.
            sobelShader = new ComputeShader(device, File.ReadAllBytes(SobelFilterFilePath));
            dbScan = new ComputeShader(device, File.ReadAllBytes(DBScanFilePath));
        }

        public static bool DeviceSupportsCompute()
        {
            var dev= new Device(DriverType.Hardware, DeviceCreationFlags.None);
            var supports = dev.CheckFeatureSupport(SharpDX.Direct3D11.Feature.ComputeShaders);
            dev.Dispose();
            return supports;
        }


        private void RunSobelPass()
        {
            context.ComputeShader.Set(sobelShader);

            outputBuffer = GetOutputBuffer();
            outputCounter = GetCounterBuffer();
            inputTextureSRV = new ShaderResourceView(device, inputTexture);
            context.ComputeShader.SetShaderResource(0, inputTextureSRV);
            context.ComputeShader.SetUnorderedAccessView(0, outputBufferUAV);
            context.ComputeShader.SetUnorderedAccessView(1, outputCounterUAV);
            context.ComputeShader.SetUnorderedAccessView(2, outputTextureUAV);

            int threadGroupX = (inputTexture.Description.Width + 15) / 16;
            int threadGroupY = (inputTexture.Description.Height + 15) / 16;
            context.Dispatch(threadGroupX, threadGroupY, 1);

            context.ComputeShader.SetShaderResource(0, null);
            context.ComputeShader.SetUnorderedAccessView(0, null);
            context.ComputeShader.SetUnorderedAccessView(1, null);
            context.ComputeShader.SetShaderResource(1, null);
            context.ComputeShader.SetUnorderedAccessView(2, null);

            context.Flush();
        }


        private void RunDBScanPass()
        {
            outputBufferUAV?.Dispose();
            outputBufferUAV = new UnorderedAccessView(device, outputBuffer);

            context.ComputeShader.SetUnorderedAccessView(0, outputBufferUAV);
            context.ComputeShader.SetUnorderedAccessView(1, outputCounterUAV);
            context.ComputeShader.SetUnorderedAccessView(2, outputTextureUAV);

            context.ComputeShader.Set(dbScan);

            int threadsPerGroup = 256;
            int threadGroups = (int)Math.Ceiling((double)bufferSize / threadsPerGroup);
            context.Dispatch(threadGroups, 1, 1);

            context.ComputeShader.SetShaderResource(0, null);
            context.ComputeShader.SetUnorderedAccessView(0, null);
            context.ComputeShader.SetUnorderedAccessView(1, null);
            context.ComputeShader.SetUnorderedAccessView(2, null);
        }

        public override IEnumerable<IPointData> GetEdges(Bitmap bitmap)
        {
            InitializeGPUParams();
            CopyCapturedBitmapToGPUTexture(bitmap);
            RunSobelPass();
            SquashBuffer();
            RunDBScanPass(); // TODO: Remove this (only kept for testing)

            IEnumerable<IPointData> ret = GetPointBuffer().Cast<IPointData>();

            ResetCounterBuffer();
            return ret;
        }

        public override IEnumerable<Rectangle> GetRects(Bitmap bitmap)
        {
            InitializeGPUParams();
            CopyCapturedBitmapToGPUTexture(bitmap);
            RunSobelPass();
            SquashBuffer();
            RunDBScanPass();

            var points = GetPointBuffer();

            Dictionary<uint, List<BufferedPoint>> clusters = new Dictionary<uint, List<BufferedPoint>>();
            
            //Some points marked as noise
            int NOISE = 0;
            int NOISE_PADDING = 10;


            foreach (BufferedPoint point in points)
            {
                if(point.CLUSTER_LABEL == NOISE || point.Y <= NOISE_PADDING || point.Y >= inputTexture.Description.Height- NOISE_PADDING || point.X <= NOISE_PADDING || point.X >= inputTexture.Description.Width- NOISE_PADDING) continue;

                if(!clusters.ContainsKey(point.CLUSTER_LABEL))
                    clusters[point.CLUSTER_LABEL] = new List<BufferedPoint>();
                clusters[point.CLUSTER_LABEL].Add(point);
            }

            //GetClusterRect();

            List<Rectangle> rects = new List<Rectangle>();
            foreach (var item in clusters)
            {
                rects.Add(GetClusterRect(item.Value));
            }

            ResetCounterBuffer();
            return rects;
        }

        private void InitializeGPUParams()
        {
            if(paramBuffer != null)
            {
                paramBuffer.Dispose();
            }

            shaderParams = new Params() { 
                iterations = detectionSettings.iterations,
                m = detectionSettings.m,
                epsilon = detectionSettings.epsilon };

            paramBuffer = BufferCreate.InitializeConstantBuffer<Params>(device)
                                        .SetConstantBufferData(shaderParams, context)
                                        .GetBuffer();
        }

        private Buffer GetOutputBuffer()
        {
            int currBufferSize = (inputTexture.Description.Height * inputTexture.Description.Width) / bufferSizeScalarReduction;
            if (bufferSize != currBufferSize || outputBuffer == null)
            {
                bufferSize = currBufferSize;
                outputBuffer?.Dispose();

                outputBuffer = BufferCreate.InitializeRWBuffer<BufferedPoint>(device, currBufferSize)
                                            .GetBuffer();

                outputBufferUAV?.Dispose();
                outputBufferUAV = new UnorderedAccessView(device, outputBuffer);
            }
            return outputBuffer;
        }

        private Buffer GetCounterBuffer()
        {
            if (outputCounter == null)
            {
                outputCounter?.Dispose();
                outputCounter = BufferCreate.InitializeRWBuffer<int> (device, 1)
                                .GetBuffer();

                outputCounterUAV?.Dispose();
                outputCounterUAV = new UnorderedAccessView(device, outputCounter);
            }
            return outputCounter;
        }


        private void ResetCounterBuffer()
        {
            int[] initialCounter = { 0 };
            context.UpdateSubresource(initialCounter, outputCounter);
        }


        private int GetEdgeDetectionCount()
        {
            var resCounterBuffer = BufferCreate.InitializeStagingBuffer<int>(device, 1)
                                               .CopyFrom(outputCounter, context)
                                               .GetBuffer();

            context.CopyResource(outputCounter, resCounterBuffer);
            context.MapSubresource(resCounterBuffer, MapMode.Read, MapFlags.None, out var counterSize);
            int[] counterResult = new int[1];
            counterSize.ReadRange(counterResult, 0, 1);
            context.UnmapSubresource(resCounterBuffer, 0);

            int validEntries = counterResult[0];

            resCounterBuffer.Dispose();
            return validEntries;
        }

        private void SquashBuffer()
        {
            bufferSize = GetEdgeDetectionCount();
            outputBuffer = BufferCreate.ResizeBuffer<BufferedPoint>(device, context, outputBuffer, bufferSize);
        }

        BufferedPoint[] GetPointBuffer()
        {
            var stagingBuffer = BufferCreate.InitializeStagingBuffer<BufferedPoint>(device, bufferSize)
                                            .CopyFrom(outputBuffer, context)
                                            .GetBuffer();

            //Get the pixels obtained through the compute shader.
            context.MapSubresource(stagingBuffer, MapMode.Read, MapFlags.None, out var dataStream);
            var results = new BufferedPoint[bufferSize];
            dataStream.ReadRange(results, 0, bufferSize);
            context.UnmapSubresource(stagingBuffer, 0);

            dataStream.Dispose();
            stagingBuffer.Dispose();

            return results;
        }

        /// <summary>
        /// Changes the flags to allow the cpu to read the result texture and copies it over.
        /// </summary>
        /// <param name="gpuSrcDesc"></param>
        /// <param name="gpuSrcTexture"></param>
        /// <param name="cpuDestinationTexture"></param>
        private void MoveTextureToCPU(Texture2D gpuSrcTexture, out Texture2D cpuDestinationTexture)
        {
            Texture2DDescription cpuDescription;
            cpuDescription = gpuSrcTexture.Description;
            cpuDescription.Usage = ResourceUsage.Staging;
            cpuDescription.BindFlags = BindFlags.None;
            cpuDescription.CpuAccessFlags = CpuAccessFlags.Read;

            cpuDestinationTexture = new Texture2D(device, cpuDescription);

            // Copy data from the GPU gpuSrcTexture to the staging gpuSrcTexture
            device.ImmediateContext.CopyResource(gpuSrcTexture, cpuDestinationTexture);
        }

        private void InitializeTexture2D(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var dataBox = new DataRectangle(bitmapData.Scan0, bitmapData.Stride);


            inputTexture?.Dispose();
            inputTexture = new Texture2D(device,
                new Texture2DDescription
                {
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    ArraySize = 1,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Dynamic,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    Format = Format.B8G8R8A8_UNorm, //TODO: Can probaly change this to B8G8R8 or similar if it exists, might need to recompile the hlsl file too.
                    MipLevels = 1,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0)
                },
                dataBox
            );


            outputTexture?.Dispose();
            outputTexture = new Texture2D(device,
                new Texture2DDescription
                {
                    Width = inputTexture.Description.Width,
                    Height = inputTexture.Description.Height,
                    ArraySize = inputTexture.Description.ArraySize,
                    BindFlags = BindFlags.UnorderedAccess,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = Format.R32G32_SInt, //RWTexture2D<int2> on the GPU
                    MipLevels = inputTexture.Description.MipLevels,
                    SampleDescription = new SampleDescription(1, 0)
                }
            );


            outputTextureUAV?.Dispose();
            outputTextureUAV = new UnorderedAccessView(device,outputTexture);

            bitmap.UnlockBits(bitmapData);
        }

        private void CopyCapturedBitmapToGPUTexture(Bitmap bitmap)
        {
            if (inputTexture == null || inputTexture.Description.Width != bitmap.Width || inputTexture.Description.Height != bitmap.Height) {
                InitializeTexture2D(bitmap);
            }

            else
            {
                var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                DataBox dataBox = context.MapSubresource(inputTexture, 0, MapMode.WriteDiscard, MapFlags.None);

                //Copy data to the GPU using marshal.
                int dataSize = bitmapData.Stride * bitmap.Height;

                // Create a managed array to hold the bitmap data
                byte[] data = new byte[dataSize];

                // Copy the bitmap data to the managed array
                Marshal.Copy(bitmapData.Scan0, data, 0, dataSize);
                Marshal.Copy(data, 0, dataBox.DataPointer, dataSize);
                context.UnmapSubresource(inputTexture, 0);

                bitmap.UnlockBits(bitmapData);
            }
        }
    }
}
