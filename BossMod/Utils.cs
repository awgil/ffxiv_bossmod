using Dalamud.Game.ClientState.Objects.Types;
using System;
using System.Numerics;

namespace BossMod
{
    class Utils
    {
        public static string ObjectString(GameObject obj)
        {
            return $"{obj.DataId:X} '{obj.Name}' <{obj.ObjectId:X}>";
        }

        public static string ObjectKindString(GameObject obj)
        {
            if (obj.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc)
                return $"{obj.ObjectKind}/{(Dalamud.Game.ClientState.Objects.Enums.BattleNpcSubKind)obj.SubKind}";
            else if (obj.SubKind == 0)
                return $"{obj.ObjectKind}";
            else
                return $"{obj.ObjectKind}/{obj.SubKind}";
        }

        public static string RadianString(float rad)
        {
            return $"{(rad / Math.PI * 180):f0}";
        }

        public static string Vec3String(Vector3 pos)
        {
            return $"[{pos.X:f2}, {pos.Y:f2}, {pos.Z:f2}]";
        }

        public static string QuatString(Quaternion q)
        {
            return $"[{q.X:f2}, {q.Y:f2}, {q.Z:f2}, {q.W:f2}]";
        }

        public static string ActionString(uint actionID)
        {
            var actionData = Service.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()?.GetRow(actionID);
            string name = actionData?.Name ?? "<not found>";
            return $"{actionID} '{name}'";
        }

        public static string CastTimeString(float current, float total)
        {
            return $"{current:f2}/{total:f2}";
        }

        public static unsafe T ReadField<T>(void* address, int offset) where T : unmanaged
        {
            return *(T*)((IntPtr)address + offset);
        }

        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject* GameObjectInternal(GameObject? obj)
        {
            return (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)obj?.Address;
        }

        public static unsafe FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara* BattleCharaInternal(BattleChara? chara)
        {
            return (FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara*)chara?.Address;
        }

        public static unsafe Vector3 BattleCharaCastLocation(BattleChara chara)
        {
            return BattleCharaInternal(chara)->SpellCastInfo.CastLocation;
        }

        public static unsafe ulong SceneObjectFlags(FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object* o)
        {
            return ReadField<ulong>(o, 0x38);
        }
    }
}
