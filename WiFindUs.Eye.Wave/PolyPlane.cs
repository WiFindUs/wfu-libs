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
using WaveEngine.Framework.Managers;
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
		private readonly Vector3 xStep, zStep, firstVertex;
		private VertexPositionNormalColorTexture[] vertices;
		private Mesh mesh;
		private ushort[] indices;
		private DynamicVertexBuffer vertexBuffer;
		private DynamicIndexBuffer indexBuffer;
		private BoundingBox boundingBox;
		private readonly FaceBoundingBox[] faceBoundingBoxes;
		private const uint FBB_SIZE = 8;

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
			uint faceCols = Subdivisions + 1;

			//work out extremes
			firstVertex = -side1 - side2;
			xStep = ((side1 - side2) - firstVertex) / (float)faceCols;
			zStep = ((-side1 + side2) - firstVertex) / (float)faceCols;

			//divisions
			vertexCount = (Subdivisions + 2) * (Subdivisions + 2);
			triangleCount = faceCols * faceCols * 2;
			indexCount = triangleCount * 3;

			//bounding boxes
			BoundingBox = boundingBox = new BoundingBox();
			if (Subdivisions >= FBB_SIZE * 2 - 1)
			{
				uint bbCols = (faceCols / FBB_SIZE) + (faceCols % FBB_SIZE == 0 ? 0u : 1u);
				faceBoundingBoxes = new FaceBoundingBox[bbCols * bbCols];
				uint index = 0;
				for (uint row = 0; row < faceCols; row += FBB_SIZE)
				{
					for (uint column = 0; column < faceCols; column += FBB_SIZE)
					{
						faceBoundingBoxes[index] = new FaceBoundingBox(
							row, column,
							Math.Min(FBB_SIZE, faceCols - column),
							Math.Min(FBB_SIZE, faceCols - row));
						index++;
					}
				}
			}
			else
				faceBoundingBoxes = null;
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

		public PolyPlane RecalculateVertexNormal(uint row, uint column)
		{
			row = row % (Subdivisions + 2);
			column = column % (Subdivisions + 2);

			Vector3 normal = Vector3.Zero;

			//three faces to the left
			if (column > 0)
			{

				//two faces to the bottom-left
				if (row > 0)
				{
					normal += CalculateFaceNormal(row - 1, column - 1, true);
					normal += CalculateFaceNormal(row - 1, column - 1, false);
				}

				//one face above
				if (row < (Subdivisions+1))
					normal += CalculateFaceNormal(row, column - 1, false);

			}

			//three faces to the right
			if (column < (Subdivisions+1))
			{
				//one face to the bottom-right
				if (row > 0)
					normal += CalculateFaceNormal(row - 1, column, false);

				//two faces to the top-right
				if (row < (Subdivisions + 1))
				{
					normal += CalculateFaceNormal(row, column, true);
					normal += CalculateFaceNormal(row, column, false);
				}
			}

			normal.Normalize();
			vertices[(Subdivisions + 2) * row + column].Normal
				= normal;

			return this;
		}

		public float? Intersects(ref Ray ray, out Vector3 normal)
		{
			if (ray == null)
			{
				normal = Vector3.Zero;
				return null;
			}

			//check outer bounding box
			float? result;
			boundingBox.Intersects(ref ray, out result);
			if (!result.HasValue)
			{
				normal = Vector3.Zero;
				return null;
			}

			//get local ray
			Matrix w2l = Transform3D.WorldToLocalTransform;
			Matrix l2w = Transform3D.WorldTransform;
			Vector3 rayStart, rayDir;
			Vector3.Transform(ref ray.Direction, ref w2l, out rayDir);
			Vector3.Transform(ref ray.Position, ref w2l, out rayStart);

			if (faceBoundingBoxes != null)
			{
				for (uint i = 0; i < faceBoundingBoxes.Length; i++)
				{
					faceBoundingBoxes[i].BoundingBox.Intersects(ref ray, out result);
					if (!result.HasValue)
						continue;

					result = IntersectsFacesLocal(ref rayStart, ref rayDir,
						faceBoundingBoxes[i].Row, faceBoundingBoxes[i].Column,
						faceBoundingBoxes[i].Width, faceBoundingBoxes[i].Height,
						out normal);
					if (result.HasValue)
					{
						Vector3.Transform(ref normal, ref l2w, out normal);
						return result;
					}
				}
			}
			else
			{
				result = IntersectsFacesLocal(ref rayStart, ref rayDir, 0, 0, (Subdivisions + 1), (Subdivisions + 1), out normal);
				if (result.HasValue)
				{
					Vector3.Transform(ref normal, ref l2w, out normal);
					return result;
				}
			}

			normal = Vector3.Zero;
			return null;
		}

		public void UpdateBoundingBox()
		{
			Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
			for (int i = 0; i < vertices.Length; i++)
			{
				Vector3.Min(ref min, ref vertices[i].Position, out min);
				Vector3.Max(ref max, ref vertices[i].Position, out max);
			}
			Matrix l2w = Transform3D.WorldTransform;
			Vector3.Transform(ref min, ref l2w, out min);
			Vector3.Transform(ref max, ref l2w, out max);
			boundingBox.Min = min;
			boundingBox.Max = max;
			BoundingBox = boundingBox;

			if (faceBoundingBoxes != null)
			{
				for (int i = 0; i < faceBoundingBoxes.Length; i++)
					faceBoundingBoxes[i].Update(Subdivisions, ref l2w, ref vertices);
			}

			BoundingBoxRefreshed = true;
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
			UpdateBoundingBox();
			
			//indices
			indices = new ushort[indexCount];
			idx = 0;
			for (uint row = 0; row < (Subdivisions + 1); row++)
			{
				uint rowLowStart = row * (Subdivisions + 2); //first index of bottom line
				uint rowHighStart = (row + 1) * (Subdivisions + 2); //first index of top line

				//create triangles from left to right
				//uses anticlockwise winding order for both
				for (uint column = 0; column < (Subdivisions + 1); column++)
				{
					//triangle 0 (top triangle)
					indices[idx++] = (ushort)(rowLowStart + column); //bottom-left
					indices[idx++] = (ushort)(rowHighStart + column + 1); //top-right
					indices[idx++] = (ushort)(rowHighStart + column); //top-left

					//triangle 1 (bottom triangle)
					indices[idx++] = (ushort)(rowLowStart + column); //bottom-left
					indices[idx++] = (ushort)(rowLowStart + column + 1); //bottom-right
					indices[idx++] = (ushort)(rowHighStart + column + 1); //top right
					
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

		/////////////////////////////////////////////////////////////////////
		// PRIVATE METHODS
		/////////////////////////////////////////////////////////////////////

		internal void DrawDebugLines()
		{
			Matrix l2w = Transform3D.WorldTransform;
			Color color = Color.Red;
			float length = (Size / (float)(Subdivisions + 1)) * 0.5f;
			uint index = 0;
			for (uint row = 0; row < (Subdivisions + 2); row++)
			{
				for (uint column = 0; column < (Subdivisions + 2); column++)
				{
					Vector3 start = vertices[index].Position;
					Vector3 end = vertices[index].Position + (vertices[index].Normal * length);
					Vector3.Transform(ref start, ref l2w, out start);
					Vector3.Transform(ref end, ref l2w, out end);
					RenderManager.LineBatch3D.DrawLine(ref start, ref end, ref color);
					index++;
				}
			}

			color = Color.Cyan;
			RenderManager.LineBatch3D.DrawBoundingBox(ref boundingBox, ref color);


			if (faceBoundingBoxes != null)
			{
				color = Color.Magenta;
				for (int i = 0; i < faceBoundingBoxes.Length; i++)
					RenderManager.LineBatch3D.DrawBoundingBox(ref faceBoundingBoxes[i].BoundingBox, ref color);
			}
		}

		private void GetFaceVertices(uint row, uint column, bool top, out uint v0, out uint v1, out uint v2)
		{
			row = row % (Subdivisions + 1); //wraps at length-1 because faces, not vertices
			column = column % (Subdivisions + 1);
			
			uint bl = row * (Subdivisions + 2) + column;
			uint tl = bl + (Subdivisions + 2);

			v0 = bl;
			v1 = (top ? tl : bl) + 1;
			v2 = top ? tl : tl + 1;
		}

		private Vector3 CalculateFaceNormal(uint row, uint column, bool top)
		{
			uint v0, v1, v2;
			GetFaceVertices(row, column, top, out v0, out v1, out v2);

			return CalculateFaceNormal(
				ref vertices[v0].Position,
				ref vertices[v1].Position,
				ref vertices[v2].Position
			);
		}

		private Vector3 CalculateFaceNormal(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2)
		{
			Vector3 a = v0 - v1;
			a.Normalize();
			Vector3 b = v0 - v2;
			b.Normalize();
			Vector3.Cross(ref a, ref b, out b);
			b.Normalize();
			return b;
		}

		private float? IntersectsFacesLocal(ref Vector3 rayStart, ref Vector3 rayDir, uint row, uint col, uint width, uint height, out Vector3 normal)
		{
			float? result;
			for (uint r = row; r < row + height; r++)
			{
				for (uint c = col; c < col + width; c++)
				{
					//check top triangle
					uint v0, v1, v2;
					GetFaceVertices(r, c, true, out v0, out v1, out v2);
					result = IntersectsFaceLocal(ref rayStart, ref rayDir,
						ref vertices[v0].Position,
						ref vertices[v1].Position,
						ref vertices[v2].Position,
						out normal);
					if (result.HasValue)
						return result;

					//check bottom triangle
					GetFaceVertices(r, c, false, out v0, out v1, out v2);
					result = IntersectsFaceLocal(ref rayStart, ref rayDir,
						ref vertices[v0].Position,
						ref vertices[v1].Position,
						ref vertices[v2].Position,
						out normal);
					if (result.HasValue)
						return result;
				}
			}
			normal = Vector3.Zero;
			return null;
		}

		//adapted from http://geomalgorithms.com/a06-_intersect-2.html
		private float? IntersectsFaceLocal(ref Vector3 rayStart, ref Vector3 rayDir,
			ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, out Vector3 normal)
		{
			// get triangle edge vectors and plane normal
			Vector3 u = v1 - v0;
			Vector3 v = v2 - v0;
			Vector3.Cross(ref u, ref v, out normal);

			Vector3 w0 = rayStart - v0;
			float a = -Vector3.Dot(ref normal, ref w0);
			float b = Vector3.Dot(ref normal, ref rayDir);

			// ray is  parallel to triangle plane
			if (Math.Abs(b) < 0.00000001f)
				return null;

			// get intersect point of ray with triangle plane
			float r = a / b;
			if (r < 0.0f)
				return null;

			// intersect point of ray and plane
			Vector3 I = rayStart + r * rayDir;

			// is I inside T?
			float uu, uv, vv, wu, wv, D;
			uu = Vector3.Dot(ref u, ref u);
			uv = Vector3.Dot(ref u, ref v);
			vv = Vector3.Dot(ref v, ref v);
			Vector3 w = I - v0;
			wu = Vector3.Dot(ref w, ref u);
			wv = Vector3.Dot(ref w, ref v);
			D = uv * uv - uu * vv;

			// get and test parametric coords
			float s, t;
			s = (uv * wv - vv * wu) / D;
			if (s < 0.0f || s > 1.0f)         // I is outside T
				return null;
			t = (uv * wu - uu * wv) / D;
			if (t < 0.0f || (s + t) > 1.0f)  // I is outside T
				return null;

			float result;
			Vector3.Distance(ref rayStart, ref I, out result);
			normal.Normalize();
			return result;
		}

		private class FaceBoundingBox
		{
			public readonly uint Row;
			public readonly uint Column;
			public readonly uint Width;
			public readonly uint Height;
			public BoundingBox BoundingBox;

			public FaceBoundingBox(uint r, uint c, uint w, uint h)
			{
				Row = r;
				Column = c;
				Width = w;
				Height = h;
				BoundingBox = new BoundingBox();
			}

			public void Update(uint subdivs, ref Matrix LocalToWorld, ref VertexPositionNormalColorTexture[] verts)
			{
				Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				for (uint row = Row; row <= (Row + Height); row++ )
				{
					for (uint column = Column; column <= (Column + Width); column++)
					{
						uint index = (subdivs + 2) * row + column;
						Vector3.Min(ref min, ref verts[index].Position, out min);
						Vector3.Max(ref max, ref verts[index].Position, out max);
					}
				}
				Vector3.Transform(ref min, ref LocalToWorld, out min);
				Vector3.Transform(ref max, ref LocalToWorld, out max);
				BoundingBox.Min = min;
				BoundingBox.Max = max;
			}
		}
	}
}
