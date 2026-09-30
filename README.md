# TEKLA Auto Grating (Tekla Structures 2023 plugin)

Pick two opposite corners of an area (work plane horizontal, Z of the first point = top of support steel). The plugin:

- **Auto-sizes grating**: shrinks the area by the edge clearance and splits it into equal panels ≤ max width/length with a panel gap (no slivers).
- **Auto-sizes openings**: finds parts that pierce the grating level (pipes, columns, beams), grows their footprint by the opening clearance, rounds up to a multiple of "Round opening size to", merges overlapping openings, and cuts them from each panel.
- **Top plate**: optional chequered/cover plate of given thickness on each panel, cut with the same openings.

## Build
`set TEKLA_BIN=C:\Program Files\Tekla Structures\2023.0\bin` then `dotnet build -c Release`.
Copy `TeklaAutoGrating.dll` to `...\Environments\<env>\extensions\plugins` (or `bin\plugins`) and run it from Applications & components ("AutoGrating").

## Notes
- Not compiled/tested here (no Tekla or .NET SDK in the build container). The dialog base-class calls (`Apply/Modify/Get`) and control types follow the Tekla Open API plugin samples; verify against your 2023 API on first build.
- Grating is modelled as a solid contour plate (height = grating depth); swap in a bar-by-bar model if needed.
- Layout math lives in `GratingLayout.cs` and has no Tekla dependency.
