# DynamicVar Type Reference

游戏源码 [sts2/src/Core/Localization/DynamicVars/](sts2/src/Core/Localization/DynamicVars/) 中的可用类型：

| 数值类型 | 使用 | 示例 |
|---------|------|------|
| `DamageVar` | 伤害（有预览修正） | `new DamageVar(4, ValueProp.Move)` |
| `BlockVar` | 格挡（有预览修正） | `new BlockVar(5, ValueProp.Move)` |
| `CardsVar` | 抽牌 | `new CardsVar(2)` |
| `HealVar` | 治疗 | `new HealVar(3)` |
| `IntVar` | 通用数值（印记层数、层数等） | `new IntVar("Weak", 2)` |
| `EnergyVar` | 能量数值，配合 `{Name:energyIcons()}` 渲染角色能量图标 | `new EnergyVar(3)` |

## 预览行为

- `DamageVar` / `BlockVar` 重写了 `UpdateCardPreview`，工具提示会显示附魔/Hook 修正后的实际值
- `IntVar` / `CardsVar` 无预览逻辑，只显示基础值

## 多段伤害/格挡

后续段用自定义名：
```csharp
new DamageVar("Dam2", 6, ValueProp.Move)
new BlockVar("Block2", 3, ValueProp.Move)
```
