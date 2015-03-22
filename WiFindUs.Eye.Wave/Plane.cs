using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WiFindUs.Eye.Wave
{
    public class Plane : Mesh
    {
        /*
        private int subDivisions = -1;
        private float width, height, subdivWidth, subdivHeight;

        /////////////////////////////////////////////////////////////////////
        // PROPERTIES
        /////////////////////////////////////////////////////////////////////

        public float Width
        {
            get { return width; }
            set
            {
                width = value;
                subdivWidth = width / (subDivisions + 1);
                Flush();
            }
        }

        public float Height
        {
            get { return height; }
            set
            {
                height = value;
                subdivHeight = height / (subDivisions + 1);
                Flush();
            }
        }

        public int SubDivisions
        {
            get { return subDivisions; }
            set
            {
                int sd = value < 0 ? 0 : value;
                if (sd == subDivisions)
                    return;
                subDivisions = sd;
                subdivWidth = width / (subDivisions + 1);
                subdivHeight = height / (subDivisions + 1);
                Flush(true);
            }
        }

        public long SquareCount
        {
            get { return (SubDivisions + 1L) * (SubDivisions + 1L); }
        }

        public override long TriangleCount
        {
            get { return SquareCount * 2L; }
        }

        public override long VertexCount
        {
            get { return (SubDivisions + 2L) * (SubDivisions + 2L); }
        }

        public float SubDivisionWidth
        {
            get { return subdivWidth; }
        }

        public float SubDivisionHeight
        {
            get { return subdivHeight; }
        }

        /////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS
        /////////////////////////////////////////////////////////////////////

        public Plane(float width, float height, int subdivs = 0)
            : base()
        {
            this.width = width;
            this.height = height;
            SubDivisions = subdivs;
        }

        /////////////////////////////////////////////////////////////////////
        // PROTECTED METHODS
        /////////////////////////////////////////////////////////////////////

        protected override void AssignVertex(long index, ref float x, ref float y, ref float z)
        {
            int perRow = (SubDivisions + 2);
            int row = (int)index / perRow;
            int column = (int)index - (row * perRow);

            x = column * SubDivisionWidth;
            z = row * SubDivisionHeight;
        }

        protected override void AssignTriangle(long index, ref uint point1index, ref uint point2index, ref uint point3index)
        {
            int perRow = (SubDivisions + 1);
            int square = (int)index / 2;
            int row = square / perRow;
            int column = square - (row * perRow);

            int vertsPerRow = (SubDivisions + 2);
            point1index = (uint)(row * vertsPerRow + column);
            point3index = (uint)(point1index + vertsPerRow + 1);
            if (index % 2 == 0)
                point2index = point3index-1;
            else
                point2index = point1index+1;
        }
         * 
         * */
    }
        

}
