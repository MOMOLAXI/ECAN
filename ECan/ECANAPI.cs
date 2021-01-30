using System;
using System.Collections.Generic;
using ECAN;

namespace ECanTest
{
    [Flags]
    public enum E_CONNECT_STATUS
    {
        Connected,
        Closed
    }

    /// <summary>
    /// 所有API :
    /// ECANAPI._.Initialize();
    /// ECANAPI._.OpenChannel();
    /// ECANAPI._.Update();
    /// ECANAPI._.OpenDevice();
    /// ECANAPI._.ResetCAN();
    /// ECANAPI._.WriteDataToChannel();
    /// ECANAPI._.ReadDataFromChannel();
    /// ECANAPI._.GetBoardInfo();
    /// ECANAPI._.CloseDecive();
    /// </summary>
    public class ECANAPI
    {
        //单例连接
        public static ECANAPI _ = new ECANAPI();

        //连接状态
        public E_CONNECT_STATUS DLLStatus { get; set; }
        public E_CONNECT_STATUS CANStatus { get; set; }
        public bool Connected => (DLLStatus & CANStatus) == E_CONNECT_STATUS.Connected;

        private SendChannel SendChannel { get; set; }
        private ReceiveChannel ReceiveChannel { get; set; }

        private ECANAPI()
        {
            //初始管道关闭
            SendChannel.CloseChannel();
            ReceiveChannel.CloseChannel();
        }

        public void Initialize()
        {
            //创建管道
            SendChannel = SendChannel.Create(ECANUtility.SEND_MSG_BUF_MAX);
            ReceiveChannel = ReceiveChannel.Create(ECANUtility.REC_MSG_BUF_MAX);
        }

        //打开管道
        public void OpenChannel()
        {
            SendChannel.OpenChannel();
            ReceiveChannel.OpenChannel();
        }

        //每帧更新，读取数据, 在MonoBehaviour的Update中调用
        public void Update(float dt)
        {
            ReadDataFromChannel();
            ReadError();
        }

        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="baud_rate">波特率</param>
        public void OpenDevice(byte baud_rate)
        {
            //如果已经开启，先关闭后重新初始化再开启
            CloseDecive();
            INIT_CONFIG config = ECANUtility.GetConfig(baud_rate);

            //打开设备
            bool open_success =
                ECANDLL.OpenDevice(ECANUtility.DEVICE_TYPE, ECANUtility.DEVICE_IND, ECANUtility.RESERVED) ==
                E_CAN_STATUS.STATUS_OK;
            if (!open_success)
            {
                //Debug.Error("打开失败，检查dll是否导入项目")
                return;
            }

            DLLStatus = E_CONNECT_STATUS.Connected;

            //初始化CAN
            bool init_can_success =
                ECANDLL.InitCAN(ECANUtility.DEVICE_TYPE, ECANUtility.DEVICE_IND, ECANUtility.CAN_IND, ref config) !=
                E_CAN_STATUS.STATUS_OK;
            if (!init_can_success)
            {
                //Debug.Log("初始化CAN失败")
                ECANDLL.CloseDevice(ECANUtility.DEVICE_TYPE, ECANUtility.DEVICE_IND);
                return;
            }

            CANStatus = E_CONNECT_STATUS.Connected;
            //启动CAN
            bool start_can_success =
                ECANDLL.StartCAN(ECANUtility.DEVICE_TYPE, ECANUtility.DEVICE_IND, ECANUtility.CAN_IND) ==
                E_CAN_STATUS.STATUS_OK;
            if (!start_can_success)
            {
                //Debug.Log("启动CAN失败")
            }
        }

        /// <summary>
        /// 重置CAN
        /// </summary>
        public void ResetCAN()
        {
            if (!Connected)
            {
                //Debug.Log("CAN未连接")
                return;
            }

            bool reset_success = ECANDLL.ResetCAN(1, 0, 0) == E_CAN_STATUS.STATUS_OK;
            if (!reset_success)
            {
                //Debug.Log("重启失败, 检查设备")
            }
        }

