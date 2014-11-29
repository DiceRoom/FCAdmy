using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TitleContorl.xaml の相互作用ロジック
	/// </summary>
	public partial class TournamentSelectScene : UserControl, SceneInterface
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public TournamentSelectScene()
		{
			InitializeComponent();
		}

        /************************************************************************/
        /* 継承処理                                                             */
        /************************************************************************/

		/// <summary>
		/// 画面を覆うフィルターの見える横幅のサイズ
		/// </summary>
		/// <returns>フィルター幅</returns>
		public double GetDisplayFilterWidth() { return 50; }

		/// <summary>
		/// 他のシーンから戻ってきた際の処理
		/// </summary>
		public void SceneBack()
		{
			BattleOperatorManager.GetInstance().CancelBattle();
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// チームバトルフラグの設定
		/// </summary>
		/// <param name="iTeamBattleFlg">チームバトルフラグ</param>
		public void SetTeamBattleFlg(bool iTeamBattleFlg) { mTeamBattleFlg = iTeamBattleFlg; }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! ボタンが押された
		private void _ClickButton(object iSender, RoutedEventArgs iArgs)
		{
			var button = iSender as Button;
			if (button != null)
			{
				switch (button.TabIndex)
				{
				//リーグ形式
				case 0:
					break;
				//シンプルトーナメント形式
				case 1:
					_SetupSimpleTournament();
					break;
				//ダブルイリミネーション形式
				case 2:
					break;
				//フリートーナメント形式
				case 3:
					break;
				//それ以外は戻ると見なす
				default:
					SceneManager.GetInstance().ReturnBackScene();
					break;
				}
			}
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! シンプルトーナメントのセットアップ
		private void _SetupSimpleTournament()
		{
			//大会名
			var window = new BattleNameInputWindow();
			string battle_Name = "";
			window.Closing += (iSender, iArgs) =>
			{
				battle_Name = window.BattleNameTextBox.Text.Trim();
			};
			window.ShowDialog();
			if (battle_Name.Length == 0)
			{
				return;
			}

			var btl_Op_Manager = BattleOperatorManager.GetInstance();
			btl_Op_Manager.BeginBattle(BattleManager.EBattleKind.BATTLE_SIMPLE_TOURNAMENT);
			btl_Op_Manager.SetBattleName(battle_Name);
			btl_Op_Manager.SetBattleObject(new SimpleTournamentObject());
			var scene = (MemberSelectScene)SceneManager.GetInstance().AdvanceScene(SceneManager.EScene.SCENE_MEMBER_SELECT);

			//メンバーが決定された時の処理
			scene.SetDecideMemberDelegate(delegate()
			{
				//チームバトルの場合はチーム選択画面へ
				if (mTeamBattleFlg)
				{
					var team_Scene = (TeamSelectScene)SceneManager.GetInstance().AdvanceScene(SceneManager.EScene.SCENE_TEAM_SELECT);
					team_Scene.SetDecideTeamDelegate(delegate()
					{
						_BeginSimpleTournament();
					}
					);
				}
				else
				{
					_BeginSimpleTournament();
				}
			}
			);

		}

		//============================================================================
		//! シンプルトーナメントの実行
		private void _BeginSimpleTournament()
		{
			SceneManager.GetInstance().AdvanceScene(SceneManager.EScene.SCENE_BATTLE_SIMPLE_TOURNAMENT);
		}

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// チームバトルか
		/// </summary>
		private bool mTeamBattleFlg;
	}
}
