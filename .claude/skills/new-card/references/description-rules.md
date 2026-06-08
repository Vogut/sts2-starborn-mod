# Card & Power Description Rules

## 卡牌 description 规则

- 始终用 `{VarName:diff()}` 显示可升级数值，不要硬编码
- 升级改变**文字内容**时用 `{IfUpgraded:show:升级版|基础版}`，原版**没有** upgradeDescription 这个 key
- 印记 >3 层时调谐显示为超限：用 `{IfCanOverload:[gold]超限{Overload:elementIcon()}[/gold]|[gold]调谐{Tuning:elementIcon()}[/gold]}`（需卡牌声明 `IfCanOverloadVar` + `Overload` var）
- **不要**将 `{IfCanOverload:...|...}` 嵌套在 `{IfUpgraded:show:...|}` 内部——`|` 分隔符会冲突
- 升级**减少**数值时用 `{VarName:inverseDiff()}`
- 战斗内实时数值用 `{InCombat:\n（造成{CalculatedDamage}点伤害）|}` — 非战斗时隐藏
- 多行用 `\n` 分隔，每句以 `。` 结尾

### 标准句式

参照原版 [sts2/localization/zhs/cards.json](sts2/localization/zhs/cards.json)：

| 效果 | 句式 |
|------|------|
| 伤害 | `造成{Damage:diff()}点伤害。` |
| 格挡 | `获得{Block:diff()}点[gold]格挡[/gold]。` |
| 能量 | `获得{Energy:energyIcons()}。` |
| 抽牌 | `抽{Cards:diff()}张牌。` |
| 状态 | `给予{Weak:diff()}层[gold]虚弱[/gold]。` |
| 耗能展示 | `0{energyPrefix:energyIcons(1)}` |
| 生命失去 | `失去{HpLoss:diff()}点生命。` |
| 生命上限 | `失去{MaxHp:diff()}点最大生命。` |
| 治疗 | `回复{Heal:diff()}点生命。` |

### 元素印记 Icon

元素印记属性在描述中**必须**渲染为图标，**禁止**用 `[gold]`/`[red]`/纯文本：

```
正确: 造成{Damage:diff()}点伤害。触发{ElementMark:elementIcon()}调谐。
错误: 造成{Damage:diff()}点伤害。触发[gold]火属性[/gold]调谐。
错误: 造成{Damage:diff()}点伤害。触发[red]火[/red]调谐。
```

- `{ElementMark:elementIcon()}` 自动渲染对应元素图标
- 所有元素类型一律用 icon，没有例外

### 格式化器选择

- `diff()` — 数值差异化显示（伤害、格挡、抽牌等通用值）
- `energyIcons()` — 能量图标专用，渲染角色对应颜色的能量图标；**禁止**手写 `[blue]N[/blue]点能量`
- `starIcons()` — 星能量图标专用
- 区分原则：看原版同类卡牌用哪个格式化器就照搬

## 能力 description 规则

能力需要**两个** key：`description`（静态，硬编码基础值）和 `smartDescription`（动态，`{Amount}` 占位）：

```json
"description": "前4张攻击牌额外触发调谐1/1……",
"smartDescription": "前[blue]{Amount}[/blue]张攻击牌额外触发调谐1/1……"
```

- 静态数值用 `[blue]` 包裹：`[blue]1[/blue]`
- 动态数值用 `[blue]{Amount}[/blue]`
- 参照 [sts2/localization/zhs/powers.json](sts2/localization/zhs/powers.json) 已有能力的写法
