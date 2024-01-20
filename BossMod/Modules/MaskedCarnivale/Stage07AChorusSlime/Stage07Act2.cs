using System.Collections.Generic;
using System.Linq;
using BossMod.Components;

namespace BossMod.MaskedCarnivale.Stage07.Act2
{
    public enum OID : uint
    {
        Boss = 0x2705, //R=1.6
        Sprite = 0x2704, //R0.8
    };

    public enum AID : uint
    {
    Detonation = 14696, // 2705->self, no cast, range 6+R circle
    Blizzard = 14709, // 2704->player, 1,0s cast, single-target
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
            hints.Add("Pull or push the Lava Slimes to the Ice Sprites and then hit the slimes\nfrom a distance to set of the explosions.");
        } 
    }       
class Stage01States : StateMachineBuilder
    {
        public Stage01States(BossModule module) : base(module)
        {
            TrivialPhase()
            .ActivateOnEnter<SlimeExplosion>()
            .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Sprite).All(e => e.IsDead);
        }
    }

public class Stage01 : BossModule
    {
        public Stage01(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
        }
        protected override void DrawArenaForeground(int pcSlot, Actor pc)
        {
                Arena.AddQuad(new(107,110),new(110,113),new(113,110),new(110,107), ArenaColor.Border, 2);
                Arena.AddQuad(new(93,110),new(90,107),new(87,110),new(90,113), ArenaColor.Border, 2);
                Arena.AddQuad(new(90,93),new(93,90),new(90,87),new(87,90), ArenaColor.Border, 2);
                Arena.AddQuad(new(110,93),new(113,90),new(110,87),new(107,90), ArenaColor.Border, 2);
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
