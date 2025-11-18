using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MdXaml;

namespace DifyVsix
{
    public static class ScrollHelper
    {
        // 通用方法：从 VisualTree 中查找指定类型的子控件
        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child != null && child is T foundChild)
                {
                    return foundChild;
                }
                else
                {
                    T grandChild = FindVisualChild<T>(child);
                    if (grandChild != null)
                    {
                        return grandChild;
                    }
                }
            }
            return null;
        }

        // 针对 MarkdownScrollViewer 的扩展方法
        public static void ScrollMarkdownToEnd(this MarkdownScrollViewer mdViewer)
        {
            // 查找内部的 ScrollViewer
            ScrollViewer scrollViewer = FindVisualChild<ScrollViewer>(mdViewer);

            if (scrollViewer != null)
            {
                // 在内部 ScrollViewer 上调用 ScrollToEnd()
                scrollViewer.ScrollToEnd();
            }
        }
    }
}
