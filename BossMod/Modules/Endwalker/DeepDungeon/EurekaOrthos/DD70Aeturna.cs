namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD70Aeturna;

public enum OID : uint
{
    Boss = 0x3D1B, // R5.950, x1
    AllaganCrystal = 0x3D1C, // R1.500, x4
    Helper = 0x233C, // R0.500, x12, 523 type
};

public enum AID : uint
{
    AutoAttack = 6497, // 3D1B->player, no cast, single-target
    FallingRock = 31441, // 233C->self, 2.5s cast, range 3 circle
    Ferocity = 31442, // 3D1B->self, 5.0s cast, single-target
    FerocityTetherStretchSuccess = 31443, // 3D1B->player, no cast, single-target
    FerocityTetherStretchFail = 31444, // 3D1B->player, no cast, single-target
    Impact = 31438, // 3D1C->self, 2.5s cast, range 5 circle
    PreternaturalTurnCircle = 31436, // 3D1B->self, 6.0s cast, range 15 circle
    PreternaturalTurnDonut = 31437, // 3D1B->self, 6.0s cast, range 6-30 donut
    Roar = 31435, // 3D1B->self, 5.0s cast, range 60 circle
    ShatterCircle = 31439, // 3D1C->self, 3.0s cast, range 8 circle
    ShatterCone = 31440, // 3D1C->self, 2.5s cast, range 18+R 150-degree cone
    SteelClaw = 31445, // 3D1B->player, 5.0s cast, single-target
    Teleport = 31446, // 3D1B->location, no cast, single-target, boss teleports mid
};

public enum IconID : uint
{
    tankbuster = 198, // player
};

public enum TetherID : uint
{
    FerocityTetherGood = 1, // Boss->player
    FerocityTetherStretch = 57, // Boss->player
};

class SteelClaw : Components.SingleTargetDelayableCast
{
    public SteelClaw() : base(ActionID.MakeSpell(AID.SteelClaw)) { }
}

class FerocityGood : Components.BaitAwayTethers  //TODO: consider generalizing stretched tethers?
{
    private ulong target;

    public FerocityGood() : base(new AOEShapeCone(0, 0.Degrees()), (uint)TetherID.FerocityTetherGood) { }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(module, source, tether);
        if (tether.ID == (uint)TetherID.FerocityTetherGood)
            target = tether.Target;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (DrawTethers && target == pc.InstanceID && CurrentBaits.Count > 0)
        {
            foreach (var b in ActiveBaits)
            {
                if (arena.Config.ShowOutlinesAndShadows)
                    arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Safe);
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.Add("Tether is stretched!", false);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 15));
    }
}

class FerocityBad : Components.BaitAwayTethers
{
    private ulong target;

    public FerocityBad() : base(new AOEShapeCone(0, 0.Degrees()), (uint)TetherID.FerocityTetherStretch) { }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        base.OnTethered(module, source, tether);
        if (tether.ID == (uint)TetherID.FerocityTetherStretch)
            target = tether.Target;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (DrawTethers && target == pc.InstanceID && CurrentBaits.Count > 0)
        {
            foreach (var b in ActiveBaits)
            {
                if (arena.Config.ShowOutlinesAndShadows)
                    arena.AddLine(b.Source.Position, b.Target.Position, 0xFF000000, 2);
                arena.AddLine(b.Source.Position, b.Target.Position, ArenaColor.Danger);
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.Add("Stretch tether further!");
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        if (target == actor.InstanceID && CurrentBaits.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Circle(module.PrimaryActor.Position, 15));
    }
}

class PreternaturalTurnCircle : Components.SelfTargetedAOEs
{
    public PreternaturalTurnCircle() : base(ActionID.MakeSpell(AID.PreternaturalTurnCircle), new AOEShapeCircle(15)) { }
}

class PreternaturalTurnDonut : Components.SelfTargetedAOEs
{
    public PreternaturalTurnDonut() : base(ActionID.MakeSpell(AID.PreternaturalTurnDonut), new AOEShapeDonut(6, 30)) { }
}

class Shatter : Components.GenericAOEs
{
    private bool FerocityCasted;
    private readonly List<Actor> _crystals = [];
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone cone = new(23.95f, 75.Degrees());
    private static readonly AOEShapeCircle circle = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(4);

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var s in _crystals)
            arena.Actor(s, ArenaColor.Object, true);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Impact)
            _crystals.Add(caster);
        if ((AID)spell.Action.ID == AID.Ferocity)
            FerocityCasted = true;
        if (!FerocityCasted && (AID)spell.Action.ID == AID.PreternaturalTurnDonut)
            foreach (var c in module.Enemies(OID.AllaganCrystal))
                _aoes.Add(new(circle, c.Position, activation: spell.NPCFinishAt.AddSeconds(0.5f)));
        if (!FerocityCasted && (AID)spell.Action.ID == AID.PreternaturalTurnCircle)
            foreach (var c in module.Enemies(OID.AllaganCrystal))
                _aoes.Add(new(cone, c.Position, c.Rotation, spell.NPCFinishAt.AddSeconds(0.5f)));
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ShatterCircle or AID.ShatterCone)
        {
            _aoes.Clear();
            _crystals.Clear();
        }
        if ((AID)spell.Action.ID is AID.PreternaturalTurnCircle or AID.PreternaturalTurnDonut)
            FerocityCasted = false;
    }
}

class Roar : Components.RaidwideCast
{
    public Roar() : base(ActionID.MakeSpell(AID.Roar)) { }
}

class FallingRock : Components.SelfTargetedAOEs
{
    public FallingRock() : base(ActionID.MakeSpell(AID.FallingRock), new AOEShapeCircle(3)) { }
}

class Impact : Components.SelfTargetedAOEs
{
    public Impact() : base(ActionID.MakeSpell(AID.Impact), new AOEShapeCircle(5)) { }
}

class DD70AeturnaStates : StateMachineBuilder
{
    public DD70AeturnaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SteelClaw>()
            .ActivateOnEnter<FerocityGood>()
            .ActivateOnEnter<FerocityBad>()
            .ActivateOnEnter<PreternaturalTurnCircle>()
            .ActivateOnEnter<PreternaturalTurnDonut>()
            .ActivateOnEnter<Shatter>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<Impact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 903, NameID = 12246)]
public class DD70Aeturna : BossModule
{
    public DD70Aeturna(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(-300, -300), 20)) { }
}
