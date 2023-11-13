// <copyright file="FiveTwentyNineSystem.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FiveTwentyNineTiles
{
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Prefabs;
    using Game.Serialization;
    using Unity.Collections;
    using Unity.Entities;

    /// <summary>
    /// The 529 tile mod system.
    /// </summary>
    internal sealed partial class FiveTwentyNineSystem : GameSystemBase, IPostDeserialize
    {
        // Query to find milestones.
        private EntityQuery _milestoneQuery;

        // Query to find map tiles.
        private EntityQuery _mapTileQuery;

        /// <summary>
        /// Called by the game in post-deserialization.
        /// </summary>
        /// <param name="context">Game context.</param>
        public void PostDeserialize(Context context)
        {
            // Unlock all tiles.
            if (Mod.ActiveSettings.UnlockAll)
            {
                EntityManager.RemoveComponent<Native>(_mapTileQuery.ToEntityArray(Allocator.Temp));
            }
        }

        /// <summary>
        /// Called when the system is created.
        /// </summary>
        protected override void OnCreate()
        {
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
            MilestoneJob milestoneJob = default;
            milestoneJob.m_MilestoneDataType = SystemAPI.GetComponentTypeHandle<MilestoneData>(false);
            milestoneJob.Run(_milestoneQuery);
        }

        /// <summary>
        /// Job to reassign milestone tile counts.
        /// </summary>
        public partial struct MilestoneJob : IJobEntity
        {
            /// <summary>
            /// Data type handle (<see cref="MilestoneData"/>).
            /// </summary>
            public ComponentTypeHandle<MilestoneData> m_MilestoneDataType;

            /// <summary>
            /// Job execution.
            /// </summary>
            /// <param name="milestone"><see cref="MilestoneData"/> component.</param>
            public void Execute(ref MilestoneData milestone)
            {
                switch (milestone.m_MapTiles)
                {
                    case 3:
                        milestone.m_MapTiles = 4;
                        break;
                    case 4:
                        milestone.m_MapTiles = 5;
                        break;
                    case 5:
                        milestone.m_MapTiles = 7;
                        break;
                    case 6:
                        milestone.m_MapTiles = 7;
                        break;
                    case 7:
                        milestone.m_MapTiles = 8;
                        break;
                    case 8:
                        milestone.m_MapTiles = 10;
                        break;
                    case 9:
                        milestone.m_MapTiles = 11;
                        break;
                    case 10:
                        milestone.m_MapTiles = 12;
                        break;
                    case 12:
                        milestone.m_MapTiles = 14;
                        break;
                    case 15:
                        milestone.m_MapTiles = 18;
                        break;
                    case 18:
                        milestone.m_MapTiles = 22;
                        break;
                    case 21:
                        milestone.m_MapTiles = 25;
                        break;
                    case 24:
                        milestone.m_MapTiles = 29;
                        break;
                    case 28:
                        milestone.m_MapTiles = 34;
                        break;
                    case 32:
                        milestone.m_MapTiles = 38;
                        break;
                    case 36:
                        milestone.m_MapTiles = 43;
                        break;
                    case 41:
                        milestone.m_MapTiles = 49;
                        break;
                    case 46:
                        milestone.m_MapTiles = 55;
                        break;
                    case 51:
                        milestone.m_MapTiles = 61;
                        break;
                    case 56:
                        milestone.m_MapTiles = 68;
                        break;
                }
            }
        }
    }
}
