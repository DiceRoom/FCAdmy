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
	/// AutoSaveWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class WaitSaveWindow : Window
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public WaitSaveWindow()
		{
			InitializeComponent();

			//メッセージ付与
			MessageLabel.Content = "プロジェクトファイルを保存中です...";

			//位置を変更
			SourceInitialized += (iSender, iArgs) =>
			{
				var main_Window = Application.Current.MainWindow;
				Left = main_Window.Left + (main_Window.Width / 2) - (Width / 2);
				Top = main_Window.Top + (main_Window.Height / 2) - (Height / 2);
			};
		}
	}
}
