using Kitchen;
using KitchenData;
using System;
using System.Collections.Generic;
using UITools.Patches;
using UnityEngine;

namespace UITools
{
    public static class UITools
    {
        private const string FILE_EXPLORER_NAME = "File Explorer";

        private static GameObject Container;
        private static Dictionary<string, BaseWindowController> _controllers = new Dictionary<string, BaseWindowController>();

        public static TController RequestWindow<TController>(string windowName, Vector2 windowSize = default, float contentScale = 1f, bool startOpen = false) where TController : BaseWindowController, new()
        {
            if (Container == null)
            {
                Container = new GameObject("UITools Container");
            }
            if (windowName == null)
                windowName = "Window Name";
            string windowID = $"{typeof(TController)}:{windowName}";
            if (!_controllers.TryGetValue(windowID, out BaseWindowController windowController))
            {
                GameObject windowGameObject = new GameObject(typeof(TController).ToString());
                windowController = windowGameObject.AddComponent<TController>();
                windowController.WindowName = windowName;
                if (contentScale < 0.1f)
                    contentScale = 0.1f;
                windowController.ContentScale = contentScale;
                if (windowSize != default)
                {
                    windowController.AspectRatio = windowSize.x / windowSize.y;
                    windowController.WindowHeight = windowSize.y;
                }
                windowController.SetActive(startOpen);
                windowGameObject.transform.SetParent(Container.transform, false);
                _controllers.Add(windowID, windowController);
            }
            return (TController)windowController;
        }

        public static void OpenFileDialog(Action<DialogResult, IEnumerable<string>> callback, bool selectMultiple = false, string startDirectory = null)
        {
            FileExplorerController fileExplorer = RequestWindow<FileExplorerController>(FILE_EXPLORER_NAME, windowSize: new Vector2(1920 * 0.6f, 1080 * 0.6f), contentScale: 1.5f, startOpen: false);
            fileExplorer.RequestFile(callback, selectMultiple, startDirectory);
        }

        public static void OpenFolderDialog(Action<DialogResult, IEnumerable<string>> callback, bool selectMultiple = false, string startDirectory = null)
        {
            FileExplorerController fileExplorer = RequestWindow<FileExplorerController>(FILE_EXPLORER_NAME, windowSize: new Vector2(1920 * 0.6f, 1080 * 0.6f), contentScale: 1.5f, startOpen: false);
            fileExplorer.RequestFolder(callback, selectMultiple, startDirectory);
        }

        private static HashSet<StartDayWarning> _usedStartDayWarnings = new HashSet<StartDayWarning>();
        public static StartDayWarningDefinition AddStartDayWarning(string displayText, string description, Func<WarningLevel> getCurrentWarningLevel)
        {
            if(_usedStartDayWarnings == null)
            {
                _usedStartDayWarnings = new HashSet<StartDayWarning>();
                foreach (StartDayWarning vanillaStartDayWarning in Enum.GetValues(typeof(StartDayWarning)))
                {
                    RegisterUsed(vanillaStartDayWarning);
                }
            }
            StartDayWarningDefinition startDayWarningDefinition = new StartDayWarningDefinition(displayText, description, getCurrentWarningLevel);
            int intID = 0;
            while (_usedStartDayWarnings.Contains((StartDayWarning)intID) || intID == 0)
            {
                intID = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            }
            startDayWarningDefinition.ID = (StartDayWarning)intID;
            RegisterUsed(startDayWarningDefinition.ID);
            SStartDayWarnings_Patch.AddWarning(startDayWarningDefinition);
            RegisterStartDayWarningLocalisation.Register(startDayWarningDefinition);
            return startDayWarningDefinition;

            bool RegisterUsed(StartDayWarning startDayWarning)
            {
                if (!_usedStartDayWarnings.Contains(startDayWarning))
                {
                    _usedStartDayWarnings.Add(startDayWarning);
                    return true;
                }
                return false;
            }
        }
    }
}
