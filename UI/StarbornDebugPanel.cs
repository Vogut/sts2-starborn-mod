#if DEBUG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Ui.Shell;
using STS2RitsuLib.Ui.Shell.Theme;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.UI;

public partial class StarbornDebugPanel : Control
{
    private TabContainer _tabs = null!;
    private LineEdit _cardSearch = null!;
    private ItemList _cardList = null!;
    private LineEdit _powerSearch = null!;
    private ItemList _powerList = null!;
    private LineEdit _relicSearch = null!;
    private ItemList _relicList = null!;

    private static CardModel[]? _allCards;
    private static PowerModel[]? _allPowers;
    private static RelicModel[]? _allRelics;

    public override void _Ready()
    {
        ZIndex = 999;
        MouseFilter = MouseFilterEnum.Ignore;
        Visible = false;
        // Must fill NGame (parent Control) so anchored children get real screen dimensions.
        // Without this the panel has 0×0 size and all tab content is clipped invisible.
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        BuildPanel();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!Visible) return;
        if (@event is InputEventKey { Pressed: true, KeyLabel: Key.Escape })
        {
            Visible = false;
            MouseFilter = MouseFilterEnum.Ignore;
            GetViewport().SetInputAsHandled();
        }
    }

    public void ToggleVisibility()
    {
        Visible = !Visible;
        MouseFilter = Visible ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
        if (Visible)
        {
            try
            {
                // Defer data load to panel-open time: ModelDb is guaranteed ready by now.
                CacheModels();
                RefreshCardList();
                RefreshPowerList();
                RefreshRelicList();
            }
            catch (Exception e)
            {
                GD.PrintErr($"[StarbornDebugPanel] ToggleVisibility failed: {e}");
            }
        }
    }

    private ColorRect _backdrop = null!;

    private static void CacheModels()
    {
        // ModelDb.AllCards / AllRelics only cover vanilla pools (AllCharacters is hardcoded).
        // ModelDb.AllAbstractModelSubtypes uses ReflectionHelper.ModTypes which may be cached
        // before Starborn is registered via RitsuLib — so it also misses mod types.
        // Solution: enumerate the Starborn assembly directly and merge with vanilla results.
        var modAsm = typeof(StarbornDebugPanel).Assembly;

        var starbornCards = modAsm.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(CardModel)))
            .Select(t => ModelDb.GetByIdOrNull<CardModel>(ModelDb.GetId(t)))
            .OfType<CardModel>();

        _allCards = (ModelDb.AllCards ?? [])
            .Concat(starbornCards)
            .GroupBy(c => c.Id).Select(g => g.First())
            .OrderBy(c => c.Id.Entry)
            .ToArray();

        var starbornPowers = modAsm.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(PowerModel)))
            .Select(t => ModelDb.GetByIdOrNull<PowerModel>(ModelDb.GetId(t)))
            .OfType<PowerModel>();
        _allPowers = (ModelDb.AllPowers ?? [])
            .Concat(starbornPowers)
            .GroupBy(p => p.Id).Select(g => g.First())
            .OrderBy(p => p.Id.Entry)
            .ToArray();

        var starbornRelics = modAsm.GetTypes()
            .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(RelicModel)))
            .Select(t => ModelDb.GetByIdOrNull<RelicModel>(ModelDb.GetId(t)))
            .OfType<RelicModel>();
        _allRelics = (ModelDb.AllRelics ?? [])
            .Concat(starbornRelics)
            .GroupBy(r => r.Id).Select(g => g.First())
            .OrderBy(r => r.Id.Entry)
            .ToArray();
    }

    // ── Panel structure ──────────────────────────────────────

    private void BuildPanel()
    {
        var margin = 80;
        var t = RitsuShellTheme.Current;

        _backdrop = new ColorRect
        {
            Color = t.Color.ModalBackdrop,
            AnchorLeft = 0, AnchorTop = 0, AnchorRight = 1, AnchorBottom = 1,
            MouseFilter = MouseFilterEnum.Ignore,
        };
        AddChild(_backdrop);

        var root = new MarginContainer
        {
            AnchorLeft = 0, AnchorTop = 0, AnchorRight = 1, AnchorBottom = 1,
        };
        root.AddThemeConstantOverride("margin_left", margin);
        root.AddThemeConstantOverride("margin_top", 60);
        root.AddThemeConstantOverride("margin_right", margin);
        root.AddThemeConstantOverride("margin_bottom", 60);
        AddChild(root);

        // Framed panel card — gives the panel rounded corners, border and shadow.
        var panel = new PanelContainer { SizeFlagsVertical = Control.SizeFlags.ExpandFill };
        panel.AddThemeStyleboxOverride("panel",
            RitsuShellPanelStyles.CreateFramedSurface(t.Component.OverlayPanel.Bg, t.Metric.Radius.Overlay));
        root.AddChild(panel);

        var innerMargin = new MarginContainer();
        innerMargin.AddThemeConstantOverride("margin_left", 16);
        innerMargin.AddThemeConstantOverride("margin_top", 12);
        innerMargin.AddThemeConstantOverride("margin_right", 16);
        innerMargin.AddThemeConstantOverride("margin_bottom", 12);
        panel.AddChild(innerMargin);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        innerMargin.AddChild(vbox);

        vbox.AddChild(BuildTitleBar());

        _tabs = new TabContainer
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
        };
        vbox.AddChild(_tabs);

        _tabs.AddChild(BuildCombatTab());
        _tabs.AddChild(BuildCardsTab());
        _tabs.AddChild(BuildPowersTab());
        _tabs.AddChild(BuildRelicsTab());
        _tabs.AddChild(BuildKiboTab());
        _tabs.AddChild(BuildElementTab());
    }

    private Control BuildTitleBar()
    {
        var t = RitsuShellTheme.Current;
        var bar = new HBoxContainer { LayoutMode = 1 };
        bar.AddThemeConstantOverride("separation", 8);

        var title = new Label
        {
            Text = "Starborn Dev Panel",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        title.AddThemeColorOverride("font_color", t.Text.RichTitle);
        title.AddThemeFontOverride("font", t.Font.BodyBold);
        title.AddThemeFontSizeOverride("font_size", t.Metric.FontSize.HeaderTitle);
        bar.AddChild(title);

        var self = this;
        var closeBtn = new Button { Text = "×", LayoutMode = 1 };
        ApplyButtonStyle(closeBtn);
        closeBtn.Pressed += () => { self.Visible = false; self._backdrop.MouseFilter = Control.MouseFilterEnum.Ignore; };
        bar.AddChild(closeBtn);

        bar.AddChild(new HSeparator { LayoutMode = 1 });
        return bar;
    }

    // ── Combat Tab ───────────────────────────────────────────

    private ScrollContainer BuildCombatTab()
    {
        var (scroll, content) = MakeTabScroll("战斗");

        AddSectionTitle(content, "快捷操作");
        AddButtonRow(content,
            ("+3 能量", () => SafeExec(() => DoGainEnergy())),
            ("+5 抽牌", () => SafeExec(() => DoDrawCards())),
            ("回满血", () => SafeExec(() => DoHealFull())),
            ("秒杀全体", () => SafeExec(() => DoKillAll()))
        );

        return scroll;
    }

    // ── Cards Tab ────────────────────────────────────────────

    private ScrollContainer BuildCardsTab()
    {
        var (scroll, content) = MakeTabScroll("卡牌");

        _cardSearch = new LineEdit
        {
            PlaceholderText = "搜索卡牌名或 ID...",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        ApplyLineEditStyle(_cardSearch);
        _cardSearch.TextChanged += _ => RefreshCardList();
        content.AddChild(_cardSearch);

        _cardList = new ItemList
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 300),
        };
        ApplyItemListStyle(_cardList);
        content.AddChild(_cardList);

        AddSectionTitle(content, "添加选中卡牌");
        AddButtonRow(content,
            ("加入手牌",   () => SafeExec(() => DoAddCardTo(PileType.Hand))),
            ("加入卡组",   () => SafeExec(() => DoAddCardTo(PileType.Draw))),
            ("加入弃牌堆", () => SafeExec(() => DoAddCardTo(PileType.Discard)))
        );

        RefreshCardList();
        return scroll;
    }

    private void RefreshCardList()
    {
        if (_allCards == null) return;
        _cardList.Clear();
        var filter = (_cardSearch.Text ?? "").Trim();
        var matches = _allCards!
            .Where(c => string.IsNullOrEmpty(filter) ||
                        c.Id.Entry.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        c.Title.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Take(80);
        foreach (var c in matches)
            _cardList.AddItem($"{c.Title}  [{c.Id.Entry}]");
    }

    // ── Powers Tab ───────────────────────────────────────────

    private bool _powerTargetSelf = true;
    private int _powerStacks = 1;

    private ScrollContainer BuildPowersTab()
    {
        var (scroll, content) = MakeTabScroll("Power");

        _powerSearch = new LineEdit
        {
            PlaceholderText = "搜索 Power 名或 ID...",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        ApplyLineEditStyle(_powerSearch);
        _powerSearch.TextChanged += _ => RefreshPowerList();
        content.AddChild(_powerSearch);

        _powerList = new ItemList
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 260),
        };
        ApplyItemListStyle(_powerList);
        content.AddChild(_powerList);

        AddSectionTitle(content, "施加选中 Power");

        // Target toggle
        var targetRow = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        targetRow.AddThemeConstantOverride("separation", 6);
        var targetLabel = new Label { Text = "目标：" };
        targetRow.AddChild(targetLabel);
        var btnSelf = new Button { Text = "自身", ToggleMode = true, ButtonPressed = true };
        var btnEnemy = new Button { Text = "敌人(第一个)", ToggleMode = true, ButtonPressed = false };
        ApplyButtonStyle(btnSelf);
        ApplyButtonStyle(btnEnemy);
        btnSelf.Pressed += () => { _powerTargetSelf = true;  btnSelf.ButtonPressed = true;  btnEnemy.ButtonPressed = false; };
        btnEnemy.Pressed += () => { _powerTargetSelf = false; btnEnemy.ButtonPressed = true; btnSelf.ButtonPressed = false; };
        targetRow.AddChild(btnSelf);
        targetRow.AddChild(btnEnemy);
        content.AddChild(targetRow);

        // Stack count
        var stackRow = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        stackRow.AddThemeConstantOverride("separation", 6);
        stackRow.AddChild(new Label { Text = "层数：" });
        var stackSpin = new SpinBox { MinValue = 1, MaxValue = 999, Value = _powerStacks, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        stackSpin.ValueChanged += v => _powerStacks = (int)v;
        stackRow.AddChild(stackSpin);
        content.AddChild(stackRow);

        AddButtonRow(content, ("施加", () => SafeExec(DoApplyPower)));

        RefreshPowerList();
        return scroll;
    }

    private void RefreshPowerList()
    {
        if (_allPowers == null) return;
        _powerList.Clear();
        var filter = (_powerSearch.Text ?? "").Trim();
        var matches = _allPowers!
            .Where(p => string.IsNullOrEmpty(filter) ||
                        p.Id.Entry.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        p.Title.GetFormattedText().Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Take(80);
        foreach (var p in matches)
            _powerList.AddItem($"{p.Title.GetFormattedText()}  [{p.Id.Entry}]");
    }

    // ── Relics Tab ───────────────────────────────────────────

    private ScrollContainer BuildRelicsTab()
    {
        var (scroll, content) = MakeTabScroll("遗物");

        _relicSearch = new LineEdit
        {
            PlaceholderText = "搜索遗物名或 ID...",
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        ApplyLineEditStyle(_relicSearch);
        _relicSearch.TextChanged += _ => RefreshRelicList();
        content.AddChild(_relicSearch);

        _relicList = new ItemList
        {
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            CustomMinimumSize = new Vector2(0, 350),
        };
        ApplyItemListStyle(_relicList);
        _relicList.ItemActivated += idx => SafeExec(() => DoObtainRelic((int)idx));
        content.AddChild(_relicList);

        RefreshRelicList();
        return scroll;
    }

    private void RefreshRelicList()
    {
        if (_allRelics == null) return;
        _relicList.Clear();
        var filter = (_relicSearch.Text ?? "").Trim();
        var matches = _allRelics!
            .Where(r => string.IsNullOrEmpty(filter) ||
                        r.Id.Entry.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        r.Title.GetFormattedText().Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Take(80);
        foreach (var r in matches)
            _relicList.AddItem($"{r.Title.GetFormattedText()}  [{r.Id.Entry}]");
    }

    // ── Kibo Tab ─────────────────────────────────────────────

    private ScrollContainer BuildKiboTab()
    {
        var (scroll, content) = MakeTabScroll("奇波");

        AddSectionTitle(content, "召唤 / 切换");
        foreach (string typeId in KiboTypeId.All)
        {
            var captured = typeId;
            AddButton(content, KiboDisplayName(captured), () => SafeExec(() => DoSummonKibo(captured)));
        }

        AddSectionTitle(content, "批量操作");
        AddButton(content, "全部激活 (SwitchOnAll)", () => SafeExec(DoSwitchOnAll));
        AddButton(content, "随机切换", () => SafeExec(DoSummonRandom));
        return scroll;
    }

    /// <summary>
    /// 动态获取 Kibo 的本地化显示名：通过其 RepCard 的 CardModel.Title。
    /// 新增的 KiboType 并不需要修改此处。
    /// </summary>
    private static string KiboDisplayName(string id)
    {
        try
        {
            var repType = KiboTypeRegistry.Get(id).RepCardType;
            return ModelDb.GetByIdOrNull<CardModel>(ModelDb.GetId(repType))?.Title ?? id;
        }
        catch { return id; }
    }

    // ── Element Tab ──────────────────────────────────────────

    private SealElementType _primaryElement = SealElementType.Fire;
    private SealElementType _secondaryElement = SealElementType.None;
    private int _elementStacks = 3;

    private ScrollContainer BuildElementTab()
    {
        var (scroll, content) = MakeTabScroll("元素");

        AddSectionTitle(content, "主印记");
        var primaryDropdown = BuildElementDropdown(e => _primaryElement = e);
        content.AddChild(primaryDropdown);

        AddSectionTitle(content, "副印记");
        var secondaryDropdown = BuildElementDropdown(e => _secondaryElement = e);
        content.AddChild(secondaryDropdown);

        AddSectionTitle(content, "层数");
        var stackInput = new SpinBox
        {
            MinValue = 1,
            MaxValue = 99,
            Value = _elementStacks,
        };
        stackInput.ValueChanged += v => _elementStacks = (int)v;
        content.AddChild(stackInput);

        AddSectionTitle(content, "操作");
        AddButtonRow(content,
            ("设置印记+层数", () => SafeExec(DoSetElements)),
            ("仅 +层数", () => SafeExec(DoGainStacks)),
            ("触发谐调", () => SafeExec(DoTriggerTuning)),
            ("触发超限", () => SafeExec(DoTriggerOverload))
        );
        return scroll;
    }

    private static OptionButton BuildElementDropdown(Action<SealElementType> onChanged)
    {
        var dropdown = new OptionButton
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        var values = Enum.GetValues<SealElementType>();
        foreach (var et in values)
            dropdown.AddItem(ElementDisplayName(et));
        dropdown.ItemSelected += idx => onChanged(values[idx]);
        return dropdown;
    }

    /// <summary>
    /// 动态获取属性类型的本地化名称：通过 LocString 查询 powers 表中的 .title 条目。
    /// 新增属性只需在本地化 JSON 中添加对应 key 即可自动生效。
    /// </summary>
    private static string ElementDisplayName(SealElementType et)
    {
        if (et == SealElementType.Any)
            return et.ToString();

        var loc = SealElementLocalization.Title(et);
        return loc.Exists() ? loc.GetFormattedText() : et.ToString();
    }

    // ── Actions ──────────────────────────────────────────────

    private static async Task DoGainEnergy()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        await PlayerCmd.GainEnergy(3, player);
    }

    private static async Task DoDrawCards()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var ctx = new BlockingPlayerChoiceContext();
        await CardPileCmd.Draw(ctx, 5, player);
    }

    private static async Task DoHealFull()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var missing = player.Creature.MaxHp - player.Creature.CurrentHp;
        if (missing > 0)
            await CreatureCmd.Heal(player.Creature, missing);
    }

    private static async Task DoKillAll()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var enemies = player.Creature.CombatState?.Enemies.ToList();
        if (enemies == null) return;
        foreach (var enemy in enemies)
            await CreatureCmd.Kill(enemy);
    }

    private async Task DoAddCardTo(PileType pile)
    {
        var player = GetFirstPlayer();
        if (player == null) return;

        var selected = _cardList.GetSelectedItems();
        if (selected.Length == 0) return;
        var listIndex = selected[0];

        var filter = (_cardSearch.Text ?? "").Trim();
        var match = _allCards!
            .Where(c => string.IsNullOrEmpty(filter) ||
                        c.Id.Entry.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        c.Title.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Skip(listIndex)
            .FirstOrDefault();
        if (match == null) return;

        var instance = player.RunState.CreateCard(match, player);
        await CardPileCmd.Add(instance, pile, CardPilePosition.Random);
    }

    private async Task DoApplyPower()
    {
        var player = GetFirstPlayer();
        if (player == null) return;

        var selected = _powerList.GetSelectedItems();
        if (selected.Length == 0) return;
        var listIndex = selected[0];

        var filter = (_powerSearch.Text ?? "").Trim();
        var match = _allPowers!
            .Where(p => string.IsNullOrEmpty(filter) ||
                        p.Id.Entry.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        p.Title.GetFormattedText().Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Skip(listIndex)
            .FirstOrDefault();
        if (match == null) return;

        var ctx = new BlockingPlayerChoiceContext();
        var target = _powerTargetSelf
            ? player.Creature
            : player.Creature.CombatState?.Enemies.FirstOrDefault();
        if (target == null) return;
        await PowerCmd.Apply(ctx, match.ToMutable(), target, _powerStacks, player.Creature, null);
    }

    private async Task DoObtainRelic(int listIndex)
    {
        var player = GetFirstPlayer();
        if (player == null) return;

        var filter = (_relicSearch.Text ?? "").Trim();
        var match = _allRelics!
            .Where(r => string.IsNullOrEmpty(filter) ||
                        r.Id.Entry.Contains(filter, StringComparison.OrdinalIgnoreCase))
            .Skip(listIndex)
            .FirstOrDefault();
        if (match == null) return;

        await RelicCmd.Obtain(match.ToMutable(), player);
    }

    private static async Task DoSummonKibo(string typeId)
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var ctx = new BlockingPlayerChoiceContext();
        await KiboCmd.Summon(ctx, player, typeId);
    }

    private static async Task DoSwitchOnAll()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var ctx = new BlockingPlayerChoiceContext();
        await KiboCmd.SwitchOnAll(ctx, player);
    }

    private static async Task DoSummonRandom()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var ctx = new BlockingPlayerChoiceContext();
        await KiboCmd.SummonRandom(ctx, player);
    }

    private async Task DoSetElements()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var ctx = new BlockingPlayerChoiceContext();
        await SealElementMarkCmd.SetElementType(ctx, MarkSlot.Primary, player, _primaryElement);
        await SealElementMarkCmd.SetElementType(ctx, MarkSlot.Secondary, player, _secondaryElement);
        await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Primary, player, _elementStacks);
        await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Secondary, player, _elementStacks);
    }

    private async Task DoGainStacks()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var ctx = new BlockingPlayerChoiceContext();
        await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Primary, player, _elementStacks);
        await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Secondary, player, _elementStacks);
    }

    private static async Task DoTriggerTuning()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var ctx = new BlockingPlayerChoiceContext();
        var stacks = ElementMarkState.GetStacks(player, MarkSlot.Primary);
        if (stacks < ElementMarkState.ThresholdStacks) return;
        await StarbornCmd.Tuning(ctx, MarkSlot.Primary, player, stacks);
    }

    private static async Task DoTriggerOverload()
    {
        var player = GetFirstPlayer();
        if (player == null) return;
        var ctx = new BlockingPlayerChoiceContext();
        var stacks = ElementMarkState.GetStacks(player, MarkSlot.Primary);
        if (stacks < ElementMarkState.ThresholdStacks) return;
        await StarbornCmd.Overload(ctx, MarkSlot.Primary, player, stacks);
    }

    // ── Helpers ──────────────────────────────────────────────

    private static Player? GetFirstPlayer()
    {
        var combatState = CombatManager.Instance.DebugOnlyGetState();
        return combatState?.Players.FirstOrDefault();
    }

    private static async void SafeExec(Func<Task> action)
    {
        try { await action(); }
        catch (Exception e) { GD.PrintErr($"[StarbornDebugPanel] {e.Message}\n{e.StackTrace}"); }
    }

    // ── UI builders ──────────────────────────────────────────

    private static (ScrollContainer, VBoxContainer) MakeTabScroll(string name)
    {
        var scroll = new ScrollContainer
        {
            Name = name,
            SizeFlagsVertical = Control.SizeFlags.ExpandFill,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
        };
        var content = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        var margin = new MarginContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        margin.AddThemeConstantOverride("margin_top", 8);
        margin.AddThemeConstantOverride("margin_left", 12);
        margin.AddThemeConstantOverride("margin_right", 12);
        margin.AddThemeConstantOverride("margin_bottom", 4);
        margin.AddChild(content);
        scroll.AddChild(margin);
        return (scroll, content);
    }

    private static void AddSectionTitle(Container parent, string text)
    {
        var t = RitsuShellTheme.Current;
        var label = new Label { Text = text, LayoutMode = 1 };
        label.AddThemeColorOverride("font_color", t.Text.LabelSecondary);
        label.AddThemeFontOverride("font", t.Font.BodyBold);
        label.AddThemeFontSizeOverride("font_size", t.Metric.FontSize.SettingLineTitle);
        parent.AddChild(label);
        parent.AddChild(new HSeparator { LayoutMode = 1 });
    }

    private static void AddButton(Container parent, string text, Action onClick)
    {
        var btn = new Button
        {
            Text = text,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
        };
        ApplyButtonStyle(btn);
        btn.Pressed += onClick;
        parent.AddChild(btn);
    }

    private static void AddButtonRow(Container parent, params (string, Action)[] buttons)
    {
        var row = new HBoxContainer { LayoutMode = 1 };
        row.AddThemeConstantOverride("separation", 6);
        foreach (var (text, action) in buttons)
        {
            var btn = new Button
            {
                Text = text,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            };
            ApplyButtonStyle(btn);
            btn.Pressed += action;
            row.AddChild(btn);
        }
        parent.AddChild(row);
    }

    private static void ApplyButtonStyle(Button btn)
    {
        var t = RitsuShellTheme.Current;
        var r = t.Metric.Radius.Default;

        btn.AddThemeFontOverride("font", t.Font.Button);
        btn.AddThemeFontSizeOverride("font_size", t.Metric.FontSize.Button);
        btn.AddThemeColorOverride("font_color", t.Text.LabelPrimary);
        btn.AddThemeColorOverride("font_hover_color", t.Text.HoverHighlight);
        btn.AddThemeColorOverride("font_pressed_color", t.Text.HoverHighlight);
        btn.AddThemeColorOverride("font_focus_color", t.Text.LabelPrimary);

        btn.AddThemeStyleboxOverride("normal", MakePillStyle(t.Component.Pill.Default, r));
        btn.AddThemeStyleboxOverride("hover", MakePillStyle(t.Component.Pill.Hover, r));
        btn.AddThemeStyleboxOverride("pressed", MakePillStyle(t.Component.Pill.Default, r));
        btn.AddThemeStyleboxOverride("focus", new StyleBoxEmpty());
    }

    private static StyleBoxFlat MakePillStyle(BgBorder colors, int radius)
    {
        return new StyleBoxFlat
        {
            BgColor = colors.Bg,
            BorderColor = colors.Border,
            BorderWidthLeft = 1, BorderWidthTop = 1,
            BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = radius, CornerRadiusTopRight = radius,
            CornerRadiusBottomRight = radius, CornerRadiusBottomLeft = radius,
            ContentMarginLeft = 10, ContentMarginRight = 10,
            ContentMarginTop = 4, ContentMarginBottom = 4,
        };
    }

    private static void ApplyItemListStyle(ItemList list)
    {
        var t = RitsuShellTheme.Current;
        var r = t.Metric.Radius.Default;
        var shell = t.Component.ListShell;

        var bg = new StyleBoxFlat
        {
            BgColor = shell.Bg,
            BorderColor = shell.Border,
            BorderWidthLeft = 1, BorderWidthTop = 1,
            BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = r, CornerRadiusTopRight = r,
            CornerRadiusBottomRight = r, CornerRadiusBottomLeft = r,
            ShadowColor = shell.Shadow, ShadowSize = 3,
            ContentMarginLeft = 4, ContentMarginRight = 4,
            ContentMarginTop = 4, ContentMarginBottom = 4,
        };
        list.AddThemeStyleboxOverride("panel", bg);
        list.AddThemeColorOverride("font_color", t.Text.LabelPrimary);
        list.AddThemeColorOverride("font_selected_color", t.Text.HoverHighlight);
        list.AddThemeColorOverride("guide_color", t.Color.Divider);
        var selStyle = new StyleBoxFlat
        {
            BgColor = t.Component.ListItem.Accent.Bg,
            BorderColor = t.Component.ListItem.Accent.Border,
            BorderWidthLeft = 1, BorderWidthTop = 1,
            BorderWidthRight = 1, BorderWidthBottom = 1,
            CornerRadiusTopLeft = r - 2, CornerRadiusTopRight = r - 2,
            CornerRadiusBottomRight = r - 2, CornerRadiusBottomLeft = r - 2,
        };
        list.AddThemeStyleboxOverride("selected", selStyle);
        list.AddThemeStyleboxOverride("selected_focus", selStyle);
    }

    private static void ApplyLineEditStyle(LineEdit edit)
    {
        var t = RitsuShellTheme.Current;
        edit.AddThemeStyleboxOverride("normal", RitsuShellChromeStyles.CreateEntryFieldFrameStyle(false));
        edit.AddThemeStyleboxOverride("focus", RitsuShellChromeStyles.CreateEntryFieldFrameStyle(true));
        edit.AddThemeStyleboxOverride("read_only", RitsuShellChromeStyles.CreateEntryFieldFrameStyle(false));
        edit.AddThemeFontOverride("font", t.Font.Body);
        edit.AddThemeFontSizeOverride("font_size", t.Metric.FontSize.SettingLineTitle);
        edit.AddThemeColorOverride("font_color", t.Text.LabelPrimary);
        edit.AddThemeColorOverride("font_placeholder_color", t.Text.Hint);
        edit.AddThemeColorOverride("caret_color", t.Text.LabelPrimary);
        edit.AddThemeColorOverride("selection_color", t.Component.ListItem.Accent.Bg);
    }
}
#endif
