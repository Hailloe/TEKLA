using System;
using System.Collections;
using System.Collections.Generic;
using Tekla.Structures.Geometry3d;
using Tekla.Structures.Model;
using Tekla.Structures.Model.UI;
using Tekla.Structures.Plugins;

namespace TeklaAutoGrating
{
    [Plugin("AutoGrating")]
    [PluginUserInterface("TeklaAutoGrating.AutoGratingForm")]
    [InputObjectDependency(InputObjectDependency.NOT_DEPENDENT)]
    public class AutoGratingPlugin : PluginBase
    {
        private readonly Model _model;
        private readonly AutoGratingData _d;
        private Point _p1, _p2;

        public AutoGratingPlugin(AutoGratingData data)
        {
            _model = new Model();
            _d = data;
        }

        public override List<InputDefinition> DefineInput()
        {
            var picker = new Picker();
            var input = new List<InputDefinition>();
            // Pick two opposite corners of the area to be covered by grating.
            var a = picker.PickPoint("Pick first corner of grating area");
            var b = picker.PickPoint("Pick opposite corner");
            input.Add(new InputDefinition(a));
            input.Add(new InputDefinition(b));
            return input;
        }

        public override bool Run(List<InputDefinition> input)
        {
            try
            {
                Defaults();
                _p1 = (Point)input[0].GetInput();
                _p2 = (Point)input[1].GetInput();

                double z = _p1.Z; // top of supporting steel
                var area = GratingLayout.FitToSupport(
                    new Rect(Math.Min(_p1.X, _p2.X), Math.Min(_p1.Y, _p2.Y),
                             Math.Max(_p1.X, _p2.X), Math.Max(_p1.Y, _p2.Y)),
                    _d.EdgeClearance);
                if (area.Width <= 0 || area.Height <= 0) return false;

                var openings = _d.AutoOpening == 1 ? DetectOpenings(area, z) : new List<Rect>();
                var panels = GratingLayout.BuildPanels(area, _d.MaxWidth, _d.MaxLength, _d.PanelGap, openings);

                foreach (var p in panels) CreatePanel(p, z);
                _model.CommitChanges();
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.ToString(), "AutoGrating");
                return false;
            }
        }

        private void Defaults()
        {
            if (_d.MaxWidth <= 0) _d.MaxWidth = 1000;
            if (_d.MaxLength <= 0) _d.MaxLength = 6000;
            if (_d.PanelGap < 0) _d.PanelGap = 0;
            if (_d.GratingHeight <= 0) _d.GratingHeight = 30;
            if (string.IsNullOrEmpty(_d.GratingName)) _d.GratingName = "GRATING";
            if (string.IsNullOrEmpty(_d.Material)) _d.Material = "S235JR";
            if (string.IsNullOrEmpty(_d.Class)) _d.Class = "8";
            if (_d.OpeningClearance < 0) _d.OpeningClearance = 0;
            if (_d.TopPlateThickness <= 0) _d.TopPlateThickness = 6;
            if (string.IsNullOrEmpty(_d.TopPlateMaterial)) _d.TopPlateMaterial = _d.Material;
        }

        /// <summary>
        /// Find parts (pipes, columns, beams...) that pass through the grating level inside
        /// the area and return their plan footprint grown to the opening size.
        /// </summary>
        private List<Rect> DetectOpenings(Rect area, double z)
        {
            var result = new List<Rect>();
            var min = new Point(area.MinX, area.MinY, z - 10);
            var max = new Point(area.MaxX, area.MaxY, z + _d.GratingHeight + _d.TopPlateThickness + 10);
            var sel = _model.GetModelObjectSelector().GetObjectsByBoundingBox(min, max);
            double top = z + _d.GratingHeight;
            while (sel.MoveNext())
            {
                var part = sel.Current as Part;
                if (part == null) continue;
                if (part.Name == _d.GratingName || part.Name == "GRATING_TOP") continue; // skip own output
                Solid s = part.GetSolid();
                if (s == null) continue;
                // Obstruction only if it actually pierces the grating level
                // (starts below the grating and ends above the support level).
                bool pierces = s.MinimumPoint.Z < z - 1e-3 && s.MaximumPoint.Z > top - 1e-3;
                if (!pierces) continue;
                var fp = new Rect(s.MinimumPoint.X, s.MinimumPoint.Y, s.MaximumPoint.X, s.MaximumPoint.Y);
                if (!fp.Intersects(area)) continue;
                result.Add(GratingLayout.SizeOpening(fp, _d.OpeningClearance, _d.OpeningRound));
            }
            return result;
        }

        private void CreatePanel(GratingPanel p, double z)
        {
            double h = _d.GratingHeight;
            var grating = MakePlate(p.Bounds, z + h / 2, h, _d.GratingName, _d.Material);
            if (!grating.Insert()) return;
            foreach (var o in p.Openings) Cut(grating, o, z, h + _d.TopPlateThickness);

            if (_d.TopPlate == 1)
            {
                double t = _d.TopPlateThickness;
                var top = MakePlate(p.Bounds, z + h + t / 2, t, "GRATING_TOP", _d.TopPlateMaterial);
                if (!top.Insert()) return;
                foreach (var o in p.Openings) Cut(top, o, z, h + t);
            }
        }

        private static ContourPlate MakePlate(Rect r, double zMid, double thickness, string name, string material)
        {
            var cp = new ContourPlate();
            cp.AddContourPoint(new ContourPoint(new Point(r.MinX, r.MinY, zMid), null));
            cp.AddContourPoint(new ContourPoint(new Point(r.MaxX, r.MinY, zMid), null));
            cp.AddContourPoint(new ContourPoint(new Point(r.MaxX, r.MaxY, zMid), null));
            cp.AddContourPoint(new ContourPoint(new Point(r.MinX, r.MaxY, zMid), null));
            cp.Profile.ProfileString = "PL" + thickness;
            cp.Material.MaterialString = material;
            cp.Name = name;
            cp.Position.Depth = Position.DepthEnum.MIDDLE;
            return cp;
        }

        private void Cut(ContourPlate target, Rect o, double z, double totalThickness)
        {
            double zMid = z + totalThickness / 2;
            var cutter = MakePlate(o, zMid, totalThickness + 20, "OPENING_CUT", _d.Material);
            cutter.Class = BooleanPart.BooleanOperativeClassName;
            var bp = new BooleanPart { Father = target, Type = BooleanPart.BooleanTypeEnum.BOOLEAN_CUT };
            bp.SetOperativePart(cutter);
            bp.Insert();
        }
    }
}
