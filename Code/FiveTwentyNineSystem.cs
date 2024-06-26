﻿// <copyright file="FiveTwentyNineSystem.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the Apache Licence, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace FiveTwentyNineTiles
{
    using Colossal.Entities;
    using Colossal.Logging;
    using Colossal.Serialization.Entities;
    using Game;
    using Game.Areas;
    using Game.Common;
    using Game.Prefabs;
    using Game.Serialization;
    using Unity.Collections;
    using Unity.Entities;
    using Unity.Mathematics;

    /// <summary>
    /// The 529 tile mod system.
    /// </summary>
    internal sealed partial class FiveTwentyNineSystem : GameSystemBase, IPostDeserialize
    {
        private ILog _log;

        // Query to find milestones.
        private EntityQuery _milestoneQuery;

        // Query to find a;; map tiles.
        private EntityQuery _lockedMapTileQuery;

        // Query to find unlocked map tiles.
        private EntityQuery _unlockedMapTileQuery;

        // Query to find locked features.
        private EntityQuery _featureQuery;

        // Query to find our custom milestone component.
        private EntityQuery _customMilestoneQuery;

        /// <summary>
        /// Called by the game in post-deserialization.
        /// </summary>
        /// <param name="context">Game context.</param>
        public void PostDeserialize(Context context)
        {
            // Revert any existing custom milestone.
            foreach (Entity entity in _customMilestoneQuery.ToEntityArray(Allocator.Temp))
            {
                _log.Info("removing start bonus");
                EntityManager.AddComponent<Deleted>(entity);
            }

            int totalTiles = _lockedMapTileQuery.CalculateEntityCount() + _unlockedMapTileQuery.CalculateEntityCount();
            int extraTiles = totalTiles - 441;
            _log.Info($"{totalTiles} total map tiles detected");

            // Unlock all tiles, if that's what we're doing.
            if (Mod.Instance.ActiveSettings.UnlockAll)
            {
                _log.Info("unlocking all tiles");
                EntityManager.RemoveComponent<Native>(_lockedMapTileQuery.ToEntityArray(Allocator.Temp));

                // All done - no point in doing anything else.
                return;
            }

            // Unlock extra tiles at start if that's what we're doing.
            if (Mod.Instance.ActiveSettings.ExtraTilesAtStart)
            {
                _log.Info("allocating extra tiles to start");

                // Unlock map tile purchasing feature.
                EnableTilePurchasing();

                // Create new milestone entity with initial unlocked tile count.
                Entity extraMilestone = EntityManager.CreateEntity();
                EntityManager.AddComponentData(extraMilestone, new MilestoneData { m_MapTiles = extraTiles });
                EntityManager.AddComponentData(extraMilestone, new CustomMilestone { });
            }

            // Otherwise assign extra tiles to the final milestone, if that's what we're doing.
            else if (Mod.Instance.ActiveSettings.ExtraTilesAtEnd)
            {
                _log.Info("allocating extra tiles to final milestone");

                // Iterate through milestones, looking for the last one.
                foreach (Entity entity in _milestoneQuery.ToEntityArray(Allocator.Temp))
                {
                    // Final milestone has index of 20.
                    if (EntityManager.TryGetComponent(entity, out MilestoneData milestone) && milestone.m_Index == 20)
                    {
                        milestone.m_MapTiles += extraTiles;
                        EntityManager.SetComponentData(entity, milestone);

                        // All done here.
                        break;
                    }
                }
            }

            // Otherwise, assign extra tiles across all milestones, if that's what we're doing.
            else if (Mod.Instance.ActiveSettings.AssignToMilestones)
            {
                _log.Info("updating milestones");

                foreach (Entity entity in _milestoneQuery.ToEntityArray(Allocator.Temp))
                {
                    if (EntityManager.TryGetComponent(entity, out MilestoneData milestone))
                    {
                        UpdateMilestone(ref milestone, totalTiles);
                        EntityManager.SetComponentData(entity, milestone);
                    }
                }
            }

            // Re-lock tiles, if that's what we're doing.
            if (Mod.Instance.ActiveSettings.RelockAllTiles)
            {
                _log.Info("re-locking all tiles");
                EntityManager.AddComponent<Native>(_unlockedMapTileQuery.ToEntityArray(Allocator.Temp));

                return;
            }

            // Remove all unlocked tiles if this is a new game and we're starting with no unlocked tiles.
            if (context.purpose == Purpose.NewGame && Mod.Instance.ActiveSettings.NoStartingTiles)
            {
                _log.Info("locking all tiles");

                // Ensure purchasing feature is unlocked before locking all tiles.
                EnableTilePurchasing();
                EntityManager.AddComponent<Native>(_unlockedMapTileQuery.ToEntityArray(Allocator.Temp));
            }
        }

        /// <summary>
        /// Called when the system is created.
        /// </summary>
        protected override void OnCreate()
        {
            base.OnCreate();

            // Set log.
            _log = Mod.Instance.Log;

            // Initialize queries.
            _milestoneQuery = SystemAPI.QueryBuilder().WithAllRW<MilestoneData>().Build();
            _lockedMapTileQuery = SystemAPI.QueryBuilder().WithAll<MapTile>().WithAllRW<Native>().Build();
            _unlockedMapTileQuery = SystemAPI.QueryBuilder().WithAll<MapTile>().WithNone<Native>().Build();
            _featureQuery = SystemAPI.QueryBuilder().WithAll<FeatureData, PrefabData>().WithAllRW<Locked>().Build();
            _customMilestoneQuery = SystemAPI.QueryBuilder().WithAllRW<CustomMilestone>().Build();
        }

        /// <summary>
        /// Called every update.
        /// </summary>
        protected override void OnUpdate()
        {
        }

        /// <summary>
        /// Unlocks the map tile purchasing feature.
        /// </summary>
        private void EnableTilePurchasing()
        {
            PrefabSystem prefabSystem = World.GetOrCreateSystemManaged<PrefabSystem>();
            foreach (Entity entity in _featureQuery.ToEntityArray(Allocator.Temp))
            {
                if (EntityManager.TryGetComponent(entity, out PrefabData prefabData) && prefabSystem.GetPrefab<PrefabBase>(prefabData) is PrefabBase prefab)
                {
                    // Looking for map tiles feature.
                    if (prefab.name.Equals("Map Tiles"))
                    {
                        // Remove locking.
                        EntityManager.RemoveComponent<Locked>(entity);
                        EntityManager.RemoveComponent<UnlockRequirement>(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Updates a given milestone to increase the number of unlockable map tiles.
        /// </summary>
        /// <param name="milestone">Milestone to alter.</param>
        /// <param name="totalTitles">Total map tiles available.</param>
        private void UpdateMilestone(ref MilestoneData milestone, int totalTitles)
        {
            switch (milestone.m_Index)
            {
                case 1:
                    milestone.m_MapTiles = 4;
                    break;
                case 2:
                    milestone.m_MapTiles = 5;
                    break;
                case 3:
                    milestone.m_MapTiles = 7;
                    break;
                case 4:
                    milestone.m_MapTiles = 7;
                    break;
                case 5:
                    milestone.m_MapTiles = 8;
                    break;
                case 6:
                    milestone.m_MapTiles = 10;
                    break;
                case 7:
                    milestone.m_MapTiles = 11;
                    break;
                case 8:
                    milestone.m_MapTiles = 12;
                    break;
                case 9:
                    milestone.m_MapTiles = 14;
                    break;
                case 10:
                    milestone.m_MapTiles = 18;
                    break;
                case 11:
                    milestone.m_MapTiles = 22;
                    break;
                case 12:
                    milestone.m_MapTiles = 25;
                    break;
                case 13:
                    milestone.m_MapTiles = 29;
                    break;
                case 14:
                    milestone.m_MapTiles = 34;
                    break;
                case 15:
                    milestone.m_MapTiles = 38;
                    break;
                case 16:
                    milestone.m_MapTiles = 43;
                    break;
                case 17:
                    milestone.m_MapTiles = 49;
                    break;
                case 18:
                    milestone.m_MapTiles = 55;
                    break;
                case 19:
                    milestone.m_MapTiles = 61;
                    break;
                case 20:
                    // Allocate remaining tiles (68 for 529).
                    int remainingTiles = totalTitles - 461;
                    milestone.m_MapTiles = math.max(0, remainingTiles);
                    break;
            }
        }

        /// <summary>
        /// Custom component to indicate a custom milestone.
        /// </summary>
        private struct CustomMilestone : IComponentData
        {
        }
    }
}
