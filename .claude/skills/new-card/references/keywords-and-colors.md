# Keywords & Color Conventions

## 关键词规则

- 消耗、保留、固有、虚无等原版关键词和自定义关键词（`RegisterOwnedCardKeyword`）通过 `CanonicalKeywords` 属性以**图标**形式显示在卡面上
- description 中**不要**写 `[gold]消耗[/gold]`、`[gold]保留[/gold]` 等关键词图标文本——图标已由 `CanonicalKeywords` 渲染
- 参照原版 HOTFIX：base 有 Exhaust，description 只写效果文本，升级 `RemoveKeyword` 去掉图标
- **例外**：在描述中引用某个状态/机制名时仍需 `[gold]` 包裹，如"给予[gold]虚弱[/gold]"——这是状态名不是关键词图标

## 颜色约定

| 标记 | 用途 | 示例 |
|------|------|------|
| `[gold]术语[/gold]` | 游戏术语 | 格挡、力量、虚弱、易伤、手牌、抽牌堆 |
| `[blue]数值[/blue]` | 数字 | 主要用于 smartDescription 的 `[blue]{Amount}[/blue]` |
| `[red]术语[/red]` | 敌对/警告性质术语 | — |

## 元素印记：必须用 Icon，禁止文字

卡牌描述中提到元素印记属性时，**必须**用 `{ElementMark:elementIcon()}` 渲染为元素图标：

```
正确: 将主/副属性固定为{ElementMark:elementIcon()}。
错误: 将主/副属性固定为[red]火[/red]。
错误: 将主/副属性固定为[gold]火属性[/gold]。
```

- `[gold]`、`[red]`、纯文本都**不允许**——一律用 `elementIcon()` 格式化器，没有例外
