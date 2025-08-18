using System.Collections.Generic;
using DV;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Map;
using MeshXtensions;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(RailwayMeshGenerator), nameof(RailwayMeshGenerator.ScheduleGenerateBaseAndRail))]
    public class RailwayMeshGenerator_ScheduleGenerateBaseAndRail_Patch
    {
        private static bool Prefix(RailwayMeshGenerator __instance, TrackChunk chunk, List<(MeshSweeperJob, JobHandle, Mesh)> ___activeJobs, Vector2[] ___gravelShapePoints, Vector2[] ___leftRailShapePoints,
            Vector2[] ___rightRailShapePoints)
        {
            if (Maps.IsDefaultMap)
                return true;

            Vector3 position = chunk.track.transform.position;
            Vector3 globalOffset = -position;

            TrackChunkPoolObject ballastPoolObject = null;
            if (chunk.track.GetComponent<Track>().generateBallast)
            {
                ballastPoolObject = TrackChunkPoolObject.TakeFromPool(__instance.parent, position);
                ballastPoolObject.SetMaterial(__instance.baseMat);
                BaseType baseType = chunk.track.baseType;
                UVType basePathUv = baseType.basePathUV;
                float basePathUvScale = baseType.basePathUVScale;
                UVType baseShapeUv = baseType.baseShapeUV;
                float baseShapeUvScale = baseType.baseShapeUVScale;
                MeshSweeperJob ballastMeshSweeper = new MeshSweeperJob(chunk.pointSet, chunk.minIndex, chunk.maxIndex, globalOffset, ___gravelShapePoints, basePathUv, basePathUvScale, baseShapeUv, baseShapeUvScale);
                ___activeJobs.Add((ballastMeshSweeper, ballastMeshSweeper.ScheduleSelf(), ballastPoolObject.mesh));
            }

            TrackChunkPoolObject leftRailPoolObject = TrackChunkPoolObject.TakeFromPool(__instance.parent, position);
            TrackChunkPoolObject rightRailPoolObject = TrackChunkPoolObject.TakeFromPool(__instance.parent, position);
            chunk.AssignPoolObjects(ballastPoolObject, leftRailPoolObject, rightRailPoolObject);
            leftRailPoolObject.SetMaterial(__instance.railMat);
            rightRailPoolObject.SetMaterial(__instance.railMat);
            MeshSweeperJob leftRailMeshSweeper = new MeshSweeperJob(chunk.pointSet, chunk.minIndex, chunk.maxIndex, globalOffset, ___leftRailShapePoints);
            MeshSweeperJob rightRailMeshSweeper = new MeshSweeperJob(chunk.pointSet, chunk.minIndex, chunk.maxIndex, globalOffset, ___rightRailShapePoints);
            ___activeJobs.Add((leftRailMeshSweeper, leftRailMeshSweeper.ScheduleSelf(), leftRailPoolObject.mesh));
            ___activeJobs.Add((rightRailMeshSweeper, rightRailMeshSweeper.ScheduleSelf(), rightRailPoolObject.mesh));
            return false;
        }
    }

    [HarmonyPatch(typeof(RailwayMeshGenerator), nameof(RailwayMeshGenerator.UpdateSleepersData))]
    public class RailwayMeshGenerator_UpdateSleepersData_Patch
    {
        private static bool Prefix(TrackChunk chunk)
        {
            if (Maps.IsDefaultMap) return true;

            var track = chunk.track.GetComponent<Track>();
            return track.generateSleepers;
        }
    }
}
