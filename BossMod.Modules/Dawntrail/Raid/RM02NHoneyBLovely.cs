namespace BossMod.Dawntrail.Raid.RM02NHoneyBLovely;

public enum OID : uint
{
    Boss = 0x422A, // R5.004, x1
    Helper = 0x233C, // R0.500, x28, Helper type
    Groupbee = 0x422B, // R1.500, x16
    Sweetheart = 0x422C, // R1.000, x35
    PoisonCloud = 0x4230, // R1.000, x8
    TowerA = 0x1EBAA2, // R0.500, x4, EventObj type
    TowerB = 0x1EBAA3, // R0.500, x4, EventObj type
}

public enum AID : uint
{
    AutoAttack = 37319, // Boss->player, no cast, single-target
    Teleport = 37219, // Boss->location, no cast, single-target

    CallMeHoney = 37220, // Boss->self, 5.0s cast, range 60 circle, raidwide
    HoneyBFinale = 37243, // Boss->self, 5.0s cast, range 60 circle, raidwide
    HoneyBLive = 37234, // Boss->self, 2.0+6.3s cast, single-target, visual (raidwide + hearts)
    HoneyBLiveAOE = 39550, // Helper->self, no cast, range 60 circle, raidwide

    TemptingTwist1 = 37222, // Boss->self, 5.5+0.7s cast, single-target, visual (donut)
    TemptingTwist2 = 37228, // Boss->self, 5.5+0.7s cast, single-target, visual (donut)
    TemptingTwistAOE1 = 39738, // Helper->self, 6.2s cast, range 7-30 donut
    TemptingTwistAOE2 = 39740, // Helper->self, 6.2s cast, range 7-30 donut
    HoneyBeeline1 = 37221, // Boss->self, 5.5+0.7s cast, single-target, visual (line)
    HoneyBeeline2 = 37227, // Boss->self, 5.5+0.7s cast, single-target, visual (line)
    HoneyBeelineAOE1 = 39737, // Helper->self, 6.2s cast, range 60 width 14 rect
    HoneyBeelineAOE2 = 39739, // Helper->self, 6.2s cast, range 60 width 14 rect

    HoneyedBreeze = 37223, // Boss->self, 4.0+1.0s cast, single-target, visual (tankbuster cones)
    HoneyedBreezeAOE = 37224, // Helper->self, no cast, range 40 30-degree cone

    LoveMeTender = 37236, // Boss->self, 4.0s cast, single-target, visual (spreads / stacks / puddles)
    Heartsore = 37242, // Helper->player, 7.0s cast, range 6 circle spread
    Heartsick = 39821, // Helper->players, 7.0s cast, range 6 circle 4-man stack
    HeartStruck1 = 37237, // Helper->location, 3.0s cast, range 4 circle
    HeartStruck2 = 37238, // Helper->location, 4.0s cast, range 6 circle
    HeartStruck3 = 37239, // Helper->location, 5.0s cast, range 10 circle
    Fracture = 37240, // Helper->self, 8.0s cast, range 4 circle, tower
    BigBurst = 37241, // Helper->self, no cast, range 60 circle, tower fail

    Loveseeker = 39530, // Boss->self, 3.0+1.0s cast, single-target, visual (out)
    LoveseekerAOE = 39617, // Helper->self, 4.0s cast, range 10 circle
    BlowKiss = 37235, // Boss->self, 6.0s cast, range 40 120-degree cone

    SplashOfVenom = 37225, // Boss->self, 3.0s cast, single-target, visual (delayed spreads)
    SplashOfVenomAOE = 37231, // Helper->player, 7.0s cast, range 6 circle spread
    DropOfVenom = 37226, // Boss->self, 3.0s cast, single-target, visual (delayed stack)
    DropOfVenomAOE = 37232, // Helper->players, 7.0s cast, range 6 circle stack
    PoisonCloudMove = 37229, // PoisonCloud->location, no cast, single-target
    Splinter = 37230, // PoisonCloud->self, 4.0s cast, range 8 circle

    SweetheartTouch = 37244, // Sweetheart->player, no cast, single-target

    AlarmPheromones = 37245, // Boss->self, 3.0s cast, single-target, visual (bees)
    BlindingLoveBee1 = 37246, // Groupbee->self, 6.3+0.7s cast, single-target
    BlindingLoveBee2 = 37247, // Groupbee->self, 6.3+0.7s cast, single-target
    BlindingLove1 = 39525, // Helper->self, 7.0s cast, range 50 width 8 rect
    BlindingLove2 = 39526, // Helper->self, 7.0s cast, range 50 width 8 rect
}

