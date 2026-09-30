using System;
using System.Collections.Generic;

namespace TeklaAutoGrating
{
    /// <summary>Axis-aligned rectangle in the plan (XY) plane, model units (mm).</summary>
    public struct Rect
    {
        public double MinX, MinY, MaxX, MaxY;
        public Rect(double minX, double minY, double maxX, double maxY)
        {
            MinX = minX; MinY = minY; MaxX = maxX; MaxY = maxY;
        }
        public double Width { get { return MaxX - MinX; } }
        public double Height { get { return MaxY - MinY; } }
        public bool Intersects(Rect o)
        {
            return MinX < o.MaxX && MaxX > o.MinX && MinY < o.MaxY && MaxY > o.MinY;
        }
        public Rect Intersection(Rect o)
        {
            return new Rect(Math.Max(MinX, o.MinX), Math.Max(MinY, o.MinY),
                            Math.Min(MaxX, o.MaxX), Math.Min(MaxY, o.MaxY));
        }
    }

    public class GratingPanel
    {
        public Rect Bounds;
        /// <summary>Openings (already clipped to the panel) to be cut out.</summary>
        public List<Rect> Openings = new List<Rect>();
    }

    /// <summary>Tekla-independent layout math: panel splitting and opening sizing.</summary>
    public static class GratingLayout
    {
        /// <summary>Shrink the picked area by the edge clearance, keeping it valid.</summary>
        public static Rect FitToSupport(Rect area, double edgeClearance)
        {
            return new Rect(area.MinX + edgeClearance, area.MinY + edgeClearance,
                            area.MaxX - edgeClearance, area.MaxY - edgeClearance);
        }

        /// <summary>
        /// Split a rectangle into equal panels no larger than maxWidth (X, across the
        /// bars) and maxLength (Y, along the span), leaving a gap between panels.
        /// Equal splitting avoids slivers that a "fill with max size" approach leaves.
        /// </summary>
        public static List<Rect> SplitPanels(Rect area, double maxWidth, double maxLength, double gap)
        {
            var result = new List<Rect>();
            if (area.Width <= 0 || area.Height <= 0) return result;
            int nx = Math.Max(1, (int)Math.Ceiling((area.Width - gap) / (maxWidth + gap) - 1e-9));
            int ny = Math.Max(1, (int)Math.Ceiling((area.Height - gap) / (maxLength + gap) - 1e-9));
            double w = (area.Width - gap * (nx - 1)) / nx;
            double h = (area.Height - gap * (ny - 1)) / ny;
            for (int i = 0; i < nx; i++)
                for (int j = 0; j < ny; j++)
                {
                    double x = area.MinX + i * (w + gap);
                    double y = area.MinY + j * (h + gap);
                    result.Add(new Rect(x, y, x + w, y + h));
                }
            return result;
        }

        /// <summary>
        /// Auto-size an opening around an obstruction: grow by clearance, then round the
        /// size up to a multiple of roundStep (0 = no rounding), keeping it centred.
        /// </summary>
        public static Rect SizeOpening(Rect obstruction, double clearance, double roundStep)
        {
            double cx = (obstruction.MinX + obstruction.MaxX) / 2;
            double cy = (obstruction.MinY + obstruction.MaxY) / 2;
            double w = RoundUp(obstruction.Width + 2 * clearance, roundStep);
            double h = RoundUp(obstruction.Height + 2 * clearance, roundStep);
            return new Rect(cx - w / 2, cy - h / 2, cx + w / 2, cy + h / 2);
        }

        public static double RoundUp(double value, double step)
        {
            if (step <= 0) return value;
            return Math.Ceiling(value / step - 1e-9) * step;
        }

        /// <summary>Merge overlapping openings so cutters do not fight each other.</summary>
        public static List<Rect> MergeOverlapping(List<Rect> rects)
        {
            var list = new List<Rect>(rects);
            bool merged = true;
            while (merged)
            {
                merged = false;
                for (int i = 0; i < list.Count && !merged; i++)
                    for (int j = i + 1; j < list.Count && !merged; j++)
                        if (list[i].Intersects(list[j]))
                        {
                            var a = list[i]; var b = list[j];
                            list[i] = new Rect(Math.Min(a.MinX, b.MinX), Math.Min(a.MinY, b.MinY),
                                               Math.Max(a.MaxX, b.MaxX), Math.Max(a.MaxY, b.MaxY));
                            list.RemoveAt(j);
                            merged = true;
                        }
            }
            return list;
        }

        /// <summary>Assign each (merged) opening to every panel it overlaps, clipped to the panel.</summary>
        public static List<GratingPanel> BuildPanels(Rect area, double maxWidth, double maxLength,
            double gap, List<Rect> openings)
        {
            var merged = MergeOverlapping(openings);
            var panels = new List<GratingPanel>();
            foreach (var r in SplitPanels(area, maxWidth, maxLength, gap))
            {
                var p = new GratingPanel { Bounds = r };
                foreach (var o in merged)
                    if (o.Intersects(r)) p.Openings.Add(o.Intersection(r));
                panels.Add(p);
            }
            return panels;
        }
    }
}
