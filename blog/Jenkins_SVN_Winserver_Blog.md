**前言：**
最近在做NetCore项目，因为项目是部署在WinServer的IIS上(别问我为啥不用Linux)，而更新的办法是采用`mstsc`(远程桌面)后手动复制覆盖dll文件。
而作为一个只会CRUD的菜鸟本鸟，在每次修改完代码提交SVN以后，都得知会专人等待发布更新，相当痛苦。
由于是项目开发初期，代码的版本迭代很频繁，不及时更新造成的结果就是与前端的对接也相应延后，
项目进度拖延是肯定的，这好吗？这不好！由此可见部署一套可行的版本管理、CI/CD方案是多么重要。下面本人将展示，自己安装配置这一套的全过程（多图警告!）
![](./Jenkins_SVN_Winserver_Blog_Img/bq3.jpg)


**1. 准备工具：**
 * [Jdk 8](https://www.oracle.com/java/technologies/javase/javase-jdk8-downloads.html)(用于支撑Jenkins运行)
 * [Jenkins](https://www.jenkins.io/download/thank-you-downloading-windows-installer-stable/)（CI/CD主要工具）
 * [SVN Server](https://tortoisesvn.net/downloads.zh.html) (搭建本地svn服务)
 * [TortoiseSVN](https://tortoisesvn.net/downloads.zh.html) (连接svn仓库的工具)
 * [NetCore 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-5.0.101-windows-x64-installer/)（net5开发运行环境）

**2. 搭建SVN Server服务并建立svn仓库**
 * 下载上面的安装包以后，我们就开始安装SVNServer了，一路下一步就ok了，这里我指定端口为**4437**。

   ![](./Jenkins_SVN_Winserver_Blog_Img/installSVNServer1.png)

   ![](./Jenkins_SVN_Winserver_Blog_Img/installSVNServer2.png)

 * 安装完了SVN服务，我们就要建立一个用于管理代码版本的SVN仓库了

   ![](./Jenkins_SVN_Winserver_Blog_Img/createRepo1.png)

 * 这里建一个名为**Net5Repo**的仓库

   ![](./Jenkins_SVN_Winserver_Blog_Img/createRepo2.png)

 * 并建一个具有读写权利的用户**admin**
 
   ![](./Jenkins_SVN_Winserver_Blog_Img/createRepo3.png)

 * 建完了仓库，我们再把SVN连接工具安装上（也是一路下一步，这里不多赘述）。

   ![](./Jenkins_SVN_Winserver_Blog_Img/createRepo4.png)

 * 然后找个文件夹CheckOut下来。因为我们是搭的本地SVN服务，路径就是https://localhost:4437/svn/ **{仓库名}**，这里 `{仓库名}`就是上面我们建的`Net5Repo`，

   ![](./Jenkins_SVN_Winserver_Blog_Img/checkoutRepo2.png)

 * 点击OK后会验证账户密码，输入正确以后就能把仓库拉取下来了。

   ![](./Jenkins_SVN_Winserver_Blog_Img/checkoutRepo.png)

**3. 新建Net5.0项目,并传至svn仓库**
 * 打开VS2019新建Net5.0项目，

   ![](./Jenkins_SVN_Winserver_Blog_Img/createVSProJ1.png)

 * 这里我们就建一个名为NetApi的Web项目

   ![](./Jenkins_SVN_Winserver_Blog_Img/createVSProJ2.png)

 * 注意选择NetCore版本

   ![](./Jenkins_SVN_Winserver_Blog_Img/createVSProJ3.png)

 * 这里用运行`win+r`打开`cmd`，`cd`到项目根目录，然后启动项目。

   `cd C:\WorkSpace\SVN\Net5Repo\NetApi`
   `dotnet run`

   ![](./Jenkins_SVN_Winserver_Blog_Img/runProj1.png)

 * 因为`net 5.0`集成了`swagger`，因此我们可以直接访问Api地址: https://localhost:5001/swagger/index.html

   ![](./Jenkins_SVN_Winserver_Blog_Img/runProj2.png)
 * swagger可以代替接口文档对接前端，实际开发中只要把这个地址丢过去就行了，简直不要太完美。![](./Jenkins_SVN_Winserver_Blog_Img/)

 * 提交我们项目的第一版到SVN仓库
   ![](./Jenkins_SVN_Winserver_Blog_Img/CommitSVN1.png)

 * 但是可以看到 提交的文件非常多，乱七八糟，有些是vs编译产生的临时文件，例如`/bin`、`/obj`、`*.csproj.user`(vs用户的本地配置文件)，我们肯定是不能维护进版本库的，因此添加规则，忽略之。
      * 首先右键我们的项目文件夹`NetApi`，依次选择`TortoiesSVN`->`Properties`，点击`New...`，选择`Other`，属性名选择`svn:global-ignores`，然后敲入`bin`和`obj`，点击ok。
        ![](./Jenkins_SVN_Winserver_Blog_Img/CommitSVN2.png)

      * 然后对`NetApi`文件夹下的`bin`以及`obj`文件夹标记为删除，最后提交。这样一来编译产生的文件就不会出现在提交列表里，非常的干净清爽。
        ![](./Jenkins_SVN_Winserver_Blog_Img/CommitSVN3.png)

**4. 安装并配置jenkins**
 * 首先安装Jenkins
      * 这里把端口指定为**8888**。

        ![](./Jenkins_SVN_Winserver_Blog_Img/installJenkins1.jpg)
      * 然后按提示步骤一步步下去。

        ![](./Jenkins_SVN_Winserver_Blog_Img/installJenkins2.png)
      * 这里要注意选择**安装推荐的插件**，可以省不少事。

        ![](./Jenkins_SVN_Winserver_Blog_Img/installJenkins3.png)
      * 安装完插件重启后，会让你创建管理员，这里我们创建一个`admin`账户。

        ![](./Jenkins_SVN_Winserver_Blog_Img/installJenkins4.png)
 * 然后登录jenkins

   ![](./Jenkins_SVN_Winserver_Blog_Img/loginJenkins.png)

 * 安装`Subversion`插件，依次点击`Manage Jenkins`->`Manage Plugins`,搜索`Subversion`勾选安装并重启。
 * 接下来就开始配置任务了，我们在首页点击`新建Item`，任务名就叫`NetApi`，选择`FreeStyle Project` (yeah~ freestyle!)![](./Jenkins_SVN_Winserver_Blog_Img/bq2.gif)

   ![](./Jenkins_SVN_Winserver_Blog_Img/createItem1.png)

 1. 我们在源码管理里选择`Subversion`(SVN)
      * `Credentials`就是凭据，也就是我们的`svn`用户连接账号`admin`，添加之。

        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem2.png)

      * `Repository URL`里输入我们前面建的本地svn仓库地址（https://localhost:4437/svn/Net5Repo/NetApi ）

        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem3.png)

 2. `构建触发器`里定义条件，选择`Poll SCM`(具体规则可以点击右侧的![](./Jenkins_SVN_Winserver_Blog_Img/wenhao.png)查看)，这里我们设定5分钟检测一次 `H/5 * * * *`
   ![](./Jenkins_SVN_Winserver_Blog_Img/createItem4.png)

 3. 我们先保存后点击`Build Now`构建一次看看效果。可以看到东西都被下载到了工作区。
   ![](./Jenkins_SVN_Winserver_Blog_Img/createItem5.png)

 4. 这里我们以Jenkins构建后的工作区（workspace）为主，编译发布`NetCore`项目，然后建立成服务。
      * 我们打开`cmd`，`cd`到工作区里，一般文件路径为：主目录路径(可在首页点击`Manage Jenkins`->`Configure System`中查看)\workspace\任务名（这里我们是NetApi）
      `cd C:\Windows\System32\config\systemprofile\AppData\Local\Jenkins\.jenkins\workspace\NetApi`

      * 然后发布到指定文件夹（[微软dotnet CLI指令文档](https://docs.microsoft.com/zh-cn/dotnet/core/tools/dotnet-publish)），这里指定为 C:\WorkSpace\PublishSite\NetApi
      `dotnet publish -o C:\WorkSpace\PublishSite\NetApi`
      
        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem6.png)

      * 可以看到编译后的文件都被发布到了我们指定的目录下，

        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem7.png)

      * 接着我们以**管理员方式打开cmd**，输入以下指令，建立服务。服务名为`NetApi`，`bindPath`需要绝对路径指定到具体的`*.exe`文件，端口我们设置为`8800`，启动方式为`auto`(自动)，然后敲入启动指令，却发现失败了。
      `sc create NetApi binPath= "C:\WorkSpace\PublishSite\NetApi\NetApi.exe --urls=\"http://127.0.0.1:8800\"" DisplayName= "net接口" start= auto`

        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem8.png)
      * [查阅文档](https://docs.microsoft.com/zh-cn/aspnet/core/host-and-deploy/windows-service?view=aspnetcore-5.0&tabs=visual-studio#app-configuration)是缺少了引用包，[Microsoft.Extensions.Hosting.WindowsServices](https://www.nuget.org/packages/Microsoft.Extensions.Hosting.WindowsServices) 与[Microsoft.AspNetCore.Hosting.WindowsServices](https://www.nuget.org/packages/Microsoft.AspNetCore.Hosting.WindowsServices)，并修改`Program.cs`的`IHostBuilder`方法。我们照做之，代码如下。
        ```
        namespace NetApi
        {
            public class Program
            {
                public static void Main(string[] args)
                {
                    CreateHostBuilder(args).Build().Run();
                }
                public static IHostBuilder CreateHostBuilder(string[] args) =>
                     Host.CreateDefaultBuilder(args)
                        .UseWindowsService()//添加此行
                        .ConfigureWebHostDefaults(webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                        });
            }
        }
        ```
      * 然而此处有一个坑，因为发布后的版本采用的环境是`Production`，新建项目默认只在`Development`开启`Swagger`,因此我们需要修改`Startup.cs`的`Configure`方法。
        ```
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NetApi v1"));
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        ```
      * 然后我们提交`svn`，可以看到`jenkins`自动构建了本次提交。

        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem12.png)

      * 重复上述`cd`到`jenkins`工作区，然后发布`dotnet publish -o `的工作，我们成功注册了NetApi的服务，并启动之。

        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem9.png)
      
      * 然后打开浏览器访问 http://localhost:8800/swagger/index.html 可以看到服务起来了。
      
        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem10.png)

      * 打开任务管理器也能看到

        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem11.png)

      * 如此一来我们也就清楚了，该如何利用`cmd`的命令行来编译、发布并部署`Net5.0`项目，继续配置`jenkins`

 5. 在`构建`里我们选择`Execute Windows batch command`。并贴入如下代码并保存。
      ```
      sc stop NetApi
      dotnet publish -o C:\WorkSpace\PublishSite\NetApi
      sc start NetApi
      ```
      * 点击`Build Now`，检查下是否构建成功。

        ![](./Jenkins_SVN_Winserver_Blog_Img/createItem12.png)

      * 可以看到一切顺利，服务也成功跑起来了。

**好了到此，我们就算是把这一套CI/CD给安装配置完毕了，写个简单接口测试下**
 * 新建控制器 UserController.cs，添加Login的Get方法，返回success字符串。
   ![](./Jenkins_SVN_Winserver_Blog_Img/test1.png)

 * 然后提交svn。

   ![](./Jenkins_SVN_Winserver_Blog_Img/test2.png)

 * 直接打开swagger页面，刷新下，自动发布更新了。。![](./Jenkins_SVN_Winserver_Blog_Img/bq4.jpg)

   ![](./Jenkins_SVN_Winserver_Blog_Img/test3.jpg)

 * 看下jenkins自动构建了。

   ![](./Jenkins_SVN_Winserver_Blog_Img/test4.png)

**完毕。**

