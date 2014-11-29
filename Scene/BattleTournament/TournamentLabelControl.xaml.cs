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
	/// TournamentTeamLabelControl.xaml の相互作用ロジック
	/// </summary>
	public partial class TournamentLabelControl : UserControl
	{
		/************************************************************************/
		/* 基本処理	                                                            */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TournamentLabelControl()
		{
			InitializeComponent();

			TopTextBlock.Text = "No Regist";
			SelectFilter.Visibility = Visibility.Hidden;
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// 表示情報設定
		/// </summary>
		/// <param name="iInfoIndex">表示情報番号</param>
		public void SetInfo(int iInfoIndex)
		{
			string top_Name = "";
			int character_Index = -1;

			//チーム戦の場合
			if (BattleOperatorManager.GetInstance().IsTeamBattleFlg())
			{
                var team_Info = BattleOperatorManager.GetInstance().GetTeamInfo(iInfoIndex);
                top_Name = team_Info.mTeamName;

                var join_Info = new BattleManager.SBattleJoinInfo();
                BattleOperatorManager.GetInstance().GetJoinInfo(ref join_Info, team_Info.mJoinIDList[0]);
                character_Index = join_Info.mUserCharacterID;
			}
			//シングル戦の場合
			else
			{
				var join_Info = new BattleManager.SBattleJoinInfo();
				BattleOperatorManager.GetInstance().GetJoinInfo(ref join_Info, iInfoIndex);
				top_Name = MemberManager.GetInstance().GetMemberInfo(join_Info.mMemberID).mName;
				character_Index = join_Info.mUserCharacterID;
			}

			//設定
			TopTextBlock.Text = top_Name;
			bool set_Valid_Use_Character_ID_Flg = (character_Index != -1);
			if (set_Valid_Use_Character_ID_Flg)
			{
				try
				{
					TopCharacterImage.Source = PresetManager.GetInstance().GetCharacterInfo(character_Index).mIconImage;
				}
				catch (System.Exception)
				{
					set_Valid_Use_Character_ID_Flg = false;
				}
			}
			if (!set_Valid_Use_Character_ID_Flg)
			{
				TopCharacterImage.Source = PresetManager.GetInstance().GetUnknownCharacterIcon();
			}
			TopCharacterImage.Visibility = Visibility.Visible;
			mInfoIndex = iInfoIndex;
		}

		/// <summary>
		/// 表示情報取得
		/// </summary>
		/// <returns>表示情報番号</returns>
		public int GetSetInfo()
		{
			return mInfoIndex;
		}

		/// <summary>
		/// 非表示未設定
		/// </summary>
		public void RemoveInfo()
		{
			mInfoIndex = -1;
			TopCharacterImage.Visibility = Visibility.Hidden;
		}

		/// <summary>
		/// 何からの情報が設定されているかの取得
		/// </summary>
		/// <returns>設定存在フラグ</returns>
		public bool IsSettingInfo()
		{
			return (mInfoIndex != -1);
		}

		/// <summary>
		/// トップテキスト名の取得
		/// </summary>
		/// <returns></returns>
		public string GetTopName()
		{
			return TopTextBlock.Text;
		}

		/// <summary>
		/// フィルターの表示切替
		/// </summary>
		/// <param name="iVisiblgFlg">表示フラグ</param>
		public void SetVisibleFilter(bool iVisiblgFlg)
		{
			SelectFilter.Visibility = iVisiblgFlg ? Visibility.Visible : Visibility.Hidden;
		}

		/// <summary>
		/// フィルターの表示状態の取得
		/// </summary>
		/// <returns>表示状態フラグ</returns>
		public bool IsVisibleFilter()
		{
			return (SelectFilter.Visibility == Visibility.Visible);
		}

		/// <summary>
		/// ラベルにTransform値を付与する
		/// </summary>
		/// <param name="iTransformGroup">Transform値</param>
		public void SetLabelTransform(TransformGroup iTransformGroup)
		{
			//TeamName.LayoutTransform = iTransformGroup;
		}

		/// <summary>
		/// 表示しているキャラクターイメージソースの取得
		/// </summary>
		/// <returns></returns>
		public ImageSource GetDisplayImageSource()
		{
			return TopCharacterImage.Source;
		}

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 情報番号
		/// </summary>
		private int mInfoIndex = -1;
	}
}
