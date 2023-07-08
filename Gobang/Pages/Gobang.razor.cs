using Gobang.GameHub;
using Gobang.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;

namespace Gobang.Pages
{
    public partial class Gobang : IDisposable
    {
        [Inject] private NavigationManager? NavigationManager { get; set; }

        [Inject]
        private ISnackbar Snackbar { get; set; }

        [Parameter]
        public string RoomName { get; set; }

        private int[,] Chess { get; set; } = new int[19, 19];

        private string first = "He";

        private bool IsInGame = false;

        private bool IsInRoom = false;

        private GoBangRoom? Room { get; set; }

        private string msgs;

        private int MineChess = 1;

        private string? _hubUrl;
        private HubConnection? _hubConnection;

        protected override async Task OnInitializedAsync()
        {
            if (_hubConnection == null)
            {
                string baseUrl = NavigationManager!.BaseUri;

                _hubUrl = baseUrl.TrimEnd('/') + GoBangHub.HubUrl;

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .ConfigureLogging(logging => logging.AddConsole())
                    .AddNewtonsoftJsonProtocol()
                    .Build();

                _hubConnection.On<int[,]>("SynchronizeCheckerboard", SynchronizeCheckerboard);
                _hubConnection.On<string>("Alert", Alert);
                await _hubConnection.StartAsync();
            }

            await base.OnInitializedAsync();
        }

        protected override async Task OnParametersSetAsync()
        {
            if (!string.IsNullOrEmpty(RoomName))
            {
                IsInRoom = true;
                IsInGame = true;
                MineChess = 2;
                Room = new GoBangRoom() { RoomName = RoomName };
                await _hubConnection!.SendAsync("GetIntoRoom", RoomName, "");
            }
            await base.OnParametersSetAsync();
        }

        private async Task Alert(string msg)
        {
            await JS.InvokeAsync<string>("alert", msg);

            if (msg == "\n你个渣渣👎")
            {
                IsInGame = false;
                Chess = new int[19, 19];
            }
        }

        private async Task CreateRoom()
        {
            var roomname = await JS.InvokeAsync<string>("prompt", "请输入房间名称!", "房间" + Guid.NewGuid());

            if (string.IsNullOrEmpty(roomname)) return;

            IsInRoom = true;

            MineChess = 1;
            if (string.IsNullOrEmpty(roomname)) return;
            await _hubConnection!.SendAsync("CreateRoom", roomname, "");
            Room = new GoBangRoom() { RoomName = roomname };
        }

        private async Task GetIntoRoom()
        {
            var roomname = await JS.InvokeAsync<string>("prompt", "请输入房间名称!");
            if (string.IsNullOrEmpty(roomname)) return;

            IsInRoom = true;
            IsInGame = true;
            await _hubConnection!.SendAsync("GetIntoRoom", roomname, "");
            MineChess = 2;
            Room = new GoBangRoom() { RoomName = roomname };
        }

        private async Task Invite()
        {
            Snackbar.Add("复制链接成功,快去邀请你的朋友吧!🚀");
            await JS.InvokeVoidAsync("copyToClipboard", NavigationManager.BaseUri + Room!.RoomName);
        }

        private async Task StartGame()
        {
            // 初始化棋盘
            Chess = new int[19, 19];

            IsInGame = true;
            await _hubConnection!.SendAsync("Playing", Room, Chess);
        }

        private async Task Playing((int, int) value)
        {
            (int row, int cell) = value;

            var numEqual = Chess.OfType<int>().Count(x => x == 1) == Chess.OfType<int>().Count(x => x == 2);

            if (MineChess == 1)
            {
                if (!numEqual)
                {
                    Snackbar.Add("对方落子时间!🚀");
                    return;
                }
            }
            else
            {
                if (numEqual)
                {
                    Snackbar.Add("对方落子时间!🚀");
                    return;
                }
            }

            //是否开始游戏，当前判断没开始给出提示
            if (!IsInGame)
            {
                Snackbar.Add("\n💪点击开始游戏按钮开启对局，请阅读游戏规则💪");

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
                await _hubConnection!.SendAsync("Win", Room);
            }

            // 我方落子之后通知对方落子
            await _hubConnection!.SendAsync("Playing", Room, Chess);
            StateHasChanged();
        }

        private void SynchronizeCheckerboard(int[,] chess)
        {
            Chess = chess;
            InvokeAsync(StateHasChanged);
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

        public async void Dispose()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }
}