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
	/// MemberListControl.xaml の相互作用ロジック
	/// </summary>
	public partial class CharacterSelectListControl : UserControl
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public CharacterSelectListControl()
		{
			InitializeComponent();
			FilterRectangle.Visibility = Visibility.Hidden;
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// キャラクター情報の設定
		/// </summary>
		/// <param name="iCharacterInfo">キャラクター情報</param>
		public void SetCharacterInfo(PresetManager.SCharacterInfo iCharacterInfo)
		{
			//保持
			mUseCharacterInfo = iCharacterInfo;

			//テキスト系設定
			CharacterNoTextBlock.Text = string.Format("No.{0:D2}", iCharacterInfo.mIndex);
			CharacterNameTextBlock.Text = iCharacterInfo.mName;

			//アイコン設定
			try
			{
				CharacterImage.Source = iCharacterInfo.mIconImage;
			}
			catch (System.Exception)
			{
			}
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! カーソルが項目に乗った時の処理
		private void _MouseEnter(object iSender, MouseEventArgs iArgs)
		{
			FilterRectangle.Visibility = Visibility.Visible;
		}

		//============================================================================
		//! カーソルが項目から出た時の処理
		private void _MouseLeave(object iSender, MouseEventArgs iArgs)
		{
			FilterRectangle.Visibility = Visibility.Hidden;
		}

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 表示中のメンバー情報
		/// </summary>
		private PresetManager.SCharacterInfo mUseCharacterInfo;
	}
}
