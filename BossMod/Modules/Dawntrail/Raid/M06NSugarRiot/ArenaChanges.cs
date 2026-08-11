namespace BossMod.Dawntrail.Raid.M06NSugarRiot;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private bool _risky = true;
    private AOEInstance[] _aoe = [];
    private bool active;
    private AOEShapeCustom? shape;
    private readonly M06NSugarRiot bossmod = (M06NSugarRiot)module;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x04)
        {
            return;
        }
        switch (state)
        {
            case 0x00020001u:
                active = true;
                break;
            case 0x00800040u:
                var center = Arena.Center;
                shape ??= GetRiverAOE(center);
                _aoe = [new(shape, center, default, WorldState.FutureTime(7d), shapeDistance: shape.Distance(center, default))];
                _risky = true;
                break;
            case 0x00200010u:
                Arena.Bounds = new ArenaBoundsCustom([new Square(Arena.Center, 20f)], bossmod.GetCombinedRiver());
                active = false;
                _aoe = [];
                break;
            case 0x08000004u:
                Arena.Bounds = new ArenaBoundsSquare(20f);
                break;
        }
    }

    AOEShapeCustom GetRiverAOE(WPos center)
    {
        return new AOEShapeCustom(center, bossmod.GetCombinedRiver());
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoe.Length != 0)
        {
            return;
        }
        switch (spell.Action.ID)
        {
            case (uint)AID.TasteOfFire:
                AddAOE(shape ??= GetRiverAOE(Arena.Center), Colors.SafeFromAOE, true);
                _risky = false;
                break;
            case (uint)AID.TasteOfThunder:
                AddAOE(shape ??= GetRiverAOE(Arena.Center));
                break;
        }

        void AddAOE(AOEShapeCustom shape, uint color = default, bool invert = false)
        {
            var center = Arena.Center;
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 4.2d), color, shapeDistance: !invert ? shape.Distance(center, default) : shape.InvertedDistance(center, default))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TasteOfFire or (uint)AID.TasteOfThunder)
        {
            _aoe = [];
            _risky = true;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!active || _aoe.Length == 0)
        {
            return;
        }

        ref var aoe = ref _aoe[0];
        var isInside = aoe.Check(actor.Position);
        if (!_risky)
        {
            hints.Add("Be inside river!", !isInside);
            return;
        }
        if (isInside)
        {
            hints.Add("GTFO from river!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (!active)
        {
            return;
        }
        var pos = actor.Position;
        if (actor.PrevPosition != pos)
        {
            hints.WantJump = bossmod.IntersectJumpEdge(pos, (pos - actor.PrevPosition).Normalized(), 1f);
        }
    }
}
