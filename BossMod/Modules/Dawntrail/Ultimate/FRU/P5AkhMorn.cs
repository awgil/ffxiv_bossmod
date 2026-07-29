namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P5AkhMorn(BossModule module) : Components.UniformStackSpread(module, 4f, 0, 4)
{
    public Actor? Source;
    private static readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly P5FulgentBlade? _fulgent = module.FindComponent<P5FulgentBlade>();
    private BitMask _leftSoakers;
    private DateTime _activation;

    public override void Update()
    {
        Stacks.Clear();
        if (Source != null)
        {
            var left = Source.Rotation.ToDirection().OrthoL();
            Actor? targetL = null, targetR = null;
            float distL = float.MaxValue, distR = float.MaxValue;
            foreach (var p in Raid.WithoutSlot(false, true, true))
            {
                var off = p.Position - Source.Position;
                var side = left.Dot(off) > 0;
                ref var target = ref side ? ref targetL : ref targetR;
                ref var dist = ref side ? ref distL : ref distR;
                var d = off.LengthSq();
                if (d < dist)
                {
                    dist = d;
                    target = p;
                }
            }
            if (targetL != null)
                AddStack(targetL, _activation);
            if (targetR != null)
                AddStack(targetR, _activation);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Source != null && _leftSoakers.Any() && _fulgent?.NumCasts > 6)
        {
            var dir = Source.Rotation + (_leftSoakers[slot] ? -45f : 45f).Degrees(); // note that left group go to boss right!
            hints.AddForbiddenZone(new SDInvertedCircle(Source.Position + 5f * dir.ToDirection(), 1f), _activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.AkhMornPandora)
        {
            Source = caster;
            _activation = Module.CastFinishAt(spell, 0.1d);
            foreach (var (slot, group) in _config.P5AkhMornAssignments.Resolve(Raid))
                _leftSoakers[slot] = group == 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.AkhMornPandoraAOE1 or (uint)AID.AkhMornPandoraAOE2)
            Source = null;
    }
}
