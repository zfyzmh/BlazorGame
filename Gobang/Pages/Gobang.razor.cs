using Microsoft.JSInterop;

namespace Gobang.Pages
{
    public partial class Gobang
    {
        private int[,] Chess = new int[19, 19];

        private string first = "ai";

        private bool IsInGame = false;

        private string? msgs;

        private int AIChess = 1;

        private int MineChess = 2;

        private void StartGame()
        {
            // 初始化棋盘
            Chess = new int[19, 19];

            // 是否开始游戏，点击按钮重置显示消息
            if (IsInGame)
            {
                msgs = string.Empty;
            }
            else
            {
                // 电脑先手
                if (first == "ai")
                {
                    AIChess = 1;
                    MineChess = 2;

                    //电脑落子正中心天元位置
                    Chess[9, 9] = AIChess;

                    msgs = "电脑：执黑子 ⚫ 我：执白子 ⚪";
                }
                else
                {
                    // 我先手的话则我执黑子，电脑执白子
                    MineChess = 1;
                    AIChess = 2;

                    msgs = "我：执黑子 ⚫ 电脑：执白子 ⚪";
                }
            }

            // 改变游戏状态，用于显示不同文字的按钮
            IsInGame = !IsInGame;
        }

        private async Task Playing(int row, int cell)
        {
            // 是否开始游戏，当前判断没开始给出提示
            if (!IsInGame)
            {
                await JS.InvokeAsync<Task>("alert", "\n💪点击开始游戏按钮开启对局，请阅读游戏规则💪");
                return;
            }

            // 已落子直接返回，不做任何操作
            if (Chess[row, cell] != 0)
                return;

            // 根据传进来的坐标进行我方落子
            Chess[row, cell] = MineChess;

            if (IsWin(MineChess, row, cell))
            {
                await JS.InvokeAsync<Task>("alert", "\n恭喜，你赢了👍");

                IsInGame = !IsInGame;
                return;
            }

            // 我方落子之后电脑落子
            await AIPlaying(AIChess);
        }

        private async Task AIPlaying(int chess)
        {
            // 我方
            var minePoints = new List<ValuedPoint>();
            // 电脑
            var aiPonints = new List<ValuedPoint>();

            for (int i = 0; i < 19; i++)
            {
                for (int j = 0; j < 19; j++)
                {
                    // 还未落子的位置列表
                    if (Chess[i, j] == 0)
                    {
                        minePoints.Add(GetValuedPoint(chess, i, j));

                        aiPonints.Add(GetValuedPoint((chess == 1 ? 2 : 1), i, j));
                    }
                }
            }

            // 获取最佳位置
            var minePoint = minePoints.OrderByDescending(x => x.Score).FirstOrDefault();
            var aiPonint = aiPonints.OrderByDescending(x => x.Score).FirstOrDefault();

            if (minePoint != null && aiPonint != null)
            {
                // 如果某个位置对手分数高于我方，则抢占位置
                if (minePoint.Score > aiPonint.Score)
                {
                    Chess[minePoint.Point.Row, minePoint.Point.Cell] = chess;

                    if (IsWin(AIChess, minePoint.Point.Row, minePoint.Point.Cell))
                    {
                        await JS.InvokeAsync<Task>("alert", "\n电脑赢了，你个渣渣👎");
                        IsInGame = !IsInGame;
                        return;
                    }
                }
                else
                {
                    Chess[aiPonint.Point.Row, aiPonint.Point.Cell] = chess;

                    if (IsWin(AIChess, aiPonint.Point.Row, aiPonint.Point.Cell))
                    {
                        await JS.InvokeAsync<Task>("alert", "\n电脑赢了，你个渣渣👎");
                        IsInGame = !IsInGame;
                        return;
                    }
                }
            }
        }

