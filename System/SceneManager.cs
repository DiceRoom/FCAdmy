using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// ダミーシーン(後で消すよ)
	/// </summary>
	public class SceneDummy : UserControl, SceneInterface
	{
		public SceneDummy()
		{
		}
		~SceneDummy()
		{
		}
		public double GetDisplayFilterWidth() { return 0; }
		public void SceneBack() { }
	}

	/// <summary>
	/// シーンのマネージャ
	/// </summary>
	public partial class SceneManager : Singleton<SceneManager>
	{
		/************************************************************************/
		/* 公開処理                                                             */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SceneManager()
		{
			//デリゲート作成
			mCreateSceneDelegateList[(int)EScene.SCENE_MAIN] = _CreateScene<MainScene>;
			mCreateSceneDelegateList[(int)EScene.SCENE_MEMBER] = _CreateScene<MemberScene>;
			mCreateSceneDelegateList[(int)EScene.SCENE_MEMBER_DETAILS] = _CreateScene<MemberDetailsScene>;
			mCreateSceneDelegateList[(int)EScene.SCENE_BATTLE_HISTORY] = _CreateScene<SceneDummy>;
			mCreateSceneDelegateList[(int)EScene.SCENE_BATTLE_TOURNAMENT_SELECT] = _CreateScene<TournamentSelectScene>;
			mCreateSceneDelegateList[(int)EScene.SCENE_BATTLE_ROUND_ROBIN] = _CreateScene<SceneDummy>;
			mCreateSceneDelegateList[(int)EScene.SCENE_BATTLE_SIMPLE_TOURNAMENT] = _CreateScene<SimpleTournamentScene>;
			mCreateSceneDelegateList[(int)EScene.SCENE_BATTLE_DOUBLE_ELM] = _CreateScene<SceneDummy>;
			mCreateSceneDelegateList[(int)EScene.SCENE_RESULT_TOURNAMENT] = _CreateScene<SceneDummy>;
			mCreateSceneDelegateList[(int)EScene.SCENE_INTRASQUAD_GAME] = _CreateScene<SceneDummy>;
			mCreateSceneDelegateList[(int)EScene.SCENE_SYSTEM_SETTINGS] = _CreateScene<SettingsScene>;
			mCreateSceneDelegateList[(int)EScene.SCENE_MEMBER_SELECT] = _CreateScene<MemberSelectScene>;
			mCreateSceneDelegateList[(int)EScene.SCENE_TEAM_SELECT] = _CreateScene<TeamSelectScene>;

			//シーン変更用のタイマー
			mTimer.Tick += _BeginFinishWindowFilter;
			mTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
		}

		/// <summary>
		/// コントロール系の設定
		/// </summary>
		/// <param name="iContentControl">コンテンツを実際に設定するコントロール</param>
		/// <param name="iLeftFilter">画面を覆う左側フィルター</param>
		/// <param name="iRightFilter">画面を覆う右側フィルター</param>
		public void SetControls(ContentControl iContentControl, Image iLeftFilter, Image iRightFilter)
		{
			mDisplayControl = iContentControl;
			mFilterImageList[0] = iLeftFilter;
			mFilterImageList[1] = iRightFilter;
		}

		/// <summary>
		/// 指定シーンを作成して進む
		/// </summary>
		/// <param name="iScene">シーン番号</param>
		/// <returns>シーンインスタンス</returns>
		public SceneInterface AdvanceScene(EScene iScene)
		{
            //コールバック初期化
            SystemManager.GetInstance().SetCloseCheckCallback(null);
            SystemManager.GetInstance().SetInputKeyDelegate(null);

			//シーンの作成と追加
			var scene = mCreateSceneDelegateList[(int)iScene]();
			var user_Control = (UserControl)scene;
			user_Control.TabIndex = (int)iScene;
			mSceneStack.Add(scene);

			//シーン変更演出の開始(ルートシーンの場合は開けるところから
			if (mSceneStack.Count == 1)
			{
				_WrapWindowFilter(null, null);
			}
			else
			{
				_BeginChangeScene();
			}
			
			return scene;
		}

		/// <summary>
		/// 一つ前のシーンに戻る
		/// </summary>
		public void ReturnBackScene()
		{
            //コールバック初期化
            SystemManager.GetInstance().SetCloseCheckCallback(null);
            SystemManager.GetInstance().SetInputKeyDelegate(null);

			//一つ削除
			mSceneStack.RemoveAt(mSceneStack.Count - 1);

			//初期化
			mSceneStack[mSceneStack.Count - 1].SceneBack();

			//演出
			_BeginChangeScene();
		}

        /// <summary>
        /// 現在何階層シーンが詰まれているかの取得
        /// </summary>
        /// <returns>改装</returns>
        public int GetCurrentSceneDepth()
        {
            return mSceneStack.Count;
        }

		/// <summary>
		/// 指定したシーン番号まで戻る(無い場合は無視)
		/// </summary>
		/// <param name="iScene">シーン番号</param>
		public void ReturnBackScene(EScene iScene)
		{
			int return_Index = -1;
			int scene_Index = (int)iScene;
			for(int index = mSceneStack.Count - 2 ; index >= 0 ; -- index)
			{
				var control = (UserControl)mSceneStack[index];
				if (control.TabIndex == scene_Index)
				{
					return_Index = index;
					break;
				}
			}

			//無い場合及び現在は無視
			if(return_Index == -1)
			{
				return;
			}

			//一つ前まで削除
			++return_Index;
			mSceneStack.RemoveRange(return_Index, mSceneStack.Count - return_Index - 1);

			//指定位置の一つ前まで戻って一つ前に戻る処理を行う
			ReturnBackScene();
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! シーンの作成
		public ClassType _CreateScene<ClassType>() where ClassType : new()
		{
			var scene = new ClassType();
			return scene;
		}

		//============================================================================
		//! シーン変更時の演出開始
		private void _BeginChangeScene()
		{
			//コントロールを一旦切る
			mDisplayControl.IsEnabled = false;

			//アニメーション設定
			var storyboard = new Storyboard();
			storyboard.Completed += _WrapWindowFilter;
			var animation = new ThicknessAnimation
			{
				From = new Thickness(mFilterImageList[0].Margin.Left, 0, 0, 0),
				EasingFunction = new PowerEase() {EasingMode = EasingMode.EaseInOut, Power = mEasingPower},
				To = new Thickness(0, 0, 0, 0),
				Duration = TimeSpan.FromMilliseconds(mWindowFilterTime)
			};
			Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
			storyboard.Children.Add(animation);
			mFilterImageList[0].BeginStoryboard(storyboard);

			storyboard = new Storyboard();
			animation = new ThicknessAnimation
			{
				From = new Thickness(0, 0, mFilterImageList[1].Margin.Right,0),
				EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut, Power = mEasingPower},
				To = new Thickness(0, 0, 0, 0),
				Duration = TimeSpan.FromMilliseconds(mWindowFilterTime)
			};
			Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
			storyboard.Children.Add(animation);
			mFilterImageList[1].BeginStoryboard(storyboard);
		}

		//============================================================================
		//! 画面をフィルターで覆った時に呼ばれる
		private void _WrapWindowFilter(object iSender, EventArgs iArgs)
		{
			//コントロールの変更
			var control = mSceneStack[mSceneStack.Count - 1];
			mDisplayControl.Content = control;
			Application.Current.MainWindow.Title = "FCあどみぃ！ -" + ((UserControl)control).Name + "-";

			//タイマー開始
			mTimer.Start();
		}

		//============================================================================
		//! 覆ったフィルターを外す
		private void _BeginFinishWindowFilter(object iSender, EventArgs iArgs)
		{
			//タイマー停止
			mTimer.Stop();

			var in_Fillter_Width = mSceneStack[mSceneStack.Count - 1].GetDisplayFilterWidth();

			//アニメーション設定
			var storyboard = new Storyboard();
			storyboard.Completed += _EndSceneAnimation;
			var x_Pos = -(mFilterImageList[0].Width - in_Fillter_Width);
			var animation = new ThicknessAnimation
			{
				From = new Thickness(0, 0, 0, 0),
				EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut, Power = mEasingPower },
				To = new Thickness(x_Pos, 0, 0, 0),
				Duration = TimeSpan.FromMilliseconds(mWindowFilterTime)
			};
			Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
			storyboard.Children.Add(animation);
			mFilterImageList[0].BeginStoryboard(storyboard);

			storyboard = new Storyboard();
			x_Pos = -(mFilterImageList[1].Width - in_Fillter_Width);
			animation = new ThicknessAnimation
			{
				From = new Thickness(0, 0, 0, 0),
				EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut, Power = mEasingPower },
				To = new Thickness(0, 0, x_Pos, 0),
				Duration = TimeSpan.FromMilliseconds(mWindowFilterTime)
			};
			Storyboard.SetTargetProperty(animation, new PropertyPath("Margin"));
			storyboard.Children.Add(animation);
			mFilterImageList[1].BeginStoryboard(storyboard);
		}

		//============================================================================
		//! シーン変更時の演出終了
		private void _EndSceneAnimation(object iSender, EventArgs iArgs)
		{
			//コントロールを有効にする
			mDisplayControl.IsEnabled = true;
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		// シーン作成デリゲート
		private delegate ClassType CreateSceneDelegate<ClassType>();

		/************************************************************************/
		/* 変数宣言                                                             */
		/************************************************************************/

		/// <summary>
		/// シーン作成デリゲートリスト
		/// </summary>
		private CreateSceneDelegate<SceneInterface>[] mCreateSceneDelegateList = new CreateSceneDelegate<SceneInterface>[(int)EScene.SCENE_VAL];

		/// <summary>
		/// シーンのコントロールを表示するコントローラ
		/// </summary>
		private ContentControl mDisplayControl;

		/// <summary>
		/// フィルター用イメージリスト
		/// </summary>
		private Image[] mFilterImageList = new Image[2];

		/// <summary>
		/// 現在詰んでいるシーンスタック
		/// </summary>
		private List<SceneInterface> mSceneStack = new List<SceneInterface>();

		/// <summary>
		/// 登録キーダウンイベントリスト
		/// </summary>
		private List<KeyEventHandler> mKeyDownEventList = new List<KeyEventHandler>();

		/// <summary>
		/// ウィンドウにフィルターが掛かる(又は解除される)までに掛かる時間
		/// </summary>
		private float mWindowFilterTime = 400;

		/// <summary>
		/// イージングパワー
		/// </summary>
		private float mEasingPower = 3;

		/// <summary>
		/// タイマー
		/// </summary>
		private System.Windows.Threading.DispatcherTimer mTimer = new System.Windows.Threading.DispatcherTimer();
	}
}
