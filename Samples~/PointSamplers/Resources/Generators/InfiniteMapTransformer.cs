using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(MapBehaviour))]
    public class InfiniteMapTransformer : MonoBehaviour
    {
        [SerializeField] HGraphSpaceSearchSettings _searchSettings = HGraphSpaceSearchSettings.Default;
        [SerializeField] MapBehaviour _mapBehavior;
        Vector3 worldPosition;

        private void OnEnable()
        {
            graph = PCGGraphManager.Instance.PCGGraph;
            spaceSearch = new PCGGraphSpaceSearch(_searchSettings, graph);
            CollapseEventUtils.Instance.Collapsing += Instance_Collapsing;
            if (_mapBehavior == null)
                _mapBehavior = GetComponent<MapBehaviour>();
            if (_mapBehavior == null)
                return;
            _mapBehavior.InitializedMap += OnInitializedMap;
            if (_mapBehavior.Initialized)
                OnInitializedMap();
        }
        private void OnDisable()
        {
            CollapseEventUtils.Instance.Collapsing -= Instance_Collapsing;
        }
        private void Update()
        {
            worldPosition = gameObject.transform.position;
        }

        InfiniteMap _map;
        bool subscribed = false;
        PCGGraph graph;
        PCGGraphSpaceSearch spaceSearch;

        private void OnInitializedMap()
        {
            _map = _mapBehavior.Map;
            slots = new();
            graph = PCGGraphManager.Instance.PCGGraph;
            spaceSearch = new PCGGraphSpaceSearch(_searchSettings, graph);
            if (!subscribed)
               
            subscribed = true;
            if (!Application.isPlaying)
            {
                //_map.SlotAdded += OnSlotAdded;
              
            }
            _manyFountainsModule = ModuleData.Current.First(m => m.Name.Contains("HighProbability_Water_Fountain"));
            _fountainsModule = ModuleData.Current.First(m => m.Name.Contains("Water_Fountain"));
        }

        private void Instance_Collapsing(Slot obj)
        {
            //if (slots.ContainsKey(obj.Position))
            //    return;
            slots[obj.Position] = default;
            UpdateSlotModules_2(obj);
        }

        private ConcurrentDictionary<Vector3Int, byte> slots = new();
        private void OnSlotAdded(Slot obj)
        {
            if (slots.ContainsKey(obj.Position))
                return;
            slots[obj.Position] = default;
            UpdateSlotModules_2(obj);
        }
        private void UpdateSlotModules_2(Slot slot)
        {
            var spaceData = GetNodeDataForSlot(slot);
            //List<Module> modules = slot.Modules.ToList();
            //modules.Remove(_manyFountainsModule);

            List<Module> toRemove = new();
            if (spaceData != null)
            {
                var settings = PCGGraphModuleManager.LoadGraphAttributesIntoMembers(WayFunctionCollapseSettings.Default, spaceData);

                if (!settings.ManyFountains)
                {
                    toRemove.Add(_manyFountainsModule);
                }  
            }
            else
            {
                toRemove.Add(_manyFountainsModule);
            }
            if(toRemove.Count > 0)
                slot.RemoveModules(new ModuleSet(toRemove));

            //slot.Modules = new ModuleSet(modules);

        }
        private HGraphNodeData GetNodeDataForSlot(Slot slot)
        {
            var space = spaceSearch.FindSpace(GetWorldspacePosition(slot.Position));
            if (space == null)
                return default;
            return space;
        }
        private Vector3 GetWorldspacePosition(Vector3Int position)
        {
            return worldPosition + Vector3.up * InfiniteMap.BLOCK_SIZE / 2f
                + position.ToVector3() * InfiniteMap.BLOCK_SIZE;
        }
        Module _manyFountainsModule;
        Module _fountainsModule;
        private void UpdateSlotModules(Slot slot)
        {
            var spaceData = GetNodeDataForSlot(slot);
            List<Module> modules = slot.Modules.ToList();
            modules.Remove(_manyFountainsModule);

            if (spaceData != null)
            {
                var settings = PCGGraphModuleManager.LoadGraphAttributesIntoMembers(WayFunctionCollapseSettings.Default, spaceData);


                if (settings.DisallowRoofs)
                {
                    modules.RemoveAll(m => m.Name.ToLower().Contains("roof"));
                }
                if (settings.ManyFountains && modules.Contains(_fountainsModule))
                {
                    modules.Add(_manyFountainsModule);
                }
            }

            slot.Modules = new ModuleSet(modules);
        }
    }

#if USE_HGRAPH_MODULES
    [PCGGraphModule(nameSpaceName: "wfc", registerAllMembers: true)]
#endif
    public struct WayFunctionCollapseSettings
    {
        public bool DisallowRoofs;
        public bool ManyFountains;
        public static WayFunctionCollapseSettings Default = new WayFunctionCollapseSettings()
        {
            DisallowRoofs = false,
            ManyFountains = false
        };
    }
}
