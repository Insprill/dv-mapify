using System.Collections.Generic;
using DV;
using DV.PointSet;
using HarmonyLib;
using Mapify.Editor;
using Mapify.Map;
using MeshXtensions;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Mapify.Patches
{
    //TODO implement this patch so Track.generateBallast works

    // [HarmonyPatch(typeof(RailwayMeshGenerator), nameof(RailwayMeshGenerator.ScheduleGenerateBaseAndRail))]
    // public class RailwayMeshGenerator_ScheduleGenerateBaseAndRail_Patch
    // {
    //     private static bool Prefix(TrackChunk chunk)
    //     {
    //         if (Maps.IsDefaultMap)
    //             return true;
    //
    //         Vector3 position = chunk.track.transform.position;
    //         Vector3 globalOffset = -position;
    //
    //         TrackChunkPoolObject ballastPoolObject = null;
    //         Track track = chunk.track.GetComponent<Track>();
    //         if (track == null || track.generateBallast)
    //         {
    //             ballastPoolObject = TrackChunkPoolObject.TakeFromPool(RailwayMeshGenerator.Instance.parent, position);
    //             ballastPoolObject.SetMaterial(RailwayMeshGenerator.Instance.baseMat);
    //             BaseType baseType = chunk.track.baseType;
    //             UVType basePathUv = baseType.basePathUV;
    //             float basePathUvScale = baseType.basePathUVScale;
    //             UVType baseShapeUv = baseType.baseShapeUV;
    //             float baseShapeUvScale = baseType.baseShapeUVScale;
    //             MeshSweeperJob ballastMeshSweeper = new MeshSweeperJob(chunk.pointSet, chunk.minIndex, chunk.maxIndex, globalOffset, RailwayMeshGenerator.Instance.gravelShapePoints, basePathUv, basePathUvScale, baseShapeUv, baseShapeUvScale);
    //             RailwayMeshGenerator.Instance.activeJobs.Add((ballastMeshSweeper, ballastMeshSweeper.ScheduleSelf(), ballastPoolObject.mesh));
    //         }
    //
    //         TrackChunkPoolObject leftRailPoolObject = TrackChunkPoolObject.TakeFromPool(RailwayMeshGenerator.Instance.parent, position);
    //         TrackChunkPoolObject rightRailPoolObject = TrackChunkPoolObject.TakeFromPool(RailwayMeshGenerator.Instance.parent, position);
    //         chunk.AssignPoolObjects(ballastPoolObject, leftRailPoolObject, rightRailPoolObject);
    //         leftRailPoolObject.SetMaterial(RailwayMeshGenerator.Instance.railMat);
    //         rightRailPoolObject.SetMaterial(RailwayMeshGenerator.Instance.railMat);
    //         MeshSweeperJob leftRailMeshSweeper = new MeshSweeperJob(chunk.pointSet, chunk.minIndex, chunk.maxIndex, globalOffset, RailwayMeshGenerator.Instance.leftRailShapePoints);
    //         MeshSweeperJob rightRailMeshSweeper = new MeshSweeperJob(chunk.pointSet, chunk.minIndex, chunk.maxIndex, globalOffset, RailwayMeshGenerator.Instance.rightRailShapePoints);
    //         RailwayMeshGenerator.Instance.activeJobs.Add((leftRailMeshSweeper, leftRailMeshSweeper.ScheduleSelf(), leftRailPoolObject.mesh));
    //         RailwayMeshGenerator.Instance.activeJobs.Add((rightRailMeshSweeper, rightRailMeshSweeper.ScheduleSelf(), rightRailPoolObject.mesh));
    //         return false;
    //     }
    // }

    //TODO generateSleepers
    // [HarmonyPatch(typeof(RailwayMeshGenerator), nameof(RailwayMeshGenerator.UpdateSleepersData))]
    // public class RailwayMeshGenerator_UpdateSleepersData_Patch
    // {
    //     private static bool Prefix(RailwayMeshGenerator __instance, TrackChunk chunk)
    //     {
    //         if (__instance == null)
    //         {
    //             Mapify.Log("__instance is null");
    //         }
    //
    //         var track = chunk.track.GetComponent<Track>();
    //         if (track == null)
    //         {
    //             Mapify.Log("track is null");
    //         }
    //         if (!track.generateSleepers)
    //             return false;
    //
    //         __instance.sleepersHandle = new PlaceSleepersAppendJob(
    //             __instance.sleepersAnchorsTransformBufferData,
    //             __instance.sleepersAnchorsPositions,
    //             chunk.pointSet,
    //             chunk.minIndex,
    //             chunk.maxIndex,
    //             chunk.track.baseType.randomizeAnchorDirection,
    //             chunk.track.baseType.sleeperVerticalOffset
    //         ).Schedule(__instance.sleepersHandle);
    //
    //         return false;
    //     }
    // }
}
