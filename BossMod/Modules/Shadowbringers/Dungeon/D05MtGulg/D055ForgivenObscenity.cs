namespace BossMod.Shadowbringers.Dungeon.D05MtGulg.D055ForgivenObscenity;

public enum OID : uint
{
    Boss = 0x27CE, //R=5.0
    BossClones = 0x27CF, //R=5.0
    Helper = 0x2E8, //R=0.5
    Helper2 = 0x233C,
    Orbs = 0x27D0, //R=1.0
    Rings = 0x1EAB62,
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    VauthrysBlessing = 15639, // Boss->self, no cast, single-target
    OrisonFortissimo = 15636, // Boss->self, 4.0s cast, single-target
    OrisonFortissimo2 = 15637, // 233C->self, no cast, range 50 circle
    DivineDiminuendo = 15638, // Boss->self, 4.0s cast, range 8 circle
    DivineDiminuendo1 = 15640, // Boss->self, 4.0s cast, range 8 circle
    DivineDiminuendo2 = 15641, // 233C->self, 4.0s cast, range 10-16 donut
    DivineDiminuendo3 = 18025, // 233C->self, 4.0s cast, range 18-32 donut
    DivineDiminuendo4 = 15649, // BossClones->self, 4.0s cast, range 8 circle
    ConvictionMarcato = 15642, // Boss->self, 4.0s cast, range 40 width 5 rect
    ConvictionMarcato2 = 15643, // 233C->self, 4.0s cast, range 40 width 5 rect
    ConvictionMarcato3 = 15648, // BossClones->self, 4.0s cast, range 40 width 5 rect
    unknown = 16846, // 233C->self, 4.0s cast, single-target
    PenancePianissimo = 15644, // Boss->self, 3.0s cast, single-target, inverted circle voidzone appears
    FeatherMarionette = 15645, // Boss->self, 3.0s cast, single-target
    SolitaireRing = 17066, // Boss->self, 3.5s cast, single-target
    Ringsmith = 15652, // Boss->self, no cast, single-target
    GoldChaser = 15653, // Boss->self, 4.0s cast, single-target
    VenaAmoris = 15655, // Orbs->self, no cast, range 40 width 5 rect
    SacramentSforzando = 15634, // Boss->self, 4.0s cast, single-target
    SacramentSforzando2 = 15635, // 233C->player, no cast, single-target
    SanctifiedStaccato = 15654, // 233C->self, no cast, range 3 circle, sort of a voidzone around the light orbs, only triggers if you get too close
}

class Orbs(BossModule module) : Components.GenericAOEs(module, default, "GTFO from voidzone!")
{
    private readonly List<Actor> _orbs = [];
    private static readonly AOEShapeCircle circle = new(3);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var p in _orbs)
            yield return new(circle, p.Position);
    }
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Orbs)
            _orbs.Add(actor);
    }
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
            _orbs.RemoveAll(t => t.Position.AlmostEqual(actor.Position, 4));
    }
}

