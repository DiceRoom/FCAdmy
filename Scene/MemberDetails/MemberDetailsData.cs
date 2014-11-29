using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// メンバー情報保持データ
	/// </summary>
	public class MemberDetailsData
	{
		/// <summary>
		/// アイコンイメージ
		/// </summary>
		public ImageSource Icon { get; set; }

		/// <summary>
		/// メンバーID
		/// </summary>
		public int ID { get; set; }

		/// <summary>
		/// メンバーIDの文字列型
		/// </summary>
		public string IDString { get; set; }

		/// <summary>
		/// メンバー名
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// デフォルトキャラID
		/// </summary>
		public int DefaultCharaID { get; set; }

		/// <summary>
		/// 登録日時
		/// </summary>
		public string RegistaDate { get; set; }

		/// <summary>
		/// 大会参加回数
		/// </summary>
		public int JoinTournamentVal { get; set; }

		/// <summary>
		/// 最後に大会に出た日時
		/// </summary>
		public string LastJoinTournamentDate { get; set; }
	};

	/// <summary>
	/// トータルの大会データ
	/// </summary>
	public class TotalBattleData
	{
		/// <summary>
		/// アイコンイメージ
		/// </summary>
		public ImageSource Icon { get; set; }

		/// <summary>
		/// 使用キャラクター番号
		/// </summary>
		public int UseCharacterID { get; set; }

		/// <summary>
		/// 大会番号
		/// </summary>
		public int BattleID { get; set; }

		/// <summary>
		/// 大会名
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 開催日時
		/// </summary>
		public string BattleDate { get; set; }

		/// <summary>
		/// 大会種別
		/// </summary>
		public BattleManager.EBattleKind BattleKind { get; set; }
	};
}
