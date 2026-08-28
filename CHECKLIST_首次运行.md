# 首次运行 checklist —— 打开 Unity 并跑起来

> 目的：把 platformer-prototype 在 Unity 里第一次跑出画面。
> 对应 PLAN 的 **M0**（Unity 最小场景跑起来）。做完勾掉，卡住看文末「出问题排查」。

## 0. 环境确认（一次性）

- [ ] Unity Hub 已安装
- [ ] Unity 编辑器 **2022.3.62f3c1** 已在 Unity Hub 里安装（不带这个版本 Unity 会提示升级/降级）
- [ ] Visual Studio 2022 已装（C# 脚本编辑用）

## 1. 打开工程

- [ ] 打开 Unity Hub → 左侧 **Projects** → **Open**（下拉选 **Add project from disk**）
- [ ] 选择文件夹 `C:\Users\duyv\Documents\platformer-prototype` → 点 **Add Project** / **Open**
- [ ] 确认右上角版本显示为 `2022.3.62f3c1`（若有差异，按提示切换到该版本打开）
- [ ] 首次进入等右下角进度条走完（编译脚本 + 导入资源，可能需要 1–3 分钟）
- [ ] 打开 Console 窗口（Window / 一般；或 Ctrl+0），确认**没有红色报错**（黄色警告可忽略）

## 2. 一键生成场景（M0）

- [ ] 点顶部菜单 **`Prototype`** → **`搭建场景`** → **`全部（Main + Result）`**
- [ ] Console 里应出现两条提示：`Main 场景已搭建并保存` 和 `Result 场景已搭建并保存`
- [ ] 确认侧边栏 Project 窗口出现 `Assets/Scenes/Main.unity` 与 `Assets/Scenes/Result.unity`
- [ ] 确认生成了占位贴图 `Assets/Art/Placeholder_White.png`

## 3. 检查和跑

- [ ] 双击打开 `Assets/Scenes/Main.unity`
- [ ] 顶部点绿色 **▶ Play** 按钮
- [ ] **A / ←** 向左走，**D / →** 向右走
- [ ] **空格** 跳跃（站地上才能跳）
- [ ] 走向**黄色小方块**收集物，看到左上角「分数」增加
- [ ] 收集满 **50 分**，画面自动切到 `Result` 结算场景
- [ ] 点「再玩一次」按钮，回到 `Main` 且分数清零
- [ ] 再点一次 **▶ Play** 停止运行

## 4. 收尾

- [ ] `Ctrl+S` 保存 Main / Result 场景（如改动过）
- [ ] 可选：把这次跑出的画面录屏，作为作品集/演示素材
- [ ] （交接项）下次双击 `.sln` 或从 Hub 打开同一文件夹即可继续

---

## 出问题排查

| 现象 | 很可能的原因 | 处理 |
|---|---|---|
| Console 红色报错多 | 首次导入未完成 / 版本不符 | 等进度条走完；确认是 2022.3.62f3c1 |
| 「分数」不动 | 没碰到触发区，或玩家标签丢了 | 玩家应带 `Player` 标签；收集物 Collider 勾 Is Trigger |
| 按空格跳不起来 | 不在地面检测范围内 | 检查地面紧贴玩家脚下、间距 <0.6 个单位 |
| 物体看不见（透明） | 场景是旧引用、占位贴图丢了 | 重新跑一次「搭建场景/全部」 |
| Play 画面黑/是 3D 视角 | 该编译代码没生效 | 重启 Unity 等脚本重编，再跑一次搭建场景 |