using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Graphics.VertexFormats;
using WaveEngine.Common.Math;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.Primitives;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.Graphics;
using WaveEngine.Materials;

namespace WiFindUs.Eye.Wave
{
	public class PolyPlane : BaseModel
	{
		[RequiredComponent]
		public Transform3D Transform3D;
		[RequiredComponent]
		public MaterialsMap Material;
		public readonly float Size;
		public readonly uint Subdivisions;

		private readonly uint vertexCount;
		private readonly uint triangleCount;
		private readonly uint indexCount;
		private readonly Vector3 side1, side2;
		private readonly Vector3 Normal;
		private Vector3 xStep, zStep, firstVertex;
		private VertexPositionNormalColorTexture[] vertices;
		private Mesh mesh;
		private ushort[] indices;
		private DynamicVertexBuffer vertexBuffer;
		private DynamicIndexBuffer indexBuffer;
		
		private static int instances;

		/////////////////////////////////////////////////////////////////////
		// PROPERTIES
		/////////////////////////////////////////////////////////////////////

		public override int MeshCount
		{
			get { return 1; }
		}

		public override bool HasCollisionInfo
		{
			get { return true; }
		}

		public Mesh Mesh
		{
			get { return mesh; }
		}

		/////////////////////////////////////////////////////////////////////
		// CONSTRUCTORS/INITIALIZERS
		/////////////////////////////////////////////////////////////////////

		public PolyPlane(Vector3? normal = null, float size = 1.0f, uint subdivs = 0)
		{
			//orientation
			Size = size;
			Normal = normal ?? Vector3.Up;
			side1 = new Vector3(Normal.Y, Normal.Z, Normal.X) * (Size / 2.0f);
			side2 = Vector3.Cross(Normal, side1);
			Subdivisions = subdivs;

			//work out extremes
			firstVertex = -side1 - side2;
			xStep = ((side1 - side2) - firstVertex) / (float)(Subdivisions + 1);
			zStep = ((-side1 + side2) - firstVertex) / (float)(Subdivisions + 1);

			//divisions
			vertexCount = (Subdivisions + 2) * (Subdivisions + 2);
			triangleCount = (Subdivisions + 1) * (Subdivisions + 1) * 2;
			indexCount = triangleCount * 3;
		}

		/////////////////////////////////////////////////////////////////////
		// PUBLIC METHODS
		/////////////////////////////////////////////////////////////////////

		public override Vector3[] GetVertices()
		{
			Vector3[] verts = new Vector3[vertices.Length];
			for (int i = 0; i < vertices.Length; i++)
				verts[i] = vertices[i].Position;
			return verts;
		}

		public override int[] GetIndices()
		{
			int[] idcs = new int[indices.Length];
			for (int i = 0; i < indices.Length; i++)
				idcs[i] = indices[i];
			return idcs;
		}

		public PolyPlane SetVertexPosition(uint row, uint column, float offsetAlongNormal = 0.0f)
		{
			row = row % (Subdivisions + 2);
			column = column % (Subdivisions + 2);
			vertices[(Subdivisions + 2) * row + column].Position
				= firstVertex + (xStep * column) + (zStep * row) + Normal * offsetAlongNormal;
			return this;
		}

		public PolyPlane FlushVertices()
		{
			vertexBuffer.SetData(vertices, (int)vertexCount);
			RenderManager.GraphicsDevice.Graphics.BindVertexBuffer(vertexBuffer);
			return this;
		}

		/////////////////////////////////////////////////////////////////////
		// PROTECTED METHODS
		/////////////////////////////////////////////////////////////////////

		protected override void Initialize()
		{
			base.Initialize();

			//vertices
			vertices = new VertexPositionNormalColorTexture[vertexCount];
			uint idx = 0;
			for (uint row = 0; row < (Subdivisions + 2); row++)
			{
				float rowPC = (float)row / (float)(Subdivisions + 1);
				for (uint column = 0; column < (Subdivisions + 2); column++)
				{
					float colPC = (float)column / (float)(Subdivisions + 1);
					vertices[idx++] = new VertexPositionNormalColorTexture(
						firstVertex + (xStep * column) + (zStep * row),
						Normal,
						Color.White,
						new Vector2(colPC, 1.0f - rowPC));
				}
			}
			vertexBuffer = new DynamicVertexBuffer(VertexPositionNormalColorTexture.VertexFormat);
			FlushVertices();
			

			//indices
			indices = new ushort[indexCount];
			idx = 0;
			for (uint row = 0; row < (Subdivisions + 1); row++)
			{
				uint rowLowStart = row * (Subdivisions + 2); //first index of top line
				uint rowHighStart = (row + 1) * (Subdivisions + 2); //first index of bottom line

				for (uint column = 0; column < (Subdivisions + 1); column++)
				{
					indices[idx++] = (ushort)(rowLowStart + column);
					indices[idx++] = (ushort)(rowHighStart + column + 1);
					indices[idx++] = (ushort)(rowLowStart + column + 1);

					indices[idx++] = (ushort)(rowLowStart + column);
					indices[idx++] = (ushort)(rowHighStart + column);
					indices[idx++] = (ushort)(rowHighStart + column + 1);
				}
			}
			indexBuffer = new DynamicIndexBuffer(indices);
			RenderManager.GraphicsDevice.Graphics.BindIndexBuffer(indexBuffer);

			//mesh
			mesh = new Mesh(0, (int)vertexCount, 0, (int)triangleCount, vertexBuffer, indexBuffer, PrimitiveType.TriangleList);
		}

		protected override void LoadModel()
		{

		}

		protected override void UnloadModel()
		{
			RenderManager.GraphicsDevice.Graphics.DestroyIndexBuffer(mesh.IndexBuffer);
			RenderManager.GraphicsDevice.Graphics.DestroyVertexBuffer(mesh.VertexBuffer);
			mesh = null;
			indexBuffer = null;
			vertexBuffer = null;
			indices = null;
			vertices = null;
		}
	}
}
