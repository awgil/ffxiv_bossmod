// CONTRIB: made by malediktus, not checked
using System.Linq;

namespace BossMod.Endwalker.Dungeon.D11LapisManalis.D110CaladriusMaturus
{
    public enum OID : uint
    {
        Boss = 0x3D56, //R=3.96
        Caladrius = 0x3CE2, //R=1.8
    }

    public enum AID : uint
    {
        AutoAttack = 872, // Caladrius/Boss->player, no cast, single-target
        TransonicBlast = 32535, // Caladrius->self, 4,0s cast, range 9 90-degree cone
    };

    class TransonicBlast : Components.SelfTargetedAOEs
    {
        public TransonicBlast() : base(ActionID.MakeSpell(AID.TransonicBlast), new AOEShapeCone(9, 45.Degrees())) { }
    }

    class D110CaladriusMaturusStates : StateMachineBuilder
    {
        public D110CaladriusMaturusStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<TransonicBlast>()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Caladrius).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 896, NameID = 12078)]
    public class D110CaladriusMaturus : BossModule
    {
        public D110CaladriusMaturus(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(47, -570.5f), 8.5f, 11.5f)) { }

        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
            foreach (var s in Enemies(OID.Caladrius))
                Arena.Actor(s, ArenaColor.Enemy);
        }
    }
}
