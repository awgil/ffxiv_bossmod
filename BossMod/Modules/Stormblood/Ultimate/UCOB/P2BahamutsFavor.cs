using BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3QueensGuard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BossMod.Stormblood.Ultimate.UCOB
{
    class P2BahamutsFavor : BossComponent
    {
        public Actor? Source;
        public List<AID> PendingMechanics = new();
        public DateTime NextActivation;

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (PendingMechanics.Count > 0)
            {
                hints.Add(string.Join(" > ", PendingMechanics.Select(aid => aid switch
                {
                    AID.IronChariot => "Out",
                    AID.LunarDynamo => "In",
                    AID.ThermionicBeam => "Stack",
                    AID.RavenDive or AID.MeteorStream => "Spread",
                    AID.DalamudDive => "Tankbuster",
                    _ => "???"
                })));
            }
        }

        public override void OnActorNpcYell(BossModule module, Actor actor, ushort id)
        {
            var (aid1, aid2) = id switch
            {
                6492 => (AID.LunarDynamo, AID.IronChariot),
                6493 => (AID.LunarDynamo, AID.ThermionicBeam),
                6494 => (AID.ThermionicBeam, AID.IronChariot),
                6495 => (AID.ThermionicBeam, AID.LunarDynamo),
                6496 => (AID.RavenDive, AID.IronChariot),
                6497 => (AID.RavenDive, AID.LunarDynamo),
                6500 => (AID.MeteorStream, AID.DalamudDive),
                6501 => (AID.DalamudDive, AID.ThermionicBeam),
                _ => (default, default)
            };
            if (aid1 != default)
            {
                Source = actor;
                PendingMechanics.Clear();
                PendingMechanics.Add(aid1);
                PendingMechanics.Add(aid2);
                NextActivation = module.WorldState.CurrentTime.AddSeconds(5.1f);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if (PendingMechanics.Count > 0 && (AID)spell.Action.ID == PendingMechanics.First())
            {
                PendingMechanics.RemoveAt(0);
                NextActivation = module.WorldState.CurrentTime.AddSeconds(3.1f);
            }
        }
    }

    class P2BahamutsFavorIronChariotLunarDynamo : Components.GenericAOEs
    {
        private P2BahamutsFavor? _favor;

        private static AOEShapeCircle _shapeChariot = new(8.55f);
        private static AOEShapeDonut _shapeDynamo = new(6, 22); // TODO: verify inner radius

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            AOEShape? shape = _favor != null && _favor.PendingMechanics.Count > 0 ? _favor.PendingMechanics[0] switch
            {
                AID.IronChariot => _shapeChariot,
                AID.LunarDynamo => _shapeDynamo,
                _ => null
            } : null;
            if (shape != null && _favor?.Source != null)
                yield return new(shape, _favor.Source.Position, default, _favor.NextActivation);
        }

        public override void Init(BossModule module) => _favor = module.FindComponent<P2BahamutsFavor>();
    }

    class P2BahamutsFavorThermionicBeam : Components.UniformStackSpread
    {
        private P2BahamutsFavor? _favor;

        public P2BahamutsFavorThermionicBeam() : base(4, 0, 8) { }

        public override void Init(BossModule module) => _favor = module.FindComponent<P2BahamutsFavor>();

        public override void Update(BossModule module)
        {
            bool stackImminent = _favor != null && _favor.PendingMechanics.Count > 0 && _favor.PendingMechanics[0] == AID.ThermionicBeam;
            if (stackImminent && Stacks.Count == 0 && module.Raid.Player() is var target && target != null) // note: target is random
                AddStack(target, _favor!.NextActivation);
            else if (!stackImminent && Stacks.Count > 0)
                Stacks.Clear();
            base.Update(module);
        }
    }

    class P2BahamutsFavorRavenDive : Components.UniformStackSpread
    {
        private P2BahamutsFavor? _favor;

        public P2BahamutsFavorRavenDive() : base(0, 3, alwaysShowSpreads: true) { }

        public override void Init(BossModule module) => _favor = module.FindComponent<P2BahamutsFavor>();

        public override void Update(BossModule module)
        {
            bool spreadImminent = _favor != null && _favor.PendingMechanics.Count > 0 && _favor.PendingMechanics[0] == AID.RavenDive;
            if (spreadImminent && Spreads.Count == 0)
                AddSpreads(module.Raid.WithoutSlot(true), _favor!.NextActivation);
            else if (!spreadImminent && Spreads.Count > 0)
                Spreads.Clear();
            base.Update(module);
        }
    }

    class P2BahamutsFavorFireball : Components.UniformStackSpread
    {
        public Actor? Target;
        private BitMask _forbidden;
        private DateTime _activation;

        public P2BahamutsFavorFireball() : base(4, 0, 1) { }

        public void Show()
        {
            if (Target != null)
                AddStack(Target, _activation, _forbidden);
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Firescorched)
            {
                _forbidden.Set(module.Raid.FindSlot(actor.InstanceID));
                foreach (ref var s in Stacks.AsSpan())
                    s.ForbiddenPlayers = _forbidden;
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Firescorched)
            {
                _forbidden.Clear(module.Raid.FindSlot(actor.InstanceID));
                foreach (ref var s in Stacks.AsSpan())
                    s.ForbiddenPlayers = _forbidden;
            }
        }

        public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
        {
            if ((TetherID)tether.ID == TetherID.Fireball)
            {
                Target = module.WorldState.Actors.Find(tether.Target);
                _activation = module.WorldState.CurrentTime.AddSeconds(5.1);
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.FireballP2)
            {
                Stacks.Clear();
                Target = null;
                _activation = default;
            }
        }
    }

    class P2BahamutsFavorChainLightning : Components.UniformStackSpread
    {
        public P2BahamutsFavorChainLightning() : base(0, 5, alwaysShowSpreads: true) { }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Thunderstruck)
                AddSpread(actor, status.ExpireAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.ChainLightningAOE)
                Spreads.Clear();
        }
    }

    class P2BahamutsFavorDeathstorm : BossComponent
    {
        public int NumDeathstorms { get; private set; }
        private List<(Actor player, DateTime expiration, bool cleansed)> _dooms = new();
        private List<(WPos predicted, Actor? voidzone)> _cleanses = new();

        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            var doomOrder = _dooms.FindIndex(d => d.player == actor);
            if (doomOrder >= 0 && !_dooms[doomOrder].cleansed)
                hints.Add($"Doom {doomOrder + 1}", (_dooms[doomOrder].expiration - module.WorldState.CurrentTime).TotalSeconds < 3);
        }

        public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
        {
            var doomOrder = _dooms.FindIndex(d => d.player == pc);
            if (doomOrder >= 0 && !_dooms[doomOrder].cleansed && doomOrder < _cleanses.Count)
                arena.AddCircle(_cleanses[doomOrder].voidzone?.Position ?? _cleanses[doomOrder].predicted, 1, ArenaColor.Safe);
        }

        public override void OnActorCreated(BossModule module, Actor actor)
        {
            if ((OID)actor.OID == OID.VoidzoneSalvation)
            {
                var index = _cleanses.FindIndex(z => z.voidzone == null && z.predicted.AlmostEqual(actor.Position, 0.5f));
                if (index >= 0)
                    _cleanses.Ref(index).voidzone = actor;
                else
                    module.ReportError(this, $"Failed to find voidzone predicted pos for {actor}");
            }
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Doom)
            {
                _dooms.Add((actor, status.ExpireAt, false));
                _dooms.SortBy(d => d.expiration);
            }
        }

        public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
        {
            if ((SID)status.ID == SID.Doom)
            {
                var index = _dooms.FindIndex(d => d.player == actor);
                if (index >= 0)
                    _dooms.Ref(index).cleansed = true;
                else
                    module.ReportError(this, $"Failed to find doom on {actor}");
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.WingsOfSalvation)
                _cleanses.Add((spell.LocXZ, null));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID == AID.Deathstorm)
            {
                _dooms.Clear();
                _cleanses.Clear();
                ++NumDeathstorms;
            }
        }
    }

    class P2BahamutsFavorWingsOfSalvation : Components.LocationTargetedAOEs
    {
        public P2BahamutsFavorWingsOfSalvation() : base(ActionID.MakeSpell(AID.WingsOfSalvation), 4) { }
    }
}
