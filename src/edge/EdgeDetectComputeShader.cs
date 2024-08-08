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

namespace Clickless.src
{
    class EdgeDetectComputeShader : IEdgeProvider
    {
        private Device device;
        private ComputeShader computeShader;
        private DeviceContext context;
        private Texture2D inputTexture;
        private Buffer outputBuffer;
        private Buffer outputCounter;
        private int bufferSize = 0;

        UnorderedAccessView outputBufferUav;
        UnorderedAccessView outputCounterUAV;


        ShaderResourceView shaderResourceView;
        UnorderedAccessView unorderedAccessView;

        private string computeFilePath { get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "SobelFilter.cso"); }

        public EdgeDetectComputeShader()
        {
            device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            context = device.ImmediateContext;

            //Load the bytecode.
            var shaderBytecode = File.ReadAllBytes(computeFilePath);
            computeShader = new ComputeShader(device, shaderBytecode);
            // Set compute shader and resources
            context.ComputeShader.Set(computeShader);
        }

        private Buffer GetOutputBuffer()
        {
            if (bufferSize != inputTexture.Description.Height * inputTexture.Description.Width || outputBuffer == null)
            {
                bufferSize = inputTexture.Description.Height * inputTexture.Description.Width;
                outputBuffer = new Buffer(device, new BufferDescription
                {
                    SizeInBytes = sizeof(int) * 2 * bufferSize,
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.UnorderedAccess,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.BufferStructured,
                    StructureByteStride = sizeof(int) * 2,
                });
                outputBufferUav = new UnorderedAccessView(device, outputBuffer);
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



        public IEnumerable<IPointData> GetEdges(Bitmap bitmap)
        {

            Stopwatch timer = Stopwatch.StartNew();

            CopyCapturedBitmapToGPUTexture(bitmap);

            timer.Stop();
            TimeSpan timespan = timer.Elapsed;
            Console.WriteLine("Copying to Gpu took: " + timespan.TotalMilliseconds);

            var outputBuffer = GetOutputBuffer();
            var outputCounter = GetCounterBuffer();

            timer = Stopwatch.StartNew();

            shaderResourceView = new ShaderResourceView(device, inputTexture);


            context.ComputeShader.SetShaderResource(0, shaderResourceView);
            //context.ComputeShader.SetUnorderedAccessView(0, unorderedAccessView, 0);
            context.ComputeShader.SetUnorderedAccessView(0, outputBufferUav);
            context.ComputeShader.SetUnorderedAccessView(1, outputCounterUAV);

            int[] initialCounter = { 0 };
            context.UpdateSubresource(initialCounter, outputCounter);

            // Dispatch compute shader
            int threadGroupX = (inputTexture.Description.Width + 15) / 16;
            int threadGroupY = (inputTexture.Description.Height + 15) / 16;
            context.Dispatch(threadGroupX, threadGroupY, 1);

            timer.Stop();
            timespan = timer.Elapsed;
            Console.WriteLine("Compute Took: " + timespan.TotalMilliseconds);


            timer = Stopwatch.StartNew();


            // Create a staging buffer for reading data back
            var stagingBuffer = new Buffer(device, new BufferDescription
            {
                SizeInBytes = Utilities.SizeOf<BufferedInt2>() * bufferSize,
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = Utilities.SizeOf<BufferedInt2>(),
            });


            var resCounterBuffer = new Buffer(device, new BufferDescription
            {
                SizeInBytes = sizeof(int),
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = sizeof(int),
            });


            context.CopyResource(outputBuffer, stagingBuffer);
            context.CopyResource(outputCounter, resCounterBuffer);


            //Get the buffer size.
            DataStream counterSize;
            context.MapSubresource(resCounterBuffer, MapMode.Read, MapFlags.None, out counterSize);
            int[] counterResult = new int[1];
            counterSize.ReadRange(counterResult, 0, 1);
            context.UnmapSubresource(resCounterBuffer, 0);

            int validEntries = counterResult[0];
            Console.WriteLine("EdgeCt: " + validEntries + " Maximum Should be " + bufferSize);


            DataStream dataStream;
            context.MapSubresource(stagingBuffer, MapMode.Read, MapFlags.None, out dataStream);
            var results = new BufferedInt2[validEntries];
            dataStream.ReadRange(results, 0, validEntries);
            context.UnmapSubresource(stagingBuffer, 0);

            ConcurrentBag<IPointData> edges = new ConcurrentBag<IPointData>();

            var ret = results.Cast<IPointData>();

            timer.Stop();
            timespan = timer.Elapsed;
            Console.WriteLine("Edge Movement Took: " + timespan.TotalMilliseconds);


            timer = Stopwatch.StartNew();

            dataStream.Dispose();
            shaderResourceView.Dispose();

            timer.Stop();
            timespan = timer.Elapsed;
            Console.WriteLine("Cleanup Took: " + timespan.TotalMilliseconds);



            return ret;
            //Parallel.ForEach(results, (item)=>
            //{
            //    edges.Add(new EdgePt(item.X, item.Y));
            //} );

            //foreach (var coord in results)
            //{
            //    if (coord.X != 0 || coord.Y != 0)
            //    {
            //        Console.WriteLine($"Pixel coordinate: ({coord.X}, {coord.Y})");
            //    }
            //}
        }

        public struct BufferedInt2 : IPointData
        {
            public int X;
            public int Y;

            public Dbscan.Point Point => new Dbscan.Point(X, Y);
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

            bitmap.UnlockBits(bitmapData);
        }

        private void CopyCapturedBitmapToGPUTexture(Bitmap bitmap)
        {
            if (inputTexture == null || inputTexture.Description.Width != bitmap.Width || inputTexture.Description.Height != bitmap.Height) {

                Console.WriteLine("Initializing gpu.");
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

        


        ////Form used to debug.
        //private static void DisplayBitmap(Bitmap bitmap)
        //{
        //    // Gather system information
        //    string systemInfo = $"OS: {Environment.OSVersion}\n" +
        //                        $"64-bit OS: {Environment.Is64BitOperatingSystem}\n" +
        //                        $"64-bit Process: {Environment.Is64BitProcess}\n" +
        //                        $"Processor Count: {Environment.ProcessorCount}\n" +
        //                        $"Machine Name: {Environment.MachineName}\n" +
        //                        $"User Name: {Environment.UserName}\n" +
        //                        $"System Directory: {Environment.SystemDirectory}\n" +
        //                        $"Current Directory: {Environment.CurrentDirectory}\n" +
        //                        $"Memory Usage: {Environment.WorkingSet / 1024 / 1024} MB";

        //    Form form = new Form
        //    {
        //        Text = "Processed Image",
        //        ClientSize = new Size(bitmap.Width, bitmap.Height)
        //    };

        //    PictureBox pictureBox = new PictureBox
        //    {
        //        Dock = DockStyle.Fill,
        //        Image = bitmap,
        //        SizeMode = PictureBoxSizeMode.Zoom
        //    };

        //    Label infoLabel = new Label
        //    {
        //        Text = systemInfo,
        //        AutoSize = true,
        //        BackColor = System.Drawing.Color.FromArgb(200, 255, 255, 255), // semi-transparent white background
        //        ForeColor = System.Drawing.Color.Black,
        //        TextAlign = ContentAlignment.BottomRight,
        //        Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        //        Padding = new Padding(5)
        //    };

        //    form.Controls.Add(pictureBox);
        //    form.Controls.Add(infoLabel);

        //    // Position the label in the bottom right corner
        //    infoLabel.Location = new Point(form.ClientSize.Width - infoLabel.Width - 10, form.ClientSize.Height - infoLabel.Height - 10);

        //    // Handle form resize to reposition the label
        //    form.Resize += (sender, e) =>
        //    {
        //        infoLabel.Location = new Point(form.ClientSize.Width - infoLabel.Width - 10, form.ClientSize.Height - infoLabel.Height - 10);
        //    };

        //    Application.Run(form);
        //}
    }
}
