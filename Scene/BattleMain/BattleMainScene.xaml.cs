using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TitleContorl.xaml の相互作用ロジック
	/// </summary>
	public partial class BattleMainScene : UserControl, SceneInterface
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public BattleMainScene()
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
		public void SceneBack() { }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! ボタンが押された
		private void _ClickButton(object iSender, RoutedEventArgs iArgs)
		{
			var control = iSender as Control;
			if (control != null)
			{
				var manager = SceneManager.GetInstance();
				switch (control.TabIndex)
				{
				//シングル戦
				case 0:
					((BattleSelectMainScene)manager.AdvanceScene(SceneManager.EScene.SCENE_BATTLE_SELECT_MAIN)).SetSingleBattleFlg(true);
					break;
				//チーム戦
				case 1:
					((BattleSelectMainScene)manager.AdvanceScene(SceneManager.EScene.SCENE_BATTLE_SELECT_MAIN)).SetSingleBattleFlg(false);
					break;
				//紅白戦
				case 2:
					break;
				//履歴
				case 3:
					break;
				default:
					manager.ReturnBackScene();
					break;
				}
			}
		}
	}
}
