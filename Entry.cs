using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using STS2_Starborn.Cards.Common;
using STS2_Starborn.Relics;

namespace STS2_Starborn;

// 必须要加的属性，用于注册Mod。字符串和初始化函数命名一致。
[ModInitializer(nameof(Init))]
public class Entry
{
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(Const.ModId);    
    // 初始化函数
    public static void Init()
    {
        // harmony可用，但是最好用ritsu的封装patch（TODO）
        // var harmony = new Harmony("com.example.testmod");
        // harmony.PatchAll();
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        // 自动注册内容
        ModTypeDiscoveryHub.RegisterModAssembly(Const.ModId, assembly);
        // 古老牙齿
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<TestCard, TestAncientCard>();
        // 奥伯拉斯之触
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<TestRelic, TestRelic>();
    }
    
}
