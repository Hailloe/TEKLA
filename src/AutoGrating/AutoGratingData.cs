using Tekla.Structures.Plugins;

namespace TeklaAutoGrating
{
    /// <summary>Plugin attributes; names are what the dialog binds to.</summary>
    public class AutoGratingData
    {
        [StructuresField("MaxWidth")] public double MaxWidth;          // panel size across bars
        [StructuresField("MaxLength")] public double MaxLength;        // panel size along span
        [StructuresField("PanelGap")] public double PanelGap;
        [StructuresField("EdgeClearance")] public double EdgeClearance;
        [StructuresField("GratingHeight")] public double GratingHeight;
        [StructuresField("GratingProfileName")] public string GratingName;
        [StructuresField("Material")] public string Material;
        [StructuresField("Class")] public string Class;
        [StructuresField("AutoOpening")] public int AutoOpening;      // 1 = detect obstructions
        [StructuresField("OpeningClearance")] public double OpeningClearance;
        [StructuresField("OpeningRound")] public double OpeningRound;  // round size up to multiple
        [StructuresField("TopPlate")] public int TopPlate;             // 1 = add top (chequered) plate
        [StructuresField("TopPlateThickness")] public double TopPlateThickness;
        [StructuresField("TopPlateMaterial")] public string TopPlateMaterial;
    }
}
