using System.Collections.Generic;
using System.Linq;

namespace BossMod.ReplayVisualization
{
    public class ColumnEnemyCasts : ColumnGenericHistory, IToggleableColumn
    {
        public bool Visible
        {
            get => Width > 0;
            set => Width = value ? DefaultWidth : 0;
        }

        public ColumnEnemyCasts(Timeline timeline, StateMachineTree tree, List<int> phaseBranches, Replay replay, Replay.Encounter enc, Replay.Participant enemy)
            : base(timeline, tree, phaseBranches)
        {
            var moduleInfo = ModuleRegistry.FindByOID(enc.OID);
            foreach (var c in enemy.Casts)
            {
                var name = $"{c.ID} ({moduleInfo?.ActionIDType?.GetEnumName(c.ID.ID)}) {ReplayUtils.ParticipantString(enemy, c.Time.Start)} -> {ReplayUtils.ParticipantString(c.Target, c.Time.Start)}";
                this.AddHistoryEntryRange(enc.Time.Start, c.Time, name, c.Interruptible ? 0x80800080 : 0x80808080).AddCastTooltip(c);
            }
            foreach (var a in replay.EncounterActions(enc).Where(a => a.Source == enemy))
            {
                var name = $"{a.ID} ({moduleInfo?.ActionIDType?.GetEnumName(a.ID.ID)}) {ReplayUtils.ParticipantString(a.Source, a.Timestamp)} -> {ReplayUtils.ParticipantString(a.MainTarget, a.Timestamp)} #{a.GlobalSequence}";
                var color = ColumnUtils.ActionHasDamageToPlayerEffects(a) ? 0xffffffff : 0x80808080;
                this.AddHistoryEntryDot(enc.Time.Start, a.Timestamp, name, color).AddActionTooltip(a);
            }
        }
    }
}
