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
	/// TournamentResultWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class TournamentResultWindow : Window
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="iLeafName1">リーフ1名</param>
		/// <param name="iLeafName2">リーフ2名</param>
		public TournamentResultWindow(string iLeafName1, string iLeafName2)
		{
			InitializeComponent();

			//チーム名設定
			UpLeafRadioButton.Content = iLeafName1;
			DownLeafRadioButton.Content = iLeafName2;
		}

		/// <summary>
		/// ウィンドウ表示
		/// </summary>
		public void ShowDialogEx()
		{
			DecideButton.Click += _ClickButton;
			ShowDialog();
		}

		/************************************************************************/
		/* コールバック定義                                                     */
		/************************************************************************/

		//============================================================================
		//! ボタンが押された時の処理
		private void _ClickButton(object iSender, RoutedEventArgs iArgs)
		{
			Close();
		}
	}
}
