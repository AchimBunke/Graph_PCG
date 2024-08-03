using System;
using UnityEditor;
using UnityUtilities.Timers;

namespace Achioto.Gamespace_PCG.Editor.Graph
{
    // Currently not used because HGraph events notifies on changes
#if false
    [Obsolete]
    [InitializeOnLoad]
    public static class PCGGraphManagerUpdater
    {
        const float interval = 3f;
        const float unfocusedInterval = 60000f;
        static ManualTimer _timer;

        static PCGGraphManagerUpdater()
        {
            _timer = new(interval);
            EditorApplication.update += OnEditorUpdate;
            EditorApplication.focusChanged += hasFocus => _timer.Interval = hasFocus ? interval : unfocusedInterval;
            //_timer.Elapsed += () => PCGGraphManager.Instance.SetPCGGraphDirty();
            _timer.StartTimer();
        }
        private static double lastUpdateTime = 0;
        private static void OnEditorUpdate()
        {
            double currentTime = EditorApplication.timeSinceStartup;
            double deltaTime = currentTime - lastUpdateTime;
            _timer.Update((float)deltaTime);
            lastUpdateTime = currentTime;
        }
    }
#endif
}