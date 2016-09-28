using System;
using System.Collections.Generic;
using System.Linq;
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
using WpfEarthLibrary;

namespace VisualSVGLibrary.Panel
{
    /// <summary>
    /// PanelInfo.xaml 的交互逻辑
    /// </summary>
    public partial class PanelInfo : UserControl
    {
        //Earth uc;
        public PanelInfo()
        {
            InitializeComponent();
            //uc = earth;
            comboBox.SelectedIndex = 0;
        }

        string SelectedTag = "";
        PanelInfo_Warning panelWarning = new PanelInfo_Warning();
        PanelInfo_System panelSystem = new PanelInfo_System();

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedTag = (comboBox.SelectedItem as ComboBoxItem).Tag.ToString();
            grdPanel.Children.Clear();
            switch (SelectedTag)
            {
                case "warning":
                    grdPanel.Children.Add(panelWarning);
                    break;
                case "system":
                    grdPanel.Children.Add(panelSystem);
                    break;
            }
        }
    }
}
