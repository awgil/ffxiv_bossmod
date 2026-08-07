namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE212ManyMouthstoFeed;

public enum OID : uint
{
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    PelekysHelper = 0x233C, // R0.500, x40, Helper type
    Pelekys = 0x4BCA, // R7.000, x1
    Pelekys1 = 0x4BCC, // R0.500, x1
    Venom = 0x1EBFED, // R0.500, x4, EventObj type
    Actor1ec007 = 0x1EC007, // R0.500, x1, EventObj type
    UnknownActor = 0x4BCD, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    UnknownWeaponskill = 47214, // Pelekys1->self, no cast, range ?-30 donut
    AutoAttack = 50850, // Pelekys->player, no cast, single-target
    AcridRain1 = 47231, // Pelekys->self, 5.0s cast, single-target
    AcridRain2 = 47232, // PelekysHelper->self, no cast, ???
    CentralGardening1 = 47218, // Pelekys->self, 5.0s cast, single-target
    CentralGardening2 = 47220, // PelekysHelper->self, 6.0s cast, range 52 width 10 rect
    SideGardening1 = 47219, // Pelekys->self, 5.0s cast, single-target
    SideGardening2 = 49729, // PelekysHelper->self, 6.0s cast, range 26 180.000-degree cone
    SideGardening3 = 47221, // PelekysHelper->self, 6.0s cast, range 26 180.000-degree cone
    NoxiousNectar = 49730, // Pelekys->self, 3.0s cast, single-target
    NoxiousNectar1 = 49885, // Pelekys->self, no cast, single-target
    Venom = 47216, // PelekysHelper->self, 4.8s cast, range 2 circle
    Venom1 = 47217, // PelekysHelper->self, no cast, range 2 circle
    NoxiousNectar2 = 47215, // Pelekys->self, no cast, single-target
    PollenLure = 47222, // Pelekys->self, 4.0s cast, single-target
    Devour = 47223, // Pelekys->self, 7.0s cast, range 10 circle
    PoisonHeart1 = 47229, // Pelekys->self, 4.0s cast, single-target
    PoisonHeart2 = 47230, // PelekysHelper->location, 3.0s cast, range 5 circle
    VenomMist1 = 47225, // Pelekys->self, 5.0s cast, single-target
    VenomMist2 = 50548, // PelekysHelper->self, 6.0s cast, range 30 90.000-degree cone
    VenomMist3 = 50547, // PelekysHelper->self, 6.0s cast, range 30 90.000-degree cone
    VenomMist4 = 50549, // PelekysHelper->self, 6.0s cast, range 30 90.000-degree cone
    VenomMist5 = 47227, // Pelekys->self, 5.0s cast, single-target
    VenomMist6 = 47228, // PelekysHelper->self, 6.0s cast, range 30 90.000-degree cone
}

public enum SID : uint
{
    Toxicosis = 4379, // PelekysHelper->player, extra=0x0
    VulnerabilityUp = 2347, // PelekysHelper/Pelekys->player, extra=0x1/0x2/0x3/0x4/0x5/0x6/0x7/0x8/0x9/0xA
    UnknownStatus1 = 2552, // none->Pelekys, extra=0x3F2/0x3F3
    Poison = 5425, // PelekysHelper->player, extra=0x0
    UnknownStatus2 = 2056, // none->UnknownActor, extra=0x3C2
    QuickerStep = 4799, // none->player, extra=0x0
}

