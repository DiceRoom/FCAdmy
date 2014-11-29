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
	public partial class NoTeamMemberControl : UserControl
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
        public NoTeamMemberControl()
		{
			InitializeComponent();
            FilterRectangle.Visibility = Visibility.Hidden;
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// 参加メンバー情報の設定
		/// </summary>
        /// <param name="iJoinMemberInfo">参加メンバー情報</param>
		public void SetJoinMemberInfo(BattleManager.SBattleJoinInfo iJoinMemberInfo)
		{
			//テキスト系設定
            MemberNoTextBlock.Text = string.Format("参加No.{0:D2}", iJoinMemberInfo.mJoinID + 1);
            MemberNameTextBlock.Text = MemberManager.GetInstance().GetMemberInfo(iJoinMemberInfo.mMemberID).mName;

			//アイコン設定
            bool set_Valid_Use_Character_ID_Flg = (iJoinMemberInfo.mUserCharacterID != -1);
            if (set_Valid_Use_Character_ID_Flg)
            {
                try
                {
                    UseCharacterImage.Source = PresetManager.GetInstance().GetCharacterInfo(iJoinMemberInfo.mUserCharacterID).mIconImage;
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

        /// <summary>
        /// フィルターの表示を開始する
        /// </summary>
        public void OnSelectionFilter()
        {
            MouseEnter += _MouseEnter;
            MouseLeave += _MouseLeave;
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
