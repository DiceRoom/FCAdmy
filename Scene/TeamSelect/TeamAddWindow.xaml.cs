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
	/// MemberAddWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class TeamAddWindow : Window
	{
		/************************************************************************/
		/* 公開処理																*/
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
        public TeamAddWindow()
		{
			InitializeComponent();
			TeamNameTextBox.Text = "";
			TeamNameTextBox.Focus();
		}

		/// <summary>
		/// コールバックの設定(先にやってしまうと困る)
		/// </summary>
		public void SettingCallback()
		{
			AddButton.Click += _ClickButton;
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! ボタンが押された時の処理
		private void _ClickButton(object iSender, RoutedEventArgs iArgs)
		{
			//OKボタンの時は内部チェック
			if (((Button)iSender).TabIndex == 0)
			{
				//テキストボックスチェック
				if (TeamNameTextBox.Text.Length == 0)
				{
					SystemUtility.DisplayErrorDialog("チーム名を入力してください");
					iArgs.Handled = true;
					return;
				}
			}

			Close();
		}
	}
}
