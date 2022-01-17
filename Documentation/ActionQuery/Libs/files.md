# File manipulation library (`files`)

## How it works

This library is using a C# type `PostedFile` to represent binary data and/or files - either uploaded or on disk. The type has the following structure:

```c#
    class PostedFile
    {
        string ContentType;
        string FileName;
        long Length;
        string Name; 
        Stream OpenReadStream();
    }
```

Not all members are shown above, only the ones concerning directly the ActionQuery scripts. The Name property is also not very useful when used in AC scripts (it can be set, but nothing else can be done with it).

Whenever the further usage of a PostedFile does not require some of the properties, they can be left empty. 

The OpenReadStream method is not directly accessible from scripts, but it is used internally as result of some calls. As seen above it returns a read stream through which the data is read and processed further (saved to disk, used as source for a bitmap etc.). See also the [basicimage](basicimage.md) library.

## Transactions

NodeScripter and Scripter

## Functions

**`IsPostedFile(arg)`** - checks whether arg is a `PostedFile` and returns `true` / `false`.

**`PostedFileSize(arg)`** - returns the size of the `PostedFile`. If arg is not a `PostedFile` throws an exception.

**`PostedFileName(arg)`** - returns the FileName of the `PostedFile`. If arg is not a `PostedFile` throws an exception.

**`PostedFileType(arg)`** - returns the content type of the `PostedFile`. If arg is not a `PostedFile` throws an exception.

**`CombinePaths(path1, path2)`** - combines path1 and path2. path2 must not be fully qualified path.

**`DeleteFile(fullPath)`** - schedules the file for deletion on successful transaction. If the script is executed without a transaction, deletes the file immediately.

**`SaveFile(file, path)`** - Saves the PostedFile specified by file to path. If any of them is missing or is not of the expected type, throws an exception.

**`PrependFileName(prefix, fileName)`** - prepends the file name with prefix converted to string.

**`CreateDirectory(path)`** - creates the directories up to path.

**`SaveFileToSpread(basedir, spread, id, file)`** - saves the PostedFile (file) to a directory in the spread. The `spread` and `id` must be integer. `basedir` is the directory under which the spread is created.

**`PostedFile(path, contentType)`**
**`PostedFile(postedFile, contentType, name, filename)`** - Create a PostedFile from file on disk or from another PostedFile with the specified content type, name and filename. `name` is rarely needed - mostly if the data is sent to a consumer that will treat it as uploaded file.

**`FileResponse(postedFile)`** - Configures CoreKraft for binary response with the content of the file. It is recommended to follow this function with `BailOut()`.

**`FileExists(path)`**