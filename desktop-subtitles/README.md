# 实时翻译字幕（桌面版，Windows）

把网页版实时翻译做成电脑程序，多了一层 **透明、永远置顶、鼠标可以点穿** 的字幕，浮在 Teams、视频、Tekla 等任何程序上面。

## 下载安装

1. 打开仓库的 **Releases** 页面，下载最新的：
   - `LiveSubtitles-Setup-x.x.x.exe`：安装版（推荐）
   - `LiveSubtitles-x.x.x-portable.exe`：免安装，双击直接运行
2. 第一次打开时 Windows 可能提示“Windows 已保护你的电脑 / 未知发布者”（因为程序没有付费代码签名）：点 **更多信息 → 仍要运行**。

## 使用

1. 打开后先点 **⚙️ 设置**：
   - **语音识别**填 AI 识别的 Key（默认硅基流动 SenseVoice，在 cloud.siliconflow.cn 申请）。
     桌面版不能用浏览器自带的免费识别，必须用 AI 识别。
   - 想用 AI 翻译的话，再填 Claude 或 DeepSeek 的 Key；不填就用免费的 Google 翻译。
2. 左上角声音来源：
   - **💻 电脑声音**：会议、视频里别人的声音（自动录电脑播放的声音，不用选窗口，戴耳机也可以）
   - **💻+🎤 电脑声音 + 我的麦克风**：同时记录“我”和“对方”
3. 点 **开始翻译**，屏幕下方会出现字幕。

## 字幕层

| 操作 | 方法 |
|---|---|
| 锁定 / 解锁 | **Ctrl+Shift+L**，或主窗口的「🔒 已锁定 / 🔓 可拖动」按钮 |
| 隐藏 / 显示 | **Ctrl+Shift+H**，或主窗口的「隐藏字幕层」按钮 |
| 移动、调大小 | 先解锁，拖动字幕区域移动，拖边缘调大小，调好后再锁定 |
| 字号、只看中文 | 主窗口的 A− / A+、「原文」按钮 |

- **锁定**（默认）：只显示字，背景透明，鼠标点击会穿过字幕，落到下面的程序上
- **解锁**：显示虚线框和半透明底色，可以拖动、调大小；位置会记住
- 关闭主窗口即退出程序

## 开发

```bash
cd desktop-subtitles
npm install
npm start        # 运行（会先把 ../meeting-translator 复制到 web/）
npm run dist     # 在 Windows 上打包 exe（GitHub Actions 会自动做）
```

推送到默认分支后，`.github/workflows/desktop-subtitles.yml` 会在 Windows 上打包，并把 exe 发布到 Releases。
