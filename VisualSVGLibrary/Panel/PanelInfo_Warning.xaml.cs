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
using System.Collections.ObjectModel;
using VisualSVGLibrary.ViewModels;
using ToggleSwitch;

namespace VisualSVGLibrary.Panel
{
    /// <summary>
    /// PanelInfo_Warning.xaml 的交互逻辑
    /// </summary>
    public partial class PanelInfo_Warning : UserControl
    {
        public PanelInfo_Warning()
        {
            InitializeComponent();
            this.DataContext = new WarningModel();
        }

        System.Windows.Threading.DispatcherTimer datatimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
        Random rd = new Random();
        ObservableCollection<RunDataWarning> lstWarnType;
        //IEnumerable<WarningInfo> lstWarnInfo = new IEnumerable<WarningInfo>();



        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            lstWarnType = new ObservableCollection<RunDataWarning>();

            lstWarnType.Add(new RunDataWarning() { IsChecked = false, Name = "检修", objStatus=pSymbolObject.ECStatus.检修, count = 0, color = Colors.Blue });
            lstWarnType.Add(new RunDataWarning() { IsChecked = false, Name = "故障", objStatus = pSymbolObject.ECStatus.故障, count = 0, color = Colors.Red });
            lstWarnType.Add(new RunDataWarning() { IsChecked = false, Name = "停电", objStatus = pSymbolObject.ECStatus.停电, count = 0, color = Colors.Green });
            lstWarnType.Add(new RunDataWarning() { IsChecked = false, Name = "过载", objStatus = pSymbolObject.ECStatus.过载, count = 0, color = Colors.Sienna });
            lstWarnType.Add(new RunDataWarning() { IsChecked = false, Name = "轻载", objStatus = pSymbolObject.ECStatus.轻载, count = 0, color = Colors.Thistle });
            lstWarnType.Add(new RunDataWarning() { IsChecked = false, Name = "电压等高线", objStatus=pSymbolObject.ECStatus.自定5, count=9999 });

            lstWarning.ItemsSource = lstWarnType;
            pieChart.DataSource = from wType in lstWarnType
                                  where wType.count!=9999
                                  select wType;
            //lstList.ItemsSource = lstWarnInfo;

            datatimer.Tick += new EventHandler(datatimer_Tick);
            datatimer.Start();
        }

        private void Grid_Unloaded(object sender, RoutedEventArgs e)
        {
            datatimer.Stop();
            foreach(var wtype in lstWarnType)
            {
                wtype.IsChecked = false;
            }
            Update();
        }

