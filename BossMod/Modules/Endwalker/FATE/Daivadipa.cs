namespace BossMod.Endwalker.FATE.Daivadipa;

public enum OID : uint
{
    Boss = 0x356D, // R=8.0
    OrbOfImmolationBlue = 0x3570, //R=1.0
    OrbOfImmolationRed = 0x356F, //R=1.0
    OrbOfConflagrationBlue = 0x3572, //R=1.0
    OrbOfConflagrationRed = 0x3571, //R=1.0
    Helper1 = 0x3573, //R=0.5
    Helper2 = 0x3574, //R=0.5
    Helper3 = 0x3575, //R=0.5
};

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Drumbeat = 26510, // Boss->player, 5,0s cast, single-target
    LeftwardTrisula = 26508, // Boss->self, 7,0s cast, range 65 180-degree cone
    RightwardParasu = 26509, // Boss->self, 7,0s cast, range 65 180-degree cone
    Lamplight = 26497, // Boss->self, 2,0s cast, single-target
    LoyalFlame = 26499, // Boss->self, 5,0s cast, single-target, blue first
    LoyalFlame2 = 26498, // Boss->self, 5,0s cast, single-target, red first
    LitPath1 = 26501, // OrbOfImmolation->self, 1,0s cast, range 50 width 10 rect, blue orb
    LitPath2 = 26500, // OrbOfImmolation2->self, 1,0s cast, range 50 width 10 rect, red orbs
    CosmicWeave = 26513, // Boss->self, 4,0s cast, range 18 circle
    YawningHells = 26511, // Boss->self, no cast, single-target
    YawningHells2 = 26512, // Helper1->location, 3,0s cast, range 8 circle
    ErrantAkasa = 26514, // Boss->self, 5,0s cast, range 60 90-degree cone
    InfernalRedemption = 26517, // Boss->self, 5,0s cast, single-target
    InfernalRedemption2 = 26518, // Helper3->location, no cast, range 60 circle
    IgnitingLights = 26503, // Boss->self, 2,0s cast, single-target
    IgnitingLights2 = 26502, // Boss->self, 2,0s cast, single-target
    Burn = 26507, // OrbOfConflagration->self, 1,0s cast, range 10 circle, blue orbs
    Burn2 = 26506, // OrbOfConflagration2->self, 1,0s cast, range 10 circle, red orbs   
    KarmicFlames = 26515, // Boss->self, 5,5s cast, single-target
    KarmicFlames2 = 26516, // Helper2->location, 5,0s cast, range 50 circle, damage fall off, safe distance should be about 20
    DivineCall = 27080, // Boss->self, 4,0s cast, range 65 circle, forced backwards march
    DivineCall2 = 26520, // Boss->self, 4,0s cast, range 65 circle, forced right march
    DivineCall3 = 27079, // Boss->self, 4,0s cast, range 65 circle, forced forward march
    DivineCall4 = 26519, // Boss->self, 4,0s cast, range 65 circle, forced left march
};

public enum SID : uint
{
    Hover = 1515, // none->OrbOfImmolation, extra=0x64
    AboutFace = 1959, // Boss->player, extra=0x0
    RightFace = 1961, // Boss->player, extra=0x0
    ForwardMarch = 1958, // Boss->player, extra=0x0
    LeftFace = 1960, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x2/0x8/0x1/0x4
};

