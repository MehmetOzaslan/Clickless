using SharpDX.Direct3D11;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using SharpDX.DXGI;
using Format = SharpDX.DXGI.Format;

namespace Clickless
{
    public class TextureCreate
    {

        private Device _device;
        private Texture2D _texture;

        private TextureCreate(Device device, Texture2D texture)
        {
            _texture = texture;
            _device = device;
        }

        public Texture2D GetTexture()
        {
            return _texture;
        }

        #region Texture Intialization Functons

        public static TextureCreate CreateGPUResourceFromBitmap(Device device, Bitmap bitmap, Format format, int miplevels = 1, int arraySize = 1)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var dataBox = new DataRectangle(bitmapData.Scan0, bitmapData.Stride);

            var texture = new Texture2D(device,
                new Texture2DDescription
                {
                    Width = bitmap.Width,
                    Height = bitmap.Height,
                    ArraySize = arraySize,
                    BindFlags = BindFlags.ShaderResource,
                    Usage = ResourceUsage.Dynamic,
                    CpuAccessFlags = CpuAccessFlags.Write,
                    Format = format,
                    MipLevels = miplevels,
                    OptionFlags = ResourceOptionFlags.None,
                    SampleDescription = new SampleDescription(1, 0)
                },
                dataBox
            );

            bitmap.UnlockBits(bitmapData);

            return new TextureCreate(device, texture);
        }


        public static TextureCreate CreateRWTexture(Device device,  int width, int height, Format format, int miplevels = 1, int arraySize = 1) { 
            var texture = new Texture2D(device,
                new Texture2DDescription
                {
                    Width = width,
                    Height = height,
                    ArraySize = arraySize,
                    BindFlags = BindFlags.UnorderedAccess,
                    Usage = ResourceUsage.Default,
                    CpuAccessFlags = CpuAccessFlags.None,
                    Format = format,
                    MipLevels = miplevels,
                    SampleDescription = new SampleDescription(1, 0)
                }
            );
            return new TextureCreate(device, texture);
        }


        #endregion

        #region Utility Functions

        /// <summary>
        /// Updates a texture already on the GPU
        /// </summary>
        public static void UpdateFromBitmap(DeviceContext context, Texture2D texture, Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            DataBox dataBox = context.MapSubresource(texture, 0, MapMode.WriteDiscard, MapFlags.None);

            //Copy data to the GPU using marshal.
            int dataSize = bitmapData.Stride * bitmap.Height;
            byte[] data = new byte[dataSize];
            Marshal.Copy(bitmapData.Scan0, data, 0, dataSize);
            Marshal.Copy(data, 0, dataBox.DataPointer, dataSize);
            context.UnmapSubresource(texture, 0);

            bitmap.UnlockBits(bitmapData);
        }

        /// <summary>
        /// Creates a staged texture to allow the cpu to read the result texture.
        /// </summary>
        public static void CopyToCPU(Device device, Texture2D gpuSrcTexture, out Texture2D cpuDestinationTexture)
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

        #endregion


        /// <summary>
        /// Moves a texture on the gpu to a bitmap.
        /// </summary>
        public static void CopyToBitmap(Device device, Texture2D gpuSrcTexture, out Bitmap bitmapDstTexture)
        {
            Texture2D cpuTex;
            CopyToCPU(device, gpuSrcTexture, out cpuTex);

            PixelFormat format = PixelFormat.Format32bppArgb;

            bitmapDstTexture = new Bitmap(cpuTex.Description.Width, cpuTex.Description.Height, format);

            BitmapData bmpData = bitmapDstTexture.LockBits(
                                    new Rectangle(0, 0, bitmapDstTexture.Width, bitmapDstTexture.Height),
                                    ImageLockMode.WriteOnly,
                                    format);


            DataBox dataBox = device.ImmediateContext.MapSubresource(cpuTex, 0, MapMode.Read, MapFlags.None);

            // Copy the data from the CPU texture to the bitmap
            IntPtr srcPtr = dataBox.DataPointer;
            IntPtr dstPtr = bmpData.Scan0;

            for (int y = 0; y < cpuTex.Description.Height; y++)
            {
                Utilities.CopyMemory(dstPtr, srcPtr, cpuTex.Description.Width * 4); // 4 bytes per pixel (32bpp)
                srcPtr = IntPtr.Add(srcPtr, dataBox.RowPitch); // Advance the source pointer by the row pitch
                dstPtr = IntPtr.Add(dstPtr, bmpData.Stride);   // Advance the destination pointer by the stride
            }

            device.ImmediateContext.UnmapSubresource(cpuTex, 0);

            bitmapDstTexture.UnlockBits(bmpData);

            cpuTex.Dispose();
        }
    }
}
