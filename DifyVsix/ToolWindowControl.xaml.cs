using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace DifyVsix
{
    /// <summary>
    /// Interaction logic for ToolWindowControl.
    /// </summary>
    public class DifySseData
    {
        // 使用 [JsonProperty("event")] 解决 C# 关键字冲突
        [JsonProperty("event")]
        public string EventName { get; set; }

        [JsonProperty("conversation_id")]
        public string ConversationId { get; set; }

        [JsonProperty("message_id")]
        public string MessageId { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; } // 时间戳通常是 long 或 int

        [JsonProperty("task_id")]
        public string TaskId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("answer")]
        public string Answer { get; set; } // AI 流式输出的内容片段

        // Dify 的 from_variable_selector 可能是字符串数组
        [JsonProperty("from_variable_selector")]
        public List<string> FromVariableSelector { get; set; }
    }
    public partial class ToolWindowControl : UserControl
    {

        //读取解决方案
        private OptionPage optionPage;
        private void ReadWorkspace()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2 dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            if (dte == null || dte.Solution == null)
            {
                System.Diagnostics.Debug.WriteLine("未找到当前解决方案");
                return;
            }

            string solutionName = dte.Solution.FullName;
            System.Diagnostics.Debug.WriteLine($"当前解决方案: {solutionName}");

            foreach (Project project in dte.Solution.Projects)
            {
                System.Diagnostics.Debug.WriteLine($"项目: {project.Name}");
                try
                {
                    foreach (ProjectItem item in project.ProjectItems)
                    {
                        System.Diagnostics.Debug.WriteLine($"  文件: {item.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"读取项目文件失败: {ex.Message}");
                }
            }
        }

        //读取当前文件
        private void ReadCurrentFileContent()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2 dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            if (dte == null || dte.ActiveDocument == null)
            {
                System.Diagnostics.Debug.WriteLine("未找到当前打开的文件");
                return;
            }

            string filePath = dte.ActiveDocument.FullName;
            if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            {
                System.Diagnostics.Debug.WriteLine("当前文件路径无效或文件不存在");
                return;
            }

            try
            {
                string fileContent = System.IO.File.ReadAllText(filePath, Encoding.UTF8);
                System.Diagnostics.Debug.WriteLine($"当前文件路径: {filePath}");
                System.Diagnostics.Debug.WriteLine($"文件内容:\n{fileContent}");

                // 如果需要传递到前端 WebView2，可以这样：
                string safeContent = fileContent
                    .Replace("\\", "\\\\")
                    .Replace("'", "\\'")
                    .Replace("\"", "\\\"")
                    .Replace("\r\n", "\\n")
                    .Replace("\n", "\\n");

                string jsCode = $"showFileContent('{safeContent}');";
                _ = mdview.CoreWebView2.ExecuteScriptAsync(jsCode);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"读取文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindowControl"/> class.
        /// </summary>
        public ToolWindowControl()
        {
            this.InitializeComponent();
        }
        private async System.Threading.Tasks.Task Window_LoadedAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string appName = "YourAppName"; // 请替换为您的项目名称

                string userDataFolder = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(localAppData, appName, "WebView2Data")
                );
                // 3. 创建 CoreWebView2Environment
                CoreWebView2Environment environment = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null, // 使用默认 Edge 安装
                    userDataFolder: userDataFolder // 明确指定可写路径
                );
                // 确保初始化完成
                await mdview.EnsureCoreWebView2Async(environment);

                // 订阅 WebMessageReceived 事件
                if (mdview.CoreWebView2 != null)
                {
                    mdview.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;
                   // 确保启用了 Web 消息传递
                   // 默认通常是启用的，但最好检查一下
                   mdview.CoreWebView2.Settings.IsWebMessageEnabled = true;
                   mdview.ZoomFactor = 1.0;
                }

                ////  加载本地 HTML 字符串
                //string htmlContent = @"";
                //mdview.NavigateToString(htmlContent);

                string assemblyDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // 假设 Step 1 中设置的 HTML 资源文件夹名为 'HtmlAssets'
                string localAssetPath = System.IO.Path.Combine(assemblyDir, "HtmlAssets");

                if (!Directory.Exists(localAssetPath))
                {
                    // 部署失败或路径错误
                    System.Diagnostics.Debug.WriteLine($"错误：未找到 VSIX 资源路径：{localAssetPath}");
                    return;
                }

                // 3. 设置虚拟主机名映射 (Virtual Hostname Mapping)
                // 将 "vsix.local" 这个安全的 HTTPS 虚拟域名映射到本地文件夹
                mdview.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "vsix.local",                               // 虚拟域名 (必须是 HTTPS 方案，例如 vsix.local)
                    localAssetPath,                             // 实际本地文件夹路径
                    CoreWebView2HostResourceAccessKind.Allow    // 允许访问
                );

                // 4. 导航到虚拟 URL
                // 导航到 https://vsix.local/index.html
                mdview.CoreWebView2.Navigate("https://vsix.local/index.html");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"窗体初始化失败: {ex}");
            }
        }
        /// <summary>
        /// 加载配置项
        /// </summary>
        private void LoadOptionPage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            IVsShell shell = (IVsShell)ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            if (shell == null)
            {
                // 核心服务获取失败，非常罕见
                System.Diagnostics.Debug.Fail("无法获取 SVsShell 服务.");
                return;
            }

            // 2. 获取您的 Package GUID
            // 假设 MyVSPackage 是您的主 Package 类名
            Guid packageGuid = new Guid(AICodingPackage.PackageGuidString);

            IVsPackage package;
            int hr = shell.IsPackageLoaded(
                ref packageGuid,
                out package); // 尝试获取已加载的 Package

            if (hr != Microsoft.VisualStudio.VSConstants.S_OK || package == null)
            {
                // 未加载或加载失败：强制加载 (LoadPackage)
                hr = shell.LoadPackage(ref packageGuid, out package);

                if (hr != Microsoft.VisualStudio.VSConstants.S_OK || package == null)
                {
                    // 报告加载错误
                    Marshal.ThrowExceptionForHR(hr);
                    return;
                }
            }

            // 3. 将 IVsPackage 转换为您的具体 Package 类型
            AICodingPackage myPackage = package as AICodingPackage;
            if (myPackage == null)
            {
                System.Diagnostics.Debug.Fail("Package 转换失败.");
                return;
            }

            // 4. 获取 DialogPage
            // 注意：GetDialogPage 仍需要在 UI 线程上调用
            OptionPage optionPage = (OptionPage)myPackage.GetDialogPage(typeof(OptionPage));
            this.optionPage = optionPage;
        }
        private void MyToolWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _ = Window_LoadedAsync(sender, e);

            LoadOptionPage();
        }
        private async System.Threading.Tasks.Task StreamingOutputAsync(StreamReader reader)
        {
            try
            {
                bool firstIn = true;
                string alltext = "";
                while (!reader.EndOfStream)
                {
                    // 异步读取一行数据
                    var line = await reader.ReadLineAsync();

                    if (line == null)
                        continue;
                    const string DataPrefix = "data:";
                    if (!line.StartsWith(DataPrefix))
                        continue;
                    // 提取 data: 后面的所有内容，并去除前后空白
                    string jsonPayload = line.Substring(DataPrefix.Length).Trim();

                    // 2. 使用 Newtonsoft.Json 进行反序列化
                    DifySseData data = JsonConvert.DeserializeObject<DifySseData>(jsonPayload);

                    if (data == null)
                        continue;
                    if (data.EventName != "message")
                        continue;
                    // 3. 成功解析，可以使用对象属性

                    // 流式输出中

                    alltext += data.Answer;
                    if (firstIn)
                    {
                        string tempText = alltext;
                        tempText = tempText
                            .Replace("\\", "\\\\") // 先转义反斜杠
                            .Replace("'", "\\'")   // 转义单引号
                            .Replace("\"", "\\\"") // 转义双引号
                            .Replace("\r\n", "\\n")// 处理换行符
                            .Replace("\n", "\\n"); // 处理换行符
                        string type = "bot";
                        string jsCode = $"addMessage('{tempText}', '{type}');";
                        try
                        {

                            string resultJson = await mdview.CoreWebView2.ExecuteScriptAsync(jsCode);
                        }
                        catch (System.Exception ex)
                        {
                            MessageBox.Show($"执行 JS 脚本出错: {ex.Message}", "C# 调用错误");
                        }
                        firstIn = false;
                    }

                    string tempTextAdd = alltext;
                    tempTextAdd = tempTextAdd
                        .Replace("\\", "\\\\") // 先转义反斜杠
                        .Replace("'", "\\'")   // 转义单引号
                        .Replace("\"", "\\\"") // 转义双引号
                        .Replace("\r\n", "\\n")// 处理换行符
                        .Replace("\n", "\\n"); // 处理换行符
                    string setjsCode = $"setMessage('{tempTextAdd}');";
                    try
                    {
                        string resultJson = await mdview.CoreWebView2.ExecuteScriptAsync(setjsCode);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"执行 JS 脚本出错: {ex.Message}", "C# 调用错误");
                    }
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON解析失败: {ex.Message}");
            }
        }
        private async System.Threading.Tasks.Task CoreWebView2_WebMessageReceivedAsync(CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                // async 逻辑写这里
                string message = e.TryGetWebMessageAsString();
                string ipAddress = "http://" + optionPage.IpAddress + "/v1/chat-messages";
                var options = new RestClientOptions(ipAddress.Trim())
                {
                    Timeout = TimeSpan.FromMinutes(5), // SSE长连接超时设置
                    ThrowOnAnyError = true
                };

                var client = new RestClient(options);
                var request = new RestRequest("", Method.Post)
                {
                    CompletionOption = HttpCompletionOption.ResponseHeadersRead // 关键：先读取响应头再处理流
                };
                string tokens = "Bearer " + optionPage.AccessToken.Trim();
                request.AddHeader("Authorization", tokens);
                request.AddHeader("Content-Type", "application/json");
                request.AddHeader("Accept", "text/event-stream");
                request.AddHeader("Accept-Encoding", "gzip, deflate, br");
                request.AddHeader("User-Agent", "PostmanRuntime-ApipostRuntime/1.1.0");
                request.AddHeader("Connection", "keep-alive");
                request.AddParameter("application/json", "{\"inputs\":{},\"query\":\"" + message + "\",\"response_mode\":\"streaming\",\"conversation_id\":\"\",\"user\":\"abc-123\"}", ParameterType.RequestBody);
                var stream = await client.DownloadStreamAsync(request);
                var reader = new StreamReader(stream, Encoding.UTF8);
                // 循环读取，直到流结束 (即服务器断开连接)
                await StreamingOutputAsync(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"流式请求失败: {ex}");
            }
        }
        private void DteCommand_FileCompare(string file1Path, string file2Path)
        {
            DTE2 dte = (DTE2)Package.GetGlobalService(typeof(DTE));
            if (dte == null || dte.ActiveDocument == null)
            {
                System.Diagnostics.Debug.WriteLine("未找到当前打开的文件");
                return;
            }
            const string gitCommand = "Tools.DiffFiles";
            string arguments = $"\"{file1Path}\" \"{file2Path}\"";
            try
            {
                // 3. 执行命令
                // 参数通常是可选的，这里不需要参数
                dte.ExecuteCommand(gitCommand,arguments);
            }
            catch (Exception ex)
            {
                // 捕获命令执行失败
                Console.WriteLine($"文件对比失败: {ex}");
            }
        }
        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _ = CoreWebView2_WebMessageReceivedAsync(e);
            
            //调用读取环境
            ReadWorkspace();
            //调用读取当前文件内容
            ReadCurrentFileContent();
            //文件对比
            //DteCommand_FileCompare("C:\\Users\\Method-Jiao\\Desktop\\1.cpp", "C:\\Users\\Method-Jiao\\Desktop\\2.cpp");
        }
    }
}