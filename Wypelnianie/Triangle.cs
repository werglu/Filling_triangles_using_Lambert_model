using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wypelnianie
{
    public class Triangle
    {
        public double KAS { get; set; }
        public double KAD { get; set; }
        public Point a { get; set; }
        public Point b { get; set; }
        public Point c { get; set; }
        public List<Edge> edges = new List<Edge>();

        public void SetEdges(Random random)
        {
            KAS = random.NextDouble();
            KAD = random.NextDouble();
            Edge e1 = new Edge(b, a);
            Edge e2 = new Edge(b, c);
            Edge e3 = new Edge(c, a);

            if (a.Y < b.Y)
            {
                e1 = new Edge(a, b);
            }
            if (c.Y < b.Y)
            {
                e2 = new Edge(c, b);
            }
            if (a.Y < c.Y)
            {
                e3 = new Edge(a, c);
            }
            edges.Add(e1);
            edges.Add(e2);
            edges.Add(e3);
        }
    }

    public class Edge
    {
        public Point start { get; set; }
        public Point end { get; set; }
        public double x = -1;

        public Edge(Point s, Point e)
        {
            start = s;
            end = e;
        }

    }
}
