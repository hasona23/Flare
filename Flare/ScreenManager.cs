/*namespace Ember;
TODO: Make Resizable Screen

public class ScreenManager : IDisposable
{
    private RenderTarget2D _screenBuffer;
    private WindowSettings _windowSettings;

    private readonly GameWindow _window;
    private readonly GraphicsDeviceManager _graphics;
    private Rectangle _prevBounds;
    private Rectangle _screenBounds;
    public float Scale = 1;

    public ScreenManager(WindowSettings windowSettings, GameWindow window, GraphicsDeviceManager graphics)
    {
        _windowSettings = windowSettings;
        _window = window;
        _graphics = graphics;
        _screenBuffer = new RenderTarget2D(graphics.GraphicsDevice, windowSettings.Width, windowSettings.Height);
        _screenBounds = new Rectangle(0, 0, windowSettings.Width, windowSettings.Width);
        _prevBounds = _screenBounds;

        if (windowSettings.AllowResizing)
            window.AllowUserResizing = true;
        if (windowSettings.Borderless)
            window.IsBorderless = true;

        window.ClientSizeChanged += OnWindowSizeChanged;

        window.Title = windowSettings.Title;
        graphics.PreferredBackBufferWidth = windowSettings.Width;
        graphics.PreferredBackBufferHeight = windowSettings.Height;
        _graphics.ApplyChanges();

        if (windowSettings.FullScreen)
            IsFullScreen = true;
        OnWindowSizeChanged(null, EventArgs.Empty);

    }

    public Vector2 Resolution => new Vector2(_windowSettings.Width, _windowSettings.Height);
    public Point Size => _screenBounds.Size;
    private bool _isFullScreen;
    public bool IsFullScreen
    {
        get
        {
            return _isFullScreen;
        }
        set
        {
            SetFullScreen(value);
        }
    }

    public void Dispose()
    {
        _screenBuffer.Dispose();
        _window.ClientSizeChanged -= OnWindowSizeChanged;
        GC.SuppressFinalize(this);
    }

    private void SetFullScreen(bool fullScreen)
    {
        _isFullScreen = fullScreen;
        if (IsFullScreen)
        {
            _prevBounds = _window.ClientBounds;
            _window.AllowUserResizing = false;
            _window.IsBorderless = true;

            _window.Position = Point.Zero;
            _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
        }
        else
        {
            if (!_windowSettings.Borderless)
                _window.IsBorderless = false;
            if (_windowSettings.AllowResizing)
                _window.AllowUserResizing = true;

            _window.Position = _prevBounds.Location;
            _graphics.PreferredBackBufferWidth = _prevBounds.Width;
            _graphics.PreferredBackBufferHeight = _prevBounds.Height;
            _graphics.ApplyChanges();
        }

        OnWindowSizeChanged(null, EventArgs.Empty);
    }

    public void ChangeResolution(Point resolution)
    {
        _windowSettings.Width = resolution.X;
        _windowSettings.Height = resolution.Y;
        _screenBuffer.Dispose();
        _screenBuffer = new RenderTarget2D(_graphics.GraphicsDevice, resolution.X, resolution.Y);
    }
    public void OnWindowSizeChanged(object? sender, EventArgs e)
    {
        var windowSize = _window.ClientBounds.Size.ToVector2();
        if (windowSize.X < _windowSettings.Width)
            _graphics.PreferredBackBufferWidth = _windowSettings.Width;
        if (windowSize.Y < _windowSettings.Height)
            _graphics.PreferredBackBufferHeight = _windowSettings.Height;
        _graphics.ApplyChanges();

        windowSize = _window.ClientBounds.Size.ToVector2();

        var scaleVector = new Vector2(windowSize.X / _windowSettings.Width,
            windowSize.Y / _windowSettings.Height);
        Scale = MathF.Min(scaleVector.X, scaleVector.Y);
        var newResolution = new Vector2(_windowSettings.Width, _windowSettings.Height) * Scale;
       
        _screenBounds.X = (int)((windowSize.X - newResolution.X) / 2);
        _screenBounds.Y = (int)((windowSize.Y - newResolution.Y) / 2);
        _screenBounds.Size = newResolution.ToPoint();
        _screenBounds = new Rectangle((int)((windowSize.X - newResolution.X) / 2), (int)((windowSize.Y - newResolution.Y) / 2),
            (int)newResolution.X, (int)newResolution.Y);

       

    }


    public Vector2 GetAdjustedMousePosition()
    {
        return (Mouse.GetState().Position - _screenBounds.Location).ToVector2() / Scale;
    }

    public void AttachScreenBuffer(Color? backgroundColor = null)
    {
        _graphics.GraphicsDevice.SetRenderTarget(_screenBuffer);
        if (backgroundColor != null)
            _graphics.GraphicsDevice.Clear(backgroundColor.Value);
    }

    public void DetachScreenBuffer()
    {
        _graphics.GraphicsDevice.SetRenderTarget(null);
    }

    public void DrawScreen(SpriteBatch spriteBatch, Color? backgroundColor = null)
    {

        _graphics.GraphicsDevice.Clear(backgroundColor ?? Color.Black);
        spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        spriteBatch.Draw(_screenBuffer, _screenBounds, Color.White);
        spriteBatch.End();
    }

}*/