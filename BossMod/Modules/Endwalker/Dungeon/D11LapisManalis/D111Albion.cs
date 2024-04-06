namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D111Albion;

public enum OID : uint
{
    Boss = 0x3CFE, //R=4.6
    WildBeasts = 0x3D03, //R=0.5
    Helper = 0x233C,
    WildBeasts1 = 0x3CFF, // R1,320
    WildBeasts2 = 0x3D00, // R1,700
    WildBeasts3 = 0x3D02, // R4,000
    WildBeasts4 = 0x3D01, // R2,850
    IcyCrystal = 0x3D04, // R2,000
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 32812, // Boss->location, no cast, single-target, boss teleports mid
    CallOfTheMountain = 31356, // Boss->self, 3,0s cast, single-target, boss calls wild beasts
    WildlifeCrossing = 31357, // WildBeasts->self, no cast, range 7 width 10 rect
    AlbionsEmbrace = 31365, // Boss->player, 5,0s cast, single-target
    RightSlam = 32813, // Boss->self, 5,0s cast, range 80 width 20 rect
    LeftSlam = 32814, // Boss->self, 5,0s cast, range 80 width 20 rect
    KnockOnIce = 31358, // Boss->self, 4,0s cast, single-target
    KnockOnIce2 = 31359, // Helper->self, 6,0s cast, range 5 circle
    Icebreaker = 31361, // Boss->3D04, 5,0s cast, range 17 circle
    IcyThroes = 31362, // Boss->self, no cast, single-target
    IcyThroes2 = 32783, // Helper->self, 5,0s cast, range 6 circle
    IcyThroes3 = 31363, // Helper->player, 5,0s cast, range 6 circle
    IcyThroes4 = 32697, // Helper->self, 5,0s cast, range 6 circle
    RoarOfAlbion = 31364, // Boss->self, 7,0s cast, range 60 circle
};

public enum IconID : uint
{
    Tankbuster = 218, // player
    Target = 210, // IceCrystal
    Spreadmarker = 139, // player
};

class WildlifeCrossing : Components.GenericAOEs
{
    private static readonly AOEShapeRect rect = new(20, 5, 20);
    private static readonly Angle _rot90 = 90.Degrees();
    private static readonly Angle _rotM90 = -90.Degrees();
    private (bool active, WPos position, Angle rotation, int count, DateTime reset, List<Actor> beasts) stampede1;
    private (bool active, WPos position, Angle rotation, int count, DateTime reset, List<Actor> beasts) stampede2;
    private readonly (bool, WPos, Angle, int, DateTime, List<Actor>)[] stampedePositions =
    [
        (true, new WPos(4, -759), _rot90, 0, default(DateTime), new List<Actor>()),
        (true, new WPos(44, -759), _rotM90, 0, default(DateTime), new List<Actor>()),
        (true, new WPos(4, -749), _rot90, 0, default(DateTime), new List<Actor>()),
        (true, new WPos(44, -749), _rotM90, 0, default(DateTime), new List<Actor>()),
        (true, new WPos(4, -739), _rot90, 0, default(DateTime), new List<Actor>()),
        (true, new WPos(44, -739), _rotM90, 0, default(DateTime), new List<Actor>()),
        (true, new WPos(4, -729), _rot90, 0, default(DateTime), new List<Actor>()),
        (true, new WPos(44, -729), _rotM90, 0, default(DateTime), new List<Actor>()),
    ];
    private bool Newstampede => stampede1 == default;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (stampede1.active && stampede1.beasts.Count > 0)
            yield return new(new AOEShapeRect(CalculateStampedeLength(stampede1.beasts) + 30, 5), new(stampede1.beasts.Last().Position.X, stampede1.position.Z), stampede1.rotation);
        if (stampede2.active && stampede2.beasts.Count > 0)
            yield return new(new AOEShapeRect(CalculateStampedeLength(stampede2.beasts) + 30, 5), new(stampede2.beasts.Last().Position.X, stampede2.position.Z), stampede2.rotation);
        if (stampede1.active && stampede1.beasts.Count == 0)
            yield return new(rect, stampede1.position, _rot90);
        if (stampede2.active && stampede2.beasts.Count == 0)
            yield return new(rect, stampede2.position, _rot90);
    }

    private static float CalculateStampedeLength(List<Actor> beasts) => (beasts.First().Position - beasts.Last().Position).Length();

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (state == 0x00020001)
        {
            if (index == 0x21)
                if (Newstampede)
                    stampede1 = stampedePositions[0];
                else
                    stampede2 = stampedePositions[0];
            if (index == 0x25)
                if (Newstampede)
                    stampede1 = stampedePositions[1];
                else
                    stampede2 = stampedePositions[1];
            if (index == 0x22)
                if (Newstampede)
                    stampede1 = stampedePositions[2];
                else
                    stampede2 = stampedePositions[2];
            if (index == 0x26)
                if (Newstampede)
                    stampede1 = stampedePositions[3];
                else
                    stampede2 = stampedePositions[3];
            if (index == 0x23)
                if (Newstampede)
                    stampede1 = stampedePositions[4];
                else
                    stampede2 = stampedePositions[4];
            if (index == 0x27)
                if (Newstampede)
                    stampede1 = stampedePositions[5];
                else
                    stampede2 = stampedePositions[5];
            if (index == 0x24)
                if (Newstampede)
                    stampede1 = stampedePositions[6];
                else
                    stampede2 = stampedePositions[6];
            if (index == 0x28)
                if (Newstampede)
                    stampede1 = stampedePositions[7];
                else
                    stampede2 = stampedePositions[7];
        }
    }

    public override void Update(BossModule module)
    {
        var stampede1Position = new WPos(24, stampede1.position.Z);
        var stampede2Position = new WPos(24, stampede2.position.Z);

        foreach (var oid in new[] { OID.WildBeasts4, OID.WildBeasts3, OID.WildBeasts2, OID.WildBeasts1 })
        {
            var beasts = module.Enemies(oid);
            foreach (var b in beasts)
            {
                if (b.Position.InRect(stampede1Position, stampede1.rotation, 33, 33, 5) && !stampede1.beasts.Contains(b))
                    stampede1.beasts.Add(b);
                if (b.Position.InRect(stampede2Position, stampede2.rotation, 33, 33, 5) && !stampede2.beasts.Contains(b))
                    stampede2.beasts.Add(b);
            }
        }

        if (stampede1.reset != default && module.WorldState.CurrentTime > stampede1.reset)
            stampede1 = default;
        if (stampede2.reset != default && module.WorldState.CurrentTime > stampede2.reset)
            stampede2 = default;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.WildlifeCrossing)
        {
            if (MathF.Abs(caster.Position.Z - stampede1.position.Z) < 1)
                ++stampede1.count;
            if (MathF.Abs(caster.Position.Z - stampede2.position.Z) < 1)
                ++stampede2.count;
            if (stampede1.count == 30) //sometimes stampedes only have 30 instead of 31 hits for some reason, so i take the lower value and add a 0,5s reset timer via update
                stampede1.reset = module.WorldState.CurrentTime.AddSeconds(0.5f);
            if (stampede2.count == 30)
                stampede1.reset = module.WorldState.CurrentTime.AddSeconds(0.5f);
        }
    }
}

