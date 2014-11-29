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
	/// MemberDetailsScene.xaml の相互作用ロジック
	/// </summary>
	public partial class MemberDetailsScene : UserControl, SceneInterface
	{
		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MemberDetailsScene()
		{
			InitializeComponent();

			//データを作ってバインドする
			_InitializeTable();
		}

		/// <summary>
		/// デストラクタ
		/// </summary>
		~MemberDetailsScene()
		{
		}

		/// <summary>
		/// 画面を覆うフィルターの見える横幅のサイズ
		/// </summary>
		/// <returns>フィルター幅</returns>
		public double GetDisplayFilterWidth() { return 35; }

		/// <summary>
		/// 別のシーンから戻って来た時に呼び出される処理
		/// </summary>
		public void SceneBack() { }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/
		
		//============================================================================
		//! ダブルクリックがされた
		private void _DoubleClickCell(object iSender, MouseButtonEventArgs iArgs)
		{
			var cell = iSender as DataGridCell;
			if (cell != null)
			{
				var items = MemberDataGrid.SelectedItems;
				if (items.Count > 0)
				{
					//データを取得してウィンドウを表示する
					var item = (MemberDetailsData)items[0];
					var window = new MenberDetailWindow(item.ID);
					window.ShowDialog();
				}
			}
		}

		//============================================================================
		//! 戻るボタンが押された時の処理
		private void _ClickReturn(object iSender, RoutedEventArgs iArgs)
		{
			SceneManager.GetInstance().ReturnBackScene();
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		//============================================================================
		//! テーブルの初期化
		private void _InitializeTable()
		{
			//メンバー全員を入れる
			var member_manager = MemberManager.GetInstance();
			var battle_manager = BattleManager.GetInstance();
			foreach (var iMember in member_manager.GetMemberList())
			{
				//基本情報
				var data = new MemberDetailsData();
				data.ID = iMember.mID;
				data.IDString = string.Format("{0:D3}", iMember.mID);
				data.Name = iMember.mName;
				data.DefaultCharaID = iMember.mDefaultCharacterID;
				var split = iMember.mResistDate.Split(' ');
				data.RegistaDate = (split.Length == 2) ? split[0] + "\n" + split[1] : iMember.mResistDate;

				//大会情報
				var result_list = battle_manager.GetMemberBattleResultList(iMember.mID, true);
				data.JoinTournamentVal = result_list.Count;
				if (data.JoinTournamentVal == 0)
				{
					data.LastJoinTournamentDate = "---";
				}
				else
				{
					int battle_index = result_list[0].mIndex;
					data.LastJoinTournamentDate += battle_manager.GetBattle(result_list[0].mIndex).mDate;
					for (var index = 1; index < result_list.Count; ++index)
					{
						var date = battle_manager.GetBattle(result_list[index].mIndex).mDate;
						if (data.LastJoinTournamentDate.CompareTo(date) < 0)
						{
							data.LastJoinTournamentDate = date;
							battle_index = result_list[index].mIndex;
						}
					}
					data.LastJoinTournamentDate = battle_manager.GetBattle(battle_index).mName + "\n" + data.LastJoinTournamentDate;
				}

				//アイコン設定
				bool set_Valid_Use_Character_ID_Flg = (iMember.mDefaultCharacterID != -1);
				if (set_Valid_Use_Character_ID_Flg)
				{
					try
					{
						data.Icon = PresetManager.GetInstance().GetCharacterInfo(iMember.mDefaultCharacterID).mIconImage;
					}
					catch (System.Exception)
					{
						set_Valid_Use_Character_ID_Flg = false;
					}
				}
				if (!set_Valid_Use_Character_ID_Flg)
				{
					data.Icon = PresetManager.GetInstance().GetUnknownCharacterIcon();
				}

				mMemberGridTable.Add(data);
			}

			//バインド
			MemberDataGrid.ItemsSource = mMemberGridTable;
		}

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// メンバー登録用グリッド
		/// </summary>
		private List<MemberDetailsData> mMemberGridTable = new List<MemberDetailsData>();	
	}
}
