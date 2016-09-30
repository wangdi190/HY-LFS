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
using VisualSVGLibrary;

namespace VisualSVGTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        HySVG hySvg;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            HySVG.FileSvgDir = ".\\svg\\";

            hySvg = new HySVG();
            grdMain.Children.Add(hySvg);

            hySvg.UpdateVisual("万益能源主站 接线图-测试.svg");
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if ((sender as Button).Content.ToString() == "北京")
        //    {
        //        hySvg.UpdateVisual("主接线图.fac.pic1.svg");
        //        (sender as Button).Content = "四川";
        //    }
        //    else
        //    {
        //        hySvg.UpdateVisual("主接线图.fac.pic.svg");
        //        (sender as Button).Content = "北京";
        //    }
            
        //    //hySvg.Stop();
        //}

        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    hySvg.Wd();
        //}
    }
}
