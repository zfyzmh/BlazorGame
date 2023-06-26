using Gobang.Model;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace Gobang.GameHub
{
    public class GoBangHub : Hub
    {
        public const string HubUrl = "/gobang";

        private readonly List<GoBangRoom> goBangRooms = new();

        public override Task OnConnectedAsync()
        {
            Console.WriteLine($"{Context.ConnectionId} connected");
            return base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Disconnected {exception?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// 创建房间(即创建群组)
        /// </summary>
        /// <param name="roomName">房间名(群组名)</param>
        /// <param name="password">密码(可选)</param>
        /// <returns></returns>
        public async Task CreateRoom(string roomName, string? password = null)
        {
            goBangRooms.Add(new GoBangRoom() { Guid = Guid.NewGuid(), RoomName = roomName, Password = password });

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        /// <summary>
        /// 加入房间(群组)
        /// </summary>
        /// <param name="roomName">房间名(群组名)</param>
        /// <param name="password">密码(可选)</param>
        /// <returns></returns>
        public async Task GetIntoRoom(string roomName, string? password = null)
        {
            var room = goBangRooms.FirstOrDefault(m => m.RoomName == roomName);

            if (room == null)
            {
                await Clients.Caller.SendAsync("Alert", "未找到该房间!");
                return;
            }
            else
            {
                if (!string.IsNullOrEmpty(room.Password) && room.Password != password)
                {
                    await Clients.Caller.SendAsync("Alert", "房间密码错误!");
                    return;
                }
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        /// <summary>
        /// 落子
        /// </summary>
        /// <param name="room">房间</param>
        /// <param name="Chess">棋盘</param>
        /// <param name="row">行</param>
        /// <param name="cell">列</param>
        /// <param name="blackOrWhite">1为黑子,2为白子,黑子先行</param>
        /// <returns></returns>
        public async Task Playing(GoBangRoom room, int[,] Chess, int row, int cell, int blackOrWhite)
        {
            goBangRooms.First(m => m.Guid == room.Guid).Chess = Chess;
            await Clients.OthersInGroup(room.RoomName).SendAsync("Playing", row, cell, blackOrWhite);
        }

        public async Task Win(GoBangRoom room)
        {
            await Clients.OthersInGroup(room.RoomName).SendAsync("Alert", "\n你个渣渣👎");
        }
    }
}