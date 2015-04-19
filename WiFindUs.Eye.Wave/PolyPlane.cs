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
		public event Action<PolyPlane> BoundingBoxUpdated;
		
		[RequiredComponent]
		public Transform3D Transform3D;
		[RequiredComponent]
		public MaterialsMap Material;
		public readonly float Size;
		public readonly uint Subdivisions;
		public Color DebugNormalColor = Color.Red;
		public Color DebugSegmentColor = Color.Magenta;
		public Color DebugBoxColor = Color.Cyan;
		public readonly bool Clockwise;

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
		private readonly Segment[] segments;
		private const uint SEGMENT_SIZE = 8;
		

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

		public PolyPlane(Vector3? normal = null, float size = 1.0f, uint subdivs = 0, bool cw = true)
		{
			//orientation
			Clockwise = cw;
			Size = Math.Max(1.0f, size);
			if (normal.HasValue)
			{ 
				normal.Value.Normalize();
				Normal = normal.Value;
			}
			else
				Normal = Vector3.Up;
			side1 = new Vector3(Normal.Y, Normal.Z, Normal.X) * (Size / 2.0f);
			side2 = Vector3.Cross(Normal, side1);
			Subdivisions = Math.Min(subdivs, 2046);
			uint faceCols = Subdivisions + 1;

			//work out extremes
			firstVertex = -side1 - side2;
			xStep = ((side1 - side2) - firstVertex) / (float)faceCols;
			zStep = ((-side1 + side2) - firstVertex) / (float)faceCols;

			//divisions
			vertexCount = (Subdivisions + 2) * (Subdivisions + 2);
			triangleCount = faceCols * faceCols * 2;
			indexCount = triangleCount * 3;

			//segment boxes for broad-phase intersection testing
			BoundingBox = boundingBox = new BoundingBox();
			if (Subdivisions >= SEGMENT_SIZE * 2 - 1)
			{
				uint bbCols = (faceCols / SEGMENT_SIZE) + (faceCols % SEGMENT_SIZE == 0 ? 0u : 1u);
				segments = new Segment[bbCols * bbCols];
				uint index = 0;
				for (uint row = 0; row < faceCols; row += SEGMENT_SIZE)
				{
					for (uint column = 0; column < faceCols; column += SEGMENT_SIZE)
					{
						segments[index] = new Segment(
							row, column,
							Math.Min(SEGMENT_SIZE, faceCols - column),
							Math.Min(SEGMENT_SIZE, faceCols - row));
						index++;
					}
				}
			}
			else
				segments = null;
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

			//check outer bounding box (transform to world)
			Matrix l2w = Transform3D.WorldTransform;
			float? result;
			boundingBox.Transform(ref l2w).Intersects(ref ray, out result);
			if (!result.HasValue)
			{
				normal = Vector3.Zero;
				return null;
			}

			//get local ray
			Matrix w2l = Transform3D.WorldToLocalTransform;
			Vector3 rayStart, rayDir;
			Vector3.Transform(ref ray.Direction, ref w2l, out rayDir);
			Vector3.Transform(ref ray.Position, ref w2l, out rayStart);

			//if we're segmented, check only triangles within intersected segment
			if (segments != null)
			{
				for (uint i = 0; i < segments.Length; i++)
				{
					segments[i].BoundingBox
						.Transform(ref l2w).Intersects(ref ray, out result);
					if (!result.HasValue)
						continue;

					result = IntersectsFacesLocal(ref rayStart, ref rayDir,
						segments[i].Row, segments[i].Column,
						segments[i].Width, segments[i].Height,
						out normal);
					if (result.HasValue)
					{
						Vector3.Transform(ref normal, ref l2w, out normal);
						return result;
					}
				}
			}
			else //otherwise just brute-force it
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
			boundingBox.Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			boundingBox.Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

			//if we're segmented, do segments first and use those for faster updating of the main bounding box
			if (segments != null)
			{
				for (int i = 0; i < segments.Length; i++)
				{
					segments[i].Update(Subdivisions, ref vertices);
					Vector3.Min(ref boundingBox.Min, ref segments[i].BoundingBox.Min, out boundingBox.Min);
					Vector3.Max(ref boundingBox.Max, ref segments[i].BoundingBox.Max, out boundingBox.Max);
				}
			}
			else //otherwise just brute-force it
			{
				for (int i = 0; i < vertices.Length; i++)
				{
					Vector3.Min(ref boundingBox.Min, ref vertices[i].Position, out boundingBox.Min);
					Vector3.Max(ref boundingBox.Max, ref vertices[i].Position, out boundingBox.Max);
				}
			}
			BoundingBox = boundingBox;
			BoundingBoxRefreshed = true;

			if (BoundingBoxUpdated != null)
				BoundingBoxUpdated(this);
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
				for (uint column = 0; column < (Subdivisions + 1); column++)
				{
					//triangle 0 (top triangle)
					indices[idx++] = (ushort)(rowLowStart + column); //bottom-left
					indices[idx++] = (ushort)(rowHighStart + column + (Clockwise ? 0 : 1));
					indices[idx++] = (ushort)(rowHighStart + column + (Clockwise ? 1 : 0));

					//triangle 1 (bottom triangle)
					indices[idx++] = (ushort)(rowLowStart + column); //bottom-left
					indices[idx++] = (ushort)((Clockwise ? rowHighStart : rowLowStart) + column + 1);
					indices[idx++] = (ushort)((Clockwise ? rowLowStart : rowHighStart) + column + 1);
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
					RenderManager.LineBatch3D.DrawLine(ref start, ref end, ref DebugNormalColor);
					index++;
				}
			}

			BoundingBox bb = boundingBox.Transform(ref l2w);
			RenderManager.LineBatch3D.DrawBoundingBox(ref bb, ref DebugBoxColor);

			if (segments != null)
			{
				for (int i = 0; i < segments.Length; i++)
				{
					bb = segments[i].BoundingBox.Transform(ref l2w);
					RenderManager.LineBatch3D.DrawBoundingBox(ref bb, ref DebugSegmentColor);
				}
			}
		}

		private void GetFaceVertices(uint row, uint column, bool top, out uint v0, out uint v1, out uint v2)
		{
			row = row % (Subdivisions + 1); //wraps at length-1 because faces, not vertices
			column = column % (Subdivisions + 1);
			
			uint bl = row * (Subdivisions + 2) + column;
			uint tl = bl + (Subdivisions + 2);
			uint tr = tl + 1;
			uint br = bl + 1;

			v0 = bl;
			if (top)
			{
				v1 = Clockwise ? tl : tr;
				v2 = Clockwise ? tr : tl;
			}
			else
			{
				v1 = Clockwise ? tr : br;
				v2 = Clockwise ? br : tr;
			}
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
			Vector3 a = v0 - (Clockwise ? v2 : v1);
			a.Normalize();
			Vector3 b = v0 - (Clockwise ? v1 : v2);
			b.Normalize();
			Vector3.Cross(ref a, ref b, out b);
			b.Normalize();
			return b;
		}

		private float? IntersectsFacesLocal(ref Vector3 rayStart, ref Vector3 rayDir,
			uint row, uint col, uint width, uint height, out Vector3 normal)
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
		private static float? IntersectsFaceLocal(ref Vector3 rayStart, ref Vector3 rayDir,
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

		private class Segment
		{
			public readonly uint Row;
			public readonly uint Column;
			public readonly uint Width;
			public readonly uint Height;
			public BoundingBox BoundingBox;

			public Segment(uint r, uint c, uint w, uint h)
			{
				Row = r;
				Column = c;
				Width = w;
				Height = h;
				BoundingBox = new BoundingBox();
			}

			public void Update(uint subdivs, ref VertexPositionNormalColorTexture[] verts)
			{
				BoundingBox.Min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
				BoundingBox.Max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
				for (uint row = Row; row <= (Row + Height); row++ )
				{
					for (uint column = Column; column <= (Column + Width); column++)
					{
						uint index = (subdivs + 2) * row + column;
						Vector3.Min(ref BoundingBox.Min, ref verts[index].Position, out BoundingBox.Min);
						Vector3.Max(ref BoundingBox.Max, ref verts[index].Position, out BoundingBox.Max);
					}
				}
			}
		}
	}
}
