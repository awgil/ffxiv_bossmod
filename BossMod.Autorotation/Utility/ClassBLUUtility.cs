namespace BossMod.Autorotation;

public sealed class ClassBLUUtility(RotationModuleManager manager, Actor player) : RoleCasterUtility(manager, player)
{
    public enum Track { Bristle = SharedTrack.Count, WhiteWind, IceSpikes, OffGuard, Transfusion, Diamondback, MightyGuard, ToadOil, MoonFlute, PeculiarLight, VeilOfTheWhorl, EerieSoundwave, PomCure, Gobskin, Avail, FrogLegs, Whistle, Cactguard, AngelWhisper, Exuviation, ColdFog, Stotram, AngelsSnack, ChelonianGate, BasicInstinct, DragonForce, Schiltron, Rehydration, ForceField }
    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(BLU.AID.None);

    public static RotationModuleDefinition Definition(ActionDefinitions adefs)
    {
        var res = new RotationModuleDefinition("Utility: BLU", "Planner support for utility actions\nNOTE: There are a lot of Utility actions for Blue Mage.\nFor better accessibility:\n  1. Select 'Column Visibility' tab\n  2. Choose which skills you would like to keep visible or invisible", "Utility for planner", "Akechi", RotationModuleQuality.Basic, BitMask.Build((int)Class.BLU), 80);
        DefineShared(res, adefs, IDLimitBreak3);

        DefineSimpleConfig(res, adefs, Track.Bristle, "Bristle", "Bristle", 100, BLU.AID.Bristle, 30);
        DefineSimpleConfig(res, adefs, Track.WhiteWind, "White Wind", "W.Wind", 100, BLU.AID.WhiteWind);
        DefineSimpleConfig(res, adefs, Track.IceSpikes, "Ice Spikes", "I.Spikes", 50, BLU.AID.IceSpikes, 15);
        DefineSimpleConfig(res, adefs, Track.OffGuard, "Off Guard", "OffG.", 25, BLU.AID.OffGuard, 15);
        DefineSimpleConfig(res, adefs, Track.Transfusion, "Transfusion", "Transf.", 10, BLU.AID.Transfusion);
        DefineSimpleConfig(res, adefs, Track.Diamondback, "Diamondback", "D.back", 100, BLU.AID.Diamondback, 10);
        DefineSimpleConfig(res, adefs, Track.MightyGuard, "Mighty Guard", "MightyG.", 10, BLU.AID.MightyGuard);
        DefineSimpleConfig(res, adefs, Track.ToadOil, "Toad Oil", "T.Oil", 10, BLU.AID.ToadOil, 180);
        DefineSimpleConfig(res, adefs, Track.MoonFlute, "Moon Flute", "MoonFlute", 100, BLU.AID.MoonFlute, 15);
        DefineSimpleConfig(res, adefs, Track.PeculiarLight, "Peculiar Light", "P.Light", 25, BLU.AID.PeculiarLight, 15);
        DefineSimpleConfig(res, adefs, Track.VeilOfTheWhorl, "Veil Of The Whorl", "V.Whorl", 100, BLU.AID.VeilOfTheWhorl, 30);
        DefineSimpleConfig(res, adefs, Track.EerieSoundwave, "Eerie Soundwave", "Soundwave", 100, BLU.AID.EerieSoundwave);
        DefineSimpleConfig(res, adefs, Track.PomCure, "Pom Cure", "PomCure", 100, BLU.AID.PomCure);
        DefineSimpleConfig(res, adefs, Track.Gobskin, "Gobskin", "Gobskin", 100, BLU.AID.Gobskin, 30);
        DefineSimpleConfig(res, adefs, Track.Avail, "Avail", "Avail", 100, BLU.AID.Avail, 12);
        DefineSimpleConfig(res, adefs, Track.FrogLegs, "Frog Legs", "F.Legs", 10, BLU.AID.FrogLegs);
        DefineSimpleConfig(res, adefs, Track.Whistle, "Whistle", "Whistle", 100, BLU.AID.Whistle, 30);
        DefineSimpleConfig(res, adefs, Track.Cactguard, "Cactguard", "CactG.", 100, BLU.AID.Cactguard, 6);
        DefineSimpleConfig(res, adefs, Track.AngelWhisper, "Angel Whisper", "A.Whisper", 100, BLU.AID.AngelWhisper);
        DefineSimpleConfig(res, adefs, Track.Exuviation, "Exuviation", "Exuvi", 100, BLU.AID.Exuviation);
        DefineSimpleConfig(res, adefs, Track.ColdFog, "Cold Fog", "C.Fog", 50, BLU.AID.ColdFog, 5);
        DefineSimpleConfig(res, adefs, Track.Stotram, "Stotram (Heal)", "Stotram", 100, BLU.AID.StotramHeal);
        DefineSimpleConfig(res, adefs, Track.AngelsSnack, "Angels Snack", "A.Snack", 100, BLU.AID.AngelsSnack, 15);
        DefineSimpleConfig(res, adefs, Track.ChelonianGate, "Chelonian Gate", "C.Gate", 100, BLU.AID.ChelonianGate, 10);
        DefineSimpleConfig(res, adefs, Track.BasicInstinct, "Basic Instinct", "Instinct", 10, BLU.AID.BasicInstinct);
        DefineSimpleConfig(res, adefs, Track.DragonForce, "Dragon Force", "D.Force", 100, BLU.AID.DragonForce, 15);
        DefineSimpleConfig(res, adefs, Track.Schiltron, "Schiltron", "Schilt", 100, BLU.AID.Schiltron, 15);
        DefineSimpleConfig(res, adefs, Track.Rehydration, "Rehydration", "Rehyd.", 100, BLU.AID.Rehydration);
        DefineSimpleConfig(res, adefs, Track.ForceField, "Force Field", "F.Field", 100, BLU.AID.ForceField, 10);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);
        ExecuteSimple(strategy.Option(Track.Bristle), BLU.AID.Bristle, Player, 1); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.WhiteWind), BLU.AID.WhiteWind, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.IceSpikes), BLU.AID.IceSpikes, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.OffGuard), BLU.AID.OffGuard, primaryTarget, 1); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Transfusion), BLU.AID.Transfusion, primaryTarget, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Diamondback), BLU.AID.Diamondback, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.MightyGuard), BLU.AID.MightyGuard, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.ToadOil), BLU.AID.ToadOil, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.MoonFlute), BLU.AID.MoonFlute, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.PeculiarLight), BLU.AID.PeculiarLight, primaryTarget, 1); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.VeilOfTheWhorl), BLU.AID.VeilOfTheWhorl, Player);
        ExecuteSimple(strategy.Option(Track.EerieSoundwave), BLU.AID.EerieSoundwave, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.PomCure), BLU.AID.PomCure, primaryTarget, 1.5f); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Gobskin), BLU.AID.Gobskin, primaryTarget, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Avail), BLU.AID.Avail, primaryTarget, 1); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.FrogLegs), BLU.AID.FrogLegs, primaryTarget, 1); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Whistle), BLU.AID.Whistle, Player, 1); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Cactguard), BLU.AID.Cactguard, primaryTarget, 1); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.AngelWhisper), BLU.AID.AngelWhisper, primaryTarget, 10); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Exuviation), BLU.AID.Exuviation, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.ColdFog), BLU.AID.ColdFog, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Stotram), BLU.AID.StotramHeal, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.AngelsSnack), BLU.AID.AngelsSnack, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.ChelonianGate), BLU.AID.ChelonianGate, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.BasicInstinct), BLU.AID.BasicInstinct, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.DragonForce), BLU.AID.DragonForce, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Schiltron), BLU.AID.Schiltron, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.Rehydration), BLU.AID.Rehydration, Player, 5); // TODO[cast-time]: adjustment (swiftcast etc)
        ExecuteSimple(strategy.Option(Track.ForceField), BLU.AID.ForceField, Player, 2); // TODO[cast-time]: adjustment (swiftcast etc)
    }
}
