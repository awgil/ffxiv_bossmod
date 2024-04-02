namespace BossMod.Stormblood.Ultimate.UCOB;

class Hatch : Components.CastCounter
{
    public bool Active = true;
    public int NumNeurolinkSpawns { get; private set; }
    public int NumTargetsAssigned { get; private set; }
    private IReadOnlyList<Actor> _orbs = ActorEnumeration.EmptyList;
    private IReadOnlyList<Actor> _neurolinks = ActorEnumeration.EmptyList;
    private BitMask _targets;

    public Hatch() : base(ActionID.MakeSpell(AID.Hatch)) { KeepOnPhaseChange = true; }

    public void Reset()
    {
        _targets.Reset();
        NumTargetsAssigned = NumCasts = 0;
    }

    public override void Init(BossModule module)
    {
        _orbs = module.Enemies(OID.Oviform);
        _neurolinks = module.Enemies(OID.Neurolink);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (!Active)
            return;

        var inNeurolink = _neurolinks.InRadius(actor.Position, 2).Any();
        if (_targets[slot])
            hints.Add("Go to neurolink!", !inNeurolink);
        else if (inNeurolink)
            hints.Add("GTFO from neurolink!");
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Active && _targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Active)
            foreach (var o in _orbs.Where(o => !o.IsDead))
                arena.ZoneCircle(o.Position, 1, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (Active)
            foreach (var neurolink in _neurolinks)
                arena.AddCircle(neurolink.Position, 2, _targets[pcSlot] ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Generate)
        {
            _targets.Set(module.Raid.FindSlot(actor.InstanceID));
            ++NumTargetsAssigned;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                _targets.Clear(module.Raid.FindSlot(t.ID));
        }
    }

    public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Twintania && id == 0x94)
            ++NumNeurolinkSpawns;
    }
}