class IcyThroes : Components.GenericBaitAway
{
    private readonly List<Actor> _targets = [];

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Spreadmarker)
        {
            CurrentBaits.Add(new(module.PrimaryActor, actor, new AOEShapeCircle(6)));
            _targets.Add(actor);
            CenterAtTarget = true;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.IcyThroes3)
        {
            CurrentBaits.Clear();
            _targets.Clear();
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_targets.Contains(actor))
            hints.Add("Bait away!");
    }
}

class Icebreaker : Components.GenericAOEs
{
    private readonly List<Actor> _casters = [];
    private static readonly AOEShapeCircle circle = new(17);
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_casters.Count > 0)
            foreach (var c in _casters)
                yield return new(circle, c.Position, activation: _activation);
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Target)
        {
            _casters.Add(actor);
            _activation = module.WorldState.CurrentTime.AddSeconds(6);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Icebreaker)
            _activation = spell.NPCFinishAt;
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Icebreaker)
            _casters.Clear();
    }
}

class IcyThroes2 : Components.SelfTargetedAOEs
{
    public IcyThroes2() : base(ActionID.MakeSpell(AID.IcyThroes4), new AOEShapeCircle(6)) { }
}


class KnockOnIce : Components.SelfTargetedAOEs
{
    public KnockOnIce() : base(ActionID.MakeSpell(AID.KnockOnIce2), new AOEShapeCircle(5)) { }
}

class RightSlam : Components.SelfTargetedAOEs
{
    public RightSlam() : base(ActionID.MakeSpell(AID.RightSlam), new AOEShapeRect(20, 80, directionOffset: -90.Degrees())) { } //full width = half width in this case + angle is detected incorrectly, length and width are also switched
}

class LeftSlam : Components.SelfTargetedAOEs
{
    public LeftSlam() : base(ActionID.MakeSpell(AID.LeftSlam), new AOEShapeRect(20, 80, directionOffset: 90.Degrees())) { } //full width = half width in this case + angle is detected incorrectly, length and width are also switched
}

class AlbionsEmbrace : Components.SingleTargetCast
{
    public AlbionsEmbrace() : base(ActionID.MakeSpell(AID.AlbionsEmbrace)) { }
}

class RoarOfAlbion : Components.CastLineOfSightAOE
{
    public RoarOfAlbion() : base(ActionID.MakeSpell(AID.RoarOfAlbion), 60, false) { }
    public override IEnumerable<Actor> BlockerActors(BossModule module) => module.Enemies(OID.IcyCrystal);
}

class D111AlbionStates : StateMachineBuilder
{
    public D111AlbionStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WildlifeCrossing>()
            .ActivateOnEnter<LeftSlam>()
            .ActivateOnEnter<RightSlam>()
            .ActivateOnEnter<AlbionsEmbrace>()
            .ActivateOnEnter<Icebreaker>()
            .ActivateOnEnter<KnockOnIce>()
            .ActivateOnEnter<IcyThroes>()
            .ActivateOnEnter<IcyThroes2>()
            .ActivateOnEnter<RoarOfAlbion>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 896, NameID = 11992)]
public class D111Albion : BossModule
{
    public D111Albion(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(24, -744), 19.5f)) { }
}
