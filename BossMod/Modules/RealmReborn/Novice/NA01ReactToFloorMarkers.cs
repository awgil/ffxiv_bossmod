namespace BossMod.RealmReborn.Novice.NA01ReactToFloorMarkers;

public enum OID : uint
{
    Boss = 0x463B, // R0.500, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    MasterOfTrials = 0x100B76,
}
public enum AID : uint
{
    Glaciate = 40679, // 463B->self, 4.5+0.5s cast, single-target
    Glaciate1 = 40680, // 233C->player/463D/463E/463F, 5.0s cast, range 6 circle
    Glaciate2 = 40691, // 463B->self, 6.5+0.5s cast, single-target
    Glaciate3 = 40692, // 233C->player/463D/463E/463F, 7.0s cast, range 6 circle
    FrigidRing = 40694, // 233C->self, 7.0s cast, range 10-20 donut
    BitterChill = 40693, // 233C->self, 11.0s cast, range 10 circle

    A1 = 40677, // 233C->location, no cast, range 20 circle --- Pretty sure this is the Heal

    SpectralBlazeVisual1 = 40682, // 463B->self, 5.5+0.5s cast, single-target
    SpectralBlazeVisual2 = 40695, // 463B->self, 6.5+0.5s cast, single-target
    SpectralBlaze1 = 40683, // 233C->463E, 6.0s cast, range 6 circle
    SpectralBlaze2 = 40696, // 233C->463D, 7.0s cast, range 6 circle
    Fireflood = 40697, // 233C->location, 7.0s cast, range 15 circle

    ScorchingStreakVisual1 = 40685, // 463B->player, 6.5+0.5s cast, single-target
    ScorchingStreakVisual2 = 40698, // 463B->player, 6.5+0.5s cast, single-target
    ScorchingStreak1 = 40686, // 233C->self, no cast, range 44 width 10 rect
    ScorchingStreak2 = 40699, // 233C->self, no cast, range 44 width 10 rect
    BlazingRing = 40700, // 233C->self, 7.0s cast, range 8-25 donut

