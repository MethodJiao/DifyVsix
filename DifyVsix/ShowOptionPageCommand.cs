using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace DifyVsix
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ShowOptionPageCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("e77de72c-f79b-4ecb-9ca4-5da4c3ba6d51");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowOptionPageCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ShowOptionPageCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ShowOptionPageCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in Command's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ShowOptionPageCommand(package, commandService);
        }
        private AICodingPackage GetMyPackageInstance()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // 使用 IVsShell 强制加载 Package
            IVsShell shell = (IVsShell)Microsoft.VisualStudio.Shell.ServiceProvider.GlobalProvider.GetService(typeof(SVsShell));
            if (shell == null) return null;

            Guid packageGuid = new Guid(AICodingPackage.PackageGuidString);
            IVsPackage package;

            // 尝试加载或获取已加载的 Package
            int hr = shell.LoadPackage(ref packageGuid, out package);

            if (hr == Microsoft.VisualStudio.VSConstants.S_OK && package is AICodingPackage myPackage)
            {
                return myPackage;
            }

            // 如果加载失败，或者转换失败
            return null;
        }
        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // 2. 获取 Package 实例 (使用 IVsShell 强制加载是最佳实践)
            AICodingPackage myPackage = GetMyPackageInstance();

            if (myPackage != null)
            {
                // 3. 调用 ShowOptionPage 来打开配置页
                // 参数是您配置类的 Type
                myPackage.ShowOptionPage(typeof(OptionPage));
            }
        }
    }
}
