using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Element;

namespace STS2_Starborn
{
    public static class Const
    {
        public const string ModId = "sts2_starborn";
        public const string Name = "Starborn";
        public const string Version = "0.1.0-alpha";

        /// <summary>资源路径，统一管理 <c>res://STS2_Starborn/</c> 下的所有路径。</summary>
        public static class Paths
        {
            private const string Root = "res://STS2_Starborn";

            // ─── Scenes ────────────────────────────────────
            public const string StarbornCharacter = Root + "/scenes/starborn_character.tscn";
            public const string EnergyCounter     = Root + "/scenes/starborn_energy_counter.tscn";
            public const string CharacterMerchant = Root + "/scenes/starborn_character_merchant.tscn";
            public const string CharacterSelectBg = Root + "/scenes/starborn_bg.tscn";

            // ─── Character select icons ────────────────────
            public const string CharacterSelectIcon       = Root + "/starborn/character/char_select_starborn.png";
            public const string CharacterSelectLockedIcon = Root + "/starborn/character/char_select_starborn_locked.png";

            // ─── Energy icons (3 个 pool 类共用) ──────────
            public const string EnergySmall = Root + "/starborn/energy/energy_starborn.png";
            public const string EnergyBig   = Root + "/starborn/energy/energy_starborn_big.png";


            // ─── Card / Relic / Power portraits (按 Type.Name 插值) ─
            public static string CardPortrait(Type cardType) =>
                $"{Root}/cards/{cardType.Name}.png";

            public static string RelicIcon(Type relicType) =>
                $"{Root}/images/relics/{relicType.Name}.png";

            public static string PowerIcon(Type powerType) =>
                $"{Root}/powers/{powerType.Name}.png";

            public static string PowerBigIcon(Type powerType) =>
                $"{Root}/powers/{powerType.Name}_big.png";

            // ─── Element icons ─────────────────────────────
            /// <summary>Power 图标：Fire.png / Water.png …（无 Icon 后缀）</summary>
            public static string ElementPowerIcon(SealElementType et) =>
                $"{Root}/Elements/{et}.png";

            /// <summary>小图标：FireIcon.png / WaterIcon.png …（有 Icon 后缀）</summary>
            public static string ElementIcon(SealElementType et) =>
                $"{Root}/Elements/Icon/{et}.png";

            // ─── Kibo ─────────────────────────────────────
            public const string KiboPileIcon = Root + "/kibo/Icons/kibo_pile.png";
            public const string KiboCollectionPileIcon = Root + "/kibo/Icons/kibo_collection.png";
            public const string KiboPedestal = Root + "/kibo/Base.png";

            public static string KiboPixelAnimation(string stem) =>
                $"{Root}/kibo/pixel_animation/{Cards.Kibo.KiboTypeId.StemToPascalCase(stem)}.png";

            public static string KiboCardPortrait(Type cardType) =>
                $"{Root}/kibo/cards/{cardType.Name}.png";
        }
    }
}
