namespace BossMod.Endwalker.Alliance.A13Azeyma;

sealed class DancingFlame(BossModule module) : Components.GenericAOEs(module, (uint)AID.DancingFlameFirst)
{
    private readonly (WPos, Angle)[] startingRects = [(new(-750f, -766.5f), Angle.AnglesCardinals[1]), (new(-733.5f, -750f), Angle.AnglesCardinals[0]),
    (new(-766.5f, -750f), Angle.AnglesCardinals[3]), (new(-750f, -733.5f), Angle.AnglesCardinals[2])];
    public List<AOEInstance> AOEs = [with(6)];
    private static readonly AOEShapeRect _shape = new(17.5f, 17.5f, 17.5f); // 15 for diagonal 'squares' + 2.5 for central cross
    private static readonly AOEShapeRect startingRect = new(3f, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.HauteAirFlare)
        {
            AOEs.Add(new(_shape, (caster.Position + 40f * caster.Rotation.ToDirection()).Quantized(), spell.Rotation, Module.CastFinishAt(spell, 1d)));
            if (AOEs.Count == 6)
            {
                AOEs.RemoveRange(0, 4);
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x1B)
        {
            if (state == 0x00080004u)
            {
                AOEs.Clear();
            }
            else if (state == 0x00020001u)
            {
                for (var i = 0; i < 4; ++i)
                {
                    var s = startingRects[i];
                    AOEs.Add(new(startingRect, s.Item1.Quantized(), s.Item2, WorldState.FutureTime(2d)));
                }
            }
        }
    }
}
