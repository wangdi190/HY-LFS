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
using System.Collections.ObjectModel;

namespace VisualSVGLibrary.Panel
{
    /// <summary>
    /// PanelInfo_System.xaml 的交互逻辑
    /// </summary>
    public partial class PanelInfo_System : UserControl
    {
        public PanelInfo_System()
        {
            InitializeComponent();
        }

        System.Windows.Threading.DispatcherTimer datatimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
        Random rd = new Random();
        ObservableCollection<RunDataSystem> lstRunDataSystem;
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            lstRunDataSystem = new ObservableCollection<RunDataSystem>();
            lstRunDataSystem.Add(new RunDataSystem() { Name = "系统频率", Value = 50.01 });
            lstRunDataSystem.Add(new RunDataSystem() { Name = "当前负荷", Value = 50.02 });
            lstRunDataSystem.Add(new RunDataSystem() { Name = "用 电 量", Value = 50.03 });

            lstRunData.ItemsSource = lstRunDataSystem;

            datatimer.Tick += new EventHandler(datatimer_Tick);
            datatimer.Start();
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            datatimer.Stop();
        }

        void datatimer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        void Update()
        {
            foreach (RunDataSystem item in lstRunDataSystem)
            {
                item.Value = rd.Next(60, 240);
                item.refresh();
            }
        }
        
    }

    public class RunDataSystem : MyClassLibrary.MVVM.NotificationObject
    {
        public string Name { get; set; }
        public double Value { get; set; }

        public void refresh()
        {
            RaisePropertyChanged(() => Value);
        }
    }
}
