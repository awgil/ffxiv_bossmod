namespace BossMod.Dawntrail.Foray.CriticalEngagement.CloisterDemon;

public enum OID : uint
{
    Boss = 0x46C9, // R11.400, x1
    Helper = 0x233C, // R0.500, x26 (spawn during fight), Helper type
    BallOfFire = 0x47F0, // R2.300, x0 (spawn during fight)
    CloisterTorch = 0x46CB, // R3.600, x0 (spawn during fight)

    Seal = 0x1EBCFC
}

public enum AID : uint
{
    AutoAttack = 41363, // Boss->player, no cast, single-target
    SundersealRoarCast = 41336, // Boss->self, 5.0s cast, ???
    SundersealRoar = 41337, // Helper->self, no cast, ???
    VoidThunderIII = 41358, // Helper->self, 5.0s cast, range 60 width 8 rect
    Unk1 = 41338, // Boss->self, no cast, single-target
    GreatBallOfFire = 41354, // Boss->self, 4.0s cast, single-target
    Explosion = 41357, // BallOfFire->self, 5.0s cast, range 22 circle
    GigaflareCast = 41361, // Boss->self, 5.0s cast, ???
    Gigaflare = 41362, // Helper->self, no cast, ???
    TidalBreath = 41360, // Boss->self, 7.0s cast, range 40 180-degree cone
    KarmicDrain = 41359, // Helper->self, 5.0s cast, range 40 60-degree cone
    BlazingFlare = 41355, // Boss->self, 4.0s cast, single-target
    Flare = 41356, // Helper->self, 6.0s cast, range 10 circle
    SealAsunder1 = 41339, // Boss->self, 33.0s cast, single-target
    SealAsunder2 = 41340, // Boss->self, 40.0s cast, single-target
    SealAsunder3 = 41341, // Boss->self, 46.0s cast, single-target
}

class VoidThunderIII(BossModule module) : Components.GenericAOEs(module, AID.VoidThunderIII)
{
    private readonly List<Actor> _castersN = [];
    private readonly List<Actor> _castersE = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _castersN.Take(4))
            yield return new AOEInstance(new AOEShapeRect(60, 4), c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
        foreach (var c in _castersE.Take(4))
            yield return new AOEInstance(new AOEShapeRect(60, 4), c.CastInfo!.LocXZ, c.CastInfo!.Rotation, Module.CastFinishAt(c.CastInfo));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (caster.Rotation.AlmostEqual(default, 0.1f))
                _castersN.Add(caster);
            else
                _castersE.Add(caster);
        }
    }

    // casts get interrupted when the seal mechanic is resolved
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _castersN.Remove(caster);
            _castersE.Remove(caster);
        }
    }
}

class Seal(BossModule module) : Components.GenericTowers(module, damageType: AIHints.PredictedDamageType.None)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Seal)
            Towers.Add(new(actor.Position, 3, 4, int.MaxValue, activation: Module.CastFinishAt(Module.PrimaryActor.CastInfo, fallback: DateTime.MaxValue - TimeSpan.FromSeconds(100))));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if ((OID)actor.OID == OID.Seal && state == 0x00040008)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 0.1f));
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.Seal)
            Towers.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 0.1f));
    }
}

class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, 22);
class SundersealRoar(BossModule module) : Components.RaidwideCast(module, AID.SundersealRoarCast);
class Gigaflare(BossModule module) : Components.RaidwideCast(module, AID.GigaflareCast);
class CloisterTorch(BossModule module) : Components.Adds(module, (uint)OID.CloisterTorch, 2);
class TidalBreath(BossModule module) : Components.StandardAOEs(module, AID.TidalBreath, new AOEShapeCone(40, 90.Degrees()));
class KarmicDrain(BossModule module) : Components.StandardAOEs(module, AID.KarmicDrain, new AOEShapeCone(40, 30.Degrees()), maxCasts: 3);
class Flare(BossModule module) : Components.StandardAOEs(module, AID.Flare, 10);

class SealAsunder1(BossModule module) : Components.CastHint(module, AID.SealAsunder1, "Enrage!", true);
class SealAsunder2(BossModule module) : Components.CastHint(module, AID.SealAsunder2, "Enrage!", true);
class SealAsunder3(BossModule module) : Components.CastHint(module, AID.SealAsunder3, "Enrage!", true);

class CloisterDemonStates : StateMachineBuilder
{
    public CloisterDemonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VoidThunderIII>()
            .ActivateOnEnter<Seal>()
            .ActivateOnEnter<Explosion>()
            .ActivateOnEnter<SundersealRoar>()
            .ActivateOnEnter<Gigaflare>()
            .ActivateOnEnter<CloisterTorch>()
            .ActivateOnEnter<TidalBreath>()
            .ActivateOnEnter<KarmicDrain>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<SealAsunder1>()
            .ActivateOnEnter<SealAsunder2>()
            .ActivateOnEnter<SealAsunder3>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13666)]
public class CloisterDemon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-340, 800), new ArenaBoundsCircle(29.5f))
{
    public override bool DrawAllPlayers => true;
}