class GoldChaser(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    private readonly List<Actor> _casters = [];
    private static readonly AOEShapeRect rect = new(100, 2.5f, 100);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_casters.Count > 1 && ((_casters[0].Position.AlmostEqual(new(-227.5f, 253), 1) && _casters[1].Position.AlmostEqual(new(-232.5f, 251.5f), 1)) || (_casters[0].Position.AlmostEqual(new(-252.5f, 253), 1) && _casters[1].Position.AlmostEqual(new(-247.5f, 251.5f), 1))))
        {
            if (_casters.Count > 2)
            {
                if (NumCasts == 0)
                    yield return new(rect, _casters[0].Position, default, _activation.AddSeconds(7.1f), ArenaColor.Danger);
                if (NumCasts is 0 or 1)
                    yield return new(rect, _casters[1].Position, default, _activation.AddSeconds(7.6f), ArenaColor.Danger);
            }
            if (_casters.Count > 4)
            {
                if (NumCasts is 0 or 1)
                {
                    yield return new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f));
                    yield return new(rect, _casters[3].Position, default, _activation.AddSeconds(8.6f));
                }
            }
            if (_casters.Count > 4)
            {
                if (NumCasts == 2)
                    yield return new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f), ArenaColor.Danger);
                if (NumCasts is 2 or 3)
                    yield return new(rect, _casters[3].Position, default, _activation.AddSeconds(8.6f), ArenaColor.Danger);
            }
            if (_casters.Count == 6)
            {
                if (NumCasts is 2 or 3)
                {
                    yield return new(rect, _casters[4].Position, default, _activation.AddSeconds(9.1f));
                    yield return new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f));
                }
            }
            if (_casters.Count == 6)
            {
                if (NumCasts == 4)
                    yield return new(rect, _casters[4].Position, default, _activation.AddSeconds(9.1f), ArenaColor.Danger);
                if (NumCasts is 4 or 5)
                    yield return new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f), ArenaColor.Danger);
            }
        }
        if (_casters.Count > 1 && ((_casters[0].Position.AlmostEqual(new(-242.5f, 253), 1) && _casters[1].Position.AlmostEqual(new(-237.5f, 253), 1)) || (_casters[0].Position.AlmostEqual(new(-252.5f, 253), 1) && _casters[1].Position.AlmostEqual(new(-227.5f, 253), 1))))
        {
            if (_casters.Count > 2)
            {
                if (NumCasts == 0)
                    yield return new(rect, _casters[0].Position, default, _activation.AddSeconds(7.1f), ArenaColor.Danger);
                if (NumCasts is 0 or 1)
                    yield return new(rect, _casters[1].Position, default, _activation.AddSeconds(7.1f), ArenaColor.Danger);
            }
            if (_casters.Count > 4)
            {
                if (NumCasts is 0 or 1)
                {
                    yield return new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f));
                    yield return new(rect, _casters[3].Position, default, _activation.AddSeconds(8.1f));
                }
            }
            if (_casters.Count > 4)
            {
                if (NumCasts == 2)
                    yield return new(rect, _casters[2].Position, default, _activation.AddSeconds(8.1f), ArenaColor.Danger);
                if (NumCasts is 2 or 3)
                    yield return new(rect, _casters[3].Position, default, _activation.AddSeconds(8.1f), ArenaColor.Danger);
            }
            if (_casters.Count == 6)
            {
                if (NumCasts is 2 or 3)
                {
                    yield return new(rect, _casters[4].Position, default, _activation.AddSeconds(11.1f));
                    yield return new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f));
                }
            }
            if (_casters.Count == 6)
            {
                if (NumCasts == 4)
                    yield return new(rect, _casters[4].Position, default, _activation.AddSeconds(11.1f), ArenaColor.Danger);
                if (NumCasts is 4 or 5)
                    yield return new(rect, _casters[5].Position, default, _activation.AddSeconds(11.1f), ArenaColor.Danger);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Rings)
        {
            _casters.Add(actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Ringsmith)
            _activation = WorldState.CurrentTime;
        if ((AID)spell.Action.ID == AID.VenaAmoris)
        {
            ++NumCasts;
            if (NumCasts == 6)
            {
                _casters.Clear();
                NumCasts = 0;
            }
        }
    }
}

class SacramentSforzando(BossModule module) : Components.SingleTargetCastDelay(module, ActionID.MakeSpell(AID.SacramentSforzando), ActionID.MakeSpell(AID.SacramentSforzando2), 0.8f);
class OrisonFortissimo(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.OrisonFortissimo), ActionID.MakeSpell(AID.OrisonFortissimo2), 0.8f);
class DivineDiminuendo(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DivineDiminuendo), new AOEShapeCircle(8));
class DivineDiminuendo1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DivineDiminuendo1), new AOEShapeCircle(8));
class DivineDiminuendo2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DivineDiminuendo2), new AOEShapeDonut(10, 16));
class DivineDiminuendo3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DivineDiminuendo3), new AOEShapeDonut(18, 32));
class DivineDiminuendo4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DivineDiminuendo4), new AOEShapeCircle(8));
class ConvictionMarcato(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ConvictionMarcato), new AOEShapeRect(40, 2.5f));
class ConvictionMarcato2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ConvictionMarcato2), new AOEShapeRect(40, 2.5f));
class ConvictionMarcato3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ConvictionMarcato3), new AOEShapeRect(40, 2.5f));

class Voidzone(BossModule module) : BossComponent(module)
{
    private bool active;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
        {
            Module.Arena.Bounds = new ArenaBoundsRect(15, 20);
            active = false;
        }
        if (state == 0x00010002)
        {
            active = true;
            Module.Arena.Bounds = new ArenaBoundsCircle(15);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (active)
            hints.PlannedActions.Add((ActionID.MakeSpell(WAR.AID.Sprint), actor, 1, false));
    }
}

class D055ForgivenObscenityStates : StateMachineBuilder
{
    public D055ForgivenObscenityStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SacramentSforzando>()
            .ActivateOnEnter<DivineDiminuendo>()
            .ActivateOnEnter<DivineDiminuendo1>()
            .ActivateOnEnter<DivineDiminuendo2>()
            .ActivateOnEnter<DivineDiminuendo3>()
            .ActivateOnEnter<DivineDiminuendo4>()
            .ActivateOnEnter<ConvictionMarcato>()
            .ActivateOnEnter<ConvictionMarcato2>()
            .ActivateOnEnter<ConvictionMarcato3>()
            .ActivateOnEnter<OrisonFortissimo>()
            .ActivateOnEnter<GoldChaser>()
            .ActivateOnEnter<Orbs>()
            .ActivateOnEnter<Voidzone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 659, NameID = 8262)]
public class D055ForgivenObscenity(WorldState ws, Actor primary) : BossModule(ws, primary, new(-240, 237), new ArenaBoundsRect(15, 20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy, true);
    }
}
