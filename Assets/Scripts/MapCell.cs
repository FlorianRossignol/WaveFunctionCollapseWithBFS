using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class MapCell
{
    public Vector2Int PositionInMap { get; private set; }
    public List<MapModuleState> States { get; private set; }
    public List<MapModule> Modules { get; private set; }
    public bool isVisited = false;
    
    
    public List<Vector2Int> AdjacentCellsPositions { get; private set; }
    public List<MapCell> AdjacentCellsPositionsMapCells { get; private set; }

    private Map _map;
    private Dictionary<MapCell, MapModuleState[]> _mapCellCashe = new Dictionary<MapCell, MapModuleState[]>();

    public MapCell(Map map, Vector2Int positionInMap, List<MapModuleState> states)
    {
        States = states;
        PositionInMap = positionInMap;
        AdjacentCellsPositions = GetAdjacentCellsPositions(map);
        _map = map;
    }

    public MapCell(Map map, Vector2Int positionInMap,List<MapModule> mapModule)
    {
        PositionInMap = positionInMap;
        Modules = mapModule;
        AdjacentCellsPositionsMapCells = getAdjacentCellsPositionsMapCells(map);
        _map = map;
    }

    List<Vector2Int> GetAdjacentCellsPositions(Map map)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        if (PositionInMap.x - 1 >= 0) cells.Add(new Vector2Int(PositionInMap.x-1, PositionInMap.y));
        if (PositionInMap.x + 1 < map.RowsCount) cells.Add(new Vector2Int(PositionInMap.x+1, PositionInMap.y));
        if (PositionInMap.y - 1 >= 0) cells.Add(new Vector2Int(PositionInMap.x, PositionInMap.y-1));
        if (PositionInMap.y + 1 < map.ColumnsCount) cells.Add(new Vector2Int(PositionInMap.x, PositionInMap.y+1));
        return cells;
    }
    
    List<MapCell> getAdjacentCellsPositionsMapCells(Map map)
    {
        List<MapCell> cells = new List<MapCell>();
        if (PositionInMap.x - 1 >= 0) cells.Add(map.MapCellsMatrix[PositionInMap.x-1,PositionInMap.y]);
        if (PositionInMap.x + 1 < map.RowsCount) cells.Add(map.MapCellsMatrix[PositionInMap.x +1,PositionInMap.y]);
        if (PositionInMap.y - 1 >= 0) cells.Add(map.MapCellsMatrix[PositionInMap.x, PositionInMap.y-1]);
        if (PositionInMap.y + 1 < map.ColumnsCount) cells.Add(map.MapCellsMatrix[PositionInMap.x,PositionInMap.y +1]);
        return cells;
    }

    public delegate MapModuleState GetModuleAction(List<MapModuleState> modules);

    public bool TrySelectState(GetModuleAction getModuleAction)
    {
        AddOrUpdateToMapCellCashe(this);
        var states = new List<MapModuleState>(States);
        while (states.Count > 0)
        {
            var selectState = getModuleAction(states);
            States = new List<MapModuleState>() { selectState };
            if (!TryUpdateAdjacentCells(this))
            {
                states.Remove(selectState);
            }
            else return true;
        }
        return false;
    }

    delegate bool TryUpdateAction();

    bool TryUpdateAdjacentCells(MapCell cellWithSelectedModule)
    {
        List<TryUpdateAction> updateAdjacentCellsActions = new List<TryUpdateAction>();

        bool updateSuccess = AdjacentCellsPositions.All(cellPos =>
        {
            return _map.MapCellsMatrix[cellPos.x, cellPos.y]
                .TryUpdateStates(this, cellWithSelectedModule, updateAdjacentCellsActions);
        });
        
        if (!updateSuccess)
        {
            ReverseStates(cellWithSelectedModule);
            return false;
        }
        else
        {
            return updateAdjacentCellsActions.All(action => action.Invoke());
        }
    }
    
    bool TryUpdateStates(MapCell otherCell, MapCell cellWithSelectedState, List<TryUpdateAction> updateAdjacentCellsActions)
    {
        AddOrUpdateToMapCellCashe(cellWithSelectedState);

        int removeModuleCount = States.RemoveAll(thisState =>
        {
            var directionToPreviusCell = otherCell.PositionInMap - PositionInMap;
            return !otherCell.States.Any(otherState => thisState.IsMatchingModules(otherState, directionToPreviusCell));
        });

        if (States.Count == 0)
            return false;

        if (removeModuleCount > 0)
            updateAdjacentCellsActions.Add(() => TryUpdateAdjacentCells(cellWithSelectedState));

        return true;
    }
    
    void AddOrUpdateToMapCellCashe(MapCell originallyUpdatedCell)
    {
        if (_mapCellCashe.ContainsKey(originallyUpdatedCell)) _mapCellCashe[originallyUpdatedCell] = States.ToArray();
        else _mapCellCashe.Add(originallyUpdatedCell, States.ToArray());
    }
    
    public void ReverseStates(MapCell originallyUpdatedCell)
    {
        if (_mapCellCashe.ContainsKey(originallyUpdatedCell))
        {
            States = new List<MapModuleState>(_mapCellCashe[originallyUpdatedCell]);
            _mapCellCashe.Remove(originallyUpdatedCell);
            foreach (var cellPos in AdjacentCellsPositions)
            {
                _map.MapCellsMatrix[cellPos.x, cellPos.y].ReverseStates(originallyUpdatedCell);
            }
        }
    }

    public List<MapCell> navigateableNeighbours()
    {
        var result = new List<MapCell>();
        AdjacentCellsPositionsMapCells = getAdjacentCellsPositionsMapCells(_map);
        RaycastHit hit;
        var localPosition = new Vector3(_map._MapSize.x * _map._CellSize + _map._CellSize / 2, 0, _map._MapSize.y * _map._CellSize + _map._CellSize / 2);
        var localPosition2 = new Vector3(0 * _map._CellSize + _map._CellSize / 2, 0, 0 * _map._CellSize + _map._CellSize / 2);
        foreach (var neighbours in AdjacentCellsPositionsMapCells)
        {
            Debug.DrawLine(localPosition,localPosition + new Vector3(0,100,0),Color.blue,Mathf.Infinity);
            var localPositionN = new Vector3(neighbours._map._MapSize.x  * neighbours._map._CellSize + neighbours._map._CellSize / 2, 0, neighbours._map._MapSize.y * neighbours._map._CellSize + neighbours._map._CellSize /2);
            var localPositionN2 = new Vector3(1 * neighbours._map._CellSize + neighbours._map._CellSize /2 , 0, 1 * neighbours._map._CellSize + neighbours._map._CellSize /2);
            var magniTudeRay =  (localPositionN2 - localPosition2).magnitude;
            if (Physics.Raycast(localPosition2,localPositionN2,out hit,magniTudeRay))
            {
                Debug.DrawRay(localPosition2,localPositionN2 * hit.distance,Color.blue,Mathf.Infinity);
                Debug.Log("hit");
                break;
            }

            Debug.DrawRay(localPosition2,localPositionN2 * 1000,Color.green,Mathf.Infinity);
            Debug.Log("not hit");
            result.Add(neighbours);
            //neighbours.isVisited = true;
        }
       
        return result;
    }
    
}