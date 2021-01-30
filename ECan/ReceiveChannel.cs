using System.Threading;
using ECAN;

namespace ECanTest
{
    //接受数据
    public class ReceiveChannel
    {
        //接受消息缓冲区
        public CanData[] ReceiveBuff;

        //消息缓冲区头指针
        public uint HeadPointer;

        //消息缓冲区尾指针
        public uint TailPointer;

        //Timer每次实例化后就会开始tick，检测buff接受数据
        private Timer _receive_timer;

        private bool IsProcOpen { get; set; }

        //接受消息的回调
        private AutoResetEvent _receive_reset_action;
        private TimerCallback _receive_execute_action;

        public static ReceiveChannel Create(int capacity)
        {
            ReceiveChannel channel = new ReceiveChannel();
            channel.Initialize(capacity);
            channel._reset_flag();
            return channel;
        }

        /// <summary>
        /// 打开数据接收管道
        /// </summary>
        public void OpenChannel()
        {
            IsProcOpen = true;
        }

        /// <summary>
        /// 关闭数据接收管道
        /// </summary>
        public void CloseChannel()
        {
            IsProcOpen = false;
        }

        public void ShiftHead()
        {
            ++HeadPointer;
        }

        public void ShiftTail()
        {
            ++TailPointer;
        }

        internal void Initialize(int capacity)
        {
            ReceiveBuff = new CanData[capacity];
            _receive_reset_action = new AutoResetEvent(false);
            _receive_execute_action = _read_message;
            _receive_timer = new Timer(
                _receive_execute_action,
                _receive_reset_action,
                ECANUtility.REC_MSG_DUE_TIME,
                ECANUtility.REC_MSG_PERIOD);
            _reset_flag();
        }

        private void _read_message(object state)
        {
            if (IsProcOpen)
            {
                ECANUtility.ReadMessages(this);
            }
        }

        private void _reset_flag()
        {
            HeadPointer = 0;
            TailPointer = 0;
        }
    }
}
