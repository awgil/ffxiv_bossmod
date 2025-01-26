using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;
public sealed class ClassSCHUtility(RotationModuleManager manager, Actor player) : RoleHealerUtility(manager, player)
{
    public enum Track
    {
        WhisperingDawn = SharedTrack.Count,
        FeyIllumination,
        SacredSoil,
        Indomitability,
        Shield,
        EmergencyTactics,
        Dissipation,
        Excog,
        Aetherpact,
        FeyBlessing,
        Seraph,
        Expedient,
        Seraphism
    }

    public enum ShieldStrategy
    {
        None,
        Succor,
        CritSuccor,
        Spreadlo,
        CritSpreadlo,
        ProSpreadlo,
        CritProSpreadlo
    }

    public enum SoilPosition
    {
        None,
        ArenaCenter,
        Pet,
        Self,
    }

    public static readonly ActionID IDLimitBreak3 = ActionID.MakeSpell(SCH.AID.AngelFeathers);

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: SCH", "Cooldown Planner support for Utility Actions.\nNOTE: This is NOT a rotation preset! All Utility modules are STRICTLY for cooldown-planning usage.", "Utility for planner", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.SCH), 100);
        DefineShared(res, IDLimitBreak3);

        DefineSimpleConfig(res, Track.WhisperingDawn, "WhisperingDawn", "WD", 100, SCH.AID.WhisperingDawn, 21);
        DefineSimpleConfig(res, Track.FeyIllumination, "FeyIllumination", "FI", 101, SCH.AID.FeyIllumination, 20);
        res.Define(Track.SacredSoil).As<SoilPosition>("SacredSoil", "Soil", 200)
            .AddOption(SoilPosition.None, "Do not use")
            .AddOption(SoilPosition.ArenaCenter, "Place at arena center", effect: 15, cooldown: 30, minLevel: 50)
            .AddOption(SoilPosition.Pet, "Place on pet, or self if no pet is summoned", effect: 15, cooldown: 30, minLevel: 50)
            .AddOption(SoilPosition.Self, "Place on self", effect: 15, cooldown: 30, minLevel: 50);
        DefineSimpleConfig(res, Track.Indomitability, "Indomitability", "Indom", 103, SCH.AID.Indomitability);

        res.Define(Track.Shield).As<ShieldStrategy>("Shield", "Shield", 150)
            .AddOption(ShieldStrategy.None, "None")
            .AddOption(ShieldStrategy.Succor, "Succor", minLevel: 35, effect: 30)
            .AddOption(ShieldStrategy.CritSuccor, "Recitation + Succor", minLevel: 74, effect: 30, cooldown: 90)
            .AddOption(ShieldStrategy.Spreadlo, "Adloquium + Deployment Tactics (Spreadlo)", minLevel: 56, effect: 30, cooldown: 90)
            .AddOption(ShieldStrategy.CritSpreadlo, "Recitation + Spreadlo", minLevel: 74, effect: 30, cooldown: 90)
            .AddOption(ShieldStrategy.ProSpreadlo, "Spreadlo + Protraction", minLevel: 86, effect: 30, cooldown: 90)
            .AddOption(ShieldStrategy.CritProSpreadlo, "Recitation + Spreadlo + Protraction", minLevel: 86, effect: 30, cooldown: 90);

        DefineSimpleConfig(res, Track.EmergencyTactics, "EmergencyTactics", "Emerg", 105, SCH.AID.EmergencyTactics, 15);
        DefineSimpleConfig(res, Track.Dissipation, "Dissipation", "Disp", 106, SCH.AID.Dissipation, 30);
        DefineSimpleConfig(res, Track.Excog, "Excogitation", "Excog", 107, SCH.AID.Excogitation);
        DefineSimpleConfig(res, Track.Aetherpact, "Aetherpact", "FeyUnion", 108, SCH.AID.Aetherpact);
        DefineSimpleConfig(res, Track.FeyBlessing, "FeyBlessing", "Blessing", 109, SCH.AID.FeyBlessing);
        DefineSimpleConfig(res, Track.Seraph, "SummonSeraph", "Seraph", 110, SCH.AID.SummonSeraph, effect: 22f);
        DefineSimpleConfig(res, Track.Expedient, "Expedient", "Exped", 180, SCH.AID.Expedient, 20);
        DefineSimpleConfig(res, Track.Seraphism, "Seraphism", "Seraphism", 112, SCH.AID.Seraphism);

