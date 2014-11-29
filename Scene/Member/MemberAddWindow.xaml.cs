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
	public partial class MemberAddWindow : Window
	{
		/************************************************************************/
		/* 公開処理																*/
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MemberAddWindow()
		{
			InitializeComponent();
			MemberNameTextBox.Text = "";
			MemberNameTextBox.Focus();

			//コンボボックス設定
			CharacterComboBox.Items.Add("未選択");
			foreach (var iInfo in PresetManager.GetInstance().GetCharacterList())
			{
				CharacterComboBox.Items.Add(iInfo.mName);
			}
			CharacterComboBox.SelectedIndex = 0;
		}

		/// <summary>
		/// コールバックの設定(先にやってしまうと困る)
		/// </summary>
		public void SettingCallback()
		{
			AddButton.Click += _ClickButton;
		}

		/// <summary>
		/// 選択しているキャラクター番号の取得
		/// </summary>
		/// <returns></returns>
		public int GetSelectCharacterIndex()
		{
			var chara_Name = CharacterComboBox.SelectedItem.ToString();
			return PresetManager.GetInstance().GetCharacterIndex(chara_Name);
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
				if (MemberNameTextBox.Text.Length == 0)
				{
					SystemUtility.DisplayErrorDialog("メンバー名を入力してください");
					iArgs.Handled = true;
					return;
				}
			}

			Close();
		}
	}
}