        private ValuedPoint GetValuedPoint(int chess, int row, int cell)
        {
            var aiChess = chess == 1 ? 2 : 1;

            int HScore = 0, VScore = 0, PScore = 0, LScore = 0;

            #region 横方向 ➡⬅

            {
                var i = 1;
                var score = 1;
                var validPlace = 0;
                var rightValid = true;
                var leftValid = true;
                var rightSpace = 0;
                var leftSpace = 0;
                var isDead = false;

                while (i < 5)
                {
                    var right = cell + i;
                    if (rightValid && right < 19)
                    {
                        if (Chess[row, right] == chess)
                        {
                            if (rightSpace == 0)
                                score++;
                            validPlace++;
                        }
                        else if (Chess[row, right] == 0)
                        {
                            rightSpace++;
                            validPlace++;
                        }
                        else if (Chess[row, right] == aiChess)
                        {
                            rightValid = false;
                            if (rightSpace == 0)
                                isDead = true;
                        }
                    }

                    var left = cell - i;
                    if (leftValid && left >= 0)
                    {
                        if (Chess[row, left] == chess)
                        {
                            if (leftSpace == 0)
                                score++;
                            validPlace++;
                        }
                        else if (Chess[row, left] == 0)
                        {
                            leftSpace++;
                            validPlace++;
                        }
                        else if (Chess[row, left] == aiChess)
                        {
                            leftValid = false;
                            if (leftSpace == 0)
                                isDead = true;
                        }
                    }

                    i++;
                }

                if (score >= 5)
                    HScore = 100000;

                if (score == 4)
                {
                    if (!isDead)
                        HScore = 80000;
                    else
                        HScore = validPlace <= 4 ? 0 : 8000;
                }

                if (score == 3)
                {
                    if (!isDead)
                        HScore = validPlace <= 4 ? 0 : 4000;
                    else
                        HScore = validPlace <= 4 ? 0 : 2000;
                }

                if (score == 2)
                {
                    if (!isDead)
                        HScore = validPlace <= 4 ? 0 : 600;
                    else
                        HScore = validPlace <= 4 ? 0 : 300;
                }
            }

            #endregion 横方向 ➡⬅

            #region 竖方向 ⬇⬆

            {
                var i = 1;
                var score = 1;
                var validPlace = 0;
                var topValid = true;
                var bottomValid = true;
                var topSpace = 0;
                var bottomSpace = 0;
                var isDead = false;

                while (i < 5)
                {
                    var top = row - i;
                    if (topValid && top >= 0)
                    {
                        if (Chess[top, cell] == chess)
                        {
                            if (topSpace == 0)
                                score++;
                            validPlace++;
                        }
                        else if (Chess[top, cell] == 0)
                        {
                            topSpace++;
                            validPlace++;
                        }
                        else if (Chess[top, cell] == aiChess)
                        {
                            topValid = false;
                            if (topSpace == 0)
                                isDead = true;
                        }
                    }

                    var bottom = row + i;
                    if (bottomValid && bottom < 19)
                    {
                        if (Chess[bottom, cell] == chess)
                        {
                            if (bottomSpace == 0)
                                score++;
                            validPlace++;
                        }
                        else if (Chess[bottom, cell] == 0)
                        {
                            bottomSpace++;
                            validPlace++;
                        }
                        else if (Chess[bottom, cell] == aiChess)
                        {
                            bottomValid = false;
                            if (bottomSpace == 0)
                                isDead = true;
                        }
                    }

                    i++;
                }

                if (score >= 5)
                    VScore = 100000;

                if (score == 4)
                {
                    if (!isDead)
                        VScore = 80000;
                    else
                        VScore = validPlace <= 4 ? 0 : 8000;
                }
                if (score == 3)
                {
                    if (!isDead)
                        VScore = validPlace <= 4 ? 0 : 4000;
                    else
                        VScore = validPlace <= 4 ? 0 : 2000;
                }
                if (score == 2)
                {
                    if (!isDead)
                        VScore = validPlace <= 4 ? 0 : 600;
                    else
                        VScore = validPlace <= 4 ? 0 : 300;
                }
            }

            #endregion 竖方向 ⬇⬆

            #region 撇方向 ↙↗

            {
                var i = 1;
                var score = 1;
                var validPlace = 0;
                var topValid = true;
                var bottomValid = true;
                var topSpace = 0;
                var bottomSpace = 0;
                var isDead = false;

                while (i < 5)
                {
                    var rightTopRow = row - i;
                    var rightTopCell = cell + i;
                    if (topValid && rightTopRow >= 0 && rightTopCell < 19)
                    {
                        if (Chess[rightTopRow, rightTopCell] == chess)
                        {
                            if (topSpace == 0)
                                score++;
                            validPlace++;
                        }
                        else if (Chess[rightTopRow, rightTopCell] == 0)
                        {
                            topSpace++;
                            validPlace++;
                        }
                        else if (Chess[rightTopRow, rightTopCell] == aiChess)
                        {
                            topValid = false;
                            if (topSpace == 0)
                                isDead = true;
                        }
                    }

                    var leftBottomRow = row + i;
                    var leftBottomCell = cell - i;
                    if (bottomValid && leftBottomRow < 19 && leftBottomCell >= 0)
                    {
                        if (Chess[leftBottomRow, leftBottomCell] == chess)
                        {
                            if (bottomSpace == 0)
                                score++;
                            validPlace++;
                        }
                        else if (Chess[leftBottomRow, leftBottomCell] == 0)
                        {
                            bottomSpace++;
                            validPlace++;
                        }
                        else if (Chess[leftBottomRow, leftBottomCell] == aiChess)
                        {
                            bottomValid = false;
                            if (bottomSpace == 0)
                                isDead = true;
                        }
                    }

                    i++;
                }

                if (score >= 5)
                    PScore = 100000;

                if (score == 4)
                {
                    if (!isDead)
                        PScore = 80000;
                    else
                        PScore = validPlace <= 4 ? 0 : 9000;
                }
                if (score == 3)
                {
                    if (!isDead)
                        PScore = validPlace <= 4 ? 0 : 4500;
                    else
                        PScore = validPlace <= 4 ? 0 : 3000;
                }
                if (score == 2)
                {
                    if (!isDead)
                        PScore = validPlace <= 4 ? 0 : 800;
                    else
                        PScore = validPlace <= 4 ? 0 : 500;
                }
            }

            #endregion 撇方向 ↙↗

            #region 捺方向 ↘↖

            {
                var i = 1;
                var score = 1;
                var validPlace = 0;
                var topSpace = 0;
                var bottomSpace = 0;
                var topValid = true;
                var bottomValid = true;
                var isDead = false;

                while (i < 5)
                {
                    var leftTopRow = row - i;
                    var leftTopCell = cell - i;
                    if (topValid && leftTopRow >= 0 && leftTopCell >= 0)
                    {
                        if (Chess[leftTopRow, leftTopCell] == chess)
                        {
                            if (topSpace == 0)
                                score++;
                            validPlace++;
                        }
                        else if (Chess[leftTopRow, leftTopCell] == 0)
                        {
                            topSpace++;
                            validPlace++;
                        }
                        else if (Chess[leftTopRow, leftTopCell] == aiChess)
                        {
                            topValid = false;
                            if (topSpace == 0)
                                isDead = true;
                        }
                    }

                    var rightBottomRow = row + i;
                    var rightBottomCell = cell + i;
                    if (bottomValid && rightBottomRow < 19 && rightBottomCell < 19)
                    {
                        if (Chess[rightBottomRow, rightBottomCell] == chess)
                        {
                            if (bottomSpace == 0)
                                score++;
                            validPlace++;
                        }
                        else if (Chess[rightBottomRow, rightBottomCell] == 0)
                        {
                            bottomSpace++;
                            validPlace++;
                        }
                        else if (Chess[rightBottomRow, rightBottomCell] == aiChess)
                        {
                            bottomValid = false;
                            if (bottomSpace == 0)
                                isDead = true;
                        }
                    }

                    i++;
                }

                if (score >= 5)
                    LScore = 100000;

                if (score == 4)
                {
                    if (!isDead)
                        LScore = 80000;
                    else
                        LScore = validPlace <= 4 ? 0 : 9000;
                }

                if (score == 3)
                {
                    if (!isDead)
                        LScore = validPlace <= 4 ? 0 : 4500;
                    else
                        LScore = validPlace <= 4 ? 0 : 3000;
                }

                if (score == 2)
                {
                    if (!isDead)
                        LScore = validPlace <= 4 ? 0 : 800;
                    else
                        LScore = validPlace <= 4 ? 0 : 500;
                }
            }

            #endregion 捺方向 ↘↖

            return new ValuedPoint
            {
                Score = HScore + VScore + PScore + LScore,
                Point = new Point
                {
                    Row = row,
                    Cell = cell
                }
            };
        }

