using Silk.NET.OpenGL;
using System;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace GLSample.Rendering
{
    public class GLInstancedMesh : IDisposable
    {
        public GLMesh Mesh { get; }
        public uint InstanceCount { get; }


        public uint InstanceBufferHandle { get; }

        private GL _gl;

        internal GLInstancedMesh(GL gl, GLMesh mesh, Vector4[] instances)
        {
            _gl = gl;
            Mesh = mesh;
            InstanceBufferHandle = gl.CreateBuffer();
            InstanceCount = (uint) instances.Length;
            gl.NamedBufferStorage<Vector4>(InstanceBufferHandle, instances, BufferStorageMask.None);
        }

        public void Dispose()
        {
            _gl.DeleteBuffer(InstanceBufferHandle);
        }

        public void Draw()
        {
            unsafe
            {
                _gl.BindBufferBase(BufferTargetARB.ShaderStorageBuffer, 4, InstanceBufferHandle);
                _gl.BindVertexArray(Mesh.VertexArrayHandle);
                _gl.DrawElementsInstanced(PrimitiveType.Triangles, Mesh.IndexCount, DrawElementsType.UnsignedInt, null, InstanceCount);
            }
        }
    }
}
