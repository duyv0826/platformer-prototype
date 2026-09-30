# platformer-prototype — 2D 平台跳跃原型

> 面向《游戏策划 / 游戏引擎设计 / 交互设计概论》学期课程的 2D 平台跳跃原型，
> 由私有仓库《Unity 原型脚手架》的框架合并而来，按 `PLAN.md` 定为 **Unity 2022.3.62f3c1**。
> 原脚手架是 3D（CharacterController），这里已统一适配为 **2D（Rigidbody2D）**。

## 目录结构

```
Assets/
├─ Scenes/            # 场景（Main / Result）
├─ Prefabs/           # 预设体
├─ Resources/         # 运行时 Resources.Load 资源
├─ Art/               # 美术素材（精灵/材质）
├─ Audio/             # 音效/音乐
├─ Editor/
│  └─ SceneSetupHelper.cs   # 一键搭建 2D 场景
└─ Scripts/
   ├─ Core/           # 核心框架（复用自脚手架）
   │  ├─ MonoSingleton.cs
   │  ├─ EventBus.cs
   │  ├─ Events.cs
   │  ├─ GameManager.cs
   │  └─ SceneManager.cs
   ├─ Gameplay/       # 2D 玩法（玩家/收集物/相机/过关）
   ├─ UI/             # HUD / 结算
   └─ Utilities/
Packages/manifest.json
ProjectSettings/
├─ ProjectVersion.txt     # Unity 2022.3.62f3c1
└─ EditorSettings.asset   # Default Behavior Mode = 2D（配 Assets/Editor/Platformer2DSettings.cs 兜底）
```

## 怎么跑起来

1. 用 **Unity 2022.3.62f3c1** 打开本文件夹作为工程根目录（已预置 `EditorSettings.asset` 的 2D 行为模式；其余如 Input 轴、Player 标签等使用引擎默认值）。
2. 菜单栏执行 **`Prototype / 搭建场景 / 全部（Main + Result）`**，一键生成 `Assets/Scenes/Main.unity` 与 `Assets/Scenes/Result.unity`。
3. 打开 `Main` 场景，点 **Play**。
4. **A / D 或方向键**左右移动，**空格**跳跃；碰到收集物加分（15 颗红心各 5 分、3 颗蓝宝石各 15 分，连续收集有连击加成）。
5. 走到终点**碰到旗帜**即通关并切换至 `Result` 结算场景；点「再玩一次」重置本局回到 `Main`。分数不决定通关，只决定结算星级里的连击表现。

数据流：`Collectible2D` → `GameManager.RegisterCollect`（连击计分）→ `EventBus.Publish(ScoreChangedEvent)` → `HUD` 订阅刷新，模块间零直接引用。
通关数据流：`FlagGoal.OnTriggerEnter2D` → `GameManager.MarkWon` → `SceneManager.Load("Result")` → `ResultUI` 按 `Collected / TotalCollectibles` 评星。

## 与脚手架的差异（已按 2D 适配）

| 脚手架（3D） | 本工程（2D） | 变化 |
|---|---|---|
| `PlayerController`（CharacterController） | `PlayerController2D`（Rigidbody2D + 跳跃） | 重写 |
| `Collectible`（`OnTriggerEnter`） | `Collectible2D`（`OnTriggerEnter2D`） | 改用 2D 触发 |
| `CameraFollow`（透视 + LookAt） | `CameraFollow2D`（正交侧视跟随） | 重写 |
| 场景：Plane/Capsule/灯光 | 场景：白色精灵方块 + 正交相机 | 2D 化 |

`Core/` 与 `UI/` 为引擎无关逻辑，原样保留。