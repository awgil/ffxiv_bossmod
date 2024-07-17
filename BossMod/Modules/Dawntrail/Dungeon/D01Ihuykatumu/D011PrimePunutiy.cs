namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D011PrimePunutiy;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x16 (spawn during fight), 523 type
    Boss = 0x4190, // R7.990, x1
    ProdigiousPunutiy = 0x4191, // R4.230, x0 (spawn during fight)
    Punutiy = 0x4192, // R2.820, x0 (spawn during fight)
    PetitPunutiy = 0x4193, // R2.115, x0 (spawn during fight)
    IhuykatumuFlytrap = 0x4194, // R1.600, x0 (spawn during fight)
}

public enum AID : uint
{
    PunutiyPress = 36492, // Boss->self, 5.0s cast, range 60 circle
    Hydrowave = 36493, // Boss->self, 4.0s cast, range 60 30-degree cone
    AddsHydrowave = 36509,
    Resurface = 36494, // Boss->self, 5.0s cast, range 100 60-degree cone
    Resurface2 = 36495, // Boss->self, 7.0s cast, single-target
    Bury1 = 36497, // 233C->self, 4.0s cast, range 12 circle
    Bury2 = 36498, // 233C->self, 4.0s cast, range 8 circle
    Bury3 = 36499, // 233C->self, 4.0s cast, range 25 width 6 rect
    Bury4 = 36500, // 233C->self, 4.0s cast, range 35 width 10 rect
    Bury5 = 36501, // 233C->self, 4.0s cast, range 4 circle
    Bury6 = 36502, // 233C->self, 4.0s cast, range 6 circle
    Bury7 = 36503, // 233C->self, 4.0s cast, range 25 width 6 rect
    Bury8 = 36504, // 233C->self, 4.0s cast, range 35 width 10 rect
    Decay = 36505, // 4194->self, 7.0s cast, range ?-40 donut
    ShoreShaker = 36514, // Boss->self, 4.0+1.0s cast, single-target
    ShoreShaker1 = 36515, // 233C->self, 5.0s cast, range 10 circle
    ShoreShaker2 = 36516, // 233C->self, 7.0s cast, range ?-20 donut
    ShoreShaker3 = 36517, // 233C->self, 9.0s cast, range ?-30 donut
}

class PunutiyFlop(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PunutiyPress));
class Hydrowave(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Hydrowave), new AOEShapeCone(60, 15.Degrees()));

class Bury(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, AOEInstance AOE)> _activeAOEs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _activeAOEs.Select(x => x.AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? toAdd = (AID)spell.Action.ID switch
        {
            AID.Bury1 => new AOEShapeCircle(12),
            AID.Bury2 => new AOEShapeCircle(8),
            AID.Bury3 or AID.Bury7 => new AOEShapeRect(25, 3),
            AID.Bury4 or AID.Bury8 => new AOEShapeRect(35, 5),
            AID.Bury5 => new AOEShapeCircle(4),
            AID.Bury6 => new AOEShapeCircle(6),
            _ => null
        };
        if (toAdd != null)
            _activeAOEs.Add((caster, new AOEInstance(toAdd, caster.Position, spell.Rotation, Module.CastFinishAt(spell))));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        _activeAOEs.RemoveAll(x => x.Caster == caster);
    }
}

class Resurface(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Resurface)
            _aoe = new AOEInstance(new AOEShapeCone(100, 32.Degrees()), caster.Position, spell.Rotation, Module.CastFinishAt(spell));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Resurface2)
            _aoe = null;
    }
}

class Decay(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Decay), new AOEShapeDonut(5, 40))
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
        => Arena.Actors(Module.Enemies(OID.IhuykatumuFlytrap).Where(x => !x.IsDead), ArenaColor.Object, allowDeadAndUntargetable: true);
}

abstract class TetherBait(BossModule module, bool centerAtTarget = false) : Components.GenericBaitAway(module, default, true, centerAtTarget)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var b in ActiveBaits)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
            Arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID != 17)
            return;

        var tar = WorldState.Actors.Find(tether.Target);
        if (tar == null)
            return;

        OnTetherCreated(source, tar);
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == 17)
            CurrentBaits.Clear();
    }

    protected abstract void OnTetherCreated(Actor source, Actor target);
}

class FlopBait(BossModule module) : TetherBait(module, true)
{
    protected override void OnTetherCreated(Actor source, Actor target)
    {
        switch ((OID)source.OID)
        {
            case OID.ProdigiousPunutiy:
                CurrentBaits.Add(new(source, target, new AOEShapeCircle(14)));
                break;
            case OID.PetitPunutiy:
                CurrentBaits.Add(new(source, target, new AOEShapeCircle(6)));
                break;
        }
    }
}

class HydroBait(BossModule module) : TetherBait(module, false)
{
    protected override void OnTetherCreated(Actor source, Actor target)
    {
        if ((OID)source.OID == OID.Punutiy)
            CurrentBaits.Add(new(source, target, new AOEShapeCone(60, 15.Degrees())));
    }
}

class ShoreShaker(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ShoreShaker)
            AddSequence(Module.Center, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.ShoreShaker1 => 0,
            AID.ShoreShaker2 => 1,
            AID.ShoreShaker3 => 2,
            _ => -1
        };
        if (!AdvanceSequence(order, caster.Position, WorldState.FutureTime(2f)))
            ReportError($"unexpected order {order}");
    }
}

class D011PrimePunutiyStates : StateMachineBuilder
{
    public D011PrimePunutiyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PunutiyFlop>()
            .ActivateOnEnter<Hydrowave>()
            .ActivateOnEnter<Resurface>()
            .ActivateOnEnter<Bury>()
            .ActivateOnEnter<Decay>()
            .ActivateOnEnter<FlopBait>()
            .ActivateOnEnter<HydroBait>()
            .ActivateOnEnter<ShoreShaker>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12723)]
public class D011PrimePunutiy(WorldState ws, Actor primary) : BossModule(ws, primary, new(35, -95), new ArenaBoundsSquare(20));
