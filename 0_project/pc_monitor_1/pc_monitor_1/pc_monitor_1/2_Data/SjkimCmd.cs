using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pc_monitor_1._2_Data
{
    internal class SjkimCmd
    {
        public enum Communication_enum
        {
            NONE = 0,
            SERIAL,
            USB,
            BLUETOOTH,
            ETHERNET
        }

        public enum CmdList_enum
        {
            ONLINE = 0,         // 0
            STANDBY,            // 1
            ALC_ON,             // 2
            ALC_OFF,            // 3
            ALC_SET,            // 4
            AGC_SET,            // 5
            ALC_READ,           // 6
            AGC_READ,           // 7
            SET_TIME,           // 8
            FREQ_SET,           // 9
            SERIAL_START,       // 10
            USB_START,          // 11
            ETHERNETSTART,      // 12
            SERIAL_STOP,        // 13 
            USB_STOP,           // 14
            ETHERNET_STOP,      // 15
            IP_SET,             // 16
            IP_GET,             // 17
            LOG_LOAD_100,       // 18
            LOG_LOAD_1000,      // 19
            LOG_READ,           // 20
            PARA_SET_0,         // 21
            PARA_GET_0,         // 22
            PARA_GET_1,         // 23
            FREQ_PWR_GET,       // 24
            FREQ_PWR_SET,       // 25
            FREQ_INPUT_GET,     // 26
            FREQ_INPUT_SET,     // 27
            NORMAL,             // 28
            SYS_ID_GET,         // 29
            FAULT_LOG,          // 30
            LENGTH
        }

        public enum Faultmask_enum
        {
            FAULT_FWD = 1,      // 1
            FAULT_VSWR,         // 2
            FAULT_INPUT,        // 3
            FAULT_TEMPERATURE,  // 4
            FAULT_CURRENT,      // 5
            FAULT_INTERLOCK,    // 6
            FAULT_VOLTAGE,      // 7
            FAULT_INTERNAL,     // 8
            FAULT_MASK_LENGTH
        };

        public enum FreqCommand_enum
        {
            FWD_DAC_SET    = 0,  // 0
            FWD_FILE_SET   = 1,  // 1
            FWD_FILE_GET   = 2,  // 2
            INPUT_FILE_SET = 3,  // 3
            INPUT_FILE_GET = 4,  // 4
            FREQ_COMMAND_LENGTH
        }
    }
}
