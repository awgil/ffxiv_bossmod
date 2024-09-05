namespace BossMod.Stormblood.Dungeon.D04DomaCastle.D043HypertunedGrynewaht;

public enum OID : uint
{
    Boss = 0x1BD4, // R0.600, x?
    HypertunedGrynewaht = 0x1BD5, // R0.500, x?, Helper type
    RetunedMagitekBit = 0x1BD6, // R0.900, x?
    RetunedMagitekBit2 = 0x1BD7, // R0.500, x?
    MagitekChakram = 0x1BD8, // R3.000, x?

}

public enum AID : uint
{
    Attack = 870, // 1BE3/1BDC/1BDD/1BD9/1BDF/1BE0/1BE5/1BE6/1BD4->player, no cast, single-target
    Chainsaw = 8360, // 1BD4->self, 2.0s cast, range 4+R width 2 rect
    Chainsaw2 = 8361, // 1BD5->self, no cast, range 4+R width 2 rect
    DelayActionCharge = 8364, // 1BD4->self, no cast, single-target
    DelayActionCharge2 = 8365, // 1BD5->players, no cast, range 6 circle
    ChainMine = 9287, // 1BD2/1BD6->self, no cast, range 50+R width 3 rect
    ChainMine2 = 8359, // 1BD3/1BD7->player, no cast, single-target
    Gunsaw = 8362, // 1BD4->self, no cast, range 60+R width 2 rect
    GunsawFollow = 8363, // 1BD4->self, no cast, range 60+R width 2 rect
    ThermobaricCharge = 8366, // 1BD4->self, no cast, single-target
    ThermobaricCharge2 = 8367, // 1BD5->location, 10.0s cast, range 60 circle
    A = 8368, // 1BD4->self, no cast, single-target
    Attack2 = 9144, // 1BD7->self, no cast, range 3 circle
    CleanCut = 8369, // 1BD8->location, 6.0s cast, width 8 rect charge
}
public enum GID : uint
{
    Prey = 1253,
}
public enum IconID : uint
{
    DelayActionCharge = 99,
}
public enum TetherID : uint
{
    ChainMine = 60,
}
class Chainsaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Chainsaw), new AOEShapeRect(4f + 0.6f, 1f));
class Chainsaw2(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.Chainsaw2))
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Chainsaw2)
        {
            NumCasts++;
            _aoes.Add(new(new AOEShapeRect(2f + 0.6f, 1f, 2f), caster.Position + 2 * caster.Rotation.ToDirection(), caster.Rotation));
        }
        if (NumCasts == 4)
        {
            NumCasts = 0;
            _aoes.Clear();
        }
    }
};
class DelayActionCharge2(BossModule module) : Components.IconStackSpread(module, 0, (uint)IconID.DelayActionCharge, default, ActionID.MakeSpell(AID.DelayActionCharge2), 0, 6, 5.1f);
class ChainMines(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _tethers = [];
    private static readonly AOEShapeRect rect = new(50, 1.5f, 5);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var source in _tethers)
        {
            yield return new AOEInstance(rect, source.Position + 2 * source.Rotation.ToDirection(), source.Rotation);
        }
    }
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainMine)
            _tethers.Add(source);
    }
    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.ChainMine)
            _tethers.Remove(source);
    }
};
class Gunsaw(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Gunsaw), new AOEShapeRect(60.5f, 1f));
class GunsawFollow(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.GunsawFollow))
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
        {
            if (NumCasts >= 4)
            {
                NumCasts = 0;
                CurrentBaits.Clear();
            }
            else
            {
                var guessedTarget = WorldState.Party.WithoutSlot().MinBy(x => MathF.Abs(caster.Rotation.Rad - Angle.FromDirection(caster.DirectionTo(x)).Rad));
                CurrentBaits = [new Bait()
                {
                    Source = caster,
                    Target = guessedTarget!,
                    Shape = new AOEShapeRect(60.5f, 1),
                }];
            }
        }
    }
}
class ThermobaricChargeBait(BossModule module) : BossComponent(module)
{
    private int PreyTarget => WorldState.Party.WithSlot().Where(x => x.Item2.FindStatus(GID.Prey) != null).Select(x => x.Item1).FirstOrDefault(-1);
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (pcSlot == PreyTarget)
            Arena.AddCircle(pc.Position, 30, ArenaColor.AOE);
    }
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (slot == PreyTarget)
            hints.Add("Bait marker away!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot == PreyTarget)
        {
            hints.AddForbiddenZone(new AOEShapeCross(20, 12), Module.Center);
        }
    }
}
class ThermobaricChargeImpact(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ThermobaricCharge2)
        {
            _aoes.Add(new(new AOEShapeCircle(30), caster.Position, Activation: Module.CastFinishAt(spell, 1f)));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ThermobaricCharge2)
        {
            _aoes.Clear();
        }
    }
}
class CleanCut(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _chakrams = [];
    private static readonly AOEShapeRect rect = new(50, 4, 3f);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var b in _chakrams)
        {
            yield return new AOEInstance(rect, b.Position + 2 * b.Rotation.ToDirection(), b.Rotation);
        }
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.MagitekChakram)
        {
            _chakrams.Add(actor);
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((OID)caster.OID is OID.MagitekChakram && (AID)spell.Action.ID is AID.CleanCut)
        {
            _chakrams.Remove(caster);
        }
    }
    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID is OID.MagitekChakram)
        {
            _chakrams.Remove(actor);
        }
    }
};
class D043HypertunedGrynewahtStates : StateMachineBuilder
{
    public D043HypertunedGrynewahtStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Chainsaw>()
            .ActivateOnEnter<Chainsaw2>()
            .ActivateOnEnter<ChainMines>()
            .ActivateOnEnter<DelayActionCharge2>()
            .ActivateOnEnter<ThermobaricChargeBait>()
            .ActivateOnEnter<ThermobaricChargeImpact>()
            .ActivateOnEnter<Gunsaw>()
            .ActivateOnEnter<GunsawFollow>()
            .ActivateOnEnter<CleanCut>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 241, NameID = 6205)]
public class D043HypertunedGrynewaht(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240f, -198f), new ArenaBoundsSquare(20));
