using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using Microsoft.Win32;
using WindowsForm = System.Windows.Forms;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TitleContorl.xaml の相互作用ロジック
	/// </summary>
	public partial class LoadScene : UserControl
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public LoadScene()
		{
			InitializeComponent();
			ContentBorder.IsEnabled = false;

			//前に読み込んだファイルが存在しない場合はクリック出来ない
			var get_Obj = DataManager.GetInstance().GetData(SystemUtility.GetSceneName(this));
			if (get_Obj == null)
			{
				RefPrevFileButton.IsEnabled = false;
			}
			else
			{
				var info = (SSerialData)get_Obj;
				if (System.IO.File.Exists(info.mLastRefFilePath))
				{
					RefPrevFileButton.IsEnabled = true;
				}
			}

            //情報設定
            var version = Version.CurrentVersion;
            InfoTextBlock.Text = string.Format("FCあどみぃ！ Ver{0} -{1}-" ,version, SystemManager.GetInstance().GetUserName());
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! ロード完了
		private void _Loaded(object iSender, RoutedEventArgs iArgs)
		{
			//開始アニメーション
			var storyboard = new Storyboard();
			storyboard.Completed += (iCmpSender, iCmpArgs) =>
			{
				ContentBorder.IsEnabled = true;
			};
			var animation = new DoubleAnimation
			{
				From = 0,
				EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut, Power = 4 },
				To = ContentBorder.Width,
				Duration = TimeSpan.FromMilliseconds(500)
			};
			Storyboard.SetTargetProperty(animation, new PropertyPath("Width"));
			storyboard.Children.Add(animation);
			animation = new DoubleAnimation
			{
				From = 0,
				EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut, Power = 4 },
				To = ContentBorder.Height,
				Duration = TimeSpan.FromMilliseconds(500)
			};
			Storyboard.SetTargetProperty(animation, new PropertyPath("Height"));
			storyboard.Children.Add(animation);
			ContentBorder.BeginStoryboard(storyboard);
		}

		//============================================================================
		//! 最後に読み込んだプロジェクトの読み込みボタンが押された
		private void _ClickRefPrevProject(object iSender, RoutedEventArgs iArgs)
		{
			var get_Obj = DataManager.GetInstance().GetData(SystemUtility.GetSceneName(this));
			if (get_Obj != null && _LoadProjectFile(((SSerialData)get_Obj).mLastRefFilePath))
			{
				_StartUpTool();
			}
		}

		//============================================================================
		//! 新しいプロジェクトの作成ボタンが押された
		private void _ClickNewProject(object iSender, RoutedEventArgs iArgs)
		{
			//プリセットデータを選択させて一旦保存してから開始
			var window = new SelectPresetWindow();
			window.ShowDialog();
			string preset_name = window.GetPresetName();
			if (preset_name == null)
			{
				return;
			}

			//プロジェクトの保管先の設定
			SaveFileDialog dialog = new SaveFileDialog();
			dialog.Filter = "FCあどみぃ！プロジェクトファイル(*.pjb)|*.pjb|すべてのファイル(*.*)|*.*";
			dialog.Title = "プロジェクトファイルを選択してください";
			dialog.RestoreDirectory = true;
			dialog.InitialDirectory = SystemUtility.GetSystemPath();
			if (dialog.ShowDialog() == true)
			{
				//データの設定
				DataManager.GetInstance().SetData(SaveManager.GetInstance().PresetInfoKey, preset_name);

				//一旦状態保存
				var info = new SSerialData();
				info.mLastRefFilePath = dialog.FileName;
				DataManager.GetInstance().SetData(SystemUtility.GetSceneName(this) , info);
				SaveManager.GetInstance().ProjectFilePath = dialog.FileName;
				SaveManager.GetInstance().SaveProject();
				SaveManager.GetInstance().SaveContentData();
				
				//必要なマネージャの初期化
				MemberManager.GetInstance().LoadMemberData();
				BattleManager.GetInstance().LoadBattleData();
				PresetManager.GetInstance().LoadPreset();
                SystemManager.GetInstance().LoadSystemData();

				_StartUpTool();
			}
		}

		//============================================================================
		//! プロジェクトの読み込みボタンが押された
		private void _ClickLoadProject(object iSender, RoutedEventArgs iArgs)
		{
			//OpenFileDialogクラスのインスタンスを作成
			OpenFileDialog dialog = new OpenFileDialog();
			dialog.Filter = "FCあどみぃ！プロジェクトファイル(*.pjb)|*.pjb|すべてのファイル(*.*)|*.*";
			dialog.Title = "プロジェクトファイルを選択してください";
			dialog.RestoreDirectory = true;
			if (dialog.ShowDialog() == true && _LoadProjectFile(dialog.FileName))
			{
				//読み込みファイルの保存
				var info = new SSerialData();
				info.mLastRefFilePath = dialog.FileName;
				DataManager.GetInstance().SetData(SystemUtility.GetSceneName(this), info);
				SaveManager.GetInstance().SaveContentData();

				//開始
				_StartUpTool();
			}
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! プロジェクトファイルのロード
		private bool _LoadProjectFile(string iFilePath)
		{
			try
			{
				//ファイルが無ければ無視
				if (!System.IO.File.Exists(iFilePath))
				{
					return false;
				}

				//必要なマネージャの初期化
				SaveManager.GetInstance().ProjectFilePath = iFilePath;
				DataManager.GetInstance().ReadData(iFilePath);
				MemberManager.GetInstance().LoadMemberData();
				BattleManager.GetInstance().LoadBattleData();
				PresetManager.GetInstance().LoadPreset();
                SystemManager.GetInstance().LoadSystemData();

				return true;
			}
			catch (System.Exception)
			{
				return false;
			}
		}

		//============================================================================
		//! ツール起動処理
		private void _StartUpTool()
		{
			//無効に
			IsEnabled = false;

			//アニメーション設定
			var storyboard = new Storyboard();
			storyboard.Completed += (iSender, iArgs) =>
			{
				Visibility = Visibility.Hidden;
				SceneManager.GetInstance().AdvanceScene(SceneManager.EScene.SCENE_MAIN);
			};
			var animation = new DoubleAnimation
			{
				From = ContentBorder.Opacity,
				EasingFunction = new PowerEase() { EasingMode = EasingMode.EaseInOut, Power = 2 },
				To = 0,
				Duration = TimeSpan.FromMilliseconds(300)
			};
			Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity"));
			storyboard.Children.Add(animation);
			ContentBorder.BeginStoryboard(storyboard);
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 書き出し情報
		/// </summary>
		[Serializable]
		private struct SSerialData
		{
			/// <summary>
			/// 最後に読み込んだファイルのパス
			/// </summary>
			public string mLastRefFilePath;
		};
	}
}
