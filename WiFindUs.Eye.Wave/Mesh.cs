using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye.Wave
{
    public abstract class Mesh
    {
        /*
        private bool disposed = false;
        private uint[] buffers;
        private float[] vertices;
        private uint[] indices;

        private const uint BUFFER_COUNT = 2;
        private const uint VERTEX_BUFFER = 0;
        private const uint INDICES_BUFFER = 1;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public bool IsDisposed
        {
            get { return disposed; }
        }

        public abstract long TriangleCount { get; }

        public abstract long VertexCount { get; }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////


        /////////////////////////////////////////////////////////////////////
        // PUBLIC METHODS
        /////////////////////////////////////////////////////////////////////

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Draw()
        {
            if (IsDisposed)
                return;

            //todo: normals
            //GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.NormalBufferID);
            //GL.NormalPointer(NormalPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);
            //GL.EnableClientState(EnableCap.NormalArray);

            //todo: texture2d
            //GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.TexCoordBufferID);
            //GL.TexCoordPointer(2, TexCoordPointerType.Float, 8, IntPtr.Zero);
            //GL.EnableClientState(EnableCap.TextureCoordArray);

            //vertex buffer
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[VERTEX_BUFFER]);
            GL.VertexPointer(3, VertexPointerType.Float, Vector3.SizeInBytes, IntPtr.Zero);
            GL.EnableClientState(ArrayCap.VertexArray);

            //element indices buffer
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers[INDICES_BUFFER]);

            //draw
            GL.DrawElements(PrimitiveType.Triangles, (int)TriangleCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected void Flush(bool recreateBuffers = false)
        {
            if (recreateBuffers)
                RecreateBuffers();
            UpdateVertices();
            UpdateTriangles();
            FlushBuffer(VERTEX_BUFFER);
            FlushBuffer(INDICES_BUFFER);
        }

        protected void RecreateBuffers()
        {
            //recreate buffers
            if (buffers != null)
                GL.DeleteBuffers(buffers.Length, buffers);
            else
                buffers = new uint[BUFFER_COUNT];
            GL.GenBuffers(buffers.Length, buffers);

            //init arrays
            vertices = new float[VertexCount];
            indices = new uint[TriangleCount * 3];
        }

        protected abstract void AssignVertex(long index, ref float x, ref float y, ref float z);

        protected abstract void AssignTriangle(long index, ref uint point1index, ref uint point2index, ref uint point3index);

        protected void UpdateVertices()
        {
            for (long i = 0L; i < vertices.LongLength; i++)
                AssignVertex(i, ref vertices[i], ref vertices[i + 1L], ref vertices[i + 2L]);
        }

        protected void UpdateTriangles()
        {
            for (long i = 0L; i < TriangleCount; i++)
            {
                long start = i * 3;
                AssignTriangle(i, ref indices[start], ref indices[start + 1L], ref indices[start + 2L]);
            }
        }

        protected void FlushBuffer(uint bufferIndex)
        {
            if (bufferIndex >= BUFFER_COUNT)
                return;
            
            switch (bufferIndex)
            {
                case VERTEX_BUFFER:
                    GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[VERTEX_BUFFER]);
                    GL.BufferData(BufferTarget.ArrayBuffer,
                        (IntPtr)(vertices.LongLength * sizeof(float)),
                        vertices,
                        BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                    break;

                case INDICES_BUFFER:
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, buffers[INDICES_BUFFER]);
                    GL.BufferData(BufferTarget.ElementArrayBuffer,
                        (IntPtr)(indices.LongLength * sizeof(uint)),
                        indices,
                        BufferUsageHint.StaticDraw);
                    GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
                    break;
            }

        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (buffers != null)
                    GL.DeleteBuffers(buffers.Length, buffers);
                buffers = null;
            }

            disposed = true;
        }
*/

        /////////////////////////////////////////////////////////////////////
        // PRIVATE METHODS
        /////////////////////////////////////////////////////////////////////
    }
}
