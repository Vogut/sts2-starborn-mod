# Seal Element Mark System

印记系统有两档：**调谐**（Tuning，≥3层）和**超限**（Overload，5层满）。每回合结束自动检查并触发。

## Core Constants

[Combat/ElementMarkManager.cs](Combat/ElementMarkManager.cs)：
- `MaxSealStacks = 5`（层数上限）
- `ThresholdStacks = 3`（调谐阈值）
- 两个槽位：`MarkSlot.Primary`、`MarkSlot.Secondary`

## Commands

[Commands/SealElementMarkCmd.cs](Commands/SealElementMarkCmd.cs) + [Commands/StarbornCmd.cs](Commands/StarbornCmd.cs)：

| 操作 | API | 说明 |
|------|-----|------|
| 切换属性 | `SealElementMarkCmd.SetElementType(ctx, slot, player, type)` | 改变印记的元素类型 |
| 叠加层数 | `SealElementMarkCmd.GainElementMarks(ctx, slot, player, stacks)` | 增加印记层数 |
| 切换+叠加 | `SealElementMarkCmd.GainElementMarks(ctx, slot, player, stacks, elementType)` | 先切换元素再叠加（推荐） |
| 移除层数 | `SealElementMarkCmd.RemoveElementMarks(ctx, slot, player, stacks)` | 减少印记层数 |
| 调谐 | `StarbornCmd.Tuning(ctx, slot, player, consume, source)` | 消耗 consume 层触发调谐 |
| 调谐+切换 | `StarbornCmd.Tuning(ctx, slot, player, consume, targetElement, source)` | 先切换到 targetElement 再调谐 |
| 超限 | `StarbornCmd.Overload(ctx, slot, player, consume, source)` | 消耗 consume 层触发超限 |
| 超限+切换 | `StarbornCmd.Overload(ctx, slot, player, consume, targetElement, source)` | 先切换再超限 |

### GainElementMarks 重载（推荐）

新增重载 `GainElementMarks(ctx, slot, player, stacks, elementType)` 内部先调用 `SetElementType` 再叠加，一步到位。有元素类型参数时应优先用这个重载，而不是分开调用 `SetElementType` + `GainElementMarks`。

## 卡牌父类属性

[Cards/StarbornCard.cs](Cards/StarbornCard.cs)：

```csharp
PrimaryStacks           // int，主印记当前层数（Canonical 模式下返回 0）
SecondaryStacks         // int，副印记当前层数
PrimaryElementType      // SealElementType，主印记元素类型
SecondaryElementType    // SealElementType，副印记元素类型
```

## 可打出性检查

```csharp
StarbornCmd.CanTuning(player, slot)   // 元素已设置 且 层数 ≥ 调谐消耗（默认1）
StarbornCmd.CanOverload(player, slot) // 层数 ≥ 阈值(3) 且 层数 ≥ 超限消耗（默认2）
PrimaryStacks > 0                     // 简单检查印记是否已激活
```

在卡牌中：
```csharp
protected override bool IsPlayable =>
    StarbornCmd.CanTuning(Owner, MarkSlot.Primary);
```

## DynamicVar 工厂方法

用 `StarbornCardVars`（固定元素）或父类 `Tuning/Overload/ElementMark()` 方法（支持 `Any`）：

| 用途 | 固定元素 | 支持 Any |
|------|---------|----------|
| 印记层数 | `StarbornCardVars.ElementMark(stacks, type)` | `ElementMark(stacks, type)` |
| 调谐消耗 | `StarbornCardVars.Tuning(stacks, type)` | `Tuning(stacks, type)` |
| 超限消耗 | `StarbornCardVars.Overload(stacks, type)` | `Overload(stacks, type)` |

- **固定元素**（如 `SealElementType.Light`）：描述用 `{ElementMark:elementIcon()}` 渲染对应元素图标
- **SealElementType.Any**：必须用父类方法 `Tuning(stacks, SealElementType.Any)` 等方法，它们会创建 `SealElementVar` 并设 `ResolveFromCurrentMark = true`——预览显示 Any 图标，战斗中动态解析为当前印记的真实元素
- 描述中**禁止**手写任何形式的元素文字代替图标：`[gold]火属性[/gold]`、`[red]火[/red]`、`[gold]水[/gold]` 等一律不行——**必须**用 `{ElementMark:elementIcon()}`（包括 Any 也要用 icon）

## 元素类型引用——禁止硬编码