public enum SID : uint
{
    LovelyPoison = 3915, // Boss->Boss, extra=0x31B
    Hearts0 = 3916, // none->player, extra=0x2D6
    Hearts1 = 3917, // none->player, extra=0x2D7
    Hearts2 = 3918, // none->player, extra=0x2D8
    Hearts3 = 3919, // none->player, extra=0x2D9
    FatalAttraction = 3920, // none->player, extra=0x2162, stun at 3 hearts
}

public enum IconID : uint
{
    HoneyedBreeze = 230, // player
    DropOfVenom = 317, // player
    SplashOfVenom = 375, // player
    Heartsore = 515, // player
    Heartsick = 517, // player
}

class CallMeHoney(BossModule module) : Components.RaidwideCast(module, AID.CallMeHoney);
class HoneyBFinale(BossModule module) : Components.RaidwideCast(module, AID.HoneyBFinale);
class HoneyBLive(BossModule module) : Components.RaidwideCastDelay(module, AID.HoneyBLive, AID.HoneyBLiveAOE, 6.3f);
class TemptingTwist(BossModule module) : Components.GroupedAOEs(module, [AID.TemptingTwistAOE1, AID.TemptingTwistAOE2], new AOEShapeDonut(7, 30));
class HoneyBeeline(BossModule module) : Components.GroupedAOEs(module, [AID.HoneyBeelineAOE1, AID.HoneyBeelineAOE2], new AOEShapeRect(60, 7));
class HoneyedBreeze(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40, 15.Degrees()), (uint)IconID.HoneyedBreeze, AID.HoneyedBreezeAOE, 4.9f, damageType: AIHints.PredictedDamageType.Tankbuster);
class Hearts(BossModule module) : Components.CastStackSpread(module, AID.Heartsick, AID.Heartsore, 6, 6, 4, 4, true);
class Venom(BossModule module) : Components.CastStackSpread(module, AID.DropOfVenomAOE, AID.SplashOfVenomAOE, 6, 6, 4, 8, true);
class HeartStruck1(BossModule module) : Components.StandardAOEs(module, AID.HeartStruck1, new AOEShapeCircle(4));
class HeartStruck2(BossModule module) : Components.StandardAOEs(module, AID.HeartStruck2, new AOEShapeCircle(6));
class HeartStruck3(BossModule module) : Components.StandardAOEs(module, AID.HeartStruck3, new AOEShapeCircle(10));
class Loveseeker(BossModule module) : Components.StandardAOEs(module, AID.LoveseekerAOE, new AOEShapeCircle(10));
class BlowKiss(BossModule module) : Components.StandardAOEs(module, AID.BlowKiss, new AOEShapeCone(40, 60.Degrees()));
class BlindingLove(BossModule module) : Components.GroupedAOEs(module, [AID.BlindingLove1, AID.BlindingLove2], new AOEShapeRect(50, 4), 6, true);

class Fracture(BossModule module) : Components.CastTowers(module, AID.Fracture, 4)
{
    public override void Update()
    {
        if (Towers.Count == 0)
            return;
        BitMask forbidden = default;
        foreach (var (i, p) in Raid.WithSlot(true))
            forbidden[i] = p.FindStatus(SID.Hearts2) != null || p.FindStatus(SID.Hearts3) != null;
        foreach (ref var t in Towers.AsSpan())
            t.ForbiddenSoakers = forbidden;
    }
}

class PoisonCloud(BossModule module) : Components.GenericAOEs(module, AID.Splinter)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.PoisonCloudMove)
            _aoes.Add(new(_shape, spell.TargetXZ, default, WorldState.FutureTime(6)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var index = _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            var aoe = new AOEInstance(_shape, caster.Position, default, Module.CastFinishAt(spell));
            if (index >= 0)
                _aoes[index] = aoe;
            else
                _aoes.Add(aoe);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class Sweethearts(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _hearts = [];

    private static readonly AOEShapeCircle _body = new(1.8f);
    private static readonly AOEShapeRect _path = new(3, 1.8f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var h in _hearts)
        {
            yield return new(_body, h.Position);
            yield return new(_path, h.Position, h.Rotation);
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Sweetheart && id == 0x11D3)
            _hearts.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor) => _hearts.Remove(actor);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SweetheartTouch)
            _hearts.Remove(caster);
    }
}

