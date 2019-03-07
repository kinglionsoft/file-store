# 分布式文件系统搭建
Service Fabric + FastDFS + AspNetCore 搭建分布式文件系统和存储服务。

## 安全性
* FastDFS以组为单位存储文件。每个组有多个存储节点（Storage)
* 上传、删除安全性：API 网关认证后可以访问
* 下载安全性：将FastDFS中特定的组定义为非安全组，存储可以公开访问的文件；其他组的文件，API 网关认证后可以访问

## 配置

## 使用


### 上传
* 接口地址：POST {服务器地址}/file/upload
* Content-Type: multipart/form-data
* 参数说明:
- 1. Extension	: string，文件扩展名。不能为空。
- 2. Group 		: string，文件组，可不指定
* 返回值:

``` javascript
{	
	"success": true | false,  // 业务是否成功
	"error":"", // 业务失败的信息
	"data":[ // 文件url:
		"http://{file.domain.com}/group1/M00/00/00/CgAVUlj4JV2ALE_QAABDIrIZd7w317.jpg"
    ]
}
```

### 删除
* 接口地址：POST {服务器地址}/file/delete?url={文件url，URL编码}
* 返回值:

``` javascript
{	
	"success": true | false,  // 业务是否成功
	"error":""
}
```

### 单文件下载

* http://xxx.domain.com/group1/M00/00/01/ooYBAFx2JGKAJdtkAAAw_JvwOOU227.png?filename=2222.png
* 若url中带有文件名（filename）参数，下载是会重命名。
* **filename**必须是最后一个参数

### 打包下载 

#### POST
* 接口地址：POST {服务器地址}/file/download
* Content-Type: application/json
* 参数说明:

``` js
{
	"FileName": "test.zip",
	"Files": {
		"1.png": "http://file.domain.com/group1/M00/00/68/oYYBAFxmcp6AQsyIAAAUa4KwmUU490.png",
		"img/2.png": "http://file.domain.com/group1/M00/00/68/oYYBAFxmcp6AQsyIAAAUa4KwmUU490.png"
	}
}
```
* 返回值: test.zip 文件流

#### GET
* 接口地址：GET {服务器地址}/file/download?filename=test.zip&files[<文件名>]=<url编码后的地址>
* 示例：
```
/file/download?filename=test.zip&files[1.png]=http%3A%2F%2Fstore.yitu666.com%3A8880%2Fgroup1%2FM00%2F00%2F01%2FooYBAFx2JGKAJdtkAAAw_JvwOOU227.png
```
* 返回值: test.zip 文件流

## 示例

### Form
示例：
``` html
<form action="/storage/upload" method="post" enctype="multipart/form-data">
    <ul class="form-ul">
        <li>图片：<input name="file" type="file" accept="image/*" /></li>
        <li>扩展名: <input name="Extension" value="jpg" maxlength="6" /></li>
        <li style="height: 30px;"><input type="submit" value="提交" /></li>
    </ul>
</form>
```

### Ajax
示例：
``` html
<form  method="post" enctype="multipart/form-data" id="uploadForm">
    <ul class="form-ul">
        <li>图片：<input name="file" type="file" accept="image/*"  required/></li>
		<li>扩展名: <input name="Extension" value="jpg" maxlength="6" /></li>
        <li style="height: 30px;"><input type="button" value="提交" id="ajaxSubmitBtn"/></li>
    </ul>
    <p id="result"></p>
</form>
```
``` javascript
<script src="//cdn.bootcss.com/jquery/2.2.1/jquery.min.js"></script>
<script src="//cdn.bootcss.com/jquery.form/4.2.1/jquery.form.min.js"></script>

<script>
    $('#ajaxSubmitBtn').click(function () {
		$('#result').text('提交中。。。');
		$('#uploadForm').ajaxSubmit({
			success: function (data) {
				$('#result').text(JSON.stringify(data));
			},
			error: function (error) { $('#result').text(error.responseText);},
			url: 'http://{file.domain.com}/file/upload', 
			type: 'post', 
			dataType: 'json' ,
			beforeSend: function (xhr) {
				xhr.setRequestHeader('Token', 'yk1234');
				return true;
			}
		});
	});
</script>
```

### Ionic/Cordova
使用[http://ionicframework.com/docs/v2/native/transfer/](http://ionicframework.com/docs/v2/native/transfer/)进行上传。
``` javascript
 fileTransfer.upload("<file path>", "<api endpoint>", {
     params:{
         "Extension":"jpg",
         "Group":"group1"
     }
 })
   .then((data) => {
     // success
   }, (err) => {
     // error
   })
```
### .Net Core 2.0
1. 安装 FileStorage.SDK.Client; 
2. 若使用了DI。以.Net Core DI为例：

``` C#
// 在Startup.ConfigureServices方法中：
services.AddFileStorage(option =>
{
    FileServer = "文件服务器地址", // 测试环境为 192.168.0.237:8080
});

// 使用时，注入IFileStorage
public class HomeController{
    private readonly IFileStorage _storageManager;
    public HomeController(IFileStorage storageManager)
    {
        this._storageManager = storageManager;
    }

    public async Task Upload()
    {
       await this.IFileStorage.UploadAsync("<local file>", "jpg");
    }
}

```

若未使用DI，则直接使用FileStorageFactory。

``` C#
// 全局初始化
FileStorageFactory.Initialize(new FileStorageOption
{
    FileServer = "file.domain.com"
});

// 使用
var storage = FileStorageFactory.Create();
foreach (var file in GetFiles())
{
    var result = await storage.UploadAsync(file);
    Assert.True(result.Success);
    Assert.Equal(1, result.Data.Length);
    var delete = await storage.TryDeleteAsync(result.Data[0]);
    Assert.True(delete);
} 
```
 
## TODOS

* 图片缩略图
* 集成API网关

