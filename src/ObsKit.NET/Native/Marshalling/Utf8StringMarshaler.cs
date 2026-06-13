using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace ObsKit.NET.Native.Marshalling;

/// <summary>
/// Custom marshaler for UTF-8 strings used in P/Invoke calls to libobs.
/// </summary>
[CustomMarshaller(typeof(string), MarshalMode.Default, typeof(Utf8StringMarshaler))]
internal static class Utf8StringMarshaler
{
    public static nint ConvertToUnmanaged(string? managed)
    {
        if (managed is null)
            return 0;

        var byteCount = Encoding.UTF8.GetByteCount(managed) + 1;
        var ptr = Marshal.AllocHGlobal(byteCount);

        unsafe
        {
            var span = new Span<byte>((void*)ptr, byteCount);
            Encoding.UTF8.GetBytes(managed, span);
            span[^1] = 0; // null terminator
        }

        return ptr;
    }

    public static string? ConvertToManaged(nint unmanaged)
    {
        if (unmanaged == 0)
            return null;

        return Marshal.PtrToStringUTF8(unmanaged);
    }

    public static void Free(nint unmanaged)
    {
        if (unmanaged != 0)
            Marshal.FreeHGlobal(unmanaged);
    }
}

/// <summary>
/// Marshaler for strings that should not be freed (owned by OBS).
/// </summary>
[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedOut, typeof(Utf8StringMarshalerNoFree))]
internal static class Utf8StringMarshalerNoFree
{
    public static string? ConvertToManaged(nint unmanaged)
    {
        if (unmanaged == 0)
            return null;

        return Marshal.PtrToStringUTF8(unmanaged);
    }
}
