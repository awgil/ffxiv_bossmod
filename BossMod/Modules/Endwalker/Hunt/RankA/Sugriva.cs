namespace BossMod.Endwalker.Hunt.RankA.Sugriva;

public enum OID : uint
{
    Boss = 0x35FC, // R6.000, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Twister = 27219, // Boss->players, 5.0s cast, range 8 circle stack + knockback 20
    BarrelingSmash = 27220, // Boss->player, no cast, single-target, charges to random player and starts casting Spark or Scythe Tail immediately afterwards
    Spark = 27221, // Boss->self, 5.0s cast, range 14-24+R donut
    ScytheTail = 27222, // Boss->self, 5.0s cast, range 17 circle
    Butcher = 27223, // Boss->self, 5.0s cast, range 8 120-degree cone
    Rip = 27224, // Boss->self, 2.5s cast, range 8 120-degree cone
    RockThrowFirst = 27225, // Boss->location, 4.0s cast, range 6 circle
    RockThrowRest = 27226, // Boss->location, 1.6s cast, range 6 circle
    Crosswind = 27227, // Boss->self, 5.0s cast, range 36 circle
    ApplyPrey = 27229, // Boss->player, 0.5s cast, single-target
}

class Twister(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Twister), 20, shape: new AOEShapeCircle(8))
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Casters.Count > 0)
            hints.Add("Stack and knockback");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Casters.FirstOrDefault()?.CastInfo?.TargetID == player.InstanceID ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var caster = Casters.FirstOrDefault();
        var target = caster != null ? WorldState.Actors.Find(caster.CastInfo!.TargetID) : null;
        if (target != null)
            Shape!.Outline(Arena, target);
    }
}

class Spark(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Spark), new AOEShapeDonut(14, 30));
class ScytheTail(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ScytheTail), new AOEShapeCircle(17));

class Butcher(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.Butcher), new AOEShapeCone(8, 60.Degrees()), endsOnCastEvent: true)
{
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

class Rip(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Rip), new AOEShapeCone(8, 60.Degrees()));

// TODO: generalize to baited aoe
class RockThrow(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.RockThrowRest))
{
    private Actor? _target;
    private static readonly AOEShapeCircle _shape = new(6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Active())
            yield return new(_shape, Module.PrimaryActor.CastInfo!.LocXZ, default, Module.PrimaryActor.CastInfo.NPCFinishAt);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return player == _target ? PlayerPriority.Interesting : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target != null)
            Arena.AddCircle(_target.Position, _shape.Radius, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ApplyPrey:
                NumCasts = 0;
                _target = WorldState.Actors.Find(spell.TargetID);
                if (_target?.Type == ActorType.Chocobo) //Player Chocobos are immune against prey, so mechanic doesn't happen if a chocobo gets selected
                    _target = null;
                break;
            case AID.RockThrowRest:
                if (NumCasts >= 1)
                    _target = null;
                break;
        }
    }

    private bool Active() => (Module.PrimaryActor.CastInfo?.IsSpell() ?? false) && (AID)Module.PrimaryActor.CastInfo!.Action.ID is AID.RockThrowFirst or AID.RockThrowRest;
}

class Crosswind(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Crosswind));

class SugrivaStates : StateMachineBuilder
{
    public SugrivaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Twister>()
            .ActivateOnEnter<Spark>()
            .ActivateOnEnter<ScytheTail>()
            .ActivateOnEnter<Butcher>()
            .ActivateOnEnter<Rip>()
            .ActivateOnEnter<RockThrow>()
            .ActivateOnEnter<Crosswind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 10626)]
public class Sugriva(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);
