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
	public partial class MemberListControl : UserControl
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MemberListControl()
		{
			InitializeComponent();
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// メンバー情報の設定
		/// </summary>
		/// <param name="iMemberInfo">メンバー情報</param>
		public void SetMemberInfo(MemberManager.SMemberInfo iMemberInfo)
		{
			//保持
			mUseMemberInfo = iMemberInfo;

			//テキスト系設定
			MemberNoTextBlock.Text = string.Format("MemberNo.{0:D3}", iMemberInfo.mID + 1);
			MemberNameTextBlock.Text = iMemberInfo.mName;

			//アイコン設定
			bool set_Valid_Use_Character_ID_Flg = (iMemberInfo.mDefaultCharacterID != -1);
			if (set_Valid_Use_Character_ID_Flg)
			{
				try
				{
					UseCharacterImage.Source = PresetManager.GetInstance().GetCharacterInfo(iMemberInfo.mDefaultCharacterID).mIconImage;
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
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 表示中のメンバー情報
		/// </summary>
		private MemberManager.SMemberInfo mUseMemberInfo;
	}
}
