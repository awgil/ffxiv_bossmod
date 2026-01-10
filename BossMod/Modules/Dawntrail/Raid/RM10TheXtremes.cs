namespace BossMod.Dawntrail.Raid.RM10TheXtremes;

public enum OID : uint
{
    Boss = 0x4B53, // R4.000, x? (RedHot)
    DeepBlue = 0x4B54, // R4.000, x?  
    TheXtremes = 0x4BDE, // R1.000, x?
    XtremeAether = 0x4B55, // R1.500, x?
    Helper = 0x233C, // R0.500, x?, Helper type
    SickSwell = 0x4B56, // R1.000, x?

    InfernoVoidzone = 0x1EBF31,
    HotAerialVoidzone = 0x1EBf32,
    CutbackBlazeVoidzone = 0x1EBF34,
    BlastingSnapVoidzone = 0x1EBF35,
}

public enum AID : uint
{
    AutoAttack = 48637, // 4B53->player, no cast, single-target
    AutoAttack1 = 48638, // 4B54->player, no cast, single-target

    HotImpact = 46464, // 4B53->players, 5.0s cast, range 6 circle SharedTankbuster
    AlleyOopInferno = 46470, // 4B53->self, 4.3+0.7s cast, single-target
    AlleyOopInferno1 = 46471, // 233C->player, 5.0s cast, range 5 circle
    CutbackBlaze = 46478, // 4B53->self, 4.3+0.7s cast, single-target
    CutbackBlaze1 = 46479, // 233C->self, no cast, range 60 ?-degree cone
    DiversDare = 46466, // 4B53->self, 5.0s cast, range 60 circle

    SickSwell = 46480, // 4B54->self, 3.0s cast, single-target
    SickSwell1 = 46481, // 233C->self, 7.0s cast, range 50 width 50 rect
    SickestTakeOff = 46482, // 4B54->self, 4.0s cast, single-target
    SickestTakeOff1 = 46483, // 233C->self, 7.0s cast, range 50 width 15 rect

    DeepVarial = 47247, // 4B54->location, 5.3+1.0s cast, ???
    DeepVarial1 = 46488, // 233C->self, 6.8s cast, range 60 120-degree cone
    DeepImpact = 46465, // 4B54->player, 5.0s cast, range 6 circle

    XtremeSpectacular = 46498, // 4B54->self, 4.0s cast, single-target
    XtremeSpectacular1 = 46497, // 4B53->self, 4.0s cast, single-target
    XtremeSpectacular2 = 46499, // 4BDE->self, 7.4s cast, range 50 width 40 rect
    XtremeSpectacular3 = 46555, // 4BDE->self, no cast, range 60 circle
    XtremeSpectacular4 = 47049, // 4BDE->self, no cast, range 60 circle

    EpicBrotherhood = 46459, // 4B54->4B53, no cast, single-target
    EpicBrotherhood1 = 46458, // 4B53->4B54, no cast, single-target

    HotAerial = 46474, // 4B53->self, 4.9s cast, single-target
    HotAerial1 = 46475, // 4B53->location, no cast, single-target
    HotAerial2 = 46476, // 233C->self, 6.0s cast, range 6 circle

    SteamBurst = 46507, // 4B55->self, 3.0s cast, range 9 circle

    Pyrotation = 46472, // 4B53->self, 4.3+0.7s cast, single-target
    Pyrotation1 = 46473, // 233C->players, no cast, range 6 circle
    AlleyOopMaelstrom = 46494, // 4B54->self, 3.0s cast, single-target
    AlleyOopMaelstrom1 = 46495, // 233C->self, 3.0s cast, range 60 30-degree cone
    AlleyOopMaelstrom2 = 46496, // 233C->self, 3.0s cast, range 60 15-degree cone
    DiversDare1 = 46467, // 4B54->self, 5.0s cast, range 60 circle

