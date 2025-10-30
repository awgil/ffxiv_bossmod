namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class GrimEmbraceBait(BossModule module) : Components.GenericBaitAway(module)
{
    public struct PlayerState
    {
        public AOEShapeRect? Shape;
        public DateTime Activation;
    }

    private readonly PlayerState[] _states = new PlayerState[PartyState.MaxAllianceSize];

    private static readonly AOEShapeRect _shapeForward = new(8, 4);
    private static readonly AOEShapeRect _shapeBackward = new(0, 4, 8);

    public override void Update()
    {
        CurrentBaits.Clear();
        var deadline = WorldState.FutureTime(7);
        foreach (var (i, p) in Raid.WithSlot())
        {
            ref var s = ref _states[i];
            if (s.Shape != null && s.Activation != default && s.Activation < deadline)
                CurrentBaits.Add(new(p, p, s.Shape, s.Activation));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref var s = ref _states[slot];
        if (s.Shape != null && s.Activation != default)
            hints.Add($"Dodge {(s.Shape == _shapeForward ? "backward" : "forward")} in {Math.Max(0, (s.Activation - WorldState.CurrentTime).TotalSeconds):f1}s", false);
        base.AddHints(slot, actor, hints);
    }

    //public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    //{
    //    var s = _states[slot];
    //    if (s.Shape != null)
    //        hints.AddSpecialMode(AIHints.SpecialMode.Pyretic, s.Activation); // TODO: reconsider? i want to ensure character won't turn last moment...
    //}

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var shape = (TetherID)tether.ID switch
        {
            TetherID.GrimEmbraceForward => _shapeForward,
            TetherID.GrimEmbraceBackward => _shapeBackward,
            _ => null
        };
        if (shape != null && Raid.FindSlot(source.InstanceID) is var slot && slot >= 0 && slot < _states.Length)
            _states[slot].Shape = shape;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DeadlyEmbrace && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot < _states.Length)
            _states[slot].Activation = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DeadlyEmbrace && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot < _states.Length)
            _states[slot] = default;
    }
}

class GrimEmbraceAOE(BossModule module) : Components.StandardAOEs(module, AID.GrimEmbraceAOE, new AOEShapeRect(8, 4));
