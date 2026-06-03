
using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using MoonWorks.Storage;

public static class TextUtils
{
    public static unsafe object DeserializeJson(
        TitleStorage storage,
        string filePath,
        Type type,
        JsonSerializerContext context
        )
    {
        if (!storage.GetFileSize(filePath, out var size))
		{
			return null;
		}

        object result;
		var buffer = NativeMemory.Alloc((nuint) size);
		var span = new Span<byte>(buffer, (int) size);
		if (storage.ReadFile(filePath, span))
		{
            result = JsonSerializer.Deserialize(
                span,
                type, 
                context
            );
		}
        else
        {
            result = null;
        }

		NativeMemory.Free(buffer);
		return result;
    }
}