<div align="center">
<img style="width: auto; height: auto; border-radius: 12px;" src="STS2_Starborn\ModImage.jpg" alt="WineFox Mod"/>
<h1>STS2 Starborn Mod</h1>
<div style="display: flex;flex-direction: row;justify-content: center;">
<img src="https://img.shields.io/badge/-C%23-239120?style=for-the-badge&logo=csharp&logoColor=white" alt="C#"/> 
<img src="https://img.shields.io/badge/-.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET"/> 
<img src="https://img.shields.io/badge/-Godot-478CBF?style=for-the-badge&logo=godotengine&logoColor=white" alt="Godot"/> 
<img src="https://img.shields.io/badge/-Slay%20the%20Spire%202-8B0000?style=for-the-badge&logoColor=white" alt="Slay the Spire 2"/> 
<a href="https://github.com/BAKAOLC/STS2-RitsuLib"><img src="https://img.shields.io/badge/-STS2--RitsuLib-5538DD?style=for-the-badge&logo=github&logoColor=white" alt="STS2-RitsuLib"/></a> 
<img src="https://img.shields.io/badge/version-0.1.0--aplha-2196f3?style=for-the-badge" alt="Version"/>
</div>
</div>



---

> [!IMPORTANT]
> 本项目大部分内容由AI驱动开发。

---

## 角色特色
主轴是**奇波**和**印记**两套机制，围绕它们构建了卡牌和遗物。奇波拥有自己的牌堆，并在每个回合结束时会随机打一张牌（不会吃大部分效果，不会计入出牌数）。印记参考原版，最大5层，3/5层时会在回合结束消耗特定数量的印记触发调谐/超限效果。（**随时可能更改**）

---

## 设计目标
- 目前总体上希望该角色偏向运转，平均卡组大小在25以下。
- 强度尽量合理（指我玩的爽）

---

## 施工中...

### 核心机制
- [x] 印记 / 超限 / 调谐 —— 机制构建
- [x] 印记 / 超限 / 调谐 —— 测试卡牌
- [x] 奇波 —— 机制构建
- [x] 奇波 —— 测试卡牌
- [x] 属性印记重构（从 Power 中抽离，改用独立 UI）
- [x] 奇波持久化卡堆、战斗内外存储解耦
- [ ] 各属性效果


### 卡牌
- [ ] 初始牌　　　`1 / 2`
- [ ] 普通牌　　　`1 / 20`
- [ ] 罕见牌　　　`2 / 35`
- [ ] 稀有牌      `0 / 25`
- [ ] 远古牌　　　`0 / 5`
- [x] 事件牌　　　`3 / 3` *（暂定）*

### 遗物
- [ ] 角色遗物  （还未定）

### 其他
- [x] 彩蛋事件
- [x] 御三家选择事件
- [ ] 文本描述统一
- [ ] 美术 （不会）

## 卡牌列表

### 初始牌组（Basic）

| 卡牌 | 费用 | 类型 | 效果 | 升级 |
|------|------|------|------|------|
| 打击（StarbornStrike） | 1 | 攻击 | 造成 6 点伤害。 | 伤害→9 |
| 防御（StarbornDefend） | 1 | 技能 | 获得 5 点格挡。 | 格挡→8 |
| 协同（Synergy） | 1 | 攻击 | 造成7点伤害。释放当前奇波的大招。 |   |
|   | 1 |   |   |   |

### 普通牌（Common）
| 卡牌 | 费用 | 类型 | 效果 | 升级 |
|------|------|------|------|------|
| 不够大！（NeedMoreBigger）  | 1 | 攻击 | 给目标施加1层**变大**。造成8 ×**变大**层数的伤害。  | 8->12  |

### 罕见牌（Uncommon）
| 卡牌 | 费用 | 类型 | 效果 | 升级 |
|------|------|------|------|------|
| 不够小！（NoBigger）  | 1 | 技能 | 获得7格挡，给目标施加1层**变小**和1层**虚弱**。 | 虚弱层数→2 |
| 随机上岗（RandomDuty）  | 2 | 能力 | 回合开始时，随机切换一只奇波，并释放其一张普通能力牌。 | 待定  |


### 稀有牌（Rare）
| 卡牌 | 费用 | 类型 | 效果 | 升级 |
|------|------|------|------|------|
| 救世形态（SaverForm） | 3 | 能力 | 调谐/超限的印记消耗-1。<br>回合开始时印记属性不再重置。 | **待定** |
| 红宝石布什曲（RubyIsTier0） | 2 | 能力 | 将主/副属性固定为火。每回合开始时获得2/2层印记。<br>每回合打出的前4张攻击牌获得：调谐 1/1，造成当前灼烧层数的伤害。 | 前4张→前6张 |
| 异闪（VariableShiny） | 2 | 技能 | 升级你所有的奇波牌。 |   |




### 远古牌（Ancient）
| 卡牌 | 费用 | 类型 | 效果 | 升级 |
|------|------|------|------|------|
|   | 3 |   |   |   |



### 事件牌（Event）
| 卡牌 | 费用 | 类型 | 效果 | 升级 |
|------|------|------|------|------|
| LuckyE  | 1 | 技能  |**消耗** 在下个回合，使所有攻击伤害翻倍并使受到的所有伤害和生命减少效果降低为1 | 移除**消耗**  |
| 演算（Simulation） | 1 | 能力  | **固有** 你的所有牌耗能减少1点。 | 抽[blue]2[/blue]张牌。  |
| 失控（OutOfControl） | 无法打出 | 能力  | **保留**。**永恒** 。在你的回合结束和战斗结束时，失去1点生命上限。 |   |

---

## 遗物列表
| 遗物 | 稀有度 | 效果 |
|------|------|------|
| 星结卡 |  初始遗物 | 未定 |
| 心智魔方（WisdomCube） |  事件 |  将1张LuckyE加入你的卡组 |
| 元魔方（MetaCube） |  事件 |  将1张演算和1张失控加入你的卡组 |

---

## 事件列表
| 事件 | 条件 | 选项1 | 选项2 | 选项3 |
|------|------|------|------|------|
| 御三家 | 持有初始遗物**星结卡** | 小芽狐 | 河狸仔 | 焰哞哞 |
| ？？实验室 | 持有**锚**、**船甲板**和**舵盘** | 心智魔方 | 元魔方 | - |


## 项目参考
- [杀戮尖塔2mod制作教程](https://tutorials.sts2modding.com)
- [STS2_WineFox](https://github.com/LuoTianOrange/STS2_WineFox)
