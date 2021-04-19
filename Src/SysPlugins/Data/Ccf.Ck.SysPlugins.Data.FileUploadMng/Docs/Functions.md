# FileUploadMng functions




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