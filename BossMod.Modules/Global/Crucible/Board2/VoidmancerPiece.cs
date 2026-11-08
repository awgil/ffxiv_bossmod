namespace BossMod.Global.Crucible.VoidmancerPiece;

public enum OID : uint
{
    Boss = 0x4C5C, // R2.340, x1
    ZombiePiece = 0x4C5D, // R0.750, x20
    Malady = 0x4C5E, // R1.000, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x1, Helper type
}

public enum AID : uint
{
    Water = 50793, // Boss->player, no cast, single-target
    DeathDriveCast = 48181, // Boss->self, 7.0s cast, single-target
    DeathDrive = 48182, // Helper->location, 4.0s cast, range 10 circle
    ZombieAutos = 48251, // 4C5D->player, no cast, single-target
    DarkOrbCast = 48185, // Boss->self, 6.5+1.5s cast, single-target
    DarkOrb = 48186, // Helper->self, 8.0s cast, range 18 circle
    EvilMist = 48183, // Boss->self, 4.0s cast, range 60 circle
    Necropurge = 48184, // 4C5E->self, 1.0s cast, range 8 circle
    Mindjack = 48187, // Boss->self, 4.0s cast, range 100 circle
}

public enum SID : uint
{
    WitsEnd = 5424, // 4C5D->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xB/0xA
    ForcedMarch = 1257, // Boss->player, extra=0x2/0x4
    AboutFace = 2162, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    RightFace = 2164, // Boss->player, extra=0x0
    Confused = 1283, // 4C5D->player, extra=0x0
}

public enum IconID : uint
{
    DeathDrive = 707, // player->self
}

class DeathDriveBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private int[] _zombieCounter = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        foreach (var bait in ActiveBaitsOn(actor))
        {
            var num = Module.Enemies(OID.ZombiePiece).Count(z => z.IsDead && z.Position.InCircle(actor.Position, 10.75f));
            hints.Add($"Zombies hit: {num}", false);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_zombieCounter.Length == 0)
        {
            var zombies = Module.Enemies(OID.ZombiePiece);

            _zombieCounter = hints.GenerateFromMap((p, _) => zombies.Count(z => z.IsDead && z.Position.InCircle(p, 10.75f)));
        }

        foreach (var bait in ActiveBaitsOn(actor))
            hints.AddForbiddenZone(Sdf.Indexed((_, i) => _zombieCounter.BoundSafeAt(i) > 1), bait.Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurrentBaits.Count > 0)
            foreach (var m in Module.Enemies(OID.ZombiePiece).Where(z => z.IsDead))
                Arena.AddCircle(m.Position, 0.75f, ArenaColor.Object);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.DeathDrive)
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(10), WorldState.FutureTime(7.6f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DeathDrive)
            CurrentBaits.Clear();
    }

    public override void OnIsDeadChanged(Actor actor)
    {
        if ((OID)actor.OID == OID.ZombiePiece)
            _zombieCounter = []; // force recalculation
    }
}

class DeathDrive(BossModule module) : Components.StandardAOEs(module, AID.DeathDrive, 10);
class ZombiePiece(BossModule module) : ProximityAdds(module, OID.ZombiePiece);
class DarkOrb(BossModule module) : Components.StandardAOEs(module, AID.DarkOrb, 18);
class EvilMist(BossModule module) : Components.RaidwideCast(module, AID.EvilMist);

class Necropurge(BossModule module) : Components.GenericAOEs(module, AID.Necropurge)
{
    readonly List<(Actor Source, DateTime Next, int Casts)> _orbs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _orbs.Select(o => new AOEInstance(new AOEShapeCircle(8), o.Source.Position, default, o.Next));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Malady)
            _orbs.Add((actor, WorldState.FutureTime(3.8f), 0));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            var ix = _orbs.FindIndex(o => o.Source == caster);
            if (ix >= 0)
            {
                _orbs.Ref(ix).Casts++;
                _orbs.Ref(ix).Next = WorldState.FutureTime(5.1f);
            }
            if (_orbs[ix].Casts >= 8)
                _orbs.RemoveAt(ix);
        }
    }
}

// TODO: hints to not walk into necropurge
class Mindjack(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, 0, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace);

class VoidmancerPieceStates : StateMachineBuilder
{
    public VoidmancerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DeathDriveBait>()
            .ActivateOnEnter<DeathDrive>()
            .ActivateOnEnter<ZombiePiece>()
            .ActivateOnEnter<DarkOrb>()
            .ActivateOnEnter<EvilMist>()
            .ActivateOnEnter<Necropurge>()
            .ActivateOnEnter<Mindjack>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14552)]
public class VoidmancerPiece(ModuleInit init) : BossModule(init, new(120, 0), new ArenaBoundsSquare(20));

