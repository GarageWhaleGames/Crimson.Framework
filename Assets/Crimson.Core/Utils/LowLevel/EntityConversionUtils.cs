// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Reflection;
// using Unity.Entities;
// using UnityEngine;
//
// namespace Crimson.Core.Utils.LowLevel
// {
//     public class EntityConversionUtils
//     {
//         private delegate void ConvertDelegate(World gameObjectWorld);
//
//         private static readonly ConvertDelegate Convert = (ConvertDelegate) typeof(GameObjectConversionUtility)
//             .GetMethod("Convert", BindingFlags.Static | BindingFlags.NonPublic)
//             ?.CreateDelegate(typeof(ConvertDelegate));
//
//         private delegate Entity GameObjectToConvertedEntityDelegate(World gameObjectWorld, GameObject gameObject);
//
//         private static readonly GameObjectToConvertedEntityDelegate GameObjectToConvertedEntity =
//             (GameObjectToConvertedEntityDelegate)
//             typeof(GameObjectConversionUtility)
//                 .GetMethod("GameObjectToConvertedEntity", BindingFlags.Static | BindingFlags.NonPublic)
//                 ?.CreateDelegate(typeof(GameObjectToConvertedEntityDelegate));
//
//         private delegate ComponentType GetComponentTypeDelegate(ComponentDataProxyBase n);
//
//         private static readonly GetComponentTypeDelegate GetComponentType =
//             (GetComponentTypeDelegate) typeof(ComponentDataProxyBase)
//                 .GetMethod("GetComponentType", BindingFlags.Instance | BindingFlags.NonPublic)
//                 ?.CreateDelegate(typeof(GetComponentTypeDelegate));
//
//         private delegate void UpdateComponentDataDelegate(ComponentDataProxyBase n, EntityManager manager,
//             Entity entity);
//
//         private static readonly UpdateComponentDataDelegate UpdateComponentData =
//             (UpdateComponentDataDelegate) typeof(ComponentDataProxyBase)
//                 .GetMethod("UpdateComponentData", BindingFlags.Instance | BindingFlags.NonPublic, null,
//                     CallingConventions.Any, new[]
//                     {
//                         typeof(EntityManager), typeof(Entity)
//                     }, null)
//                 ?.CreateDelegate(typeof(UpdateComponentDataDelegate));
//
//         /*private delegate void SetComponentObjectDelegate(Unity.Entities.EntityManager n, Unity.Entities.Entity entity, Unity.Entities.ComponentType componentType,
//             System.Object componentObject);
//
//         private static readonly SetComponentObjectDelegate SetComponentObject = null;
//         /*private static readonly SetComponentObjectDelegate SetComponentObject =
//             (SetComponentObjectDelegate) typeof(Unity.Entities.EntityManager)
//                 .GetMethod("SetComponentObject", BindingFlags.Instance | BindingFlags.NonPublic, null,
//                     CallingConventions.Any, new[]
//                     {
//                         typeof(Unity.Entities.Entity), typeof(Unity.Entities.ComponentType),
//                         typeof(System.Object)
//                     }, null)
//                 ?.CreateDelegate(typeof(SetComponentObjectDelegate));*/
//
//         
//
//         public static void ConvertAndInjectOriginal(GameObject root)
//         {
//             using (var gameObjectWorld =
//                 new GameObjectConversionSettings(World.DefaultGameObjectInjectionWorld,
//                         GameObjectConversionUtility.ConversionFlags.AssignName)
//                     .CreateConversionWorld())
//             {
//                 AddToEntityManager(gameObjectWorld.EntityManager, root);
//
//                 Convert(gameObjectWorld);
//
//                 var entity = GameObjectToConvertedEntity(gameObjectWorld, root);
//
//                 InjectOriginalComponents(World.DefaultGameObjectInjectionWorld.EntityManager, entity, root.transform);
//             }
//         }
//
//         public static Entity AddToEntityManager(EntityManager entityManager, GameObject gameObject)
//         {
//             ComponentType[] types;
//             Component[] components;
//             GetComponents(gameObject, true, out types, out components);
//
//             EntityArchetype archetype;
//             try
//             {
//                 archetype = entityManager.CreateArchetype(types);
//             }
//             catch (Exception e)
//             {
//                 for (int i = 0; i < types.Length; ++i)
//                 {
//                     if (Array.IndexOf(types, types[i]) == i) continue;
//
//                     Debug.LogError(
//                         $"[ACTOR CONVERSION] GameObject '{gameObject}' has multiple {types[i]} components and cannot be converted, skipping.");
//                     return Entity.Null;
//                 }
//
//                 throw e;
//             }
//
//             var entity = CreateEntity(entityManager, archetype, components, types);
//
//             return entity;
//         }
//
//         private static void GetComponents(GameObject gameObject, bool includeGameObjectComponents,
//             out ComponentType[] types,
//             out Component[] components)
//         {
//             if (gameObject == null)
//             {
//                 types = new ComponentType[0];
//                 components = new Component[0];
//                 return;
//             }
//             components = gameObject.GetComponents<Component>()?
//                 .Where(comp => comp.GetType().GetCustomAttribute<DoNotAddToEntity>() == null)?.ToArray();
//
//             var componentCount = 0;
//             for (var i = 0; i != components.Length; i++)
//             {
//                 var com = components[i];
//                 var componentData = com as ComponentDataProxyBase;
//
//                 if (com == null)
//                     Debug.LogError($"[ACTOR CONVERSION] The referenced script is missing on {gameObject.name}",
//                         gameObject);
//                 else if (componentData != null)
//                     componentCount++;
//                 else if (includeGameObjectComponents && !(com is GameObjectEntity))
//                     componentCount++;
//             }
//
//             types = new ComponentType[componentCount];
//
//             var t = 0;
//             for (var i = 0; i != components.Length; i++)
//             {
//                 var com = components[i];
//                 var componentData = com as ComponentDataProxyBase;
//
//                 if (componentData != null)
//                 {
//                     if (GetComponentType is null)
//                     {
//                         throw new Exception("[ACTOR CONVERSION] General Error, everything is screwed up =(");
//                     }
//
//                     types[t++] = GetComponentType(componentData);
//                 }
//                 else if (includeGameObjectComponents && !(com is GameObjectEntity) && com != null)
//                     types[t++] = com.GetType();
//             }
//         }
//
//         private static Entity CreateEntity(EntityManager entityManager, EntityArchetype archetype,
//             IReadOnlyList<Component> components, IReadOnlyList<ComponentType> types)
//         {
//             var entity = entityManager.CreateEntity(archetype);
//             var t = 0;
//             for (var i = 0; i != components.Count; i++)
//             {
//                 var com = components[i];
//                 var componentDataProxy = com as ComponentDataProxyBase;
//
//                 if (componentDataProxy != null)
//                 {
//                     if (UpdateComponentData is null)
//                     {
//                         throw new Exception("[ACTOR CONVERSION] General Error, everything is screwed up =(");
//                     }
//
//                     UpdateComponentData(componentDataProxy, entityManager, entity);
//                     t++;
//                 }
//                 else if (!(com is GameObjectEntity) && com != null)
//                 {
//                     /*if (SetComponentObject is null)
//                     {
//                         throw new Exception("[ACTOR CONVERSION] General Error, everything is screwed up =(");
//                     }*/
//
//                     entityManager.SetComponentObject(entity, types[t], com);
//                     t++;
//                 }
//             }
//
//             return entity;
//         }
//
//         private static void InjectOriginalComponents(EntityManager entityManager, Entity entity, Component component)
//         {
//             foreach (var com in component.GetComponents<Component>())
//             {
//                 if (com is GameObjectEntity || com is ConvertToEntity || com is ComponentDataProxyBase)
//                     continue;
//
//                 entityManager.AddComponentObject(entity, com);
//             }
//         }
//     }
// }