# DifyVsix — Visual Studio 扩展

一个用于 Visual Studio 的扩展（VSIX），提供与 Dify 工作流相关的工具窗口和命令，方便在 IDE 中集成并使用 Dify 功能。

**主要用途**：在 Visual Studio 中快速访问 Dify 相关操作（工具窗口、命令等），并在扩展中承载静态 HTML 资产用于界面展示。

**状态**：仓库包含可在 Visual Studio 中打开并调试的 VSIX 项目（适用于 Visual Studio 2019/2022）。

**目录快速导航**
- `DifyVsix/`: 扩展的源代码和资源（主要工程文件）。
- `DifyVsix/HtmlAssets/`: 扩展使用的静态前端资源（如 `index.html`）。
- `bin/` 和 `obj/`: 构建输出与中间文件。

**特性（示例）**
- 在 Visual Studio 中提供自定义工具窗口（`ToolWindow`）。
- 注册扩展命令供菜单/工具栏调用（`ToolWindowCommand`）。
- 内嵌静态 HTML 页面作为扩展 UI（`HtmlAssets/index.html`）。

**先决条件**
- Windows（开发机）
- Visual Studio 2019 或 2022（带 VSIX 开发工作负载）
- .NET Framework（由项目文件指定）

**安装与构建**

推荐方式：使用 Visual Studio

- 在 Visual Studio 中打开解决方案 `DifyVsix.sln`。
- 选择目标配置（`Debug` 或 `Release`），然后选择 `生成 -> 生成解决方案`。
- 生成成功后，VSIX 包通常出现在项目的 `bin\{Configuration}` 文件夹下。

使用命令行（PowerShell 示例）

```powershell
# 在仓库根目录运行（确保已安装 MSBuild/Visual Studio 命令行工具）
msbuild .\DifyVsix.sln /p:Configuration=Release

# 在构建输出目录查找生成的 VSIX
Get-ChildItem -Path .\DifyVsix\bin\Release -Filter *.vsix -Recurse
```

**运行与调试**

- 在 Visual Studio 中将 VSIX 项目设为启动项目，按 `F5` 启动扩展调试，这会打开一个 Visual Studio Experimental Instance（实验实例），你可以在其中测试扩展的工具窗口与命令。
- 若要在本地安装生成的 `.vsix` 包，双击该文件或使用 `VSIXInstaller.exe` 安装。

**项目结构说明（重要文件）**
- `DifyVsixPackage.cs` — 扩展包的入口与注册逻辑。
- `ToolWindowControl.xaml(.cs)` — 工具窗口的界面与逻辑实现。
- `ToolWindowCommand.cs` — 注册并处理扩展命令。
- `HtmlAssets/index.html` — 内嵌的前端页面资源。

**常见任务**
- 添加/修改工具窗口：编辑 `ToolWindowControl.xaml` 并更新对应的代码文件。
- 添加命令：在 `.vsct` 文件和命令实现类中注册并实现行为。

**贡献**

欢迎提交 Issue 或 PR：
- Fork 本仓库并创建你的分支：`git checkout -b feature/your-feature`
- 提交并推送：`git commit -am "Add feature" && git push origin feature/your-feature`
- 在 GitHub 上发起 Pull Request，描述变更目的与测试步骤。

如果你希望我在 README 中加入截图、徽章（CI、NuGet）或发布流程（自动打包/上传 VSIX），请回复我需要的内容／凭证。

**许可**

本文档未指定许可；如果仓库使用特定开源许可，请在此处注明（例如 MIT、Apache-2.0 等）。

