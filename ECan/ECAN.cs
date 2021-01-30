using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ECAN
{
    [Flags]
    public enum E_CAN_STATUS : uint
    {
        /// <summary>
        ///  error
        /// </summary>
        STATUS_ERR = 0x00000,

        /// <summary>
        /// No error
        /// </summary>
        STATUS_OK = 0x00001,
    }

    //和外部C++链接库通信的数据结构
    [StructLayout(LayoutKind.Sequential)]
    public struct CanData
    {
        public uint ID;
        public uint TimeStamp;
        public byte TimeFlag;
        public byte SendType;
        public byte RemoteFlag;
        public byte ExternFlag;
        public byte DataLen;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] data;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Reserved;
    }

    public struct CAN_ERR_INFO
    {
        public uint ErrCode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] Passive_ErrData;

        public byte ArLost_ErrData;
    }
    
    public struct INIT_CONFIG
    {
        public uint AccCode;
        public uint AccMask;
        public uint Reserved;
        public byte Filter;
        public byte Timing0;
        public byte Timing1;
        public byte Mode;
    }

    public struct BOARD_INFO
    {
        public ushort hw_Version;
        public ushort fw_Version;
        public ushort dr_Version;
        public ushort in_Version;
        public ushort irq_Num;
        public byte can_Num;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] str_Serial_Num;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] str_hw_Type;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] Reserved;
    }

    //动态链接库调用
    public static class ECANDLL
    {
        /// <returns></returns>
        [DllImport("ECANVCI64.dll", EntryPoint = "OpenDevice")]
        public static extern E_CAN_STATUS OpenDevice(
            uint DeviceType,
            uint DeviceInd,
            uint Reserved);

        [DllImport("ECANVCI64.dll", EntryPoint = "CloseDevice")]
        public static extern E_CAN_STATUS CloseDevice(
            uint DeviceType,
            uint DeviceInd);


        [DllImport("ECANVCI64.dll", EntryPoint = "InitCAN")]
        public static extern E_CAN_STATUS InitCAN(
            uint DeviceType,
            uint DeviceInd,
            uint CANInd,
            ref INIT_CONFIG InitConfig);


        [DllImport("ECANVCI64.dll", EntryPoint = "StartCAN")]
        public static extern E_CAN_STATUS StartCAN(
            uint DeviceType,
            uint DeviceInd,
            uint CANInd);


        [DllImport("ECANVCI64.dll", EntryPoint = "ResetCAN")]
        public static extern E_CAN_STATUS ResetCAN(
            uint DeviceType,
            uint DeviceInd,
            uint CANInd);


        [DllImport("ECANVCI64.dll", EntryPoint = "Transmit")]
        public static extern E_CAN_STATUS Transmit(
            uint DeviceType,
            uint DeviceInd,
            uint CANInd,
            CanData[] Send,
            ushort length);


        [DllImport("ECANVCI64.dll", EntryPoint = "Receive")]
        public static extern E_CAN_STATUS Receive(
            uint DeviceType,
            uint DeviceInd,
            uint CANInd,
            out CanData Receive,
            uint length,
            uint WaitTime);

        [DllImport("ECANVCI64.dll", EntryPoint = "ReadErrInfo")]
        public static extern E_CAN_STATUS ReadErrInfo(
            uint DeviceType,
            uint DeviceInd,
            uint CANInd,
            out CAN_ERR_INFO ReadErrInfo);


        [DllImport("ECANVCI64.dll", EntryPoint = "ReadBoardInfo")]
        public static extern E_CAN_STATUS ReadBoardInfo(
            uint DeviceType,
            uint DeviceInd,
            out BOARD_INFO ReadErrInfo);
    }
}