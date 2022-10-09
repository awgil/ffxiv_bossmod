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

    public enum SID : uint
    {
        Rancor = 351, // Tonberry->Boss, extra=num stacks
    };

    class D153TonberryKingStates : StateMachineBuilder
    {
        public D153TonberryKingStates(BossModule module) : base(module)
        {
            TrivialPhase();
        }
    }

    public class D153TonberryKing : BossModule
    {
        public D153TonberryKing(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(73, -435), 30)) { }

        public override void CalculateAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
        {
            base.CalculateAIHints(slot, actor, assignment, hints);
            //var rancorStacks = PrimaryActor.FindStatus(SID.Rancor)?.Extra ?? 0;
            hints.AssignPotentialTargetPriorities(a => (OID)a.OID switch
            {
                //OID.Tonberry => a.HP.Cur > 1 && rancorStacks < 3 ? 0 : -1, // note: we don't bother with them - let tank aoe them when they reach center and invuln high stacks...
                OID.Boss => 1,
                _ => 0
            });
        }
    }
}
