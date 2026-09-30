using System.Drawing;
using System.Windows.Forms;
using Tekla.Structures.Dialog;
using Tekla.Structures.Dialog.UIControls;

namespace TeklaAutoGrating
{
    /// <summary>Plugin dialog, built in code. Controls bind by AttributeName to AutoGratingData.</summary>
    public class AutoGratingForm : PluginFormBase
    {
        public AutoGratingForm()
        {
            Text = "Auto Grating";
            Size = new Size(360, 520);
            var table = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };
            Controls.Add(table);

            AddText(table, "Max panel width", "MaxWidth", "1000");
            AddText(table, "Max panel length", "MaxLength", "6000");
            AddText(table, "Gap between panels", "PanelGap", "5");
            AddText(table, "Edge clearance", "EdgeClearance", "5");
            AddText(table, "Grating height", "GratingHeight", "30");
            AddText(table, "Name", "GratingProfileName", "GRATING", false);
            AddText(table, "Material", "Material", "S235JR", false);
            AddCheck(table, "Auto opening at obstructions", "AutoOpening", true);
            AddText(table, "Opening clearance", "OpeningClearance", "25");
            AddText(table, "Round opening size to", "OpeningRound", "50");
            AddCheck(table, "Top plate", "TopPlate", true);
            AddText(table, "Top plate thickness", "TopPlateThickness", "6");
            AddText(table, "Top plate material", "TopPlateMaterial", "S235JR", false);

            var buttons = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 36 };
            buttons.Controls.Add(Btn("OK", (s, e) => { Apply(); Close(); }));
            buttons.Controls.Add(Btn("Modify", (s, e) => Modify()));
            buttons.Controls.Add(Btn("Get", (s, e) => Get()));
            buttons.Controls.Add(Btn("Cancel", (s, e) => Close()));
            Controls.Add(buttons);
        }

        private static Button Btn(string text, System.EventHandler h)
        {
            var b = new Button { Text = text, Width = 75 };
            b.Click += h;
            return b;
        }

        private static void AddText(TableLayoutPanel t, string label, string attr, string def, bool numeric = true)
        {
            t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
            var tb = new StructuresTextBox { Text = def, Width = 160 };
            tb.AttributeName = attr;
            tb.AttributeTypeName = numeric ? "Double" : "String";
            t.Controls.Add(tb);
        }

        private static void AddCheck(TableLayoutPanel t, string label, string attr, bool def)
        {
            t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
            var cb = new StructuresCheckBox { Checked = def };
            cb.AttributeName = attr;
            cb.AttributeTypeName = "Integer";
            t.Controls.Add(cb);
        }
    }
}
