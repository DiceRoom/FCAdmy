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
	public partial class SelectMemberControl : UserControl
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SelectMemberControl()
		{
			InitializeComponent();
            FilterRectangle.Visibility = Visibility.Hidden;
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// メンバー情報の設定
		/// </summary>
		/// <param name="iJoinIndex">参加番号</param>
		/// <param name="iMemberInfo">メンバー情報</param>
		public void SetMemberInfo(int iJoinIndex, string iMemberName, int iCharacterID)
		{
			//テキスト系設定
			MemberNoTextBlock.Text = string.Format("参加No.{0:D2}", iJoinIndex + 1);
			MemberNameTextBlock.Text = iMemberName;

			//アイコン設定
			SetCharacter(iCharacterID);
		}

		/// <summary>
		/// キャラクター番号の設定
		/// </summary>
		/// <param name="iCharacterID">キャラクター</param>
		public void SetCharacter(int iCharacterID)
		{
			bool set_Valid_Use_Character_ID_Flg = (iCharacterID != -1);
			if (set_Valid_Use_Character_ID_Flg)
			{
				try
				{
					UseCharacterImage.Source = PresetManager.GetInstance().GetCharacterInfo(iCharacterID).mIconImage;
				}
				catch (System.Exception)
				{
					set_Valid_Use_Character_ID_Flg = false;
				}
			}
			if (!set_Valid_Use_Character_ID_Flg)
			{
				UseCharacterImage.Source = PresetManager.GetInstance().GetUnknownCharacterIcon();
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
	}
}
