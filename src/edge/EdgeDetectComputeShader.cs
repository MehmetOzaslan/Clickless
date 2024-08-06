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
using static Clickless.src.MLClientOpenCVSharp;

namespace Clickless.src.edge
{
    class EdgeDetectComputeShader
    {
        private Device device;
        private ComputeShader computeShader;
        private DeviceContext context;
        ShaderResourceView shaderResourceView;
        UnorderedAccessView unorderedAccessView;

        private string computeFilePath { get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SobelFilter.cso"); }   

        public EdgeDetectComputeShader()
        {
            device = new Device(DriverType.Hardware, DeviceCreationFlags.None);
            context = device.ImmediateContext;

            //Load the bytecode.
            var shaderBytecode = File.ReadAllBytes(computeFilePath);
            computeShader = new ComputeShader(device, shaderBytecode);
        }



        public IEnumerable<EdgePt> GetBitmapEdges(Bitmap bitmap)
        {
            Texture2D inputTexture;
            Texture2DDescription inputTextureDesc;
            CopyCapturedBitmapToGPUTexture(device, bitmap, out inputTexture, out inputTextureDesc);
            
            Texture2DDescription outputTextureDesc;
            Texture2D outputTexture;
            CreateGPUOutputTexture(inputTextureDesc.Width, inputTextureDesc.Height, out outputTexture, out outputTextureDesc);

            shaderResourceView = new ShaderResourceView(device, inputTexture);
            unorderedAccessView = new UnorderedAccessView(device, outputTexture);

            // Set compute shader and resources
            context.ComputeShader.Set(computeShader);
            context.ComputeShader.SetShaderResource(0, shaderResourceView);
            context.ComputeShader.SetUnorderedAccessView(0, unorderedAccessView, 0);

            // Dispatch compute shader
            int threadGroupX = (inputTexture.Description.Width + 15) / 16;
            int threadGroupY = (inputTexture.Description.Height + 15) / 16;
            context.Dispatch(threadGroupX, threadGroupY, 1);

            // Move the texture to the CPU
            Texture2D cpuTexture;
            MoveTextureToCPU(outputTextureDesc, outputTexture, out cpuTexture);
            var dataBox = context.MapSubresource(cpuTexture, 0, MapMode.Read, MapFlags.None);
            var dataPtr = dataBox.DataPointer;

            // Iterate over the gpuSrcTexture data
            int width = outputTextureDesc.Width;
            int height = outputTextureDesc.Height;
            int pixelSize = 4; // For R8G8B8A8 format

            //NOTE: This can be turned into an enumerable passed into the dbscan to eke out some more performance on the memory side.
            ConcurrentBag<EdgePt> edges = new ConcurrentBag<EdgePt>();
            Parallel.For(0, height * width, (int i) =>
            {
                int x = i % width;
                int y = i / width;
                int index = (y * width + x) * pixelSize;
                byte r = Marshal.ReadByte(dataPtr, index);
                byte g = Marshal.ReadByte(dataPtr, index + 1);
                byte b = Marshal.ReadByte(dataPtr, index + 2);
                byte a = Marshal.ReadByte(dataPtr, index + 3);

                var res = r & g & b & a;

                //Add the coordinates of non-zero pixels.
                if(res > 0)
                {
                    edges.Add(new EdgePt(x, y));
                }

            });

            return edges;
        }


        private void CreateGPUOutputTexture(int width, int height, out Texture2D outputTexture, out Texture2DDescription outputTextureDesc)
        {
            // Create output gpuSrcTexture
            outputTextureDesc = new Texture2DDescription
            {
                Width = width,
                Height = height,
                ArraySize = 1,
                BindFlags = BindFlags.UnorderedAccess | BindFlags.ShaderResource,
                Usage = ResourceUsage.Default,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R32_Float,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0)
            };
            outputTexture = new Texture2D(device, outputTextureDesc);
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

        /// <summary>
        /// Takes a bitmap and loads it to the gpu.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="bitmap"></param>
        /// <param name="gpuDestinationTexture"></param>
        /// <param name="gpuDestinationTextureDesc"></param>
        private static void CopyCapturedBitmapToGPUTexture(Device device, Bitmap bitmap, out Texture2D gpuDestinationTexture, out Texture2DDescription gpuDestinationTextureDesc )
        {
            gpuDestinationTextureDesc = new Texture2DDescription
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                Usage = ResourceUsage.Immutable,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm, //TODO: Can probaly change this to B8G8R8 or similar if it exists, might need to recompile the hlsl file too.
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0)
            };

            //Loading here.
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            gpuDestinationTexture = new Texture2D(device, gpuDestinationTextureDesc, new DataRectangle(bitmapData.Scan0, bitmapData.Stride));
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