        private void HorizontalToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            //pSymbolObject.ECStatus ecStatus = (pSymbolObject.ECStatus)(sender as HorizontalToggleSwitch).Tag;
            //if (ecStatus == pSymbolObject.ECStatus.自定5)//(szName == "电压等高线")
            //    RefreshData.showVLContour(true);
            //else
                Update();
        }

        private void HorizontalToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {
            //string szName = (sender as HorizontalToggleSwitch).Name;
            //pSymbolObject.ECStatus ecStatus = (pSymbolObject.ECStatus)(sender as HorizontalToggleSwitch).Tag;
            //if (ecStatus == pSymbolObject.ECStatus.自定5) //(szName == "电压等高线")
            //    RefreshData.showVLContour(false);
            //else
                Update();
        }

        
        void datatimer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        
        void Update()
        {
            ShowObjStatus();
            pieChart.UpdateData();


            foreach (RunDataWarning wType in lstWarnType)
            {
                if (wType.Name == "电压等高线" )
                {
                    if (wType.IsChecked) RefreshData.showVLContour(true);
                    else RefreshData.showVLContour(false);

                    break;
                }
            }
            HySVG.uc.UpdateModel();
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    //pLayer layer;
        //    //if (HySVG.uc.objManager.zLayers.TryGetValue("Disconnector", out layer))
        //    //{
        //    //    foreach (PowerBasicObject obj in layer.pModels.Values)
        //    //    {
        //    //        if (obj is pSymbolObject && obj.submodels.Count>0)
        //    //        {
        //    //            (obj.submodels.Values.ElementAt(0) as pData).datas.ElementAt(0).argu = "wd";
        //    //            (obj.submodels.Values.ElementAt(0) as pData).datas.ElementAt(0).color = Colors.White;
        //    //            (obj.submodels.Values.ElementAt(0) as pData).datas.ElementAt(0).value = 30;
        //    //        }
        //    //    }
        //    //}
        //    //HySVG.uc.UpdateModel();
        //}

        public void ShowObjStatus()
        {
            pLayer layer;
            if (HySVG.uc.objManager.zLayers.TryGetValue("yx", out layer))
            {
                IEnumerable<WarningInfo> lstWarnInfo = from obj in layer.pModels.Values
                                                       orderby (obj as pSymbolObject).objStatus
                                                from warn in lstWarnType
                                                where warn.IsChecked == true && warn.objStatus == (obj as pSymbolObject).objStatus
                                                select new WarningInfo { obj = (obj as pSymbolObject), Name = obj.id, brush = warn.brush, color=warn.color,tip = warn.Name };
                lstList.ItemsSource = lstWarnInfo;


                foreach (WarningInfo info in lstWarnInfo)
                {
                    info.obj.isShowSubObject = true;
                    if (info.obj.submodels.Count == 0)
                    {
                        pData pd = new pData(info.obj.parent)
                        {
                            id = info.obj.id + info.tip,
                            radScale = 0.02, //底部尺寸
                            valueScale = 0.002, //值转高度的系数，高度=值*valueScale
                            location = info.obj.location,
                            isShowLabel = true,
                        };
                        pd.datas.Add(new Data() { id = pd.id + "Data", argu = info.tip, value = 30, color = info.color, geokey = "圆锥体", format = "{1}" });
                        info.obj.AddSubObject("数据对象1", pd);
                    }
                    else
                    {
                        foreach (PowerBasicObject obj in info.obj.submodels.Values)
                        {
                            if (obj is pData)
                            {
                                foreach (Data dt in (obj as pData).datas)
                                {
                                    dt.argu = info.tip;
                                    dt.color = info.color;
                                    dt.value = 30 + rd.NextDouble();
                                }
                            }
                        }
                    }
                }

                foreach (RunDataWarning rWarn in lstWarnType)
                {
                    rWarn.objs = from obj in layer.pModels.Values where (obj as pSymbolObject).objStatus == rWarn.objStatus select obj;
                    rWarn.count = rWarn.objs.Count();

                    if (rWarn.IsChecked == false)
                    {
                        foreach (pSymbolObject obj in rWarn.objs)
                        {
                            obj.isShowSubObject = false;
                        }
                    }
                }

                IEnumerable<PowerBasicObject> objnormals = from obj in layer.pModels.Values where (obj as pSymbolObject).objStatus == pSymbolObject.ECStatus._正常 select obj;
                foreach (pSymbolObject obj in objnormals)
                {
                    obj.isShowSubObject = false;
                }
            }

            
        }

        private void lstList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBox).SelectedItem != null)
            {
                PowerBasicObject selobj = ((sender as ListBox).SelectedItem as WarningInfo).obj;
                HySVG.uc.camera.aniLook(selobj.VecLocation, 5);

                (selobj as pSymbolObject).aniTwinkle.doCount = 20;
                (selobj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
            }
        }

        
    }

    public class RunDataWarning
    {
        public RunDataWarning() { }

        public bool IsChecked { get; set; }
        public string Name { get; set; }
        public int count { get; set; }
        public Color color { get; set; }
        public Brush brush { get { return new SolidColorBrush(color); } }
        public pSymbolObject.ECStatus objStatus { get; set; }

        public IEnumerable<PowerBasicObject> objs { get; set; }
    }

    public class WarningInfo
    {
        public string Name { get; set; }
        public Color color { get; set; }
        public Brush brush { get; set; }
        public string tip { get; set; }
        public pSymbolObject obj { get; set; }
    }
}
