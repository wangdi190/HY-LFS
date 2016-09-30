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
using System.IO;
using VisualSVGLibrary.Panel;

namespace VisualSVGLibrary
{
    /// <summary>
    /// HySVG.xaml 的交互逻辑
    /// </summary>
    public partial class HySVG : UserControl
    {
        public static Earth uc;

        public static string FileSvgDir = ".\\svg\\";

        public HySVG()
        {
            InitializeComponent();
            Init();
        }

        public void Wd()
        {
            uc.UpdateModel();
        }

        private PhraseSVG phSvg;
        private PanelInfo panelInfo;
        void Init()
        {
            uc = new Earth();
            uc.config.tooltipClickEnable = true;
            uc.config.tooltipMoveEnable = true;
            uc.config.pickEnable = true;
            uc.config.enableMinimap = false;
            uc.earthManager.mapType = EMapType.无;  //设置为无地图模式
            grdSvg.Children.Add(uc);

            phSvg = new PhraseSVG();

            panelInfo = new PanelInfo();
            grdInfo.Children.Add(panelInfo);

            genGeomeries();
            genSymbol();
        }

        public void UpdateVisual(string FileSvgName)
        {
            phSvg.VisualFileSvg(FileSvgDir + FileSvgName);
        }

        void genSymbol()
        {
            DirectoryInfo SymbolFolder = new DirectoryInfo(FileSvgDir+"symbols\\");
            foreach (FileInfo file in SymbolFolder.GetFiles())
            {

                phSvg.PhraseSymbol(file.FullName, file.Name);
                //if (file.Name.Length > 8)
                //{
                //    string str = file.Name.Substring(file.Name.Length - 8);
                //    if (str == ".icn.svg")
                //        phSvg.PhraseSymbol(file.FullName, file.Name);
                //}
            }

            //phSvg.InitGzSymbol();
        }

        void genGeomeries()
        {
            uc.objManager.AddBoxResource("立方体", 1, 1, 1);
            uc.objManager.AddCylinderResource("圆柱体", 1, 1, 1, 16, 1);
            uc.objManager.AddCylinderResource("圆锥体", 0, 1, 1, 16, 1);
            uc.objManager.AddSphereResource("球体", 1, 16, 16);
        }
    }
}
