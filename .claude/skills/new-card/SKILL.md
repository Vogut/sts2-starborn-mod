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
