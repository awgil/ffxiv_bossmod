namespace BossMod.Dawntrail.Foray.CriticalEngagement.TinyMage;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x24, Helper type
    Boss = 0x4C6D, // R2.500, x1
    TinyMage = 0x4D55, // R1.000, x1
    TinyApprentice = 0x4C6E, // R1.000, x0 (spawn during fight)
    FlareSphereGrow = 0x4C6F, // R0.700-1.904, x0 (spawn during fight)
    FlareSphere = 0x4C70, // R1.400, x0 (spawn during fight)
    HolySphereGrow = 0x4C71, // R0.700-1.904, x0 (spawn during fight)
    HolySphere = 0x4C72, // R1.400, x0 (spawn during fight)
    ArcaneSphere1 = 0x4C73, // R1.000, x0 (spawn during fight)
    ArcaneSphere2 = 0x4C74, // R1.000, x0 (spawn during fight)
    TetherHelper = 0x4EBB, // R1.750, x0 (spawn during fight)
}

public enum AID : uint
{
    DeathWall = 49057, // 4D55->self, no cast, range 20-25 donut
    AutoAttack = 48305, // Boss->player, no cast, single-target
    TinyWarp = 48331, // Boss->location, no cast, single-target
    SmallForOne = 48306, // Boss->self, 3.0s cast, single-target
    ArcaneAggregation1 = 48307, // 4C6E->self, 3.0s cast, single-target
    ArcaneAggregation2 = 48308, // 4C6E->self, 3.0s cast, single-target
    ArcaneAggregation3 = 49718, // 4C6E->self, 5.5s cast, single-target
    ArcaneAggregation4 = 49719, // 4C6E->self, 5.5s cast, single-target
    UnkAdds1 = 50530, // 4C6E->self, no cast, single-target
    UnkAdds2 = 50638, // 4C6E->self, no cast, single-target
    Recharge1 = 48309, // 4C6E->self, 1.5s cast, single-target
    Recharge2 = 48310, // 4C6E->self, 1.5s cast, single-target
    RechargeInstant = 49059, // 4C6E/Boss->self, no cast, single-target
    TinyFlareVisual = 48313, // 4C6F/4C70->self, no cast, single-target
    TinyFlare = 48311, // Helper->self, 2.0s cast, range 18 circle
    TinyThunderIIICast = 48329, // Boss->self, 5.0s cast, single-target
    TinyThunderIII = 48330, // Helper->self, no cast, ???
    TinyHolyVisual = 48314, // 4C72/4C71->self, no cast, single-target
    TinyHolyAOE = 48312, // Helper->self, 2.0s cast, range 50 circle
    TinyHolyKB = 49058, // Helper->self, no cast, ???
    TinyQuakeIIICast = 48322, // Boss->self, 3.5+0.5s cast, single-target
    TinyQuakeIII1 = 48323, // Helper->self, 4.0s cast, range 10 circle
    TinyQuakeIII2 = 48324, // Helper->self, 4.0s cast, range 10-20 donut
    TinyQuakeIII3 = 48325, // Helper->self, 4.0s cast, range 20-30 donut
    AllForOne = 50762, // Boss->self, 3.0s cast, single-target
    Meteor = 48326, // 4C73->self, 130.0s cast, single-target
    CometCast = 48327, // 4C74->self, 60.0s cast, range 60 circle
    CometAOE = 49061, // Helper->self, no cast, ???
    DiminutiveDualcast = 48317, // Boss->self, 5.5+0.5s cast, single-target
    TinyBlizzardIII = 48319, // Helper->self, 6.0s cast, range 40 60-degree cone
    TinyFireIII = 48318, // Helper->self, 6.0s cast, range 14 circle
    TinyMeteorCast = 48320, // Boss->self, 5.0s cast, single-target
    TinyMeteor = 48321, // Helper->location, 4.0s cast, range 6 circle
}

public enum SID : uint
{
    SustainedDamage = 3795, // none->_Gen_FlareSphere/_Gen_HolySphere1, extra=0x1/0x2
    UnkAdds = 2552, // none->_Gen_TinyApprentice, extra=0x198
    SpeedCast = 3445, // none->_Gen_ArcaneSphere/_Gen_ArcaneSphere1, extra=0xA/0x15/0x1E
}

public enum TetherID : uint
{
    SphereTether = 415, // _Gen_HolySphere/_Gen_FlareSphere1->_Gen_HolySphere/_Gen_FlareSphere1
    UnkSphere1 = 60, // _Gen_ArcaneSphere->_Gen_
    UnkSphere2 = 422, // _Gen_TinyApprentice/Boss->_Gen_ArcaneSphere/_Gen_
}

