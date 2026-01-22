# DPI Awareness for DXGI Desktop Duplication

When using DXGI Desktop Duplication (`MonitorCaptureMethod.DesktopDuplication`) for monitor capture on Windows, your application must be configured as **per-monitor DPI aware**.

## The Problem

The DXGI `IDXGIOutput5::DuplicateOutput1` API requires the calling thread to be per-monitor DPI aware. Without this, you will encounter the error:

```
IDXGIOutput5::DuplicateOutput1: The calling thread must be per-monitor DPI aware to use the output duplication APIs.
```

## Solution

Add an application manifest to your project that declares per-monitor DPI awareness.

### Step 1: Create app.manifest

Create a file named `app.manifest` in your project root:

```xml
<?xml version="1.0" encoding="utf-8"?>
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <assemblyIdentity version="1.0.0.0" name="MyApplication.app"/>
  <trustInfo xmlns="urn:schemas-microsoft-com:asm.v2">
    <security>
      <requestedPrivileges xmlns="urn:schemas-microsoft-com:asm.v3">
        <requestedExecutionLevel level="asInvoker" uiAccess="false" />
      </requestedPrivileges>
    </security>
  </trustInfo>

  <application xmlns="urn:schemas-microsoft-com:asm.v3">
    <windowsSettings>
      <!-- Per-monitor DPI awareness V2 - required for DXGI output duplication -->
      <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
      <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true/pm</dpiAware>
    </windowsSettings>
  </application>

</assembly>
```

### Step 2: Reference the manifest in your .csproj

Add the `ApplicationManifest` property to your project file:

```xml
<PropertyGroup>
  <ApplicationManifest>app.manifest</ApplicationManifest>
</PropertyGroup>
```

## Alternative: Use Windows Graphics Capture

If you cannot configure DPI awareness (e.g., library constraints), use Windows Graphics Capture instead:

```csharp
var source = MonitorCapture.FromPrimary()
    .SetCaptureMethod(MonitorCaptureMethod.WindowsGraphicsCapture);
```

Windows Graphics Capture (WGC) is the recommended capture method for Windows 10 1903+ and does not have the DPI awareness requirement.

## More Information

- [Microsoft Docs: High DPI Desktop Application Development](https://docs.microsoft.com/en-us/windows/win32/hidpi/high-dpi-desktop-application-development-on-windows)
- [Microsoft Docs: DPI Awareness Context](https://docs.microsoft.com/en-us/windows/win32/hidpi/dpi-awareness-context)
