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
	public partial class ScreenShotMemberControl : UserControl
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public ScreenShotMemberControl()
		{
			InitializeComponent();
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
            MemberNoTextBlock.Text = string.Format("参加No.{0:D2}", iJoinMemberInfo.mJoinID);
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
	}
}