class TinyThunderIII(BossModule module) : Components.RaidwideCastDelay(module, AID.TinyThunderIIICast, AID.TinyThunderIII, 0.6f);

class RelayFlare(BossModule module) : Components.GenericAOEs(module, AID.TinyFlare)
{
    AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.FlareSphereGrow && (SID)status.ID == SID.SustainedDamage && status.Extra == 1)
        {
            var off = Arena.Center - (actor.Position - Arena.Center);
            _predicted = new(new AOEShapeCircle(18), off, default, WorldState.FutureTime(8.4f));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _predicted.HasValue)
            _predicted = _predicted.Value with { Activation = Module.CastFinishAt(spell) };
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TinyFlare)
            _predicted = null;
    }
}

class RelayHoly(BossModule module) : Components.Knockback(module, AID.TinyHolyAOE)
{
    Source? _source;

    public override IEnumerable<Source> Sources(int slot, Actor actor) => Utils.ZeroOrOne(_source);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.HolySphereGrow && (SID)status.ID == SID.SustainedDamage && status.Extra == 1)
        {
            var off = Arena.Center - (actor.Position - Arena.Center);
            _source = new(off, 15, WorldState.FutureTime(8.6f));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _source != null)
            _source = new(spell.LocXZ, 15, Module.CastFinishAt(spell, 0.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TinyHolyKB)
            _source = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var src in Sources(slot, actor))
            if (!IsImmune(slot, src.Activation))
            {
                var s = src.Origin;
                var d = src.Distance;
                var sh = ShapeDistance.InvertedCircle(Arena.Center, 20);
                hints.AddForbiddenZone(p =>
                {
                    var dir = (p - s).Normalized();
                    var proj = p + dir * d;
                    return sh(proj);
                }, src.Activation);
            }
    }
}

class TinyTether(BossModule module) : Components.Knockback(module)
{
    enum Orb
    {
        Flare,
        Holy
    }

    record struct Mechanic(Orb Type, WPos Origin, DateTime Activation);

    readonly List<Mechanic> _mechanics = [];

