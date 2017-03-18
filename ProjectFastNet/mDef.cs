using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectFastNet
{
    class mDef
    {
        public const ushort SYS_RESET_IND              = 0X4180;
        public const ushort SYS_CERSION_RSP            = 0x6102;
        public const ushort SYS_ISAK_NV_READ_RSP       = 0x6108;
        public const ushort SYS_OSAL_START_TIMER_RSP   = 0x6109;
        public const ushort SYS_OSAL_STOP_TIMER_RSP    = 0x610B;
        public const ushort SYS_OSAL_TIMER_EXP         = 0x4181;
        public const ushort SYS_RANDOM_RSP             = 0x610C;
        public const ushort SYS_ADC_READ_RSP           = 0X610D;
        public const ushort SYS_GPIO_RSP               = 0x610E;
        public const ushort SYS_TEST_LOOPBACK_RSP      = 0X6141;
        public const ushort ZB_READ_CFG_RSP            = 0X6604;
        public const ushort ZB_WRITE_CFG_RSP           = 0X6605;
        public const ushort ZB_REGISTER_RSP            = 0X660A;
        public const ushort ZB_START_REQUEST_RSP       = 0x6600;
        public const ushort ZB_START_CONFIRM           = 0x4680;
        public const ushort ZB_PERMIT_JOIN_RSP         = 0x6608;
        public const ushort ZB_SEND_DATA_RSP = 0X6603;
        public const ushort ZB_SEND_DATA_CONFIRM = 0x4683;

        public const byte ZG_DEVICETYPE_ROUTER         = 0x01;

    }
}
