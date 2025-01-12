namespace BossMod.Dawntrail.Ultimate.FRU;

class P5AkhMorn(BossModule module) : Components.UniformStackSpread(module, 4, 0, 4)
{
    public Actor? Source;
    private DateTime _activation;

    public override void Update()
    {
        Stacks.Clear();
        if (Source != null)
        {
            var left = Source.Rotation.ToDirection().OrthoL();
            Actor? targetL = null, targetR = null;
            float distL = float.MaxValue, distR = float.MaxValue;
            foreach (var p in Raid.WithoutSlot())
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

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMornPandora)
        {
            Source = caster;
            _activation = Module.CastFinishAt(spell, 0.1f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMornPandoraAOE1 or AID.AkhMornPandoraAOE2)
            Source = null;
    }
}