        private bool IsWin(int chess, int row, int cell)
        {
            #region 横方向 ➡⬅

            {
                var i = 1;
                var score = 1;
                var rightValid = true;
                var leftValid = true;

                while (i <= 5)
                {
                    var right = cell + i;
                    if (rightValid && right < 19)
                    {
                        if (Chess[row, right] == chess)
                        {
                            score++;
                            if (score >= 5)
                                return true;
                        }
                        else
                            rightValid = false;
                    }

                    var left = cell - i;
                    if (leftValid && left >= 0)
                    {
                        if (Chess[row, left] == chess)
                        {
                            score++;
                            if (score >= 5)
                                return true;
                        }
                        else
                            leftValid = false;
                    }

                    i++;
                }
            }

            #endregion 横方向 ➡⬅

            #region 竖方向 ⬇⬆

            {
                var i = 1;
                var score = 1;
                var topValid = true;
                var bottomValid = true;

                while (i < 5)
                {
                    var top = row - i;
                    if (topValid && top >= 0)
                    {
                        if (Chess[top, cell] == chess)
                        {
                            score++;
                            if (score >= 5)
                                return true;
                        }
                        else
                            topValid = false;
                    }

                    var bottom = row + i;
                    if (bottomValid && bottom < 19)
                    {
                        if (Chess[bottom, cell] == chess)
                        {
                            score++;
                            if (score >= 5)
                                return true;
                        }
                        else
                        {
                            bottomValid = false;
                        }
                    }

                    i++;
                }
            }

            #endregion 竖方向 ⬇⬆

            #region 撇方向 ↙↗

            {
                var i = 1;
                var score = 1;
                var topValid = true;
                var bottomValid = true;

                while (i < 5)
                {
                    var rightTopRow = row - i;
                    var rightTopCell = cell + i;
                    if (topValid && rightTopRow >= 0 && rightTopCell < 19)
                    {
                        if (Chess[rightTopRow, rightTopCell] == chess)
                        {
                            score++;
                            if (score >= 5)
                                return true;
                        }
                        else
                            topValid = false;
                    }

                    var leftBottomRow = row + i;
                    var leftBottomCell = cell - i;
                    if (bottomValid && leftBottomRow < 19 && leftBottomCell >= 0)
                    {
                        if (Chess[leftBottomRow, leftBottomCell] == chess)
                        {
                            score++;
                            if (score >= 5)
                                return true;
                        }
                        else
                            bottomValid = false;
                    }

                    i++;
                }
            }

            #endregion 撇方向 ↙↗

            #region 捺方向 ↘↖

            {
                var i = 1;
                var score = 1;
                var topValid = true;
                var bottomValid = true;

                while (i < 5)
                {
                    var leftTopRow = row - i;
                    var leftTopCell = cell - i;
                    if (topValid && leftTopRow >= 0 && leftTopCell >= 0)
                    {
                        if (Chess[leftTopRow, leftTopCell] == chess)
                        {
                            score++;
                            if (score >= 5)
                                return true;
                        }
                        else
                            topValid = false;
                    }

                    var rightBottomRow = row + i;
                    var rightBottomCell = cell + i;
                    if (bottomValid && rightBottomRow < 19 && rightBottomCell < 19)
                    {
                        if (Chess[rightBottomRow, rightBottomCell] == chess)
                        {
                            score++;
                            if (score >= 5)
                                return true;
                        }
                        else
                            bottomValid = false;
                    }

                    i++;
                }
            }

            #endregion 捺方向 ↘↖

            return false;
        }
    }
}