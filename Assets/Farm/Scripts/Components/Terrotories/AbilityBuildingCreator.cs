using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Loading;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Farm.Scripts.Components.Terrotories
{
    public class AbilityBuildingCreator : MonoBehaviour, IActorAbility
    {
        [PropertyOrder(0)]
        public InputActionReference BuildAction;

        [PropertyOrder(2)]
        public ActorSpawnerSettings BuildingSpawnData;

        [PropertyOrder(1)]
        public InputActionReference PositionAction;

        private Entity _entity;

        private Vector2 _viewPostion;
        public IActor Actor { get; set; }

        public void AddComponentData(ref Entity entity, IActor actor)
        {
            _entity = entity;
            Actor = actor;
            Actor.Owner = actor;

            PositionAction.action.performed += SetBuildPosition;
            BuildAction.action.performed += Build;
        }

        public void Build(Vector3 position)
        {
            BuildingSpawnData.SpawnPoints.Clear();
            BuildingSpawnData.SpawnPoints.Add(CreateSpawnPoint(position));

            ActorSpawn.Spawn(BuildingSpawnData);
        }

        public void Execute()
        {
        }

        private void Build(InputAction.CallbackContext context)
        {
            var ray = Camera.main.ScreenPointToRay(_viewPostion);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Build(hit.point);
            }
            Debug.Log(nameof(Build));
        }

        private GameObject CreateSpawnPoint(Vector3 position)
        {
            var spawnPoint = new GameObject("BuildingSpawnPoint");
            spawnPoint.transform.position = position;
            return spawnPoint;
        }

        private void SetBuildPosition(InputAction.CallbackContext context)
        {
            _viewPostion = context.ReadValue<Vector2>();
        }
    }
}