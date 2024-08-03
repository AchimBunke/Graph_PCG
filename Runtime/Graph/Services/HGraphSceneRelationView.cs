using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using Achioto.Gamespace_PCG.Runtime.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Services
{
    /// <summary>
    /// Controls the current visible Relations.
    /// Singleton.
    /// </summary>
    public class HGraphSceneRelationView
    {
        /// <summary>
        /// The RelationView.
        /// </summary>
        CollectionView<HGraphSceneRelation> RelationView { get; }

        static HGraphSceneRelationView _instance;
        public static HGraphSceneRelationView Instance => _instance ?? (_instance = new HGraphSceneRelationView());
        /// <summary>
        /// Called once on build.
        /// </summary>
        private HGraphSceneRelationView()
        {
            RelationView = new CollectionView<HGraphSceneRelation>();
            RelationView.ViewSource = RelationViewSource;
            RelationView.ViewChanged += RelationView_ViewChanged; ;
        }

        /// <summary>
        /// Handles changes of the RelationView.
        /// Enable/Disable Relations depending on the filtered view.
        /// </summary>
        /// <param name="refresh"></param>
        private void RelationView_ViewChanged(object _, CollectionViewChangedEventArgs<HGraphSceneRelation> args)
        {
            var nextView = RelationView.FilteredItems;
            // Remove objects that were destroyed since the last filtered view change.
            currentFilteredView.RemoveAll(x => x == null);
            // Disable all old relations
            foreach (var rel in currentFilteredView.Except(nextView))
                rel.HideLine();

            // Enable all new relations
            foreach (var rel in nextView.Except(currentFilteredView))
                rel.ShowLine();
            currentFilteredView = nextView.ToList();
        }

        /// <summary>
        /// A ObservableCollection used as the RelationViewSource in the RelationView.
        /// </summary>
        public ObservableCollection<HGraphSceneRelation> RelationViewSource { get; private set; } = new();
        /// <summary>
        /// The current filtered view on the source.
        /// </summary>
        List<HGraphSceneRelation> currentFilteredView = new();


    }
}