using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using STS2RitsuLib.CardPiles;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.UI;

public partial class KiboStorageViewer : Control, ICapstoneScreen
{
    private CardPile _pile = null!;
    private Player _player = null!;

    private HBoxContainer _tabBar = null!;
    private Button _collectionTab = null!;
    private Button _abilitiesTab = null!;
    private VBoxContainer _cardList = null!;
    private ScrollContainer _scroll = null!;
    private Button _backButton = null!;

    private bool _showingCollection = true;

    public NetScreenType ScreenType => NetScreenType.None;
    public bool UseSharedBackstop => true;
    public Control? DefaultFocusedControl => _backButton;

    public KiboStorageViewer() { }

    public void Setup(CardPile pile, Player player)
    {
        _pile = pile;
        _player = player;
        BuildUi();
    }

    public void AfterCapstoneOpened()
    {
        RefreshCardList();
    }

    public void AfterCapstoneClosed()
    {
        QueueFree();
    }

    private void BuildUi()
    {
        AnchorLeft = 0.1f;
        AnchorRight = 0.9f;
        AnchorTop = 0.1f;
        AnchorBottom = 0.9f;
        OffsetLeft = 0;
        OffsetRight = 0;
        OffsetTop = 0;
        OffsetBottom = 0;

        // Background
        var bg = new ColorRect
        {
            Color = new Color(0.08f, 0.08f, 0.12f, 0.95f),
        };
        bg.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(bg);

        // Top bar: Back + Tabs
        var topBar = new HBoxContainer();
        topBar.SetAnchorsAndOffsetsPreset(LayoutPreset.TopWide);
        topBar.OffsetBottom = 48;
        AddChild(topBar);

        _backButton = new Button
        {
            Text = "<- Back",
            SizeFlagsHorizontal = SizeFlags.ShrinkBegin,
        };
        _backButton.Pressed += OnBackPressed;
        topBar.AddChild(_backButton);

        _tabBar = new HBoxContainer();
        _tabBar.AddThemeConstantOverride("separation", 8);
        topBar.AddChild(_tabBar);

        _collectionTab = new Button
        {
            Text = "Collection",
            ToggleMode = true,
            ButtonPressed = true,
        };
        _collectionTab.Pressed += () => SwitchTab(true);
        _tabBar.AddChild(_collectionTab);

        _abilitiesTab = new Button
        {
            Text = "Abilities",
            ToggleMode = true,
        };
        _abilitiesTab.Pressed += () => SwitchTab(false);
        _tabBar.AddChild(_abilitiesTab);

        // Scroll area
        _scroll = new ScrollContainer();
        _scroll.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        _scroll.OffsetTop = 56;
        _scroll.OffsetBottom = -16;
        AddChild(_scroll);

        _cardList = new VBoxContainer();
        _cardList.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _cardList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _scroll.AddChild(_cardList);
    }

    private void SwitchTab(bool collection)
    {
        _showingCollection = collection;
        _collectionTab.ButtonPressed = collection;
        _abilitiesTab.ButtonPressed = !collection;
        RefreshCardList();
    }

    private void RefreshCardList()
    {
        // Recreate the list container to force layout refresh
        _cardList.QueueFree();
        _cardList = new VBoxContainer();
        _cardList.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _scroll.AddChild(_cardList);

        var cards = _pile.Cards.ToList();
        if (cards.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "(empty)",
                Modulate = new Color(0.5f, 0.5f, 0.5f),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _cardList.AddChild(emptyLabel);
            return;
        }

        foreach (var card in cards)
        {
            var isRep = IsRepCardType(card.GetType());
            if (_showingCollection && !isRep) continue;
            if (!_showingCollection && isRep) continue;

            var row = new HBoxContainer();
            row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

            var nameLabel = new Label
            {
                Text = card.Title,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                VerticalAlignment = VerticalAlignment.Center,
            };
            row.AddChild(nameLabel);

            if (card.IsUpgraded)
            {
                var upgradeLabel = new Label
                {
                    Text = "+",
                    Modulate = new Color(0.3f, 1f, 0.4f),
                };
                row.AddChild(upgradeLabel);
            }

            _cardList.AddChild(row);
        }
    }

    private void OnBackPressed()
    {
        STS2RitsuLib.Screens.ModScreenService.Close();
    }

    private static bool IsRepCardType(Type type)
    {
        return type.BaseType == typeof(KiboCard) &&
               type.Name.EndsWith("RepCard");
    }
}
