﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenCvSharp.Blob
{
    /// <summary>
    /// Polygon based contour.
    /// </summary>
    public class CvContourPolygon : List<CvPoint>
    {
        /// <summary>
        /// 
        /// </summary>
        public CvContourPolygon()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        public CvContourPolygon(IEnumerable<CvPoint> list)
            : base(list)
        {
        }

        /// <summary>
        /// Converts this to CSV string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (CvPoint p in this)
            {
                sb.AppendFormat("{0},{1}", p.X, p.Y).AppendLine();
            }
            return sb.ToString();
        }

        #region ContourConvexHull
        /// <summary>
        /// Calculates convex hull of a contour.
        /// Uses the Melkman Algorithm. Code based on the version in http://w3.impa.br/~rdcastan/Cgeometry/.
        /// </summary>
        /// <param name="p">Contour (polygon type).</param>
        /// <returns>Convex hull.</returns>
        public CvContourPolygon ContourConvexHull()
        {
            if (Count <= 3)
                return new CvContourPolygon(this);

            List<CvPoint> dq = new List<CvPoint>(); // instead of std::deq...

            if (CrossProductPoints(this[0], this[1], this[2]) > 0)
            {
                dq.Add(this[0]);
                dq.Add(this[1]);
            }
            else
            {
                dq.Add(this[1]);
                dq.Add(this[0]);
            }

            dq.Add(this[2]);
            dq.Insert(0, this[2]);

            for (int i = 3; i < Count; i++)
            {
                int s = dq.Count;

                if ((CrossProductPoints(this[i], dq[0], dq[1]) >= 0) &&
                    (CrossProductPoints(dq[s - 2], dq[s - 1], this[i]) >= 0))
                    continue; // TODO Optimize.

                while (CrossProductPoints(dq[s - 2], dq[s - 1], this[i]) < 0)
                {
                    dq.RemoveAt(dq.Count - 1);
                    s = dq.Count;
                }

                dq.Add(this[i]);

                while (CrossProductPoints(this[i], dq[0], dq[1]) < 0)
                    dq.RemoveAt(0);

                dq.Insert(0, this[i]);
            }

            return new CvContourPolygon(dq);
        }

        private static double CrossProductPoints(CvPoint a, CvPoint b, CvPoint c)
        {
            double abx = b.X - a.X;
            double aby = b.Y - a.Y;
            double acx = c.X - a.X;
            double acy = c.Y - a.Y;
            return abx * acy - aby * acx;
        }

        #endregion

        #region WriteAsCsv
        /// <summary>
        /// Write a contour to a CSV (Comma-separated values) file.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void WriteAsCsv(string fileName)
        {
            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.Write(ToString());
            }
        }
        #endregion

        #region WriteAsSvg
        /// <summary>
        /// Write a contour to a SVG file.
        /// </summary>
        /// <param name="fileName">File name</param>
        public void WriteAsSvg(string fileName)
        {
            WriteAsSvg(fileName, CvColor.Black, CvColor.White);
        }
        /// <summary>
        /// Write a contour to a SVG file.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="stroke">Stroke color</param>
        /// <param name="fill">Fill color</param>
        public void WriteAsSvg(string fileName, CvScalar stroke, CvScalar fill)
        {
            int minx = Int32.MaxValue;
            int miny = Int32.MaxValue;
            int maxx = Int32.MinValue;
            int maxy = Int32.MinValue;

            StringBuilder buffer = new StringBuilder();
            foreach (CvPoint p in this)
            {
                if (p.X > maxx)
                    maxx = p.X;
                if (p.X < minx)
                    minx = p.X;
                if (p.Y > maxy)
                    maxy = p.Y;
                if (p.Y < miny)
                    miny = p.Y;
                buffer.AppendFormat("{0},{1} ", p.X, p.Y);
            }

            using (StreamWriter writer = new StreamWriter(fileName, false))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"no\"?>");
                writer.WriteLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 20010904//EN\" \"http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd\">" );
                writer.WriteLine("<svg xmlns=\"http://www.w3.org/2000/svg\" xmlns:xlink=\"http://www.w3.org/1999/xlink\" xml:space=\"preserve\" "+
                    "width=\"{0}px\" height=\"{1}px\" viewBox=\"{2} {3} {4} {5}\" zoomAndPan=\"disable\" >",
                    maxx - minx, maxy - miny, minx, miny, maxx, maxy);
                writer.WriteLine("<polygon fill=\"rgb({0},{1},{2})\" stroke=\"rgb({3},{4},{5})\" stroke-width=\"1\" points=\"{6}\"/>",
                    fill.Val0, fill.Val1, fill.Val2, stroke.Val0, stroke.Val1, stroke.Val2, buffer.ToString());
                writer.WriteLine("</svg>");
            }
        }
        #endregion
    }
}