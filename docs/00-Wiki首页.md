# platformer-prototype · Code Wiki

> 面向《游戏策划 / 游戏引擎设计 / 交互设计概论》学期课程的 Unity 2D 平台跳跃原型。
> 本 Wiki 对项目的代码结构、模块职责、关键类与函数、依赖关系与运行方式做结构化梳理。

---

## 文档导航

| 文档 | 内容 |
|---|---|
| **[01-项目架构总览.md](./01-项目架构总览.md)** | 项目定位、技术栈、目录结构、整体架构图、数据流 |
| **[02-核心框架-Core.md](./02-核心框架-Core.md)** | 命名空间 Prototype.Core：EventBus、Events、GameManager、MonoSingleton、SceneManager |
| **[03-玩法系统-Gameplay.md](./03-玩法系统-Gameplay.md)** | 命名空间 Prototype.Gameplay：玩家控制、收集物、敌人、陷阱、移动平台、相机、终点、过关条件、关卡配置、测试工具 |
| **[04-UI系统.md](./04-UI系统.md)** | 命名空间 Prototype.UI：HUD（游戏内 UI）、ResultUI（结算界面） |
| **[05-特效与音频.md](./05-特效与音频.md)** | 命名空间 Prototype.Effects + Prototype.Audio：CollectBurst 粒子、FloatingText 飘分、Sfx 程序化音效 |
| **[06-编辑器工具.md](./06-编辑器工具.md)** | Assets/Editor/：场景搭建、像素素材加载、自动测试入口、2D 模式配置、通关闭环验证 |
| **[07-运行指南.md](./07-运行指南.md)** | 环境要求、搭建步骤、操作说明、关卡数据、菜单命令速查 |
| **[08-附录-索引与依赖.md](./08-附录-索引与依赖.md)** | 类索引、事件索引、C# 文件清单、Packages 依赖、git 历史摘要 |

---

## 快速数据

| 项 | 值 |
|---|---|
| 引擎 | Unity 2022.3.62f3c1 |
| 语言 | C# |
| 命名空间 | Prototype |
| 代码文件 | 26 个 C# 脚本（含 Editor 7 个） |
| 场景 | Main.unity + Result.unity |
| 美术资源 | Kenney Pixel Platformer（CC0，501 文件） |
| 音频 | 全部程序化合成，无外部音频文件 |
| 物理 | Rigidbody2D + BoxCollider2D（Layer: Ground） |
| 远程仓库 | https://github.com/duyv0826/platformer-prototype.git |

---

## 一句话数据流

```
Collectible2D.OnTriggerEnter2D
  -> GameManager.RegisterCollect()        // 更新分数/连击
  -> EventBus.Publish(ScoreChangedEvent)  // 广播
      -> HUD.OnScoreChanged()              // 刷新分数/进度条
      -> WinCondition.OnScoreChanged()     // 达到目标 -> 切场景
      -> ResultUI（结算页读取最终分数）
```

模块之间 **零直接引用**，全部通过静态 EventBus 解耦。
