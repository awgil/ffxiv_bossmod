namespace BossMod.Global.Crucible.VoidmancerPiece;

public enum OID : uint
{
    Boss = 0x4C5C, // R2.340, x1
    _Gen_ZombiePiece = 0x4C5D, // R0.750, x20
    _Gen_Malady = 0x4C5E, // R1.000, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x1, Helper type
}

public enum AID : uint
{
    _Spell_Water = 50793, // Boss->player, no cast, single-target
    _Weaponskill_DeathDrive = 48181, // Boss->self, 7.0s cast, single-target
    _Weaponskill_DeathDrive1 = 48182, // Helper->location, 4.0s cast, range 10 circle
    _AutoAttack_ = 48251, // 4C5D->player, no cast, single-target
    _Spell_DarkOrb = 48185, // Boss->self, 6.5+1.5s cast, single-target
    _Spell_DarkOrb1 = 48186, // Helper->self, 8.0s cast, range 18 circle
    _Weaponskill_EvilMist = 48183, // Boss->self, 4.0s cast, range 60 circle
    _Weaponskill_Necropurge = 48184, // 4C5E->self, 1.0s cast, range 8 circle
    _Weaponskill_Mindjack = 48187, // Boss->self, 4.0s cast, range 100 circle
}

public enum SID : uint
{
    _Gen_WitsEnd = 5424, // 4C5D->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xB/0xA
    _Gen_AboutFace = 2162, // Boss->player, extra=0x0
    _Gen_ForcedMarch = 1257, // Boss->player, extra=0x2/0x4
    _Gen_Confused = 1283, // 4C5D->player, extra=0x0
    _Gen_RightFace = 2164, // Boss->player, extra=0x0
    _Gen_LeftFace = 2163, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    _Gen_Icon_suteloc7s10m_t0j2 = 707, // player->self
}

class DeathDriveBait(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private (WPos, int)[] grid = [];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        foreach (var bait in ActiveBaitsOn(actor))
        {
            var num = Module.Enemies(OID._Gen_ZombiePiece).Count(z => z.IsDead && z.Position.InCircle(actor.Position, 10.75f));
            hints.Add($"Zombies hit: {num}", false);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (grid.Length == 0)
        {
            var map = new Pathfinding.Map();
            hints.InitPathfindMap(map);
            grid = new (WPos, int)[(map.Width + 1) * (map.Height + 1)];

            var zombies = Module.Enemies(OID._Gen_ZombiePiece);

            foreach (var (cell, p) in map.EnumerateGrid())
                grid[cell] = (p, zombies.Count(z => z.Position.InCircle(p, 10.75f)));
        }

        //foreach (var bait in ActiveBaitsOn(actor))
        //    hints.AddForbiddenZone(Sdf.Discrete((_, i) => i < 0 || grid[i].Item2 > 1), bait.Activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        if (CurrentBaits.Count > 0)
            foreach (var m in Module.Enemies(OID._Gen_ZombiePiece))
                Arena.AddCircle(m.Position, 0.75f, ArenaColor.Object);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_suteloc7s10m_t0j2)
            CurrentBaits.Add(new(actor, actor, new AOEShapeCircle(10), WorldState.FutureTime(7.6f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_DeathDrive1)
            CurrentBaits.Clear();
    }
}

class DeathDrive(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_DeathDrive1, 10);
class ZombiePiece(BossModule module) : Components.Adds(module, (uint)OID._Gen_ZombiePiece);
class DarkOrb(BossModule module) : Components.StandardAOEs(module, AID._Spell_DarkOrb1, 18);
class EvilMist(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_EvilMist);

class Necropurge(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_Necropurge)
{
    readonly List<(Actor Source, DateTime Next, int Casts)> _orbs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _orbs.Select(o => new AOEInstance(new AOEShapeCircle(8), o.Source.Position, default, o.Next));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID._Gen_Malady)
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
            .ActivateOnEnter<Necropurge>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14552)]
public class VoidmancerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

