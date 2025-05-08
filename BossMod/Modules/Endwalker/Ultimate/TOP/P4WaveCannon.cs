namespace BossMod.Endwalker.Ultimate.TOP;

class P4WaveCannonProtean(BossModule module) : Components.GenericBaitAway(module)
{
    private Actor? _source;

    private static readonly AOEShapeRect _shape = new(100, 3);

    public void Show()
    {
        NumCasts = 0;
        if (_source != null)
            foreach (var p in Raid.WithoutSlot(true))
                CurrentBaits.Add(new(_source, p, _shape));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.P4WaveCannonVisualStart)
        {
            _source = caster;
            Show();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.P4WaveCannonProtean)
        {
            CurrentBaits.Clear();
            ++NumCasts;
        }
    }
}

class P4WaveCannonProteanAOE(BossModule module) : Components.StandardAOEs(module, AID.P4WaveCannonProteanAOE, new AOEShapeRect(100, 3));

// TODO: generalize (line stack)
class P4WaveCannonStack : BossComponent
{
    public bool Imminent;
    private BitMask _targets;
    private readonly int[] _playerGroups = Utils.MakeArray(PartyState.MaxPartySize, -1);
    private BitMask _westStack;

    private static readonly AOEShapeRect _shape = new(100, 3);

    public bool Active => _targets.Any();

    public P4WaveCannonStack(BossModule module) : base(module)
    {
        foreach (var (s, g) in Service.Config.Get<TOPConfig>().P4WaveCannonAssignments.Resolve(Raid))
            _playerGroups[s] = g;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Imminent && Raid.WithSlot(true).IncludedInMask(_targets).WhereActor(p => _shape.Check(actor.Position, Module.Center, Angle.FromDirection(p.Position - Module.Center))).Count() is var clips && clips != 1)
            hints.Add(clips == 0 ? "Share the stack!" : "GTFO from second stack!");
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (SafeDir(slot) is var safeDir && safeDir != default)
            movementHints.Add(actor.Position, Module.Center + 12 * safeDir.ToDirection(), ArenaColor.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Imminent)
            foreach (var (_, p) in Raid.WithSlot(true).IncludedInMask(_targets))
                _shape.Outline(Arena, Module.Center, Angle.FromDirection(p.Position - Module.Center), ArenaColor.Safe);

        var safeDir = SafeDir(pcSlot);
        if (safeDir != default)
            Arena.AddCircle(Module.Center + 12 * safeDir.ToDirection(), 1, ArenaColor.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.P4WaveCannonStackTarget:
                _targets.Set(Raid.FindSlot(spell.MainTargetID));
                if (_targets.NumSetBits() > 1)
                    InitWestStack();
                break;
            case AID.P4WaveCannonStack:
                Imminent = false;
                _targets.Reset();
                _westStack.Reset();
                break;
        }
    }

    private void InitWestStack()
    {
        int e4 = -1, w4 = -1, maxTarget = -1;
        for (int i = 0; i < _playerGroups.Length; ++i)
        {
            var g = _playerGroups[i];
            if ((g & 1) == 0)
                _westStack.Set(i);
            if (g == 6)
                w4 = i;
            if (g == 7)
                e4 = i;
            if (_targets[i] && (maxTarget < 0 || g > _playerGroups[maxTarget]))
                maxTarget = i;
        }

        if (_westStack.None())
            return; // no assignments available

        var westTargets = _westStack & _targets;
        if (westTargets.None())
        {
            // swap W4 with southernmost target
            _westStack.Clear(w4);
            _westStack.Set(maxTarget);
        }
        else if (westTargets == _targets)
        {
            // swap E4 with southernmost target
            _westStack.Set(e4);
            _westStack.Clear(maxTarget);
        }
    }

    private Angle SafeDir(int slot)
    {
        var group = _playerGroups[slot];
        return group < 0 ? default : Imminent
            ? (_westStack[slot] ? -22.5f : 22.5f).Degrees()
            : (((group & 1) != 0) ? 1 : -1) * (112.5f - 22.5f * (group >> 1)).Degrees();
    }
}
