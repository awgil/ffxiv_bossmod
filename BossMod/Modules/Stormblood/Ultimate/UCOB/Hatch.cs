namespace BossMod.Stormblood.Ultimate.UCOB;

class Hatch : Components.CastCounter
{
    public bool Active = true;
    public int NumNeurolinkSpawns { get; private set; }
    public int NumTargetsAssigned { get; private set; }
    private readonly IReadOnlyList<Actor> _orbs;
    private readonly IReadOnlyList<Actor> _neurolinks;
    private BitMask _targets;

    public Hatch(BossModule module) : base(module, ActionID.MakeSpell(AID.Hatch))
    {
        _orbs = module.Enemies(OID.Oviform);
        _neurolinks = module.Enemies(OID.Neurolink);
        KeepOnPhaseChange = true;
    }

    public void Reset()
    {
        _targets.Reset();
        NumTargetsAssigned = NumCasts = 0;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Active)
            return;

        var inNeurolink = _neurolinks.InRadius(actor.Position, 2).Any();
        if (_targets[slot])
            hints.Add("Go to neurolink!", !inNeurolink);
        else if (inNeurolink)
            hints.Add("GTFO from neurolink!");
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return Active && _targets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Irrelevant;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Active)
            foreach (var o in _orbs.Where(o => !o.IsDead))
                Arena.ZoneCircle(o.Position, 1, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Active)
            foreach (var neurolink in _neurolinks)
                Arena.AddCircle(neurolink.Position, 2, _targets[pcSlot] ? ArenaColor.Safe : ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Generate)
        {
            _targets.Set(Raid.FindSlot(actor.InstanceID));
            ++NumTargetsAssigned;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                _targets.Clear(Raid.FindSlot(t.ID));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Twintania && id == 0x94)
            ++NumNeurolinkSpawns;
    }
}
