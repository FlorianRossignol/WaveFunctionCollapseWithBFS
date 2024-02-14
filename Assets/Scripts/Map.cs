using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    [SerializeField] public Vector2Int _MapSize = new Vector2Int(5, 5);

    [SerializeField] public float _CellSize;

    [SerializeField] private MapModule[] _mapModules;
    
    [SerializeField] private List<MapModuleContact> _ContactType = new List<MapModuleContact>();

    public MapCell[,] MapCellsMatrix;

    public MapCell[,] MapCellsMatrix2;
    
    [SerializeField] private MapModule _firstMapModule;
    [SerializeField] private MapModule _endMapModule;
    [SerializeField] private MapModule _pathMapModule;

    public int RowsCount => MapCellsMatrix.GetLength(0);

    public int ColumnsCount => MapCellsMatrix.GetLength(1);
    
    public int RowsCount2 => MapCellsMatrix2.GetLength(0);

    public int ColumnsCount2 => MapCellsMatrix2.GetLength(1);

    private MapCell[] _mapCellsArray;

    private MapCell[] _mapCellsArray2;
 
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            InitializeMap();
            InstanciateStartEndModuleForBfs();
            FillCells();
            //CreateMapBfs();
            CreateMap();
            BFS(MapCellsMatrix[0,0], MapCellsMatrix[_MapSize.x,_MapSize.y]);
            
        }
    }

    void InitializeMap()
    {
        MapCellsMatrix = new MapCell[_MapSize.x, _MapSize.y];

        var mapModules = GetMapModules();
        for (int i = 0; i < _MapSize.x; i++)
        {
            for (int j = 0; j < _MapSize.y; j++)
            {
                MapCellsMatrix[i, j] = new MapCell(this, new Vector2Int(i, j), new List<MapModuleState>(mapModules));
            }
        }

        _mapCellsArray = MapCellsMatrix.Cast<MapCell>().ToArray();
    }

    void InitializeMap2()
    {
        MapCellsMatrix2 = new MapCell[_MapSize.x, _MapSize.y];

        var mapModules = new List<MapModule>();
        mapModules.Add(_firstMapModule);
        mapModules.Add(_pathMapModule);
        mapModules.Add(_endMapModule);
        for (int i = 0; i < _MapSize.x; i++)
        {
            for (int j = 0; j < _MapSize.y; j++)
            {
               MapCellsMatrix2[i, j] = new MapCell(this, new Vector2Int(i, j),mapModules);
            }
        }

        _mapCellsArray2 = MapCellsMatrix2.Cast<MapCell>().ToArray();
    }

    
    void FillCells()
    {
        Profiler.BeginSample("Waaaaaaaaave");
        Debug.Log("code profiling");
        MapCell cell = null;
       
        do
        {
            var cellsWithUnselectedState = _mapCellsArray.Where(c => c.States.Count > 1).ToArray();
            if (cellsWithUnselectedState.Length == 0)
            {
                return;
            }
            
            var minStatesCount = cellsWithUnselectedState.Min(c => c.States.Count);
            
            cell = cellsWithUnselectedState.First(c => c.States.Count == minStatesCount);
         
        } 
        while (cell.TrySelectState(states => states[Random.Range(0,states.Count)]));
        Profiler.EndThreadProfiling();
    }

    void CreateMap()
    {
       
        //var endModule = new Vector2(_MapSize.x, _MapSize.y);
        for (int i = 0; i < _MapSize.x; i++)
        {
            for (int j = 0; j < _MapSize.y; j++)
            {
                var localPosition = new Vector3(i * _CellSize, 0, j * _CellSize);
                
                MapCellsMatrix[i,j].States[0].InstantiatePrefab(this,localPosition);
            }
        }
    }

    void InstanciateStartEndModuleForBfs()
    {
        var startModule = new Vector3(0, 0, 0);
        var endModule = new Vector3(72, 0, 72);
        _firstMapModule.InstantiateSpecificPrefab(this,startModule,_firstMapModule);
        _endMapModule.InstantiateSpecificPrefab(this,endModule,_endMapModule);
    }

    void CreateMapBfs()
    {
        for (int i = 0; i < _MapSize.x; i++)
        {
            for (int j = 0; j < _MapSize.y; j++)
            {
                var localPosition = new Vector3(i * _CellSize, 0, j * _CellSize);
                BFS(MapCellsMatrix[i, j], MapCellsMatrix[i, j]);
            }
        }
    }

    List<MapModuleState> GetMapModules()
    {
        List<MapModuleState> mapModules = new List<MapModuleState>();
        foreach (var module in _mapModules)
        {
            mapModules.AddRange(module.GetMapModuleFromPrefab());
        }

        return mapModules;
    }
    

    public MapModuleContact GetContact(string contactType)
    {
        return _ContactType.First(contact => contact.ContactTypes == contactType);
    }
    
   public bool BFS(MapCell v, MapCell endMapCell)
   {
       //TODO profiler et avoir un résultat positif
        Queue<MapCell> queue = new Queue<MapCell>();
        v.isVisited = true;
        queue.Enqueue(v);
        
        while (queue.Count > 0)
        {
            v = queue.Dequeue();
            
            foreach (var variableNeighbour in v.navigateableNeighbours())
            {
                if (v.navigateableNeighbours().Count == 0)
                {
                    Debug.LogError("neighbour is empty");
                }
                if (variableNeighbour.isVisited == false)
                {
                    queue.Enqueue(variableNeighbour); 
                    variableNeighbour.isVisited = true;
                }
                
            }
            if (v == endMapCell)
            {
                Debug.Log("found path !");
                return true;
            }
        }
        Debug.Log(" not found path !");
        return false;
   }
   
}
