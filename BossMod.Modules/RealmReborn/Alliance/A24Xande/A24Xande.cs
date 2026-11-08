namespace BossMod.RealmReborn.Alliance.A24Xande;

public enum OID : uint
{
    Boss = 0xBDB, // R4.000, x1
    Helper = 0x19A, // R0.500, x5
    StarfallCircle = 0xBDC, // R1.000, spawns in the center during the untargetable phases
    StonefallCircle = 0xBDD, // R1.000, spawns on the ring ~26y out during the untargetable phases
    AetherialMine = 0xBDF, // R1.000, x8, has to be soaked
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    DoubleAxeHandle = 2354, // Boss->player, no cast, single-target
    KnucklePress = 2355, // Boss->self, 3.0s cast, range 6+R circle
    BurningRaveBoss = 2356, // Boss->location, 3.5s cast, range 8 circle
    BurningRave = 2357, // Helper->location, 3.5s cast, range 8 circle
    AuraCannon = 2358, // Boss->self, 3.0s cast, range 60+R width 10 rect
    AncientQuake = 2359, // Boss->self, no cast, range 60 circle, raidwide
    AncientQuaga = 2361, // Boss->self, 5.0s cast, range 60 circle, hits everyone without levitation
    AetherochemicalReaction = 2365, // AetherialMine->self, no cast, range 60+R circle, raidwide from an unsoaked mine
    AetherochemicalExplosion = 2366, // AetherialMine->self, no cast, range 3 circle, soaked mine
    Imperium = 2367, // Helper->location, no cast, range 6 circle stack, grants levitation
}

public enum SID : uint
{
    Levitation = 12, // none->player, extra=0x0, while standing on a pad left by imperium
}

public enum IconID : uint
{
    Imperium = 317, // player
}

class KnucklePress(BossModule module) : Components.StandardAOEs(module, AID.KnucklePress, 10);
class BurningRave(BossModule module) : Components.GroupedAOEs(module, [AID.BurningRaveBoss, AID.BurningRave], new AOEShapeCircle(8));
class AuraCannon(BossModule module) : Components.StandardAOEs(module, AID.AuraCannon, new AOEShapeRect(64, 5));
class AncientQuaga(BossModule module) : Components.CastCounter(module, AID.AncientQuaga);
class Imperium(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Imperium, AID.Imperium, 6, 7.1f, 4)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Imperium && Stacks.Count > 0)
        {
            Stacks.Remove(Stacks.MinBy(s => (s.Target.Position - spell.TargetXZ).LengthSq()));
            ++NumFinishedStacks;
        }
    }
}

class Levitation(BossModule module) : BossComponent(module)
{
    private const float PadRadius = 5;
    private readonly List<WPos> _pads = [];
    private DateTime _quagaAt;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AncientQuaga)
            _quagaAt = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Imperium:
                _pads.Add(spell.TargetXZ);
                break;
            case AID.AncientQuaga:
                _quagaAt = default;
                _pads.Clear();
                break;
        }
    }

    private bool OnPad(Actor actor) => _pads.Any(p => actor.Position.InCircle(p, PadRadius));

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_quagaAt != default && _pads.Count > 0)
            hints.Add("Stand on a levitation pad!", !OnPad(actor));
        else if (_quagaAt != default && actor.FindStatus(SID.Levitation) == null)
            hints.Add("Stack for imperium to get levitation!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_pads.Count == 0)
            return;

        if (_quagaAt != default)
        {
            hints.AddForbiddenZone(ShapeDistance.Intersection([.. _pads.Select(p => ShapeDistance.InvertedCircle(p, PadRadius))]), _quagaAt);
        }
        else if (!OnPad(actor))
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(_pads.MinBy(p => (p - actor.Position).LengthSq()), PadRadius, 5));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var p in _pads)
            Arena.ZoneCircle(p, PadRadius, ArenaColor.SafeFromAOE);
    }
}

class AetherialMines(BossModule module) : Components.GenericTowers(module)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.AetherialMine)
            Towers.Add(new(actor.Position, 3, 1, int.MaxValue, activation: WorldState.FutureTime(9.8f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AetherochemicalExplosion or AID.AetherochemicalReaction)
        {
            ++NumCasts;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in Towers)
            DrawTower(Arena, t.Position, t.Radius, t.IsInside(pc) || t.NumInside(Module) < t.MinSoakers);
    }
}

class Circles(BossModule module) : Components.AddsMulti(module, [OID.StarfallCircle, OID.StonefallCircle], 2)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveActors.Any())
            hints.Add("Kill the starfall and stonefall circles!");
    }
}

class A24XandeStates : StateMachineBuilder
{
    private const float Late = 1000;

    public A24XandeStates(BossModule module) : base(module)
    {
        DeathPhase(0, Fight)
            .ActivateOnEnter<KnucklePress>()
            .ActivateOnEnter<AncientQuaga>()
            .ActivateOnEnter<BurningRave>()
            .ActivateOnEnter<AuraCannon>()
            .ActivateOnEnter<Imperium>()
            .ActivateOnEnter<Levitation>()
            .ActivateOnEnter<AetherialMines>()
            .ActivateOnEnter<Circles>();
    }

    private void Fight(uint id)
    {
        ComponentCondition<AetherialMines>(id, 28.1f, c => c.Towers.Count > 0, "Mines", Late);
        ComponentCondition<AetherialMines>(id + 0x10000, 9.8f, c => c.NumCasts >= 1, "Mines resolve", Late);
        ComponentCondition<KnucklePress>(id + 0x20000, 1.2f, c => c.NumCasts >= 1, "Out", Late);
        Downtime(id + 0x30000, 8.1f, 25.0f);
        ComponentCondition<Imperium>(id + 0x40000, 8.1f, c => c.NumFinishedStacks >= 1, "Stacks", Late);
        ComponentCondition<AncientQuaga>(id + 0x50000, 8.2f, c => c.NumCasts >= 1, "Quaga", Late);
        ComponentCondition<AuraCannon>(id + 0x60000, 14.6f, c => c.NumCasts >= 1, "Line", Late);
        Downtime(id + 0x70000, 5.0f, 28.8f);
        ComponentCondition<Imperium>(id + 0x80000, 8.2f, c => c.NumFinishedStacks >= 4, "Stacks", Late);
        ComponentCondition<AncientQuaga>(id + 0x90000, 4.1f, c => c.NumCasts >= 2, "Quaga", Late);
        ComponentCondition<AuraCannon>(id + 0xA0000, 18.6f, c => c.NumCasts >= 2, "Line", Late);
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void Downtime(uint id, float delay, float duration)
    {
        Targetable(id, false, delay, "Disappear");
        ComponentCondition<Circles>(id + 0x100, 7.5f, c => c.ActiveActors.Any(), "Circles", Late);
        Targetable(id + 0x200, true, duration, "Return");
    }
}

[ModuleInfo(Contributors = "Nonu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 102, NameID = 2824, SortOrder = 8)]
public class A24Xande(ModuleInit init) : BossModule(init, new(-400, -200), new ArenaBoundsCircle(30));
