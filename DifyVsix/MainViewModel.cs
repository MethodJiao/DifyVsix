using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DifyVsix
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private string _markdownContent = "";

        // 绑定到 MarkdownScrollViewer.Markdown 属性
        public string MarkdownContent
        {
            get => _markdownContent;
            set
            {
                if (_markdownContent != value)
                {
                    _markdownContent = value;
                    OnPropertyChanged(); // 核心：通知 UI 属性已更改
                }
            }
        }

        public void AddNewEntry(string message)
        {
            // 更新属性，这将自动触发 MarkdownScrollViewer 重新渲染
            this.MarkdownContent += message;

        }

        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
