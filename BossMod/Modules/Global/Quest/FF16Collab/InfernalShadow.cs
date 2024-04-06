namespace BossMod.Global.Quest.FF16Collab.InfernalShadow;

class VulcanBurst : Components.RaidwideCast
{
    public VulcanBurst() : base(ActionID.MakeSpell(AID.VulcanBurstReal), "Time your dodge correctly") { }
}

class Hellfire : Components.RaidwideCastDelay
{
    public Hellfire() : base(ActionID.MakeSpell(AID.HellfireVisual), ActionID.MakeSpell(AID.HellfireRaidwide), 0.6f) { }
}

class Incinerate : Components.SpreadFromCastTargets
{
    public Incinerate() : base(ActionID.MakeSpell(AID.IncinerateReal), 5) { }
}

class SpreadingFire : Components.ConcentricAOEs
{
    private static readonly AOEShape[] _shapes = [new AOEShapeCircle(10), new AOEShapeDonut(10, 20), new AOEShapeDonut(20, 30), new AOEShapeDonut(30, 40)];

    public SpreadingFire() : base(_shapes) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SpreadingFire1st)
            AddSequence(caster.Position, spell.NPCFinishAt);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (Sequences.Count > 0)
        {
            var order = (AID)spell.Action.ID switch
            {
                AID.SpreadingFire1st => 0,
                AID.SpreadingFire2nd => 1,
                AID.SpreadingFire3rd => 2,
                AID.SpreadingFire4th => 3,
                _ => -1
            };
            AdvanceSequence(order, caster.Position, module.WorldState.CurrentTime.AddSeconds(2));
        }
    }
}

class SmolderingClaw : Components.SelfTargetedAOEs
{
    public SmolderingClaw() : base(ActionID.MakeSpell(AID.SmolderingClawReal), new AOEShapeCone(40, 75.Degrees())) { }
}

class TailStrike : Components.SelfTargetedAOEs
{
    public TailStrike() : base(ActionID.MakeSpell(AID.TailStrikeReal), new AOEShapeCone(40, 75.Degrees())) { }
}

class FireRampageCleave : Components.GenericAOEs
{
    private static readonly AOEShapeCone cone = new(40, 90.Degrees());
   private readonly List<(WPos position, Angle rotation, DateTime activation, uint AID)> _castersunsorted = [];
    private List<(WPos position, Angle rotation, DateTime activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(cone, _casters[0].position, _casters[0].rotation, _casters[0].activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FieryRampageCleaveReal or AID.FieryRampageCleaveReal2)
        {
            _castersunsorted.Add((caster.Position, spell.Rotation, spell.NPCFinishAt, spell.Action.ID)); //casters appear in random order in raw ops
            _casters = _castersunsorted.OrderBy(x => x.AID).Select(x => (x.position, x.rotation, x.activation)).ToList();
        }
    }


    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID is AID.FieryRampageCleaveReal or AID.FieryRampageCleaveReal2)
        {
            _casters.RemoveAt(0);
            _castersunsorted.Clear();
        }
    }
}

class FieryRampageCircle : Components.SelfTargetedAOEs
{
    public FieryRampageCircle() : base(ActionID.MakeSpell(AID.FieryRampageCircleReal), new AOEShapeCircle(16)) { }
}

class FieryRampageRaidwide : Components.RaidwideCast
{
    public FieryRampageRaidwide() : base(ActionID.MakeSpell(AID.FieryRampageRaidwideReal), "Time your dodge correctly") { }
}

class PyrosaultReal : Components.SelfTargetedAOEs
{
    public PyrosaultReal() : base(ActionID.MakeSpell(AID.PyrosaultReal), new AOEShapeCircle(10)) { }
}

class Fireball : Components.LocationTargetedAOEs
{
    public Fireball() : base(ActionID.MakeSpell(AID.FireballReal), 6) { }
}

class CrimsonRush : Components.ChargeAOEs
{
    public CrimsonRush() : base(ActionID.MakeSpell(AID.CrimsonRushReal), 10) { }
}

