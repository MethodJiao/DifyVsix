using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
namespace DifyVsix
{
    [Guid("e77de72c-f79b-4ecb-9ca4-5da4c3ba9d51")]
    public class OptionPage : DialogPage
    {
        // --- IP 地址配置 ---
        [Category("服务器连接")]
        [DisplayName("服务器 地址")]
        [Description("请输入服务器的 IP 地址 (例如: 192.168.1.1)。")]
        public string IpAddress { get; set; } = "192.168.190.144";

        // --- Token 配置 ---
        [Category("认证设置")]
        [DisplayName("秘钥")]
        [Description("请输入用于 API 认证的访问令牌。")]
        // 推荐使用 PasswordPropertyText 或 EditorAttribute 来隐藏 Token
        //[PasswordPropertyText(true)]
        public string AccessToken { get; set; } = "app-8XPZct7dPmHbfhE1VuI30KG2";
    }
}