    Watersnaking = 46463, // 4B54->self, 5.0s cast, range 60 circle
    Firesnaking = 46462, // 4B53->self, 5.0s cast, range 60 circle
    InsaneAir = 47252, // 4B54->self, 5.9+1.5s cast, single-target
    InsaneAir1 = 47251, // 4B53->self, 5.9+1.5s cast, single-target
    InsaneAir2 = 47254, // 4B54->self, 3.9+1.5s cast, single-target
    InsaneAir3 = 47253, // 4B53->self, 3.9+1.5s cast, single-target

    PlungingSnap = 46504, // 4B54->self, no cast, single-target
    PlungingSnap1 = 46506, // 233C->self, no cast, range 60 ?-degree cone
    BlastingSnap = 46503, // 4B53->self, no cast, single-target
    BlastingSnap1 = 46505, // 233C->self, no cast, range 60 ?-degree cone

}
public enum IconID : uint
{
    HotImpactSharedTankbuster = 259, // player->self
    AlleyOopInfernoSpread = 660, // player->self
    CutbackBlaze = 664, // Boss->player
    DeepImpact = 344, // player->self
    Pyrotation = 659, // player->self

    WaterSnakeBait1 = 661, // player->self
    FireSnakeBait1 = 662, // player->self

    WaterSnakeBait2 = 651, // a->player
    FireSnakeBait2 = 665, // a->player

    WaterSnakeBait3 = 635, // player->self
    FireSnakeBait3 = 636, // player->self
}
public enum SID : uint
{
    Watersnaking = 4975, // none->player, extra=0x0
    Firesnaking = 4974, // none->player, extra=0x0
}

class HotImpact(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.HotImpactSharedTankbuster, AID.HotImpact, 6);
class AlleyOopInferno(BossModule module) : Components.IconStackSpread(module, 0, (uint)IconID.AlleyOopInfernoSpread, null, AID.AlleyOopInferno1, 0, 5, 0);
class AlleyOopInfernoVoidzones(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.AlleyOopInferno1, module => module.Enemies(OID.InfernoVoidzone).Where(z => z.EventState != 7), 0.9f);
class CutbackBlaze(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 25.Degrees()), (uint)IconID.CutbackBlaze, AID.CutbackBlaze);
class CutbackBlazeVoidzones(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.CutbackBlaze, module => module.Enemies(OID.CutbackBlazeVoidzone).Where(z => z.EventState != 7), 0.9f)
{
    public new AOEShapeCone Shape = new(60, 25.Degrees());
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var z in Sources(Module))
            yield return new(Shape, z.Position, z.Rotation);
    }
}
class DiversDare(BossModule module) : Components.RaidwideCast(module, AID.DiversDare);
class DiversDare1(BossModule module) : Components.RaidwideCast(module, AID.DiversDare1);
class SickSwell(BossModule module) : Components.KnockbackFromCastTarget(module, AID.SickSwell1, 10, kind: Kind.DirForward);
class SickestTakeOff(BossModule module) : Components.StandardAOEs(module, AID.SickestTakeOff1, new AOEShapeRect(50f, 7.5f));
class DeepVarial(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeCone cone = new(60f, 45.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count > 0)
        {
            foreach (var e in _aoes)
            {
                yield return e with { Color = ArenaColor.AOE };
            }
        }
    }
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DeepVarial1)
        {
            _aoes.Add(new(cone, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation));

        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.DeepVarial1)
        {
            _aoes.Clear();
        }
    }
}
class DeepImpact(BossModule module) : Components.BaitAwayCast(module, AID.DeepImpact, new AOEShapeCircle(6), true);
class XtremeSpectacular(BossModule module) : Components.StandardAOEs(module, AID.XtremeSpectacular2, new AOEShapeRect(50f, 15f));
class HotAerial(BossModule module) : Components.CastTowers(module, AID.HotAerial2, 6, 1);
class HotAerialVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, AID.HotAerial1, module => module.Enemies(OID.HotAerialVoidzone).Where(z => z.EventState != 7), 0.9f);
class SteamBurst(BossModule module) : Components.StandardAOEs(module, AID.SteamBurst, 9f);
class Pyrotation(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Pyrotation, AID.Pyrotation1, 6, 0);
class AlleyOopMaelstrom1(BossModule module) : Components.StandardAOEs(module, AID.AlleyOopMaelstrom1, new AOEShapeCone(60f, 15f.Degrees()));
class AlleyOopMaelstrom2(BossModule module) : Components.StandardAOEs(module, AID.AlleyOopMaelstrom2, new AOEShapeCone(60f, 7.5f.Degrees()));