        /// <summary>
        /// 写入数据到channel，在通过channel的update更新到CAN
        /// </summary>
        /// 参数为UI上的输入数据
        public void WriteDataToChannel(string id, int nud_length, List<string> data, bool chbExtended, bool chb_remote)
        {
            if (!Connected)
            {
                return;
            }

            CanData frame_data = new CanData
            {
                SendType = 0,
                data = new byte[8],
                Reserved = new byte[2],
                ID = Convert.ToUInt32(id, 16),
                DataLen = Convert.ToByte(nud_length),
                ExternFlag = chbExtended ? (byte) 1 : (byte) 0
            };

            if (chb_remote)
            {
                frame_data.RemoteFlag = 1;
            }
            else
            {
                frame_data.RemoteFlag = 0;
                int data_length = frame_data.DataLen - 1;
                for (int i = 0; i <= data_length; i++)
                {
                    frame_data.data[i] = Convert.ToByte(data[i], 0x10);
                }
            }

            SendChannel.SendBuff[SendChannel.HeadPointer].ID = frame_data.ID;
            SendChannel.SendBuff[SendChannel.HeadPointer].DataLen = frame_data.DataLen;
            SendChannel.SendBuff[SendChannel.HeadPointer].data = frame_data.data;
            SendChannel.SendBuff[SendChannel.HeadPointer].ExternFlag = frame_data.ExternFlag;
            SendChannel.SendBuff[SendChannel.HeadPointer].RemoteFlag = frame_data.RemoteFlag;
            SendChannel.ShiftHead();
            if (SendChannel.HeadPointer >= ECANUtility.SEND_MSG_BUF_MAX)
            {
                SendChannel.HeadPointer = 0;
            }
        }

        /// <summary>
        /// 每次Timer的Update读数据到Channel
        /// </summary>
        /// <returns>读出的数据</returns>
        public List<string> ReadDataFromChannel()
        {
            List<string> ret_data = new List<string>();
            for (int i = 0; i < 50; ++i)
            {
                if (ReceiveChannel.HeadPointer == ReceiveChannel.TailPointer)
                {
                    break;
                }

                string single_row_data = INFOMaker.START;

                CanData frameinfo = ReceiveChannel.ReceiveBuff[ReceiveChannel.TailPointer];
                ReceiveChannel.ShiftTail();
                if (ReceiveChannel.TailPointer >= ECANUtility.REC_MSG_BUF_MAX)
                {
                    ReceiveChannel.TailPointer = 0;
                }

                INFOMaker.AppendTime(ref single_row_data, frameinfo);
                INFOMaker.AppendID(ref single_row_data, frameinfo);
                INFOMaker.AppendRemote(ref single_row_data, frameinfo);
                INFOMaker.AppendExternFlag(ref single_row_data, frameinfo);
                INFOMaker.AppendData(ref single_row_data, frameinfo);

                ret_data.Add(single_row_data);
                if (ret_data.Count > 500)
                {
                    ret_data.Clear();
                }
            }

            return ret_data;
        }

        /// <summary>
        /// 读取板子信息
        /// </summary>
        /// <returns></returns>
        public BOARD_INFO GetBoardInfo()
        {
            BOARD_INFO info;
            bool read_success = ECANDLL.ReadBoardInfo(ECANUtility.DEVICE_TYPE, ECANUtility.DEVICE_IND, out info) ==
                                E_CAN_STATUS.STATUS_OK;
            return read_success ? info : default;
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        public void CloseDecive()
        {
            ECANDLL.CloseDevice(ECANUtility.DEVICE_TYPE, ECANUtility.DEVICE_IND);
            DLLStatus = E_CONNECT_STATUS.Closed;
        }

        private void ReadError()
        {
            //返回的错误信息
            CAN_ERR_INFO error_info;
            bool read_success =
                ECANDLL.ReadErrInfo(ECANUtility.DEVICE_TYPE, ECANUtility.DEVICE_IND, ECANUtility.CAN_IND,
                    out error_info) == E_CAN_STATUS.STATUS_OK;
            if (read_success)
            {
                Console.WriteLine(error_info);
            }
        }
    }
}
