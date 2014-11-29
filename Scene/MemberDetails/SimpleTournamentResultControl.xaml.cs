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
	/// SingleSimpleTournamentResultControl.xaml の相互作用ロジック
	/// </summary>
	public partial class SimpleTournamentResultControl : UserControl
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SimpleTournamentResultControl(int iMemberID, BattleManager.SMemberBattleResult iResult, BattleManager.SBattleInfo iBattleInfo)
		{
			InitializeComponent();

			//情報登録
			bool team_flg = (iBattleInfo.mTeamList.Count != 0);
			MainGroupBox.Header = "大会No." + iResult.mIndex.ToString() + " - " + iBattleInfo.mName + " -";
			TournamentKindTextBlock.Text = "種別：" + (team_flg ? "チーム戦" : "シングル戦");
			JoinNumTextBlock.Text = "参加人数：" + iBattleInfo.mJoinList.Count.ToString() + "人";
			DateTextBlock.Text = "開催日時 " + iBattleInfo.mDate;
			JoinTeamNumTextBlock.Text = team_flg ? "参加チーム数：" + iBattleInfo.mTeamList.Count.ToString() + "チーム" : "";
			ResultTextBlock.Text = "- ";
			var obj = (SimpleTournamentObject)iBattleInfo.mBattleObject;
			var result = obj.GetSimpleTournamentResult(iBattleInfo, iMemberID);
			if (result.mRank == 1)
			{
				ResultTextBlock.Text += "優勝";
			}
			else if (result.mRank == 2)
			{
				ResultTextBlock.Text += "準優勝";
				ResultTextBlock.Foreground = SystemUtility.CreateLinearGradientBrash(SystemUtility.StringToColor("#FFFFFF00"), SystemUtility.StringToColor("#FFFFFFF55"));
			}
			else
			{
				ResultTextBlock.Text += ("第" + result.mRank + "位");
				ResultTextBlock.Foreground = new SolidColorBrush(SystemUtility.StringToColor("#FF5555FF"));
			}
			ResultTextBlock.Text += " -";
			BeginVersusNumTextBlock.Text = result.mBeginVersus.ToString() + "回戦からの開始";
			var character_Info = PresetManager.GetInstance().GetCharacterInfo(iResult.mUseCharacterID);
			CharacterIcon.Source = character_Info.mIconImage;
			UseCharacterTextBlock.Text = "使用キャラ：" + character_Info.mName;
		}
	}
}
