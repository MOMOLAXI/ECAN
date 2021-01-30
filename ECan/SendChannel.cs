using System.Threading;
using ECAN;

namespace ECanTest
{
    //发送数据
    public class SendChannel
    {
        //发送消息缓冲区
        public CanData[] SendBuff;

        //发送消息缓冲区头指针
        public uint HeadPointer;

        //发送消息缓冲区尾指针
        public uint TailPointer;

        //Timer每次实例化后就会开始tick, 检测buff发送数据
        private Timer _send_timer;
        private AutoResetEvent _send_reset_action;
        private TimerCallback _send_execute_action;

        public bool IsProcOpen { get; set; }

        public static SendChannel Create(int capacity)
        {
            SendChannel channel = new SendChannel();
            channel._initialize(capacity);
            channel._reset_buff_pointer();
            return channel;
        }

        private void _initialize(int capacity)
        {
            SendBuff = new CanData[capacity];
            _send_reset_action = new AutoResetEvent(false);
            _send_execute_action = _send_message;
            _send_timer = new Timer(
                _send_execute_action,
                _send_reset_action,
                ECANUtility.SEND_MSG_DUE_TIME,
                ECANUtility.SEND_MSG_PERIOD);
            _reset_buff_pointer();
        }

        public void OpenChannel()
        {
            IsProcOpen = true;
        }

        public void CloseChannel()
        {
            IsProcOpen = false;
        }

        public void ShiftTail()
        {
            ++TailPointer;
        }

        public void ShiftHead()
        {
            ++HeadPointer;
        }

        private void _reset_buff_pointer()
        {
            HeadPointer = 0;
            TailPointer = 0;
        }

        private void _send_message(object state)
        {
            if (IsProcOpen)
            {
                ECANUtility.SendMessages(this);
            }
        }
    }
}
