﻿using BossMod.Autorotation;
using RID = BossMod.Roleplay.AID;

namespace BossMod.Shadowbringers.Quest.DeathUntoDawn.P3;

public enum OID : uint
{
    Boss = 0x3201,
    Helper = 0x233C,
    MoonGana = 0x3219,
    SpiritGana = 0x321A,
    RavanasWill = 0x321B,
}

public enum AID : uint
{
    _Ability_Explosion = 24046, // 3204->self, 5.0s cast, range 80 width 10 cross
    _Ability_AtmaLinga = 24052, // Boss->self, no cast, range 40 circle
    _Ability_SoulsOfWar = 24037, // Boss->self, 2.0s cast, single-target
    _Spell_Freeze = 24039, // SpiritGana->2E2E, 8.0s cast, single-target
    _Ability_BloodyFuller = 24040, // Boss->self, 6.0s cast, range 40 circle
    _Ability_Chandrahas = 24041, // Boss->self, no cast, range 40 circle
    _Ability_BlindingBlade = 24051, // Boss->player, no cast, range 11 ?-degree cone
    _Ability_TheSeeingTail = 24047, // Boss->self, 1.5s cast, single-target
    _Ability_Revengeance = 24048, // Helper->player, no cast, single-target
    _Ability_ = 24045, // Boss->self, no cast, single-target
    _Spell_Flare = 24038, // MoonGana->2E2E, 8.0s cast, single-target
}

public enum SID : uint
{
    Invincibility = 325,
}

class GrahaAI(WorldState ws) : UnmanagedRotation(ws, 25)
{
    private IEnumerable<Actor> Adds => World.Actors.Where(x => (OID)x.OID is OID.MoonGana or OID.SpiritGana or OID.RavanasWill && x.IsTargetable && !x.IsDead);

    // Ravana's Wills just move to boss, whereas butterflies are only a threat once they start casting
    private bool ShouldBreak(Actor a) => StatusDetails(a, Roleplay.SID.Break, Player.InstanceID).Left == 0 && ((OID)a.OID == OID.RavanasWill || a.CastInfo != null);

    protected override void Exec(Actor? primaryTarget)
    {
        var adds = Adds.ToList();

        if (adds.Any(ShouldBreak))
        {
            Hints.GoalZones.Add(p => adds.Count(a => a.Position.InCircle(p, 20)));
            if (adds.Any(a => ShouldBreak(a) && a.Position.InCircle(Player.Position, 20)))
                UseAction(RID.Break, Player);
        }

        if (MP >= 1000 && Player.HPMP.CurHP * 3 < Player.HPMP.MaxHP)
            UseAction(RID.CureII, Player);

        if (MP < 800)
            UseAction(RID.AllaganBlizzardIV, primaryTarget);

        if (primaryTarget?.OID == 0x3201)
        {
            var thunder = StatusDetails(primaryTarget, Roleplay.SID.ThunderIV, Player.InstanceID);
            if (thunder.Left < 3)
                UseAction(RID.ThunderIV, primaryTarget);
        }

        switch (ComboAction)
        {
            case RID.FireIV:
                UseAction(RID.FireIV2, primaryTarget);
                break;
            case RID.FireIV2:
                UseAction(RID.FireIV3, primaryTarget);
                break;
            case RID.FireIV3:
                UseAction(RID.Foul, primaryTarget);
                break;
            default:
                UseAction(RID.FireIV, primaryTarget);
                break;
        }
    }
}

class AutoGraha(BossModule module) : Components.RotationModule<GrahaAI>(module);
class DirectionalParry(BossModule module) : Components.DirectionalParry(module, 0x3201)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Module.PrimaryActor.FindStatus(680) != null)
        {
            var dist = new AOEShapeCone(100, 45.Degrees()).Distance(Module.PrimaryActor.Position, Module.PrimaryActor.Rotation);
            hints.AddForbiddenZone(dist, WorldState.FutureTime(1));
            if (dist(actor.Position) < 0)
                hints.ForcedTarget = actor;
        }
    }
}
class Explosion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID._Ability_Explosion), new AOEShapeCross(80, 5), maxCasts: 2);

class LunarRavanaStates : StateMachineBuilder
{
    public LunarRavanaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AutoGraha>()
            .ActivateOnEnter<DirectionalParry>()
            .ActivateOnEnter<Explosion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69602, NameID = 10037)]
public class LunarRavana(WorldState ws, Actor primary) : BossModule(ws, primary, new(-144, 83), new ArenaBoundsCircle(20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var h in hints.PotentialTargets)
            h.Priority = h.Actor.FindStatus(SID.Invincibility) == null ? 1 : 0;
    }
}
