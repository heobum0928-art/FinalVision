using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Project.UI.Common
{
    public class ListViewEx : ListView
    {
        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ListViewExItem();
        }
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ListViewExItem;
        }
    }
    public class ListViewExItem : ListViewItem
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
        }
    }
}
