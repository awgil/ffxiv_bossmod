﻿namespace BossMod.RealmReborn.Quest.Devastation;

public enum OID : uint
{
    Boss = 0x3933, // R1.750, x?
    SeaOfPitch = 0x1EB738, // R0.500, x?, EventObj type
    Firesphere = 0x3934, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AncientFireIII = 29327, // Boss->self, 4.0s cast, range 40 circle
    DarkThunder = 29329, // _Gen_Lahabrea->self, 4.0s cast, range 1 circle
    EndOfDays = 29331, // Boss->self, 4.0s cast, range 60 width 8 rect
    EndOfDaysAdds = 29762, // _Gen_PhantomLahabrea->self, 4.0s cast, range 60 width 8 rect
    Nightburn = 29340, // Boss->player, 4.0s cast, single-target
    FiresphereSummon = 29332, // Boss->self, 4.0s cast, single-target
    Burst = 29333, // _Gen_Firesphere->self, 3.0s cast, range 8 circle
    AncientEruption = 29335, // _Gen_Lahabrea->self, 4.0s cast, range 6 circle
    FluidFlare = 29760, // _Gen_Lahabrea->self, 4.0s cast, range 40 60-degree cone
    AncientCross = 29756, // _Gen_Lahabrea->self, 4.0s cast, range 6 circle
    BurstFlare = 29758, // _Gen_Lahabrea->self, 5.0s cast, range 60 circle
    GripOfNight = 29337, // Boss->self, 6.0s cast, range 40 150-degree cone
}

class BurstFlare(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BurstFlare), 10)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // don't add any hints if Burst hasn't gone off yet, it tends to spook AI mode into running into deathwall
        if (Module.Enemies(OID.Firesphere).Any(x => x.CastInfo?.RemainingTime > 0))
            return;

        foreach (var c in Casters)
            hints.AddForbiddenZone(new AOEShapeDonut(5, 100), Arena.Center, default, Module.CastFinishAt(c.CastInfo));
    }
}

class GripOfNight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GripOfNight), new AOEShapeCone(40, 75.Degrees()));

class AncientCross(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AncientCross), new AOEShapeCircle(6), maxCasts: 8);

class AncientEruption(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AncientEruption), new AOEShapeCircle(6));

class FluidFlare(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FluidFlare), new AOEShapeCone(40, 30.Degrees()));

class FireSphere(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Burst))
{
    private DateTime? _predictedCast = null;
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.FiresphereSummon)
            _predictedCast = WorldState.CurrentTime.AddSeconds(12);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Burst)
            _predictedCast = Module.CastFinishAt(spell);
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_predictedCast is DateTime dt && dt > WorldState.CurrentTime)
            foreach (var enemy in Module.Enemies(OID.Firesphere))
                yield return new AOEInstance(new AOEShapeCircle(8), enemy.Position, default, dt);
    }
}

class DevastationStates : StateMachineBuilder
{
    public DevastationStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeathWall>()
            .ActivateOnEnter<DarkThunder>()
            .ActivateOnEnter<SeaOfPitch>()
            .ActivateOnEnter<AncientFire>()
            .ActivateOnEnter<EndOfDays>()
            .ActivateOnEnter<Nightburn>()
            .ActivateOnEnter<FireSphere>()
            .ActivateOnEnter<AncientEruption>()
            .ActivateOnEnter<FluidFlare>()
            .ActivateOnEnter<AncientCross>()
            .ActivateOnEnter<BurstFlare>()
            .ActivateOnEnter<GripOfNight>()
            .ActivateOnEnter<EndOfDaysAdds>()
            ;
    }
}

class Nightburn(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Nightburn), "WoLbuster");

class AncientFire(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AncientFireIII), hint: "Raidwide + spawn deathwall");

class DeathWall(BossModule module) : BossComponent(module)
{
    private bool _active = false;
    private bool _completed = false;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AncientFireIII && !_completed)
            _active = true;
    }
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_active)
            new AOEShapeDonut(15, 100).Draw(Arena, Arena.Center, default, ArenaColor.AOE);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_active)
            hints.AddForbiddenZone(new AOEShapeDonut(15, 100), Arena.Center);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0 && state == 0x20001)
        {
            Module.Arena.Bounds = new ArenaBoundsCircle(15);
            _completed = true;
            _active = false;
        }
    }
}

class DarkThunder(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DarkThunder), new AOEShapeCircle(1));

class SeaOfPitch(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.SeaOfPitch).Where(x => x.EventState != 7));

class EndOfDays(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EndOfDays), new AOEShapeRect(60, 4));
class EndOfDaysAdds(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EndOfDaysAdds), new AOEShapeRect(60, 4));

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 867, NameID = 2143)]
public class Devastation(WorldState ws, Actor primary) : BossModule(ws, primary, new(-704, 480), new ArenaBoundsCircle(20));

