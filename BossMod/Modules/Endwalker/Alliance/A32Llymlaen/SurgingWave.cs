namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class SurgingWaveCorridor(BossModule module) : BossComponent(module)
{
    public WDir CorridorDir;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x49 && state is 0x02000001 or 0x00200001 or 0x00800040 or 0x08000400)
        {
            CorridorDir = state switch
            {
                0x00800040 => new(-1, 0),
                0x08000400 => new(+1, 0),
                _ => default
            };
        }
    }
}

class SurgingWaveAOE(BossModule module) : Components.StandardAOEs(module, AID.SurgingWaveAOE, new AOEShapeCircle(6));
class SurgingWaveShockwave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.SurgingWaveShockwave, 68, true);
class SurgingWaveSeaFoam(BossModule module) : Components.PersistentVoidzone(module, 1.5f, m => m.Enemies(OID.SeaFoam).Where(x => !x.IsDead));

class SurgingWaveFrothingSea(BossModule module) : Components.GenericAOEs(module, AID.SurgingWaveFrothingSea)
{
    private readonly SurgingWaveCorridor? _corridor = module.FindComponent<SurgingWaveCorridor>();

    // TODO: not sure about how exactly it is implemented in game - each repeated aoe has the same origin
    private const float _advance = 4.5f;
    private const float _length = 6;
    private static readonly AOEShapeRect _shape = new(80, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_corridor != null)
        {
            var offset = A32Llymlaen.CorridorHalfLength * 2 - _length - _advance * NumCasts;
            yield return new(_shape, A32Llymlaen.DefaultCenter + _corridor.CorridorDir * offset, Angle.FromDirection(_corridor.CorridorDir));
        }
    }
}

class LeftStrait(BossModule module) : Components.StandardAOEs(module, AID.LeftStrait, new AOEShapeCone(100, 90.Degrees()));
class RightStrait(BossModule module) : Components.StandardAOEs(module, AID.RightStrait, new AOEShapeCone(100, 90.Degrees()));
class ToTheLast(BossModule module) : Components.StandardAOEs(module, AID.ToTheLastAOE, new AOEShapeRect(80, 5), 1);
