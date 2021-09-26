using Crimson.Core.Common;
using Crimson.Core.Components;
using Crimson.Core.Utils.LowLevel;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;

namespace Crimson.Core.Systems
{
    [UpdateBefore(typeof(FixedUpdateGroup))]
    public class UserInputSystem : JobComponentSystem
    {
        private EntityCommandBufferSystem _barrier;

        private InputAction _moveAction;
        private InputAction _mouseAction;
        private InputAction _lookAction;

        private List<InputAction> _customActions = new List<InputAction>();
        private List<InputAction> _customSticksInputActions = new List<InputAction>();

        private float2 _moveInput;
        private float2 _mouseInput;
        private float2 _lookInput;

        private NativeArray<float> _customInputs;
        private NativeArray<float2> _customSticksInputs;

        protected override void OnCreate()
        {
            _barrier = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        }


        protected override void OnStartRunning()
        {
            _customInputs = new NativeArray<float>(Constants.INPUT_BUFFER_CAPACITY, Allocator.Persistent);
            _customSticksInputs = new NativeArray<float2>(Constants.INPUT_BUFFER_CAPACITY, Allocator.Persistent);

            _moveAction = new InputAction("move", binding: "<Gamepad>/leftStick");
            _moveAction.AddCompositeBinding("Dpad")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            _moveAction.performed += context => { _moveInput = context.ReadValue<Vector2>(); };
            _moveAction.started += context => { _moveInput = context.ReadValue<Vector2>(); };
            _moveAction.canceled += context => { _moveInput = context.ReadValue<Vector2>(); };
            _moveAction.Enable();

            _mouseAction = new InputAction("mouse", binding: "<Mouse>/position");
            _mouseAction.performed += context => { _mouseInput = context.ReadValue<Vector2>(); };
            _mouseAction.canceled += context => { _mouseInput = context.ReadValue<Vector2>(); };
            _mouseAction.Enable();

            // _lookAction = new InputAction("look", binding: "<Gamepad>/rightStick");
            // _lookAction.AddCompositeBinding("Dpad")
            //     .With("Up", "<Keyboard>/upArrow")
            //     .With("Down", "<Keyboard>/downArrow")
            //     .With("Left", "<Keyboard>/leftArrow")
            //     .With("Right", "<Keyboard>/rightArrow");
            //
            // _lookAction.AddBinding(new InputBinding("<Pointer>/delta"));
            //
            // _lookAction.performed += context => { _lookInput = context.ReadValue<Vector2>(); };
            // _lookAction.canceled += context => { _lookInput = context.ReadValue<Vector2>(); };
            // _lookAction.Enable();

            //Here goes custom Actions for virtual keys 0..9
            for (var i = 0; i <= 9; i++)
            {
                var j = i;
                var action = new InputAction($"CustomAction{j}", binding: $"<Keyboard>/{j}");

                switch (j)
                {
                    case 0:
                        action.AddBinding(new InputBinding("<Mouse>/leftButton"));
                        break;
                    case 1:
                        action.AddBinding(new InputBinding("<Mouse>/rightButton"));
                        break;
                }

                action.started += context => { _customInputs[j] = context.ReadValue<float>(); };
                action.performed += context => { _customInputs[j] = context.ReadValue<float>(); };
                action.canceled += context => { _customInputs[j] = context.ReadValue<float>(); };
                action.Enable();
                _customActions.Add(action);
            }

            RegisterCustomSticks();
        }
        protected override void OnStopRunning()
        {
            _mouseAction.Disable();
            //_lookAction.Disable();
            _moveAction.Disable();

            foreach (var c in _customActions)
            {
                c.Disable();
            }

            _customInputs.Dispose();

            foreach (var c in _customSticksInputActions)
            {
                c.Disable();
            }

            _customSticksInputs.Dispose();

            var perkStickControls = InputSystem.devices.FirstOrDefault(x => x is CustomDevice);
            if (perkStickControls != null)
                InputSystem.RemoveDevice(perkStickControls);
        }

        private void RegisterCustomSticks()
        {
            if (InputSystem.devices.FirstOrDefault(x => x is CustomDevice) == null)
            {
                InputSystem.AddDevice(new InputDeviceDescription
                {
                    interfaceName = "CustomDevice",
                    product = "Custom Device"
                });
            }

            for (var k = 0; k <= 4; k++)
            {
                var j = k;

                _customSticksInputActions.Add(new InputAction($"customStick_{j}", binding: $"<CustomDevice>/customStick_{j}"));
                _customSticksInputActions.Last().started += context => { _customSticksInputs[j] = context.ReadValue<Vector2>(); };
                _customSticksInputActions.Last().performed += context => { _customSticksInputs[j] = context.ReadValue<Vector2>(); };
                _customSticksInputActions.Last().canceled += context => { _customSticksInputs[j] = context.ReadValue<Vector2>(); };
                _customSticksInputActions.Last().Enable();
            }
        }

        //[BurstCompile]
        private struct PlayerInputJob : IJobForEachWithEntity<PlayerInputData, UserInputData>
        {
            public EntityCommandBuffer.Concurrent Ecb;

            [ReadOnly] public float2 MoveInput;
            [ReadOnly] public float2 MouseInput;
            [ReadOnly] public float2 LookInput;
            [ReadOnly] public NativeArray<float> CustomInputs;
            [ReadOnly] public NativeArray<float2> CustomSticksInputs;

            public void Execute(Entity entity, int index, ref PlayerInputData inputData, ref UserInputData u)
            {
                inputData.Move = MoveInput;
                inputData.Mouse = MouseInput;
                inputData.Look = LookInput;

                var debugString = "";
                
                for (var i = 0; i < CustomInputs.Length; i++)
                {
                    debugString += $"{CustomInputs[i]:N1} |";
                    inputData.CustomInput[i] = CustomInputs[i];
                }

                Debug.Log(debugString);
                for (var i = 0; i < CustomSticksInputs.Length; i++)
                {
                    inputData.CustomSticksInput[i] = CustomSticksInputs[i];
                }
            }
        }

        [BurstCompile]
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var job = new PlayerInputJob
            {
                Ecb = _barrier.CreateCommandBuffer().ToConcurrent(),
                MoveInput = _moveInput,
                MouseInput = _mouseInput,
                LookInput = _lookInput,
                CustomInputs = _customInputs,
                CustomSticksInputs = _customSticksInputs
            };
            inputDeps = job.Schedule(this, inputDeps);
            _barrier.AddJobHandleForProducer(inputDeps);
            return inputDeps;
        }
    }
}