`SealElementType` 在 `CanonicalVars` 通过 `StarbornCardVars.ElementMark(4, SealElementType.Light)` 已定义一次。**OnPlay 中不要重复写 `SealElementType.Light`**，应从 `SealElementVar.ElementType` 提取：

```csharp
// ✓ Correct — 元素类型从 DynamicVar 提取，只定义一次
var elementType = ((SealElementVar)DynamicVars["ElementMark"]).ElementType;
await SealElementMarkCmd.GainElementMarks(ctx, slot, Owner, stacks, elementType);
await StarbornCmd.Overload(ctx, slot, Owner, consume, elementType, this);

// ✗ Wrong — 元素类型硬编码
await StarbornCmd.Overload(ctx, slot, Owner, 1, SealElementType.Light, this);
```

## 典型卡牌模式

### 切换印记 + 叠加层数

参照 [SwitchPrimaryMarkCard](Cards/Common/SwitchPrimaryMarkCard.cs)：
```csharp
// DynamicVar: StarbornCardVars.ElementMark(2, SealElementType.Fire)
// ★ 推荐：用 GainElementMarks 重载一步完成
var et = ((SealElementVar)DynamicVars["ElementMark"]).ElementType;
await SealElementMarkCmd.GainElementMarks(ctx, slot, Owner, DynamicVars["ElementMark"].IntValue, et);
// 升级: DynamicVars["ElementMark"].BaseValue += 1
```

### 调谐卡

参照 [TuningCard](Cards/Common/TuningCard.cs)：
```csharp
// IsPlayable: StarbornCmd.CanTuning(Owner, MarkSlot.Primary)
// DynamicVar: StarbornCardVars.Tuning(2, SealElementType.Fire)
var et = ((SealElementVar)DynamicVars["Tuning"]).ElementType;
await StarbornCmd.Tuning(ctx, MarkSlot.Primary, Owner, DynamicVars["Tuning"].IntValue, et, this);
```

### 超限卡

参照 [OverloadCard](Cards/Uncommon/OverloadCard.cs)：
```csharp
// IsPlayable: StarbornCmd.CanOverload(Owner, MarkSlot.Primary)
// DynamicVar: StarbornCardVars.Overload(2, SealElementType.Fire)
var et = ((SealElementVar)DynamicVars["Overload"]).ElementType;
var need = ElementMarkManager.ThresholdStacks - PrimaryStacks;
if (need > 0) await SealElementMarkCmd.GainElementMarks(ctx, slot, Owner, need, et);
await StarbornCmd.Overload(ctx, MarkSlot.Primary, Owner, DynamicVars["Overload"].IntValue, et, this);
```

### 印记平衡

参照 [AlignmentCard](Cards/Uncommon/AlignmentCard.cs)：
```csharp
// 比较主/副印记层数，补齐较低一方（只操作 GainElementMarks，不切换属性）
var diff = primaryStacks - secondaryStacks;
if (diff > 0) await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Secondary, Owner, diff);
```

### 同时消耗主/副印记

参照 [StarbornCoffeeCard](Cards/Uncommon/StarbornCoffeeCard.cs)：
```csharp
// DynamicVar: StarbornCardVars.ElementMark(1, SealElementType.Any)
// IsPlayable: PrimaryStacks >= 1 && SecondaryStacks >= 1
await SealElementMarkCmd.RemoveElementMarks(ctx, MarkSlot.Primary, Owner, 1);
await SealElementMarkCmd.RemoveElementMarks(ctx, MarkSlot.Secondary, Owner, 1);
```
- 描述用 `{ElementMark:elementIcon()}/{ElementMark:elementIcon()}` — 一个 `Any` 类型的 `ElementMark` var 引用两次，中间用 `/` 分隔，分别对应主/副印记各消耗 1 层

## Hook Interfaces（能力/遗物使用）

| 接口 | 用途 | 文件 |
|------|------|------|
| `ISealElementMarkListener` | 监听元素变化，可阻止切换 | [Hooks/ISealElementMarkListener.cs](Hooks/ISealElementMarkListener.cs) |
| `ITuningOverloadListener` | 监听调谐/超限前后 | [Hooks/ITuningOverloadListener.cs](Hooks/ITuningOverloadListener.cs) |
| `IConsumeModifier` | 修改调谐/超限的层数消耗 | [Hooks/IConsumeModifier.cs](Hooks/IConsumeModifier.cs) |
