namespace BossMod.Shadowbringers.Dungeon.D13Paglthan.D131Amhuluk;

public enum OID : uint
{
    Boss = 0x3169, // R7.008, x1
    Helper = 0x233C, // R0.500, x6, Helper type
    BallOfLevin = 0x31A2, // R1.300, x0 (spawn during fight)
    SuperchargedLevin = 0x31A3, // R2.300, x0 (spawn during fight)
}

public enum AID : uint
{
    CriticalRip = 23630, // Boss->player, 5.0s cast, single-target
    LightningBolt = 23627, // Boss->self, 10.0s cast, single-target
    LightningBoltRod = 23628, // Helper->location, no cast, range 10 circle
    ElectricBurst = 23629, // Boss->self, 4.5s cast, range 50 width 40 rect
    WideBlaster = 24773, // Boss->self, 4.0s cast, range 26 120-degree cone
    SpikeFlail = 23631, // Boss->self, 1.0s cast, range 25 60-degree cone
}

public enum SID : uint
{
    LightningRod = 2574, // none->player/31B6, extra=0x114
}

class Levin(BossModule module) : Components.GenericAOEs(module)
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var big in Module.Enemies(OID.SuperchargedLevin))
            yield return new AOEInstance(new AOEShapeCircle(10), big.Position);
        foreach (var small in Module.Enemies(OID.BallOfLevin))
            yield return new AOEInstance(new AOEShapeCircle(5), small.Position);
    }
}

// claims to be a 50/40 rect, but hits behind boss, idk man
class ElectricBurst(BossModule module) : Components.RaidwideCast(module, AID.ElectricBurst);
class CriticalRip(BossModule module) : Components.SingleTargetCast(module, AID.CriticalRip);

class SpikeBlaster(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.WideBlaster)
        {
            aoes.Add(new AOEInstance(new AOEShapeCone(26, 60.Degrees()), caster.Position, spell.Rotation, Module.CastFinishAt(spell), ArenaColor.Danger));
            aoes.Add(new AOEInstance(new AOEShapeCone(25, 30.Degrees()), caster.Position, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 2.7f), ArenaColor.AOE));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WideBlaster:
                aoes.RemoveAt(0);
                aoes.Ref(0).Color = ArenaColor.Danger;
                break;
            case AID.SpikeFlail:
                aoes.RemoveAt(0);
                break;
        }
    }
}

class LightningRod(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(int, Actor)> Rods = [];
    private BitMask RodStates;
    private BitMask PlayerStates;
    private DateTime? LightningBoltAt;

    private IEnumerable<Actor> ActiveRods => Rods.IncludedInMask(RodStates).Select(r => r.Item2);
    private IEnumerable<Actor> InactiveRods => Rods.ExcludedFromMask(RodStates).Select(r => r.Item2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (LightningBoltAt == null)
            yield break;

        foreach (var r in ActiveRods)
            yield return new AOEInstance(new AOEShapeCircle(10), r.Position, Activation: LightningBoltAt.Value);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x31B6)
            Rods.Add((Rods.Count, actor));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LightningBolt)
            LightningBoltAt = Module.CastFinishAt(spell);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LightningBoltRod)
            LightningBoltAt = null;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightningRod)
        {
            if (actor.OID == 0x31B6)
                RodStates.Set(Rods.FindIndex(r => r.Item2 == actor));
            else
                PlayerStates.Set(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.LightningRod)
        {
            if (actor.OID == 0x31B6)
                RodStates.Clear(Rods.FindIndex(r => r.Item2 == actor));
            else
                PlayerStates.Clear(Raid.FindSlot(actor.InstanceID));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        if (PlayerStates[pcSlot])
            foreach (var rod in InactiveRods)
                Arena.AddCircle(rod.Position, 3, ArenaColor.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (var (_, player) in Raid.WithSlot().IncludedInMask(PlayerStates))
            Arena.AddCircle(player.Position, 10, ArenaColor.Danger);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (PlayerStates[slot])
            hints.Add("Pass debuff!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!PlayerStates[slot] || LightningBoltAt == null)
            return;

        var inactiveRods = InactiveRods.Select(r => ShapeContains.InvertedCircle(r.Position, 3)).ToList();
        if (inactiveRods.Count > 0)
        {
            var zone = ShapeContains.Intersection(inactiveRods);
            hints.AddForbiddenZone(zone, LightningBoltAt.Value);
        }
    }
}

class AmhulukStates : StateMachineBuilder
{
    public AmhulukStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CriticalRip>()
            .ActivateOnEnter<SpikeBlaster>()
            .ActivateOnEnter<LightningRod>()
            .ActivateOnEnter<ElectricBurst>()
            .ActivateOnEnter<Levin>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 777, NameID = 10075)]
public class Amhuluk(WorldState ws, Actor primary) : BossModule(ws, primary, new(-520, 145), new ArenaBoundsCircle(20));
