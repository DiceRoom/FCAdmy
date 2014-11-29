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
	/// MenberDetailWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MenberDetailWindow : Window
	{
		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		/// <param name="iMemberID">メンバーID</param>
		public MenberDetailWindow(int iMemberID)
		{
			InitializeComponent();

			//メンバー情報登録
			var member_manager = MemberManager.GetInstance();
			var member_info = member_manager.GetMemberInfo(iMemberID);
			Title = member_info.mName + "の詳細";
			RegistTextBlock.Text = string.Format("登録番号No.{0:D3}", iMemberID);
			RegistDateTextBlock.Text = "登録日時 " + member_info.mResistDate;
			MemberNameTextBlock.Text = member_info.mName;
			MemberNameShadowTextBlock.Text = member_info.mName;

			//アイコン設定
			bool set_Valid_Use_Character_ID_Flg = (member_info.mDefaultCharacterID != -1);
			if (set_Valid_Use_Character_ID_Flg)
			{
				try
				{
					CharacterIcon.Source = PresetManager.GetInstance().GetCharacterInfo(member_info.mDefaultCharacterID).mIconImage;
				}
				catch (System.Exception)
				{
					set_Valid_Use_Character_ID_Flg = false;
				}
			}
			if (!set_Valid_Use_Character_ID_Flg)
			{
				CharacterIcon.Source = PresetManager.GetInstance().GetUnknownCharacterIcon();
			}

			//大会情報登録
			_CreateBattleTable(iMemberID);
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! 大会の簡易一覧のセルがダブルクリックされた
		private void _DoubleClickSimpleDispCell(object iSender, MouseButtonEventArgs iArgs)
		{
			//移動先のタブ検出
			var cell = iSender as DataGridCell;
			int tab_index = 0;
			if (cell != null)
			{
				var items = BattleGridView.SelectedItems;
				if (items.Count > 0)
				{
					var item = (TotalBattleData)items[0];
					switch (item.BattleKind)
					{
					case BattleManager.EBattleKind.BATTLE_SIMPLE_TOURNAMENT:
						tab_index = 1;
						break;
					}
				}
			}

			//タブ移動
			if (tab_index != 0)
			{
				Action asyncWork = () =>
				{
					Dispatcher.Invoke(new Action(() =>
					{
						BattleResultTabControl.SelectedIndex = 1;
					}));
				};
				asyncWork.BeginInvoke(null, null);
			}
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! 各種大会テーブルの作成
		private void _CreateBattleTable(int iMemberID)
		{
			var battle_manager = BattleManager.GetInstance();
			var preset_manager = PresetManager.GetInstance();
			var result_list = battle_manager.GetMemberBattleResultList(iMemberID, false);
			var total_list = new List<TotalBattleData>();
			foreach (var iIndex in System.Linq.Enumerable.Range(0, result_list.Count))
			{
				//大会情報取得
				var result = result_list[result_list.Count - iIndex - 1];
				var info = battle_manager.GetBattle(result.mIndex);

				//全ての部分の追加
				var total_data = new TotalBattleData();
				total_data.Icon = preset_manager.GetCharacterInfo(result.mUseCharacterID).mIconImage;
				total_data.UseCharacterID = result.mUseCharacterID;
				total_data.BattleID = result.mIndex;
				total_data.Name = info.mName;
				var split_list = info.mDate.Split(' ');
				total_data.BattleDate = split_list[0] + "\n" + split_list[1];
				total_data.BattleKind = info.mBattleKind;
				total_list.Add(total_data);

				//種別別に登録
				if(info.mBattleKind == BattleManager.EBattleKind.BATTLE_SIMPLE_TOURNAMENT)
				{
					var control = new SimpleTournamentResultControl(iMemberID, result, info);
					SimpleTournamentStackPanel.Children.Add(control);
				}

			}
			BattleGridView.ItemsSource = total_list;
		}
	}
}
