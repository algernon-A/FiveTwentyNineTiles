// <copyright file="FiveTwentyNineSystem.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Colossal.Collections;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Prefabs;
    using Game.Serialization;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Jobs;

    /// <summary>
    /// The 529 tile mod system.
    /// </summary>
    internal sealed class FiveTwentyNineSystem : GameSystemBase, IPostDeserialize
    {
        // Query to find milestones.
        private EntityQuery _milestoneQuery;

        // Query to find map tiles.
        private EntityQuery _mapTileQuery;

        // Type and job data.
        private TypeHandle _typeHandle;

        /// <summary>
        /// Called by the game in post-deserialization.
        /// </summary>
        /// <param name="context">Game context.</param>
        public void PostDeserialize(Context context)
        {
            // Unlock all tiles.
            if (SettingsSystem.ActiveSettings.UnlockAll)
            {
                EntityManager.RemoveComponent<Native>(_mapTileQuery.ToEntityArray(Allocator.Temp));
            }
        }

        /// <summary>
        /// Called when the system is created.
        /// </summary>
        protected override void OnCreate()
        {
            Log.Debug("529.OnCreate");
            base.OnCreate();

            // Initialize queries.
            _milestoneQuery = GetEntityQuery(ComponentType.ReadWrite<MilestoneData>());
            _mapTileQuery = GetEntityQuery(ComponentType.ReadOnly<MapTile>());
            RequireForUpdate(_milestoneQuery);
            RequireForUpdate(_mapTileQuery);
        }

        /// <summary>
        /// Called every update.
        /// </summary>
        protected override void OnUpdate()
        {
            // Run update milestone job.
            _typeHandle.__Game_Milestones_RW_ComponentTypeHandle.Update(ref CheckedStateRef);
            MilestoneJob milestoneJob = default;
            milestoneJob.m_MilestoneDataType = _typeHandle.__Game_Milestones_RW_ComponentTypeHandle;
            milestoneJob.Run(_milestoneQuery);
        }

        /// <summary>
        /// Called when created for compilation.
        /// </summary>
        protected override void OnCreateForCompiler()
        {
            base.OnCreateForCompiler();
            _typeHandle.AssignHandles(ref CheckedStateRef);
        }

        /// <summary>
        /// Job to reassign milestone tile counts.
        /// </summary>
        public struct MilestoneJob : IJobChunk
        {
            /// <summary>
            /// Data type handle (<see cref="MilestoneData"/>).
            /// </summary>
            public ComponentTypeHandle<MilestoneData> m_MilestoneDataType;

            /// <summary>
            /// Job execution.
            /// </summary>
            /// <param name="chunk"><see cref="Chunk"/> containing entities.</param>
            /// <param name="unfilteredChunkIndex">The index of the current <see cref="Chunk"/> within the list of all chunks in all archetypes matched by the <see cref="EntityQuery"/> that the job was run against.</param>
            /// <param name="useEnabledMask">A value indicating whether <c>chunkEnabledMask</c> should be used to filter the provided <see cref="EntityQuery"/>.</param>
            /// <param name="chunkEnabledMask">A mask to filter the provided <see cref="EntityQuery"/>.  Ignored if <c>useEnabledMask</c> is <c>false</c>.</param>
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                // Iterate through each MilestoneData component in this chunk and update the tile count.
                NativeArray<MilestoneData> milestoneArray = chunk.GetNativeArray(ref m_MilestoneDataType);
                for (int i = 0; i < chunk.Count; i++)
                {
                    ref MilestoneData reference = ref milestoneArray.ElementAt(i);
                    int newTileCount = reference.m_MapTiles;

                    switch (newTileCount)
                    {
                        case 3:
                            newTileCount = 4;
                            break;
                        case 4:
                            newTileCount = 5;
                            break;
                        case 5:
                            newTileCount = 7;
                            break;
                        case 6:
                            newTileCount = 7;
                            break;
                        case 7:
                            newTileCount = 8;
                            break;
                        case 8:
                            newTileCount = 10;
                            break;
                        case 9:
                            newTileCount = 11;
                            break;
                        case 10:
                            newTileCount = 12;
                            break;
                        case 12:
                            newTileCount = 14;
                            break;
                        case 15:
                            newTileCount = 18;
                            break;
                        case 18:
                            newTileCount = 22;
                            break;
                        case 21:
                            newTileCount = 25;
                            break;
                        case 24:
                            newTileCount = 29;
                            break;
                        case 28:
                            newTileCount = 34;
                            break;
                        case 32:
                            newTileCount = 38;
                            break;
                        case 36:
                            newTileCount = 43;
                            break;
                        case 41:
                            newTileCount = 49;
                            break;
                        case 46:
                            newTileCount = 55;
                            break;
                        case 51:
                            newTileCount = 61;
                            break;
                        case 56:
                            newTileCount = 68;
                            break;
                    }

                    reference.m_MapTiles = newTileCount;
                }
            }
        }

        /// <summary>
        /// Struct containing type and job information.
        /// </summary>
        private struct TypeHandle
        {
            /// <summary>
            /// Component type handle.
            /// </summary>
            internal ComponentTypeHandle<MilestoneData> __Game_Milestones_RW_ComponentTypeHandle;

            /// <summary>
            /// Entity type handle.
            /// </summary>
            [ReadOnly]
            internal EntityTypeHandle __Unity_Entities_Entity_TypeHandle;

            /// <summary>
            /// Assigns handles.
            /// </summary>
            /// <param name="state">Unity <see cref="SystemState"/>.</param>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            internal void AssignHandles(ref SystemState state)
            {
                __Unity_Entities_Entity_TypeHandle = state.GetEntityTypeHandle();
                __Game_Milestones_RW_ComponentTypeHandle = state.GetComponentTypeHandle<MilestoneData>(isReadOnly: false);
            }
        }
    }
}
