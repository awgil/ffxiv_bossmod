using System.Linq;
using BossMod.Components;

// CONTRIB: made by malediktus, not checked
namespace BossMod.MaskedCarnivale.Stage07.Act2
{
    public enum OID : uint
    {
        Boss = 0x2705, //R=1.6
        Sprite = 0x2704, //R=0.8
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
            foreach (var p in module.Enemies(OID.Boss).Where(x => x.HP.Cur > 0))
                arena.AddCircle(p.Position, 7.6f, ArenaColor.Danger);
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var player = module.Raid.Player();
            if (player != null)
                foreach (var p in module.Enemies(OID.Boss).Where(x => x.HP.Cur > 0))
                    if (player.Position.InCircle(p.Position, 7.6f))
                    {
                        hints.Add("In slime explosion radius!");
                    }
        }
    }

    class Hints : BossComponent
    {
        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            hints.Add("Pull or push the Lava Slimes to the Ice Sprites and then hit the slimes\nfrom a distance to set of the explosions.");
        }
    }

    class Layout : Layout4Quads { }

    class Stage07Act2States : StateMachineBuilder
    {
        public Stage07Act2States(BossModule module) : base(module)
        {
            TrivialPhase()
                .Raw.Update = () => module.Enemies(OID.Boss).All(e => e.IsDead) && module.Enemies(OID.Sprite).All(e => e.IsDead);
        }
    }

    [ModuleInfo(CFCID = 617, NameID = 8094)]
    public class Stage07Act2 : BossModule
    {
        public Stage07Act2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 25))
        {
            ActivateComponent<Hints>();
            ActivateComponent<Layout>();
            ActivateComponent<SlimeExplosion>();
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
