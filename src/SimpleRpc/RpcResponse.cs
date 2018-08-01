using MessagePack;

namespace SimpleRpc
{
    [MessagePackObject]
    public class RpcResponse
    {
        /// <summary>
        /// Gets or sets the result of the method call. Null if an error occurred, or if the method does not return a value.
        /// </summary>
        /// <value>The result.</value>
        [Key(0)]
        public object Result { get; set; }

        /// <summary>
        /// Gets or sets the error that occurred. Null if no error occurred.
        /// </summary>
        /// <value>The error.</value>
        [Key(1)]
        public RpcError Error { get; set; }
    }
}
