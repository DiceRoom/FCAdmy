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
using System.Windows.Shapes;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TeamMemberValSettingWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class TeamMemberValSettingWindow : Window
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TeamMemberValSettingWindow()
		{
			InitializeComponent();
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! キャンセル
		private void _ClickCancelButton(object iSender, RoutedEventArgs iArgs)
		{
			Close();
		}
	}
}
