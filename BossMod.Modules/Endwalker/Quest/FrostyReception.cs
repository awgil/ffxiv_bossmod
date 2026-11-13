namespace BossMod.Endwalker.Quest.AFrostyReception;

public enum OID : uint
{
    Boss = 0x3646,
    Helper = 0x233C,
    LockOn = 0x3648, // R1.200, x0 (spawn during fight)
    DotharliKhunChuluu = 0x364A, // Ally
}

public enum AID : uint
{
    GigaTempest = 27440, // Boss->self, 5.0s cast, range 20 circle
    SpiralScourge = 27441, // Boss->player/Lucia, 4.0s cast, single-target
    Ruination1 = 27443, // Boss->self, 5.0s cast, range 40 width 8 cross
    Ruination2 = 27444, // Helper->self, 5.0s cast, range 30 width 8 rect
    GunberdVisual = 27447, // Boss->self, 5.0s cast, single-target
    Gunberd = 27448, // Boss->players/allies, no cast, range 40 width 6 line stack
    ResinBomb = 27449, // Helper->Lyse/Magnai/Sadu/Pipin/Lucia/Cirina, 5.0s cast, range 5 circle
    LockOn1 = 27461, // _Gen_6->self, 1.0s cast, range 6 circle
    MagitekCannon = 27457, // _Gen_TelotekReaper1->player/Lyse/Sadu/Magnai/Lucia/Cirina/Pipin, 5.0s cast, range 6 circle
    Bombardment = 27459, // Helper->location, 4.0s cast, range 6 circle
    BombsAway1 = 27460, // Boss->self, 3.0s cast, single-target
    BombsAway2 = 27462, // Boss->self, 3.0s cast, single-target
    LockOn2 = 27463, // _Gen_6->self, 1.0s cast, range 6 circle
    GaseousBomb = 27464, // Helper->self, 10.0s cast, range 20 proximity
    AetherochemicalGrenadoVisual = 27501, // Boss->player, 5.0s cast, single-target
    AetherochemicalGrenado = 27510, // Helper->player, no cast, range 6 circle
}

class GigaTempest(BossModule module) : Components.RaidwideCast(module, AID.GigaTempest);
class SpiralScourge(BossModule module) : Components.SingleTargetCast(module, AID.SpiralScourge);
class GaseousBomb(BossModule module) : Components.ProximityAOEs(module, AID.GaseousBomb, 18);
class Ruination(BossModule module) : Components.StandardAOEs(module, AID.Ruination1, new AOEShapeCross(40, 4));
class Ruination2(BossModule module) : Components.StandardAOEs(module, AID.Ruination2, new AOEShapeRect(30, 4));
class ResinBomb(BossModule module) : Components.SpreadFromCastTargets(module, AID.ResinBomb, 5);
class MagitekCannon(BossModule module) : Components.SpreadFromCastTargets(module, AID.MagitekCannon, 6);
class Bombardment(BossModule module) : Components.StandardAOEs(module, AID.Bombardment, 6);

class Gunberd(BossModule module) : Components.GenericAOEs(module, AID.Gunberd, invertedText: "Stack in the line!")
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_aoe is { } aoe)
            hints.AddPredictedDamage(Raid.WithSlot().Mask(), aoe.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GunberdVisual)
            _aoe = new(new AOEShapeRect(40, 3), caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.2f), Inverted: true);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Gunberd)
            _aoe = null;
    }
}

class AetherochemicalGrenado(BossModule module) : Components.GenericBaitAway(module, AID.AetherochemicalGrenado, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AetherochemicalGrenadoVisual && WorldState.Actors.Find(spell.TargetID) is { } target)
            CurrentBaits.Add(new(caster, target, new AOEShapeCircle(6), Module.CastFinishAt(spell, 0.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AetherochemicalGrenado)
            CurrentBaits.Clear();
    }
}

class LockOn(BossModule module) : Components.GenericAOEs(module)
{
    private class Caster
    {
        public required Actor Actor;
        public required DateTime FinishAt;
    }

    private readonly List<Caster> Casters = [];
    private float _delay = 6.6f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCircle(6), c.Actor.Position, default, c.FinishAt));

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.LockOn)
            Casters.Add(new() { Actor = actor, FinishAt = WorldState.FutureTime(_delay) });
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BombsAway1)
            _delay = 6.6f;
        else if ((AID)spell.Action.ID == AID.BombsAway2)
            _delay = 9;

        if ((AID)spell.Action.ID is AID.LockOn1 or AID.LockOn2)
        {
            var c = Casters.FindIndex(p => p.Actor == caster);
            if (c >= 0)
                Casters[c].FinishAt = Module.CastFinishAt(spell);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LockOn1 or AID.LockOn2)
            Casters.RemoveAll(c => c.Actor == caster);
    }
}

class VergiliaVanCorculumStates : StateMachineBuilder
{
    public VergiliaVanCorculumStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GigaTempest>()
            .ActivateOnEnter<SpiralScourge>()
            .ActivateOnEnter<Gunberd>()
            .ActivateOnEnter<AetherochemicalGrenado>()
            .ActivateOnEnter<GaseousBomb>()
            .ActivateOnEnter<Ruination>()
            .ActivateOnEnter<Ruination2>()
            .ActivateOnEnter<LockOn>()
            .ActivateOnEnter<ResinBomb>()
            .ActivateOnEnter<MagitekCannon>()
            .ActivateOnEnter<Bombardment>();
    }
}

[ModuleInfo(Contributors = "xan, Kagekazu", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 69919, NameID = 10572)]
public class VergiliaVanCorculum(ModuleInit init) : BossModule(init, new(0, -78), new ArenaBoundsCircle(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc) => Arena.Actors(WorldState.Actors.Where(x => !x.IsAlly), ArenaColor.Enemy);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var enemy in hints.PotentialTargets)
            if (WorldState.Actors.Find(enemy.Actor.TargetID)?.OID == (uint)OID.DotharliKhunChuluu)
                enemy.Priority = 1;
    }
}
