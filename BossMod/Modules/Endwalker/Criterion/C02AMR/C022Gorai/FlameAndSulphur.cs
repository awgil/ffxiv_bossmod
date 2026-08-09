namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

sealed class FlameAndSulphur(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(14)];
    private readonly AOEShapeRect _shapeFlameExpand = new(46f, 5f);
    private readonly AOEShapeRect _shapeFlameSplit = new(46f, 2.5f);
    private readonly AOEShapeCircle _shapeRockExpand = new(11f);
    private readonly AOEShapeDonut _shapeRockSplit = new(5f, 16f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var activation = Module.CastFinishAt(spell, 3.1f);

        switch (spell.Action.ID)
        {
            case (uint)AID.BrazenBalladSplitting:
                AddAOEs(_shapeFlameSplit, (uint)OID.FlameAndSulphurFlame, true);
                AddAOEs(_shapeRockSplit, (uint)OID.FlameAndSulphurRock);
                break;
            case (uint)AID.BrazenBalladExpanding:
                AddAOEs(_shapeFlameExpand, (uint)OID.FlameAndSulphurFlame);
                AddAOEs(_shapeRockExpand, (uint)OID.FlameAndSulphurRock);
                break;
        }

        void AddAOEs(AOEShape shape, uint actors, bool twice = false)
        {
            var enemies = Module.Enemies(actors);
            var count = enemies.Count;

            if (!twice)
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    AddAOE(shape, enemy.Position, enemy.Rotation);
                }
            else
                for (var i = 0; i < count; ++i)
                {
                    var enemy = enemies[i];
                    var rot = enemy.Rotation;
                    var offset = rot.ToDirection().OrthoL() * 7.5f;
                    AddAOE(shape, enemy.Position + offset, rot);
                    AddAOE(shape, enemy.Position - offset, rot);
                }
            void AddAOE(AOEShape shape, WPos origin, Angle rotation) => _aoes.Add(new(shape, origin.Quantized(), rotation, activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NFireSpreadExpand:
            case (uint)AID.NFireSpreadSplit:
            case (uint)AID.NFallingRockExpand:
            case (uint)AID.NFallingRockSplit:
            case (uint)AID.SFireSpreadExpand:
            case (uint)AID.SFireSpreadSplit:
            case (uint)AID.SFallingRockExpand:
            case (uint)AID.SFallingRockSplit:
                ++NumCasts;
                _aoes.Clear();
                break;
        }
    }
}
