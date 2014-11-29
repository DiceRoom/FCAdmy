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
	public partial class BattleNameInputWindow : Window
	{
		/************************************************************************/
		/* 公開処理																*/
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
        public BattleNameInputWindow()
		{
			InitializeComponent();
			BattleNameTextBox.Text = "";
			BattleNameTextBox.Focus();
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
				if (BattleNameTextBox.Text.Length == 0)
				{
					SystemUtility.DisplayErrorDialog("大会名を入力してください");
					iArgs.Handled = true;
					return;
				}

				//過去の大会に同名の物が無いかチェック
				foreach (var iBattle in BattleManager.GetInstance().GetBattleList())
				{
					if (iBattle.mName == BattleNameTextBox.Text)
					{
						SystemUtility.DisplayErrorDialog("過去に行った大会で同名の物が存在します\n違う名前をを入力してください");
						iArgs.Handled = true;
						return;
					}
				}

				//使えない物をチェック
				var check_Char_List = new List<char>(new char[] { '\\', ':', '*', '?', '"', '<', '>', '|', '#', '{', '}', '%', '&', '~' });
				foreach (char iChar in BattleNameTextBox.Text)
				{
					if (check_Char_List.IndexOf(iChar) != -1)
					{
						SystemUtility.DisplayErrorDialog(string.Format("大会名に[\"{0}\"]の文字は使用できません", iChar));
						iArgs.Handled = true;
						return;
					}
				}
			}
			else
			{
				//キャンセルの場合
				BattleNameTextBox.Text = "";
			}

			Close();
		}
	}
}
