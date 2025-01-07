namespace BossMod.Stormblood.Quest.Naadam;

public enum OID : uint
{
    Boss = 0x1B31,
    Helper = 0x233C,
    _Gen_BuduganShaman = 0x1B33, // R0.500, x1
    _Gen_BuduganHunter = 0x1B32, // R0.500, x2 (spawn during fight)
    _Gen_BuduganWarrior = 0x1B30, // R0.500, x2 (spawn during fight)
    _Gen_OroniriBrother = 0x1B2F, // R0.500, x0 (spawn during fight)
    _Gen_DotharliHunter = 0x1B35, // R0.500, x0 (spawn during fight)
    _Gen_MagnaiTheOlder = 0x1B38, // R0.500, x0 (spawn during fight)
    _Gen_DotharliSpiritcaller = 0x1B36, // R0.500, x0 (spawn during fight)
    _Gen_MagnaiTheOlder1 = 0x18D6, // R0.500, x0 (spawn during fight)
    _Gen_DotharliWarrior = 0x1B34, // R0.500, x0 (spawn during fight)
    _Gen_SaduHeavensflame = 0x1B39, // R0.500, x0 (spawn during fight)
    _Gen_OroniriWarrior = 0x1E2B, // R0.500, x0 (spawn during fight)
    _Gen_BuduganWarrior1 = 0x1E2C, // R0.500, x0 (spawn during fight)
    _Gen_DotharliHunter1 = 0x1E2D, // R0.500, x0 (spawn during fight)
    _Gen_DotharliHunter2 = 0x1B37, // R0.500, x0 (spawn during fight)
    _Gen_DotharliHunter3 = 0x1DDA, // R0.500, x0 (spawn during fight)
    _Gen_OroniriHunter = 0x1DD9, // R0.500, x0 (spawn during fight)
    _Gen_StellarChuluu = 0x1B3F, // R1.800, x0 (spawn during fight)
    _Gen_StellarChuluu1 = 0x1B40, // R1.800, x0 (spawn during fight)
    _Gen_DomanHoplomachus = 0x1B3C, // R0.500, x0 (spawn during fight)
    _Gen_DomanSignifer = 0x1B3E, // R0.500, x0 (spawn during fight)
    _Gen_DomanLaquierius = 0x1B3D, // R0.500, x0 (spawn during fight)
    _Gen_ArmoredWeapon = 0x1B3B, // R5.400, x0 (spawn during fight)
    _Gen_Grynewaht = 0x1B3A, // R0.500, x0 (spawn during fight)
    Ovoo = 0x1EA4E1
}

public enum AID : uint
{
    Holmgang = 8391,
    _AutoAttack_Attack = 873, // _Gen_BuduganHunter->player/1B2D, no cast, single-target
    _Spell_Stone = 970, // _Gen_BuduganShaman->1B49, 1.0s cast, single-target
    _Weaponskill_TrueThrust = 722, // Boss/_Gen_DotharliHunter/_Gen_DotharliHunter1/_Gen_OroniriHunter/_Gen_DotharliHunter3->player/1B2E/1B48/_Gen_BuduganWarrior/1B2B/1B2C, no cast, single-target
    _AutoAttack_Attack1 = 871, // Boss/_Gen_DotharliHunter/_Gen_DotharliHunter1/_Gen_OroniriHunter/_Gen_DotharliHunter3->player/1B2E/1B48/_Gen_BuduganWarrior/1B2B/1B2C, no cast, single-target
    _AutoAttack_Attack2 = 870, // _Gen_BuduganWarrior/_Gen_DotharliWarrior/_Gen_OroniriBrother/_Gen_BuduganWarrior1/_Gen_OroniriWarrior/_Gen_MagnaiTheOlder->player/1B2B/1B2C/_Gen_DotharliSpiritcaller/_Gen_OroniriBrother/_Gen_DotharliWarrior/_Gen_DotharliHunter/1B2D/1B2E/_Gen_StellarChuluu/_Gen_SaduHeavensflame, no cast, single-target
    _Weaponskill_HeavyShot = 8185, // _Gen_BuduganHunter->player/1B2D, no cast, single-target
    _Weaponskill_HeavySwing = 8186, // _Gen_BuduganWarrior/_Gen_DotharliWarrior/_Gen_BuduganWarrior1/_Gen_MagnaiTheOlder->player/1B2C/1B2B/_Gen_DotharliSpiritcaller/_Gen_OroniriBrother/_Gen_DotharliHunter/1B2D/1B2E/_Gen_StellarChuluu, no cast, single-target
    _Weaponskill_FastBlade = 717, // _Gen_OroniriBrother/_Gen_OroniriWarrior->player/_Gen_DotharliWarrior, no cast, single-target
    _Spell_Fire = 966, // _Gen_DotharliSpiritcaller->player/_Gen_BuduganWarrior, 1.0s cast, single-target
    _Weaponskill_Tomahawk = 8390, // _Gen_MagnaiTheOlder->1B2E, no cast, single-target
    _Spell_Fire1 = 9123, // _Gen_SaduHeavensflame->_Gen_MagnaiTheOlder, no cast, single-target
    _Spell_Fire2 = 9120, // _Gen_StellarChuluu->player, 1.0s cast, single-target
    _Weaponskill_BrokenRidge = 8395, // _Gen_MagnaiTheOlder->self, 5.0s cast, range 50 circle
    _Weaponskill_ViolentEarth = 8388, // _Gen_MagnaiTheOlder->self, 1.5s cast, single-target
    _Spell_ViolentEarth = 8389, // _Gen_MagnaiTheOlder1->location, 3.0s cast, range 6 circle
    _Weaponskill_ViolentEarth1 = 8533, // _Gen_MagnaiTheOlder->self, no cast, single-target
    _Spell_DispellingWind = 8394, // _Gen_SaduHeavensflame->self, 3.0s cast, range 40+R width 8 rect
    _Weaponskill_FallingDusk = 8392, // _Gen_SaduHeavensflame->location, 60.0s cast, range 25 circle
    _Weaponskill_Epigraph = 8339, // 1A58->self, 3.0s cast, range 45+R width 8 rect

