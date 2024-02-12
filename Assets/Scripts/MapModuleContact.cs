using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapModuleContact 
{
  [SerializeField] private string _contactType;
  [SerializeField] private List<string> _notSuitableContactTypes;

  public string ContactTypes => _contactType;
  public List<string> NotSuitableContactTypes => _notSuitableContactTypes;

  public bool IsMatchingContacts(MapModuleContact other)
  {
    return !other._notSuitableContactTypes.Contains(ContactTypes) &&
           !NotSuitableContactTypes.Contains(other.ContactTypes);
  }
}

public static class ContactDirectionInMap
{
    public static Vector2 Forward => new Vector2(0, 1);
    public static Vector3 Back => new Vector2(0, -1);
    public static Vector3 Right => new Vector2(1, 0);
    public static Vector3 Left => new Vector2(-1, 0);
}