using System;
using System.Collections;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace Mapify.Components
{
    /// <summary>
    /// See StationProceduralJobsControllerPatch
    /// </summary>
    public class CustomJobsController: MonoBehaviour
    {
        private StationProceduralJobsController vanillaJobController;

        public void Setup(StationProceduralJobsController aVanillaController)
        {
            vanillaJobController = aVanillaController;
        }

        /// <summary>
        /// Same as StationProceduralJobsController.GenerateProceduralJobsCoro but allows for more jobs to be generated
        /// </summary>
        public IEnumerator GenerateProceduralJobsCoro()
        {
            var generateJobsAttempts = 0;
            var forcePlayerLicensedJobGeneration = true;
            var log = new StringBuilder();

            Mapify.Log(vanillaJobController.stationController.stationInfo.YardID + " job generation started");

            var jobsCapacity = vanillaJobController.generationRuleset.jobsCapacity;
            // The base game has LICENSED_JOBS_GENERATION_ATTEMPTS set to 1/3 of JOB_GENERATION_ATTEMPTS so I'm going to do the same
            var licensedJobGenerationAttempts = (int)Mathf.Ceil(jobsCapacity/3f);

            while (vanillaJobController.stationController.logicStation.availableJobs.Count < vanillaJobController.generationRuleset.jobsCapacity && generateJobsAttempts < jobsCapacity)
            {
                yield return WaitFor.FixedUpdate;

                if (generateJobsAttempts > licensedJobGenerationAttempts & forcePlayerLicensedJobGeneration)
                {
                    log.AppendLine("Couldn't generate any player licensed job");
                    forcePlayerLicensedJobGeneration = false;
                }

                var tickCount = Environment.TickCount;
                var jobChain = vanillaJobController.procJobGenerator.GenerateJobChain(new Random(tickCount), forcePlayerLicensedJobGeneration);

                // StationProceduralJobsController.JobGenerationAttempt can't be accessed here. These lines aren't important anyway (they're only for showing a notification in the game console) so I'm leaving them out

                // Action generationAttempt = vanillaJobController.JobGenerationAttempt;
                // if (generationAttempt != null)
                //     generationAttempt();

                if (jobChain != null)
                {
                    if (forcePlayerLicensedJobGeneration)
                        forcePlayerLicensedJobGeneration = false;
                    log.AppendLine($"Generated {jobChain.jobChainGO.name} ({jobChain.currentJobInChain.ID}) | rng seed: {tickCount}");

                    for (var i = 0; i < StationProceduralJobsController.WAIT_FRAMES_BETWEEN_JOBS; ++i)
                        yield return null;
                }
                else
                {
                    ++generateJobsAttempts;
                    yield return null;
                }
            }

            Mapify.Log(log.ToString());

            //STATIONID: generated X jobs
            Mapify.Log($"{vanillaJobController.stationController.stationInfo.YardID}: generated {vanillaJobController.stationController.logicStation.availableJobs.Count} jobs");

            vanillaJobController.generationCoro = null;
        }
    }
}
