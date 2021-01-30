using ECAN;

namespace ECanTest
{
    
    //ECAN工具
    public static class ECANUtility
    {
        //======================================= 配置信息====================================//
        //设备信息
        public const int DEVICE_TYPE = 1;
        public const int DEVICE_IND = 0;
        public const int CAN_IND = 0;
        public const int RESERVED = 0;

        //每次读取消息数量最大值
        public const int READ_MSG_MAX_COUNT = 500;

        //每次发送消息数量的最大值
        public const int SEND_MSG_MAX_COUNT = 200;

        //接受消息的缓冲大小
        public const int REC_MSG_BUF_MAX = 0x2710;

        //发送消息的缓冲大小
        public const int SEND_MSG_BUF_MAX = 0x2710;

        public const int REC_MSG_DUE_TIME = 0;
        public const int REC_MSG_PERIOD = 20;

        public const int SEND_MSG_DUE_TIME = 0;

        public const int SEND_MSG_PERIOD = 20;

        //======================================= 配置信息====================================//


        public static void ReadMessages(ReceiveChannel channel)
        {
            for (int message_count = 0; message_count < READ_MSG_MAX_COUNT; ++message_count)
            {
                uint data_length = 1;
                E_CAN_STATUS read_status =
                    ECANDLL.Receive(DEVICE_TYPE, DEVICE_IND, CAN_IND, out CanData data, data_length, 1);
                bool read_success = read_status == E_CAN_STATUS.STATUS_OK & (data_length > 0);
                if (!read_success)
                {
                    break;
                }

                channel.ReceiveBuff[channel.HeadPointer].ID = data.ID;
                channel.ReceiveBuff[channel.HeadPointer].DataLen = data.DataLen;
                channel.ReceiveBuff[channel.HeadPointer].data = data.data;
                channel.ReceiveBuff[channel.HeadPointer].ExternFlag = data.ExternFlag;
                channel.ReceiveBuff[channel.HeadPointer].RemoteFlag = data.RemoteFlag;
                channel.ReceiveBuff[channel.HeadPointer].TimeStamp = data.TimeStamp;
                channel.ReceiveBuff[channel.HeadPointer].Reserved = data.Reserved;
                channel.ReceiveBuff[channel.HeadPointer].TimeFlag = data.TimeFlag;
                channel.ShiftHead();
                if (channel.HeadPointer >= REC_MSG_BUF_MAX)
                {
                    channel.HeadPointer = 0;
                }
            }
        }

        public static void SendMessages(SendChannel channel)
        {
            CanData[] send_data = new CanData[2];

            for (int count = 0; count < SEND_MSG_MAX_COUNT; ++count)
            {
                if (channel.HeadPointer == channel.TailPointer)
                {
                    break;
                }

                send_data[0] = channel.SendBuff[channel.TailPointer];
                send_data[1] = channel.SendBuff[channel.TailPointer];
                channel.ShiftTail();
                if (channel.TailPointer >= SEND_MSG_BUF_MAX)
                {
                    channel.TailPointer = 0;
                }

                uint data_length = 1;
                bool send_success = ECANDLL.Transmit(1, 0, 0, send_data, (ushort) data_length) ==
                                    E_CAN_STATUS.STATUS_OK;
                if (!send_success)
                {
                    //失败处理
                }
            }
        }

        public static INIT_CONFIG GetConfig(byte BaudRate)
        {
            INIT_CONFIG init_config = new INIT_CONFIG {AccCode = 0, AccMask = 0xffffff, Filter = 0};
            switch (BaudRate)
            {
                case 0 : //1000
                    init_config.Timing0 = 0;
                    init_config.Timing1 = 0x14;
                    break;
                case 1 : //800
                    init_config.Timing0 = 0;
                    init_config.Timing1 = 0x16;
                    break;
                case 2 : //666
                    init_config.Timing0 = 0x80;
                    init_config.Timing1 = 0xb6;
                    break;
                case 3 : //500
                    init_config.Timing0 = 0;
                    init_config.Timing1 = 0x1c;
                    break;
                case 4 : //400
                    init_config.Timing0 = 0x80;
                    init_config.Timing1 = 0xfa;
                    break;
                case 5 : //250
                    init_config.Timing0 = 0x01;
                    init_config.Timing1 = 0x1c;
                    break;
                case 6 : //200
                    init_config.Timing0 = 0x81;
                    init_config.Timing1 = 0xfa;
                    break;
                case 7 : //125
                    init_config.Timing0 = 0x03;
                    init_config.Timing1 = 0x1c;
                    break;
                case 8 : //100
                    init_config.Timing0 = 0x04;
                    init_config.Timing1 = 0x1c;
                    break;
                case 9 : //80
                    init_config.Timing0 = 0x83;
                    init_config.Timing1 = 0xff;
                    break;
                case 10 : //50
                    init_config.Timing0 = 0x09;
                    init_config.Timing1 = 0x1c;
                    break;
            }

            init_config.Mode = 0;
            return init_config;
        }
    }


    public static class INFOMaker
    {
        public const string START = "Received : ";

        public static void AppendTime(ref string str, CanData info)
        {
            if (info.TimeFlag == 0)
            {
                str += "Time:  ";
            }
            else
            {
                str += "Time:" + string.Format("{0:X8}h", info.TimeStamp);
            }
        }

        public static void AppendID(ref string str, CanData info)
        {
            str += "  ID:" + string.Format("{0:X8}h", info.ID);
        }

        public static void AppendRemote(ref string str, CanData info)
        {
            str += " Format:";
            str += info.RemoteFlag == 0 ? "Data " : "Remote ";
        }

        public static void AppendExternFlag(ref string str, CanData info)
        {
            str += " Type:";
            str += info.ExternFlag == 0 ? "Stand " : "Extern ";
        }

        public static void AppendData(ref string str, CanData info)
        {
            if (info.RemoteFlag == 0)
            {
                str += " Data:";
                if (info.DataLen > 8)
                {
                    info.DataLen = 8;
                }

                int mlen = info.DataLen - 1;
                for (int j = 0; j <= mlen; j++)
                {
                    str += string.Format("{0:X2}h", info.data[j]);
                }
            }
        }
    }
}
