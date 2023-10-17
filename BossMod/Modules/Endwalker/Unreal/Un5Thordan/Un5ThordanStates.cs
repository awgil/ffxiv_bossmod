using System;
using System.Linq;
using Lumina.Excel.GeneratedSheets;

namespace BossMod.Endwalker.Unreal.Un5Thordan;

class Un5ThordanStates : StateMachineBuilder
{
    public Un5ThordanStates(Un5Thordan module) : base(module)
    {
        SimplePhase(0, Phase1, "Phase 1")
            .ActivateOnEnter<AscalonsMight>()
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
        SimplePhase(1, PhaseIntermission, "Intermission")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.IsTargetable;
    }
    
    private void Phase1(uint id)
    {
        //SimpleState(id, 81.5f, "Disappear");
        //ActorCast(id , () => Module.PrimaryActor, AID.Meteorain, 0, 3, true, "Puddles bait");
        ComponentCondition<Meteorain>(id, 13.25f, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<Meteorain>();
        ComponentCondition<Meteorain>(id + 0x10, 3f, comp => comp.Casters.Count == 0, "Puddles resolve")
            .DeactivateOnExit<Meteorain>();
        Cast(id + 0x20, AID.AscalonsMercy, 2f, 3f,  "Ascalon's Mercy")
            .ActivateOnEnter<AscalonsMercy>()
            .ActivateOnEnter<AscalonsMercyHelper>()
            .DeactivateOnExit<AscalonsMercy>()
            .DeactivateOnExit<AscalonsMercyHelper>();
        Cast(id + 0x30, AID.DragonsEye, 4.35f, 3f)
            .ActivateOnEnter<DragonsGaze>();
        Cast(id + 0x40, AID.DragonsGaze, 7.18f, 3f, "Gaze")
            .DeactivateOnExit<DragonsGaze>();
        ComponentCondition<LightningStorm>(id + 0x50, 10.15f, comp => comp.Active)
            .ActivateOnEnter<LightningStorm>();
        ComponentCondition<LightningStorm>(id + 0x60, 3.6f, comp => !comp.Active, "Spread")
            .DeactivateOnExit<LightningStorm>();
        SimpleState(id + 0x70, 28.7f, "Disappear");
        /*Cast(id + 0x20, AID.DragonsEye, 0, 0, "");
        Cast(id + 0x30, AID.DragonsGaze, 14.5f, 2.7f, "Gaze")
            .ActivateOnEnter<DragonsGaze>();*/
        /*ComponentCondition<DragonsGaze>(id + 0x20, 14.5f + 2.7f, comp => comp.NumCasts > 0,  "gaze")
            .ActivateOnEnter<DragonsGaze>()
            .DeactivateOnExit<DragonsGaze>();*/

    }
    
    private void PhaseIntermission(uint id)
    {
        ComponentCondition<Heavensflame>(id, 14.2f, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<Heavensflame>()
            .ActivateOnEnter<BurningChains>()
            .ActivateOnEnter<Conviction>();
        ComponentCondition<Heavensflame>(id + 0x20, 6f, comp => comp.Casters.Count == 0, "Puddles resolve")
            .DeactivateOnExit<Heavensflame>();
        ComponentCondition<Conviction>(id + 0x30, 3f, comp => comp.Towers.Count == 0, "Towers resolve")
            .DeactivateOnExit<Conviction>();
        ComponentCondition<SerZephirin>(id + 0x10000, 3f, comp => comp.ActiveActors.Any(), "Ser Zephirin")
            .ActivateOnEnter<SerZephirin>();
        ComponentCondition<SerZephirin>(id + 0x10019, 20f, comp => !comp.ActiveActors.Any(), "DPS check")
            .DeactivateOnExit<SerZephirin>();
        ComponentCondition<SpiralThrust>(id + 0x10020, 5.7f, comp => comp.Casters.Count == 0, "Spiral Thrust", checkDelay: 1f)
            .ActivateOnEnter<SpiralThrust>()
            .DeactivateOnExit<SpiralThrust>()
            .DeactivateOnExit<BurningChains>();
        ComponentCondition<SerAdelphel>(id + 0x20000, 7f, comp => comp.ActiveActors.Any(), "Adds spawn", checkDelay: 1f)
            .ActivateOnEnter<SerAdelphel>()
            .ActivateOnEnter<SerJanlenoux>();

        Func<Actor?> eitherAdd = () =>
            Module.Enemies(OID.Adelphel).FirstOrDefault(Module.Enemies(OID.Janlenoux).FirstOrDefault());
        ActorCast(id + 0x20001, eitherAdd, AID.DivineRight, 7f, 3f,
                false, "Tank Split start")
            .ActivateOnEnter<SwordShieldOfTheHeavens>();
        ActorCast(id + 0x20020, eitherAdd, AID.HolyBladedance, 4.5f, 4f,
                false, "Tankbuster");
        ComponentCondition<SkywardLeap>(id + 0x20030, 9.5f, comp => comp.Active, "Skyward Leap")
            .ActivateOnEnter<SkywardLeap>();
        ComponentCondition<SkywardLeap>(id + 0x20040, 12f, comp => !comp.Active, "Last Skyward Leap")
            .DeactivateOnExit<SkywardLeap>();
        ActorCast(id + 0x20050, eitherAdd, AID.HoliestOfHoly, 9f, 3f,
            false, "Raidwide");
        //TODO: find and add pattern
        SimpleState(id + 0x20060, 1000.0f, "")
            .DeactivateOnExit<SerAdelphel>()
            .DeactivateOnExit<SerJanlenoux>()
            .DeactivateOnExit<SwordShieldOfTheHeavens>()
            .Raw.Update = (_) => eitherAdd() == null ? 0 : -1;
        
        ComponentCondition<SpiralPierce>(id + 0x30000, 11f, comp => comp.NumCasts > 0, "Spiral Pierce")
            .ActivateOnEnter<HiemalStorm>()
            .ActivateOnEnter<SpiralPierce>();

        SimpleState(id + 0x30010, 6f, "Knockback")
            .ActivateOnEnter<FaithUnmoving>()
            .DeactivateOnExit<FaithUnmoving>()
            .Raw.Update = (x) => x > 6f ? 0 : -1;
        /*ActorCast(id + 0x30010, () => Module.Enemies(OID.Grinnaux).FirstOrDefault(), AID.FaithUnmoving, 8f, 1f, false,
                "Knockback")
            .ActivateOnEnter<FaithUnmoving>()
            .DeactivateOnExit<FaithUnmoving>();*/
        
        
        SimpleState(id + 0x30020, 1000.0f, "");
    }
}