class RM02NHoneyBLovelyStates : StateMachineBuilder
{
    public RM02NHoneyBLovelyStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Sweethearts>();
    }

    private void SinglePhase(uint id)
    {
        CallMeHoney(id, 5.2f);
        TemptingTwist(id + 0x10000, 7.1f);
        HoneyBeeline(id + 0x20000, 3.4f);
        HoneyedBreeze(id + 0x30000, 3.5f);
        TwistOrBeeline(id + 0x40000, 4.3f);
        HoneyBLive(id + 0x50000, 8.9f);
        HeartsTowers(id + 0x60000, 5.8f);
        Loveseeker(id + 0x70000, 4.4f);
        HeartStruck(id + 0x80000, 1.2f, 4.1f);
        BlowKiss(id + 0x90000, 18.8f);
        HeartStruck(id + 0xA0000, 3.7f, 6.1f);
        BlowKiss(id + 0xB0000, 2.6f);
        HoneyBFinale(id + 0xC0000, 5.7f);
        HoneyedBreeze(id + 0xD0000, 5.2f);
        VenomBeeline(id + 0xE0000, 7.5f);
        VenomTwist(id + 0xF0000, 3.9f);
        AlarmPheromones(id + 0x100000, 9.1f);
        TwistOrBeeline(id + 0x110000, 25.2f);
        HoneyBLive(id + 0x120000, 8.6f);
        HeartsTowers(id + 0x130000, 0.8f);

        // TODO: second Love Me Tender (Heartsick stacks) never resolved in a replay; timeline past it unseen
        SimpleState(id + 0x140000, 120, "Until enrage")
            .ActivateOnEnter<CallMeHoney>()
            .ActivateOnEnter<HoneyBFinale>()
            .ActivateOnEnter<HoneyBLive>()
            .ActivateOnEnter<TemptingTwist>()
            .ActivateOnEnter<HoneyBeeline>()
            .ActivateOnEnter<HoneyedBreeze>()
            .ActivateOnEnter<Hearts>()
            .ActivateOnEnter<Venom>()
            .ActivateOnEnter<HeartStruck1>()
            .ActivateOnEnter<HeartStruck2>()
            .ActivateOnEnter<HeartStruck3>()
            .ActivateOnEnter<Fracture>()
            .ActivateOnEnter<Loveseeker>()
            .ActivateOnEnter<BlowKiss>()
            .ActivateOnEnter<PoisonCloud>();
    }

    private void CallMeHoney(uint id, float delay)
    {
        Cast(id, AID.CallMeHoney, delay, 5, "Raidwide")
            .ActivateOnEnter<CallMeHoney>()
            .DeactivateOnExit<CallMeHoney>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HoneyBFinale(uint id, float delay)
    {
        Cast(id, AID.HoneyBFinale, delay, 5, "Raidwide")
            .ActivateOnEnter<HoneyBFinale>()
            .DeactivateOnExit<HoneyBFinale>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HoneyBLive(uint id, float delay)
    {
        Cast(id, AID.HoneyBLive, delay, 2)
            .ActivateOnEnter<HoneyBLive>();
        ComponentCondition<HoneyBLive>(id + 0x10, 6.3f, c => c.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<HoneyBLive>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void TemptingTwist(uint id, float delay)
    {
        CastMulti(id, [AID.TemptingTwist1, AID.TemptingTwist2], delay, 5.5f)
            .ActivateOnEnter<TemptingTwist>();
        ComponentCondition<TemptingTwist>(id + 0x10, 0.7f, c => c.NumCasts > 0, "Donut")
            .DeactivateOnExit<TemptingTwist>();
    }

    private void HoneyBeeline(uint id, float delay)
    {
        CastMulti(id, [AID.HoneyBeeline1, AID.HoneyBeeline2], delay, 5.5f)
            .ActivateOnEnter<HoneyBeeline>();
        ComponentCondition<HoneyBeeline>(id + 0x10, 0.7f, c => c.NumCasts > 0, "Line")
            .DeactivateOnExit<HoneyBeeline>();
    }

    private void HoneyedBreeze(uint id, float delay)
    {
        CastStart(id, AID.HoneyedBreeze, delay)
            .ActivateOnEnter<HoneyedBreeze>();
        CastEnd(id + 1, 4);
        ComponentCondition<HoneyedBreeze>(id + 0x10, 0.8f, c => c.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<HoneyedBreeze>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HeartsTowers(uint id, float delay)
    {
        Cast(id, AID.LoveMeTender, delay, 4)
            .ActivateOnEnter<Hearts>()
            .ActivateOnEnter<Fracture>();
        ComponentCondition<Hearts>(id + 0x10, 8.2f, c => c.NumFinishedSpreads + c.NumFinishedStacks > 0, "Spreads/stacks")
            .DeactivateOnExit<Hearts>();
        ComponentCondition<Fracture>(id + 0x20, 9, c => c.NumCasts > 0, "Towers")
            .DeactivateOnExit<Fracture>();
    }

    private void Loveseeker(uint id, float delay)
    {
        Cast(id, AID.Loveseeker, delay, 3)
            .ActivateOnEnter<Loveseeker>();
        ComponentCondition<Loveseeker>(id + 0x10, 1, c => c.NumCasts > 0, "Out")
            .DeactivateOnExit<Loveseeker>();
    }

    private void HeartStruck(uint id, float delay, float puddleDelay)
    {
        Cast(id, AID.LoveMeTender, delay, 4)
            .ActivateOnEnter<HeartStruck1>()
            .ActivateOnEnter<HeartStruck2>()
            .ActivateOnEnter<HeartStruck3>();
        ComponentCondition<HeartStruck1>(id + 0x10, puddleDelay, c => c.NumCasts > 0 || Module.FindComponent<HeartStruck3>()?.NumCasts > 0, "Puddles");
    }

    private void BlowKiss(uint id, float delay)
    {
        Cast(id, AID.BlowKiss, delay, 6, "Cone")
            .ActivateOnEnter<BlowKiss>()
            .DeactivateOnExit<BlowKiss>()
            .DeactivateOnExit<HeartStruck1>()
            .DeactivateOnExit<HeartStruck2>()
            .DeactivateOnExit<HeartStruck3>();
    }

    private void VenomBeeline(uint id, float delay)
    {
        CastMulti(id, [AID.SplashOfVenom, AID.DropOfVenom], delay, 3)
            .ActivateOnEnter<Venom>()
            .ActivateOnEnter<PoisonCloud>();
        TwistOrBeeline(id + 0x100, 4.1f);
        ComponentCondition<PoisonCloud>(id + 0x200, 5.1f, c => c.NumCasts > 0, "Clouds");
        ComponentCondition<Venom>(id + 0x300, 5.3f, c => c.NumFinishedSpreads + c.NumFinishedStacks > 0, "Spreads/stack")
            .DeactivateOnExit<Venom>();
    }

    private void VenomTwist(uint id, float delay)
    {
        CastMulti(id, [AID.SplashOfVenom, AID.DropOfVenom], delay, 3)
            .ActivateOnEnter<Venom>();
        TwistOrBeeline(id + 0x100, 3.2f);
        ComponentCondition<Venom>(id + 0x200, 10.4f, c => c.NumFinishedSpreads + c.NumFinishedStacks > 0, "Spreads/stack")
            .DeactivateOnExit<Venom>()
            .DeactivateOnExit<PoisonCloud>();
    }

    private void TwistOrBeeline(uint id, float delay)
    {
        CastMulti(id, [AID.TemptingTwist1, AID.TemptingTwist2, AID.HoneyBeeline1, AID.HoneyBeeline2], delay, 5.5f)
            .ActivateOnEnter<TemptingTwist>()
            .ActivateOnEnter<HoneyBeeline>();
        ComponentCondition<TemptingTwist>(id + 0x10, 0.7f, c => c.NumCasts > 0 || Module.FindComponent<HoneyBeeline>()?.NumCasts > 0, "Donut / line")
            .DeactivateOnExit<TemptingTwist>()
            .DeactivateOnExit<HoneyBeeline>();
    }

    private void AlarmPheromones(uint id, float delay)
    {
        Cast(id, AID.AlarmPheromones, delay, 3)
            .ActivateOnEnter<BlindingLove>();
        ComponentCondition<BlindingLove>(id + 0x10, 9.9f, c => c.NumCasts > 0, "Bees start");
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 987, NameID = 12685)]
public class RM02NHoneyBLovely(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));
