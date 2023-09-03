using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Math;
using FFXIVClientStructs.Interop;
using ImGuiNET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BossMod
{
    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public unsafe struct CollisionQuadtreeNode
    {
        [FieldOffset(0x00)] public void** Vtbl;
        [FieldOffset(0x08)] public void** Vtbl8;
        [FieldOffset(0x10)] public CollisionQuadtreeNode* Prev;
        [FieldOffset(0x18)] public CollisionQuadtreeNode* Next;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x198)]
    public unsafe struct CollisionQuadtreeObject
    {
        [FieldOffset(0x000)] public CollisionQuadtreeNode Base;
        [FieldOffset(0x0C8)] public CollisionShapePCB* Shape; // pointer to interface really
        [FieldOffset(0x110)] public Vector3 WorldRow0;
        [FieldOffset(0x11C)] public Vector3 WorldRow1;
        [FieldOffset(0x128)] public Vector3 WorldRow2;
        [FieldOffset(0x134)] public Vector3 WorldRow3;
        [FieldOffset(0x140)] public Vector3 InvWorldRow0;
        [FieldOffset(0x14C)] public Vector3 InvWorldRow1;
        [FieldOffset(0x158)] public Vector3 InvWorldRow2;
        [FieldOffset(0x164)] public Vector3 InvWorldRow3;
        [FieldOffset(0x170)] public Vector4 BoundingSphere;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x40)]
    public unsafe struct CollisionQuadtree
    {
        [FieldOffset(0x00)] public void** Vtbl;
        [FieldOffset(0x08)] public float MinX;
        [FieldOffset(0x0C)] public float MaxX;
        [FieldOffset(0x10)] public float LeafSizeX;
        [FieldOffset(0x14)] public float MinZ;
        [FieldOffset(0x18)] public float MaxZ;
        [FieldOffset(0x1C)] public float LeafSizeZ;
        [FieldOffset(0x20)] public int NumLevels;
        [FieldOffset(0x28)] public CollisionQuadtreeNode* Nodes;
        [FieldOffset(0x30)] public int NumNodes;
        [FieldOffset(0x38)] public void* Owner;
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x40)]
    public unsafe struct CollisionShapePCB
    {
        [FieldOffset(0x00)] public void** Vtbl;
        [FieldOffset(0x08)] public void** Vtbl8;
        [FieldOffset(0x10)] public CollisionQuadtreeObject* OwnerObj;
        [FieldOffset(0x18)] public CollisionShapePCBData* Data;
    };

    [StructLayout(LayoutKind.Explicit, Size = 0x30)] // variable length structure: followed by raw verts then compressed verts then prims
    public unsafe struct CollisionShapePCBData
    {
        [FieldOffset(0x00)] public ulong Header;
        [FieldOffset(0x08)] public int Child1Offset;
        [FieldOffset(0x0C)] public int Child2Offset;
        [FieldOffset(0x10)] public Vector3 AABBMin;
        [FieldOffset(0x1C)] public Vector3 AABBMax;
        [FieldOffset(0x28)] public ushort NumVertsCompressed; // ushort[3] per vert
        [FieldOffset(0x2A)] public ushort NumPrims;
        [FieldOffset(0x2C)] public ushort NumVertsRaw; // vector3 per vert
    };

    [StructLayout(LayoutKind.Explicit, Size = 0xC)]
    public unsafe struct CollisionShapePrimitive
    {
        [FieldOffset(0x0)] public byte V1;
        [FieldOffset(0x1)] public byte V2;
        [FieldOffset(0x2)] public byte V3;
        [FieldOffset(0x4)] public uint Flags;
        [FieldOffset(0x8)] public uint Unk8;
    }

    public unsafe class DebugCollision : IDisposable
    {
        private UITree _tree = new();
        private Action _drawExtra;

        private nint _typeinfoCollisionQuadtreeObject;
        private nint _typeinfoCollisionShapePCB;

        public DebugCollision()
        {
            _typeinfoCollisionQuadtreeObject = Service.SigScanner.GetStaticAddressFromSig("48 8D 05 ?? ?? ?? ?? 48 89 07 48 8D 4C 24 ?? 33 F6");
            Service.Log($"vtbl CollisionQuadtreeObject: {_typeinfoCollisionQuadtreeObject:X}");

            _typeinfoCollisionShapePCB = Service.SigScanner.GetStaticAddressFromSig("48 8D 0D ?? ?? ?? ?? 48 89 78 10 48 89 08");
            Service.Log($"vtbl CollisionShapePCB: {_typeinfoCollisionShapePCB:X}");

            _drawExtra = () => { };
        }

        public void Dispose()
        {

        }

        public void Draw()
        {
            var module = Framework.Instance()->BGCollisionModule;
            var m1 = (void*)Utils.ReadField<nint>(module, 0x10);
            ImGui.TextUnformatted($"Module: {(nint)module:X}->{(nint)m1:X} ({Utils.ReadField<int>(m1, 0x20)} modules)");

            var m4 = (void*)Utils.ReadField<nint>(m1, 0x18);
            while (m4 != null)
            {
                var m5 = (void*)Utils.ReadField<nint>(m4, 0x28);
                var tree = (CollisionQuadtree*)Utils.ReadField<nint>(m5, 0x38);
                foreach (var n in _tree.Node($"{(nint)m4:X} -> {(nint)m5:X} -> {(nint)tree:X} quadtree: {tree->NumLevels} levels ([{tree->MinX}, {tree->MaxX}]x[{tree->MinZ}, {tree->MaxZ}], leaf {tree->LeafSizeX}x{tree->LeafSizeZ}), {tree->NumNodes} nodes", tree->NumNodes == 0))
                {
                    for (int i = 0; i < tree->NumNodes; ++i)
                    {
                        var node = tree->Nodes + i;
                        foreach (var n2 in _tree.Node($"{i}", node->Next == null, select: () => _drawExtra = () => VisualizeQuadtreeNode(node)))
                        {
                            var child = node->Next;
                            while (child != null)
                            {
                                foreach (var n3 in _tree.Node($"Object {(nint)child:X} (+{(nint)child->Vtbl - Service.SigScanner.Module.BaseAddress:X})", (nint)child->Vtbl != _typeinfoCollisionQuadtreeObject, select: () => { var obj = child; _drawExtra = (nint)obj->Vtbl == _typeinfoCollisionQuadtreeObject ? () => VisualizeObject((CollisionQuadtreeObject*)obj) : () => { }; }))
                                {
                                    if ((nint)child->Vtbl == _typeinfoCollisionQuadtreeObject)
                                        DrawObject((CollisionQuadtreeObject*)child);
                                }
                                child = child->Next;
                            }
                        }
                    }
                }
                m4 = (void*)Utils.ReadField<nint>(m4, 0x18);
            }

            _drawExtra();
        }

        private void DrawObject(CollisionQuadtreeObject* obj)
        {
            _tree.LeafNode($"Bounding sphere: [{obj->BoundingSphere.X:f3}, {obj->BoundingSphere.Y:f3}, {obj->BoundingSphere.Z:f3}] R{obj->BoundingSphere.W:f3}", select: () => _drawExtra = () => VisualizeSphere(obj->BoundingSphere));
            _tree.LeafNode($"World R0: {Utils.Vec3String(obj->WorldRow0)}");
            _tree.LeafNode($"World R1: {Utils.Vec3String(obj->WorldRow1)}");
            _tree.LeafNode($"World R2: {Utils.Vec3String(obj->WorldRow2)}");
            _tree.LeafNode($"World R3: {Utils.Vec3String(obj->WorldRow3)}");
            _tree.LeafNode($"InvWorld R0: {Utils.Vec3String(obj->InvWorldRow0)}");
            _tree.LeafNode($"InvWorld R1: {Utils.Vec3String(obj->InvWorldRow1)}");
            _tree.LeafNode($"InvWorld R2: {Utils.Vec3String(obj->InvWorldRow2)}");
            _tree.LeafNode($"InvWorld R3: {Utils.Vec3String(obj->InvWorldRow3)}");
            bool shapeHasData = obj->Shape != null && (nint)obj->Shape->Vtbl == _typeinfoCollisionShapePCB && obj->Shape->Data != null;
            foreach (var n in _tree.Node($"Shape: {(nint)obj->Shape:X} (+{(obj->Shape != null ? (nint)obj->Shape->Vtbl : Service.SigScanner.Module.BaseAddress) - Service.SigScanner.Module.BaseAddress:X})", !shapeHasData, select: () => _drawExtra = shapeHasData ? () => VisualizeShape(obj->Shape->Data, obj) : () => { }))
            {
                if (shapeHasData)
                    DrawPCBShape(obj->Shape->Data, obj);
            }
        }

        private void DrawPCBShape(CollisionShapePCBData* data, CollisionQuadtreeObject* obj)
        {
            if (data == null)
                return;
            _tree.LeafNode($"Header: {data->Header:X16}");
            _tree.LeafNode($"AABB: {Utils.Vec3String(data->AABBMin)} - {Utils.Vec3String(data->AABBMax)}", select: () => _drawExtra = () => VisualizeAABB(data->AABBMin, data->AABBMax, obj));
            foreach (var n in _tree.Node($"Vertices: {data->NumVertsRaw}+{data->NumVertsCompressed}", data->NumVertsRaw + data->NumVertsCompressed == 0))
            {
                var pRaw = (float*)(data + 1);
                for (int i = 0; i < data->NumVertsRaw; ++i)
                {
                    var v = new Vector3(pRaw[0], pRaw[1], pRaw[2]);
                    _tree.LeafNode($"[{i}] (r): {Utils.Vec3String(v)}", select: () => _drawExtra = () => VisualizeVertex(v, obj));
                    pRaw += 3;
                }
                var pCompressed = (ushort*)pRaw;
                var quantScale = (data->AABBMax - data->AABBMin) / 65535.0f;
                for (int i = 0; i < data->NumVertsCompressed; ++i)
                {
                    var v = data->AABBMin + quantScale * new Vector3(pCompressed[0], pCompressed[1], pCompressed[2]);
                    _tree.LeafNode($"[{i + data->NumVertsRaw}] (c): {Utils.Vec3String(v)}", select: () => _drawExtra = () => VisualizeVertex(v, obj));
                    pCompressed += 3;
                }
            }
            foreach (var n in _tree.Node($"Primitives: {data->NumPrims}", data->NumPrims == 0))
            {
                var pRaw = (float*)(data + 1);
                var pCompr = (ushort*)(pRaw + 3 * data->NumVertsRaw);
                var pPrims = (CollisionShapePrimitive*)(pCompr + 3 * data->NumVertsCompressed);
                for (int i = 0; i < data->NumPrims; ++i)
                {
                    _tree.LeafNode($"[{i}]: {pPrims->V1}x{pPrims->V2}x{pPrims->V3}, {pPrims->Flags:X8}, {pPrims->Unk8:X8}", select: () => { var idx = i; _drawExtra = () => VisualizePrimitive(data, idx, obj); });
                    ++pPrims;
                }
            }
            foreach (var n in _tree.Node($"Child 1 (+{data->Child1Offset})", data->Child1Offset == 0, select: () => _drawExtra = data->Child1Offset != 0 ? () => VisualizeShape((CollisionShapePCBData*)((byte*)data + data->Child1Offset), obj) : () => { }))
            {
                if (data->Child1Offset != 0)
                    DrawPCBShape((CollisionShapePCBData*)((byte*)data + data->Child1Offset), obj);
            }
            foreach (var n in _tree.Node($"Child 2 (+{data->Child2Offset})", data->Child2Offset == 0, select: () => _drawExtra = data->Child2Offset != 0 ? () => VisualizeShape((CollisionShapePCBData*)((byte*)data + data->Child2Offset), obj) : () => { }))
            {
                if (data->Child2Offset != 0)
                    DrawPCBShape((CollisionShapePCBData*)((byte*)data + data->Child2Offset), obj);
            }
        }

        private void VisualizeSphere(Vector4 sphere) => Camera.Instance?.DrawWorldSphere(new(sphere.X, sphere.Y, sphere.Z), sphere.W, ArenaColor.Safe);
        private void VisualizeAABB(Vector3 min, Vector3 max, CollisionQuadtreeObject* obj) => Camera.Instance?.DrawWorldOBB(min, max, WorldTransform(obj), ArenaColor.Safe);
        private void VisualizeVertex(Vector3 v, CollisionQuadtreeObject* obj) => Camera.Instance?.DrawWorldSphere(SharpDX.Vector3.TransformCoordinate(new(v.X, v.Y, v.Z), WorldTransform(obj)).ToSystem(), 0.1f, ArenaColor.Danger);

        private void VisualizePrimitive(CollisionShapePCBData* data, int iPrim, CollisionQuadtreeObject* obj)
        {
            var pRaw = (float*)(data + 1);
            var pCompr = (ushort*)(pRaw + 3 * data->NumVertsRaw);
            var pPrim = (CollisionShapePrimitive*)(pCompr + 3 * data->NumVertsCompressed);
            pPrim += iPrim;
            var v1 = LocalVertex(data, pPrim->V1);
            var v2 = LocalVertex(data, pPrim->V2);
            var v3 = LocalVertex(data, pPrim->V3);
            var w = WorldTransform(obj);
            var w1 = SharpDX.Vector3.TransformCoordinate(new(v1.X, v1.Y, v1.Z), w).ToSystem();
            var w2 = SharpDX.Vector3.TransformCoordinate(new(v2.X, v2.Y, v2.Z), w).ToSystem();
            var w3 = SharpDX.Vector3.TransformCoordinate(new(v3.X, v3.Y, v3.Z), w).ToSystem();
            Camera.Instance?.DrawWorldLine(w1, w2, ArenaColor.Danger);
            Camera.Instance?.DrawWorldLine(w2, w3, ArenaColor.Danger);
            Camera.Instance?.DrawWorldLine(w3, w1, ArenaColor.Danger);
        }

        private void VisualizeShape(CollisionShapePCBData* data, CollisionQuadtreeObject* obj)
        {
            for (int i = 0; i < data->NumPrims; ++i)
                VisualizePrimitive(data, i, obj);
            if (data->Child1Offset != 0)
                VisualizeShape((CollisionShapePCBData*)((byte*)data + data->Child1Offset), obj);
            if (data->Child2Offset != 0)
                VisualizeShape((CollisionShapePCBData*)((byte*)data + data->Child2Offset), obj);
        }

        private void VisualizeObject(CollisionQuadtreeObject* obj)
        {
            if (obj->Shape != null && (nint)obj->Shape->Vtbl == _typeinfoCollisionShapePCB && obj->Shape->Data != null)
                VisualizeShape(obj->Shape->Data, obj);
        }

        private void VisualizeQuadtreeNode(CollisionQuadtreeNode* node)
        {
            var child = node->Next;
            while (child != null)
            {
                if ((nint)child->Vtbl == _typeinfoCollisionQuadtreeObject)
                    VisualizeObject((CollisionQuadtreeObject*)child);
                child = child->Next;
            }
        }

        private SharpDX.Matrix WorldTransform(CollisionQuadtreeObject* obj) => new(obj->WorldRow0.X, obj->WorldRow0.Y, obj->WorldRow0.Z, 0, obj->WorldRow1.X, obj->WorldRow1.Y, obj->WorldRow1.Z, 0, obj->WorldRow2.X, obj->WorldRow2.Y, obj->WorldRow2.Z, 0, obj->WorldRow3.X, obj->WorldRow3.Y, obj->WorldRow3.Z, 1);
        private Vector3 LocalVertex(CollisionShapePCBData* data, int index)
        {
            var pRaw = (float*)(data + 1);
            if (index < data->NumVertsRaw)
            {
                pRaw += 3 * index;
                return new(pRaw[0], pRaw[1], pRaw[2]);
            }
            var pCompr = (ushort*)(pRaw + 3 * data->NumVertsRaw);
            pCompr += 3 * (index - data->NumVertsRaw);
            var quantScale = (data->AABBMax - data->AABBMin) / 65535.0f;
            return data->AABBMin + quantScale * new Vector3(pCompr[0], pCompr[1], pCompr[2]);
        }
    }
}
