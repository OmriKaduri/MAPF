﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Collections;

namespace CPF_experiment
{
    /// <summary>
    /// This class is responsible for running the experiments.
    /// </summary>
    public class Run : IDisposable
    {
        ////////debug
        // public static TextWriter resultsWriterdd;
        /////////////

        /// <summary>
        /// Delimiter character used when writing the results of the runs to the output file.
        /// </summary>
        public static readonly string RESULTS_DELIMITER = ",";

        public static readonly int SUCCESS_CODE = 1;

        public static readonly int FAILURE_CODE = 0;

        /// <summary>
        /// Number of random steps performed when generating a new problem instance for choosing a start-goal pair.
        /// </summary>
        public static int RANDOM_WALK_STEPS = 100000;

        /// <summary>
        /// Indicates the starting time in ms for timing the different algorithms.
        /// </summary>
        private double startTime;

        /// <summary>
        /// This hold an open stream to the results file.
        /// </summary>
        private TextWriter resultsWriter;

        /// <summary>
        /// EH: I introduced this variable so that debugging and experiments
        /// can have deterministic results.
        /// </summary>
        static public Random rand = new Random();

        /// <summary>
        /// Calls resultsWriter.Dispose()
        /// </summary>
        protected virtual void Dispose(bool dispose_managed)
        {
            if (dispose_managed)
            {
                if (this.resultsWriter != null)
                {
                    this.resultsWriter.Dispose();
                    this.resultsWriter = null;
                }
            }
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Open the results file for output. Currently the file is opened in append mode.
        /// </summary>
        /// <param name="fileName">The name of the results file</param>
        public void OpenResultsFile(string fileName)
        {
            this.resultsWriter = new StreamWriter(fileName, true); // 2nd argument indicates the "append" mode
        }

        /// <summary>
        /// Closes the results file.
        /// </summary>
        public void CloseResultsFile()
        {
            this.resultsWriter.Close();
        }

        /// <summary>
        /// all types of algorithms to be run
        /// </summary>
        List<ISolver> solvers;

        /// <summary>
        /// all types of heuristics used
        /// </summary>
        public List<HeuristicCalculator> heuristics; // FIXME: Make unpublic again later

        /// <summary>
        /// Counts the number of times each algorithm went out of time consecutively
        /// </summary>
        public int[] outOfTimeCounters;

        /// <summary>
        /// Construct with chosen algorithms.
        /// </summary>
        public Run()
        {
            this.watch = Stopwatch.StartNew();

            // Preparing the heuristics:
            heuristics = new List<HeuristicCalculator>();
            var sic = new SumIndividualCosts();
            heuristics.Add(sic);
            var astar = new ClassicAStar(sic);
            var cbs = new CBS_LocalConflicts(astar, astar, -1);
            var astar_with_od = new AStarWithOD(sic);
            var astar_with_partial_expansion = new AStarWithPartialExpansion(sic);
            var epea = new AStarWithPartialExpansion(sic);
            var macbsLocal5Epea = new CBS_LocalConflicts(astar, epea, 5);
            var macbsLocal50Epea = new CBS_LocalConflicts(astar, epea, 50);
            var cbsHeuristic = new CbsHeuristic(cbs, this);
            var cbsHeuristicNoSolve1 = new CbsHeuristic(cbs, this, false, 1);
            var cbsHeuristicNoSolve2 = new CbsHeuristic(cbs, this, false, 2);
            var cbsHeuristicNoSolve3 = new CbsHeuristic(cbs, this, false, 3);
            var cbsHeuristicNoSolve4 = new CbsHeuristic(cbs, this, false, 4);
            var cbsHeuristicNoSolve5 = new CbsHeuristic(cbs, this, false, 5);
            var cbsHeuristicNoSolve6 = new CbsHeuristic(cbs, this, false, 6);
            var cbsHeuristicSolve1 = new CbsHeuristic(cbs, this, true, 1);
            var cbsHeuristicSolve2 = new CbsHeuristic(cbs, this, true, 2);
            var cbsHeuristicSolve3 = new CbsHeuristic(cbs, this, true, 3);
            var cbsHeuristicSolve4 = new CbsHeuristic(cbs, this, true, 4);
            var cbsHeuristicSolve5 = new CbsHeuristic(cbs, this, true, 5);
            var cbsHeuristicSolve6 = new CbsHeuristic(cbs, this, true, 6);
            //var sicOrCbsh6 = new RandomChoiceOfHeuristic(cbsHeuristicSolve6, sic, 1.0 / 5);

            //var dynamicLazyCbsh = new DyanamicLazyCbsh(cbs, this, true);
            //heuristics.Add(dynamicLazyCbsh);

            //var dynamicLazyMacbsLocal5EpeaH = new DyanamicLazyCbsh(macbsLocal5Epea, this, true);
            //heuristics.Add(dynamicLazyMacbsLocal5EpeaH);

            //var dynamicLazyMacbsLocal50EpeaH = new DyanamicLazyCbsh(macbsLocal50Epea, this, true);
            //heuristics.Add(dynamicLazyMacbsLocal50EpeaH);

            //var dynamicLazyMacbsLocal5EpeaHForOracleMustBeLast = new DyanamicLazyCbsh(macbsLocal5Epea, this, true);
            //heuristics.Add(dynamicLazyMacbsLocal5EpeaHForOracleMustBeLast);

            // Preparing the solvers:
            solvers = new List<ISolver>();
            
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, -1, false, false, false, true, false, false, false)); // CBS/EPEA*
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, -1, false, true, false, true, false, false, true, 1)); // CBS/EPEA* with first-fit adoption 1 expansions max
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, -1, false, true, false, true, false, false, true, 256)); // CBS/EPEA* with first-fit adoption 256 expansions max
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, -1, false, true, false, true, false, false, true, int.MaxValue)); // CBS/EPEA* with first-fit adoption infinity expansions max
            // B is actually set according to the map in a hack elsewhere

            //IJCAI:

            //soldier: solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //soldier:             CBS_LocalConflicts.ConflictChoice.MOST_CONFLICTING, false, false)); // CBS/EPEA*
            
            //soldier: solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //soldier:     CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false)); // CBS/EPEA* Cardinal using MDDs

            //soldier: 
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 2, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false)); // MA-CBS(2)/EPEA*
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 4, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false)); // MA-CBS(4)/EPEA*
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 8, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false)); // MA-CBS(8)/EPEA*
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 16, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false)); // MA-CBS(16)/EPEA*
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 32, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false)); // MA-CBS(32)/EPEA*
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 64, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false)); // MA-CBS(32)/EPEA*
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 50, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false)); // MA-CBS(50)/EPEA*
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 128, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false)); // MA-CBS(128)/EPEA*
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 256, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false)); // MA-CBS(256)/EPEA*


            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(
            //    astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false), sic)); // MA-CBS(5)/EPEA* + ID.
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(
            //    astar, epea, 10, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false), sic)); // MA-CBS(10)/EPEA* + ID.
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(
            //    astar, epea, 25, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false), sic)); // MA-CBS(25)/EPEA* + ID.
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(
            //    astar, epea, 100, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false), sic)); // MA-CBS(100)/EPEA* + ID.
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(
            //    astar, epea, 150, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false), sic)); // MA-CBS(100)/EPEA* + ID.
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(
            //    astar, epea, 200, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false), sic)); // MA-CBS(100)/EPEA* + ID.
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(
            //    astar, epea, 300, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false), sic)); // MA-CBS(100)/EPEA* + ID.
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(
            //    astar, epea, 500, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, false), sic)); // MA-CBS(100)/EPEA* + ID.

            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(
            //        astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //        CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, true),
            //    sic)); // MA-CBS(B)/EPEA* + ID + restart

            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.MOST_CONFLICTING, false, false, int.MaxValue, false)); // MA-CBS(5)/EPEA*
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false)); // MA-CBS(5)/EPEA* Cardinal using MDDs
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.CARDINAL_LOOKAHEAD, false, false)); // MA-CBS(5)/EPEA* Cardinal not using MDDs

            //soldier: solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //    CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, true)); // MA-CBS(5)/EPEA* + restart
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 64, false, CBS_LocalConflicts.BypassStrategy.NONE, false,
            //            CBS_LocalConflicts.ConflictChoice.FIRST, false, false, int.MaxValue, true)); // MA-CBS(64)/EPEA* + restart

            //soldier: solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //soldier:     false, CBS_LocalConflicts.ConflictChoice.FIRST, false, false, 1)); // CBS/EPEA* + BP1
            //soldier:solvers.Add(new CBS_GlobalConflicts(astar, epea, -1, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //soldier:            false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1)); // CBS/EPEA* + CARDINAL + BP1

            //solvers.Add(new CBS_GlobalConflicts(astar, epea, -1, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, false)); // CBS + CARDINAL (lookahead) + BP1


            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, -1, false,
            //    CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, false), sic)); // CBS + CARDINAL + BP1 + ID

            //solvers.Add(new CBS_GlobalConflicts(astar, epea, -1, false, CBS_LocalConflicts.   ,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_LOOKAHEAD, false, false, 1)); // CBS/EPEA* Cardinal not using MDDs + BP1

            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.MOST_CONFLICTING, false, false, 1)); // MA-CBS(B)/EPEA* + BP1
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1)); // MA-CBS(B)/EPEA* Cardinal using MDDs + BP1
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_LOOKAHEAD, false, false, 1)); // MA-CBS(B)/EPEA* Cardinal not using MDDs + BP1

            //soldier: 
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 2, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(2)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 4, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(4)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(5)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 8, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(8)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 16, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(16)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 25, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD, false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(25)/EPEA* + cardinal + BP1 + restart, AKA ICBS(25)
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 32, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(32)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 50, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(50)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 64, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(64)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 128, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(128)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 256, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(256)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 512, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(512)/EPEA* + cardinal + BP1 + restart
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 1024, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(1024)/EPEA* + cardinal + BP1 + restart


            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true), sic)); // MA-CBS(5)/EPEA* + cardinal + BP1 + restart + ID
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 10, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true), sic)); // MA-CBS(10)/EPEA* + cardinal + BP1 + restart + ID
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 25, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true), sic)); // MA-CBS(25)/EPEA* + cardinal + BP1 + restart + ID
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 100, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true), sic)); // MA-CBS(100)/EPEA* + cardinal + BP1 + restart + ID
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 150, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true), sic)); // MA-CBS(100)/EPEA* + cardinal + BP1 + restart + ID
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 200, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true), sic)); // MA-CBS(100)/EPEA* + cardinal + BP1 + restart + ID
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 300, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true), sic)); // MA-CBS(100)/EPEA* + cardinal + BP1 + restart + ID
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 500, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true), sic)); // MA-CBS(100)/EPEA* + cardinal + BP1 + restart + ID


            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, CBS_LocalConflicts.BypassStrategy.FIRST_FIT_LOOKAHEAD,
            //    false, CBS_LocalConflicts.ConflictChoice.CARDINAL_MDD, false, false, 1, true)); // MA-CBS(B)/EPEA* Cardinal using MDDs + BP1 + restart

            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 5, false, true, false, CBS_LocalConflicts.ConflictChoice.MOST_CONFLICTING, false, false, true, int.MaxValue)); // MA-CBS/EPEA* with first-fit adoption infinity expansions max
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 1), sic)); // MA-CBS/EPEA* with first-fit adoption 1 expansions max + ID
            //solvers.Add(new CBS_GlobalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 256)); // MA-CBS/EPEA* with first-fit adoption 256 expansions max
            //solvers.Add(new IndependenceDetection(astar, new CBS_GlobalConflicts(astar, epea, 5, false, true, false, true, false, false, true, int.MaxValue), sic)); // MA-CBS/EPEA* with first-fit adoption infinity expansions max + ID

            //soldier: solvers.Add(epea); // EPEA*
            //soldier: solvers.Add(new CostTreeSearchSolverOldMatching(3)); // ICTS
            //solvers.Add(new IndependenceDetection(astar, epea, sic)); // EPEA* + ID
            //solvers.Add(new IndependenceDetection(astar, new CostTreeSearchSolverOldMatching(3), sic)); // ICTS + ID

            /*
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, false, false, true, false, false, false)); // MACBS(5)/EPEA* - Works and is very fast so is a good choice for cost validation
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 1)); // MACBS(5)/EPEA* + adoption immediately
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, int.MaxValue)); // MACBS(5)/EPEA* + adoption immediately infinite expansions
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 2)); // MACBS(5)/EPEA* + adoption immediately 2 expansions
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 4)); // MACBS(5)/EPEA* + adoption immediately 4 expansions
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 8)); // MACBS(5)/EPEA* + adoption immediately 8 expansions
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 16)); // MACBS(5)/EPEA* + adoption immediately 16 expansions
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 32)); // MACBS(5)/EPEA* + adoption immediately 32 expansions
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 64)); // MACBS(5)/EPEA* + adoption immediately 64 expansions
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 128)); // MACBS(5)/EPEA* + adoption immediately
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, 256)); // MACBS(5)/EPEA* + adoption immediately
            solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, true, int.MaxValue)); // MACBS(5)/EPEA* + adoption immediately infinite expansions
            */

            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, false)); // MACBS(5)/EPEA* + infinite lookahead adoption
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, false, 4)); // MACBS(5)/EPEA* + 4 lookahead adoption
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, false, 8)); // MACBS(5)/EPEA* + 8 lookahead adoption
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, false, 16)); // MACBS(5)/EPEA* + 16 lookahead adoption
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, false, 32)); // MACBS(5)/EPEA* + 32 lookahead adoption
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, true, false, true, false, false, false, 64)); // MACBS(5)/EPEA* + 64 lookahead adoption

            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, false, false, truesllslss, false, false, false)); // CBS/EPEA*
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, true, false, true, false, false)); // CBS+adoption
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, true, false, false, true, false, false)); // CBS+shuffle
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, true, false, true, false, true)); // CBS+MDD+adoption
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, true, false, false, true, false, true)); // CBS+MDD+shuffle
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, false, false, true, true)); // Simply tie break for more conflicts
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5, false, false, false, false)); // MACBS(5) Find first conflict
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, false, false, false, false, false)); // CBS Find first conflict
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1)); // Works and is very fast so is a good choice for cost validation
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, true, false, false)); // shuffle
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, true, false)); // Adoption
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, false, true)); // Malte
            
            //solvers.Add(new CBS_LocalConflicts(epea, epea, -1));
            //solvers.Add(new CBS_GlobalConflicts(epea, epea, -1)); // Should be identical since no merging is done.
            //solvers.Add(new CBS_LocalConflicts(epea, epea, 0));
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 0));
            //solvers.Add(new CBS_GlobalConflicts(epea, epea, 0));
            //solvers.Add(new CBS_LocalConflicts(epea, epea, 1));
            //solvers.Add(new CBS_GlobalConflicts(epea, epea, 1));
            //solvers.Add(new CBS_LocalConflicts(epea, epea, 5));
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 5));
            //solvers.Add(new CBS_GlobalConflicts(epea, epea, 5));
            //solvers.Add(new CBS_LocalConflicts(epea, epea, 10));
            //solvers.Add(new CBS_LocalConflicts(epea, epea, 100));
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 100));
            //solvers.Add(new CBS_GlobalConflicts(epea, epea, 100));
            //solvers.Add(new CBS_LocalConflicts(epea, epea, 500));
            //solvers.Add(new CBS_GlobalConflicts(epea, epea, 500));
            
            //solvers.Add(new CBS_GlobalConflicts(astar_with_od, astar_with_od, -1)); // Should be identical since no merging is done.
            //solvers.Add(new CBS_LocalConflicts(astar_with_od, astar_with_od, 0));
            //solvers.Add(new CBS_LocalConflicts(astar, astar_with_od, 0));
            //solvers.Add(new CBS_GlobalConflicts(astar_with_od, astar_with_od, 0));
            //solvers.Add(new CBS_LocalConflicts(astar_with_od, astar_with_od, 1));
            //solvers.Add(new CBS_GlobalConflicts(astar_with_od, astar_with_od, 1));
            //solvers.Add(new CBS_LocalConflicts(astar_with_od, astar_with_od, 5));
            //solvers.Add(new CBS_LocalConflicts(astar, astar_with_od, 5));
            //solvers.Add(new CBS_GlobalConflicts(astar_with_od, astar_with_od, 5));
            //solvers.Add(new CBS_LocalConflicts(astar_with_od, astar_with_od, 10));
            //solvers.Add(new CBS_LocalConflicts(astar_with_od, astar_with_od, 100));
            //solvers.Add(new CBS_LocalConflicts(astar, astar_with_od, 100));
            //solvers.Add(new CBS_GlobalConflicts(astar_with_od, astar_with_od, 100));
            //solvers.Add(new CBS_LocalConflicts(astar_with_od, astar_with_od, 500));
            //solvers.Add(new CBS_GlobalConflicts(astar_with_od, astar_with_od, 500));

            //solvers.Add(new ClassicAStar(sic, true)); // Works
            //solvers.Add(new ClassicAStar(sic, true, false)); // rM*! Works
            //solvers.Add(new ClassicAStar(sic, true, true)); // rM* shuffle! Works
            //solvers.Add(new ClassicAStar(cbsHeuristic)); // Dont Work - problem with initiating 
            //solvers.Add(new AStarWithOD(sic));  // Works
            //solvers.Add(new AStarWithOD(sic, true, false)); // rM*+OD!
            //solvers.Add(new AStarWithOD(sic, true, true)); // rM*+OD shuffle!
            //solvers.Add(new AStarWithPartialExpansionBasic(sic)); // Works
            //solvers.Add(new AStarWithPartialExpansionBasic(cbsHeuristic));
            //soldier: solvers.Add(new AStarWithPartialExpansion(sic)); // Works.
            //solvers.Add(new AStarWithPartialExpansion(sic, true, false)); // EPErM*
            //solvers.Add(new AStarWithPartialExpansion(sic, true, true)); // EPErM* shuffle
            //soldier: solvers.Add(new CBS_LocalConflicts(astar, epea, 0)); // EPEA*+(S)ID

            //solvers.Add(new ClassicAStar(cbsHeuristicSolve1));
            //solvers.Add(new ClassicAStar(cbsHeuristicSolve2));
            //solvers.Add(new ClassicAStar(cbsHeuristicSolve3));
            //solvers.Add(new ClassicAStar(cbsHeuristicSolve4));
            //solvers.Add(new ClassicAStar(cbsHeuristicSolve5));
            //solvers.Add(new ClassicAStar(cbsHeuristicSolve6));
            //solvers.Add(new ClassicAStar(cbsHeuristicNoSolve1));
            //solvers.Add(new ClassicAStar(cbsHeuristicNoSolve2));
            //solvers.Add(new ClassicAStar(cbsHeuristicNoSolve3));
            //solvers.Add(new ClassicAStar(cbsHeuristicNoSolve4));
            //solvers.Add(new ClassicAStar(cbsHeuristicNoSolve5));
            //solvers.Add(new ClassicAStar(cbsHeuristicNoSolve6));
            //solvers.Add(new ClassicAStar(sicOrCbsh6));

            //solvers.Add(new AStarWithOD(cbsHeuristicSolve1));
            //solvers.Add(new AStarWithOD(cbsHeuristicSolve2));
            //solvers.Add(new AStarWithOD(cbsHeuristicSolve3));
            //solvers.Add(new AStarWithOD(cbsHeuristicSolve4));
            //solvers.Add(new AStarWithOD(cbsHeuristicSolve5));
            //solvers.Add(new AStarWithOD(cbsHeuristicSolve6));
            //solvers.Add(new AStarWithOD(cbsHeuristicNoSolve1));
            //solvers.Add(new AStarWithOD(cbsHeuristicNoSolve2));
            //solvers.Add(new AStarWithOD(cbsHeuristicNoSolve3));
            //solvers.Add(new AStarWithOD(cbsHeuristicNoSolve4));
            //solvers.Add(new AStarWithOD(cbsHeuristicNoSolve5));
            //solvers.Add(new AStarWithOD(cbsHeuristicNoSolve6));
            //solvers.Add(new AStarWithOD(sicOrCbsh6));

            ClassicAStar solver;
            // dynamic not rational lazy A*+OD/CBS/A*/SIC:
            //solver = new AStarWithOD(sic);
            //var dynamicLazyOpenList1 = new DynamicLazyOpenList(solver, dynamicLazyCbsh, this);
            //solver.openList = dynamicLazyOpenList1;
            //solvers.Add(solver);

            // dynamic rational lazy A*+OD/CBS/A*/SIC:
            //solver = new AStarWithOD(sic);
            //var dynamicRationalLazyOpenList1 = new DynamicRationalLazyOpenList(solver, dynamicLazyCbsh, this);
            //solver.openList = dynamicRationalLazyOpenList1;
            //solvers.Add(solver);

            // dynamic rational lazy MA-CBS-local-5/A*+OD/MA-CBS-local-5/EPEA*/SIC:
            //solver = new AStarWithOD(sic);
            //var dynamicRationalLazyOpenList3 = new DynamicRationalLazyOpenList(solver, dynamicLazyMacbsLocal5EpeaH, this);
            //solver.openList = dynamicRationalLazyOpenList3;
            //solvers.Add(new CBS_LocalConflicts(astar, solver, 5));

            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicSolve1));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicSolve2));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicSolve3));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicSolve4));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicSolve5));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicSolve6));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicNoSolve1));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicNoSolve2));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicNoSolve3));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicNoSolve4));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicNoSolve5));
            //solvers.Add(new AStarWithPartialExpansion(cbsHeuristicNoSolve6));
            //solvers.Add(new AStarWithPartialExpansion(sicOrCbsh6));

            // dynamic not rational lazy EPEA*/CBS/A*/SIC:
            //solver = new AStarWithPartialExpansion(sic);
            //var dynamicLazyOpenList2 = new DynamicLazyOpenList(solver, dynamicLazyCbsh, this);
            //solver.openList = dynamicLazyOpenList2;
            //solvers.Add(solver);

            // dynamic rational lazy EPEA*/CBS/A*/SIC:
            //solver = new AStarWithPartialExpansion(sic);
            //var dynamicRationalLazyOpenList2 = new DynamicRationalLazyOpenList(solver, dynamicLazyCbsh, this);
            //solver.openList = dynamicRationalLazyOpenList2;
            //solvers.Add(solver);

            /*
             * soldiers:
            // MA-CBS-local-5 / dynamic rational lazy EPEA* / MA-CBS-local-50 / EPEA* / SIC:
            solver = new AStarWithPartialExpansion(sic);
            var dynamicRationalLazyOpenList4 = new DynamicRationalLazyOpenList(solver, dynamicLazyMacbsLocal50EpeaH, this);
            solver.openList = dynamicRationalLazyOpenList4;
            solvers.Add(new CBS_LocalConflicts(astar, solver, 5));

            // dynamic rational lazy EPEA* / MA-CBS-local-5 / EPEA* / SIC + (S)ID:
            solver = new AStarWithPartialExpansion(sic);
            var dynamicRationalLazyOpenList6 = new DynamicRationalLazyOpenList(solver, dynamicLazyMacbsLocal5EpeaH, this);
            solver.openList = dynamicRationalLazyOpenList6;
            solvers.Add(new CBS_LocalConflicts(astar, solver, 0));
             */

            /*
            //soldier: but can't handle 50 agents
            // dynamic rational lazy EPEA* / MA-CBS-local-5 / EPEA* / SIC:
            solver = new AStarWithPartialExpansion(sic);
            var dynamicRationalLazyOpenList8 = new DynamicRationalLazyOpenList(solver, dynamicLazyMacbsLocal5EpeaH, this);
            solver.openList = dynamicRationalLazyOpenList8;
            solvers.Add(solver);
             */
            //solvers.Add(new CostTreeSearchSolverNoPruning());
            //solvers.Add(new CostTreeSearchSolverKMatch(2));
            //solvers.Add(new CostTreeSearchSolverOldMatching(2));
            //solvers.Add(new CostTreeSearchSolverRepeatedMatch(2));
            //solvers.Add(new CostTreeSearchSolverKMatch(3));
            ////!@# USE ME 
            //solvers.Add(new CostTreeSearchSolverOldMatching(3)); // Use this parameter. Best according to paper. 3RE
            //solvers.Add(new CostTreeSearchSolverRepeatedMatch(3));

            //solvers.Add(new CostTreeSearchNoPruning());
            //solvers.Add(new CostTreeSearchKMatch(2));
            //solvers.Add(new CostTreeSearchOldMatching(2));
            //solvers.Add(new CostTreeSearchRepatedMatch(2));
            //solvers.Add(new CostTreeSearchKMatch(3));
            //solvers.Add(new CostTreeSearchOldMatching(3));
            //solvers.Add(new CostTreeSearchRepatedMatch(3));

            //solvers.Add(new IndependenceDetection(new AStarWithPartialExpansion()));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new AStarWithPartialExpansion(), 1, 1)));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new AStarWithPartialExpansion(), 5, 5)));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new AStarWithPartialExpansion(), 10, 10)));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new AStarWithPartialExpansion(), 100, 100)));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new AStarWithPartialExpansion(), 500, 500)));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new ClassicAStar())));

            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new ClassicAStar(), 1, 1)));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new ClassicAStar(), 5, 5)));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new ClassicAStar(), 10, 10)));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new ClassicAStar(), 100, 100)));
            //solvers.Add(new IndependenceDetection(new CBS_GlobalConflicts(new ClassicAStar(), 500, 500)));

            //solvers.Add(new IndependenceDetection(new AStarWithPartialExpansionBasic()));
            //solvers.Add(new IndependenceDetection(new AStarWithPartialExpansion()));
            //solvers.Add(new IndependenceDetection(sic));
            //solvers.Add(new IndependenceDetection());

            //solvers.Add(new CBS_IDA(new ClassicAStar())); // Don't run! Uses must conds

            //solvers.Add(new CBS_GlobalConflicts(new ClassicAStar())); // Works

            //solvers.Add(new CBS_NoDD(new ClassicAStar()));
            //solvers.Add(new CBS_NoDDb3(new ClassicAStar()));
            //solvers.Add(new CBS_GlobalConflicts(new ClassicAStar(), 1, 1)); // Run this!


            //solvers.Add(astar);
            //solvers.Add(astar_with_od);
            //solvers.Add(astar_with_partial_expansion);

            solvers.Add(new IndependenceDetection(sic)); // AStar + OD + ID

            // MA-CBS-Global-10/(A*+OD/SIC) choosing the first conflict in CBS nodes
            //solvers.Add(new CBS_GlobalConflicts(astar_with_od, astar_with_od, 10));

            // MA-CBS-Local-10/(single:A*/SIC multi:A*+OD/SIC) choosing the first conflict in CBS nodes
            //solvers.Add(new CBS_LocalConflicts(astar, astar_with_od, 10));

            // Basic-CBS/(A*+OD/SIC) choosing the first conflict in CBS nodes
            //solvers.Add(new CBS_LocalConflicts(astar_with_od, astar_with_od, -1));

            // MA - CBS - Global - 10 / (EPEA */ SIC) choosing the first conflict in CBS nodes
            solvers.Add(new CBS_GlobalConflicts(epea, epea, 10));

            // MA - CBS - Local - 10 / (single: A */ SIC multi: EPEA */ SIC) choosing the first conflict in CBS nodes
            //solvers.Add(new CBS_LocalConflicts(astar, epea, 10));

            // Basic-CBS/(A*/SIC) choosing cardinal conflicts using lookahead
            //solvers.Add(new CBS_LocalConflicts(astar, epea, -1, false, CBS_LocalConflicts.BypassStrategy.NONE, false, CBS_LocalConflicts.ConflictChoice.CARDINAL_LOOKAHEAD, false, false));

            // Basic-CBS/(A*/SIC)+ID - Use ID, for single agent solve vith A*, for groups use Basic-CBS which will use A* (again) for single agent, and epea* for groups
            solvers.Add(new IndependenceDetection(astar, new CBS_LocalConflicts(astar, epea, -1, false, CBS_LocalConflicts.BypassStrategy.NONE, false, CBS_LocalConflicts.ConflictChoice.MOST_CONFLICTING, false, false), sic));

            // ICTS 2RE
            //solvers.Add(new CostTreeSearchSolverRepeatedMatch(2));

            // ICTS + ID 
            solvers.Add(new IndependenceDetection(astar, new CostTreeSearchSolverOldMatching(3), sic));

            // EPEA* + ID
            solvers.Add(new IndependenceDetection(astar, astar_with_partial_expansion, sic));

            outOfTimeCounters = new int[solvers.Count];
            for (int i = 0; i < outOfTimeCounters.Length; i++)
            {
                outOfTimeCounters[i] = 0;
            }
        }

        /// <summary>
        /// Generates a problem instance, including a board, start and goal locations of desired number of agents
        /// and desired precentage of obstacles
        /// TODO: Refactor to use operators.
        /// </summary>
        /// <param name="gridSize"></param>
        /// <param name="agentsNum"></param>
        /// <param name="obstaclesNum"></param>
        /// <returns></returns>
        public ProblemInstance GenerateProblemInstance(int gridSize, int agentsNum, int obstaclesNum)
        {
            /**
             * Randomization based on timer is disabled for purposes of getting
             * reproducible experiments.
             */
            //Random rand = new Random();
            Debug.WriteLine(string.Format("Generating instance with {0} agents, {1} obstacles of size {2}", agentsNum, obstaclesNum, gridSize));
            if (agentsNum + obstaclesNum + 1 > gridSize * gridSize)
                throw new Exception("Not enough room for " + agentsNum + ", " + obstaclesNum + " and one empty space in a " + gridSize + "x" + gridSize + "map.");

            int x;
            int y;
            Agent[] aGoals = new Agent[agentsNum];
            AgentState[] aStart = new AgentState[agentsNum];
            bool[][] grid = new bool[gridSize][];
            bool[][] goals = new bool[gridSize][];


            for (int i = 0; i < gridSize; i++)
            {
                grid[i] = new bool[gridSize];
                goals[i] = new bool[gridSize];
            }

            // Generate random obstacles on grid
            for (int i = 0; i < obstaclesNum; i++)
            {
                x = rand.Next(gridSize);
                y = rand.Next(gridSize);
                if (grid[x][y]) // Already an obstacle
                    i--;
                grid[x][y] = true;
            }

            // Choose random goal locations
            for (int i = 0; i < agentsNum; i++)
            {
                x = rand.Next(gridSize);
                y = rand.Next(gridSize);
                if (goals[x][y] || grid[x][y]) // Already an obstacle or goal
                    i--;
                else
                {
                    goals[x][y] = true;
                    aGoals[i] = new Agent(x, y, i);
                }
            }

            // Select random start/goal locations for every agent by performing a random walk
            for (int i = 0; i < agentsNum; i++)
            {
                aStart[i] = new AgentState(aGoals[i].Goal.x, aGoals[i].Goal.y, aGoals[i]);
            }

            // Initialized here only for the IsValid() call. TODO: Think how this can be sidestepped elegantly.
            ProblemInstance problem = new ProblemInstance();
            problem.Init(aStart, grid);
            
            for (int j = 0; j < RANDOM_WALK_STEPS; j++)
            {
                for (int i = 0; i < agentsNum; i++)
                {
                    goals[aStart[i].lastMove.x][aStart[i].lastMove.y] = false; // We're going to move the goal somewhere else
                    while (true)
                    {
                        Move.Direction op = (Move.Direction)rand.Next(0, 5); // TODO: fixme
                        aStart[i].lastMove.Update(op);
                        if (problem.IsValid(aStart[i].lastMove) &&
                            !goals[aStart[i].lastMove.x][aStart[i].lastMove.y]) // this spot isn't another agent's goal
                            break;
                        else
                            aStart[i].lastMove.setOppositeMove(); // Rollback
                    }
                    goals[aStart[i].lastMove.x][aStart[i].lastMove.y] = true; // Claim agent's new goal
                }
            }

            // Zero the agents' timesteps
            foreach (AgentState agentStart in aStart) 
            {
                agentStart.lastMove.time = 0;
            }

            // TODO: There is some repetition here of previous instantiation of ProblemInstance. Think how to elegantly bypass this.
            problem = new ProblemInstance();
            problem.Init(aStart, grid);
            return problem;  
        }

        /// <summary>
        /// Generates a problem instance based on a DAO map file.
        /// TODO: Fix code dup with GenerateProblemInstance and Import later.
        /// </summary>
        /// <param name="agentsNum"></param>
        /// <returns></returns>
        public ProblemInstance GenerateDragonAgeProblemInstance(string mapFileName, int agentsNum)
        {
            Debug.WriteLine(string.Format("Generating instance with {0} agents", agentsNum));
            TextReader input = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), mapFileName));
            //"C:\\Users\\omri\\Projects\\study\\2019semB\\agentsPlanning\\MAPF\\bin\\Debug\\dao_maps\\den502d.map");
            string[] lineParts;
            string line;

            line = input.ReadLine();
            Debug.Assert(line.StartsWith("type octile"));

            // Read grid dimensions
            line = input.ReadLine();
            lineParts = line.Split(' ');
            Debug.Assert(lineParts[0].StartsWith("height"));
            int maxX = int.Parse(lineParts[1]);
            line = input.ReadLine();
            lineParts = line.Split(' ');
            Debug.Assert(lineParts[0].StartsWith("width"));
            int maxY = int.Parse(lineParts[1]);
            line = input.ReadLine();
            Debug.Assert(line.StartsWith("map"));
            bool[][] grid = new bool[maxX][];
            char cell;
            for (int i = 0; i < maxX; i++)
            {
                grid[i] = new bool[maxY];
                line = input.ReadLine();
                for (int j = 0; j < maxY; j++)
                {
                    cell = line.ElementAt(j);
                    if (cell == '@' || cell == 'O' || cell == 'T' || cell == 'W' /* Water isn't traversable from land */)
                        grid[i][j] = true;
                    else
                        grid[i][j] = false;
                }
            }

            int x;
            int y;
            Agent[] agentGoals = new Agent[agentsNum];
            AgentState[] agentStates = new AgentState[agentsNum];
            bool[][] goals = new bool[maxX][];

            for (int i = 0; i < maxX; i++)
                goals[i] = new bool[maxY];

            // Choose random valid unclaimed goal locations
            for (int i = 0; i < agentsNum; i++)
            {
                x = rand.Next(maxX);
                y = rand.Next(maxY);
                if (goals[x][y] || grid[x][y])
                    i--;
                else
                {
                    goals[x][y] = true;
                    agentGoals[i] = new Agent(x, y, i);
                }
            }

            // Select random start/goal locations for every agent by performing a random walk
            for (int i = 0; i < agentsNum; i++)
            {
                agentStates[i] = new AgentState(agentGoals[i].Goal.x, agentGoals[i].Goal.y, agentGoals[i]);
            }

            ProblemInstance problem = new ProblemInstance();
            problem.parameters[ProblemInstance.GRID_NAME_KEY] = Path.GetFileNameWithoutExtension(mapFileName);
            problem.Init(agentStates, grid);

            for (int j = 0; j < RANDOM_WALK_STEPS; j++)
            {
                for (int i = 0; i < agentsNum; i++)
                {
                    goals[agentStates[i].lastMove.x][agentStates[i].lastMove.y] = false; // We're going to move the goal somewhere else
                    // Move in a random legal direction:
                    while (true)
                    {
                        Move.Direction op = (Move.Direction)rand.Next(0, 5); // TODO: fixme
                        agentStates[i].lastMove.Update(op);
                        if (problem.IsValid(agentStates[i].lastMove) &&
                            !goals[agentStates[i].lastMove.x][agentStates[i].lastMove.y]) // This spot isn't another agent's goal
                            break;
                        else
                            agentStates[i].lastMove.setOppositeMove(); // Rollback
                    }
                    goals[agentStates[i].lastMove.x][agentStates[i].lastMove.y] = true; // Claim agent's new goal
                }
            }

            // Zero the agents' timesteps
            foreach (AgentState agentStart in agentStates)
                agentStart.lastMove.time = 0;

            return problem;
        }

        /// <summary>
        /// Solve given instance with a list of algorithms 
        /// </summary>
        /// <param name="instance">The instance to solve</param>
        public bool SolveGivenProblem(ProblemInstance instance)
        {
            //return; // add for generator
            // Preparing a list of agent indices (not agent nums) for the heuristics' Init() method
            List<uint> agentList = Enumerable.Range(0, instance.m_vAgents.Length).Select<int, uint>(x=> (uint)x).ToList<uint>(); // FIXME: Must the heuristics really receive a list of uints?

            // Solve using the different algorithms
            Debug.WriteLine("Solving " + instance);
            this.PrintProblemStatistics(instance);
            //double cr0 = instance.getConflictRation(0);
            //double cr1 = instance.getConflictRation(1);

            //Debug.WriteLine("Conflict ratio (first order): " + cr0);
            //Debug.WriteLine("Conflict ratio (second order): " + cr1);
            //this.resultsWriter.Write(cr0 + RESULTS_DELIMITER);
            //this.resultsWriter.Write(cr1 + RESULTS_DELIMITER);

            // Initializing all heuristics, whereever they're used
            for (int i = 0; i < heuristics.Count; i++)
                heuristics[i].init(instance, agentList);

            int solutionCost = -1;
            int firstSolverToSolveIndex = -1;

            for (int i = 0; i < solvers.Count; i++)
            {
                if (outOfTimeCounters[i] < Constants.MAX_FAIL_COUNT) // After "MAX_FAIL_COUNT" consecutive failures of a given algorithm we stop running it.
                                                                    // Assuming problem difficulties are non-decreasing, if it consistently failed on several problems it won't suddenly succeed in solving the next problem.
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    //if (i != 2)
                    //    continue;
                    //if (i == 1)
                    //    ((ClassicAStar)solvers[i]).debug = true;
                    //if (i == 4)
                    //    ((CBS_LocalConflicts)solvers[i]).debug = true;
                    //if (i == 4)
                    //    ((CBS_LocalConflicts)((IndependenceDetection)solvers[i]).groupSolver).debug = true;
                    if (solvers[i].GetType() == typeof(CBS_LocalConflicts) || solvers[i].GetType() == typeof(CBS_GlobalConflicts))
                    {
                        if (((CBS_LocalConflicts)solvers[i]).mergeThreshold == 314159) // MAGIC NUMBER WHICH MAKES US ADJUST B according to map
                        {
                            string gridName = (string)instance.parameters[ProblemInstance.GRID_NAME_KEY];
                            if (gridName.StartsWith("den"))
                                ((CBS_LocalConflicts)solvers[i]).mergeThreshold = 10;
                            else if (gridName.StartsWith("brc") || gridName.StartsWith("ost"))
                                ((CBS_LocalConflicts)solvers[i]).mergeThreshold = 100;
                        }
                    }


                    if (
                        (solvers[i].GetType() == typeof(IndependenceDetection) &&
                         ((IndependenceDetection)solvers[i]).groupSolver.GetType() == typeof(CBS_LocalConflicts)) ||
                        (solvers[i].GetType() == typeof(IndependenceDetection) &&
                         ((IndependenceDetection)solvers[i]).groupSolver.GetType() == typeof(CBS_GlobalConflicts))
                       )
                    {
                        if (((CBS_LocalConflicts)((IndependenceDetection)solvers[i]).groupSolver).mergeThreshold == 314159) // MAGIC NUMBER SEE ABOVE
                        {
                            string gridName = (string)instance.parameters[ProblemInstance.GRID_NAME_KEY];
                            if (gridName.StartsWith("den"))
                                ((CBS_LocalConflicts)((IndependenceDetection)solvers[i]).groupSolver).mergeThreshold = 10;
                            else if (gridName.StartsWith("brc") || gridName.StartsWith("ost"))
                                ((CBS_LocalConflicts)((IndependenceDetection)solvers[i]).groupSolver).mergeThreshold = 100;
                        }
                    }

                    this.run(solvers[i], instance);

                    Console.WriteLine();

                    int solverSolutionCost = solvers[i].GetSolutionCost();

                    if (solverSolutionCost >= 0) // Solved successfully
                    {
                        Plan plan = solvers[i].GetPlan();
                        int planSize = plan.GetSize();
                        if (planSize < 200)
                            plan.PrintPlan();
                        else
                            Console.WriteLine("Plan is too long to print (" + planSize + " steps).");
                        outOfTimeCounters[i] = 0;

                        // Validate solution:
                        if (solutionCost == -1) // Record solution cost
                        {
                            solutionCost = solverSolutionCost;
                            firstSolverToSolveIndex = i;
                        }
                        else // Problem solved before
                        {
                            //Debug.Assert(solutionCost == solverSolutionCost,
                            //    solvers[firstSolverToSolveIndex] + " solution cost is different than that of " + solvers[i]); // Assuming algs are supposed to find an optimal solution, this is an error.
                            //Debug.Assert(solvers[0].GetExpanded() == solvers[i].GetExpanded(), "Different Expanded");
                            //Debug.Assert(solvers[0].GetGenerated() == solvers[i].GetGenerated(), "Different Generated");
                            //Debug.Assert(solvers[0].GetSolutionDepth() == solvers[i].GetSolutionDepth(), "Depth Bug " + solvers[i]);
                        }
                        Console.WriteLine("+SUCCESS+ (:");
                    }
                    else
                    {
                        outOfTimeCounters[i]++;

                        Console.WriteLine("-FAILURE- ):");
                    }
                }
                else
                    PrintNullStatistics(solvers[i]);
                
                Console.WriteLine();
            }
            this.ContinueToNextLine();
            return solutionCost != -1;
        }

        ///// <summary>
        ///// Solve given instance with a list of algorithms 
        ///// </summary>
        ///// <param name="instance">The instance to solve</param>
        //public void SolveGivenProblemIncrementally(ProblemInstance instance)
        //{
        //    // Preparing a list of agent indices (not agent nums) for the heuristics' Init() method
        //    List<uint> agentList = Enumerable.Range(0, instance.m_vAgents.Length).Select(x => (uint)x).ToList(); // FIXME: Must the heuristics really receive a list of uints?

        //    AStarWithOD cooperativeAStar = new AStarWithOD();
        //    cooperativeAStar.Setup(instance, this);
        //    this.startTime = this.ElapsedMillisecondsTotal();
        //    double handlingStartTime = this.ElapsedMillisecondsTotal();
        //    double elapsedTime = 0;

        //    foreach (var agentIndex in Enumerable.Range(0, instance.m_vAgents.Length))
        //    {
        //        // Solve using the different algorithms
        //        Console.WriteLine($"Solving {instance} agent {agentIndex}");
        //        this.PrintProblemStatistics(instance);

        //        GC.Collect();
        //        GC.WaitForPendingFinalizers();

        //        this.startTime += this.ElapsedMillisecondsTotal() - handlingStartTime;
        //        bool solved = cooperativeAStar.AddOneAgent(agentIndex);
        //        elapsedTime = this.ElapsedMilliseconds();
        //        handlingStartTime = this.ElapsedMillisecondsTotal();
        //        if (solved)
        //        {
        //            Console.WriteLine("Total cost: {0}", cooperativeAStar.GetSolutionCost());
        //            Console.WriteLine("Solution depth: {0}", cooperativeAStar.GetSolutionDepth());
        //        }
        //        else
        //        {
        //            Console.WriteLine("Failed to solve");
        //            Console.WriteLine("Solution depth lower bound: {0}", cooperativeAStar.GetSolutionDepth());
        //        }
        //        Console.WriteLine();

        //        Console.WriteLine("Time In milliseconds: {0}", elapsedTime);

        //        this.PrintStatistics(instance, cooperativeAStar, elapsedTime + instance.shortestPathComputeTime, seed: 123);

        //        Console.WriteLine();

        //        int solverSolutionCost = cooperativeAStar.GetSolutionCost();

        //        if (solverSolutionCost >= 0) // Solved successfully
        //        {
        //            Plan plan = cooperativeAStar.GetPlan();
        //            int planSize = plan.GetSize();
        //            if (planSize < 50)
        //                plan.PrintPlan();
        //            else
        //                Console.WriteLine($"Plan is too long to print ({planSize} steps).");

        //            Console.WriteLine("+SUCCESS+ (:");
        //        }
        //        else
        //        {
        //            Console.WriteLine("-FAILURE- ):");
        //            break;
        //        }

        //        Console.WriteLine();
        //        this.ContinueToNextLine();
        //    }
        //}

        /// <summary>
        /// Solve a given instance with the given solver
        /// </summary>
        /// <param name="solver">The solver</param>
        /// <param name="instance">The problem instance that will be solved</param>
        private void run(ISolver solver, ProblemInstance instance)
        {
            // Run the algorithm
            bool solved;
            Console.WriteLine("-----------------" + solver + "-----------------");
            this.startTime = this.ElapsedMillisecondsTotal();
            solver.Setup(instance, this);
            solved = solver.Solve();
            double elapsedTime = this.ElapsedMilliseconds();
            if (solved)
            {
                Console.WriteLine("Total cost: {0}", solver.GetSolutionCost());
                Console.WriteLine("Solution depth: {0}", solver.GetSolutionDepth());
            }
            else
            {
                Console.WriteLine("Failed to solve");
                Console.WriteLine("Solution depth lower bound: {0}", solver.GetSolutionDepth());
            }
            Console.WriteLine();

            Console.WriteLine("Time In milliseconds: {0}", elapsedTime);
           // Console.WriteLine("Total Unique/Full Expanded Nodes: {0}", solver.GetNodesPassedPruningCounter());

            this.PrintStatistics(instance, solver, elapsedTime);
            // Solver clears itself when it finishes the search.
            solver.ClearStatistics();
        }

        /// <summary>
        /// Print the header of the results file
        /// </summary>
        public void PrintResultsFileHeader()
        {
            List<String> headers = new List<String>(){
                "GridName", "GridRows", "GridColumns", "NumOfAgents", "NumOfObstacles", "InstanceId",
                "BranchingFactor","ObstacleDensity", "AvgDistanceToGoal",
                "MaxDistanceToGoal","MinDistanceToGoal","AvgStartDistances",
                "AvgGoalDistances","PointsAtSPRatio"};

            foreach(string header in headers)
            {
                this.writeCellToCsv(header);
            }

            foreach(ISolver solver in solvers)
            {
                this.writeCellToCsv(solver + " Success");
                this.writeCellToCsv(solver + " Runtime");
                this.writeCellToCsv(solver + " Solution Cost");
                solver.OutputStatisticsHeader(this.resultsWriter);
                this.writeCellToCsv(solver + " Max Group");
                this.writeCellToCsv(solver + " Solution Depth");
            }

            this.ContinueToNextLine();
        }

        private void writeCellToCsv(Object value)
        {
            this.resultsWriter.Write(value);
            this.resultsWriter.Write(Run.RESULTS_DELIMITER);
        }

        /// <summary>
        /// Print the solver statistics to the results file.
        /// </summary>
        /// <param name="instance">The problem instance that was solved. Not used!</param>
        /// <param name="solver">The solver that solved the problem instance</param>
        /// <param name="runtimeInMillis">The time it took the given solver to solve the given instance</param>
        private void PrintStatistics(ProblemInstance instance, ISolver solver, double runtimeInMillis)
        {
            // Success col:
            if (solver.GetSolutionCost() < 0)
                this.resultsWriter.Write(Run.FAILURE_CODE + RESULTS_DELIMITER);
            else
                this.resultsWriter.Write(Run.SUCCESS_CODE + RESULTS_DELIMITER);
            // Runtime col:
            this.resultsWriter.Write(runtimeInMillis + RESULTS_DELIMITER);
            // Solution Cost col:
            this.resultsWriter.Write(solver.GetSolutionCost() + RESULTS_DELIMITER);
            // Algorithm specific cols:
            solver.OutputStatistics(this.resultsWriter);
            // Max Group col:
            this.resultsWriter.Write(solver.GetMaxGroupSize() + RESULTS_DELIMITER);
            // Solution Depth col:
            this.resultsWriter.Write(solver.GetSolutionDepth() + RESULTS_DELIMITER);
            //this.resultsWriter.Flush();
        }

        private void PrintProblemStatistics(ProblemInstance instance)
        {
            int numberOfAgents = instance.m_vAgents.Length;
            int numOfRows = instance.m_vGrid.Length;
            int numOfCols = instance.m_vGrid[0].Length;
            int gridSize = numOfCols * numOfRows;
            int numOfObstacles = (int)instance.m_nObstacles;

            // Grid Name col:
            if (instance.parameters.ContainsKey(ProblemInstance.GRID_NAME_KEY))
                this.writeCellToCsv(instance.parameters[ProblemInstance.GRID_NAME_KEY]);
            else
                this.writeCellToCsv("Unknown");
            // Grid Rows col:
            this.writeCellToCsv(numOfRows);
            // Grid Columns col:
            this.writeCellToCsv(numOfCols);
            // Num Of Agents col:
            this.writeCellToCsv(numberOfAgents);
            // Num Of Obstacles col:
            this.writeCellToCsv(numOfObstacles);
            // Instance Id col:
            this.writeCellToCsv(instance.instanceId);

            // Branching Factor col:
            int numberOfLegalMoves = Constants.ALLOW_DIAGONAL_MOVE ? Move.NUM_DIRECTIONS : Move.NUM_NON_DIAG_MOVES;
            this.writeCellToCsv(Math.Pow(numberOfLegalMoves, numberOfAgents));

            // Obstacle Density col:
            this.writeCellToCsv((float)numOfObstacles / (float)gridSize);

            // AvgDistanceToGoal col:
            this.writeCellToCsv(instance.agentDistancesToGoal.Average());

            // MaxDistanceToGoal col
            this.writeCellToCsv(instance.agentDistancesToGoal.Max());

            // MinDistanceToGoal col
            this.writeCellToCsv(instance.agentDistancesToGoal.Min());

            // AvgStartDistances col:
            this.writeCellToCsv(instance.AverageStartDistances());

            // AvgGoalDistances col:
            this.writeCellToCsv(instance.AverageGoalDistances());

            // PointsAtSPRatio col:
            this.writeCellToCsv(instance.RatioOfPointsAtSP());

        }

        private void ContinueToNextLine()
        {
            this.resultsWriter.WriteLine();
            this.resultsWriter.Flush();
        }

        private void PrintNullStatistics(ISolver solver)
        {
            // Success col:
            this.resultsWriter.Write(Run.FAILURE_CODE + RESULTS_DELIMITER);
            // Runtime col:
            this.resultsWriter.Write(Constants.MAX_TIME + RESULTS_DELIMITER);
            // Solution Cost col:
            this.resultsWriter.Write("irrelevant" + RESULTS_DELIMITER);
            // Algorithm specific cols:
            for (int i = 0; i < solver.NumStatsColumns; ++i)
                this.resultsWriter.Write("irrelevant" + RESULTS_DELIMITER);
            // Max Group col:
            this.resultsWriter.Write("irrelevant" + RESULTS_DELIMITER);
            // Solution Depth col:
            this.resultsWriter.Write("irrelevant" + RESULTS_DELIMITER);
        }
        
        public void ResetOutOfTimeCounters()
        {
            for (int i = 0; i < outOfTimeCounters.Length; i++)
            {
                outOfTimeCounters[i] = 0;
            }
        }

        private Stopwatch watch;
        private double ElapsedMillisecondsTotal()
        {
            return this.watch.Elapsed.TotalMilliseconds;
        }

        public double ElapsedMilliseconds()
        {
            return ElapsedMillisecondsTotal() - this.startTime;
        }

        public void StartOracle()
        {
            this.watch.Stop();
            // NOTE: This allows the algorithm with the oracle solve harder problems without timing out, getting
            // a higher average timeout than running without the oracle, which isn't what we want.
            // We need to start another counter when the oracle runs and when the run successfully finishes
            // substract its count
        }

        public void StopOracle()
        {
            this.watch.Start();
        }
    }
}
