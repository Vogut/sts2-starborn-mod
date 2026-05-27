# STS2 Card Creation
When I run /new-card:
1. Ask for card name, type (Attack/Skill/Power/Event), and rarity
2. grep existing cards of same type for structure reference
3. Generate: Card class file + JSON data + DynamicVar setup
4. Advise correct pool registration (EventCardPool vs CharacterPool vs PowerCardPool)
5. Run build and confirm clean

## 关键词规则
- 消耗、保留、固有、虚无等原版关键词和自定义关键词（`RegisterOwnedCardKeyword`）通过 `CanonicalKeywords` 属性以**图标**形式显示在卡面上
- description 中**不要**写 `[gold]消耗[/gold]`、`[gold]保留[/gold]` 等关键词图标文本——图标已由 `CanonicalKeywords` 渲染
- 参照原版 HOTFIX：base 有 Exhaust，description 只写效果文本，升级 `RemoveKeyword` 去掉图标
- **例外**：在描述中引用某个状态/机制名时仍需 `[gold]` 包裹，如"给予[gold]虚弱[/gold]"——这是状态名不是关键词图标

## 颜色约定
- `[gold]术语[/gold]` — 游戏术语（格挡、力量、虚弱、易伤、手牌、抽牌堆等）
- `[blue]数值[/blue]` — 数字（主要用于 smartDescription 的 `[blue]{Amount}[/blue]`）
- `[red]术语[/red]` — 敌对/警告性质术语

## DynamicVar 类型对照

游戏源码 [sts2/src/Core/Localization/DynamicVars/](sts2/src/Core/Localization/DynamicVars/) 中的可用类型：

| 数值类型 | 使用 | 示例 |
|---------|------|------|
| `DamageVar` | 伤害（有预览修正） | `new DamageVar(4, ValueProp.Move)` |
| `BlockVar` | 格挡（有预览修正） | `new BlockVar(5, ValueProp.Move)` |
| `CardsVar` | 抽牌 | `new CardsVar(2)` |
| `HealVar` | 治疗 | `new HealVar(3)` |
| `IntVar` | 通用数值（印记层数、层数等） | `new IntVar("Weak", 2)` |
| `EnergyVar` | 能量 | `new EnergyVar("Energy", 1)` |

- `DamageVar` / `BlockVar` 重写了 `UpdateCardPreview`，工具提示会显示附魔/Hook 修正后的实际值
- `IntVar` / `CardsVar` 无预览逻辑，只显示基础值
- 多段伤害/格挡后续段用自定义名：`new DamageVar("Dam2", 6, ValueProp.Move)`、`new BlockVar("Block2", 3, ValueProp.Move)`

## 印记系统 (Seal Element Marks)

印记系统有两档：**调谐**（Tuning，≥3层）和**超限**（Overload，5层满）。每回合结束自动检查并触发。

核心常量（[Combat/ElementMarkManager.cs](Combat/ElementMarkManager.cs)）：
- `MaxSealStacks = 5`（层数上限）
- `ThresholdStacks = 3`（调谐阈值）
- 两个槽位：`MarkSlot.Primary`、`MarkSlot.Secondary`

### 命令（[Commands/SealElementMarkCmd.cs](Commands/SealElementMarkCmd.cs) + [Commands/StarbornCmd.cs](Commands/StarbornCmd.cs)）

| 操作 | API | 说明 |
|------|-----|------|
| 切换属性 | `SealElementMarkCmd.SetElementType(ctx, slot, player, type)` | 改变印记的元素类型 |
| 叠加层数 | `SealElementMarkCmd.GainElementMarks(ctx, slot, player, stacks)` | 增加印记层数 |
| 移除层数 | `SealElementMarkCmd.RemoveElementMarks(ctx, slot, player, stacks)` | 减少印记层数 |
| 调谐 | `StarbornCmd.Tuning(ctx, slot, player, consume, source)` | 消耗 consume 层触发调谐 |
| 调谐+切换 | `StarbornCmd.Tuning(ctx, slot, player, consume, targetElement, source)` | 先切换到 targetElement 再调谐 |
| 超限 | `StarbornCmd.Overload(ctx, slot, player, consume, source)` | 消耗 consume 层触发超限 |
| 超限+切换 | `StarbornCmd.Overload(ctx, slot, player, consume, targetElement, source)` | 先切换再超限 |

### 卡牌父类属性（[Cards/StarbornCard.cs](Cards/StarbornCard.cs)）

```csharp
PrimaryStacks    // int，主印记当前层数（Canonical 模式下返回 0）
SecondaryStacks  // int，副印记当前层数
PrimaryElementType    // SealElementType，主印记元素类型
SecondaryElementType  // SealElementType，副印记元素类型
```

### 可打出性检查

- `StarbornCmd.CanTuning(player, slot)` — 元素已设置 且 层数 ≥ 调谐消耗（默认1）
- `StarbornCmd.CanOverload(player, slot)` — 层数 ≥ 阈值(3) 且 层数 ≥ 超限消耗（默认2）
- `PrimaryStacks > 0` — 简单检查印记是否已激活

在卡牌中：
```csharp
protected override bool IsPlayable =>
    StarbornCmd.CanTuning(Owner, MarkSlot.Primary);
```

