using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Dialog;

namespace TeklaAutoGrating
{
    /// <summary>Plugin dialog, built in code. Fields bind to AutoGratingData via StructuresDialog.</summary>
    public class AutoGratingForm : PluginFormBase
    {
        private TextBox _maxWidth;
        private TextBox _maxLength;
        private TextBox _gap;
        private TextBox _edge;
        private TextBox _height;
        private TextBox _name;
        private TextBox _material;
        private CheckBox _autoOpening;
        private TextBox _openClear;
        private TextBox _openRound;
        private CheckBox _topPlate;
        private TextBox _tpThk;
        private TextBox _tpMat;

        public AutoGratingForm()
        {
            Text = "Auto Grating";
            Size = new Size(360, 520);
            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            Controls.Add(table);

            _maxWidth = AddText(table, "Max panel width", "1000");
            _maxLength = AddText(table, "Max panel length", "6000");
            _gap = AddText(table, "Gap between panels", "5");
            _edge = AddText(table, "Edge clearance", "5");
            _height = AddText(table, "Grating height", "30");
            _name = AddText(table, "Name", "GRATING");
            _material = AddText(table, "Material", "S235JR");
            _autoOpening = AddCheck(table, "Auto opening at obstructions", true);
            _openClear = AddText(table, "Opening clearance", "25");
            _openRound = AddText(table, "Round opening size to", "50");
            _topPlate = AddCheck(table, "Top plate", true);
            _tpThk = AddText(table, "Top plate thickness", "6");
            _tpMat = AddText(table, "Top plate material", "S235JR");

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36 };
            buttons.Controls.Add(Btn("OK", (s, e) => { Apply(); Close(); }));
            buttons.Controls.Add(Btn("Modify", (s, e) => Modify()));
            buttons.Controls.Add(Btn("Get", (s, e) => Get()));
            buttons.Controls.Add(Btn("Cancel", (s, e) => Close()));
            Controls.Add(buttons);

            Bind(_maxWidth, "MaxWidth", "Distance");
            Bind(_maxLength, "MaxLength", "Distance");
            Bind(_gap, "PanelGap", "Distance");
            Bind(_edge, "EdgeClearance", "Distance");
            Bind(_height, "GratingHeight", "Distance");
            Bind(_name, "GratingProfileName", "String");
            Bind(_material, "Material", "String");
            Bind(_autoOpening, "AutoOpening", "Integer");
            Bind(_openClear, "OpeningClearance", "Distance");
            Bind(_openRound, "OpeningRound", "Distance");
            Bind(_topPlate, "TopPlate", "Integer");
            Bind(_tpThk, "TopPlateThickness", "Distance");
            Bind(_tpMat, "TopPlateMaterial", "String");

            InitializeForm();
        }

        private void Bind(Control c, string attribute, string typeName)
        {
            structuresExtender.SetAttributeName(c, attribute);
            structuresExtender.SetAttributeTypeName(c, typeName);
        }

        private static Button Btn(string text, System.EventHandler h)
        {
            var b = new Button { Text = text, Width = 75 };
            b.Click += h;
            return b;
        }

        private static TextBox AddText(TableLayoutPanel t, string label, string def)
        {
            t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
            var tb = new TextBox { Text = def, Width = 160 };
            t.Controls.Add(tb);
            return tb;
        }

        private static CheckBox AddCheck(TableLayoutPanel t, string label, bool def)
        {
            t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
            var cb = new CheckBox { Checked = def };
            t.Controls.Add(cb);
            return cb;
        }
    }
}
