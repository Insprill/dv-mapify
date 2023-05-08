using System.Collections.Generic;
using System.Reflection;
using DV;
using HarmonyLib;
using Mapify.Editor;
using MeshXtensions;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Mapify.Patches
{
    [HarmonyPatch(typeof(RailwayMeshGenerator), "ScheduleGenerateBaseAndRail")]
    public class RailwayMeshGenerator_ScheduleGenerateBaseAndRail_Patch
    {
        private static readonly FieldInfo RailwayMeshGenerator_Field_activeJobs = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "activeJobs");
        private static readonly FieldInfo RailwayMeshGenerator_Field_gravelShapePoints = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "gravelShapePoints");
        private static readonly FieldInfo RailwayMeshGenerator_Field_leftRailShapePoints = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "leftRailShapePoints");
        private static readonly FieldInfo RailwayMeshGenerator_Field_rightRailShapePoints = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "rightRailShapePoints");

        private static bool cachedValues;
        private static Vector2[] gravelShapePoints;
        private static Vector2[] leftRailShapePoints;
        private static Vector2[] rightRailShapePoints;
        private static List<(MeshSweeperJob, JobHandle, Mesh)> activeJobs;

        private static bool Prefix(RailwayMeshGenerator __instance, TrackChunk chunk)
        {
            if (!cachedValues)
            {
                gravelShapePoints = (Vector2[])RailwayMeshGenerator_Field_gravelShapePoints.GetValue(__instance);
                leftRailShapePoints = (Vector2[])RailwayMeshGenerator_Field_leftRailShapePoints.GetValue(__instance);
                rightRailShapePoints = (Vector2[])RailwayMeshGenerator_Field_rightRailShapePoints.GetValue(__instance);
                activeJobs = (List<(MeshSweeperJob, JobHandle, Mesh)>)RailwayMeshGenerator_Field_activeJobs.GetValue(__instance);
                cachedValues = true;
            }

            Vector3 position = chunk.track.transform.position;
            Vector3 globalOffset = -position;

            TrackChunkPoolObject ballastPoolObject = null;
            Track track = chunk.track.GetComponent<Track>();
            if (track == null || track.generateBallast)
            {
                ballastPoolObject = TrackChunkPoolObject.TakeFromPool(__instance.parent, position);
                ballastPoolObject.SetMaterial(__instance.baseMat);
                BaseType baseType = chunk.track.baseType;
                UVType basePathUv = baseType.basePathUV;
                float basePathUvScale = baseType.basePathUVScale;
                UVType baseShapeUv = baseType.baseShapeUV;
                float baseShapeUvScale = baseType.baseShapeUVScale;
                MeshSweeperJob ballastMeshSweeper = new MeshSweeperJob(chunk.pointSet, chunk.minIndex, chunk.maxIndex, globalOffset, gravelShapePoints, basePathUv, basePathUvScale, baseShapeUv, baseShapeUvScale);
                activeJobs.Add((ballastMeshSweeper, ballastMeshSweeper.ScheduleSelf(), ballastPoolObject.mesh));
            }

            TrackChunkPoolObject leftRailPoolObject = TrackChunkPoolObject.TakeFromPool(__instance.parent, position);
            TrackChunkPoolObject rightRailPoolObject = TrackChunkPoolObject.TakeFromPool(__instance.parent, position);
            chunk.AssignPoolObjects(ballastPoolObject, leftRailPoolObject, rightRailPoolObject);
            leftRailPoolObject.SetMaterial(__instance.railMat);
            rightRailPoolObject.SetMaterial(__instance.railMat);
            MeshSweeperJob leftRailMeshSweeper = new MeshSweeperJob(chunk.pointSet, chunk.minIndex, chunk.maxIndex, globalOffset, leftRailShapePoints);
            MeshSweeperJob rightRailMeshSweeper = new MeshSweeperJob(chunk.pointSet, chunk.minIndex, chunk.maxIndex, globalOffset, rightRailShapePoints);
            activeJobs.Add((leftRailMeshSweeper, leftRailMeshSweeper.ScheduleSelf(), leftRailPoolObject.mesh));
            activeJobs.Add((rightRailMeshSweeper, rightRailMeshSweeper.ScheduleSelf(), rightRailPoolObject.mesh));
            return false;
        }
    }

    [HarmonyPatch(typeof(RailwayMeshGenerator), "UpdateSleepersData")]
    public class RailwayMeshGenerator_UpdateSleepersData_Patch
    {
        private static readonly FieldInfo RailwayMeshGenerator_Field_sleepersHandle = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "sleepersHandle");
        private static readonly FieldInfo RailwayMeshGenerator_Field_sleepersAnchorsTransformBufferData = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "sleepersAnchorsTransformBufferData");
        private static readonly FieldInfo RailwayMeshGenerator_Field_sleepersAnchorsPositions = AccessTools.DeclaredField(typeof(RailwayMeshGenerator), "sleepersAnchorsPositions");

        private static bool cachedValues;
        private static NativeList<float> sleepersAnchorsTransformBufferData;
        private static NativeList<Vector3> sleepersAnchorsPositions;

        private static bool Prefix(RailwayMeshGenerator __instance, TrackChunk chunk)
        {
            if (!cachedValues)
            {
                sleepersAnchorsTransformBufferData = (NativeList<float>)RailwayMeshGenerator_Field_sleepersAnchorsTransformBufferData.GetValue(__instance);
                sleepersAnchorsPositions = (NativeList<Vector3>)RailwayMeshGenerator_Field_sleepersAnchorsPositions.GetValue(__instance);
                cachedValues = true;
            }

            Track track = chunk.track.GetComponent<Track>();
            if (track != null && !track.generateSleepers)
                return false;

            JobHandle sleepersHandle = (JobHandle)RailwayMeshGenerator_Field_sleepersHandle.GetValue(__instance);
            sleepersHandle = new PlaceSleepersAppendJob(
                sleepersAnchorsTransformBufferData,
                sleepersAnchorsPositions,
                chunk.pointSet,
                chunk.minIndex,
                chunk.maxIndex,
                chunk.track.baseType.randomizeAnchorDirection,
                chunk.track.baseType.sleeperVerticalOffset
            ).Schedule(sleepersHandle);
            RailwayMeshGenerator_Field_sleepersHandle.SetValue(__instance, sleepersHandle);
            return false;
        }
    }
}
