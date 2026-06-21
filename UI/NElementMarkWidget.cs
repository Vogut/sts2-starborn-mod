using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using STS2_Starborn.Cards;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;
using STS2_Starborn.Element;

namespace STS2_Starborn.UI;

public partial class NElementMarkWidget : Control
{
    private MarkSlotIcon _primary = null!;
    private MarkSlotIcon _secondary = null!;
    private Player? _player;

    // 单个印记图标大小。数值越大，主/副印记图标和点击区域一起变大。
    private const float IconSize = 56f;

    // 主印记和副印记之间的竖向间距。数值越大，两枚图标离得越开。
    private const float Spacing = 6f;

    public NElementMarkWidget()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        BuildChildren();
    }

    public override void _Ready()
    {
        ElementMarkState.MarksChanged += Refresh;
        ElementMarkState.MarkVisualChanged += OnMarkVisualChanged;
        Refresh();
    }

    public override void _ExitTree()
    {
        ElementMarkState.MarksChanged -= Refresh;
        ElementMarkState.MarkVisualChanged -= OnMarkVisualChanged;
    }

    public void Initialize(Player player)
    {
        _player = player;
        Refresh();
        StarbornCombatWidgetLayout.LayoutElementMark(this, _player);
    }

    public void Refresh()
    {
        if (_player == null)
        {
            Visible = false;
            return;
        }

        var primaryType = ElementMarkState.GetElementType(_player, MarkSlot.Primary);
        var secondaryType = ElementMarkState.GetElementType(_player, MarkSlot.Secondary);
        var primaryStacks = ElementMarkState.GetStacks(_player, MarkSlot.Primary);
        var secondaryStacks = ElementMarkState.GetStacks(_player, MarkSlot.Secondary);

        Visible = primaryType != SealElementType.None || secondaryType != SealElementType.None
            || primaryStacks > 0 || secondaryStacks > 0;

        _primary.UpdateDisplay(_player, MarkSlot.Primary);
        _secondary.UpdateDisplay(_player, MarkSlot.Secondary);
    }

    private void OnMarkVisualChanged(MarkVisualChange change)
    {
        if (_player == null || _player.NetId != change.Player.NetId)
            return;

        Visible = true;
        var slotIcon = change.Slot == MarkSlot.Primary ? _primary : _secondary;
        slotIcon.Visible = true;
        slotIcon.PlayFeedback(change, Refresh);
    }

    public override void _Process(double delta)
    {
        StarbornCombatWidgetLayout.LayoutElementMark(this, _player);

        _primary.Position = Vector2.Zero;
        _secondary.Position = new Vector2(0, IconSize + Spacing);
    }

    private void BuildChildren()
    {
        _primary = new MarkSlotIcon();
        AddChild(_primary);

        _secondary = new MarkSlotIcon();
        AddChild(_secondary);
    }

    private partial class MarkSlotIcon : Control
    {
        private TextureRect _icon = null!;
        private Label _label = null!;
        private List<IHoverTip>? _hoverTips;
        private Tween? _feedbackTween;

        public MarkSlotIcon()
        {
            MouseFilter = MouseFilterEnum.Stop;
            Build();
        }

        private void Build()
        {
            Size = new Vector2(IconSize, IconSize);
            CustomMinimumSize = Size;

            _icon = new TextureRect
            {
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Size = new Vector2(IconSize, IconSize),
                CustomMinimumSize = new Vector2(IconSize, IconSize),
            };
            AddChild(_icon);

            _label = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                Size = new Vector2(IconSize, IconSize),
            };
            // 印记层数字号。数值越大，右下角层数显示越大。
            _label.AddThemeFontSizeOverride("font_size", 22);
            AddChild(_label);

            // 动画缩放中心。保持在图标中央，获得/调谐反馈会从中心放大回弹。
            PivotOffset = new Vector2(IconSize * 0.5f, IconSize * 0.5f);

            MouseEntered += OnMouseEntered;
            MouseExited += OnMouseExited;
        }

        public void UpdateDisplay(Player? player, MarkSlot slot)
        {
            if (player == null)
            {
                Visible = false;
                return;
            }

            var elementType = ElementMarkState.GetElementType(player, slot);
            var stacks = ElementMarkState.GetStacks(player, slot);

            var iconPath = Const.Paths.ElementIcon(elementType);
            if (ResourceLoader.Exists(iconPath))
                _icon.Texture = ResourceLoader.Load<Texture2D>(iconPath);

            _label.Text = stacks.ToString();

            BuildHoverTips(player, slot, elementType);

            Visible = true;
        }

        public void PlayFeedback(MarkVisualChange change, Action? finished = null)
        {
            _feedbackTween?.Kill();
            // 反馈动画：先放大并染色，再回弹到正常大小和白色。
            Scale = Vector2.One * FeedbackScaleFor(change.Kind);
            Modulate = FeedbackColorFor(change);

            _feedbackTween = CreateTween().SetParallel();
            _feedbackTween.TweenProperty(this, "scale", Vector2.One, 0.35)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Back);
            _feedbackTween.TweenProperty(this, "modulate", Colors.White, 0.35)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Expo);
            if (finished != null)
                _feedbackTween.Finished += finished;
        }

        private static float FeedbackScaleFor(MarkVisualChangeKind kind) => kind switch
        {
            MarkVisualChangeKind.ElementChanged => 1.28f,
            MarkVisualChangeKind.Tuned => 1.24f,
            MarkVisualChangeKind.Overloaded => 1.3f,
            _ => 1.18f,
        };

        private static Color FeedbackColorFor(MarkVisualChange change) => change.Kind switch
        {
            MarkVisualChangeKind.Tuned => new Color(1f, 0.92f, 0.48f),
            MarkVisualChangeKind.Overloaded => new Color(1f, 0.98f, 0.72f),
            MarkVisualChangeKind.ElementChanged => ElementColor(change.NewElementType, true),
            MarkVisualChangeKind.StacksGained => ElementColor(change.NewElementType),
            MarkVisualChangeKind.StacksLost => new Color(0.85f, 0.9f, 1f),
            _ => Colors.White,
        };

        private static Color ElementColor(SealElementType elementType, bool includeNone = false) => elementType switch
        {
            SealElementType.Fire => new Color(1f, 0.22f, 0.12f),
            SealElementType.Water => new Color(0.22f, 0.56f, 1f),
            SealElementType.Wood => new Color(0.28f, 0.86f, 0.34f),
            SealElementType.Ice => new Color(0.35f, 0.92f, 1f),
            SealElementType.Wind => new Color(0.65f, 1f, 0.55f),
            SealElementType.Light => new Color(1f, 0.88f, 0.32f),
            SealElementType.None when includeNone => new Color(0.82f, 0.86f, 0.9f),
            _ => Colors.White,
        };

        private void BuildHoverTips(Player player, MarkSlot slot, SealElementType elementType)
        {
            var elementPower = Element.StarbornElement.For(elementType);
            var stacks = ElementMarkState.GetStacks(player, slot);

            _hoverTips = new List<IHoverTip> { StarbornTipFactory.ElementMark(elementType, stacks) };

            var combatState = player.Creature.CombatState;
            if (combatState != null)
            {
                var tuningConsume = SealElementMarkHooks.ModifyTuningConsume(
                    combatState, slot, elementPower.TuningConsume);
                var overloadConsume = SealElementMarkHooks.ModifyOverloadConsume(
                    combatState, slot, elementPower.OverloadConsume);

                _hoverTips.Add(StarbornTipFactory.Tuning(elementType, tuningConsume));
                _hoverTips.Add(StarbornTipFactory.Overload(elementType, overloadConsume));
            }

            foreach (var power in elementPower.AssociatedPowers)
                _hoverTips.Add(HoverTipFactory.FromPower(power));
        }

        private void OnMouseEntered()
        {
            if (_hoverTips != null)
                NHoverTipSet.CreateAndShow(this, _hoverTips, HoverTipAlignment.Right);
        }

        private void OnMouseExited()
        {
            NHoverTipSet.Remove(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _feedbackTween?.Kill();
                MouseEntered -= OnMouseEntered;
                MouseExited -= OnMouseExited;
            }
            base.Dispose(disposing);
        }
    }
}
