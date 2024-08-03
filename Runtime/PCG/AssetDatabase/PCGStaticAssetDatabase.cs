using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Database
{
    public class PCGStaticAssetDatabase : PCGAssetDatabase
    {
        [SerializeField] private List<Object> _assets;
        public List<Object> Assets { get => _assets; set { _assets = value; } }

        public override IEnumerable<string> GetAssetPaths()
        {
            if (_assets == null)
                return Enumerable.Empty<string>();
            return Assets.Select(a => AssetDatabase.GetAssetPath(a));
        }
    }

    [CustomEditor(typeof(PCGStaticAssetDatabase))]
    public class PCGStaticAssetDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var pcgDatabase = (PCGStaticAssetDatabase)target;
            if (GUILayout.Button("Take From Directory"))
            {
                var directory = EditorUtility.OpenFolderPanel("Asset Folder", Application.dataPath, "");
                var files = Directory.GetFiles(directory);
                Undo.RecordObject(pcgDatabase, "Set PCG Database Assets from directory");
                pcgDatabase.Assets.Clear();
                foreach (var file in files)
                {
                    var relativeFilePath = "Assets" + file.Substring(Application.dataPath.Length);
                    var asset = AssetDatabase.LoadAssetAtPath(relativeFilePath, typeof(Object));
                    if (asset == null)
                        continue;
                    pcgDatabase.Assets.Add(asset);
                }
            }

        }
    }
}