    bool _holy2;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var mech in _mechanics.Take(2).Where(m => m.Type == Orb.Holy))
            yield return new(mech.Origin, 15, mech.Activation);
    }

    IEnumerable<Components.GenericAOEs.AOEInstance> ActiveAOEs()
    {
        foreach (var (i, mech) in _mechanics.Take(2).Select((m, i) => (i, m)))
            if (mech.Type == Orb.Flare)
                yield return new(new AOEShapeCircle(18), mech.Origin, default, mech.Activation, i == 0 ? ArenaColor.Danger : ArenaColor.AOE, i == 0);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.SphereTether && WorldState.Actors.Find(tether.Target) is { } target)
        {
            var distance = (source.Position - target.Position).Length();
            var mid = WPos.Lerp(source.Position, target.Position, 0.5f);
            var ty = (OID)source.OID == OID.FlareSphere ? Orb.Flare : Orb.Holy;
            _mechanics.Add(new(ty, mid, WorldState.FutureTime(12.4f + (distance - 16) * 0.5f)));
            _mechanics.SortBy(m => m.Activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TinyFlare:
                if (_mechanics.Count > 0)
                    _mechanics.Ref(0).Activation = Module.CastFinishAt(spell);
                break;
            case AID.TinyHolyAOE:
                if (_mechanics.Count > 0)
                    _mechanics.Ref(0).Activation = Module.CastFinishAt(spell, 0.3f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TinyFlare:
                if (_mechanics.Count > 0)
                    _mechanics.RemoveAt(0);
                break;
            case AID.TinyHolyAOE:
                _holy2 = true;
                break;
            case AID.TinyHolyKB:
                if (_holy2)
                {
                    if (_mechanics.Count > 0)
                        _mechanics.RemoveAt(0);
                    _holy2 = false;
                }
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        foreach (var (i, m) in _mechanics.Take(2).Select((m, i) => (i, m)))
        {
            if (m.Type == Orb.Flare)
                Arena.ZoneCircle(m.Origin, 18, i == 0 ? ArenaColor.Danger : ArenaColor.AOE);
            if (m.Type == Orb.Holy)
                Arena.AddCircle(m.Origin, 1, ArenaColor.Danger, i == 0 ? 3 : 1);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        foreach (var aoe in ActiveAOEs())
            if (aoe.Risky && aoe.Check(actor.Position))
                hints.Add("GTFO from aoe!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        WPos orig;
        Func<WPos, float> sh;
        switch (_mechanics)
        {
            case [{ Type: Orb.Flare } f1]:
                hints.AddForbiddenZone(ShapeDistance.Circle(f1.Origin, 18), f1.Activation);
                break;
            case [{ Type: Orb.Flare } f1, { Type: Orb.Flare } f2, ..]:
                hints.AddForbiddenZone(ShapeDistance.Circle(f1.Origin, 18), f1.Activation);
                hints.AddForbiddenZone(ShapeDistance.Circle(f2.Origin, 18), f2.Activation);
                break;
            case [{ Type: Orb.Flare } f1, { Type: Orb.Holy } h2, ..]:
                hints.AddForbiddenZone(ShapeDistance.Circle(f1.Origin, 18), f1.Activation);
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(h2.Origin, 10), h2.Activation);
                break;

            case [{ Type: Orb.Holy } h1]:
                orig = h1.Origin;
                sh = ShapeDistance.InvertedCircle(Arena.Center, 20);
                hints.AddForbiddenZone(p =>
                {
                    var dir = (p - orig).Normalized() * 15;
                    return sh(p + dir);
                }, h1.Activation);
                break;
            case [{ Type: Orb.Holy } h1, { Type: Orb.Flare } f2, ..]:
                orig = h1.Origin;
                sh = ShapeDistance.Union([ShapeDistance.InvertedCircle(Arena.Center, 20), ShapeDistance.Circle(f2.Origin, 12)]);
                hints.AddForbiddenZone(p =>
                {
                    var dir = (p - orig).Normalized() * 15;
                    return sh(p + dir);
                }, h1.Activation);
                break;
            case [{ Type: Orb.Holy } h1, { Type: Orb.Holy } h2, ..]:
                orig = h1.Origin;
                sh = ShapeDistance.Union([ShapeDistance.InvertedCircle(Arena.Center, 20), ShapeDistance.InvertedCircle(h2.Origin, 10)]);
                hints.AddForbiddenZone(p =>
                {
                    var dir = (p - orig).Normalized() * 15;
                    return sh(p + dir);
                }, h1.Activation);
                break;
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (_mechanics[0].Type != Orb.Holy)
            return false;

        var danger = !pos.InCircle(Arena.Center, 20);

        if (_mechanics is [_, { Type: Orb.Flare } f, ..])
            danger |= pos.InCircle(f.Origin, 18);

        return danger;
    }
}

class ArcaneSphereBig(BossModule module) : Components.Adds(module, (uint)OID.ArcaneSphere1, forbidDots: true);
class ArcaneSphereSmall(BossModule module) : Components.Adds(module, (uint)OID.ArcaneSphere2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var target in hints.PotentialTargets.Where(t => t.Actor.OID == (uint)OID.ArcaneSphere2))
        {
            target.Priority = target.Actor.FindStatus(SID.SpeedCast)?.Extra ?? 1;
            target.AllowDOTs = false; // TODO: check if this is a good idea? if my assumption about the statuses is correct then fastest meteor will live 39 seconds and slowest will live 60
        }
    }
}

class TinyQuakeIII(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TinyQuakeIII1)
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var seq = (AID)spell.Action.ID switch
        {
            AID.TinyQuakeIII1 => 0,
            AID.TinyQuakeIII2 => 1,
            AID.TinyQuakeIII3 => 2,
            _ => -1
        };

        if (seq >= 0)
        {
            AdvanceSequence(seq, caster.Position, WorldState.FutureTime(2.1f));
            Sequences.RemoveAll(s => s.NumCastsDone >= 3);
        }
    }
}

class TinyBlizzardIII(BossModule module) : Components.StandardAOEs(module, AID.TinyBlizzardIII, new AOEShapeCone(40, 30.Degrees()), 3);
class TinyFireIII(BossModule module) : Components.StandardAOEs(module, AID.TinyFireIII, 14);
class TinyMeteor(BossModule module) : Components.StandardAOEs(module, AID.TinyMeteor, 6);

class TinyMageStates : StateMachineBuilder
{
    public TinyMageStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TinyThunderIII>()
            .ActivateOnEnter<TinyTether>()
            .ActivateOnEnter<RelayFlare>()
            .ActivateOnEnter<RelayHoly>()
            .ActivateOnEnter<ArcaneSphereSmall>()
            .ActivateOnEnter<ArcaneSphereBig>()
            .ActivateOnEnter<TinyBlizzardIII>()
            .ActivateOnEnter<TinyFireIII>()
            .ActivateOnEnter<TinyMeteor>()
            .ActivateOnEnter<TinyQuakeIII>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14795)]
public class TinyMage(WorldState ws, Actor primary) : CEModule(ws, primary, new(152, 716), new ArenaBoundsCircle(20));