class CrimsonStreak : Components.GenericAOEs
{
    private readonly List<(WPos source, AOEShape shape, Angle direction, DateTime activation)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_casters.Count > 0)
            yield return new(_casters[0].shape, _casters[0].source, _casters[0].direction, _casters[0].activation, ArenaColor.Danger);
        if (_casters.Count > 1)
            yield return new(_casters[1].shape, _casters[1].source, _casters[1].direction, _casters[1].activation);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CrimsonStreakReal)
        {
            var dir = spell.LocXZ - caster.Position;
            _casters.Add((caster.Position, new AOEShapeRect(dir.Length(), 10), Angle.FromDirection(dir), spell.NPCFinishAt));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID == AID.CrimsonStreakReal)
            _casters.RemoveAt(0);
    }
}

class Eruption : Components.LocationTargetedAOEs
{
    public Eruption() : base(ActionID.MakeSpell(AID.EruptionReal), 8) { }
}

class Eruption2 : Components.GenericAOEs
{
    private readonly List<(WPos position, DateTime activation, uint AID)> _castersunsorted = [];
    private List<(WPos position, DateTime activation)> _casters = [];
    private static readonly AOEShapeCircle circle = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (NumCasts < 10)
        {
            if (NumCasts < 6 ? _casters.Count > 2 : _casters.Count > 3)
                for (int i = 0; NumCasts < 6 ? i < 3 : i < 4; ++i)
                    yield return new(circle, _casters[i].position, activation: _casters[i].activation, color: ArenaColor.Danger);
            if (NumCasts < 3 ? _casters.Count > 5 : _casters.Count > 6)
                for (int i = 3; NumCasts < 3 ? i < 6 : i < 7; ++i)
                    yield return new(circle, _casters[i].position, activation: _casters[i].activation);
        }
        if (NumCasts >= 10)
        {
            if (_casters.Count > 3)
                for (int i = 0; _casters.Count > 6 ? i < 4 : i < 6; ++i)
                    yield return new(circle, _casters[i].position, activation: _casters[i].activation, color: ArenaColor.Danger);
            if (_casters.Count > 7)
                for (int i = 4; _casters.Count > 10 ? i < 8 : i < 10; ++i)
                    yield return new(circle, _casters[i].position, activation: _casters[i].activation);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.EruptionReal2 or AID.EruptionReal3 or AID.EruptionReal4)
        {
            _castersunsorted.Add((spell.LocXZ, spell.NPCFinishAt, spell.Action.ID));
            _casters = _castersunsorted.OrderBy(x => x.AID).Select(x => (x.position, x.activation)).ToList();
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (_casters.Count > 0 && (AID)spell.Action.ID is AID.EruptionReal2 or AID.EruptionReal3 or AID.EruptionReal4)
        {
            _casters.RemoveAt(0);
            ++NumCasts;
            if (_casters.Count == 0)
                _castersunsorted.Clear();
        }
    }
}

class BurningStrike : BossComponent
{
    private bool casting;
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BurningStrikeVisual)
            casting = true;
    }

    public override void Update(BossModule module)
    {
        var defendtargetable = module.Enemies(OID.DefendClive).Where(x => x.IsTargetable).FirstOrDefault();
        if (defendtargetable != null && casting)
            casting = false;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var defendtargetable = module.Enemies(OID.DefendClive).Where(x => x.IsTargetable).FirstOrDefault();
        if (casting && defendtargetable == null)
            hints.Add("Prepare to defend Clive!");
        if (defendtargetable != null)
            hints.Add($"Interact with {module.Enemies(OID.DefendClive).FirstOrDefault()!.Name} and solve a QTE!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var defendtargetable = module.Enemies(OID.DefendClive).Where(x => x.IsTargetable).FirstOrDefault();
        if (defendtargetable != null)
            arena.AddCircle(defendtargetable.Position, 1.4f, ArenaColor.Safe);
    }
}

class SearingStomp : BossComponent
{
    private int NumCasts;
    private bool casting;

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InfernalShroud)
        {
            ++NumCasts;
            if (NumCasts == 2)
                casting = true;
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InfernalHowlReal)
            casting = false;
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (casting)
            hints.Add("Prepare to solve a QTE!");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Quest, GroupID = 70334, NameID = 12564)] // also: CFC 959
public class InfernalShadow : BossModule
{
    public InfernalShadow(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(0, 0), 20)) { }
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.Clive))
            Arena.Actor(s, ArenaColor.Vulnerable);
        foreach (var s in Enemies(OID.InfernalSword))
            Arena.Actor(s, ArenaColor.Enemy);
        foreach (var s in Enemies(OID.DefendClive))
            Arena.Actor(s, ArenaColor.Object);
    }
}
