using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapModuleState
{
   public MapModule Prefab { get; private set; }
   public Vector3 Rotation { get; private set; }
   public Dictionary<Vector2,MapModuleContact> Contacts { get; private set; }

   public MapModuleState(MapModule prefab, Vector3 rotation)
   {
      Prefab = prefab;
      Rotation = rotation;
      Contacts = new Dictionary<Vector2, MapModuleContact>();
   }

   public bool IsMatchingModules(MapModuleState otherMapModuleState, Vector2 direction)
   {
      (var current, var other) = GetConnectedContacts(otherMapModuleState, direction);
      return current.IsMatchingContacts(other);
   }

   public (MapModuleContact Current, MapModuleContact Other) GetConnectedContacts(MapModuleState otherModuleState,
      Vector2 direction)
   {
      var currentContact = Contacts[direction];
      var otherContact = otherModuleState.Contacts[-direction];
      return (currentContact, otherContact);
   }

   public void InstantiatePrefab(Map map, Vector3 localPosition)
   {
      var GO = MonoBehaviour.Instantiate(Prefab);
      GO.transform.parent = map.transform;
      GO.transform.localPosition = localPosition;
      GO.transform.Rotate(Rotation);
   }

   public void InstantiateSpecificPrefab(Map map, Vector3 localPosition, MapModule prefab)
   {
      for (int i = 0; i < 1; i++)
      {
         var go = MonoBehaviour.Instantiate(prefab);
         go.transform.parent = map.transform;
         go.transform.localPosition = localPosition;
         go.transform.Rotate(Rotation);
      }
   }
}
