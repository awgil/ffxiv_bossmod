namespace BossMod.Shadowbringers.Dungeon.D12MatoyasRelict.D123MotherPorxie;

public enum OID : uint
{
    Boss = 0x300B, // R3.6
    AeolianCaveSprite = 0x3052, // R1.6
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 19087, // Boss->player, no cast, single-target

    TenderLoinVisual = 22803, // Boss->self, 5.0s cast, single-target
    TenderLoin = 22804, // Helper->self, no cast, range 60 circle

    HuffAndPuff1 = 22809, // Boss->self, 8.0s cast, range 40 width 40 rect, knockback 15, source forward

    MediumRearVisual = 22813, // Boss->self, no cast, single-target
    MediumRear1 = 22814, // Helper->self, 10.5s cast, range 5-40 donut
    MediumRear2 = 22815, // Helper->self, 16.0s cast, range 5-40 donut, after limit break phase success

    MeatMallet = 22806, // Boss->location, 7.0s cast, range 45 circle, damage fall off AOE

    Barbeque = 23331, // Boss->self, 2.5s cast, single-target
    BarbequeRect = 22807, // Helper->self, 3.0s cast, range 50 width 5 rect, knockback 15, source forward
    BarbequeCircle = 22808, // Helper->location, 3.0s cast, range 5 circle
    ToACrispVisual = 22820, // Boss->self, no cast, single-target
    ToACrisp = 22821, // Helper->self, no cast, range 10 width 40 rect

    MincedMeatVisual = 22801, // Boss->player, 4.0s cast, single-target
    MincedMeat = 22802, // Helper->player, no cast, single-target

    Buffet = 22822, // AeolianCaveSprite->self, 3.0s cast, range 40 width 6 rect
    HuffAndPuffVisual = 22810, // Boss->self, 50.0s cast, range 40 width 40 rect, casts during limit break, only visual
    Explosion = 20020, // AeolianCaveSprite->self, 2.5s cast, range 80 circle, knocks player up to see a visual knockback hint
    HuffAndPuff2 = 22811, // Boss->self, no cast, range 40 width 40 rect, knockback 15, source forward

    BlowItAllDown = 22812, // Boss->self, no cast, range 40 width 40 rect, knockback 50, source forward (on limit break fail)
    NeerDoneWell = 20045, // Helper->self, 8.0s cast, range 5-40 donut (on limit break fail)

    OpenFlameVisual = 22818, // Boss->self, 6.0s cast, single-target
    OpenFlame = 22819, // Helper->player, no cast, range 5 circle, spread
}

public enum IconID : uint
{
    Tankbuster = 198, // player
    Spreadmarker = 169, // player
}

class TenderLoin(BossModule module) : Components.RaidwideCastDelay(module, ActionID.MakeSpell(AID.TenderLoinVisual), ActionID.MakeSpell(AID.TenderLoin), 0.8f);
class MincedMeat(BossModule module) : Components.SingleTargetCastDelay(module, ActionID.MakeSpell(AID.MincedMeatVisual), ActionID.MakeSpell(AID.MincedMeat), 0.9f);
class OpenFlame(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.OpenFlame), 5, 6.7f);
class MeatMallet(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MeatMallet), 30);
class BarbequeCircle(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BarbequeCircle), 5);
class BarbequeRect(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BarbequeRect), new AOEShapeRect(50, 2.5f));
class Buffet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Buffet), new AOEShapeRect(40, 3));

class HuffAndPuff(BossModule module) : Components.Knockback(module, stopAtWall: true)
{
    private Actor? MediumRear;
    private Actor? PuffCast;
    private (Actor? Caster, DateTime? Activation) PuffInstant;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (PuffCast is Actor puffer)
            yield return new Source(puffer.Position, 15, Module.CastFinishAt(puffer.CastInfo), Direction: puffer.Rotation, Kind: Kind.DirForward);
        if (PuffInstant is (Actor c, DateTime t))
            yield return new Source(c.Position, 15, t, Direction: c.Rotation, Kind: Kind.DirForward);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HuffAndPuff1:
                PuffCast = caster;
                break;
            case AID.MediumRear1:
            case AID.MediumRear2:
                MediumRear = caster;
                break;
            case AID.HuffAndPuffVisual:
                PuffInstant.Caster = caster;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HuffAndPuff1:
                PuffCast = null;
                break;
            case AID.MediumRear1:
            case AID.MediumRear2:
                MediumRear = null;
                break;
            case AID.Explosion:
                PuffInstant.Activation = WorldState.FutureTime(10.9f);
                break;
            case AID.HuffAndPuff2:
                PuffInstant = (null, null);
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (MediumRear is Actor rear)
            Arena.ZoneDonut(rear.Position, 5, 40, ArenaColor.AOE);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => MediumRear != null && !pos.InCircle(MediumRear.Position, 5);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (MediumRear is not Actor p)
            return;

        foreach (var movement in CalculateMovements(slot, actor))
        {
            var offset = movement.from - movement.to;
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(p.Position + offset, 5), Module.CastFinishAt(p.CastInfo));
            break;
        }
    }
}

class Barbeque(BossModule module) : BossComponent(module)
{
    private bool active;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Barbeque)
            active = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ToACrisp)
            active = false;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active)
            hints.GoalZones.Add(p => p.X >= 15 ? 50 : 0);
    }
}

class MotherPorxieStates : StateMachineBuilder
{
    public MotherPorxieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TenderLoin>()
            .ActivateOnEnter<MincedMeat>()
            .ActivateOnEnter<OpenFlame>()
            .ActivateOnEnter<MeatMallet>()
            .ActivateOnEnter<BarbequeCircle>()
            .ActivateOnEnter<BarbequeRect>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<HuffAndPuff>()
            .ActivateOnEnter<Barbeque>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 746, NameID = 9741, Contributors = "Malediktus, xan")]
public class MotherPorxie(WorldState ws, Actor primary) : BossModule(ws, primary, default, new ArenaBoundsSquare(19.5f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(Enemies(OID.AeolianCaveSprite), ArenaColor.Enemy);
    }
}

