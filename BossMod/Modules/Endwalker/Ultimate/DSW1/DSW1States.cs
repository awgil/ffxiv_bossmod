using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Ultimate.DSW1
{
    class DSW1States : StateMachineBuilder
    {
        private DSW1 _module;

        public DSW1States(DSW1 module) : base(module)
        {
            _module = module;

            HoliestOfHoly(0x00000000, 5.2f);
            Heavensblaze(0x00010000, 8.1f);
            HyperdimensionalSlash(0x00020000, 10.4f);
            ShiningBlade(0x00030000, 3.9f);
            HoliestHallowing(0x00040000, 2.8f);
            Heavensflame(0x00050000, 4.9f);
            HoliestHallowing(0x00060000, 1.7f);
            EmptyFullDimension(0x00070000, 4.1f);
            HoliestHallowing(0x00080000, 5.2f);
            HoliestOfHoly(0x00090000, 5.1f);

            ActorCast(0x000A0000, _module.SerAdelphel, AID.BrightbladesSteel, 2.2f, 3, "Enrage");
        }

        private State HoliestOfHoly(uint id, float delay)
        {
            return ActorCast(id, _module.SerAdelphel, AID.HoliestOfHoly, delay, 4, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Heavensblaze(uint id, float delay)
        {
            ActorCast(id, _module.SerGrinnaux, AID.EmptyDimension, delay, 5, "Donut")
                .ActivateOnEnter<EmptyDimension>()
                .ActivateOnEnter<Heavensblaze>()
                .DeactivateOnExit<EmptyDimension>();
            ActorCast(id + 0x10, _module.SerCharibert, AID.Heavensblaze, 0, 5, "Tankbuster + Stack")
                .DeactivateOnExit<Heavensblaze>();
        }

        private void HyperdimensionalSlash(uint id, float delay)
        {
            ActorCastStart(id, _module.SerGrinnaux, AID.HyperdimensionalSlash, delay)
                .ActivateOnEnter<HyperdimensionalSlash>(); // icons appear just before cast start
            ActorCastEnd(id + 1, _module.SerGrinnaux, 5);
            ComponentCondition<HyperdimensionalSlash>(id + 2, 8.2f, comp => comp.NumCasts >= 2, "Slash")
                .DeactivateOnExit<HyperdimensionalSlash>();
        }

        private void ShiningBlade(uint id, float delay)
        {
            ActorCast(id, _module.SerGrinnaux, AID.FaithUnmoving, delay, 4, "Knockback")
                .ActivateOnEnter<ShiningBlade>();
            ActorCastEnd(id + 2, _module.SerAdelphel, 1, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide); // holiest-of-holy overlap
            ComponentCondition<ShiningBlade>(id + 0x10, 8.5f, comp => comp.Done, "Resolve")
                .DeactivateOnExit<ShiningBlade>();
        }

        private void HoliestHallowing(uint id, float delay)
        {
            ActorCastStart(id, _module.SerAdelphel, AID.HoliestHallowing, delay);
            Timeout(id + 1, 4, "Heal") // note: we use timeout instead of cast-end, since cast-end happens whenever anyone presses interrupt...
                .ActivateOnEnter<HoliestHallowing>()
                .DeactivateOnExit<HoliestHallowing>();
        }

        private void Heavensflame(uint id, float delay)
        {
            ActorCastStart(id, _module.SerCharibert, AID.Heavensflame, delay)
                .ActivateOnEnter<Heavensflame>(); // icons appear just before cast start
            ActorCastEnd(id + 1, _module.SerCharibert, 7);
            ComponentCondition<Heavensflame>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Heavensflame")
                .DeactivateOnExit<Heavensflame>();
        }

        private void EmptyFullDimension(uint id, float delay)
        {
            HoliestOfHoly(id, delay)
                .ActivateOnEnter<EmptyDimension>()
                .ActivateOnEnter<FullDimension>();
            ActorCastEnd(id + 0x10, _module.SerGrinnaux, 2, "Donut/circle") // holiest-of-holy overlap
                .DeactivateOnExit<EmptyDimension>()
                .DeactivateOnExit<FullDimension>();
        }
    }
}
