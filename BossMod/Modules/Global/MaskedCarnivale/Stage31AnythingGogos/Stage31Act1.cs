namespace BossMod.Global.MaskedCarnivale.Stage31.Act1;

public enum OID : uint
{
    Boss = 0x30F5, //R=2.0
    Imitation = 0x30F6, // R=2.0
    Helper = 0x233C,
}

public enum AID : uint
{
    AutoAttack = 6499, // 30F5->player, no cast, single-target
    Mimic = 23097, // 30F5->self, 5,0s cast, single-target, stop everything that does dmg
    MimickedFlameThrower = 23116, // 30F5->self, no cast, range 8 90-degree cone, unavoidable
    MimickedSap0 = 23104, // 30F5->self, 3,5s cast, single-target
    MimickedSap1 = 23101, // 233C->location, 4,0s cast, range 8 circle
    MimickedSap2 = 23105, // 30F5->self, 1,5s cast, single-target
    MimickedSap3 = 23106, // 233C->location, 2,0s cast, range 8 circle
    MimickedDoomImpending = 23113, // 30F5->self, 8,0s cast, range 80 circle, applies doom
    MimickedBunshin = 23107, // 30F5->self, 3,0s cast, single-target, summons Imitation
    MimickedFireBlast = 23109, // 30F5->self, 2,0s cast, single-target
    MimickedFireBlast2 = 23110, // 233C->self, 2,0s cast, range 70+R width 4 rect
    MimickedProteanWave = 23111, // 30F6->self, 2,0s cast, single-target
    MimickedProteanWave2 = 23112, // 233C->self, 2,0s cast, range 50 30-degree cone
    MimickedRawInstinct = 23115, // 30F5->self, 3,0s cast, single-target, buffs self with critical strikes, can be removed
    MimickedImpSong = 23114, // 30F5->self, 6,0s cast, range 40 circle, interruptible, turns player into imp
    MimickedFlare = 23098, // Boss->player, 3,0s cast, range 80 circle, possible punishment for ignoring Mimic
    MimickedHoly = 23100, // Boss->player, 3,0s cast, range 6 circle, possible punishment for ignoring Mimic
    MimickedPowerfulHit = 23103, // Boss->player, 3,0s cast, single-target, possible punishment for ignoring Mimic
    MimickedCriticalHit = 23102, // Boss->player, 3,0s cast, single-target, possible punishment for ignoring Mimic
}

public enum SID : uint
{
    Mimicry = 2450, // none->Boss, extra=0x0
    Doom = 1769, // Boss->player, extra=0x0
    Incurable = 1488, // Boss->player, extra=0x0
    CriticalStrikes = 1797, // Boss->Boss, extra=0x0
    DamageUp = 443, // none->Boss, extra=0x1
    Imp = 1103, // Boss->player, extra=0x2E
}

class Mimic(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.Mimic), "Stop attacking when cast ends");
class MimickedSap1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MimickedSap1), 8);
class MimickedSap2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MimickedSap3), 8);
class MimickedDoomImpending(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.MimickedDoomImpending), "Heal to full before cast ends!");
class MimickedProteanWave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MimickedProteanWave2), new AOEShapeCone(50, 15.Degrees()));
class MimickedFireBlast(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MimickedFireBlast2), new AOEShapeRect(70.5f, 2));
class MimickedImpSong(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.MimickedImpSong));
class MimickedRawInstinct(BossModule module) : Components.CastHint(module, ActionID.MakeSpell(AID.MimickedRawInstinct), "Applies buff, dispel it");
class MimickedFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MimickedFlare), "Use Diamondback!");
class MimickedHoly(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MimickedHoly), "Use Diamondback!");
class MimickedCriticalHit(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MimickedCriticalHit), "Use Diamondback!");
class MimickedPowerfulHit(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MimickedPowerfulHit), "Use Diamondback!");

class Hints2(BossModule module) : BossComponent(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var mimicry = Module.PrimaryActor.FindStatus(SID.Mimicry);
        if (mimicry != null)
            hints.Add($"Do no damage!");
        var crit = Module.PrimaryActor.FindStatus(SID.CriticalStrikes);
        if (crit != null)
            hints.Add("Dispel buff!");
    }
}

class Hints(BossModule module) : BossComponent(module)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        hints.Add($"For this fight Diamondback, Exuviation, Flying Sardine and a healing\nability (preferably Pom Cure with healer mimicry) are mandatory.\nEerie Soundwave is also recommended.");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        hints.Add("Requirements for achievement: Take no optional damage and finish faster\nthan ideal time.", false);
    }
}

class Stage31Act1States : StateMachineBuilder
{
    public Stage31Act1States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Mimic>()
            .ActivateOnEnter<MimickedSap1>()
            .ActivateOnEnter<MimickedSap2>()
            .ActivateOnEnter<MimickedDoomImpending>()
            .ActivateOnEnter<MimickedProteanWave>()
            .ActivateOnEnter<MimickedFireBlast>()
            .ActivateOnEnter<MimickedImpSong>()
            .ActivateOnEnter<MimickedFireBlast>()
            .ActivateOnEnter<MimickedRawInstinct>()
            .ActivateOnEnter<MimickedCriticalHit>()
            .ActivateOnEnter<MimickedPowerfulHit>()
            .ActivateOnEnter<MimickedHoly>()
            .ActivateOnEnter<MimickedFlare>()
            .ActivateOnEnter<Hints2>()
            .DeactivateOnEnter<Hints>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.MaskedCarnivale, GroupID = 754, NameID = 9908, SortOrder = 1)]
public class Stage31Act1 : BossModule
{
    public Stage31Act1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 16))
    {
        ActivateComponent<Hints>();
    }
}
