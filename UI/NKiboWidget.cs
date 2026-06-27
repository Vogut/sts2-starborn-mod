using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens;
using STS2RitsuLib.CardPiles;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Combat;

namespace STS2_Starborn.UI;

public partial class NKiboWidget : Control
{
    // 像素动画图集里的单帧尺寸。只有素材单帧大小变了才需要改。
    private const int FrameWidth = 96;
    private const int FrameHeight = 96;
    private const int FrameCount = 8;

    // 动画播放速度。数值越小切帧越快，数值越大切帧越慢。
    private const float FrameDuration = 0.12f;

    // 奇波和底座的整体缩放。数值越大，奇波和底座一起变大，锚点位置不变。
    private const float DisplayScale = 1.35f;

    // 底座素材缩放前的原始高度。只有底座素材尺寸变了才需要改。
    private const float PedestalBaseHeight = 64f;

    // 底座向上托进奇波区域的距离。数值越大，底座越靠近奇波；数值越小，间隙越大。
    private const float PedestalOverlap = 36f;

    private AtlasTexture _atlas = null!;
    private TextureRect _sprite = null!;
    private TextureRect _pedestal = null!;
    private ColorRect _cooldownOverlay = null!;
    private Label _cooldownLabel = null!;
    private Label _nameLabel = null!;
    private Label _pileCount = null!;
    private Player? _player;

    private int _currentFrame;
    private float _frameTimer;

    public NKiboWidget()
    {
        MouseFilter = MouseFilterEnum.Stop;
        BuildChildren();
    }

    public override void _Ready()
    {
        KiboPileManager.ActiveKiboChanged += OnActiveKiboChanged;
        KiboUltimateCooldownState.CooldownsChanged += OnKiboCooldownsChanged;
        ApplyLayout();
        Refresh();
    }

    public override void _ExitTree()
    {
        KiboPileManager.ActiveKiboChanged -= OnActiveKiboChanged;
        KiboUltimateCooldownState.CooldownsChanged -= OnKiboCooldownsChanged;
    }

    public void Initialize(Player player)
    {
        _player = player;
        Refresh();
        StarbornCombatWidgetLayout.LayoutKibo(this, _player);
    }

    public void Refresh()
    {
        if (_player == null)
        {
            Visible = false;
            UpdateCooldownDisplay(null);
            return;
        }

        var typeId = KiboPileManager.GetActiveKiboType(_player);
        if (typeId == null)
        {
            Visible = false;
            UpdateCooldownDisplay(null);
            return;
        }

        var def = KiboTypeRegistry.Get(typeId);
        Visible = true;

        _atlas.Atlas = GD.Load<Texture2D>(def.PixelAnimationPath);
        UpdateCooldownDisplay(typeId);
        UpdateFlightTargetPosition();
    }

    public override void _Process(double delta)
    {
        StarbornCombatWidgetLayout.LayoutKibo(this, _player);

        _frameTimer += (float)delta;
        if (_frameTimer >= FrameDuration)
        {
            _frameTimer = 0;
            _currentFrame = (_currentFrame + 1) % FrameCount;
            _atlas.Region = new Rect2(_currentFrame * FrameWidth, 0, FrameWidth, FrameHeight);
        }
    }

    private void ApplyLayout()
    {
        var spriteW = FrameWidth * DisplayScale;
        var spriteH = FrameHeight * DisplayScale;
        var pedH = PedestalBaseHeight * DisplayScale;
        var overlap = PedestalOverlap * DisplayScale;

        _sprite.Position = Vector2.Zero;
        _sprite.Size = new Vector2(spriteW, spriteH);
        _cooldownOverlay.Position = Vector2.Zero;
        _cooldownOverlay.Size = new Vector2(spriteW, spriteH);
        _cooldownLabel.Position = Vector2.Zero;
        _cooldownLabel.Size = new Vector2(spriteW, spriteH);

        _pedestal.Position = new Vector2(0, spriteH - overlap);
        _pedestal.Size = new Vector2(spriteW, pedH);

        Size = new Vector2(spriteW, spriteH + pedH - overlap);
        CustomMinimumSize = Size;

        _nameLabel.Visible = false;
        _pileCount.Visible = false;
    }

    private void BuildChildren()
    {
        _atlas = new AtlasTexture
        {
            Region = new Rect2(0, 0, FrameWidth, FrameHeight),
        };

        _pedestal = new TextureRect
        {
            Texture = GD.Load<Texture2D>(Const.Paths.KiboPedestal),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        };
        AddChild(_pedestal);

        _sprite = new TextureRect
        {
            Texture = _atlas,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        };
        AddChild(_sprite);

        _cooldownOverlay = new ColorRect
        {
            Color = new Color(0f, 0f, 0f, 0.45f),
            MouseFilter = MouseFilterEnum.Ignore,
            Visible = false,
        };
        AddChild(_cooldownOverlay);

        _cooldownLabel = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = MouseFilterEnum.Ignore,
            Visible = false,
        };
        _cooldownLabel.AddThemeColorOverride("font_color", Colors.White);
        _cooldownLabel.AddThemeColorOverride("font_shadow_color", Colors.Black);
        _cooldownLabel.AddThemeConstantOverride("shadow_offset_x", 2);
        _cooldownLabel.AddThemeConstantOverride("shadow_offset_y", 2);
        _cooldownLabel.AddThemeFontSizeOverride("font_size", 42);
        AddChild(_cooldownLabel);

        _nameLabel = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
        };
        AddChild(_nameLabel);

        _pileCount = new Label
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
        };
        AddChild(_pileCount);
    }

    private void OnActiveKiboChanged() => Refresh();
    private void OnKiboCooldownsChanged() => Refresh();

    private void UpdateCooldownDisplay(string? typeId)
    {
        var remainingTurns = _player != null && typeId != null
            ? KiboUltimateCooldownState.GetRemainingTurns(_player, typeId)
            : 0;

        var visible = remainingTurns > 0;
        _cooldownOverlay.Visible = visible;
        _cooldownLabel.Visible = visible;
        _cooldownLabel.Text = visible ? remainingTurns.ToString() : string.Empty;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false })
        {
            OpenPileScreen();
            AcceptEvent();
        }
    }

    public void UpdateFlightTargetPosition()
    {
        if (!IsInsideTree()) return;

        var spriteCenter = GlobalPosition + new Vector2(_sprite.Size.X / 2, _sprite.Size.Y / 2);
        KiboPileManager.WidgetGlobalPosition = spriteCenter;
    }

    private void OpenPileScreen()
    {
        if (_player == null) return;

        var pile = KiboPileManager.GetActivePile(_player);
        if (pile == null || pile.Cards.Count == 0) return;

        var definition = ModCardPileRegistry.Get(KiboPileManager.QualifiedPileId);
        NCardPileScreen.ShowScreen(pile, definition.Hotkeys ?? []);
    }
}
