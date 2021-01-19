using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AMCClient2
{
    /// <summary>
    /// Interaction logic for ClientView.xaml
    /// </summary>
    public partial class ClientView : BasePage<ClientInterfaceViewModel>
    {
        public ClientView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fires when the page has loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BasePage_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus on the console input box when the page is loaded
            ConsoleInput.Focus();
        }
    }
}
