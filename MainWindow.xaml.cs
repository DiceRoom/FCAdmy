using System;
using System.Collections.Generic;
using System.Windows;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// MainWindow.xaml の相互作用ロジック
	/// </summary>
	public partial class MainWindow : Window
	{
		/************************************************************************/
		/* 基本処理	                                                            */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		public MainWindow()
		{
            //ライセンスチェック
#if DL_DEBUG
            SystemManager.GetInstance().SetUserName("デバッグ起動");
#else
            //ライセンスチェック
            var path = SystemUtility.GetSystemPath() + "FCAdmy.lic";
            try
            {
                var user_Name = SystemUtility.GetLicenseUserName(path);
                SystemManager.GetInstance().SetUserName(user_Name);
            }
            catch (System.Exception)
            {
                Environment.Exit(1);
            }
#endif

			InitializeComponent();
			_InitializeSystem();
            SystemManager.GetInstance().SetScreenShotGrid(ScreenShotGrid);
		}

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		//============================================================================
		//! ロード時の処理
		private void _Loaded(object iSender, RoutedEventArgs iArgs)
		{
			//シーンの初期化
			var scene_Manager = SceneManager.GetInstance();
			scene_Manager.SetControls(ContentControl, LeftFilterImage, RightFilterImage);
			LeftFilterImage.Margin = RightFilterImage.Margin = new Thickness();

			//ロードコンテンツを設定する
#if DL_DEBUG
            //ライセンス作成
            if (true)
            {
                SystemUtility.CreateLicenseFiles(1);
                return;
            }
			bool test = true;
            if(test)
            {
                //データの設定
                DataManager.GetInstance().SetData(SaveManager.GetInstance().PresetInfoKey, "BLAZBLUE CHRONOPHANTASMA");

                //プロジェクトデータ設定
                SaveManager.GetInstance().ProjectFilePath = @"C:\ＦＣあどみぃ！\ＦＣあどみぃ！\System\デバッグプロジェクト.pjb";
                DataManager.GetInstance().ReadData(SaveManager.GetInstance().ProjectFilePath);

                //必要なマネージャの初期化
                MemberManager.GetInstance().LoadMemberData();
                BattleManager.GetInstance().LoadBattleData();
                PresetManager.GetInstance().LoadPreset();
                SystemManager.GetInstance().LoadSystemData();

                //大会設定
                var btl_Op_Manager = BattleOperatorManager.GetInstance();
                btl_Op_Manager.BeginBattle(BattleManager.EBattleKind.BATTLE_SIMPLE_TOURNAMENT);
                btl_Op_Manager.SetBattleName("デバッグ大会！");
                btl_Op_Manager.SetBattleObject(new SimpleTournamentObject());

                //メンバー設定
				//scene_Manager.AdvanceScene(SceneManager.EScene.SCENE_MEMBER_SELECT);
				foreach (var iIndex in System.Linq.Enumerable.Range(0, 32))
                {
                    btl_Op_Manager.AddMember(iIndex);
                }
                btl_Op_Manager.SetLockJoinMember(true);

                //チーム設定
				var team_Val = 16;
                if (BattleOperatorManager.GetInstance().GetJoinMemberVal() % team_Val != 0)
                {
                    throw new System.Exception();
                }
                var in_Member_Val = BattleOperatorManager.GetInstance().GetJoinMemberVal() / team_Val;
                foreach (var iIndex in System.Linq.Enumerable.Range(0, team_Val))
                {
                    var info = new BattleManager.SBattleTeamInfo();
                    info.mTeamID = iIndex;
                    info.mTeamName = string.Format("Team{0}", iIndex);
                    info.mJoinIDList = new List<int>();
                    foreach (var iMemberIndex in System.Linq.Enumerable.Range(0, in_Member_Val))
                    {
                        info.mJoinIDList.Add((iIndex * in_Member_Val) + iMemberIndex);
                    }
                    btl_Op_Manager.AddTeam(info);
                }
                btl_Op_Manager.SetLockTeam(true);



				//scene_Manager.AdvanceScene(SceneManager.EScene.SCENE_TEAM_SELECT);
				scene_Manager.AdvanceScene(SceneManager.EScene.SCENE_BATTLE_SIMPLE_TOURNAMENT);

                /*紅白戦
                var team = (TeamSelectScene)scene_Manager.AdvanceScene(SceneManager.EScene.SCENE_TEAM_SELECT);
                team.AddTeam("紅組");
                team.AddTeam("白組");
                team.SetTeamOperateFlg(false);*/

                //scene_Manager.AdvanceScene(SceneManager.EScene.SCENE_MEMBER_SELECT);
            }
            else
#endif
            {
                var load_Scene = new LoadScene();
                LoadContent.Content = load_Scene;
            }
		}

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! システム情報の初期化
		private void _InitializeSystem()
		{
			//ラムダ式で解決
			SourceInitialized += (iSender, iArgs) =>
			{
				//ウィンドウ位置の読み込み
				var get_obj = DataManager.GetInstance().GetData("MainWindow");
				if (get_obj != null)
				{
					var point = (Point)get_obj;
					Left = point.X;
					Top = point.Y;
				}
			};

            //閉じる直前
            Closing += (iSender, iArgs) =>
            {
				//閉じていいかチェック
				if (!SystemManager.GetInstance().IsWindowCheckClose())
				{
					iArgs.Cancel = true;
					return;
				}

                DataManager.GetInstance().SetData("MainWindow", new Point(Left, Top));
            };

			//キー入力があった
			KeyDown += (iSender, iArgs) =>
			{
				SystemManager.GetInstance().KeyInput(iArgs);
			};

			//閉じた後
            Closed += (iSender, iArgs) =>
            {
                //コンテンツ情報を保存して終了
                SaveManager.GetInstance().SaveContentData();
            };

			//コンテンツデータがある場合は読み込み
			string content_Path = SaveManager.GetInstance().ContentFilePath;
			if (System.IO.File.Exists(content_Path))
			{
				DataManager.GetInstance().ReadData(content_Path);
			}
		}
	}
}