sealed class AcridRain(BossModule module) : Components.RaidwideCast(module, (uint)AID.AcridRain1, "Raidwide + poison");
sealed class CentralGardening(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CentralGardening2, new AOEShapeRect(52f, 5f));
sealed class SideGardening(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.SideGardening2, (uint)AID.SideGardening3], new AOEShapeCone(26f, 90f.Degrees()));
sealed class Venom(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Venom, 2f)
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        // remove on eanim so no blip between this and growing puddle
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.Venom && state == 0x00010002)
        {
            var count = Casters.Count;
            var aoes = CollectionsMarshal.AsSpan(Casters);
            for (var i = 0; i < count; ++i)
            {
                if (aoes[i].Origin.AlmostEqual(actor.Position, 0.1f))
                {
                    Casters.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
sealed class VenomGrow(BossModule module) : Components.GenericAOEs(module)
{
    // showing max size cause AI to run straight to next safe zone
    // can AI dodge puddle? or use growing circle forbidden zones?
    private readonly List<VenomZone> _venom = [];
    private readonly float _maxRadius = 22f;
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var aoes = CreateAOEs(2f, _maxRadius);
        var count = aoes.Count;
        if (count == 0)
            return [];

        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(aoes)[..max];
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.Venom)
        {
            var position = actor.Position;
            if (state == 0x00010002)
            {
                _venom.Add(new(position));
            }
            else if (state == 0x00040008)
            {
                var count = _venom.Count;
                for (var i = 0; i < count; i++)
                {
                    var venom = _venom[i];
                    if (venom.Position.AlmostEqual(position, 0.1f))
                    {
                        venom.StartTime = WorldState.CurrentTime;
                    }
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_venom.Count != 0 && spell.Action.ID == (uint)AID.Venom1)
        {
            var position = caster.Position;
            var count = _venom.Count;
            for (var i = 0; i < count; i++)
            {
                var venom = _venom[i];
                if (venom.Position.AlmostEqual(position, 0.1f))
                {
                    venom.NumCasts++;
                    if (venom.NumCasts == 8)
                    {
                        _venom.RemoveAt(i);
                        return;
                    }
                }
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // small buffer to avoid growing puddle
        var aoes = CreateAOEs(4f, _maxRadius);
        var count = aoes.Count;
        if (count != 0)
        {
            for (var i = 0; i < count; i++)
            {
                ref var aoe = ref aoes.Ref(i);
                hints.AddForbiddenZone(aoe.Shape, aoe.Origin);
            }
        }
    }

    private List<AOEInstance> CreateAOEs(float min, float max)
    {
        var aoes = new List<AOEInstance>();
        var venoms = CollectionsMarshal.AsSpan(_venom);
        var count = venoms.Length;

        for (var i = 0; i < count; i++)
        {
            ref var venom = ref venoms[i];
            var position = venom.Position;
            var startTime = venom.StartTime;
            var radius = startTime == default ? min : min + (float)((WorldState.CurrentTime - startTime).TotalMilliseconds / 400);
            radius = radius > max ? max : radius;
            aoes.Add(new(new AOEShapeCircle(radius), position));
        }

        return aoes;
    }

    private class VenomZone(WPos position)
    {
        public WPos Position = position;
        public DateTime StartTime;
        public int NumCasts;
    }
}
sealed class Devour(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Devour, 10f);
sealed class PoisonHeart(BossModule module) : Components.SimpleAOEs(module, (uint)AID.PoisonHeart2, 5f);
sealed class VenomMist(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.VenomMist2, (uint)AID.VenomMist3, (uint)AID.VenomMist4, (uint)AID.VenomMist6], new AOEShapeCone(30f, 45f.Degrees()));

[SkipLocalsInit]
sealed class CE212ManyMouthstoFeedStates : StateMachineBuilder
{
    public CE212ManyMouthstoFeedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AcridRain>()
            .ActivateOnEnter<CentralGardening>()
            .ActivateOnEnter<SideGardening>()
            .ActivateOnEnter<Venom>()
            .ActivateOnEnter<VenomGrow>()
            .ActivateOnEnter<Devour>()
            .ActivateOnEnter<PoisonHeart>()
            .ActivateOnEnter<VenomMist>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(CE212ManyMouthstoFeedStates),
    ConfigType = null, // replace null with typeof(ManyMouthstoFeedConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Pelekys,
    Contributors = "gynorhino",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14747u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE212ManyMouthstoFeed(WorldState ws, Actor primary) : BossModule(ws, primary, new(-870f, -560f), new ArenaBoundsCircle(25f))
{
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 30f);
}
