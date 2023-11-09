// <copyright file="MapTilePurchaseSystemPatches.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// </copyright>

namespace FiveTwentyNineTiles
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;
    using Game.Simulation;
    using HarmonyLib;
    using Unity.Mathematics;

    /// <summary>
    /// Harmony patches for <see cref="MapTilePurchaseSystem"/> to implement per-tile cost limits.
    /// </summary>
    [HarmonyPatch(typeof(MapTilePurchaseSystem))]
    internal static class MapTilePurchaseSystemPatches
    {
        /// <summary>
        /// Harmony transpiler for <c>MapTilePurchaseSystem.UpdateStatus</c> to override the "insufficient funds" cost check.
        /// </summary>
        /// <param name="instructions">Original ILCode.</param>
        /// <param name="original">Method being patched.</param>
        /// <returns>Modified ILCode.</returns>
        [HarmonyPatch("UpdateStatus")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> UpdateStatusTranspiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
        {
            Logging.LogInfo("transpiling ", original.DeclaringType, '.', original.Name);

            // Parse instructions.
            IEnumerator<CodeInstruction> instructionEnumerator = instructions.GetEnumerator();
            while (instructionEnumerator.MoveNext())
            {
                CodeInstruction instruction = instructionEnumerator.Current;

                // Look for ldloc.s 5 followed by add (only instance in target).
                if (instruction.opcode == OpCodes.Ldloc_S && instruction.operand is LocalBuilder localBuilder && localBuilder.LocalIndex == 5)
                {
                    Logging.LogDebug("found ldloc.s 5");
                    yield return instruction;

                    // Check for following add.
                    instructionEnumerator.MoveNext();
                    instruction = instructionEnumerator.Current;
                    if (instruction.opcode == OpCodes.Add)
                    {
                        Logging.LogDebug("found add");
                        yield return instruction;

                        // Insert call to math.min(x, 441).
                        yield return new CodeInstruction(OpCodes.Ldc_I4, 441);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(math), nameof(math.min), new Type[] { typeof(int), typeof(int) }));

                        continue;
                    }
                }

                yield return instruction;
            }
        }
    }
}
