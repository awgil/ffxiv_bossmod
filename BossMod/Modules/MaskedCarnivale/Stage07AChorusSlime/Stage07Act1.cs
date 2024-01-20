using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.MaskedCarnivale.Stage07.Act1
{
    public enum OID : uint
    {
        Boss = 0x2703, //R=1.6
        Sprite = 0x2702, //R0.8
    };

    public enum AID : uint
    {
    Detonation = 14696, // 2703->self, no cast, range 6+R circle
    Blizzard = 14709, // 2702->player, 1,0s cast, single-target
    };
    class SlimeExplosion : GenericStackSpread
    {
        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            if (!module.Enemies(OID.Boss).All(e => e.IsDead))
                foreach (var p in module.Enemies(OID.Boss).Where(x => x.HP.Cur > 0))
                    arena.AddCircle(p.Position, 6, ArenaColor.Danger);
        }
    }
class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("For this stage the spells Sticky Tongue and Snort are recommended.\nUse them to pull or push Slimes close to\nIce Sprites. Then hit the slime from a distance to set of an explosion.");
        } 
    }
class Hints2 : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Hit the Lava Slime from a safe distance to win this act.");
        } 
    }       
class Stage01States : StateMachineBuilder
    {
        public Stage01States(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<SlimeExplosion>()
            .DeactivateOnEnter<Hints>()
            .ActivateOnEnter<Hints2>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Sprite).All(e => e.IsDead);
        }
    }

public class Stage01 : BossModule
    {
        public Stage01(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override bool CheckPull() { return PrimaryActor.IsTargetable && PrimaryActor.InCombat || Enemies(OID.Sprite).Any(e => e.InCombat); }
        protected override void DrawEnemies(int pcSlot, Actor pc)
        {
            foreach (var s in Enemies(OID.Boss))
                Arena.Actor(s, ArenaColor.Enemy, false);
            foreach (var s in Enemies(OID.Sprite))
                Arena.Actor(s, ArenaColor.Enemy, false);
        }
    }
}
