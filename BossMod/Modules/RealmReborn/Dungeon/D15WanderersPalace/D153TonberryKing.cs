using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.RealmReborn.Dungeon.D15WanderersPalace.D153TonberryKing
{
    public enum OID : uint
    {
        Boss = 0x374, // x1
        Tonberry = 0x3A3, // spawn during fight
        TonberrySlasher = 0x8D5, // spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 871, // Boss/Tonberry/TonberrySlasher->player, no cast, single-target
        LateralSlash = 419, // Boss->player, no cast, single-target, tankbuster
        Whetstone = 420, // Boss->self, no cast, single-target, buffs next tankbuster
        SharpenedKnife = 945, // Boss->player, no cast, single-target, buffed tankbuster
        ScourgeOfNym = 1392, // Boss/Tonberry->location, no cast, range 5 circle - cast when no one is in melee range
        ThroatStab = 948, // Tonberry/TonberrySlasher->player, no cast, single-target
        EveryonesGrudge = 947, // Boss->player, 3.0s cast, single-target, buffed by rancor stacks
        RancorRelease = 949, // Tonberry->Boss, 1.0s cast, single-target, gives boss rancor stack on death
    };

    internal class D153TonberryKingStates : StateMachineBuilder
    {
        public D153TonberryKingStates(BossModule module) : base(module)
        {
            TrivialPhase();
        }
    }

    public class D153TonberryKing : BossModule
    {
        public D153TonberryKing(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(73, -435), 30)) { }

        public override void CalculateAIHints(int slot, Actor actor, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, hints);
            hints.AssignPotentialTargetPriorities(a => (OID)a.OID switch
            {
                OID.Boss => 1,
                OID.Tonberry => -1,
                _ => 0
            });
        }
    }
}
