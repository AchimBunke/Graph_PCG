/*
 * MIT License
 *
 * Copyright (c) 2024 Achim Bunke
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Settings;
using System;
using UnityEditor;
using UnityUtilities.Timers;

namespace Achioto.Gamespace_PCG.Editor.Graph
{
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
            _timer.Elapsed += () =>
            {
                if (HGraphSettings.GetOrCreateSettings().AutoUpdatePCGGraph)
                    PCGGraphManager.Instance.SetPCGGraphDirty();
            };
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
}