using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
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

    private const float IconSize = 48f;
    private const float Spacing = 4f;

    public NElementMarkWidget()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        BuildChildren();
    }

    public override void _Ready()
    {
        ElementMarkState.MarksChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        ElementMarkState.MarksChanged -= Refresh;
    }

    public void Initialize(Player player)
    {
        _player = player;
        Refresh();
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

    public override void _Process(double delta)
    {
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

        public MarkSlotIcon()
        {
            MouseFilter = MouseFilterEnum.Stop;
            Build();
        }

        private void Build()
        {
            _icon = new TextureRect
            {
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                CustomMinimumSize = new Vector2(IconSize, IconSize),
            };
            AddChild(_icon);

            _label = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            AddChild(_label);

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

        private void BuildHoverTips(Player player, MarkSlot slot, SealElementType elementType)
        {
            var elementPower = Element.StarbornElement.For(elementType);

            var title = new LocString("powers",
                $"STS2_STARBORN_ELEMENT_{elementType.ToString().ToUpperInvariant()}.title");
            var desc = elementPower.ElementDescription;

            _hoverTips = new List<IHoverTip> { new HoverTip(title, desc, _icon.Texture) };

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
                NHoverTipSet.CreateAndShow(this, _hoverTips, HoverTipAlignment.Left);
        }

        private void OnMouseExited()
        {
            NHoverTipSet.Remove(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                MouseEntered -= OnMouseEntered;
                MouseExited -= OnMouseExited;
            }
            base.Dispose(disposing);
        }
    }
}
