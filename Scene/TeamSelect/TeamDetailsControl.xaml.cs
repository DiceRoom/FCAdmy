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

namespace FightingCommunityAdministrator
{
    /// <summary>
    /// TeamDetailsControl.xaml の相互作用ロジック
    /// </summary>
    public partial class TeamDetailsControl : UserControl
    {
		/************************************************************************/
		/* 基本処理	                                                            */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
        public TeamDetailsControl()
        {
            InitializeComponent();

            SpaceCoverRectangle.Visibility = Visibility.Hidden;
        }
    }
}
