# 2048 — 经典数字合成游戏

[![GitHub repo size](https://img.shields.io/github/repo-size/38749162626/2048_WebGame?style=flat-square&logo=github)](https://github.com/38749162626/2048)
[![GitHub last commit](https://img.shields.io/github/last-commit/38749162626/2048_WebGame?style=flat-square&logo=github)](https://github.com/38749162626/2048)

## 🎯 游戏目标

在 4×4 的网格中，通过滑动数字方块，将相同数字合并，**最终合成数字 `2048`** 即为胜利。游戏会记录你的最高分（BEST），不断挑战自我，看你能突破多少分！

---

## 🕹️ 玩法详解

### 基本操作
- **键盘**：按方向键（↑ ↓ ← →）或 (W A S D) 控制方块整体移动。
- **触屏**：在手机/平板上直接滑动屏幕即可。
- **手柄**：十字键控制方块整体移动。
- 每次移动，**所有方块会一同滑向对应方向**，直到撞墙或撞到其他方块。

### 核心规则
1. **合并规则**：两个 **数字相同** 的方块在移动过程中相遇，会合并为一个新方块，其数字为两者之和（例如 `2+2=4`，`4+4=8`，以此类推）。  
2. **生成规则**：每次有效移动后，会在网格的**空白位置**随机生成一个新方块，其数字为 `2`（概率约 90%）或 `4`（概率约 10%）。  
3. **结束条件**：当网格被完全填满，且**没有相邻的相同数字方块**时，游戏结束（Game Over）。  
4. **胜利条件**：一旦成功合成 `2048`，游戏即告胜利，但你可以选择继续游戏，追求更高分数。

### 计分方式
- 每次合并产生的数字会 **累加** 到当前分数（SCORE）中。  
  例如：合并 `8+8=16` 时，SCORE 增加 `16` 分。  
- 最高分（BEST）会保存在本地（PlayerPres），关闭后依然保留。

### 策略小贴士
- **保持最大数在角落**：将最大的数字（如 1024）固定在某个角落，能有效减少混乱，便于规划合并路线。  
- **避免孤立小数字**：尽量让所有方块连成一片，避免将小数字“困”在边缘无法合并。  
- **优先合成大数**：不要急于合并所有小数字，先集中资源合成一个较大的数字，再逐步扩展。  
- **观察下一步**：每次移动前思考新方块出现的位置，尽量让新方块落在你计划好的空白处。

---

## 📸 游戏截图与动效

| 游戏开始 | 游戏结束 |
|:---:|:---:|
| ![游戏进行中](./Screenshots/Begin.png) | ![游戏结束](./Screenshots/End.png) |

🎬 **动态演示**（GIF）  
![游戏动效演示](./Screenshots/Merging.gif)

---

## 🎮 在线游玩

无需下载，点击下方链接即可直接在浏览器中游玩：

[![Play Now](https://img.shields.io/badge/▶️-Play_Now-4CAF50?style=for-the-badge)](https://38749162626.github.io/2048_WebGame/)

> 游戏基于 Unity WebGL 构建，建议使用最新版 Chrome、Edge 或 Firefox 浏览器获得最佳体验。

---

## 📦 下载游戏

如果你不想从源码构建，可以直接下载已编译好的游戏包。

[![Download Latest Release](https://img.shields.io/badge/📥-Download_Latest_Release-brightgreen?style=for-the-badge&logo=github)](https://github.com/38749162626/2048/releases)

> 请从 Releases 页面选择适合你操作系统的版本。

---

## 💻 源码展示

本项目为 Unity 开发的游戏源代码。如需克隆，请按以下步骤操作：

1. **克隆仓库**  
   ```bash
   git clone https://github.com/3874916262/2048.git# 2048 — 经典数字合成游戏
