using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Project.UI.Common
{
    public class MyScrollViewer : ScrollViewer
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            //if (base.Focus())
            //    e.Handled = false;

            base.OnMouseLeftButtonDown(e);

            e.Handled = false;
        }
    }
}
