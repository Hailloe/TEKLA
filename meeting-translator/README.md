# 实时翻译（手机 + 电脑）

一个网页应用，不需要安装：对着手机/电脑说话（开会、看电视、看视频），实时显示**中文翻译 + 原文**。

- 语言：英语（美/英/澳/印/新加坡）、泰语、日语、韩语、越南语、马来语、印尼语、法语、德语、西班牙语 → 简体 / 繁体中文
- 说话过程中先显示灰色的预览翻译，一句话说完后变成正式翻译
- 字体大小可调（A− / A+），可隐藏原文，只看中文
- 屏幕常亮（运行时手机不会自动锁屏）
- 一键导出 `.txt` 记录
- 可以“添加到主屏幕”，像 App 一样打开

## 怎么用

1. 用浏览器打开网址（需要 **https**，否则浏览器不给麦克风权限）
   - 电脑 / 安卓：**Chrome** 或 **Edge**
   - iPhone / iPad：**Safari**（iOS 14.5 及以上）
2. 选择说话人的语言（例如 英语 或 泰语），点 **开始翻译**，允许麦克风权限
3. 把设备放在能清楚听到声音的地方

### 在线会议（Zoom / Teams / 腾讯会议）
浏览器只能听**麦克风**。开会时用**外放**（不戴耳机），让麦克风听到对方的声音；
或者另拿一部手机打开本页，放在电脑喇叭旁边——这样最简单，效果也好。

### 看电视
手机打开本页，放在离电视近一点的地方，电视音量适当调大即可。

## 部署（得到一个手机能打开的网址）

合并到 `main` 后，`.github/workflows/translator-pages.yml` 会自动发布到 GitHub Pages：

1. 仓库 **Settings → Pages → Build and deployment → Source** 选 **GitHub Actions**
2. 合并后，在 **Actions → translator-pages** 里能看到网址，形如
   `https://<用户名>.github.io/<仓库名>/`

也可以把 `meeting-translator` 文件夹直接拖到 Netlify / Vercel / Cloudflare Pages 任意静态托管。
本地测试：`cd meeting-translator && python3 -m http.server`，然后打开 `http://localhost:8000`（localhost 可以用麦克风）。

## 注意

- 语音识别用的是浏览器自带的服务（Chrome 走 Google，Safari 走 Apple），翻译默认用 Google 翻译。
  **在中国大陆需要能访问 Google 的网络**；不行的话可以在顶部切换到 MyMemory 翻译引擎（免费，每天有额度限制）。
- 识别会有 1–2 秒左右的延迟；口音重、多人同时说话、背景音乐大时准确率会下降。
