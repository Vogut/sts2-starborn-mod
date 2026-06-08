# DynamicVar Type Reference

游戏源码 [sts2/src/Core/Localization/DynamicVars/](sts2/src/Core/Localization/DynamicVars/) 中的可用类型：

## 类型速查

| 数值类型 | 使用 | 示例 | 访问方式 |
|---------|------|------|---------|
| `DamageVar` | 伤害（有预览修正） | `new DamageVar(4, ValueProp.Move)` | `DynamicVars.Damage` |
| `BlockVar` | 格挡（有预览修正） | `new BlockVar(5, ValueProp.Move)` | `DynamicVars.Block` |
| `CardsVar` | 抽牌 | `new CardsVar(2)` | `DynamicVars.Cards` |
| `EnergyVar` | 能量数值，配合 `{Name:energyIcons()}` 渲染角色能量图标 | `new EnergyVar(3)` | `DynamicVars.Energy` |
| `HealVar` | 治疗 | `new HealVar(3)` | `DynamicVars.Heal` |
| `RepeatVar` | 重复次数，配合 `WithHitCount()` 或 循环 等方法实现多段效果 | `new RepeatVar(4)` | `DynamicVars.Repeat` |
| `IntVar` | 通用数值（印记层数、层数等）——**仅在没有专用类型时使用** | `new IntVar("Weak", 2)` | `DynamicVars["Weak"]` |

## 严禁 IntVar 替代专用类型

```csharp
// ✗ Wrong — 能用专用类型却用 IntVar
new IntVar("Cards", 2)
new IntVar("Energy", 3)

// ✓ Correct — 使用官方专用类型
new CardsVar(2)
new EnergyVar(3)
```

**原因**：`CardsVar`、`EnergyVar` 等专用类型附带正确的工具提示格式和预览行为。`IntVar` 只有裸数字，缺少这些内置行为。仅在没有对应专用类型时（如重复次数 `new IntVar("Repeat", 3)`）才回退到 `IntVar`。

## 预览行为

- `DamageVar` / `BlockVar` 重写了 `UpdateCardPreview`，工具提示会显示附魔/Hook 修正后的实际值
- `IntVar` / `CardsVar` 无预览逻辑，只显示基础值

## 多段伤害/格挡

后续段用自定义名：
```csharp
new DamageVar("Dam2", 6, ValueProp.Move)
new BlockVar("Block2", 3, ValueProp.Move)
```

## 印记相关 DynamicVar

印记 DynamicVar 通过 `StarbornCardVars` 工厂或父类 `StarbornCard` 的 helper 方法创建。**不要手写 `IntVar`**：

```csharp
// 固定元素
StarbornCardVars.ElementMark(4, SealElementType.Light)   // 印记层数
StarbornCardVars.Tuning(2, SealElementType.Fire)          // 调谐消耗
StarbornCardVars.Overload(1, SealElementType.Fire)        // 超限消耗

// 当前元素（Any — 战斗中动态解析）
ElementMark(1, SealElementType.Any)   // 父类方法，ResolveFromCurrentMark = true
Tuning(1, SealElementType.Any)
Overload(1, SealElementType.Any)
```

`StarbornCardVars.IfCanOverload()` — 可选的条件变量，配合 JSON `{IfCanOverload:超限文本|调谐文本}` 在印记 >3 层时自动切换卡面文字。

## 元素类型引用——禁止硬编码

当卡牌始终操作固定元素时，在 `CanonicalVars` 定义一次，`OnPlay` 从 `SealElementVar.ElementType` 提取，**不要在 OnPlay 中重复写 `SealElementType.Xxx`**：

```csharp
// ✓ Correct
var elementType = ((SealElementVar)DynamicVars["ElementMark"]).ElementType;
await SealElementMarkCmd.GainElementMarks(ctx, slot, Owner, stacks, elementType);

// ✗ Wrong
await SealElementMarkCmd.GainElementMarks(ctx, slot, Owner, stacks, SealElementType.Light);
```

## AdditionalHoverTips 紧凑显示规则

当卡牌的 `AdditionalHoverTips` 引用 **≥4 张牌预览**（`HoverTipFactory.FromCard`）时，必须使用 `CompactCardGridHoverTip` 打包为 2 列紧凑网格，避免垂直堆叠溢出屏幕。

```csharp
// ✓ Correct — 4+ 张牌用 CompactCardGridHoverTip 打包
using STS2_Starborn.UI;

protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
[
    HoverTipFactory.FromPower<MyPower>(),
    new CompactCardGridHoverTip(
        TokenCardTypes.Select(t => ModelDb.GetById<CardModel>(ModelDb.GetId(t)))),
];

// ✗ Wrong — 4+ 张牌逐个返回，垂直堆叠超出屏幕
protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    new[] { HoverTipFactory.FromPower<MyPower>() }
        .Concat(TokenCardTypes.Select(t => HoverTipFactory.FromCard(
            ModelDb.GetById<CardModel>(ModelDb.GetId(t)))));
```

**规则**：
- 1~3 张牌：直接返回 `HoverTipFactory.FromCard` / `FromPower` 等
- ≥4 张牌：必须用 `new CompactCardGridHoverTip(cards)` 打包
- Power / Relic / Potion 等文本提示不计入牌数，只计算 `FromCard` 的数量

### Kibo 召唤牌专用简化

Kibo 召唤牌（有 `KiboSummonType` 的牌）直接使用 `KiboTypeDefinition.CreateCompactCardGridHoverTips()`，已内置 RepCard + 所有能力牌的打包：

```csharp
// ✓ Correct — Kibo 牌一行搞定
protected override IEnumerable<IHoverTip> AdditionalHoverTips
{
    get
    {
        yield return HoverTipFactory.FromPower<WolfPackPower>();
        yield return KiboTypeRegistry.Get(KiboTypeId.SwiftWolf)
            .CreateCompactCardGridHoverTips();
    }
}
```
