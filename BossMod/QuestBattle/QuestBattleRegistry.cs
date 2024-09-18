using System.Reflection;

namespace BossMod.QuestBattle;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class QuestAttribute(BossModuleInfo.Maturity maturity, uint cfcId) : Attribute
{
    public BossModuleInfo.Maturity Maturity => maturity;
    public uint CFCID => cfcId;
}

public static class QuestBattleRegistry
{
    public class Info
    {
        public required Type HandlerType;
        public uint CFCID;
        public BossModuleInfo.Maturity Maturity;
        public required Func<WorldState, QuestBattle> Factory;
    }

    private static readonly Dictionary<uint, Info> _questHandlersByCFC = [];

    static QuestBattleRegistry()
    {
        foreach (var t in Utils.GetDerivedTypes<QuestBattle>(Assembly.GetExecutingAssembly()).Where(t => !t.IsAbstract))
        {
            var attr = t.GetCustomAttribute<QuestAttribute>();
            if (attr == null)
            {
                Service.Log($"Quest module {t} has no Quest attribute, skipping");
                continue;
            }
            var info = new Info()
            {
                HandlerType = t,
                CFCID = attr.CFCID,
                Maturity = attr.Maturity,
                Factory = New<QuestBattle>.ConstructorDerived<WorldState>(t)
            };
            _questHandlersByCFC[info.CFCID] = info;
        }
    }

    public static QuestBattle? GetHandler(WorldState ws, uint cfcId, BossModuleInfo.Maturity minMaturity)
    {
        return _questHandlersByCFC.TryGetValue(cfcId, out var info) && info.Maturity >= minMaturity ? info.Factory(ws) : null;
    }
}

