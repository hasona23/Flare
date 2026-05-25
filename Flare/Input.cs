using System.Runtime.InteropServices;
using Silk.NET.Input;
using Silk.NET.Windowing;
using System.Numerics;
namespace Flare;

public static class Input
{
    private static IInputContext? _inputContext;
    private static IKeyboard?     _keyboard;
    private static IMouse?        _mouse;

    
    private static readonly HashSet<Key> _keysDown     = new();
    private static readonly HashSet<Key> _keysPressed  = new();   
    private static readonly HashSet<Key> _keysReleased = new();


    private static readonly List<Key> _pendingReleasedKeys = new(8);
    private static readonly List<Key> _pendingPressedKeys = new(8);
    private static readonly List<char> _pendingTypedChar = new(8);
    
    private static  string _charBuffer;

    private static readonly HashSet<MouseButton> _mouseDown     = new();
    private static readonly HashSet<MouseButton> _mousePressed  = new();
    private static readonly HashSet<MouseButton> _mouseReleased = new();

   
    public static Vector2 MousePosition { get; private set; }
    public static Vector2 MouseDelta    { get; private set; }
    public static Vector2 ScrollDelta   { get; private set; }

    private static Vector2 _lastMousePosition;
    private static Vector2 _pendingScrollDelta;

   
    public static void InitializeInput(IInputContext  inputContext)
    {
        _inputContext = inputContext;
       
        
        if (_inputContext.Keyboards.Count == 0)
            throw new InvalidOperationException("No keyboard device found.");

        _keyboard = _inputContext.Keyboards[0];
        _keyboard.KeyDown += OnKeyDown;
        _keyboard.KeyUp   += OnKeyUp;
        _keyboard.KeyChar += OnKeyChar;

        if (_inputContext.Mice.Count == 0)
            throw new InvalidOperationException("No mouse device found.");

        _mouse = _inputContext.Mice[0];
        _mouse.MouseDown   += OnMouseDown;
        _mouse.MouseUp     += OnMouseUp;
        _mouse.MouseMove   += OnMouseMove;
        _mouse.Scroll      += OnMouseScroll;

      
        _lastMousePosition = new Vector2(_mouse.Position.X, _mouse.Position.Y);
        MousePosition      = _lastMousePosition;
    }

    

    public static void Update(double deltaTime)
    {
       
        _keysPressed.Clear();
        _keysReleased.Clear();
        _mousePressed.Clear();
        _mouseReleased.Clear();
        
        foreach (var pendingPressedKey in _pendingPressedKeys)
            _keysPressed.Add(pendingPressedKey);
        foreach (var pendingReleasedKey in _pendingReleasedKeys)
            _keysReleased.Add(pendingReleasedKey);
        _charBuffer = CollectionsMarshal.AsSpan(_pendingTypedChar).ToString();
     
        
        _pendingTypedChar.Clear();
        _pendingPressedKeys.Clear();
        _pendingReleasedKeys.Clear();
        
        ScrollDelta        = _pendingScrollDelta;
        _pendingScrollDelta = Vector2.Zero;

       
        MouseDelta         = MousePosition - _lastMousePosition;
        _lastMousePosition = MousePosition;
    }

    
    public static bool IsKeyDown(Key key)     => _keysDown.Contains(key);

   
    public static bool IsKeyUp(Key key)     => !_keysDown.Contains(key);
    
    public static bool IsKeyPressed(Key key)  => _keysPressed.Contains(key);


    public static bool IsKeyReleased(Key key) => _keysReleased.Contains(key);


    public static ReadOnlySpan<char> CharsTyped => _charBuffer.AsSpan();

  

    
    public static bool IsMouseDown(MouseButton button)     => _mouseDown.Contains(button);

    public static bool IsMouseUp(MouseButton button) => !_mouseDown.Contains(button);
   
    public static bool IsMousePressed(MouseButton button)  => _mousePressed.Contains(button);

 
    public static bool IsMouseReleased(MouseButton button) => _mouseReleased.Contains(button);

  

    internal static void Dispose()
    {
        if (_keyboard is not null)
        {
            _keyboard.KeyDown -= OnKeyDown;
            _keyboard.KeyUp   -= OnKeyUp;
            _keyboard.KeyChar -= OnKeyChar;
        }

        if (_mouse is not null)
        {
            _mouse.MouseDown -= OnMouseDown;
            _mouse.MouseUp   -= OnMouseUp;
            _mouse.MouseMove -= OnMouseMove;
            _mouse.Scroll    -= OnMouseScroll;
        }
     
        _inputContext?.Dispose();
        
    }

   

    private static void OnKeyDown(IKeyboard keyboard, Key key, int scanCode)
    {
        if (_keysDown.Add(key))         
            _pendingPressedKeys.Add(key);
    }

    private static void OnKeyUp(IKeyboard keyboard, Key key, int scanCode)
    {
        if(_keysDown.Remove(key))
            _pendingReleasedKeys.Add(key);
    }

    private static void OnKeyChar(IKeyboard keyboard, char character)
    {
        _pendingTypedChar.Add(character);
    }

    private static void OnMouseDown(IMouse mouse, MouseButton button)
    {
        if (_mouseDown.Add(button))
            _mousePressed.Add(button);
    }

    private static void OnMouseUp(IMouse mouse, MouseButton button)
    {
        if(_mouseDown.Remove(button))
            _mouseReleased.Add(button);
    }

    private static void OnMouseMove(IMouse mouse, Vector2 position)
    {
        MousePosition = new Vector2(position.X, position.Y);
    }

    private static void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        _pendingScrollDelta += new Vector2(scroll.X, scroll.Y);
    }
}