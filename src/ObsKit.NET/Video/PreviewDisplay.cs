using ObsKit.NET.Native.Interop;
using ObsKit.NET.Native.Types;
using ObsKit.NET.Scenes;
using ObsKit.NET.Sources;

namespace ObsKit.NET.Video;

/// <summary>
/// Renders a live preview of the OBS canvas (or a single source) into a native window.
/// Wraps obs_display: OBS draws directly into the window's swap chain on its graphics
/// thread, so the preview costs no extra encoding or CPU copies.
/// </summary>
/// <remarks>
/// On Windows pass an HWND, on macOS an NSView pointer; on Linux use <see cref="CreateX11"/>.
/// The width/height are in physical pixels — on high-DPI displays multiply the control's
/// logical size by its DPI scale factor, and call <see cref="Resize"/> whenever the
/// host control changes size.
/// </remarks>
/// <example>
/// <code>
/// using var preview = new PreviewDisplay(panel.Handle, 1280, 720);
/// // Show a single source instead of the full canvas:
/// preview.Source = gameCapture;
/// // On control resize:
/// preview.Resize((uint)panel.ClientSize.Width, (uint)panel.ClientSize.Height);
/// </code>
/// </example>
public sealed class PreviewDisplay : IDisposable
{
    private const int GsBgra = 5;
    private const int GsZsNone = 0;

    private readonly ObsDisplayHandle _display;
    private readonly ObsDisplay.DrawCallback _drawCallback;
    private ObsSourceHandle _sourceHandle;
    private Source? _source;
    private ObsCanvasHandle _canvasHandle;
    private Canvas? _canvas;
    private bool _disposed;

    /// <summary>
    /// Creates a preview display rendering into a native window.
    /// </summary>
    /// <param name="windowHandle">The native window handle (HWND on Windows, NSView* on macOS).</param>
    /// <param name="width">The initial width of the display surface in physical pixels.</param>
    /// <param name="height">The initial height of the display surface in physical pixels.</param>
    /// <param name="backgroundColor">The letterbox background color as 0xRRGGBB (default black).</param>
    /// <exception cref="InvalidOperationException">The display could not be created (e.g. video not initialized).</exception>
    public PreviewDisplay(nint windowHandle, uint width, uint height, uint backgroundColor = 0)
        : this(CreateNative(windowHandle, width, height, backgroundColor))
    {
    }

    private PreviewDisplay(ObsDisplayHandle display)
    {
        _display = display;
        _drawCallback = OnDraw;
        ObsDisplay.obs_display_add_draw_callback(_display, _drawCallback, 0);
    }

