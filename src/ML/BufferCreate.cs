using SharpDX.Direct3D11;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

using SharpDX.DXGI;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Contexts;


namespace Clickless
{

    //https://stackoverflow.com/questions/18167216/size-of-generic-structure
    public static class TypeSize<T>
    {
        public readonly static int Size;

        static TypeSize()
        {
            var dm = new DynamicMethod("SizeOfType", typeof(int), new Type[] { });
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Sizeof, typeof(T));
            il.Emit(OpCodes.Ret);
            Size = (int)dm.Invoke(null, null);
        }
    }



    public class BufferCreate
    {
        private Device Device;
        private Buffer Buffer;

        public class BufferCreation
        {
            private Device _device;
            private Buffer _buffer;
            public BufferCreation(Device device, Buffer buffer)
            {
                _device = device;
                _buffer = buffer;
            }

            public Buffer GetBuffer()
            {
                return _buffer;
            }

            public BufferCreation SetConstantBufferData<T>(T data, DeviceContext context) where T : struct
            {
                context.ComputeShader.SetConstantBuffer(0, _buffer);
                context.UpdateSubresource(ref data, _buffer);
                return this;
            }

            public BufferCreation CopyFrom(Buffer buffer, DeviceContext context)
            {
                context.CopyResource(buffer, _buffer);
                return this;
            }
        }

        public static BufferCreation InitializeRWBuffer<T>(Device device, int count) 
        {
            var buffer =  new Buffer(device, new BufferDescription
            {
                SizeInBytes = TypeSize<T>.Size * count,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.UnorderedAccess,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = TypeSize<T>.Size,
            });

            return new BufferCreation(device, buffer);
        }

        public static BufferCreation InitializeConstantBuffer<T>(Device device)
        {
            var buffer = new Buffer(device, new BufferDescription
            {
                SizeInBytes = TypeSize<T>.Size,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.ConstantBuffer,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None
            });

            return new BufferCreation(device, buffer);
        }

        public static BufferCreation InitializeStagingBuffer<T>(Device device, int size)
        {
            var buffer = new Buffer(device, new BufferDescription
            {
                SizeInBytes = TypeSize<T>.Size * size,
                Usage = ResourceUsage.Staging,
                BindFlags = BindFlags.None,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.BufferStructured,
                StructureByteStride = TypeSize<T>.Size,
            });

            return new BufferCreation(device, buffer);
        }

        public static Buffer ResizeBuffer<T>(Device device, DeviceContext context, Buffer oldBuffer, int newSize) where T : struct
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

    }
}
