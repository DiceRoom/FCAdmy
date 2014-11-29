using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// 各シーンのマネージャ
	/// </summary>
	public partial class SceneManager
	{
		/// <summary>
		/// シーン番号
		/// </summary>
		public enum EScene
		{
			SCENE_MAIN,						//< メイン画面
			SCENE_MEMBER,					//< メンバー画面
			SCENE_MEMBER_DETAILS,			//< メンバー詳細画面
			SCENE_BATTLE_HISTORY,			//< 大会履歴画面
			SCENE_BATTLE_TOURNAMENT_SELECT,	//< 大会画面
			SCENE_BATTLE_ROUND_ROBIN,		//< ラウンドロビントーナメント画面
			SCENE_BATTLE_SIMPLE_TOURNAMENT,	//< シンプルトーナメント画面
			SCENE_BATTLE_DOUBLE_ELM,		//< ダブルイリミネーション画面
			SCENE_RESULT_TOURNAMENT,		//< トーナメント形式のリザルト画面
			SCENE_INTRASQUAD_GAME,			//< 紅白戦画面
			SCENE_SYSTEM_SETTINGS,			//< システム設定画面
			SCENE_MEMBER_SELECT,			//< 参加者選択画面
			SCENE_TEAM_SELECT,				//< チーム選択画面

			//シーン最大数
			SCENE_VAL,
		};
	}
}
