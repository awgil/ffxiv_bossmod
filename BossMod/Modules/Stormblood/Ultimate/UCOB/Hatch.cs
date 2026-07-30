namespace BossMod.Stormblood.Ultimate.UCOB;

class Hatch(BossModule module) : Components.CastCounter(module, (uint)AID.Hatch)
{
    public bool Active = true;
    public override bool KeepOnPhaseChange => true;
    public int NumNeurolinkSpawns;
    public int NumTargetsAssigned;
    private readonly List<Actor> _orbs = module.Enemies((uint)OID.Oviform);
    private readonly List<Actor> _neurolinks = module.Enemies((uint)OID.Neurolink);
    private BitMask _targets;

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
                Arena.ZoneCircle(o.Position, 1, Colors.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Active)
            foreach (var neurolink in _neurolinks)
                Arena.ZoneCircleOutline(neurolink.Position, 2, _targets[pcSlot] ? Colors.Safe : Colors.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Generate)
        {
            _targets.Set(Raid.FindSlot(actor.InstanceID));
            ++NumTargetsAssigned;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            foreach (var t in spell.Targets)
                _targets.Clear(Raid.FindSlot(t.ID));
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Twintania && id == 0x94)
            ++NumNeurolinkSpawns;
    }
}
