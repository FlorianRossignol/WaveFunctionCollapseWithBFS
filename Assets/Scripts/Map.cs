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
    [SerializeField] private Vector2Int _MapSize = new Vector2Int(5, 5);

    [SerializeField] private float _CellSize;

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
    // Start is called before the first frame update
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            InitializeMap2();
            CreateMapBfs();
            //InitializeMap();
            //FillCells();
            //CreateMap();
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
               MapCellsMatrix2[i, j] = new MapCell(this, new Vector2Int(i, j));
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
        for (int i = 0; i < _MapSize.x; i++)
        {
            for (int j = 0; j < _MapSize.y; j++)
            {
                var localPosition = new Vector3(i * _CellSize, 0, j * _CellSize);
                MapCellsMatrix[i,j].States[0].InstantiatePrefab(this,localPosition);
            }
        }
    }

    void CreateMapBfs()
    {
        for (int i = 0; i < _MapSize.x; i++)
        {
            for (int j = 0; j < _MapSize.y; j++)
            {
                BFS(MapCellsMatrix2[i,j]);
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
    
   public void BFS(MapCell v)
   {
       //TODO profiler et avoir un résultat positif
        Queue<MapCell> queue = new Queue<MapCell>();
        
        for (int i = 0; i < _MapSize.x; i++)
        {
            for (int j = 0; j < _MapSize.y; j++)
            {
                v.isVisited = true;
                queue.Enqueue(v);
                //var first = queue.Peek();
                var localPosition = new Vector3(i * _CellSize, 0, j * _CellSize);
                MapCellsMatrix2[i,j].States[0].InstantiateSpecificPrefab(this,localPosition,_firstMapModule);
                while (queue.Count > 0)
                {
                    v = queue.Dequeue();
                    
                    foreach (var variableNeighbour in v.navigateableNeighbours())
                    {
                        if (variableNeighbour.isVisited == false)
                        {
                           MapCellsMatrix2[i,j].States[0].InstantiateSpecificPrefab(this,localPosition,_pathMapModule);
                            queue.Enqueue(variableNeighbour);
                            MapCellsMatrix2[i,j].States[0].InstantiateSpecificPrefab(this,localPosition,_endMapModule);
                        }
                    
                    }
                    
                }
            }
        
            /*LinkedList<int> linkedList = new LinkedList<int>();
            LinkedListNode<int> currentNode = linkedList.First;
            while (currentNode != null && !Visited[currentNode.Value])
            {
        
                //instanciate specific prefab
                _pathMapModule.InstantiatePrefab(this, localPosition);
                queue.Enqueue(currentNode.Value);
                Visited[currentNode.Value] = true;
                currentNode = currentNode.Next;
                _endMapModule.InstantiatePrefab(this, localPosition);
            }*/
        }
    }
    
}
