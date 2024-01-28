namespace GameFrameX.Web.Api
{
    /// <summary>
    /// 消息返回统一结构
    /// </summary>
    public class ResultModel
    {
        /// <summary>
        /// 消息码
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 实际数据
        /// </summary>
        public object data { get; set; }

        public static ResultModel Create(int code, string message, object data)
        {
            return new ResultModel() {code = code, message = message, data = data};
        }

        public static ResultModel Create()
        {
            return new ResultModel() {code = ResultCode.Ok, message = ResultMessage.Ok};
        }

        public static ResultModel Create(int code, string message)
        {
            return new ResultModel() {code = code, message = message};
        }

        public static ResultModel Create(object data)
        {
            return new ResultModel() {code = ResultCode.Ok, message = ResultMessage.Ok, data = data};
        }
    }
}