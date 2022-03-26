using System.Linq;

namespace BossMod.Endwalker.HydaelynEx
{
    using static BossModule;

    class Crystallize : Component
    {
        public enum Element { None, Water, Earth, Ice }
        public Element CurElement { get; private set; }

        private static float _waterRadius = 6;
        private static float _earthRadius = 6;
        private static float _iceRadius = 5;

        public override void Init(BossModule module)
        {
            CurElement = (AID)(module.PrimaryActor.CastInfo?.Action.ID ?? 0) switch
            {
                AID.CrystallizeSwordStaffWater or AID.CrystallizeChakramWater => Element.Water,
                AID.CrystallizeStaffEarth or AID.CrystallizeChakramEarth => Element.Earth,
                AID.CrystallizeStaffIce or AID.CrystallizeChakramIce => Element.Ice,
                _ => Element.None
            };
            if (CurElement == Element.None)
                module.ReportError(this, $"Unexpected boss cast {module.PrimaryActor.CastInfo?.Action.ID ?? 0}");
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            switch (CurElement)
            {
                case Element.Water:
                    int healersInRange = module.Raid.WithoutSlot().Where(a => a.Role == Role.Healer).InRadius(actor.Position, _waterRadius).Count();
                    if (healersInRange > 1)
                        hints.Add("Hit by two aoes!");
                    else if (healersInRange == 0)
                        hints.Add("Stack with healer!");
                    break;
                case Element.Earth:
                    if (module.Raid.WithoutSlot().OutOfRadius(actor.Position, _earthRadius).Any())
                        hints.Add("Stack!");
                    break;
                case Element.Ice:
                    if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _iceRadius).Any())
                        hints.Add("Spread!");
                    break;
            }
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            string hint = CurElement switch
            {
                Element.Water => "Stack in fours",
                Element.Earth => "Stack all",
                Element.Ice => "Spread",
                _ => ""
            };
            if (hint.Length > 0)
                hints.Add(hint);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            switch (CurElement)
            {
                case Element.Water:
                    foreach (var player in module.Raid.WithoutSlot())
                    {
                        if (player.Role == Role.Healer)
                        {
                            arena.Actor(player, arena.ColorDanger);
                            arena.AddCircle(player.Position, _waterRadius, arena.ColorSafe);
                        }
                        else
                        {
                            arena.Actor(player, arena.ColorPlayerGeneric);
                        }
                    }
                    break;
                case Element.Earth:
                    arena.AddCircle(pc.Position, _earthRadius, arena.ColorSafe);
                    foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
                        arena.Actor(player, GeometryUtils.PointInCircle(player.Position - pc.Position, _earthRadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                    break;
                case Element.Ice:
                    arena.AddCircle(pc.Position, _iceRadius, arena.ColorDanger);
                    foreach (var player in module.Raid.WithoutSlot().Exclude(pc))
                        arena.Actor(player, GeometryUtils.PointInCircle(player.Position - pc.Position, _iceRadius) ? arena.ColorPlayerInteresting : arena.ColorPlayerGeneric);
                    break;
            }
        }

        // note: this is pure validation, we currently rely on crystallize cast id to determine element...
        public override void OnStatusGain(BossModule module, Actor actor, int index)
        {
            if (actor != module.PrimaryActor || (SID)actor.Statuses[index].ID != SID.CrystallizeElement)
                return;

            var element = actor.Statuses[index].Extra switch
            {
                0x151 => Element.Water,
                0x152 => Element.Earth,
                0x153 => Element.Ice,
                _ => Element.None
            };
            if (element == Element.None || element != CurElement)
                module.ReportError(this, $"Unexpected extra of element buff: {actor.Statuses[index].Extra:X4}, cur element {CurElement}");
        }

        public override void OnEventCast(BossModule module, CastEvent info)
        {
            if (CurElement == Element.None || !info.IsSpell())
                return;

            var element = (AID)info.Action.ID switch
            {
                AID.CrystallineWater => Element.Water,
                AID.CrystallineStone => Element.Earth,
                AID.CrystallineBlizzard => Element.Ice,
                _ => Element.None
            };

            if (element == Element.None)
                return;

            if (element != CurElement)
                module.ReportError(this, $"Unexpected element cast: got {(AID)info.Action.ID}, expected {CurElement}");
            CurElement = Element.None;
        }
    }
}
