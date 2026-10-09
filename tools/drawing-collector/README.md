# 结构图纸整理 (GLNG7 RECEIVED)

从 `\\192.168.100.12\...\ST7 GLNG7 (GLNG 7.1 & HRLS 4) PROJECT\RECEIVED` 里找出所有结构 (Structure) 图纸，生成图纸清单，并复制最新版本。

## 用法
在能访问公司共享盘的 Windows 电脑上，双击 `Collect-StructureDrawings.bat`。

结果在桌面 `GLNG7_Structure_Drawings`：
- `Structure_Drawing_Register_<时间>.csv` — 图纸清单 (图号、版本、格式、是否最新、来文文件夹、文件名、修改时间、路径)，Excel 直接打开
- `Latest\` — 每个图号 (每种格式) 的最新版本

其它模式 (PowerShell 中运行)：
```powershell
.\Collect-StructureDrawings.ps1 -Mode List                      # 只出清单
.\Collect-StructureDrawings.ps1 -Mode All                       # 按来文文件夹复制全部版本到 By_Transmittal\
.\Collect-StructureDrawings.ps1 -Output 'D:\GLNG7\STR'          # 换输出位置
.\Collect-StructureDrawings.ps1 -ExtraPattern '-S-\d{4}'        # 图号里用 -S- 表示结构专业
.\Collect-StructureDrawings.ps1 -Keywords ST,STR,STRUCTURAL     # 自定义关键字
```

## 识别规则
- 文件夹名或文件名里出现单独的关键字：`ST STR STRU STRUC STRUCT STRUCTURE(S) STRUCTURAL STEEL STEELWORK 结构 钢结构`
- 版本从文件名解析：`_Rev B`、` REV.0`、`-REV01`、结尾 `_C` / `_R1`
- 最新版：数字版 (0,1,2…) 高于字母版 (A,B,C…)；同版本取修改时间最新的
- 压缩包 (zip/rar/7z) 里的图纸不会被扫描，脚本会提示数量
