using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// TitleContorl.xaml の相互作用ロジック
	/// </summary>
	public partial class SimpleTournamentScene : UserControl, SceneInterface
	{
		/// <summary>
		/// コンストラクタ
		/// </summary>
		public SimpleTournamentScene()
		{
			InitializeComponent();

			//ポイント作成
			_CreatePointChecker();

			//チーム振り分け
			_SettingTeamToLeaf();
		}

        /************************************************************************/
        /* 継承処理                                                             */
        /************************************************************************/

		/// <summary>
		/// 画面を覆うフィルターの見える横幅のサイズ
		/// </summary>
		/// <returns>フィルター幅</returns>
		public double GetDisplayFilterWidth() { return 0; }

		/// <summary>
		/// 他のシーンから戻ってきた際の処理
		/// </summary>
		public void SceneBack() { }

		/************************************************************************/
		/* コールバック処理                                                     */
		/************************************************************************/

		/************************************************************************/
		/* 内部処理                                                             */
		/************************************************************************/

		//============================================================================
		//! ポイント作成
		private void _CreatePointChecker()
		{
			//深度計算
			var btl_Manager = BattleOperatorManager.GetInstance();
			int join_Val = btl_Manager.IsTeamBattleFlg() ? btl_Manager.GetTeamVal() : btl_Manager.GetJoinMemberVal();
			int depth = 0;
			while (true)
			{
				int val = (int)Math.Pow(2, (int)depth);
				if (val >= join_Val)
				{
					break;
				}
				++depth;
			}

			//ポイント作成
			var top = VersusPointChecker.CreateTopPoint();
			var root = top.CreateRootPoint();
			var checker_List = new List<VersusPointChecker>();
			checker_List.Add(root);
			foreach (int iIndex in System.Linq.Enumerable.Range(0, depth))
			{
				var create_Point_List = new List<VersusPointChecker>();
				foreach (var iPoint in checker_List)
				{
					var new_Points = iPoint.CreateSourceConnectPoint();
					create_Point_List.AddRange(new_Points);
				}
				checker_List = create_Point_List;
			}

			//足りないチーム数分だけ後ろからシードとする(片側か両側かで少し分ける？)
			var root_Val = checker_List.Count;
			foreach (var iIndex in System.Linq.Enumerable.Range(0, root_Val - join_Val))
			{
				var remove_Index = 0;
				if (iIndex % 2 == 0)
				{
					remove_Index = root_Val - iIndex - 2;
				}
				else
				{
					remove_Index = iIndex;
				}

				checker_List[remove_Index].DisConnect();
			}

			//設定
			MainControl.Initialize(top);
		}

		//============================================================================
		//! チームとリーフを繋げる
		private void _SettingTeamToLeaf()
		{
			//ランダムリストを作成
			var manager = BattleOperatorManager.GetInstance();
			var join_Val = manager.IsTeamBattleFlg() ? manager.GetTeamVal() : manager.GetJoinMemberVal();
			var team_Index_List = new List<int>();
			foreach (var iIndex in System.Linq.Enumerable.Range(0 , join_Val))
			{
				team_Index_List.Add(iIndex);
			}
			var random_List = team_Index_List.ToArray().OrderBy(i => Guid.NewGuid()).ToArray();

			//設定
			var control = MainControl.GetBattleControl();
			foreach (var iIndex in System.Linq.Enumerable.Range(0, join_Val))
			{
				control.SetLeafInfoIndex(iIndex, random_List[iIndex]);
			}
			control.OpenOptionWindow();
		}
	}
}
