using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FightingCommunityAdministrator
{
	/// <summary>
	/// 対戦地点チェッククラス
	/// </summary>
	public partial class VersusPointChecker
	{
		/************************************************************************/
		/* new での生成禁止	                                                    */
		/************************************************************************/

		/// <summary>
		/// コンストラクタ
		/// </summary>
		private VersusPointChecker()
		{
			mDepth = 0;	
		}

		/************************************************************************/
		/* 基本処理                                                             */
		/************************************************************************/

		/// <summary>
		/// トップポイントを生成する
		/// </summary>
		/// <returns>トップポイントチェッカー</returns>
		public static VersusPointChecker CreateTopPoint()
		{
			var checker = new VersusPointChecker();
			checker.mPath = "Top";
			checker.mDepth = -1;
			return checker;
		}

		/// <summary>
		/// ルートポイントを生成する(トップポイントからしか生成不可能)
		/// </summary>
		/// <returns>ルートポイントチェッカー</returns>
		public VersusPointChecker CreateRootPoint()
		{
			//TOPチェッカーからのみ
			if(mDepth != -1 || mPath != "Top")
			{
				return null;
			}

			var checker = new VersusPointChecker();
			checker.mPath = "Root";
			checker.mDepth = 0;
			checker.mDestConnectionPoint = this;
			mSrcConnectionPointList[0] = checker;
			return checker;
		}

		/// <summary>
		/// このポイントに接続される新規シードを作成して返す
		/// </summary>
		/// <returns></returns>
		public VersusPointChecker[] CreateSourceConnectPoint()
		{
			mSrcConnectionPointList[0] = new VersusPointChecker();
			mSrcConnectionPointList[0].mDepth = mDepth + 1;
			mSrcConnectionPointList[0].mPath = mPath + "_Up";
			mSrcConnectionPointList[0].SetDestConnectionPoint(this);
			mSrcConnectionPointList[1] = new VersusPointChecker();
			mSrcConnectionPointList[1].mDepth = mDepth + 1;
			mSrcConnectionPointList[1].mPath = mPath + "_Down";
			mSrcConnectionPointList[1].SetDestConnectionPoint(this);
			return GetSrcConnectPoint();
		}

		/// <summary>
		/// ネットワークから除外する
		/// </summary>
		public void DisConnect()
		{
			//移動先がTOPの場合は終了
			if (mDestConnectionPoint.mPath == "Top")
			{
				return;
			}

			//接続ポイントがある場合不可能
			if (mSrcConnectionPointList[0] != null || mSrcConnectionPointList[1] != null)
			{
				return;
			}
		
			//自分の方を削除してからチェック
			if (mDestConnectionPoint.mSrcConnectionPointList[0] == this)
			{
				mDestConnectionPoint.mSrcConnectionPointList[0] = null;
			}
			else if (mDestConnectionPoint.mSrcConnectionPointList[1] == this)
			{
				mDestConnectionPoint.mSrcConnectionPointList[1] = null;
			}
			else
			{
				//ありえないけどチェック
				return;
			}

			mPath = "DeletePoint";
			mDestConnectionPoint._CheckConnection();
			mDepth = -1;
		}

		/// <summary>
		/// 入力元シードに指定されたシードが存在する時リプレース依頼のシードに付け替える
		/// </summary>
		/// <param name="iCurrentPoint"></param>
		/// <param name="iReplacePoint"></param>
		public void ReplacePoint(VersusPointChecker iCurrentPoint, VersusPointChecker iReplacePoint)
		{
			if (mSrcConnectionPointList[0] == iCurrentPoint)
			{
				mSrcConnectionPointList[0].SetDestConnectionPoint(null);
				mSrcConnectionPointList[0] = iReplacePoint;
			}
			else if (mSrcConnectionPointList[1] == iCurrentPoint)
			{
				mSrcConnectionPointList[1].SetDestConnectionPoint(null);
				mSrcConnectionPointList[1] = iReplacePoint;
			}
		}

		/// <summary>
		/// 接続ラインを作成する
		/// </summary>
		/// <param name="iLineSize">ラインのサイズ</param>
		public void CreateLine(double iLineSize, Panel iControl)
		{
			//接続先がある場合
			if (mDestConnectionPoint != null)
			{
				//可視化
				mPointConnectLineList[0].Visibility = Visibility.Visible;
				mPointConnectLineList[1].Visibility = Visibility.Visible;

				//位置設定
				var line_Half_Size = iLineSize / 2;
				var target_Pos = mDestConnectionPoint.GetPosition();

				//横棒
				mPointConnectLineList[0].X1 = mPosition.X;
				mPointConnectLineList[0].X2 = target_Pos.X;
				mPointConnectLineList[0].Y1 = mPosition.Y;
				mPointConnectLineList[0].Y2 = mPosition.Y;

				//縦棒
				mPointConnectLineList[1].X1 = target_Pos.X;
				mPointConnectLineList[1].X2 = target_Pos.X;
				mPointConnectLineList[1].Y1 = mPosition.Y;
				mPointConnectLineList[1].Y2 = target_Pos.Y;
				if (mPointConnectLineList[1].Y1 < mPointConnectLineList[1].Y2)
				{
					mPointConnectLineList[1].Y1 -= line_Half_Size;
					mPointConnectLineList[1].Y2 += line_Half_Size;
				}
				else 
				{
					mPointConnectLineList[1].Y1 += line_Half_Size;
					mPointConnectLineList[1].Y2 -= line_Half_Size;
				}

				//縦方向に存在しない場合はVisibleを切る
				if(Math.Abs(mPosition.Y - target_Pos.Y) < 5)
				{
					mPointConnectLineList[1].Visibility = Visibility.Hidden;
				}

				//登録
				foreach (var iLine in mPointConnectLineList)
				{
					iLine.Stroke = new SolidColorBrush(Colors.White);
					iLine.StrokeThickness = iLineSize;
					iControl.Children.Add(iLine);
				}
			}
			else
			{
				//不可視化
				mPointConnectLineList[0].Visibility = Visibility.Hidden;
				mPointConnectLineList[1].Visibility = Visibility.Hidden;
			}

			//接続元がある場合は行う
			if (mSrcConnectionPointList[0] != null)
			{
				mSrcConnectionPointList[0].CreateLine(iLineSize, iControl);
			}
			if (mSrcConnectionPointList[1] != null)
			{
				mSrcConnectionPointList[1].CreateLine(iLineSize, iControl);
			}
		}

		/// <summary>
		/// 更新
		/// </summary>
		public void Refresh()
		{
			//出力先が存在し無い、又は入力元シードがどちらも結合されている場合は何もし無い
			if (mDestConnectionPoint == null || (mSrcConnectionPointList[0] != null && mSrcConnectionPointList[1] != null))
			{
				return;
			}

			//入力元どちらのシードも無い場合は何もしない
			var dest_Point = mDestConnectionPoint;
			if (mSrcConnectionPointList[0] == null && mSrcConnectionPointList[1] == null)
			{
				return;
			}

			var valid_Point = (mSrcConnectionPointList[0] != null) ? mSrcConnectionPointList[0] : mSrcConnectionPointList[1];
			valid_Point.SetDestConnectionPoint(mDestConnectionPoint);
			mDestConnectionPoint.ReplacePoint(this, valid_Point);

			//出力先シードの更新
			dest_Point.Refresh();
		}

		/************************************************************************/
		/* アクセサ                                                             */
		/************************************************************************/

		/// <summary>
		/// 出力先シードを設定
		/// </summary>
		/// <param name="iSrcPoint">出力先シード</param>
		public void SetDestConnectionPoint(VersusPointChecker iSrcPoint)
		{
			mDestConnectionPoint = iSrcPoint;
		}

		/// <summary>
		/// 出力先シードの取得
		/// </summary>
		/// <returns>出力先シード</returns>
		public VersusPointChecker GetDestConnectPoint()
		{
			return mDestConnectionPoint;
		}

		/// <summary>
		/// 入力元シードリスト(2個)の取得
		/// </summary>
		/// <returns>入力元シードリスト</returns>
		public VersusPointChecker[] GetSrcConnectPoint()
		{
			return mSrcConnectionPointList;
		}

		/// <summary>
		/// 仮想位置の設定
		/// </summary>
		/// <param name="iPosition">仮想位置</param>
		public void SetPosition(Point iPosition)
		{
			mPosition = iPosition;
		}

		/// <summary>
		/// 仮想位置の取得
		/// </summary>
		/// <returns>仮想位置</returns>
		public Point GetPosition()
		{
			return mPosition;
		}

		/// <summary>
		/// 深度の取得
		/// </summary>
		/// <returns>深度</returns>
		public int GetDepth()
		{
			return mDepth;
		}

		/// <summary>
		/// 自分をルートとした場合の最大深度を取得する
		/// </summary>
		/// <returns>深度</returns>
		public int GetMaxDepth()
		{
			//トップの場合はルートに合わせる
			if (mPath == "Top")
			{
				return mSrcConnectionPointList[0].GetMaxDepth();
			}

			//ループ
			int ret_Val = 0;
			var seed_List = new List<VersusPointChecker>();
			seed_List.Add(this);
			while (seed_List.Count != 0)
			{
				var get_List = new List<VersusPointChecker>();
				foreach (var iPoint in seed_List)
				{
					int depth = iPoint.GetDepth();
					if (ret_Val < depth)
					{
						ret_Val = depth;
					}
					
					var src_Point = iPoint.GetSrcConnectPoint();
					if (src_Point[0] != null)
					{
						get_List.Add(src_Point[0]);
					}
					if (src_Point[1] != null)
					{
						get_List.Add(src_Point[1]);
					}
				}
				seed_List = get_List;
			}

			return ret_Val - mDepth;
		}

		/// <summary>
		/// 自分をルートとした場合の全てのリーフポイントを取得する
		/// </summary>
		/// <returns></returns>
		public List<VersusPointChecker> GetLeafPointList()
		{
			var ret_List = new List<VersusPointChecker>();
			_GetLeafPoint(ref ret_List, this);
			return ret_List;
		}

		/// <summary>
		/// 自分をルートとしたリーフポイントを含まないポイントの数
		/// </summary>
		/// <returns></returns>
		public int GetInnerPointVal()
		{
			//トップは無視
			if (mPath == "Top")
			{
				return -1;
			}

			var ret_Val = 0;
			_GetCountingInnerPoint(ref ret_Val, this);
			return ret_Val;
		}

		/// <summary>
		/// ラインカラー設定
		/// </summary>
		/// <param name="iColor">色ブラシ</param>
		/// <param name="iNextChangeFlg">次のポイントの最初だけ塗りつぶす</param>
		public void SetLineColor(Brush iColor, bool iNextChangeFlg)
		{
			mPointConnectLineList[0].Stroke = iColor;
			mPointConnectLineList[1].Stroke = iColor;

			if (iNextChangeFlg && mDestConnectionPoint != null)
			{
				mDestConnectionPoint.mPointConnectLineList[0].Stroke = iColor;
			}
		}

		/// <summary>
		/// ラインのZ位置を変更
		/// </summary>
		/// <param name="iIndex"></param>
		public void SetZIndex(int iIndex)
		{
			Panel.SetZIndex(mPointConnectLineList[0], iIndex);
			Panel.SetZIndex(mPointConnectLineList[1], iIndex);
		}

		/************************************************************************/
		/* 内部定義                                                             */
		/************************************************************************/

		//============================================================================
		//! コネクションチェック
		private void _CheckConnection()
		{
			//接続先が無ければ無視
			if (mDestConnectionPoint == null)
			{
				return;
			}
		
			//接続元が1つの時は接続先と直接コネクションさせる
			var connection_Point = mSrcConnectionPointList[0];
			if (connection_Point == null)
			{
				if (mSrcConnectionPointList[1] == null)
				{
					//両方無し
					return;
				}
				connection_Point = mSrcConnectionPointList[1];
			}
			else if (mSrcConnectionPointList[1] != null)
			{
				//両方あり
				return;
			}

			//接続元を変更
			if (mDestConnectionPoint.mSrcConnectionPointList[0] == this)
			{
				mDestConnectionPoint.mSrcConnectionPointList[0] = connection_Point;
				connection_Point.mDestConnectionPoint = mDestConnectionPoint;
			}
			else if (mDestConnectionPoint.mSrcConnectionPointList[1] == this)
			{
				mDestConnectionPoint.mSrcConnectionPointList[1] = connection_Point;
				connection_Point.mDestConnectionPoint = mDestConnectionPoint;
			}
		}

		//============================================================================
		//! 指定ポイントチェッカーがリーフの場合はリストに追加、それ以外は派生
		private void _GetLeafPoint(ref List<VersusPointChecker> oLeafList, VersusPointChecker iPointChecker)
		{
			var src_Point = iPointChecker.GetSrcConnectPoint();

			//どちらとも繋がりが無ければリーフと見なす
			if (src_Point[0] == null && src_Point[1] == null)
			{
				oLeafList.Add(iPointChecker);
			}
			else
			{
				if (src_Point[0] != null)
				{
					_GetLeafPoint(ref oLeafList, src_Point[0]);
				}
				if (src_Point[1] != null)
				{
					_GetLeafPoint(ref oLeafList, src_Point[1]);
				}
			}
		}

		//============================================================================
		//! 内部ポイントの場合は加算して次へ渡す
		private void _GetCountingInnerPoint(ref int oCount, VersusPointChecker iPointChecker)
		{
			var src_Point = iPointChecker.GetSrcConnectPoint();

			//どちらとも繋がりが無ければリーフと見なす
			if (src_Point[0] != null || src_Point[1] != null)
			{
				if (src_Point[0] != null)
				{
					_GetCountingInnerPoint(ref oCount, src_Point[0]);
				}
				if (src_Point[1] != null)
				{
					_GetCountingInnerPoint(ref oCount, src_Point[1]);
				}
				++oCount;
			}
		}

		/************************************************************************/
		/* 変数定義                                                             */
		/************************************************************************/

		/// <summary>
		/// 確認用パス
		/// </summary>
		private string mPath = "";

		/// <summary>
		/// 深度
		/// </summary>
		private int mDepth;

		/// <summary>
		/// シードの仮想位置
		/// </summary>
		private Point mPosition;

		/// <summary>
		/// 入力元ポイント
		/// </summary>
		private VersusPointChecker[] mSrcConnectionPointList = new VersusPointChecker[2];

		/// <summary>
		/// 出力先ポイント
		/// </summary>
		private VersusPointChecker mDestConnectionPoint;

		/// <summary>
		/// シード接続ラインリスト
		/// </summary>
		private Line[] mPointConnectLineList = new Line[2] { new Line(), new Line() };
	};
}
