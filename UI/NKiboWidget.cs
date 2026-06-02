using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;

namespace STS2_Starborn.UI;

public partial class NKiboWidget : Control
{
    private const int FrameWidth = 96;
    private const int FrameHeight = 96;
    private const int FrameCount = 8;
    private const float FrameDuration = 0.12f;

    private AtlasTexture _atlas = null!;
    private TextureRect _sprite = null!;
    private TextureRect _pedestal = null!;
    private Label _nameLabel = null!;
    private Label _pileCount = null!;
    private Player? _player;

    private int _currentFrame;
    private float _frameTimer;

    public NKiboWidget()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        BuildChildren();
    }

    public override void _Ready()
    {
        KiboPileManager.ActiveKiboChanged += OnActiveKiboChanged;
        ApplyLayout();
        Refresh();
    }

    public override void _ExitTree()
    {
        KiboPileManager.ActiveKiboChanged -= OnActiveKiboChanged;
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

        var typeId = KiboPileManager.GetActiveKiboType(_player);
        if (typeId == null)
        {
            Visible = false;
            return;
        }

        var def = KiboTypeRegistry.Get(typeId);
        Visible = true;

        _atlas.Atlas = GD.Load<Texture2D>(def.PixelAnimationPath);
        _nameLabel.Text = def.LocKey;

        var pile = KiboPileManager.GetActivePile(_player);
        _pileCount.Text = pile?.Cards.Count.ToString() ?? "0";
    }

    public override void _Process(double delta)
    {
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
        var scale = 1.2f;
        var spriteW = FrameWidth * scale;
        var spriteH = FrameHeight * scale;
        var pedH = 64f * scale;
        var infoX = spriteW + 12f;
        var infoW = Size.X - infoX;

        _sprite.Position = Vector2.Zero;
        _sprite.Size = new Vector2(spriteW, spriteH);

        _pedestal.Position = new Vector2(0, spriteH);
        _pedestal.Size = new Vector2(spriteW, pedH);

        _nameLabel.Position = new Vector2(infoX, 8);
        _nameLabel.Size = new Vector2(infoW, 28f);

        _pileCount.Position = new Vector2(infoX, 64f);
        _pileCount.Size = new Vector2(infoW, 24f);
    }

    private void BuildChildren()
    {
        _atlas = new AtlasTexture
        {
            Region = new Rect2(0, 0, FrameWidth, FrameHeight),
        };

        _sprite = new TextureRect
        {
            Texture = _atlas,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        };
        AddChild(_sprite);

        _pedestal = new TextureRect
        {
            Texture = GD.Load<Texture2D>(Const.Paths.KiboPedestal),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
        };
        AddChild(_pedestal);

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
}
