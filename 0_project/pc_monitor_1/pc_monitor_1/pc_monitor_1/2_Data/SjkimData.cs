using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pc_monitor_1._2_Data
{
    internal class SjkimData
    {
        public enum ControlButton_enum
        {
            TABEL = 0,
            FAULT,
            COMM,
            VTC, // VOLT TEMP CUURENT
            FET, // FREQ ELAPSED TIME
            ALC_OFF,
            ALC_ON,
            READ_ATTEN,
            SET_ATTEN,
            ATTEN_1,
            ATTEN_01,
            ATTEN__1,
            ATTEN__01,
            SERIAL,
            USB,
            BLUETOOTH,
            ETHERNET,
            SET_TIME,
            ONLINE,
            FWD_DBM,
            REF_DBM,
            LENGTH
        }

        public enum Form_enum
        {
            MAIN_FORM = 0,           // 0
            IP_SETUP_FORM,           // 1     
            PARAMETER_SELECT_FORM,   // 2
            SET_PARAM_FORM,          // 3
            FREQ_PWR_MEA_FORM,       // 4
            FREQ_PWR_PROGRESS_FORM,  // 5
            INPUT_PWR_MEA_FORM,      // 6
            INPUT_PWR_PROGRESS_FORM, // 7 
            SET_FREQ_FORM,           // 8
            ERROR_LOG_FORM,          // 9
            LOGIN_FORM,              // 10
            MONITORING_FORM,         // 11
            ALC_AGC_FORM,            // 12
            LENGTH
        }

        public enum SetParaADC_enum
        {
            REFLECTED_COUPLING_VALUE_0 = 0,
            REFLECTED_REFERENCE_VOLTAGE_0,
            REFLECTED_STEP_VALUE_0,
            REFLECTED_COUPLING_VALUE_1,
            REFLECTED_REFERENCE_VOLTAGE_1,
            REFLECTED_STEP_VALUE_1,
            VOLTAGE_RATE_0,
            VOLTAGE_RATE_1,
            CURRENT_RATE_0,
            CURRENT_OFFSET_0,
            CURRENT_RATE_1,
            CURRENT_OFFSET_1,
            CURRENT_RATE_2,
            CURRENT_OFFSET_2,
            CURRENT_RATE_3,
            CURRENT_OFFSET_3,
            CURRENT_RATE_4,
            CURRENT_OFFSET_4,
            TEMP_RATE_0,
            TEMP_OFFSET_0,
            TEMP_RATE_1,
            TEMP_OFFSET_1,
            TEMP_RATE_2,
            TEMP_OFFSET_2,
            LENGTH
        }

        public enum SetParaTHRESHOLD_enum
        {
            VOLT_SET_0 = 0,
            VOLT_TOLERANCE_0,
            VOLT_SET_1,
            VOLT_TOLERANCE_1,
            OVER_CURRENT_0,
            OVER_CURRENT_1,
            OVER_CURRENT_2,
            OVER_CURRENT_3,
            OVER_CURRENT_4,
            INPUT_DUTY_THRESHOLD_VOLTAGE,
            INPUT_SIGNAL_LIMIT_VOLTAGE_0,
            OVER_INPUT_PWR_ALARM_LEVEL_0,
            OUTPUT_SIGNAL_LIMIT_VOLTAGE_0,
            OVER_FWD_PWR_ALARM_LEVEL_0,
            VSWR_ALARM_LEVEL,
            AVERAGING_TIME_IN_MILLISEC,
            ALC_MAX_POWER_DBM,
            DUTY_ALARM_LEVEL_PERCENT,
            ONLINE_TTL,
            DAC_VOLTAGE_LIMIT_LOW,
            DAC_VOLTAGE_LIMIT_HIGH,
            INTERLOCK_TTL,
            LENGTH
        }

        public enum TableData_VTC_enum
        {
            VOLT_0 = 0,
            VOLT_1,
            TEMP_SYS,
            TEMP_HEAT,
            TEMP_HPA1,
            CURRENT_0,
            CURRENT_1,
            CURRENT_2,
            CURRENT_3,
            CURRENT_4,
            LENGTH
        }

        public enum TableData_FET_enum
        {
            IP = 0,
            PORT,
            STANDBY_HOUR,
            MIN_SEC_STANDBY,
            RADIATE_HOUR,
            MIN_SEC_RADIATE,
            FREQ,
            LENGTH
        }

        public enum FAULT_enum
        {
            OVER_INPUT_PWR = 0,
            VSWR_FAULT,
            OVER_FWD_PWR,
            VOLTAGE,
            OVER_CURRENT,
            OVER_TEMP,
            FAN,
            INTERLOCK,
            LENGTH
        }

        public enum StatusPower_enum
        {
            ONLINE_STANDBY = 0,
            CLEAR_FAULT,
            FWD_POWER,
            RFL_POWER,
            VSWR_STATUS_POWER,
            LENGTH
        }

        public enum All_para_voltage_display_enum
        {
            ALL_DISPLAY_FORWARD_0 = 0,   // channel 0
            ALL_DISPLAY_FORWARD_1,       // channel 1
            ALL_DISPLAY_REFLECT_0,       // channel 2
            ALL_DISPLAY_REFLECT_1,       // channel 3
            ALL_DISPLAY_INPUT_0,         // channel 4
            ALL_DISPLAY_INPUT_1,         // channel 5
            ALL_DISPLAY_VOLTAGE_0,       // channel 6
            ALL_DISPLAY_VOLTAGE_1,       // channel 7
            ALL_DISPLAY_CURRENT_0,       // channel 8
            ALL_DISPLAY_CURRENT_1,       // channel 9
            ALL_DISPLAY_CURRENT_2,       // channel 10
            ALL_DISPLAY_CURRENT_3,       // channel 11
            ALL_DISPLAY_CURRENT_4,       // channel 12
            ALL_DISPLAY_TEMP_0,          // channel 13
            ALL_DISPLAY_TEMP_1,          // channel 14
            ALL_DISPLAY_TEMP_2,          // channel 15
            ALL_DISPLAY_ONLINE_STATE,    // channel 16
            ALL_DISPLAY_INTERLOCK_STATE, // channel 17
            ALL_DISPLAY_DAC_0,           // channel 18
            ALL_DISPLAY_LENGTH
        }
    }
}
