using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class MapModule : MonoBehaviour
{
    [SerializeField] private Map _map;

    [SerializeField] private string _forwardContactType;

    [SerializeField] private string _backContactType;

    [SerializeField] private string _rightContactType;

    [SerializeField] private string _leftContactType;


    string[] _contactTypes => new string[]
    {
        _forwardContactType,
        _rightContactType,
        _backContactType,
        _leftContactType
    };

    private Vector2[] _contactDiretions => new Vector2[]
    {
        ContactDirectionInMap.Forward,
        ContactDirectionInMap.Right,
        ContactDirectionInMap.Back,
        ContactDirectionInMap.Left
    };

    public List<MapModuleState> GetMapModuleFromPrefab()
    {
        var contactTypes = _contactTypes;
        var contactDirections = _contactDiretions;
        List<MapModuleState> mapModules = new List<MapModuleState>();
        var rotationY = 0;

        for (int i = 0; i < contactDirections.Length; i++)
        {
            MapModuleState module = new MapModuleState(this, Vector3.up * rotationY);

            for (int j = 0; j < contactTypes.Length; j++)
            {
                var typeIndex = (i + j) % contactTypes.Length;
                var contact = _map.GetContact(contactTypes[typeIndex]);
                module.Contacts.Add(contactDirections[j], contact);
            }
            
            mapModules.Add(module);
            rotationY -= 90;
        }
        return mapModules;
        
    }
}
