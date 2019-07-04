**Azure Blob Storage**

Helper library for the client library that enables working with the Microsoft Azure storage services which include the blob and file service for storing binary and text data.
. Unofficial. 

Methods available 

• SaveFile

• LoadFile

• UploadBlob

• DeleteFile

• RetrieveBlob

• FileExists

**Setup**

Available on NuGet: https://www.nuget.org/packages/LeoJHarris.XForms.Plugin.BlobStorageHelper/1.0.0 into your .forms, .android and .iOS projects

**Usage**

```csharp
Stream stream = await ImageService.Instance
.LoadUrl("https://cdn4.iconfinder.com/data/icons/iconsimple-logotypes/512/github-128.png")
.AsPNGStreamAsync();

Uri uri = await LeoJHarris.XForms.Plugin.BlobStorageHelper.CrossBlobStorageHelper.Current
.UploadBlob("#YOUR CONTAINER NAME ON AZURE#", Guid.Parse("FileName"), "#CONNECTION STRING#", stream);
```

**License**

Licensed under MIT, see license file
