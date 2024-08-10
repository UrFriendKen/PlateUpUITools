using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KitchenUITools
{
    internal class FileExplorerController : BaseWindowController
    {
        private enum SelectionType
        {
            File,
            Folder
        }

        private bool _selectMultiple = false;
        private SelectionType _selectionType;
        private Action<DialogResult, IEnumerable<string>> _callback;
        private readonly string DEFAULT_START_DIRECTORY = Application.persistentDataPath;
        private string _currentDirectory;

        private string _goToDirectoryText = string.Empty;

        private List<string> _availableFolders = new List<string>();
        private List<string> _availableFiles = new List<string>();
        private Vector2 _foldersScrollPosition = Vector2.zero;
        private Vector2 _filesScrollPosition = Vector2.zero;
        private List<string> _selectedItems = new List<string>();
        private List<string> _filteredExtensions = new List<string>();

        public FileExplorerController()
        {
        }

        /// <summary>
        /// Get user selected files. Performs call back when dialog is completed or cancelled.
        /// DialogResult.OK - One or more file(s) were selected
        /// DialogResult.Cancel - No file(s) selected
        /// </summary>
        /// <param name="selectMultiple"></param>
        /// <param name="callback">Callback when dialog is completed or cancelled</param>
        public void RequestFile(Action<DialogResult, IEnumerable<string>> callback, bool selectMultiple = false, string startDirectory = null)
        {
            RequestItem(SelectionType.File, callback, selectMultiple, startDirectory);
        }

        /// <summary>
        /// Get user selected folders. Performs call back when dialog is completed or cancelled.
        /// DialogResult.OK - One or more folder(s) were selected
        /// DialogResult.Cancel - No folders(s) selected
        /// </summary>
        /// <param name="selectMultiple"></param>
        /// <param name="callback">Callback when dialog is completed or cancelled</param>
        public void RequestFolder(Action<DialogResult, IEnumerable<string>> callback, bool selectMultiple = false, string startDirectory = null)
        {
            RequestItem(SelectionType.Folder, callback, selectMultiple, startDirectory);
        }

        private void RequestItem(SelectionType type, Action<DialogResult, IEnumerable<string>> callback, bool selectMultiple = false, string startDirectory = null)
        {
            _selectMultiple = selectMultiple;
            _selectionType = type;
            _callback = callback;
            if (!Directory.Exists(startDirectory))
                startDirectory = DEFAULT_START_DIRECTORY;
            GoToDirectory(startDirectory);
            Show();
        }

        protected override void DrawWindowContent(int windowID)
        {
            Color defaultContentColor = GUI.contentColor;
            bool isFolderSelection = _selectionType == SelectionType.Folder;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("^", GUILayout.Width(WindowWidth * 0.05f)))
            {
                GoToDirectory(Path.GetDirectoryName(_currentDirectory));
            }
            _goToDirectoryText = GUILayout.TextArea(_goToDirectoryText);
            if (GUILayout.Button("Go", GUILayout.Width(WindowWidth * 0.15f)))
            {
                GoToDirectory(DesanitizeString(_goToDirectoryText));
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _foldersScrollPosition = GUILayout.BeginScrollView(_foldersScrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar, GUILayout.Width(WindowWidth * 0.4f));
            for (int i = 0; i < _availableFolders.Count; i++)
            {
                string path = _availableFolders[i];
                if (GUILayout.Button(SanitizeString(Path.GetFileName(path))))
                {
                    GoToDirectory(path);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.BeginVertical();
            _filesScrollPosition = GUILayout.BeginScrollView(_filesScrollPosition, false, true, GUIStyle.none, GUI.skin.verticalScrollbar);
            List<string> availableItems = isFolderSelection ? _availableFolders : _availableFiles;
            if (availableItems.Count > 0)
            {
                for (int i = 0; i < availableItems.Count; i++)
                {
                    string path = availableItems[i];
                    if (_selectedItems.Contains(path))
                        GUI.contentColor = Color.green;
                    if (GUILayout.Button(SanitizeString(Path.GetFileName(path)), GUI.skin.label))
                    {
                        if (!_selectedItems.Contains(path))
                        {
                            if (!_selectMultiple)
                                _selectedItems.Clear();
                            _selectedItems.Add(path);
                        }
                        else
                        {
                            _selectedItems.Remove(path);
                        }
                    }
                    GUI.contentColor = defaultContentColor;
                }
            }
            else
            {
                GUILayout.Label(string.Empty);
            }
            GUILayout.EndScrollView();
            GUILayout.BeginHorizontal();
            GUI.contentColor = defaultContentColor;
            if (GUILayout.Button("Cancel"))
            {
                _selectedItems.Clear();
                DoCallback(DialogResult.Cancel);
                Hide();
            }
            if (_selectedItems.Count > 0 && GUILayout.Button($"Select {(_selectMultiple ? $"{_selectedItems.Count} " : string.Empty)}{_selectionType}{(_selectedItems.Count > 1 ? "s" : string.Empty)}"))
            {
                DoCallback(DialogResult.OK);
                _selectedItems.Clear();
                Hide();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private bool PathExists(string path)
        {
            return PathExists(path, out _);
        }

        private bool PathExists(string path, out bool isFile)
        {
            if (File.Exists(path))
            {
                isFile = true;
                return true;
            }
            if (Directory.Exists(path))
            {
                isFile = false;
                return true;
            }
            isFile = false;
            return false;
        }

        private bool GoToDirectory(string path)
        {
            if (path == _currentDirectory || !PathExists(path, out bool isFile) || isFile)
                return false;
            _currentDirectory = path;
            _goToDirectoryText = SanitizeString(path);
            _availableFiles = GetFilesAtPath(path).ToList();
            _availableFolders = GetFoldersAtPath(path).ToList();
            _selectedItems.Clear();
            return true;
        }

        private IEnumerable<string> GetFoldersAtPath(string path)
        {
            if (!PathExists(path, out bool isFile) || isFile)
                return new List<string>();
            return Directory.GetDirectories(path);
        }

        private IEnumerable<string> GetFilesAtPath(string path)
        {
            if (!PathExists(path, out bool isFile) || isFile)
                return new List<string>();
            return Directory.GetFiles(path);
        }

        private void DoCallback(DialogResult dialogResult)
        {
            if (_callback != null)
                _callback(dialogResult, new List<string>(_selectedItems));
            _callback = null;
            _selectedItems.Clear();
        }

        private string SanitizeString(string input)
        {
            return input?.Replace(Environment.UserName, "[USERNAME]");
        }

        private string DesanitizeString(string input)
        {
            return input?.Replace("[USERNAME]", Environment.UserName);
        }
    }
}
