namespace Gobang.Model
{
    public class GoBangRoom
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// 房间名
        /// </summary>
        public string RoomName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 棋盘备份,用于中途断线重连或后期防作弊
        /// </summary>
        public int[,] Chess { get; set; } = new int[19, 19];
    }
}