using System.Linq;
using static BossMod.Components.GenericBaitAway;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    // cones part of the mechanic
    class P6Wyrmsbreath : Components.GenericBaitAway
    {
        public Actor?[] Dragons = { null, null }; // nidhogg & hraesvelgr
        public BitMask Glows;
        private bool _allowIntersect;
        private Actor?[] _tetheredTo = new Actor?[PartyState.MaxPartySize];
        private BitMask _tooClose;

        private static AOEShapeCone _shape = new(100, 10.Degrees());

        public P6Wyrmsbreath(bool allowIntersect) : base(ActionID.MakeSpell(AID.FlameBreath)) // note: cast is arbitrary
        {
            _allowIntersect = allowIntersect;
        }

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var b = ActiveBaitsOn(actor).FirstOrDefault();
            if (b.Source == null)
            {
                if (ActiveBaits.Any(b => IsClippedBy(actor, b)))
                    hints.Add("GTFO from baits!");
            }
            else
            {
                if (_tooClose[slot])
                    hints.Add("Stretch the tether!");

                Actor? partner = IgnoredPartner(module, slot, actor);
                if (ActiveBaitsOn(actor).Any(b => PlayersClippedBy(module, b).Any(p => p != partner)))
                    hints.Add("Bait away from raid!");
                if (ActiveBaitsNotOn(actor).Any(b => b.Target != partner && IsClippedBy(actor, b)))
                    hints.Add("GTFO from baited aoe!");
            }
        }

        public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            Actor? partner = IgnoredPartner(module, pcSlot, pc);
            foreach (var bait in ActiveBaitsNotOn(pc).Where(b => b.Target != partner))
                bait.Shape.Draw(arena, BaitOrigin(bait), bait.Rotation);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            foreach (var bait in ActiveBaitsOn(pc))
                bait.Shape.Outline(arena, BaitOrigin(bait), bait.Rotation);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.DreadWyrmsbreathGlow:
                    Dragons[0] = caster;
                    Glows.Set(0);
                    break;
                case AID.GreatWyrmsbreathGlow:
                    Dragons[1] = caster;
                    Glows.Set(1);
                    break;
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if ((TetherID)tether.ID is TetherID.FlameBreath or TetherID.IceBreath or TetherID.FlameBreathNear)
            {
                var slot = module.Raid.FindSlot(source.InstanceID);
                var boss = module.WorldState.Actors.Find(tether.Target);
                if (slot >= 0 && boss != null)
                {
                    if (_tetheredTo[slot] == null)
                        CurrentBaits.Add(new(boss, source, _shape));
                    _tooClose[slot] = (TetherID)tether.ID is TetherID.FlameBreathNear;
                    _tetheredTo[slot] = boss;
                }
            }
        }

        private Actor? IgnoredPartner(BossModule module, int slot, Actor actor) => _allowIntersect && _tetheredTo[slot] != null ? module.Raid.WithSlot().WhereSlot(i => _tetheredTo[i] != null && _tetheredTo[i] != _tetheredTo[slot]).Closest(actor.Position).Item2 : null;
    }

    class P6Wyrmsbreath1 : P6Wyrmsbreath { public P6Wyrmsbreath1() : base(true) { } }

    // note: it is actually symmetrical (both tanks get tankbusters), but that is hard to express, so we select one to show arbitrarily (nidhogg)
    class P6WyrmsbreathTankbusterShared : Components.GenericSharedTankbuster
    {
        private P6Wyrmsbreath? _main;

        public P6WyrmsbreathTankbusterShared() : base(ActionID.MakeSpell(AID.DarkOrb), 6) { }

        public override void Init(BossModule module) => _main = module.FindComponent<P6Wyrmsbreath>();

        public override void Update(BossModule module)
        {
            Source = Target = null;
            if (_main?.Glows.Raw == 3)
            {
                Source = _main.Dragons[0];
                Target = module.WorldState.Actors.Find(Source?.TargetID ?? 0);
                Activation = Source?.CastInfo?.FinishAt ?? module.WorldState.CurrentTime;
            }
        }
    }
}