### DynamicVar 工厂方法

用 `StarbornCardVars`（固定元素）或父类 `Tuning/Overload/ElementMark()` 方法（支持 `Any`）：

| 用途 | 固定元素 | 支持 Any |
|------|---------|----------|
| 印记层数 | `StarbornCardVars.ElementMark(stacks, type)` | `ElementMark(stacks, type)` |
| 调谐消耗 | `StarbornCardVars.Tuning(stacks, type)` | `Tuning(stacks, type)` |
| 超限消耗 | `StarbornCardVars.Overload(stacks, type)` | `Overload(stacks, type)` |

- **固定元素**（如 `SealElementType.Fire`）：描述用 `{ElementMark:elementIcon()}` 渲染对应元素图标
- **SealElementType.Any**：必须用父类方法 `Tuning(stacks, SealElementType.Any)` 等方法，它们会创建 `SealElementVar` 并设 `ResolveFromCurrentMark = true`——预览显示 Any 图标，战斗中动态解析为当前印记的真实元素
- 描述中**禁止**手写 `[gold]火属性[/gold]`、`[gold]任意元素[/gold]` 代替图标

### 典型卡牌模式

**切换印记 + 叠加层数**（[SwitchPrimaryMarkCard](Cards/Common/SwitchPrimaryMarkCard.cs)）：
```csharp
// DynamicVar: StarbornCardVars.ElementMark(2, SealElementType.Fire)
await SealElementMarkCmd.SetElementType(ctx, MarkSlot.Primary, Owner, SealElementType.Fire);
await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Primary, Owner, DynamicVars["ElementMark"].IntValue);
// 升级: DynamicVars["ElementMark"].BaseValue += 1
```

**调谐卡**（[TuningCard](Cards/Common/TuningCard.cs)）：
```csharp
// IsPlayable: StarbornCmd.CanTuning(Owner, MarkSlot.Primary)
// DynamicVar: StarbornCardVars.Tuning(2, SealElementType.Fire)
await StarbornCmd.Tuning(ctx, MarkSlot.Primary, Owner, DynamicVars["Tuning"].IntValue, SealElementType.Fire, this);
```

**超限卡**（[OverloadCard](Cards/Uncommon/OverloadCard.cs)）：
```csharp
// IsPlayable: PrimaryStacks > 0
// 先补足到满层 → 超限 → 补剩余层
await SealElementMarkCmd.SetElementType(ctx, MarkSlot.Primary, Owner, SealElementType.Fire);
var need = ElementMarkManager.ThresholdStacks - PrimaryStacks;
if (need > 0) await SealElementMarkCmd.GainElementMarks(..., need);
await StarbornCmd.Overload(ctx, MarkSlot.Primary, Owner, 1, this);
```

**印记平衡**（[AlignmentCard](Cards/Uncommon/AlignmentCard.cs)）：
```csharp
// 比较主/副印记层数，补齐较低一方（只操作 GainElementMarks，不切换属性）
var diff = primaryStacks - secondaryStacks;
if (diff > 0) await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Secondary, Owner, diff);
```

### Hook 接口（能力/遗物使用）

| 接口 | 用途 | 文件 |
|------|------|------|
| `ISealElementMarkListener` | 监听元素变化，可阻止切换 | [Hooks/ISealElementMarkListener.cs](Hooks/ISealElementMarkListener.cs) |
| `ITuningOverloadListener` | 监听调谐/超限前后 | [Hooks/ITuningOverloadListener.cs](Hooks/ITuningOverloadListener.cs) |
| `IConsumeModifier` | 修改调谐/超限的层数消耗 | [Hooks/IConsumeModifier.cs](Hooks/IConsumeModifier.cs) |

## 卡牌 description 规则
- 始终用 `{VarName:diff()}` 显示可升级数值，不要硬编码
- 升级改变**文字内容**时用 `{IfUpgraded:show:升级版|基础版}`，原版**没有** upgradeDescription 这个 key
- 升级**减少**数值时用 `{VarName:inverseDiff()}`
- 战斗内实时数值用 `{InCombat:\n（造成{CalculatedDamage}点伤害）|}` — 非战斗时隐藏
- 多行用 `\n` 分隔，每句以 `。` 结尾
- 标准句式：
  - 伤害：`造成{Damage:diff()}点伤害。`
  - 格挡：`获得{Block:diff()}点[gold]格挡[/gold]。`
  - 状态：`给予{Weak:diff()}层[gold]虚弱[/gold]。`
  - 抽牌：`抽{Cards:diff()}张牌。`

## 能力 description 规则
- 能力需要**两个** key：`description`（静态，硬编码基础值）和 `smartDescription`（动态，`{Amount}` 占位）
- `description`：`前4张攻击牌额外触发调谐1/1……`
- `smartDescription`：`前[blue]{Amount}[/blue]张攻击牌额外触发调谐1/1……`
- 数值用 `[blue]` 包裹：`[blue]1[/blue]`（静态）或 `[blue]{Amount}[/blue]`（动态）
- 参照 [sts2/localization/zhs/powers.json](sts2/localization/zhs/powers.json) 已有能力的写法
