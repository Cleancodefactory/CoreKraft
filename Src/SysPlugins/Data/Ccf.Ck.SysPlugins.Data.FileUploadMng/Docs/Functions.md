# FileUploadMng functions

The ActionQuery (AC) context provided by this plugin includes the standard libraries and includes special functions provided by the plugin.
The functions that handle file saving, deletion and some others are available only for write operations, while the functions backing
file extraction are available only for read operations. The neutral or calculation functions are available in both cases, although some 
of them are not likely to be of any use on one or the other contexts, still they do not perform risky operations and left available.

In the reference below check the availability of the particular function.


## Functions supplied by FileUploadMng

### IsPostedFile(val)

Checks if the argument contains a posted file object (IPostedFile). Posted file objects are used both in write and read operations. In 
write operations they usually come from the client, containing an upload, but the same objects are used for files read from the disk, so
despite their name, they are more general file wrapping object.

```
read {
	parameters:[
		{name: "ct", Expression:"GetFrom('parent',name)" },
		{name: "file", Expression:"GetFrom('parent',name)" }
	]
}


FileResponse(
	PostedFile(
		CombinePaths(CSetting('UploadPath'),file),
		ct
	)
)
```


```
Configuration.json 
------------

Definition.json (some node)
-----------------------------
write {
	parameters:[
		{name: "caption", Expression:"GetFrom('current',name)" },
		{name: "uploadedfile", Expression:"GetFrom('current',name)" },
		{name: "fileid", Expression: "GetFrom('parent', name)"}

	]
}

AC Script
-----------------
if (IsPostedFile(uploadedfile),
	SetResult(
		'path',
		SaveFileToSpread(
			CSetting('BaseUploadPath'),
			Cast('int', CSetting('SpreadDirs')),
			fileid,
			uploadedfile
		),
		'contenttype',
		PostedFileType(uploadedfile),
		'filesize',
		PostedFileSize(uploadedfile)
	),
	Throw('There is no posted file')
)
```