using MessagePack;

namespace SimpleRpc
{
    [MessagePackObject]
    public class RpcError
    {
        /// <summary>
        /// Gets or sets a numeric error code.
        /// </summary>
        /// <value>The code.</value>
        [Key(0)]
        public int Code { get; set; }

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The message.</value>
        [Key(1)]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets any server/method specific data provided about the error.
        /// </summary>
        /// <value>The data.</value>
        [Key(2)]
        public object Data { get; set; }
    }
}
