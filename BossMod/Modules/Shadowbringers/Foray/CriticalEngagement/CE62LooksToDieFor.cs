namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE62LooksToDieFor;

public enum OID : uint
{
    Boss = 0x31C9, // R5.950, x1
    Helper = 0x233C, // R0.500, x16
    PurpleLevin = 0x31CA, // R1.000-2.000, spawn during fight
    BallOfFire = 0x31CB, // R1.000, spawn during fight
    //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    //_Gen_Actor1eb180 = 0x1EB180, // R0.500, x0, EventObj type, and more spawn during fight
};

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target
    Thundercall = 23964, // Boss->self, 4.0s cast, single-target, visual (summon orbs)
    ThundercallVisual = 23965, // Helper->self, 4.5s cast, single-target, ???
    ThundercallAOE = 23966, // Helper->self, no cast, ???, raidwide
    LightningBolt = 23967, // Boss->self, 3.0s cast, single-target, visual (explode orbs)
    LightningBoltAOE = 23968, // Helper->location, 4.0s cast, range 4 circle
    DistantClap = 23969, // PurpleLevin->self, 1.0s cast, range 4-10 donut
    TwistingWinds = 23970, // Boss->self, 5.0s cast, range 80 width 10 rect aoe with knockback 30
    CloudToGround = 23971, // Boss->self, no cast, single-target, visual (exaflares)
    CloudToGroundFirst = 23972, // Helper->self, 5.0s cast, range 5 circle
    CloudToGroundRest = 23973, // Helper->self, no cast, range 5 circle
    Flame = 23974, // Boss->self, 4.0s cast, single-target, visual (raidwide + spawn flames)
    FlameAOE = 23975, // Helper->self, no cast, ???, raidwide
    Burn = 23976, // BallOfFire->self, 1.0s cast, range 8 circle
    Forelash = 23977, // Boss->self, 5.0s cast, range 40 180-degree cone with knockback 15
    Backlash = 23978, // Boss->self, 5.0s cast, range 40 180-degree cone with knockback 15
    Charybdis = 23979, // Boss->self, 4.0s cast, single-target, visual (set hp to 1)
    CharybdisAOE = 23980, // Helper->self, no cast, ???, set hp to 1
    Roar = 23981, // Boss->self, 8.0s cast, single-target, visual (raidwide after charybdis)
    RoarAOE = 23982, // Helper->self, no cast, ???, raidwide
    Levinbolt = 23983, // Boss->self, no cast, single-target, visual (spread)
    LevinboltAOE = 23984, // Helper->players, 5.0s cast, range 6 circle spread
    SerpentsEdge = 23985, // Boss->player, 5.0s cast, single-target, tankbuster
    Deathwall = 24711, // Helper->self, no cast, range 20-30 donut
};

class Thundercall : Components.RaidwideCast
{
    public Thundercall() : base(ActionID.MakeSpell(AID.Thundercall), "Raidwide + summon lighting orbs") { }
}

class LightningBoltDistantClap : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeBolt = new(4);
    private static readonly AOEShapeDonut _shapeClap = new(4, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LightningBoltAOE)
            _aoes.Add(new(_shapeBolt, spell.LocXZ, spell.Rotation, spell.NPCFinishAt));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.LightningBoltAOE:
                if (_aoes.FindIndex(a => a.Origin.AlmostEqual(spell.TargetXZ, 1)) is var index && index >= 0)
                    _aoes[index] = new(_shapeClap, spell.TargetXZ, activation: module.WorldState.CurrentTime.AddSeconds(6.1f));
                break;
            case AID.DistantClap:
                _aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 1));
                break;
        }
    }
}

class TwistingWinds : Components.SelfTargetedAOEs
{
    public TwistingWinds() : base(ActionID.MakeSpell(AID.TwistingWinds), new AOEShapeRect(40, 5, 40)) { }
}

class CloudToGround : Components.Exaflare
{
    public CloudToGround() : base(5) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CloudToGroundFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 5 * spell.Rotation.ToDirection(), NextExplosion = spell.NPCFinishAt, TimeToMove = 1.1f, ExplosionsLeft = 4, MaxShownExplosions = 2 });
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CloudToGroundFirst or AID.CloudToGroundRest)
        {
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index == -1)
            {
                module.ReportError(this, $"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(module, Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class Flame : Components.RaidwideCast
{
    public Flame() : base(ActionID.MakeSpell(AID.Flame), "Raidwide + summon flame orbs") { }
}

class Burn : Components.GenericAOEs
{
    private IReadOnlyList<Actor> _flames = ActorEnumeration.EmptyList;
    private List<(Actor actor, AOEInstance? aoe)> _casters = [];

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        var deadline = new DateTime();
        foreach (var c in _casters)
        {
            if (c.aoe == null)
                continue;
            if (deadline == default)
                deadline = c.aoe.Value.Activation.AddSeconds(1);
            if (c.aoe.Value.Activation < deadline)
                yield return c.aoe.Value;
        }
    }

    public override void Init(BossModule module)
    {
        _flames = module.Enemies(OID.BallOfFire);
    }

    public override void Update(BossModule module)
    {
        foreach (var f in _flames.Where(f => f.ModelState.AnimState1 == 1 && _casters.FindIndex(c => c.actor == f) < 0))
        {
            _casters.Add((f, new (_shape, f.Position, activation: module.WorldState.CurrentTime.AddSeconds(5))));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Burn && _casters.FindIndex(f => f.actor == caster) is var index && index >= 0)
            _casters[index] = (caster, null);
    }
}

class Forelash : Components.SelfTargetedAOEs
{
    public Forelash() : base(ActionID.MakeSpell(AID.Forelash), new AOEShapeCone(40, 90.Degrees())) { }
}

class Backlash : Components.SelfTargetedAOEs
{
    public Backlash() : base(ActionID.MakeSpell(AID.Backlash), new AOEShapeCone(40, 90.Degrees())) { }
}

class Charybdis : Components.CastHint
{
    public Charybdis() : base(ActionID.MakeSpell(AID.Charybdis), "Set hp to 1") { }
}

class Roar : Components.RaidwideCast
{
    public Roar() : base(ActionID.MakeSpell(AID.Roar)) { }
}

class Levinbolt : Components.SpreadFromCastTargets
{
    public Levinbolt() : base(ActionID.MakeSpell(AID.LevinboltAOE), 6) { }
}

class SerpentsEdge : Components.SingleTargetCast
{
    public SerpentsEdge() : base(ActionID.MakeSpell(AID.SerpentsEdge)) { }
}

class CE62LooksToDieForStates : StateMachineBuilder
{
    public CE62LooksToDieForStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thundercall>()
            .ActivateOnEnter<LightningBoltDistantClap>()
            .ActivateOnEnter<TwistingWinds>()
            .ActivateOnEnter<CloudToGround>()
            .ActivateOnEnter<Flame>()
            .ActivateOnEnter<Burn>()
            .ActivateOnEnter<Forelash>()
            .ActivateOnEnter<Backlash>()
            .ActivateOnEnter<Charybdis>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<Levinbolt>()
            .ActivateOnEnter<SerpentsEdge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 30)] // bnpcname=9925
public class CE62LooksToDieFor : BossModule
{
    public CE62LooksToDieFor(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-200, -580), 20)) { }
}
