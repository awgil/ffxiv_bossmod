namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD70Aeturna;

public enum OID : uint
{
    Boss = 0x3D1B, // R5.95
    AllaganCrystal = 0x3D1C, // R1.5
    Helper = 0x233C
}

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
    Teleport = 31446 // 3D1B->location, no cast, single-target, boss teleports mid
}

public enum TetherID : uint
{
    FerocityTetherGood = 1, // Boss->player
    FerocityTetherBad = 57 // Boss->player
}

class SteelClaw(BossModule module) : Components.SingleTargetDelayableCast(module, ActionID.MakeSpell(AID.SteelClaw));
class Ferocity(BossModule module) : Components.StretchTetherDuo(module, (uint)TetherID.FerocityTetherBad, (uint)TetherID.FerocityTetherGood, 15, activationDelay: 5.7f);
class PreternaturalTurnCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PreternaturalTurnCircle), new AOEShapeCircle(15));
class PreternaturalTurnDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PreternaturalTurnDonut), new AOEShapeDonut(6, 30));

class Shatter(BossModule module) : Components.GenericAOEs(module)
{
    private bool FerocityCasted;
    private readonly List<Actor> _crystals = [];
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone cone = new(23.95f, 75.Degrees());
    private static readonly AOEShapeCircle circle = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(4);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var s in _crystals)
            Arena.Actor(s, ArenaColor.Object, true);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Impact)
            _crystals.Add(caster);
        if ((AID)spell.Action.ID == AID.Ferocity)
            FerocityCasted = true;
        if (!FerocityCasted && (AID)spell.Action.ID == AID.PreternaturalTurnDonut)
            foreach (var c in Module.Enemies(OID.AllaganCrystal))
                _aoes.Add(new(circle, c.Position, default, spell.NPCFinishAt.AddSeconds(0.5f)));
        if (!FerocityCasted && (AID)spell.Action.ID == AID.PreternaturalTurnCircle)
            foreach (var c in Module.Enemies(OID.AllaganCrystal))
                _aoes.Add(new(cone, c.Position, c.Rotation, spell.NPCFinishAt.AddSeconds(0.5f)));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
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

class Roar(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Roar));
class FallingRock(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FallingRock), new AOEShapeCircle(3));
class Impact(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Impact), new AOEShapeCircle(5));

class DD70AeturnaStates : StateMachineBuilder
{
    public DD70AeturnaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SteelClaw>()
            .ActivateOnEnter<Ferocity>()
            .ActivateOnEnter<PreternaturalTurnCircle>()
            .ActivateOnEnter<PreternaturalTurnDonut>()
            .ActivateOnEnter<Shatter>()
            .ActivateOnEnter<FallingRock>()
            .ActivateOnEnter<Roar>()
            .ActivateOnEnter<Impact>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "legendoficeman, Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 903, NameID = 12246)]
public class DD70Aeturna(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -300), new ArenaBoundsCircle(20));
