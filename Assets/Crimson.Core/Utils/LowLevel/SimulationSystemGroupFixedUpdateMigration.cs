using Unity.Entities;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace Crimson.Core.Utils.LowLevel
{
    public struct SimulationSystemGroupFixedUpdateMigration : ICustomBootstrap
    {
        public bool Initialize(string defaultWorldName)
        {
            // Initialize the world normally
            var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);

            var world = new World(defaultWorldName);
            World.DefaultGameObjectInjectionWorld = world;

            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
            
            // Moving SimulationSystemGroup to FixedUpdate is done in two parts.
            // The PlayerLoopSystem of type SimulationSystemGroup has to be found,
            // stored, and removed before adding it to the FixedUpdate PlayerLoopSystem.

            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            ScriptBehaviourUpdateOrder.AddWorldToPlayerLoop(world,ref playerLoop);

            // simulationSystem has to be constructed or compiler will complain due to
            //    using non-assigned variables.
            PlayerLoopSystem simulationSystem = new PlayerLoopSystem();
            bool simSysFound = false;

            // Find the location of the SimulationSystemGroup under the Update Loop
            for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                int subsystemListLength = playerLoop.subSystemList[i].subSystemList.Length;

                // Find Update loop...
                if (playerLoop.subSystemList[i].type != typeof(Update)) continue;
                
                // Pop out SimulationSystemGroup and store it temporarily
                var newSubsystemList = new PlayerLoopSystem[subsystemListLength - 1];
                int k = 0;

                for (var j = 0; j < subsystemListLength; ++j)
                {
                    if (playerLoop.subSystemList[i].subSystemList[j].type == typeof(SimulationSystemGroup))
                    {
                        simulationSystem = playerLoop.subSystemList[i].subSystemList[j];
                        simSysFound = true;
                    }
                    else
                    {
                        newSubsystemList[k] = playerLoop.subSystemList[i].subSystemList[j];
                        k++;
                    }
                }

                playerLoop.subSystemList[i].subSystemList = newSubsystemList;
            }

            // This should never happen if SimulationSystemGroup was created like usual
            // (or at least I think it might not happen :P )
            if (!simSysFound)
                throw new System.Exception("SimulationSystemGroup was not found!");

            // Round 2: find FixedUpdate...
            for (var i = 0; i < playerLoop.subSystemList.Length; ++i)
            {
                int subsystemListLength = playerLoop.subSystemList[i].subSystemList.Length;

                // Found FixedUpdate
                if (playerLoop.subSystemList[i].type != typeof(FixedUpdate)) continue;
                
                // Allocate new space for stored SimulationSystemGroup
                //    PlayerLoopSystem, and place simulation group at index defined by
                //    temporary variable.
                var newSubsystemList = new PlayerLoopSystem[subsystemListLength + 1];
                int k = 0;

                int indexToPlaceSimulationSystemGroupIn = subsystemListLength;

                for (var j = 0; j < subsystemListLength + 1; ++j)
                {
                    if (j == indexToPlaceSimulationSystemGroupIn)
                        newSubsystemList[j] = simulationSystem;
                    else
                    {
                        newSubsystemList[j] = playerLoop.subSystemList[i].subSystemList[k];
                        k++;
                    }
                }

                playerLoop.subSystemList[i].subSystemList = newSubsystemList;
            }

            // Set the beautiful, new player loop
            PlayerLoop.SetPlayerLoop(playerLoop);

            return true;
        }
    }
}