class Watersnaking(BossModule module) : Components.RaidwideCast(module, AID.Watersnaking);
class Firesnaking(BossModule module) : Components.RaidwideCast(module, AID.Firesnaking);
class InsaneAir1(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 15.Degrees()), 0)
{
    public override Actor? BaitSource(Actor target) => Module.Enemies(OID.DeepBlue).FirstOrDefault();

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (BaitSource(actor) is var source && source != null)
        {
            if (iconID is (uint)IconID.WaterSnakeBait1 or (uint)IconID.WaterSnakeBait2 or (uint)IconID.WaterSnakeBait3)
            {
                CurrentBaits.Add(new(source, WorldState.Actors.Find(targetID) ?? actor, Shape, WorldState.FutureTime(ActivationDelay)));
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID is AID.PlungingSnap or AID.PlungingSnap1)
            CurrentBaits.Clear();
    }
}
class InsaneAir2(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60f, 15.Degrees()), 0)
{
    public override Actor? BaitSource(Actor target) => Module.PrimaryActor;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (BaitSource(actor) is var source && source != null)
        {
            if (iconID is (uint)IconID.FireSnakeBait1 or (uint)IconID.FireSnakeBait2 or (uint)IconID.FireSnakeBait3)
            {
                CurrentBaits.Add(new(source, WorldState.Actors.Find(targetID) ?? actor, Shape, WorldState.FutureTime(ActivationDelay)));
            }
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID is AID.BlastingSnap or AID.BlastingSnap1)
            CurrentBaits.Clear();
    }
}
class BlastingSnapVoidzones(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.BlastingSnap, module => module.Enemies(OID.BlastingSnapVoidzone).Where(z => z.EventState != 7), 0.9f)
{
    public new AOEShapeCone Shape = new(60, 15.Degrees());
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var z in Sources(Module))
            yield return new(Shape, z.Position, z.Rotation);
    }
}

class RM10TheXtremesStates : StateMachineBuilder
{
    public RM10TheXtremesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HotImpact>()
            .ActivateOnEnter<AlleyOopInferno>()
            .ActivateOnEnter<AlleyOopInfernoVoidzones>()
            .ActivateOnEnter<CutbackBlaze>()
            .ActivateOnEnter<CutbackBlazeVoidzones>()
            .ActivateOnEnter<DiversDare>()
            .ActivateOnEnter<DiversDare1>()
            .ActivateOnEnter<SickSwell>()
            .ActivateOnEnter<SickestTakeOff>()
            .ActivateOnEnter<DeepVarial>()
            .ActivateOnEnter<DeepImpact>()
            .ActivateOnEnter<XtremeSpectacular>()
            .ActivateOnEnter<HotAerial>()
            .ActivateOnEnter<HotAerialVoidzone>()
            .ActivateOnEnter<SteamBurst>()
            .ActivateOnEnter<Pyrotation>()
            .ActivateOnEnter<AlleyOopMaelstrom1>()
            .ActivateOnEnter<AlleyOopMaelstrom2>()
            .ActivateOnEnter<Watersnaking>()
            .ActivateOnEnter<Firesnaking>()
            .ActivateOnEnter<InsaneAir1>()
            .ActivateOnEnter<InsaneAir2>()
            .ActivateOnEnter<BlastingSnapVoidzones>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1070, NameID = 14470)]
public class RM10TheXtremes(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 20))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.DeepBlue), ArenaColor.Enemy);
    }
}
