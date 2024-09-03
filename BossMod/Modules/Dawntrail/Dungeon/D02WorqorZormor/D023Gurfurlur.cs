namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D023Gurfurlur;

public enum OID : uint
{
    Boss = 0x415F, // R7.000, x1
    Helper = 0x233C, // R0.500, x32, Helper type
    BitingWind = 0x4160, // R1.000, x0 (spawn during fight)
    AuraSphere = 0x4162, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    HeavingHaymaker = 36269, // Boss->self, 5.0s cast, single-target, visual (raidwide)
    HeavingHaymakerAOE = 36375, // Helper->self, 5.3s cast, range 60 circle, raidwide
    Stonework = 36301, // Boss->self, 3.0s cast, single-target, visual (elemental square)
    LithicImpact = 36302, // Helper->self, 6.8s cast, range 4 width 4 rect (elemental square spawn)
    Allfire1 = 36303, // Helper->self, 7.0s cast, range 10 width 10 rect
    Allfire2 = 36304, // Helper->self, 8.5s cast, range 10 width 10 rect
    Allfire3 = 36305, // Helper->self, 10.0s cast, range 10 width 10 rect
    VolcanicDrop = 36306, // Helper->player, 5.0s cast, range 6 circle spread
    GreatFlood = 36307, // Helper->self, 7.0s cast, range 80 width 60 rect, knock-forward 25
    Sledgehammer = 36313, // Boss->self/players, 5.0s cast, range 60 width 8 rect
    SledgehammerRest = 36314, // Boss->self, no cast, range 60 width 8 rect
    SledgehammerTargetSelect = 36315, // Helper->player, no cast, single-target, visual (target select)
    SledgehammerLast = 39260, // Boss->self, no cast, range 60 width 8 rect
    ArcaneStomp = 36319, // Boss->self, 3.0s cast, single-target, visual (spawn buffing spheres)
    ShroudOfEons = 36321, // AuraSphere->player, no cast, single-target, damage up from touching spheres
    EnduringGlory = 36320, // Boss->self, 6.0s cast, range 60 circle, raidwide after spheres
    WindswrathShort = 36310, // Helper->self, 7.0s cast, range 40 circle, knockback 15
    WindswrathLong = 39074, // Helper->self, 15.0s cast, range 40 circle, knockback 15
    Whirlwind = 36311, // Helper->self, no cast, range 5 circle, tornado
}

public enum IconID : uint
{
    VolcanicDrop = 139, // player
}

class HeavingHaymaker(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.HeavingHaymakerAOE));
class LithicImpact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LithicImpact), new AOEShapeRect(2, 2, 2));

class Allfire(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(5, 5, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var deadline = _aoes.FirstOrDefault().Activation.AddSeconds(0.5f);
        return _aoes.TakeWhile(aoe => aoe.Activation < deadline);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Allfire1 or AID.Allfire2 or AID.Allfire3)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Allfire1 or AID.Allfire2 or AID.Allfire3)
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
    }
}

class VolcanicDrop(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.VolcanicDrop), 6);

class GreatFlood(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.GreatFlood), 25, kind: Kind.DirForward)
{
    private readonly List<Actor> _allfireCasters = [];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;

            if (_allfireCasters.Count == 0)
            {
                // no aoes => everything closer than knockback distance (25) to the wall is unsafe
                hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center, s.Direction, 20, 5, 20), s.Activation);
            }
            else
            {
                // safe zone is one a 10x10 rect (4 first allfires), offset by knockback distance
                var center = _allfireCasters.PositionCentroid();
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(center, s.Direction, -15, 35, 10), s.Activation);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.Allfire1)
            _allfireCasters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if ((AID)spell.Action.ID == AID.Allfire1)
            _allfireCasters.Remove(caster);
    }
}

class Sledgehammer(BossModule module) : Components.GenericWildCharge(module, 4, ActionID.MakeSpell(AID.Sledgehammer), 60)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Sledgehammer)
        {
            Source = caster;
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.InstanceID == spell.TargetID ? PlayerRole.Target : PlayerRole.Share;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SledgehammerLast)
            Source = null;
    }
}

class AuraSpheres : Components.PersistentInvertibleVoidzone
{
    public AuraSpheres(BossModule module) : base(module, 2.5f, m => m.Enemies(OID.AuraSphere).Where(x => !x.IsDead))
    {
        InvertResolveAt = WorldState.CurrentTime;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Sources(Module).Any(x => !Shape.Check(actor.Position, x)))
            hints.Add("Touch the balls!");
    }
}

class EnduringGlory(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EnduringGlory));
class BitingWind(BossModule module) : Components.PersistentVoidzone(module, 4, m => m.Enemies(OID.BitingWind), 10);

class WindswrathShort(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.WindswrathShort), 15)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (!IsImmune(slot, s.Activation))
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(s.Origin, 5), s.Activation);
        }
    }
}

class WindswrathLong(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.WindswrathLong), 15)
{
    private readonly IReadOnlyList<Actor> _tornadoes = module.Enemies(OID.BitingWind);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var s in Sources(slot, actor))
        {
            if (IsImmune(slot, s.Activation))
                continue;

            if ((s.Activation - Module.WorldState.CurrentTime).TotalSeconds > 3)
            {
                // just prefer staying near center...
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(s.Origin, 5), s.Activation);
            }
            else
            {
                // ok knockback is imminent, calculate precise safe zone
                List<Func<WPos, float>> funcs = [
                    ShapeDistance.InvertedRect(Module.Center, new WDir(0, 1), 20, 20, 20),
                    .. _tornadoes.Select(t => ShapeDistance.Capsule(t.Position, t.Rotation, 20, 4))
                ];
                float combined(WPos p)
                {
                    var offset = p - s.Origin;
                    offset += offset.Normalized() * s.Distance;
                    var adj = s.Origin + offset;
                    return funcs.Min(f => f(adj));
                }
                hints.AddForbiddenZone(combined, s.Activation);
            }
        }
    }
}

class D023GurfurlurStates : StateMachineBuilder
{
    public D023GurfurlurStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HeavingHaymaker>()
            .ActivateOnEnter<LithicImpact>()
            .ActivateOnEnter<Allfire>()
            .ActivateOnEnter<VolcanicDrop>()
            .ActivateOnEnter<GreatFlood>()
            .ActivateOnEnter<Sledgehammer>()
            .ActivateOnEnter<AuraSpheres>()
            .ActivateOnEnter<EnduringGlory>()
            .ActivateOnEnter<BitingWind>()
            .ActivateOnEnter<WindswrathShort>()
            .ActivateOnEnter<WindswrathLong>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12705)]
public class D023Gurfurlur(WorldState ws, Actor primary) : BossModule(ws, primary, new(-54, -195), new ArenaBoundsSquare(20));
