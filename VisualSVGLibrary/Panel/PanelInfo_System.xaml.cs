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
using MyClassLibrary.DevShare;
using System.ComponentModel;

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
        SystemPanelData panelData = new SystemPanelData();
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            grdMain.DataContext = panelData;
            panelData.start();


            lstRunDataSystem = new ObservableCollection<RunDataSystem>();
            lstRunDataSystem.Add(new RunDataSystem() { Name = "系统频率", Value = 50.01 });
            lstRunDataSystem.Add(new RunDataSystem() { Name = "当前负荷", Value = 50.02 });
            lstRunDataSystem.Add(new RunDataSystem() { Name = "用 电 量", Value = 50.03 });

            lstRunData.ItemsSource = lstRunDataSystem;

            datatimer.Tick += new EventHandler(datatimer_Tick);
            datatimer.Start();

            Update();
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
                item.Value = rd.Next(6000, 240000);
                item.refresh();
            }
        }
    }

    class SystemPanelData : MyClassLibrary.MVVM.NotificationObject
    {
        public SystemPanelData()
        {
            realLoads = new BindingList<ChartDataPoint>();
            planLoads = new BindingList<ChartDataPoint>();
            realEle = new BindingList<ChartDataPoint>();
        }

        public BindingList<ChartDataPoint> realLoads { get; set; }//负荷曲线
        public BindingList<ChartDataPoint> planLoads { get; set; }//负荷曲线
        public BindingList<ChartDataPoint> realEle { get; set; }//用电量

        Random rd = new Random();
        public void start()
        {
            //负荷曲线
            DateTime d = DateTime.Now;
            d = new DateTime(d.Year, d.Month, d.Day);
            for (int i = 0; i < 96; i++)
            {
                ChartDataPoint cp = new ChartDataPoint() { argudate = d.AddMinutes(15 * i), value = MyClassLibrary.MyFunction.simHourData((int)(i / 4)) * 200 + rd.Next(30) };
                if (i < 66)
                {
                    realLoads.Add(cp);
                    realEle.Add(cp);
                }

                planLoads.Add(new ChartDataPoint() { argudate = d.AddMinutes(15 * i), value = cp.value + rd.Next(20) });
            }

            RaisePropertyChanged(() => realLoads);
            RaisePropertyChanged(() => planLoads);
            RaisePropertyChanged(() => realEle);
        }

        public void readData()
        {
            
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
