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
using MyClassLibrary.DevShare;

namespace VisualSVGLibrary.Panel
{
    /// <summary>
    /// Panel.xaml 的交互逻辑
    /// </summary>
    public partial class Panel : UserControl
    {
        public Panel()
        {
            InitializeComponent();
        }

        System.Windows.Threading.DispatcherTimer datatimer = new System.Windows.Threading.DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };
        Random rd = new Random();
        PanelRefreshData reData = new PanelRefreshData();
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            grdMain.DataContext = reData;
            reData.start();

            (panel.Children[0] as RunInfoPanel).status = RunInfoPanel.EStatus.最大化;

            datatimer.Tick += new EventHandler(datatimer_Tick);
            datatimer.Start();
        }

        private void grdMain_Unloaded(object sender, RoutedEventArgs e)
        {
            datatimer.Stop();
        }

        void datatimer_Tick(object sender, EventArgs e)
        {
            Update();
        }

        void Update()
        {
            reData.readData();
            ShowObjStatus();
            showVLContour(reData.dgxIsChecked);
            HySVG.uc.UpdateModel();
        }

        private void RunInfoPanel_OnClickHeader(object sender, EventArgs e)
        {
            RunInfoPanel pan = sender as RunInfoPanel;
            pan.status = RunInfoPanel.EStatus.最大化;
        }

        
        private void HorizontalToggleSwitch_Checked(object sender, RoutedEventArgs e)
        {
            CheckCameraLookDown();
            ShowObjStatus();
        }

        private void HorizontalToggleSwitch_Unchecked(object sender, RoutedEventArgs e)
        {

            CheckCameraLookDown();
            ShowObjStatus();
        }

        void CheckCameraLookDown()
        {
            bool bLookDown = true;
            foreach (ChartDataPoint dp in reData.eventCount)
            {
                if (dp.isInclude == true)
                {
                    bLookDown = false;
                    break;
                }
            }
            if (!bLookDown)
                HySVG.uc.camera.adjustCameraAngle(45);
            else
                HySVG.uc.camera.adjustCameraAngle(0);
        }

        void ShowObjStatus()
        {
            IEnumerable < EventData > lstShowChoose= from e0 in reData.lstEvent
                  from e1 in reData.eventCount
                  where e0.eType.ToString()==e1.argu && e1.isInclude==true
                  select e0;

            foreach (EventData item in lstShowChoose)
            {
                item.obj.isShowSubObject = true;
                if (item.obj.submodels.Count == 0)
                {
                    pData pd = new pData(item.obj.parent)
                    {
                        id = item.obj.id + "pd",
                        radScale = 0.02, //底部尺寸
                        valueScale = 0.002, //值转高度的系数，高度=值*valueScale
                        location = item.obj.location,
                        isShowLabel = true,
                    };
                    pd.datas.Add(new Data() { id = pd.id + "Data", argu = item.eType.ToString(), value = 30, color = item.color, geokey = "圆锥体", format = "{1}" });
                    item.obj.AddSubObject("数据对象1", pd);
                }
                else
                {
                    foreach (PowerBasicObject obj in item.obj.submodels.Values)
                    {
                        if (obj is pData)
                        {
                            foreach (Data dt in (obj as pData).datas)
                            {
                                dt.argu = item.eType.ToString();
                                dt.color = item.color;
                                dt.value = 30 + rd.NextDouble();
                            }
                        }
                    }
                }
            }

            IEnumerable<PowerBasicObject> lstnormals = reData.getAllObjListByLayerName("yx").Where(p => !reData.lstEvent.Any(g => p.id == g.eObjID));
            foreach (PowerBasicObject obj in lstnormals)
            {
                obj.isShowSubObject = false;
            }

            IEnumerable<EventData> lstED= reData.lstEvent.Where(a=>reData.eventCount.Any(b=>b.isInclude==false&&a.eType.ToString()==b.argu));
            foreach(EventData ed in lstED)
            {
                ed.obj.isShowSubObject = false;
            }

            HySVG.uc.UpdateModel();
        }

        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as ListBox).SelectedItem != null)
            {
                PowerBasicObject selobj = ((sender as ListBox).SelectedItem as EventData).obj;
                HySVG.uc.camera.aniLook(selobj.VecLocation, 5);

                (selobj as pSymbolObject).aniTwinkle.doCount = 20;
                (selobj as pSymbolObject).AnimationBegin(pSymbolObject.EAnimationType.闪烁);
            }
        }

        private void HorizontalToggleSwitch_Checked_dgx(object sender, RoutedEventArgs e)
        {
            showVLContour(true);
        }

        private void HorizontalToggleSwitch_Unchecked_dgx(object sender, RoutedEventArgs e)
        {
            showVLContour(false);
        }


         pLayer contourLayer;
         ContourGraph.Contour con;
         List<ContourGraph.ValueDot> dots;
         pContour gcon;
        public void showVLContour(bool isShow)
        {
            if (isShow)
            {
                if (!HySVG.uc.legendManager.legends.ContainsKey("电压等值图例"))
                {
                    GradientBrushLegend legend = HySVG.uc.legendManager.createGradientBrushLegend("电压等值图例");
                    legend.header = " 电压标幺值 ";
                    legend.isShow = true;
                    legend.panel.VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
                    legend.panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                    legend.panel.Margin = new Thickness(120, 0, 0, 30);
                    legend.panel.Background = new SolidColorBrush(Color.FromArgb(0xCC, 0x00, 0x00, 0x00));
                    legend.headerBackground = Brushes.Black;
                    legend.headerForeground = Brushes.Aqua;
                    legend.headerBorderBrush = new SolidColorBrush(Colors.White);
                    legend.setText("0.9", "1", "1.1");

                    GradientStopCollection gsc = new GradientStopCollection();
                    gsc.Add(new GradientStop(Colors.Blue, 0));
                    gsc.Add(new GradientStop(Color.FromArgb(0x34, 0x00, 0xFF, 0xFF), 0.25));
                    gsc.Add(new GradientStop(Color.FromArgb(0x00, 0x00, 0xFF, 0x00), 0.5));
                    gsc.Add(new GradientStop(Color.FromArgb(0x34, 0xFF, 0xFF, 0x00), 0.75));
                    gsc.Add(new GradientStop(Colors.Red, 1));
                    legend.setGradientStop(gsc);
                }
                HySVG.uc.legendManager.legends["电压等值图例"].isShow = true;

                if (!HySVG.uc.objManager.zLayers.TryGetValue("等高图层", out contourLayer))
                {
                    contourLayer = HySVG.uc.objManager.AddLayer("等高图层", "等高图层", "等高图层");
                    //contourLayer.deepOrder = -1;

                    dots = new List<ContourGraph.ValueDot>();

                    pLayer layer;
                    if (HySVG.uc.objManager.zLayers.TryGetValue("yx", out layer))
                    {
                        foreach (pSymbolObject obj in layer.pModels.Values)
                        {
                            dots.Add(new ContourGraph.ValueDot() { id = obj.id, location = obj.center, value = rd.Next(90, 110) / (double)100.0 });
                        }

                        double minx, miny, maxx, maxy;
                        //根据scene.coordinateManager.isXAsLong确定原始坐标的横纵方向
                        miny = dots.Min(p => HySVG.uc.coordinateManager.isXAsLong ? p.location.Y : p.location.X);
                        maxy = dots.Max(p => HySVG.uc.coordinateManager.isXAsLong ? p.location.Y : p.location.X);
                        minx = dots.Min(p => HySVG.uc.coordinateManager.isXAsLong ? p.location.X : p.location.Y);
                        maxx = dots.Max(p => HySVG.uc.coordinateManager.isXAsLong ? p.location.X : p.location.Y);

                        double w = maxx - minx; double h = maxy - miny;
                        minx = minx - w * 0.2; maxx = maxx + w * 0.2;
                        miny = miny - h * 0.2; maxy = maxy + h * 0.2;
                        w = maxx - minx; h = maxy - miny;
                        //经纬换为屏幕坐标
                        int size = 1024;
                        foreach (ContourGraph.ValueDot dot in dots)
                        {
                            if (HySVG.uc.coordinateManager.isXAsLong)
                                dot.location = new Point((dot.location.X - minx) / w * size, (maxy - dot.location.Y) / h * size);
                            else
                                dot.location = new Point((dot.location.Y - minx) / w * size, (maxy - dot.location.X) / h * size);  //重新赋与新的平面点位置, 注，纬度取反，仅适用北半球
                        }


                        //设置计算参数
                        if (con == null) con = new ContourGraph.Contour();
                        con.dots = dots;
                        con.opacityType = ContourGraph.Contour.EOpacityType.倒梯形;
                        con.canvSize = new Size(size, size);
                        con.gridXCount = 200;
                        con.gridYCount = 200;
                        con.Span = 30;
                        con.maxvalue = 1.1;
                        con.minvalue = 0.9;
                        con.dataFillValue = 1;
                        con.dataFillMode = ContourGraph.Contour.EFillMode.单点包络填充;
                        con.isDrawGrid = false;
                        con.isDrawLine = false;
                        con.isFillLine = true;
                        if (gcon == null) gcon = new pContour(contourLayer) { id = "等值图" };// { minJD = minx, maxJD = maxx, minWD = miny, maxWD = maxy };
                        gcon.setRange(minx, maxx, miny, maxy);
                        gcon.brush = con.ContourBrush;
                        contourLayer.AddObject("等值线", gcon);
                        con.GenCompleted += new EventHandler(con_GenCompleted);
                        con.GenContourAsync(); //异步开始生成

                    }
                }
                else//刷新数据
                {
                    foreach (var item in dots)
                    {
                        if (item.id != null)
                        {
                            item.value = rd.Next(90, 110) / (double)100.0;
                        }
                    }
                    con.ReGenContourAsync();
                }
                if (HySVG.uc.objManager.zLayers.TryGetValue("等高图层", out contourLayer))
                    contourLayer.logicVisibility = true;

            }
            else
            {
                if (HySVG.uc.objManager.zLayers.TryGetValue("等高图层", out contourLayer))
                    contourLayer.logicVisibility = false;
                if (HySVG.uc.legendManager.legends.ContainsKey("电压等值图例"))
                    HySVG.uc.legendManager.legends["电压等值图例"].isShow = false;

                HySVG.uc.UpdateModel();
            }

        }

        void con_GenCompleted(object sender, EventArgs e) //异步完成
        {
            gcon.brush = con.ContourBrush;
            HySVG.uc.UpdateModel();

        }
    }
}
