<!-- header
{
    "title": "Case sensitivity during debugging and on production",
    "description": "Case sensitivity on Linux is a problem which we address on Windows during debugging",
    "keywords": [ "Case", "sensitivity" ]
}
-->
## Case sensitivity during debugging and on production
CoreKraft can be hosted on Windows and Linux. Linux per default is case sensitive, Windows is not. If you develop mainly under Windows the surprise by the first deployment under Linux are guaranteed. In order to help reducing the pain, we have added a case aware file provider. This provider will override the built in PhysicalFileProvider for ContentRootPath and WebRootPath.
Be warned that even under Windows you will start seeing exceptions when the casing is not correct.
The error message will be something like:
```csharp
KraftLogger.LogError($"File or Directory not found: {subpath}. Please check casing!");
```
where the subpath will have more details about the path.

