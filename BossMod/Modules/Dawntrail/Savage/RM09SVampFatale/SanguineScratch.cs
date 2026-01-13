namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

class BloodyBondageBig(BossModule module) : Components.CastTowers(module, AID.BloodyBondageParty, 6, minSoakers: 4, maxSoakers: 4);

// guessing that max tether distance is 12 units, which is the distance from the bat to the boss
class BatTether(BossModule module) : BossComponent(module)
{
    private readonly (Actor?, bool)[] _tetherTarget = new (Actor?, bool)[8];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        bool? danger = (TetherID)tether.ID switch
        {
            TetherID.ShortTether => false,
            TetherID.LongTether => true,
            _ => null
        };
        if (danger.HasValue)
        {
            if (Raid.TryFindSlot(source, out var slot) && WorldState.Actors.Find(tether.Target) is { } tar)
                _tetherTarget[slot] = (tar, danger.Value);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var (target, danger) = _tetherTarget[pcSlot];
        if (target != null)
        {
            Arena.AddLine(pc.Position, target.Position, danger ? ArenaColor.Danger : ArenaColor.Border);
            Arena.AddCircle(target.Position, 12, ArenaColor.Danger);
        }
    }
}

class BatShapePredict(BossModule module) : Components.GenericAOEs(module)
{
    static readonly AOEShape Donut = new AOEShapeDonut(8, 15);
    static readonly AOEShape Circle = new AOEShapeCircle(7);

    readonly Dictionary<ulong, WDir> _startPositions = [];

    private readonly List<AOEInstance> _aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Unk1957 && (OID)actor.OID == OID.VampetteFatale)
            _startPositions[actor.InstanceID] = actor.Position - Arena.Center;

        if ((SID)status.ID == SID.Unk2056 && _startPositions.TryGetValue(actor.InstanceID, out var pos))
        {
            _aoes.Add(new(status.Extra == 0x426 ? Circle : Donut, Arena.Center - pos, default, WorldState.FutureTime(10)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BreakdownDrop1 or AID.BreakwingBeat1 or AID.BreakdownDrop2 or AID.BreakwingBeat2)
        {
            _startPositions[caster.InstanceID] = spell.LocXZ - Arena.Center;
            _aoes.Clear();
        }
    }
}

class BreakdownDrop(BossModule module) : Components.GroupedAOEs(module, [AID.BreakdownDrop1, AID.BreakdownDrop2], new AOEShapeCircle(7));
class BreakwingBeat(BossModule module) : Components.GroupedAOEs(module, [AID.BreakwingBeat1, AID.BreakwingBeat2], new AOEShapeDonut(4, 15));

class BreakCounter(BossModule module) : Components.CastCounterMulti(module, [AID.BreakdownDrop1, AID.BreakdownDrop2, AID.BreakwingBeat1, AID.BreakwingBeat2]);

class SanguineScratch(BossModule module) : Components.StandardAOEs(module, AID.SanguineScratchFirst, new AOEShapeCone(40, 15.Degrees()));

class SanguineScratchRepeat(BossModule module) : Components.GenericAOEs(module, AID.SanguineScratchRepeat)
{
    private readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SanguineScratchFirst)
            NumCasts = 0;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SanguineScratchFirst or AID.SanguineScratchRepeat)
        {
            NumCasts++;
            _predicted.RemoveAll(p => p.Rotation.AlmostEqual(spell.Rotation, 0.05f));
            if (NumCasts < 33)
                _predicted.Add(new(new AOEShapeCone(40, 15.Degrees()), Arena.Center, spell.Rotation + 22.5f.Degrees(), WorldState.FutureTime(2.5f)));
        }
    }
}
