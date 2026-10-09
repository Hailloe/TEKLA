@echo off
rem Double-click: collect structure drawings to Desktop\GLNG7_Structure_Drawings
rem Register only: Collect-StructureDrawings.bat -Mode List
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0Collect-StructureDrawings.ps1" %*
pause