        return res;
    }

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        ExecuteShared(strategy, IDLimitBreak3, primaryTarget);

        // TODO add Consolation

        var gauge = World.Client.GetGauge<ScholarGauge>();
        var seraphTimer = gauge.SeraphTimer * 0.001f;
        var haveSeraph = seraphTimer > World.Client.AnimationLock;
        var havePet = World.Client.ActivePet.InstanceID != 0xE0000000;
        var haveEos = havePet && !haveSeraph;

        ExecuteSimple(strategy.Option(Track.WhisperingDawn), SCH.AID.WhisperingDawn, Player, havePet);
        ExecuteSimple(strategy.Option(Track.FeyIllumination), SCH.AID.FeyIllumination, Player, havePet);

        UseSoil(strategy.Option(Track.SacredSoil), gauge);
        ExecuteSimple(strategy.Option(Track.Indomitability), SCH.AID.Indomitability, Player, gauge.Aetherflow > 0);

        UseShield(strategy.Option(Track.Shield));

        ExecuteSimple(strategy.Option(Track.EmergencyTactics), SCH.AID.EmergencyTactics, Player);
        ExecuteSimple(strategy.Option(Track.Dissipation), SCH.AID.Dissipation, Player, havePet);
        ExecuteSimple(strategy.Option(Track.Excog), SCH.AID.Excogitation, null);
        ExecuteSimple(strategy.Option(Track.FeyBlessing), SCH.AID.FeyBlessing, Player, haveEos);
        ExecuteSimple(strategy.Option(Track.Seraph), SCH.AID.SummonSeraph, Player);
        ExecuteSimple(strategy.Option(Track.Expedient), SCH.AID.Expedient, Player);
        ExecuteSimple(strategy.Option(Track.Seraphism), SCH.AID.Seraphism, Player);
    }

    private void UseSoil(in StrategyValues.OptionRef opt, ScholarGauge gauge)
    {
        if (gauge.Aetherflow == 0)
            return;

        Vector3 targetPos;

        switch (opt.As<SoilPosition>())
        {
            case SoilPosition.Self:
                targetPos = Player.PosRot.XYZ();
                break;
            case SoilPosition.Pet:
                targetPos = World.Actors.Find(World.Client.ActivePet.InstanceID)?.PosRot.XYZ() ?? Player.PosRot.XYZ();
                break;
            case SoilPosition.ArenaCenter:
                if (Bossmods.ActiveModule is BossModule m)
                {
                    var center = m.Arena.Center;
                    targetPos = new Vector3(center.X, Player.PosRot.Y, center.Z);
                }
                else
                    targetPos = Player.PosRot.XYZ();
                break;
            default:
                return;
        }

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.SacredSoil), null, opt.Priority(), targetPos: targetPos);
    }

    private void UseShield(in StrategyValues.OptionRef opt)
    {
        var (use, recite, deploy, protract) = PickShield(opt);
        if (!use)
            return;

        var highestHPPartyMember = World.Party.WithoutSlot().MaxBy(x => x.HPMP.MaxHP)!;

        if (deploy && highestHPPartyMember.FindStatus(SCH.SID.Galvanize) != null)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.DeploymentTactics), highestHPPartyMember, opt.Priority());
            return;
        }

        if (recite)
        {
            if (Player.FindStatus(1896) == null)
            {
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Recitation), null, opt.Priority());
                return;
            }
        }

        if (protract)
        {
            if (highestHPPartyMember.FindStatus(2710) == null)
            {
                Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Protraction), highestHPPartyMember, opt.Priority());
                return;
            }
        }

        if (deploy)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Adloquium), highestHPPartyMember, opt.Priority());
        else if (Player.FindStatus(SCH.SID.Galvanize) == null)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(SCH.AID.Succor), Player, ActionQueue.Priority.VeryHigh);
    }

    private (bool Use, bool Recitation, bool Deployment, bool Protraction) PickShield(in StrategyValues.OptionRef opt)
    {
        var use = true;
        var recite = false;
        var deploy = false;
        var protract = false;

        switch (opt.As<ShieldStrategy>())
        {
            case ShieldStrategy.None:
                use = false;
                break;
            case ShieldStrategy.CritSuccor:
                recite = true;
                break;
            case ShieldStrategy.Spreadlo:
                deploy = true;
                break;
            case ShieldStrategy.CritSpreadlo:
                recite = deploy = true;
                break;
            case ShieldStrategy.ProSpreadlo:
                deploy = protract = true;
                break;
            case ShieldStrategy.CritProSpreadlo:
                deploy = protract = recite = true;
                break;
        }

        return (use, recite, deploy, protract);
    }
}