class LitPath : Components.GenericAOEs
{
    private static readonly AOEShapeRect rect = new(50, 5);
    private DateTime _activation;
    private bool redblue1;
    private bool redblue2;
    private bool bluered1;
    private bool bluered2;
    private const float maxError = MathF.PI / 180;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
        {
            foreach (var o in module.Enemies(OID.OrbOfImmolationBlue))
            {
                if (bluered1 && (o.Rotation.AlmostEqual(90.Degrees(), maxError) || o.Rotation.AlmostEqual(180.Degrees(), maxError)))
                    yield return new(rect, o.Position, o.Rotation, _activation.AddSeconds(1.9f));
                if (redblue2 && !redblue1 && (o.Rotation.AlmostEqual(90.Degrees(), maxError) || o.Rotation.AlmostEqual(180.Degrees(), maxError)))
                    yield return new(rect, o.Position, o.Rotation, _activation.AddSeconds(4));
            }
            foreach (var o in module.Enemies(OID.OrbOfImmolationRed))
            {
                if (bluered2 && !bluered1 && (o.Rotation.AlmostEqual(90.Degrees(), maxError) || o.Rotation.AlmostEqual(180.Degrees(), maxError)))
                    yield return new(rect, o.Position, o.Rotation, _activation.AddSeconds(4));
                if (redblue1 && (o.Rotation.AlmostEqual(90.Degrees(), maxError) || o.Rotation.AlmostEqual(180.Degrees(), maxError)))
                    yield return new(rect, o.Position, o.Rotation, _activation.AddSeconds(1.9f));
            }
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (!module.Enemies(OID.OrbOfImmolationRed).All(x => x.IsDead) && !module.Enemies(OID.OrbOfImmolationBlue).All(x => x.IsDead))
        {
            if ((AID)spell.Action.ID == AID.LoyalFlame)
            {
                _activation = spell.NPCFinishAt;
                bluered1 = true;
                bluered2 = true;
            }
            if ((AID)spell.Action.ID == AID.LoyalFlame2)
            {
                _activation = spell.NPCFinishAt;
                redblue1 = true;
                redblue2 = true;
            }
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LitPath1)
        {
            bluered1 = false;
            redblue2 = false;
            if (++NumCasts == 5)
            {
                NumCasts = 0;
                _activation = default;
            }
        }
        if ((AID)spell.Action.ID == AID.LitPath2)
        {
            bluered2 = false;
            redblue1 = false;
            if (++NumCasts == 5)
            {
                NumCasts = 0;
                _activation = default;
            }
        }
    }
}

class Burn : Components.GenericAOEs
{
    private static readonly AOEShapeCircle circle = new(10);
    private DateTime _activation;
    private bool redblue1;
    private bool redblue2;
    private bool bluered1;
    private bool bluered2;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        if (_activation != default)
        {
            foreach (var o in module.Enemies(OID.OrbOfConflagrationBlue))
            {
                if (bluered1)
                    yield return new(circle, o.Position, activation: _activation.AddSeconds(2.1f));
                if (redblue2 && !redblue1)
                    yield return new(circle, o.Position, activation: _activation.AddSeconds(6.1f));
            }
            foreach (var o in module.Enemies(OID.OrbOfConflagrationRed))
            {
                if (bluered2 && !bluered1)
                    yield return new(circle, o.Position, activation: _activation.AddSeconds(6.1f));
                if (redblue1)
                    yield return new(circle, o.Position, activation: _activation.AddSeconds(2.1f));
            }
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (!module.Enemies(OID.OrbOfConflagrationRed).All(x => x.IsDead) && !module.Enemies(OID.OrbOfConflagrationBlue).All(x => x.IsDead))
        {
            if ((AID)spell.Action.ID == AID.LoyalFlame)
            {
                _activation = spell.NPCFinishAt;
                bluered1 = true;
                bluered2 = true;
            }
            if ((AID)spell.Action.ID == AID.LoyalFlame2)
            {
                _activation = spell.NPCFinishAt;
                redblue1 = true;
                redblue2 = true;
            }
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Burn)
        {
            bluered1 = false;
            redblue2 = false;
            if (++NumCasts == 16)
            {
                NumCasts = 0;
                _activation = default;
            }
        }
        if ((AID)spell.Action.ID == AID.Burn2)
        {
            bluered2 = false;
            redblue1 = false;
            ++NumCasts;
            if (++NumCasts == 16)
            {
                NumCasts = 0;
                _activation = default;
            }
        }
    }
}

class Drumbeat : Components.SingleTargetCast
{
    public Drumbeat() : base(ActionID.MakeSpell(AID.Drumbeat)) { }
}

class LeftwardTrisula : Components.SelfTargetedAOEs
{
    public LeftwardTrisula() : base(ActionID.MakeSpell(AID.LeftwardTrisula), new AOEShapeCone(65, 90.Degrees())) { }
}

class RightwardParasu : Components.SelfTargetedAOEs
{
    public RightwardParasu() : base(ActionID.MakeSpell(AID.RightwardParasu), new AOEShapeCone(65, 90.Degrees())) { }
}

class ErrantAkasa : Components.SelfTargetedAOEs
{
    public ErrantAkasa() : base(ActionID.MakeSpell(AID.ErrantAkasa), new AOEShapeCone(60, 45.Degrees())) { }
}

