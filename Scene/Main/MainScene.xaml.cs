using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TitleContorl.xaml の相互作用ロジック
	/// </summary>
	public partial class MainScene : UserControl, SceneInterface
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MainScene()
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
		public double GetDisplayFilterWidth() { return 90; }

		/// <summary>
		/// 他のシーンから戻ってきた際の処理
		/// </summary>
		public void SceneBack() { }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! ボタンが押された
		private void _ClickButton(object iSender, RoutedEventArgs iArgs)
		{
			var button_index = ((Button)iSender).TabIndex;

			//トーナメント形式
			if (button_index < 2)
			{
				var scene = (TournamentSelectScene)SceneManager.GetInstance().AdvanceScene(SceneManager.EScene.SCENE_BATTLE_TOURNAMENT_SELECT);
				scene.SetTeamBattleFlg((button_index == 1));
			}
			//それ以外
			else
			{
				button_index -= 2;
				var scene_Index_List = new SceneManager.EScene[5]
				{
					SceneManager.EScene.SCENE_INTRASQUAD_GAME,
					SceneManager.EScene.SCENE_MEMBER,
					SceneManager.EScene.SCENE_MEMBER_DETAILS,
					SceneManager.EScene.SCENE_BATTLE_HISTORY,
					SceneManager.EScene.SCENE_SYSTEM_SETTINGS,
				};
				SceneManager.GetInstance().AdvanceScene(scene_Index_List[button_index]);
			}
		}
	}
}