    PiercingStone = 40688, // 463B->self, 4.5+0.5s cast, single-target
    PiercingStone1 = 40689, // 233C->location, 4.0s cast, range 4 circle
    PiercingStone2 = 40690, // 233C->location, 1.5s cast, range 4 circle
    PiercingStone3 = 40701, // 463B->self, 4.5+0.5s cast, single-target
    PiercingStone4 = 40702, // 233C->location, 4.0s cast, range 4 circle
    PiercingStone5 = 40703, // 233C->location, 1.5s cast, range 4 circle

}
public enum IconID : uint
{
    GlaciateSpread1 = 139,
    SpectralBlazeStack1 = 318,
    GlaciateSpread2 = 375,
    SpectralBlazeStack2 = 317,
    ScorchingStreakTankbuster = 412,
}
class StartingPositions(BossModule module) : BossComponent(module)
{
    private WPos? _readyPos;
    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            _readyPos = index switch
            {
                0 => new WPos(-3.3f, 0f),
                1 => new WPos(-9.9f, 9.5f),
                2 => new WPos(-7.7f, -0.1f),
                3 => new WPos(-14.8f, 0f),
                4 => new WPos(-3.2f, -0.1f),
                _ => default
            };
        }
        if (state == 0x00200010)
        {
            _readyPos = null;
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_readyPos is not WPos pos)
            return;
        hints.AddForbiddenZone(ShapeContains.InvertedCircle(pos, 1f));
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_readyPos is WPos pos)
            Arena.AddCircle(pos, 1f, ArenaColor.Safe);
    }
}
class Interact(BossModule module) : BossComponent(module)
{
    public Actor? InteractNPC => Module.Enemies(OID.MasterOfTrials).FirstOrDefault();
    public bool _interactReady;
    public bool _trialComplete;
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (InteractNPC != null && InteractNPC.IsTargetable && (_interactReady || _trialComplete))
        {
            hints.InteractWithOID(WorldState, OID.MasterOfTrials); // We are missing the logic to complete the NPC interaction - We need to be able to crawl through dialogue, select "Commence the final challenge.", and then select "Yes" in the following YesNo Addon.
        }
    }
    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        base.OnEventDirectorUpdate(updateID, param1, param2, param3, param4);
        if (updateID == 0x80000015 && param1 == 0x4 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0) //NPC Asks to speak
        {
            _interactReady = true;
        }
        if (updateID == 0x80000016 && param1 == 0x5 && param2 == 0x1 && param3 == 0x0 && param4 == 0x0) //Spoken to NPC - Break loop
        {
            _interactReady = false;
        }
        if (updateID == 0x00000001 && param1 == 0x0 && param2 == 0x0 && param3 == 0x0 && param4 == 0x0) //Duty has been Completed
        {
            _trialComplete = true;
        }
    }
}
class Glaciate1(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.GlaciateSpread1, AID.Glaciate1, 6f, 0, true);
class Glaciate2(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.GlaciateSpread2, AID.Glaciate3, 6f, 0, true);
class SpectralBlaze1(BossModule module) : Components.StackWithIcon(module, (uint)IconID.SpectralBlazeStack1, AID.SpectralBlazeVisual1, 6f, 0, 2)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SpectralBlazeVisual1 or AID.SpectralBlazeVisual2 or AID.SpectralBlaze1 or AID.SpectralBlaze2)
        {
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedStacks;
        }
    }
}
class SpectralBlaze2(BossModule module) : Components.StackWithIcon(module, (uint)IconID.SpectralBlazeStack2, AID.SpectralBlazeVisual2, 6f, 0, 2)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SpectralBlazeVisual1 or AID.SpectralBlazeVisual2 or AID.SpectralBlaze1 or AID.SpectralBlaze2)
        {
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            ++NumFinishedStacks;
        }
    }
}
class ScorchingStreak1(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(44, 5), (uint)IconID.ScorchingStreakTankbuster, AID.ScorchingStreakVisual1, centerAtTarget: false, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count > 0)
        {
            if (actor.Role is Role.Tank)
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(new WPos(0f, -15f), 3));
            }
            else
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(new WPos(0f, 15f), 3));
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ScorchingStreakVisual1 or AID.ScorchingStreakVisual2 or AID.ScorchingStreak1 or AID.ScorchingStreak2)
        {
            CurrentBaits.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }
}
class PiercingStone1(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4f), AID.PiercingStone1, AID.PiercingStone2, 5f, 3, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Chasers.Count > 0)
        {
            hints.AddForbiddenZone(ShapeContains.Circle(new WPos(0f, 13f), 30));
        }
    }
}
class PiercingStone2(BossModule module) : Components.StandardChasingAOEs(module, new AOEShapeCircle(4f), AID.PiercingStone4, AID.PiercingStone5, 5f, 3, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Chasers.Count > 0)
        {
            hints.AddForbiddenZone(ShapeContains.DonutSector(new WPos(0f, 0f), 13, 20, 45.Degrees(), 135.Degrees()));
        }
    }
}
class FrigidRing(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeDonut donut = new(10, 20);
    private static readonly AOEShapeCircle circ = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes.Take(1))
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FrigidRing)
        {
            _aoes.Add(new(donut, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
        if ((AID)spell.Action.ID is AID.BitterChill)
        {
            _aoes.Add(new(circ, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.FrigidRing or AID.BitterChill)
        {
            _aoes.RemoveAt(0);
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoes.Count > 1)
        {
            hints.AddForbiddenZone(ShapeContains.InvertedCircle(new WPos(0, 0), 9f));
        }
        else if (_aoes.Count == 1)
            hints.AddForbiddenZone(ShapeContains.Circle(new WPos(0, 0), 11f));
    }
}
class Fireflood(BossModule module) : Components.StandardAOEs(module, AID.Fireflood, 15f);
class BlazingRing(BossModule module) : Components.StandardAOEs(module, AID.BlazingRing, new AOEShapeDonut(8, 25));

class NA01ReactToFloorMarkersStates : StateMachineBuilder
{
    public NA01ReactToFloorMarkersStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<StartingPositions>()
            .ActivateOnEnter<Interact>()
            .ActivateOnEnter<Glaciate1>()
            .ActivateOnEnter<Glaciate2>()
            .ActivateOnEnter<SpectralBlaze1>()
            .ActivateOnEnter<SpectralBlaze2>()
            .ActivateOnEnter<ScorchingStreak1>()
            .ActivateOnEnter<PiercingStone1>()
            .ActivateOnEnter<PiercingStone2>()
            .ActivateOnEnter<FrigidRing>()
            .ActivateOnEnter<Fireflood>()
            .ActivateOnEnter<BlazingRing>();
    }
}
[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1013, NameID = 13616)]
public class NA01ReactToFloorMarkers(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(19.5f))
{
    protected override bool CheckPull() => !PrimaryActor.IsDeadOrDestroyed;
}