class CosmicWeave : Components.SelfTargetedAOEs
{
    public CosmicWeave() : base(ActionID.MakeSpell(AID.CosmicWeave), new AOEShapeCircle(18)) { }
}

class KarmicFlames : Components.SelfTargetedAOEs
{
    public KarmicFlames() : base(ActionID.MakeSpell(AID.KarmicFlames2), new AOEShapeCircle(20)) { }
}

class YawningHells : Components.LocationTargetedAOEs
{
    public YawningHells() : base(ActionID.MakeSpell(AID.YawningHells2), 8) { }
}

class InfernalRedemption : Components.RaidwideCastDelay
{
    public InfernalRedemption() : base(ActionID.MakeSpell(AID.InfernalRedemption), ActionID.MakeSpell(AID.InfernalRedemption2), 1) { }
}

class DivineCall : Components.StatusDrivenForcedMarch
{
    public DivineCall() : base(2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) { }

    public override bool DestinationUnsafe(BossModule module, int slot, Actor actor, WPos pos)
    {
        if (module.FindComponent<LeftwardTrisula>() != null && module.FindComponent<LeftwardTrisula>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
            return true;
        if (module.FindComponent<RightwardParasu>() != null && module.FindComponent<RightwardParasu>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation)))
            return true;
        if (module.FindComponent<Burn>() != null && module.FindComponent<Burn>()!.ActiveAOEs(module, slot, actor).Any() && !module.FindComponent<Burn>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation))) //safe and non-safe areas reverse by the time forced march runs out
            return true;
        if (module.FindComponent<LitPath>() != null && module.FindComponent<LitPath>()!.ActiveAOEs(module, slot, actor).Any() && !module.FindComponent<LitPath>()!.ActiveAOEs(module, slot, actor).Any(z => z.Shape.Check(pos, z.Origin, z.Rotation))) //safe and non-safe areas reverse by the time forced march runs out
            return true;
        else
            return false;
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (module.PrimaryActor.CastInfo?.IsSpell(AID.DivineCall) ?? false)
            hints.Add("Apply backwards march debuff");
        if (module.PrimaryActor.CastInfo?.IsSpell(AID.DivineCall2) ?? false)
            hints.Add("Apply right march debuff");
        if (module.PrimaryActor.CastInfo?.IsSpell(AID.DivineCall3) ?? false)
            hints.Add("Apply forwards march debuff");
        if (module.PrimaryActor.CastInfo?.IsSpell(AID.DivineCall4) ?? false)
            hints.Add("Apply left march debuff");
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var forward = actor.FindStatus(SID.ForwardMarch) != null;
        var left = actor.FindStatus(SID.LeftFace) != null;
        var right = actor.FindStatus(SID.RightFace) != null;
        var backwards = actor.FindStatus(SID.AboutFace) != null;
        var marching = actor.FindStatus(SID.ForcedMarch) != null;
        var last = ForcedMovements(module, actor).LastOrDefault();
        if (DestinationUnsafe(module, slot, actor, last.to) && !marching && (forward || left || right || backwards) && ((module.FindComponent<LitPath>() != null && module.FindComponent<LitPath>()!.ActiveAOEs(module, slot, actor).Any()) || (module.FindComponent<Burn>() != null && module.FindComponent<Burn>()!.ActiveAOEs(module, slot, actor).Any())))
            hints.Add("Aim into AOEs!");
        else if (!marching)
            base.AddHints(module, slot, actor, hints, movementHints);

    }
}

class DaivadipaStates : StateMachineBuilder
{
    public DaivadipaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Drumbeat>()
            .ActivateOnEnter<LeftwardTrisula>()
            .ActivateOnEnter<RightwardParasu>()
            .ActivateOnEnter<DivineCall>()
            .ActivateOnEnter<InfernalRedemption>()
            .ActivateOnEnter<CosmicWeave>()
            .ActivateOnEnter<YawningHells>()
            .ActivateOnEnter<ErrantAkasa>()
            .ActivateOnEnter<KarmicFlames>()
            .ActivateOnEnter<LitPath>()
            .ActivateOnEnter<Burn>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1763, NameID = 10269)]
public class Daivadipa : BossModule
{
    public Daivadipa(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(-608, 811), 24.5f)) { }
}