    private static ObsDisplayHandle CreateNative(nint windowHandle, uint width, uint height, uint backgroundColor)
    {
        if (windowHandle == 0)
            throw new ArgumentException("Window handle must not be null.", nameof(windowHandle));

        if (OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException("Use PreviewDisplay.CreateX11 on Linux.");

        var init = new ObsDisplay.GsInitData
        {
            Window = windowHandle,
            Cx = width,
            Cy = height,
            NumBackbuffers = 0,
            Format = GsBgra,
            ZsFormat = GsZsNone,
            Adapter = 0
        };

        var display = ObsDisplay.obs_display_create(ref init, ToObsColor(backgroundColor));
        if (display.IsNull)
            throw new InvalidOperationException("Failed to create display. Ensure OBS video is initialized.");

        return display;
    }

    /// <summary>
    /// Converts a 0xRRGGBB color (the public contract) to the byte order libobs decodes
    /// (vec4_from_rgba reads the low byte as red on little-endian, i.e. 0x00BBGGRR), so a
    /// caller passing 0xFF0000 actually gets red. The letterbox clear forces alpha = 1.
    /// </summary>
    private static uint ToObsColor(uint rgb)
        => ((rgb & 0x0000FFu) << 16) | (rgb & 0x00FF00u) | ((rgb >> 16) & 0x0000FFu);

    /// <summary>
    /// Creates a preview display rendering into an X11 window (Linux).
    /// </summary>
    /// <param name="windowId">The X11 window id.</param>
    /// <param name="display">The X11 Display pointer.</param>
    /// <param name="width">The initial width of the display surface in physical pixels.</param>
    /// <param name="height">The initial height of the display surface in physical pixels.</param>
    /// <param name="backgroundColor">The letterbox background color as 0xRRGGBB (default black).</param>
    /// <exception cref="InvalidOperationException">The display could not be created (e.g. video not initialized).</exception>
    public static PreviewDisplay CreateX11(uint windowId, nint display, uint width, uint height, uint backgroundColor = 0)
    {
        var init = new ObsDisplay.GsInitDataX11
        {
            WindowId = windowId,
            Display = display,
            Cx = width,
            Cy = height,
            NumBackbuffers = 0,
            Format = GsBgra,
            ZsFormat = GsZsNone,
            Adapter = 0
        };

        var handle = ObsDisplay.obs_display_create_x11(ref init, ToObsColor(backgroundColor));
        if (handle.IsNull)
            throw new InvalidOperationException("Failed to create display. Ensure OBS video is initialized.");

        return new PreviewDisplay(handle);
    }

    /// <summary>
    /// Gets or sets the source to preview. When null (default), the full main canvas
    /// is rendered. Setting a source clears <see cref="Canvas"/>.
    /// The display holds a reference to the source while it is assigned.
    /// </summary>
    public Source? Source
    {
        get => _source;
        set
        {
            ThrowIfDisposed();

            // The draw callback reads the handles while the graphics mutex is held,
            // so swapping under the same mutex keeps the references alive for it.
            // The old references are released only after leaving the mutex — a final
            // release can run a full source destroy, which must not happen while
            // holding the global graphics lock.
            ObsSourceHandle oldSource;
            ObsCanvasHandle oldCanvas;

            ObsGraphics.obs_enter_graphics();
            try
            {
                oldSource = _sourceHandle;
                _sourceHandle = value != null
                    ? ObsSource.obs_source_get_ref(value.Handle)
                    : ObsSourceHandle.Null;

                oldCanvas = _canvasHandle;
                _canvasHandle = ObsCanvasHandle.Null;
            }
            finally
            {
                ObsGraphics.obs_leave_graphics();
            }

            if (!oldSource.IsNull)
                ObsSource.obs_source_release(oldSource);
            if (!oldCanvas.IsNull)
                ObsCanvas.obs_canvas_release(oldCanvas);

            _source = value;
            _canvas = null;
        }
    }

    /// <summary>
    /// Gets or sets the canvas to preview (e.g. a vertical canvas). When null (default),
    /// the main canvas is rendered. Setting a canvas clears <see cref="Source"/>.
    /// The display holds a reference to the canvas while it is assigned.
    /// </summary>
    public Canvas? Canvas
    {
        get => _canvas;
        set
        {
            ThrowIfDisposed();

            ObsSourceHandle oldSource;
            ObsCanvasHandle oldCanvas;

            ObsGraphics.obs_enter_graphics();
            try
            {
                oldCanvas = _canvasHandle;
                _canvasHandle = value != null
                    ? ObsCanvas.obs_canvas_get_ref(value.Handle)
                    : ObsCanvasHandle.Null;

                oldSource = _sourceHandle;
                _sourceHandle = ObsSourceHandle.Null;
            }
            finally
            {
                ObsGraphics.obs_leave_graphics();
            }

            if (!oldSource.IsNull)
                ObsSource.obs_source_release(oldSource);
            if (!oldCanvas.IsNull)
                ObsCanvas.obs_canvas_release(oldCanvas);

            _canvas = value;
            _source = null;
        }
    }

    /// <summary>
    /// Gets or sets whether the display is rendered. Disable to pause drawing
    /// (e.g. when the host window is hidden) without destroying the display.
    /// </summary>
    public bool IsEnabled
    {
        get
        {
            ThrowIfDisposed();
            return ObsDisplay.obs_display_enabled(_display);
        }
        set
        {
            ThrowIfDisposed();
            ObsDisplay.obs_display_set_enabled(_display, value ? (byte)1 : (byte)0);
        }
    }

    /// <summary>
    /// Gets the current size of the display surface in physical pixels.
    /// </summary>
    public (uint Width, uint Height) Size
    {
        get
        {
            ThrowIfDisposed();
            ObsDisplay.obs_display_size(_display, out var width, out var height);
            return (width, height);
        }
    }

    /// <summary>
    /// Resizes the display surface. Call when the host control changes size.
    /// </summary>
    /// <param name="width">The new width in physical pixels.</param>
    /// <param name="height">The new height in physical pixels.</param>
    public void Resize(uint width, uint height)
    {
        ThrowIfDisposed();
        ObsDisplay.obs_display_resize(_display, width, height);
    }

    /// <summary>
    /// Sets the letterbox background color.
    /// </summary>
    /// <param name="color">The color as 0xRRGGBB.</param>
    public void SetBackgroundColor(uint color)
    {
        ThrowIfDisposed();
        ObsDisplay.obs_display_set_background_color(_display, ToObsColor(color));
    }

    /// <summary>
    /// Re-evaluates the display's color space (e.g. after toggling HDR).
    /// </summary>
    public void UpdateColorSpace()
    {
        ThrowIfDisposed();
        ObsDisplay.obs_display_update_color_space(_display);
    }

    private void OnDraw(nint param, uint cx, uint cy)
    {
        var sourceHandle = _sourceHandle;
        var canvasHandle = _canvasHandle;
        uint baseWidth, baseHeight;

        if (!sourceHandle.IsNull)
        {
            baseWidth = ObsSource.obs_source_get_width(sourceHandle);
            baseHeight = ObsSource.obs_source_get_height(sourceHandle);
        }
        else if (!canvasHandle.IsNull)
        {
            var canvasInfo = default(ObsVideoInfo);
            if (!ObsCanvas.obs_canvas_get_video_info(canvasHandle, ref canvasInfo))
                return;

            baseWidth = canvasInfo.BaseWidth;
            baseHeight = canvasInfo.BaseHeight;
        }
        else
        {
            var ovi = default(ObsVideoInfo);
            if (!ObsCore.obs_get_video_info(ref ovi))
                return;

            baseWidth = ovi.BaseWidth;
            baseHeight = ovi.BaseHeight;
        }

        if (baseWidth == 0 || baseHeight == 0 || cx == 0 || cy == 0)
            return;

        GetScaleAndCenterPos((int)baseWidth, (int)baseHeight, (int)cx, (int)cy, out var x, out var y, out var scale);

        ObsGraphics.gs_viewport_push();
        ObsGraphics.gs_projection_push();
        ObsGraphics.gs_set_viewport(x, y, (int)(scale * baseWidth), (int)(scale * baseHeight));
        ObsGraphics.gs_ortho(0f, baseWidth, 0f, baseHeight, -100f, 100f);

        if (!sourceHandle.IsNull)
            ObsGraphics.obs_source_video_render(sourceHandle);
        else if (!canvasHandle.IsNull)
            ObsCanvas.obs_render_canvas_texture(canvasHandle);
        else
            ObsDisplay.obs_render_main_texture();

        ObsGraphics.gs_viewport_pop();
        ObsGraphics.gs_projection_pop();
    }

    private static void GetScaleAndCenterPos(int baseCx, int baseCy, int windowCx, int windowCy,
        out int x, out int y, out float scale)
    {
        var windowAspect = (double)windowCx / windowCy;
        var baseAspect = (double)baseCx / baseCy;
        int newCx, newCy;

        if (windowAspect > baseAspect)
        {
            scale = (float)windowCy / baseCy;
            newCx = (int)(windowCy * baseAspect);
            newCy = windowCy;
        }
        else
        {
            scale = (float)windowCx / baseCx;
            newCx = windowCx;
            newCy = (int)(windowCx / baseAspect);
        }

        x = windowCx / 2 - newCx / 2;
        y = windowCy / 2 - newCy / 2;
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary>
    /// Destroys the display and releases the previewed source reference.
    /// </summary>
    public void Dispose()
    {
        ReleaseDisplay();
        GC.SuppressFinalize(this);
    }

    ~PreviewDisplay()
    {
        ReleaseDisplay();
    }

    private void ReleaseDisplay()
    {
        if (_disposed)
            return;

        _disposed = true;

        // After obs_shutdown the native display, its canvas and source are already freed and the
        // global obs pointer is NULL. obs_display_destroy dereferences obs with no guard, so calling
        // it now would crash on the finalizer thread at exit. Skip native teardown when the core is
        // down — everything it would free is already gone.
        if (Obs.IsInitialized)
        {
            // Removing the callback blocks until any in-flight draw finishes,
            // so the source/canvas references can be released safely afterwards.
            ObsDisplay.obs_display_remove_draw_callback(_display, _drawCallback, 0);
            ObsDisplay.obs_display_destroy(_display);

            if (!_sourceHandle.IsNull)
                ObsSource.obs_source_release(_sourceHandle);

            if (!_canvasHandle.IsNull)
                ObsCanvas.obs_canvas_release(_canvasHandle);
        }

        _sourceHandle = ObsSourceHandle.Null;
        _canvasHandle = ObsCanvasHandle.Null;
        _source = null;
        _canvas = null;
    }
}
