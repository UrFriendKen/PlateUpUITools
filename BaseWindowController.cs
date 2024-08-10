using KitchenUITools.Utils;
using UnityEngine;

namespace KitchenUITools
{
    public abstract class BaseWindowController : MonoBehaviour
    {
        private enum SizeClampMode
        {
            Height,
            Width
        }

        private Texture2D _backgroundTexture = new Texture2D(100, 100, TextureFormat.RGBAFloat, mipChain: false);
        protected Texture2D BackgroundTexture => _backgroundTexture;

        private Color _backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        public Color BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                _backgroundColor = value;
            }
        }

        private GUIStyle _windowStyle;
        private GUIStyle _verticalScrollbarStyle;
        private GUIStyle _horizontalScrollbarStyle;

        public const float STANDARD_ASPECT_RATIO = 16f / 9f;
        public const float STANDARD_HEIGHT = 1080;
        public const float STANDARD_WIDTH = STANDARD_ASPECT_RATIO * STANDARD_HEIGHT;
        private float _windowScale = 0.75f;
        public float _aspectRatio = STANDARD_ASPECT_RATIO;
        private readonly float _screenAspectRatio = Screen.width / Screen.height; 
        private SizeClampMode _sizeClampMode => AspectRatio > _screenAspectRatio ? SizeClampMode.Width : SizeClampMode.Height;
        private float _contentScale = 2f;

        public bool AllowWindowResize = true;
        private bool _isResizingHorizontal = false;
        private bool _isResizingVertical = false;
        private Vector2 _startResizeWindowSize = Vector2.zero;
        private Vector2 _mouseStartDragPosition = Vector2.zero;

        public bool AllowScrollingContentResize = true;
        public float MinContentScale = 0.1f;
        public float MaxContentScale = 10f;
        private float ScrollZoomSensitivity = 0.1f;
        public float ContentScale
        {
            get
            {
                return _contentScale;
            }
            set
            {
                float oldScale = _contentScale;
                _contentScale = Mathf.Clamp(value, MinContentScale, MaxContentScale);
                float factor = oldScale / _contentScale;
                if (_windowRect != default)
                {
                    _windowRect.height *= factor;
                    _windowRect.width = _windowRect.height * AspectRatio;
                    _windowRect.x *= factor;
                    _windowRect.y *= factor;
                }
            }
        }
        public float AspectRatio
        {
            get
            {
                return _aspectRatio;
            }
            set
            {
                _aspectRatio = value;
                ClampWindowSize();
            }
        }
        public float WindowScale
        {
            get
            {
                return _windowScale;
            }
            set
            {
                _windowScale = value;
                ClampWindowSize();
            }
        }
        public float WindowHeight
        {
            get
            {
                return GetWindowSize().y;
            }
            set
            {
                if (_windowRect != default)
                    _windowRect.height = value;
                else
                    _windowScale = value / STANDARD_HEIGHT;
                ClampWindowSize();
            }
        }
        public float WindowWidth
        {
            get
            {
                return GetWindowSize().x;
            }
            set
            {
                if (_windowRect != default)
                    _windowRect.height = value;
                else
                    _windowScale = value / STANDARD_HEIGHT;
                ClampWindowSize();
            }
        }

        private Vector2 _scaleVector => new Vector2((float)Screen.height / STANDARD_HEIGHT, (float)Screen.height / STANDARD_HEIGHT);

        private bool _isActive = false;
        public bool IsActive => _isActive;

        public void SetActive(bool active)
        {
            _isActive = active;
        }
        public void Show()
        {
            _isActive = true;
        }
        public void Hide()
        {
            _isActive = false;
        }

        public string WindowName = string.Empty;
        private Rect _windowRect;

        public BaseWindowController()
        {
            for (int i = 0; i < _backgroundTexture.width; i++)
            {
                for (int j = 0; j < _backgroundTexture.height; j++)
                {
                    if (i >= j)
                        _backgroundTexture.SetPixel(i, j, Color.gray);
                }
            }
            _backgroundTexture.Apply();
        }


        private int? _windowID = null;
        protected int WindowID => _windowID ?? 0;
        private Vector2 _contentScrollPosition = Vector2.zero;


        private Color? _defaultColor;
        protected Color GUIDefaultColor => _defaultColor ?? Color.white;
        private Color? _defaultContentColor;
        protected Color GUIDefaultContentColor => _defaultContentColor ?? Color.white;
        private Color? _defaultBackgroundColor;
        protected Color GUIDefaultBackgroundColor => _defaultBackgroundColor ?? Color.black;

        public void OnGUI()
        {
            if (_defaultColor == null || _defaultContentColor == null || _defaultBackgroundColor == null)
            {
                _defaultColor = GUI.color;
                _defaultContentColor = GUI.contentColor;
                _defaultBackgroundColor = GUI.backgroundColor;
            }

            if (_windowStyle == null)
                _windowStyle = new GUIStyle(GUI.skin.window);
            if (_horizontalScrollbarStyle == null)
                _horizontalScrollbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar);
            if (_verticalScrollbarStyle == null)
                _verticalScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar);

            if (_isActive)
            {
                if (_windowRect == default)
                {
                    float startHeight;
                    float startWidth;
                    if (AspectRatio <= STANDARD_ASPECT_RATIO)
                    {
                        startHeight = STANDARD_HEIGHT / _contentScale * _windowScale;
                        startWidth = startHeight * AspectRatio;
                    }
                    else
                    {
                        startWidth = STANDARD_WIDTH / _contentScale * _windowScale;
                        startHeight = startWidth / AspectRatio;
                    }
                    float startX = (STANDARD_WIDTH / _contentScale - startWidth) / 2f;
                    float startY = (STANDARD_HEIGHT / _contentScale - startHeight) / 2f;
                    _windowRect = new Rect(startX, startY, startWidth, startHeight);
                }
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0f, (STANDARD_WIDTH / _contentScale) - _windowRect.width);
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0f, (STANDARD_HEIGHT / _contentScale) - _windowRect.height);
                if (_windowID == null)
                {
                    _windowID = HashUtils.GetInt32HashCode($"{GetType()}:{WindowName}");
                }
                GUIUtility.ScaleAroundPivot(_scaleVector * _contentScale, new Vector2(0f, 0f));
                GUI.backgroundColor = _backgroundColor;
                GUILayout.BeginArea(new Rect(0, 0, STANDARD_WIDTH, STANDARD_HEIGHT));
                _windowRect = GUILayout.Window(_windowID.Value, _windowRect, DrawWindow, WindowName, _windowStyle);
                GUILayout.EndArea();
            }
        }
        private void DrawWindow(int windowID)
        {
            GUILayout.Space(2);
            if (AllowScrollingContentResize)
                ScrollZoom();
            Rect resizeRect = new Rect(_windowRect.width - 20f, _windowRect.height - 20f, 20f, 20f);
            if (AllowWindowResize)
                GUI.DrawTexture(resizeRect, BackgroundTexture, ScaleMode.StretchToFill);
            _contentScrollPosition = GUILayout.BeginScrollView(_contentScrollPosition, false, false, GUIStyle.none, _verticalScrollbarStyle);
            
            DrawWindowContent(windowID);
            GUILayout.EndScrollView();
            if (AllowWindowResize)
                DragResize(resizeRect, resizeRect);
            GUI.DragWindow();
        }
        private void ClampWindowSize()
        {
            if (_windowRect != default)
            {
                _windowRect.width = Mathf.Clamp(_windowRect.width, 0f, (STANDARD_WIDTH / _contentScale) - _windowRect.x);
                _windowRect.height = Mathf.Clamp(_windowRect.height, 0f, (STANDARD_HEIGHT / _contentScale) - _windowRect.y);
                _aspectRatio = _windowRect.width / _windowRect.height;
                return;
            }
            _windowScale = Mathf.Clamp01(_windowScale);
            _aspectRatio = _aspectRatio < 0f ? 0f : _aspectRatio;
        }
        private void DragResize(Rect hortizontalHandle, Rect verticalHandle)
        {
            Event e = Event.current;
            if (e.type == EventType.MouseDown)
            {
                _isResizingHorizontal = hortizontalHandle.Contains(e.mousePosition);
                _isResizingVertical = verticalHandle.Contains(e.mousePosition);
                if (_isResizingHorizontal || _isResizingVertical)
                {
                    _startResizeWindowSize = new Vector2(_windowRect.size.x, _windowRect.size.y);
                    _mouseStartDragPosition = new Vector2(e.mousePosition.x, e.mousePosition.y);
                    e.Use();
                }
                return;
            }
            if (e.type == EventType.MouseUp)
            {
                _isResizingHorizontal = false;
                _isResizingVertical = false;
                _windowScale = _windowRect.height / STANDARD_HEIGHT;
                AspectRatio = _windowRect.width / _windowRect.height;
                e.Use();
                return;
            }
            if ((_isResizingHorizontal || _isResizingVertical)  && e.type == EventType.MouseDrag)
            {
                float relScaleX = e.mousePosition.x / _mouseStartDragPosition.x;
                float relScaleY = e.mousePosition.y / _mouseStartDragPosition.y;

                if (_isResizingHorizontal)
                    _windowRect.width = relScaleX * _startResizeWindowSize.x;
                if (_isResizingVertical)
                    _windowRect.height = relScaleY * _startResizeWindowSize.y;
                ClampWindowSize();
                _windowScale = _windowRect.height / STANDARD_HEIGHT;
                AspectRatio = _windowRect.width / _windowRect.height;
                e.Use();
                return;
            }
        }
        private void ScrollZoom()
        {
            Event e = Event.current;
            if (e.type == EventType.ScrollWheel && (e.modifiers.HasFlag(EventModifiers.Control) || e.modifiers.HasFlag(EventModifiers.Command)))
            {
                ContentScale -= e.delta.y * ScrollZoomSensitivity;
            }
        }
        private Vector2 GetWindowSize()
        {
            float width;
            float height;
            if (_windowRect == default)
            {
                switch (_sizeClampMode)
                {
                    case SizeClampMode.Width:
                        width = STANDARD_WIDTH / _contentScale * _windowScale;
                        height = width / AspectRatio;
                        break;
                    default:
                    case SizeClampMode.Height:
                        height = STANDARD_HEIGHT / _contentScale * _windowScale;
                        width = height * AspectRatio;
                        break;
                }
            }
            else
            {
                width = _windowRect.width;
                height = _windowRect.height;
            }
            return new Vector2(width, height);
        }
        protected abstract void DrawWindowContent(int windowID);
    }
}