    _AutoAttack_Attack3 = 872, // _Gen_Grynewaht->player, no cast, single-target
    _Weaponskill_AugmentedShatter = 8494, // _Gen_Grynewaht->player, no cast, single-target
    _Weaponskill_MagitekCannon = 9121, // _Gen_ArmoredWeapon->1DBE, no cast, single-target
    _Weaponskill_DiffractiveLaser = 9122, // _Gen_ArmoredWeapon->location, 3.0s cast, range 5 circle
    _Weaponskill_AugmentedSuffering = 8492, // _Gen_Grynewaht->self, 3.5s cast, range 6+R circle
    _Weaponskill_AugmentedUprising = 8493, // _Gen_Grynewaht->self, 3.0s cast, range 8+R 120-degree cone
}

public enum SID : uint
{
    EarthenAccord = 778
}

class DiffractiveLaser(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_DiffractiveLaser), 5);
class AugmentedSuffering(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AugmentedSuffering), new AOEShapeCircle(6.5f));
class AugmentedUprising(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_AugmentedUprising), new AOEShapeCone(8.5f, 60.Degrees()));

class ViolentEarth(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_ViolentEarth), 6);
class DispellingWind(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Spell_DispellingWind), new AOEShapeRect(40.5f, 4));
class Epigraph(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Weaponskill_Epigraph), new AOEShapeRect(45, 4));

class DrawOvoo : BossComponent
{
    private Actor? Ovoo => WorldState.Actors.FirstOrDefault(o => o.OID == 0x1EA4E1);

    public DrawOvoo(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(Ovoo, ArenaColor.Object, true);
    }
}

class ActivateOvoo(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.MountId == 117)
            hints.WantDismount = true;

        var beingAttacked = false;

        foreach (var e in hints.PotentialTargets)
        {
            if (e.Actor.TargetID == actor.InstanceID)
                beingAttacked = true;
            else
                e.Priority = AIHints.Enemy.PriorityForbidden;
        }

        var ovoo = WorldState.Actors.FirstOrDefault(x => x.OID == 0x1EA4E1);
        if (!beingAttacked && (ovoo?.IsTargetable ?? false))
            hints.InteractWithTarget = ovoo;
    }
}

class ProtectOvoo(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var e in hints.PotentialTargets)
        {
            if (e.Actor.FindStatus(SID.EarthenAccord) != null)
                e.Priority = 5;
            else if (e.Actor.OID == (uint)OID._Gen_StellarChuluu)
                e.Priority = 1;
            else
                e.Priority = 0;
        }
    }
}

class ProtectSadu(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var chuluu = WorldState.Actors.Where(x => (OID)x.OID == OID._Gen_StellarChuluu1).Select(x => x.InstanceID).ToList();

        foreach (var e in hints.PotentialTargets)
        {
            if (chuluu.Contains(e.Actor.TargetID))
                e.Priority = 5;
            else if ((OID)e.Actor.OID == OID._Gen_Grynewaht)
                e.Priority = 1;
            else
                e.Priority = 0;
        }
    }
}

class OvooStates : StateMachineBuilder
{
    public OvooStates(BossModule module) : base(module)
    {
        bool DutyEnd() => module.WorldState.CurrentCFCID != 246;

        TrivialPhase()
            .ActivateOnEnter<ActivateOvoo>()
            .ActivateOnEnter<DrawOvoo>()
            .Raw.Update = () => Module.WorldState.Actors.Any(x => x.OID == (uint)OID._Gen_MagnaiTheOlder && x.IsTargetable) || DutyEnd();
        TrivialPhase(1)
            .ActivateOnEnter<ProtectOvoo>()
            .ActivateOnEnter<ActivateOvoo>()
            .ActivateOnEnter<ViolentEarth>()
            .ActivateOnEnter<DispellingWind>()
            .ActivateOnEnter<Epigraph>()
            .Raw.Update = () => Module.WorldState.Actors.Any(x => x.OID == (uint)OID._Gen_Grynewaht && x.IsTargetable) || DutyEnd();
        TrivialPhase(2)
            .ActivateOnEnter<DiffractiveLaser>()
            .ActivateOnEnter<AugmentedSuffering>()
            .ActivateOnEnter<AugmentedUprising>()
            .ActivateOnEnter<ProtectSadu>()
            .Raw.Update = DutyEnd;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 68051, PrimaryActorOID = (uint)OID.Ovoo)]
public class Ovoo(WorldState ws, Actor primary) : BossModule(ws, primary, new(354, 296.5f), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => Raid.Player()?.Position.InCircle(PrimaryActor.Position, 15) ?? false;

    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);
}

