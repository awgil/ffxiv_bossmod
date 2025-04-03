
namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

class DeepCut(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(60, 30.Degrees()), 471, ActionID.MakeSpell(AID._Weaponskill_DeepCut1));
class DiscoInferno(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_DiscoInfernal));
class CelebrateGoodTimes(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID._Weaponskill_CelebrateGoodTimes));

class GetDown(BossModule module) : Components.StandardAOEs(module, ActionID.MakeSpell(AID._Weaponskill_GetDown), new AOEShapeCircle(7));

class GetDownProtean(BossModule module) : Components.GenericProtean(module, ActionID.MakeSpell(AID._Weaponskill_GetDown1), new AOEShapeCone(40, 22.5f.Degrees()))
{
    private BitMask targetedPlayers;
    private Role3 lastTarget;

    public override IEnumerable<(Actor source, Actor target)> ActiveAOEs()
    {
        foreach (var (_, tar) in Raid.WithSlot().ExcludedFromMask(targetedPlayers).Where(r => r.Item2.Class.GetRole3() != lastTarget))
            yield return (Module.PrimaryActor, tar);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            var closest = Raid.WithoutSlot().MinBy(c => (Module.PrimaryActor.AngleTo(c) - spell.Rotation).Abs().Rad);
            if (closest != null)
            {
                targetedPlayers.Set(Raid.FindSlot(closest.InstanceID));
                lastTarget = closest.Class.GetRole3();
            }
        }
    }
}

class DancingGreenStates : StateMachineBuilder
{
    public DancingGreenStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<ABSide>()
            .ActivateOnEnter<CelebrateGoodTimes>()
            .ActivateOnEnter<DiscoInferno>();
    }

    private void SinglePhase(uint id)
    {
        DeepCut(id, 9.2f);

        id += 0x10000;
        Flip(id, 7.7f);
        TwistNDrop(id + 0x10, 2.25f);

        id += 0x10000;
        Flip(id, 2.9f);
        TwistNDrop(id + 0x10, 2.2f);
        Cast(id + 0x20, AID._Weaponskill_CelebrateGoodTimes, 0.9f, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);

        id += 0x10000;
        DiscoPhase(id, 8.4f);

        id += 0x10000;
        Cast(id, AID._Weaponskill_EnsembleAssemble, 8.6f, 3)
            .ActivateOnEnter<GetDown>()
            .ActivateOnEnter<GetDownProtean>();
        Cast(id + 0x10, AID._Weaponskill_ArcadyNightFever, 3.1f, 4.8f);
        ComponentCondition<GetDown>(id + 0x12, 0.5f, g => g.NumCasts > 0, "Proteans 1");

        SimpleState(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<Wavelength>();
    }

    private void DeepCut(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_DeepCut, delay)
            .ActivateOnEnter<DeepCut>();
        CastEnd(id + 2, 5);

        ComponentCondition<DeepCut>(id + 4, 0.7f, d => d.NumCasts >= 2, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<DeepCut>();
    }

    private void Flip(uint id, float delay)
    {
        CastMulti(id, [AID._Weaponskill_FlipToASide, AID._Weaponskill_FlipToBSide], delay, 4);
    }

    private void TwistNDrop(uint id, float delay)
    {
        CastStartMulti(id, RM05SDancingGreen.TwistNDrop.BossCasts, delay)
            .ActivateOnEnter<TwistNDrop>()
            .ActivateOnEnter<PlayASide>()
            .ActivateOnEnter<PlayBSide>()
            .ActivateOnEnter<PlaySideCounter>();

        CastEnd(id + 0x02, 5);

        ComponentCondition<TwistNDrop>(id + 0x20, 3.5f, t => t.Side2, "Left/right");
        ComponentCondition<PlaySideCounter>(id + 0x22, 1.8f, p => p.NumCasts >= 2, "Stack/spread")
            .DeactivateOnExit<PlaySideCounter>()
            .DeactivateOnExit<PlayASide>()
            .DeactivateOnExit<PlayBSide>()
            .DeactivateOnExit<TwistNDrop>();
    }

    private void DiscoPhase(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_DiscoInfernal, delay, 4, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .ActivateOnEnter<FloorCounter>()
            .ActivateOnEnter<InsideOutside>()
            .ActivateOnEnter<FunkyFloor>()
            .ActivateOnEnter<BurnBabyBurn>();

        id += 0x100;

        CastStart(id, AID._Weaponskill_FunkyFloor, 5);
        ComponentCondition<FloorCounter>(id + 0x10, 3.1f, c => c.NumCasts > 0, "Floor start");

        CastMulti(id + 0x20, [AID._Weaponskill_InsideOut1, AID._Weaponskill_OutsideIn1], 3.1f, 5, "In/out");
        ComponentCondition<InsideOutside>(id + 0x22, 2.5f, i => i.NumCasts >= 2, "Out/in");

        ComponentCondition<BurnBabyBurn>(id + 0x30, 8.3f, b => b.NumShort == 0, "Debuffs 1");
        ComponentCondition<BurnBabyBurn>(id + 0x40, 7.9f, b => b.NumLong == 0, "Debuffs 2");

        TwistNDrop(id + 0x100, 2.8f);
        Cast(id + 0x200, AID._Weaponskill_CelebrateGoodTimes, 1, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<BurnBabyBurn>()
            .DeactivateOnExit<FunkyFloor>()
            .DeactivateOnExit<InsideOutside>()
            .DeactivateOnExit<FloorCounter>();

        DeepCut(id + 0x300, 2.1f);
    }

    //private void XXX(uint id, float delay)
}
