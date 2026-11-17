namespace BossMod.Stormblood.Alliance.A23Construct7;

class ComputationMode(BossModule module) : BossComponent(module)
{
    bool Active;

    record struct Predicate(int Mod, DateTime Deadline);

    Predicate? _pred;

    readonly int[] HPBoost = new int[PartyState.MaxAllianceSize];

    static readonly (int Value, WPos Position)[] Towers = [
        (1, new(782.5f, -137)),
        (2, new(794, -137)),
        (3, new(806, -137)),
        (4, new(817.5f, -137))
    ];

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (!Active)
            return;

        foreach (var (i, p) in Towers)
        {
            var color = Ok(pcSlot, pc, i) && !Ok(pcSlot, pc, 0) ? ArenaColor.Safe : ArenaColor.Border;
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(p, 5, 0xFF000000, 2);
            Arena.AddCircle(p, 5, color);
        }
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EA1A1 && state == 0x00010002)
            Active = true;

        if (actor.OID == 0x1EA1A1 && state == 0x00040008)
        {
            Active = false;
            Array.Fill(HPBoost, 0);
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        var id = (SID)status.ID switch
        {
            SID.HPBoost1 => 1,
            SID.HPBoost2 => 2,
            SID.HPBoost3 => 3,
            SID.HPBoost4 => 4,
            _ => 0
        };
        if (id > 0 && Raid.TryFindSlot(actor, out var slot))
            HPBoost[slot] = id;
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID is SID.HPBoost1 or SID.HPBoost2 or SID.HPBoost3 or SID.HPBoost4 && Raid.TryFindSlot(actor, out var slot))
            HPBoost[slot] = 0;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DivideByThree:
                _pred = new(3, Module.CastFinishAt(spell));
                break;
            case AID.DivideByFour:
                _pred = new(4, Module.CastFinishAt(spell));
                break;
            case AID.DivideByFive:
                _pred = new(5, Module.CastFinishAt(spell));
                break;
            case AID.Indivisible:
                _pred = new(1, Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DivideByFour or AID.DivideByThree or AID.DivideByFive or AID.Indivisible)
            _pred = null;
    }

    private uint BaseHP(int slot, Actor actor) => actor.HPMP.MaxHP - (uint)HPBoost[slot];

    private bool Ok(int slot, Actor actor, int towerValue)
    {
        var hpHere = BaseHP(slot, actor) + towerValue;

        return _pred switch
        {
            { Mod: 1 } => hpHere > 1 && hpHere % 2 > 0 && hpHere % 3 > 0 && hpHere % 5 > 0,
            { Mod: 3 } => hpHere % 3 == 0,
            { Mod: 4 } => hpHere % 4 == 0,
            { Mod: 5 } => hpHere % 5 == 0,
            _ => false,
        };
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_pred is not { } p)
            return;

        if (Ok(slot, actor, 0))
            hints.AddForbiddenZone(ShapeDistance.Union([.. Towers.Where(t => !Ok(slot, actor, t.Value)).Select(t => ShapeDistance.Circle(t.Position, 5))]), p.Deadline);
        else
            hints.AddForbiddenZone(ShapeDistance.Union([.. Towers.Where(t => Ok(slot, actor, t.Value)).Select(t => ShapeDistance.Circle(t.Position, 5))]).Inverted(), p.Deadline);
    }
}
