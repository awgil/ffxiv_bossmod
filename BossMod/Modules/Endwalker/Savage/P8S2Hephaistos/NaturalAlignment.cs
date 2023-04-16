using System.Linq;

namespace BossMod.Endwalker.Savage.P8S2
{
    class NaturalAlignment : Components.GenericStackSpread
    {
        public enum Mechanic { None, StackSpread, FireIce }

        private BitMask _targets;
        private BitMask _inverse;
        private Mechanic CurMechanic;
        private Actor? CurMechanicSource;
        private bool CurMechanicInverted;
        public int CurMechanicProgress { get; private set; }

        public NaturalAlignment() : base(true) { }

        public override void Update(BossModule module)
        {
            Stacks.Clear();
            Spreads.Clear();
            if (CurMechanicProgress >= 2 || CurMechanicSource == null)
                return;

            bool firstPart = CurMechanicProgress == (CurMechanicInverted ? 1 : 0);
            var potentialTargets = module.Raid.WithSlot().ExcludedFromMask(_targets).Actors().ToList();
            switch (CurMechanic)
            {
                case Mechanic.StackSpread:
                    if (firstPart)
                    {
                        // no idea how stack target is actually selected, assume it is closest...
                        var stackTarget = potentialTargets.Closest(CurMechanicSource.Position);
                        if (stackTarget != null)
                            Stacks.Add(new(stackTarget, 6, 6, 6, default, _targets));
                    }
                    else
                    {
                        foreach (var target in potentialTargets)
                            Spreads.Add(new(target, 6));
                    }
                    break;
                case Mechanic.FireIce:
                    if (firstPart)
                    {
                        foreach (var target in potentialTargets.SortedByRange(CurMechanicSource.Position).TakeLast(3))
                            Stacks.Add(new(target, 6, 2, 2));
                    }
                    else
                    {
                        foreach (var target in potentialTargets.SortedByRange(CurMechanicSource.Position).Take(2))
                            Stacks.Add(new(target, 5, 3, 3));
                    }
                    break;
            }
            base.Update(module);
        }

        public override void AddGlobalHints(BossModule module, GlobalHints hints)
        {
            if (CurMechanicProgress >= 2 || CurMechanicSource == null)
                return;
            bool firstPart = CurMechanicProgress == (CurMechanicInverted ? 1 : 0);
            var hint = CurMechanic switch
            {
                Mechanic.StackSpread => firstPart ? "Stack" : "Spread",
                Mechanic.FireIce => firstPart ? "Fire" : "Ice",
                _ => ""
            };
            if (hint.Length > 0)
                hints.Add($"Next NA: {hint}");
        }

        public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
        {
            switch ((SID)status.ID)
            {
                case SID.InverseMagicks:
                    _inverse.Set(module.Raid.FindSlot(actor.InstanceID));
                    break;
                case SID.NaturalAlignmentMechanic:
                    switch (status.Extra)
                    {
                        case 0x209: // initial application
                            _targets.Set(module.Raid.FindSlot(actor.InstanceID));
                            break;
                        case 0x1E0: // stack->spread filling progress bars
                        case 0x1E1: // stack->spread empty progress bars
                            CurMechanic = Mechanic.StackSpread;
                            CurMechanicSource = actor;
                            CurMechanicInverted = _inverse[module.Raid.FindSlot(actor.InstanceID)];
                            CurMechanicProgress = 0;
                            break;
                        case 0x1E2: // spread->stack filling progress bars
                        case 0x1E3: // spread->stack empty progress bars
                            CurMechanic = Mechanic.StackSpread;
                            CurMechanicSource = actor;
                            CurMechanicInverted = !_inverse[module.Raid.FindSlot(actor.InstanceID)];
                            CurMechanicProgress = 0;
                            break;
                        case 0x1DC: // fire->ice filling progress bars
                        case 0x1DD: // fire->ice empty progress bars
                            CurMechanic = Mechanic.FireIce;
                            CurMechanicSource = actor;
                            CurMechanicInverted = _inverse[module.Raid.FindSlot(actor.InstanceID)];
                            CurMechanicProgress = 0;
                            break;
                        case 0x1DE: // ice->fire filling progress bars
                        case 0x1DF: // ice->fire empty progress bars
                            CurMechanic = Mechanic.FireIce;
                            CurMechanicSource = actor;
                            CurMechanicInverted = !_inverse[module.Raid.FindSlot(actor.InstanceID)];
                            CurMechanicProgress = 0;
                            break;
                    }
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.ForcibleTrifire:
                case AID.ForcibleFireStack:
                    if (CurMechanicProgress == (CurMechanicInverted ? 1 : 0))
                        ++CurMechanicProgress;
                    break;
                case AID.ForcibleDifreeze:
                case AID.ForcibleFireSpread:
                    if (CurMechanicProgress == (CurMechanicInverted ? 0 : 1))
                        ++CurMechanicProgress;
                    break;
            }
        }
    }
}
