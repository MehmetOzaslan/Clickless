using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX.DXGI;
using System;
using System.IO;
using SharpDX.Direct3D;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Media.TextFormatting;
using System.Runtime.Remoting.Contexts;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using static Clickless.src.MLClient;
using System.Diagnostics;
using Buffer = SharpDX.Direct3D11.Buffer;
using Dbscan;
using System.Linq;
using System.Windows.Markup;
using Clickless.src.edge;

namespace Clickless.src
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

    class ImageRectDetectComputeShader : ImageToRectProvider
    {

        private Params shaderParams = new Params { epsilon = 10, m = 10, iterations = 50 };

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
        }

        public override IEnumerable<IPointData> GetEdges(Bitmap bitmap)
        {
            InitializeGPUParams();
            CopyCapturedBitmapToGPUTexture(bitmap);
            RunSobelPass();
            SquashBuffer();
            RunDBScanPass(); // TODO: Remove this (only kept for testing)


            IEnumerable<IPointData> ret = GetPointBuffer().Cast<IPointData>();

            inputTextureSRV?.Dispose();
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



            inputTextureSRV?.Dispose();
            ResetCounterBuffer();

            return null;
        }

        private void InitializeGPUParams()
        {
            if(paramBuffer != null)
            {
                paramBuffer.Dispose();
            }

            paramBuffer = new Buffer(device, new BufferDescription
            {
                SizeInBytes = Utilities.SizeOf<Params>(),
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            //Use the parameters.
            context.ComputeShader.SetConstantBuffer(0, paramBuffer);
            context.UpdateSubresource(ref shaderParams, paramBuffer);
        }

        private Buffer GetOutputBuffer()
        {
            int currBufferSize = (inputTexture.Description.Height * inputTexture.Description.Width) / bufferSizeScalarReduction;
            if (bufferSize != currBufferSize || outputBuffer == null)
            {
                bufferSize = currBufferSize;
                outputBuffer?.Dispose();
                outputBuffer = new Buffer(device, new BufferDescription
                {
                    SizeInBytes = Utilities.SizeOf<BufferedPoint>() * bufferSize,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.UnorderedAccess,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.BufferStructured,
                    StructureByteStride = Utilities.SizeOf<BufferedPoint>(),
                });
                outputBufferUAV?.Dispose();
                outputBufferUAV = new UnorderedAccessView(device, outputBuffer);
            }
            return outputBuffer;
        }

        private Buffer GetCounterBuffer()
        {
            if (outputCounter == null)
            {
                outputCounter = new Buffer(device, new BufferDescription
                {
                    SizeInBytes = sizeof(int),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.UnorderedAccess,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.BufferStructured,
                    StructureByteStride = sizeof(int),
                });

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
            var resCounterBuffer = new Buffer(device, new BufferDescription
            {
                SizeInBytes = sizeof(int),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = sizeof(int),
            });

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
            outputBuffer = new BufferResizer(device).ResizeBuffer<BufferedPoint>(outputBuffer, bufferSize);
        }

        BufferedPoint[] GetPointBuffer()
        {
            // Create a staging buffer for reading data back
            var stagingBuffer = new Buffer(device, new BufferDescription
            {
                SizeInBytes = Utilities.SizeOf<BufferedPoint>() * bufferSize,
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = Utilities.SizeOf<BufferedPoint>(),
            });

            context.CopyResource(outputBuffer, stagingBuffer);


            //Get the pixels obtained through the compute shader.
            context.MapSubresource(stagingBuffer, MapMode.Read, MapFlags.None, out var dataStream);
            var results = new BufferedPoint[bufferSize];
            dataStream.ReadRange(results, 0, bufferSize);
            context.UnmapSubresource(stagingBuffer, 0);


            Console.WriteLine($"Distinct values: {results.Select(x => x.CLUSTER_LABEL).Distinct().Count()} Total: {bufferSize}");


            for (int i = 0; i < 100; i++)
            {
                var res = results[i];
                Console.WriteLine($"XY:({res.X},{res.Y}) ID:{res.CLUSTER_LABEL} Edges:{res.EDGE_COUNT} ");
            }

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
        private void MoveTextureToCPU(Texture2DDescription gpuSrcDesc, Texture2D gpuSrcTexture, out Texture2D cpuDestinationTexture)
        {
            Texture2DDescription cpuDescription;
            cpuDescription = gpuSrcDesc;
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



    public class BufferResizer : IDisposable
    {
        private Device device;
        private DeviceContext context;

        public BufferResizer(Device device)
        {
            this.device = device;
            this.context = device.ImmediateContext;
        }

        public Buffer ResizeBuffer<T>(Buffer oldBuffer, int newSize) where T : struct
        {
            // Create a new buffer with the desired size
            var newBuffer = new Buffer(device, new BufferDescription
            {
                SizeInBytes = Utilities.SizeOf<T>() * newSize,
                Usage = oldBuffer.Description.Usage,
                BindFlags = oldBuffer.Description.BindFlags,
                CpuAccessFlags = oldBuffer.Description.CpuAccessFlags,
                OptionFlags = oldBuffer.Description.OptionFlags,
                StructureByteStride = oldBuffer.Description.StructureByteStride
            });

            // Determine the number of elements to copy
            int oldSize = oldBuffer.Description.SizeInBytes / Utilities.SizeOf<T>();
            int elementsToCopy = Math.Min(oldSize, newSize);

            // Copy data from the old buffer to the new buffer
            if (elementsToCopy > 0)
            {
                context.CopySubresourceRegion(oldBuffer, 0, new ResourceRegion(0, 0, 0, elementsToCopy * Utilities.SizeOf<T>(), 1, 1), newBuffer, 0);
            }

            // Release the old buffer
            oldBuffer.Dispose();

            return newBuffer;
        }

        public void Dispose()
        {
            context?.ClearState();
            context?.Flush();
            device?.Dispose();
        }
    }
}
