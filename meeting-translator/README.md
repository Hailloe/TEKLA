# 实时翻译（手机 + 电脑）

一个网页应用，不需要安装：对着手机/电脑说话（开会、看电视、看视频），实时显示**中文翻译 + 原文**。

- 语言：英语（美/英/澳/印/新加坡）、泰语、日语、韩语、越南语、马来语、印尼语、法语、德语、西班牙语 → 简体 / 繁体中文
- 说话过程中先显示灰色的预览翻译，一句话说完后变成正式翻译
- 字体大小可调（A− / A+），可隐藏原文，只看中文
- 屏幕常亮（运行时手机不会自动锁屏）
- 一键导出 `.txt` 记录
- 可以“添加到主屏幕”，像 App 一样打开
- **悬浮字幕小窗**（电脑版 Chrome / Edge）：点顶部「小窗」，弹出一个永远置顶、电影字幕样式（黑底白字、只显示最新两句）的小窗口，可拖到视频或 Teams 下方；小窗右上角有开始/停止和字号按钮
- **⚙️ 设置**：所有 API Key（Claude、DeepSeek、AI 语音识别）和会议专有名词都在这里填，以后想用 AI 随时自己设置
- **声音来源**：麦克风 / 电脑声音 / **电脑声音 + 我的麦克风**（会议记录里同时有「我」和「对方」）
- 状态栏旁的绿色音量条显示网页有没有收到声音；6 秒没声音会提示原因
- **选择麦克风**（⚙️ 设置 → 麦克风）：例如戴蓝牙耳机时选手机自带的麦克风。用 AI 识别时手机、电脑都有效；用浏览器自带识别时只有电脑版 Chrome / Edge 有效（手机上由系统决定，安卓可以在蓝牙耳机设置里关掉“通话音频”）

## AI 翻译（Claude）

顶部翻译引擎选 **Claude AI 翻译**，填入自己的 Claude API Key（在 [Anthropic Console](https://console.anthropic.com/settings/keys) 申请）。

- 会结合前几句的上下文翻译，口语、专业词、语音识别听错的词都翻得更自然
- 可以填“会议主题 / 专有名词”，例如 `钢结构项目周会；Tekla、BIM、RFI`，AI 会照着翻
- 模型：Opus 5.5（最准）/ Sonnet 5.5（较快）/ Haiku 5.5（最快最省）
- Key 只保存在本机浏览器，只发给 `api.anthropic.com`；按用量计费
- Claude 出错（Key 不对、没额度、网络）时这一句会自动改用 Google 翻译

### DeepSeek / 其他 AI

翻译引擎选 **DeepSeek / 其他 AI**，填入 DeepSeek 的 API Key（在 [platform.deepseek.com](https://platform.deepseek.com/api_keys) 申请）。

- 默认接口 `https://api.deepseek.com`，模型 `deepseek-v4-flash`（快、便宜；想更准可改成 `deepseek-v4-pro`）
- 已自动关闭 DeepSeek 的“深度思考”，翻译出字更快
- 其他兼容 OpenAI 接口格式的 AI（如通义千问、Kimi、智谱等）也能用：把“接口地址”和“模型”改成它们文档里写的即可
- 如果提示“连接失败（该服务不允许网页直接调用）”，说明这家 AI 不允许浏览器直接访问，只能换一家

### AI 语音识别（可选）

在 **⚙️ 设置 → 语音识别** 里把识别方式改成 **AI 识别**：

- 默认用国内的 **硅基流动 SenseVoice**（[cloud.siliconflow.cn](https://cloud.siliconflow.cn/account/ak) 申请 Key），**不需要连 Google**，口音、专业词更准
- 也可以用 OpenAI Whisper：接口地址 `https://api.openai.com/v1`，模型 `whisper-1`
- 一句话说完后整句识别，比浏览器自带的慢 1–3 秒
- 声音来源选「电脑声音 + 我的麦克风」时，会分别识别并标出 **我 / 对方**；你说的中文直接记录，不再翻译
- 如果提示「连接失败（该服务不允许网页直接调用）」，说明这家服务不允许浏览器直接访问，只能换一家

### 延迟

| 阶段 | 大约 |
|---|---|
| 说话中的灰色预览（Google） | 0.5–1 秒 |
| 一句话说完 → 语音识别确定这一句 | 0.5–1.5 秒 |
| Claude 开始出字（逐字显示） | Haiku 约 0.5–1 秒，Sonnet 约 1–2 秒，Opus 约 1.5–3 秒 |

Claude 的结果出来之前，屏幕上会先保留 Google 的预览，所以看起来几乎没有空白等待。
追求最低延迟选 Haiku 5.5；追求翻译质量选 Opus 5.5。

## 桌面字幕版（Windows）

想要 **透明背景、鼠标可以点穿** 的字幕，浮在 Teams、视频等任何程序上面：到仓库 **Releases** 页面下载 `LiveSubtitles` 桌面版，说明见 [desktop-subtitles/README.md](../desktop-subtitles/README.md)。

## 怎么用

1. 用浏览器打开网址（需要 **https**，否则浏览器不给麦克风权限）
   - 电脑 / 安卓：**Chrome** 或 **Edge**
   - iPhone / iPad：**Safari**（iOS 14.5 及以上）
2. 选择说话人的语言（例如 英语 或 泰语），点 **开始翻译**，允许麦克风权限
3. 把设备放在能清楚听到声音的地方

### 在线会议（Teams / Zoom / 腾讯会议）

**电脑上用「电脑声音」模式（推荐，戴耳机也能用）** —— 需要电脑版 Chrome 或 Edge 135 以上：

1. 用 Chrome / Edge 打开本页，左上角声音来源选 **💻 电脑声音**
2. 点 **开始翻译**，浏览器会弹出“选择要共享的内容”：
   - **Teams 网页版**（teams.microsoft.com 在浏览器标签页里开会）：选 **标签页** → 选会议那个标签页 → 打开 **同时共享标签页音频**（Windows / Mac 都可以）
   - **Teams 电脑客户端**（Windows）：选 **整个屏幕** → 打开 **同时共享系统音频**
   - Mac 上用 Teams 客户端时，系统音频不一定能共享，建议改用 Teams 网页版开会
3. 这里的“共享”只是让本页听到声音，**不会**把你的屏幕共享给会议里的人

**或者用第二台设备**：手机打开本页（声音来源只有麦克风），电脑外放，手机放在喇叭旁边。

### 看电视
手机打开本页，放在离电视近一点的地方，电视音量适当调大即可。

## 部署（得到一个手机能打开的网址）

合并到仓库默认分支后，`.github/workflows/translator-pages.yml` 会自动发布到 GitHub Pages：

1. 仓库 **Settings → Pages → Build and deployment → Source** 选 **GitHub Actions**
2. 合并后，在 **Actions → translator-pages** 里能看到网址，形如
   `https://<用户名>.github.io/<仓库名>/`

也可以把 `meeting-translator` 文件夹直接拖到 Netlify / Vercel / Cloudflare Pages 任意静态托管。
本地测试：`cd meeting-translator && python3 -m http.server`，然后打开 `http://localhost:8000`（localhost 可以用麦克风）。

## 注意

- 语音识别用的是浏览器自带的服务（Chrome 走 Google，Safari 走 Apple），翻译默认用 Google 翻译。
  **在中国大陆需要能访问 Google 的网络**；不行的话可以在顶部切换到 MyMemory 翻译引擎（免费，每天有额度限制）。
- 识别会有 1–2 秒左右的延迟；口音重、多人同时说话、背景音乐大时准确率会下降。
