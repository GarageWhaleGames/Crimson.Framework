// GENERATED AUTOMATICALLY FROM 'Assets/Farm/Scripts/Inputs/FarmInputActions.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @FarmInputActions : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @FarmInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""FarmInputActions"",
    ""maps"": [
        {
            ""name"": ""FarmActionMap"",
            ""id"": ""27b55cb9-3da5-4e07-b63d-0146862049f2"",
            ""actions"": [
                {
                    ""name"": ""CreateBuilding"",
                    ""type"": ""Button"",
                    ""id"": ""756fc0e8-6e77-4bed-b673-ee05132a8c82"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""SelectBuildingPosition"",
                    ""type"": ""Value"",
                    ""id"": ""a1af9c4b-1751-4d96-bc72-ab6b9a612c9a"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""f64d58d9-62ed-4755-80db-a5a9439529ec"",
                    ""path"": ""<Mouse>/press"",
                    ""interactions"": ""Tap"",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""CreateBuilding"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0dcfaa70-8d09-42bb-ad99-66d79b374757"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""SelectBuildingPosition"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        // FarmActionMap
        m_FarmActionMap = asset.FindActionMap("FarmActionMap", throwIfNotFound: true);
        m_FarmActionMap_CreateBuilding = m_FarmActionMap.FindAction("CreateBuilding", throwIfNotFound: true);
        m_FarmActionMap_SelectBuildingPosition = m_FarmActionMap.FindAction("SelectBuildingPosition", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // FarmActionMap
    private readonly InputActionMap m_FarmActionMap;
    private IFarmActionMapActions m_FarmActionMapActionsCallbackInterface;
    private readonly InputAction m_FarmActionMap_CreateBuilding;
    private readonly InputAction m_FarmActionMap_SelectBuildingPosition;
    public struct FarmActionMapActions
    {
        private @FarmInputActions m_Wrapper;
        public FarmActionMapActions(@FarmInputActions wrapper) { m_Wrapper = wrapper; }
        public InputAction @CreateBuilding => m_Wrapper.m_FarmActionMap_CreateBuilding;
        public InputAction @SelectBuildingPosition => m_Wrapper.m_FarmActionMap_SelectBuildingPosition;
        public InputActionMap Get() { return m_Wrapper.m_FarmActionMap; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(FarmActionMapActions set) { return set.Get(); }
        public void SetCallbacks(IFarmActionMapActions instance)
        {
            if (m_Wrapper.m_FarmActionMapActionsCallbackInterface != null)
            {
                @CreateBuilding.started -= m_Wrapper.m_FarmActionMapActionsCallbackInterface.OnCreateBuilding;
                @CreateBuilding.performed -= m_Wrapper.m_FarmActionMapActionsCallbackInterface.OnCreateBuilding;
                @CreateBuilding.canceled -= m_Wrapper.m_FarmActionMapActionsCallbackInterface.OnCreateBuilding;
                @SelectBuildingPosition.started -= m_Wrapper.m_FarmActionMapActionsCallbackInterface.OnSelectBuildingPosition;
                @SelectBuildingPosition.performed -= m_Wrapper.m_FarmActionMapActionsCallbackInterface.OnSelectBuildingPosition;
                @SelectBuildingPosition.canceled -= m_Wrapper.m_FarmActionMapActionsCallbackInterface.OnSelectBuildingPosition;
            }
            m_Wrapper.m_FarmActionMapActionsCallbackInterface = instance;
            if (instance != null)
            {
                @CreateBuilding.started += instance.OnCreateBuilding;
                @CreateBuilding.performed += instance.OnCreateBuilding;
                @CreateBuilding.canceled += instance.OnCreateBuilding;
                @SelectBuildingPosition.started += instance.OnSelectBuildingPosition;
                @SelectBuildingPosition.performed += instance.OnSelectBuildingPosition;
                @SelectBuildingPosition.canceled += instance.OnSelectBuildingPosition;
            }
        }
    }
    public FarmActionMapActions @FarmActionMap => new FarmActionMapActions(this);
    public interface IFarmActionMapActions
    {
        void OnCreateBuilding(InputAction.CallbackContext context);
        void OnSelectBuildingPosition(InputAction.CallbackContext context);
    }
}
